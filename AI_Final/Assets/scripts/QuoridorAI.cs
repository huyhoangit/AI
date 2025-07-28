using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// AI System cho Quoridor Game sử dụng Minimax với Alpha-Beta Pruning
/// Kết hợp với A* pathfinding, Decision Tree Classifier và Entropy Analysis
/// </summary>
public class QuoridorAI : MonoBehaviour
{
    [Header("AI Settings")]
    public int playerID = 2; // AI luôn là player 2
    public int maxDepth = 4; // Độ sâu tìm kiếm Minimax
    public float thinkTime = 1.0f; // Thời gian AI "suy nghĩ"
    
    [Header("AI Strategy")]
    [Range(0f, 1f)]
    public float aggressiveness = 0.6f; // Mức độ tấn công vs phòng thủ
    [Range(0f, 1f)]
    public float wallPlacementProbability = 0.3f; // Xác suất đặt wall thay vì di chuyển
    
    [Header("Decision Tree & Entropy Settings")]
    [Range(0f, 1f)]
    public float entropyThreshold = 0.7f; // Ngưỡng entropy để kích hoạt randomness
    [Range(0f, 2f)]
    public float softmaxTemperature = 1.0f; // Temperature cho softmax distribution
    public bool useDecisionTree = true; // Bật/tắt Decision Tree
    public bool useEntropyAnalysis = true; // Bật/tắt Entropy Analysis
    
    [Header("References")]
    public ChessPlayer aiPlayer; // Quân AI
    public ChessPlayer humanPlayer; // Quân người chơi
    public WallPlacer aiWallPlacer; // WallPlacer của AI
    
    private BoardManager boardManager;
    private GameState currentGameState;
    private bool isThinking = false;
    
    // Cache cho A* pathfinding
    private Dictionary<Vector2Int, List<Vector2Int>> pathCache = new Dictionary<Vector2Int, List<Vector2Int>>();
    
    // Decision Tree Classifier
    private DecisionTreeClassifier decisionTree;
    
    // Entropy Analysis
    private List<float> recentMoveScores = new List<float>();
    private Dictionary<string, int> movePatternHistory = new Dictionary<string, int>();
    
    // 1. Thêm biến QLearningAgent
    private QLearningAgent qAgent = new QLearningAgent();
    [Header("Q-Learning Settings")]
    public bool useQLearning = true; // Bật/tắt Q-learning
    public string qTablePath = "StreamingAssets/qtable.json";
    public bool allowQTableSaving = false; // Chỉ save Q-table khi đang training
    public bool isTrainedModel = false; // Flag để biết model đã trained
    
    // Backup path alternatives
    private string GetQTablePath()
    {
        // Try multiple paths for better compatibility - prioritize StreamingAssets
        string[] paths = {
            Path.Combine(Application.streamingAssetsPath, "qtable.json"),         // StreamingAssets (highest priority)
            Path.Combine(Application.dataPath, "StreamingAssets", "qtable.json"), // Manual StreamingAssets path
            Path.Combine(Application.dataPath, "qtable.json"),                    // Assets/qtable.json (trong build)
            Path.Combine(Application.persistentDataPath, "qtable.json"),          // Persistent data (cross-platform)
            qTablePath,                                                           // Original path
            "qtable.json"                                                         // Current directory fallback
        };
        
        foreach (string path in paths)
        {
            Debug.Log($"🔍 Checking Q-table path: {path}");
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                Debug.Log($"✅ Found Q-table at: {path}");
                Debug.Log($"📊 File size: {fileInfo.Length / 1024f:F1} KB, Modified: {fileInfo.LastWriteTime}");
                return path;
            }
        }
        
        // If not found, copy from StreamingAssets to persistentDataPath
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "qtable.json");
        string assetsPath = Path.Combine(Application.dataPath, "qtable.json");
        string targetPath = Path.Combine(Application.persistentDataPath, "qtable.json");
        
        // Try to copy from StreamingAssets first
        if (File.Exists(streamingPath))
        {
            try
            {
                File.Copy(streamingPath, targetPath, true);
                Debug.Log($"📁 Copied Q-table from StreamingAssets: {streamingPath} to {targetPath}");
                return targetPath;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Failed to copy from StreamingAssets: {e.Message}");
            }
        }
        
        // Fallback: copy from Assets
        if (File.Exists(assetsPath))
        {
            try
            {
                File.Copy(assetsPath, targetPath, true);
                Debug.Log($"📁 Copied Q-table from Assets: {assetsPath} to {targetPath}");
                return targetPath;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Failed to copy from Assets: {e.Message}");
            }
        }
        
        Debug.Log($"📁 No Q-table found, will create new at: {targetPath}");
        return targetPath;
    }

    // 2. Trong Start(), load Q-table nếu có
    void Start()
    {
        boardManager = BoardManager.Instance;
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }
        
        // Tự động tìm các component cần thiết
        FindGameComponents();
        
        // Khởi tạo game state
        InitializeGameState();
        
        // Khởi tạo Decision Tree Classifier
        InitializeDecisionTree();
        
        // KIỂM TRA xem có Self-Play Trainer đang chạy không
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null && trainer.autoStartTraining)
        {
            Debug.LogWarning("⚠️ QuoridorSelfPlayTrainer is auto-training! This may override your Q-table.");
            Debug.LogWarning("💡 Suggest: Set autoStartTraining = false on QuoridorSelfPlayTrainer to use existing trained model");
        }
        
        // Get best available Q-table path
        string actualQTablePath = GetQTablePath();
        
        // Trong Start(), load Q-table nếu có TRƯỚC khi initialize
        qAgent.LoadQTable(actualQTablePath);
        
        // Nếu Q-table có dữ liệu (đã trained), preserve epsilon và init without reset
        if (qAgent.GetQTableSize() > 1000) // Nếu có > 1000 states (đã trained)
        {
            qAgent.SetTrainedEpsilon(0.1f, 1000, 1000); // 10% exploration, đã trained
            qAgent.InitializeWithoutEpsilonReset(); // Preserve trained epsilon
            isTrainedModel = true; // Đánh dấu là model đã trained
            allowQTableSaving = false; // Không save để tránh ghi đè
            Debug.Log($"🎓 Using trained Q-table with exploitation mode (ε=0.1)");
            Debug.Log($"🔒 Q-Learning will NOT retrain - using existing knowledge");
            Debug.Log($"🚫 Q-table saving DISABLED to preserve trained model");
        }
        else
        {
            // Nếu chưa có trained data, initialize normal
            qAgent.Initialize();
            isTrainedModel = false;
            allowQTableSaving = true; // Cho phép save khi đang training
            Debug.Log($"🆕 Starting fresh Q-Learning training");
            Debug.Log($"💾 Q-table saving ENABLED for training");
        }
        
        // Debug Q-table status after loading
        Debug.Log($"🧠 Q-Learning enabled: {useQLearning}");
        Debug.Log($"📊 Q-table loaded with {qAgent.GetQTableSize()} states");
        
        Debug.Log($"🤖 QuoridorAI initialized for Player {playerID} with Decision Tree and Entropy Analysis");
        
        // Debug all paths for troubleshooting
        DebugAllPaths();
    }
    
    /// <summary>
    /// Debug all available paths for Q-table
    /// </summary>
    [ContextMenu("Debug Q-table Paths")]
    public void DebugAllPaths()
    {
        Debug.Log("=== Q-TABLE PATH DEBUG ===");
        Debug.Log($"📁 Application.dataPath: {Application.dataPath}");
        Debug.Log($"📁 Application.persistentDataPath: {Application.persistentDataPath}");
        Debug.Log($"📁 Application.streamingAssetsPath: {Application.streamingAssetsPath}");
        Debug.Log($"📁 Current Directory: {System.Environment.CurrentDirectory}");
        
        string[] testPaths = {
            Path.Combine(Application.streamingAssetsPath, "qtable.json"),
            Path.Combine(Application.dataPath, "qtable.json"),
            Path.Combine(Application.dataPath, "StreamingAssets", "qtable.json"),
            Path.Combine(Application.persistentDataPath, "qtable.json"),
            qTablePath
        };
        
        for (int i = 0; i < testPaths.Length; i++)
        {
            bool exists = File.Exists(testPaths[i]);
            Debug.Log($"🔍 Path {i+1}: {testPaths[i]} -> {(exists ? "✅ EXISTS" : "❌ NOT FOUND")}");
            
            if (exists)
            {
                FileInfo info = new FileInfo(testPaths[i]);
                Debug.Log($"    📊 Size: {info.Length / 1024f:F1} KB, Modified: {info.LastWriteTime}");
            }
        }
    }
    
    /// <summary>
    /// Tự động tìm các component trong game
    /// </summary>
    void FindGameComponents()
    {
        // Tìm tất cả ChessPlayer
        ChessPlayer[] allPlayers = FindObjectsByType<ChessPlayer>(FindObjectsSortMode.None);
        
        foreach (ChessPlayer player in allPlayers)
        {
            if (player.playerID == playerID)
            {
                aiPlayer = player;
                Debug.Log($"✅ Found AI Player: {player.name}");
            }
            else if (player.playerID != playerID)
            {
                humanPlayer = player;
                Debug.Log($"✅ Found Human Player: {player.name}");
            }
        }
        
        // Tìm WallPlacer của AI
        var allWallPlacers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var component in allWallPlacers)
        {
            if (component.GetType().Name == "WallPlacer")
            {
                var playerIDField = component.GetType().GetField("playerID");
                if (playerIDField != null)
                {
                    int wallPlayerID = (int)playerIDField.GetValue(component);
                    if (wallPlayerID == playerID)
                    {
                        // Cast to WallPlacer (using reflection)
                        Debug.Log($"✅ Found AI WallPlacer: {component.name}");
                        break;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Khởi tạo trạng thái game
    /// </summary>
    void InitializeGameState()
    {
        currentGameState = new GameState();
        
        if (aiPlayer != null)
        {
            currentGameState.aiPosition = new Vector2Int(aiPlayer.col, aiPlayer.row);
        }
        
        if (humanPlayer != null)
        {
            currentGameState.humanPosition = new Vector2Int(humanPlayer.col, humanPlayer.row);
        }
        
        currentGameState.placedWalls = GetAllPlacedWalls();
        currentGameState.aiWallsLeft = 10; // Mỗi player có 10 walls
        currentGameState.humanWallsLeft = 10;
    }
    
    /// <summary>
    /// AI thực hiện lượt đi
    /// </summary>
    public void MakeAIMove()
    {
        if (isThinking) return;
        
        StartCoroutine(MakeAIMoveCoroutine());
    }
    
    /// <summary>
    /// Coroutine để AI suy nghĩ và thực hiện nước đi
    /// </summary>
    System.Collections.IEnumerator MakeAIMoveCoroutine()
    {
        isThinking = true;
        Debug.Log($"🤖 AI is thinking...");
        
        // Cập nhật game state hiện tại
        UpdateGameState();
        
        // Thời gian suy nghĩ
        yield return new UnityEngine.WaitForSeconds(thinkTime);
        
        // Tìm nước đi tốt nhất bằng Minimax hoặc Q-Learning
        AIMove bestMove = null;
        List<AIMove> possibleMoves = GeneratePossibleMoves(currentGameState, true);
        
        if (useQLearning)
        {
            Debug.Log($"🧠 Using Q-Learning algorithm (Epsilon: {qAgent.GetEpsilonInfo().currentEpsilon:F3})");
            bestMove = qAgent.ChooseAction(currentGameState, possibleMoves);
            Debug.Log($"🎯 Q-learning selected move: {qAgent.EncodeAction(bestMove)}");
        }
        else
        {
            Debug.Log($"🔍 Using Minimax algorithm (Depth: {maxDepth})");
            bestMove = FindBestMove(); // Minimax
            Debug.Log($"🎯 Minimax selected move: {bestMove?.moveType} to {bestMove?.targetPosition}");
        }
        
        if (bestMove != null)
        {
            // Lưu lại state và action để cập nhật Q sau khi thực hiện
            lastState = currentGameState.Clone();
            lastAction = bestMove;
            ExecuteAIMove(bestMove);
        }
        else
        {
            Debug.LogWarning("⚠️ AI couldn't find a valid move!");
        }
        
        isThinking = false;
    }
    
    /// <summary>
    /// Public method để GameManager call - sẽ chọn algorithm dựa vào useQLearning flag
    /// </summary>
    public AIMove GetBestMove()
    {
        UpdateGameState();
        List<AIMove> possibleMoves = GeneratePossibleMoves(currentGameState, true);
        
        if (useQLearning)
        {
            Debug.Log($"🧠 AI using Q-Learning (ε={qAgent.GetEpsilonInfo().currentEpsilon:F3}, States={qAgent.GetQTableSize()})");
            var move = qAgent.ChooseAction(currentGameState, possibleMoves);
            Debug.Log($"🎯 Q-Learning selected: {qAgent.EncodeAction(move)}");
            return move;
        }
        else
        {
            Debug.Log($"🔍 AI using Minimax (Depth={maxDepth}, Aggressiveness={aggressiveness:F2})");
            var move = FindBestMove();
            Debug.Log($"🎯 Minimax selected: {move?.moveType} to {move?.targetPosition}");
            return move;
        }
    }
    
    /// <summary>
    /// Cập nhật trạng thái game hiện tại
    /// </summary>
    void UpdateGameState()
    {
        if (aiPlayer != null)
        {
            currentGameState.aiPosition = new Vector2Int(aiPlayer.col, aiPlayer.row);
        }
        
        if (humanPlayer != null)
        {
            currentGameState.humanPosition = new Vector2Int(humanPlayer.col, humanPlayer.row);
        }
        
        currentGameState.placedWalls = GetAllPlacedWalls();
        
        // Clear path cache khi có wall mới
        pathCache.Clear();
    }
    
    /// <summary>
    /// Tìm nước đi tốt nhất bằng Minimax với Alpha-Beta Pruning, Decision Tree và Entropy Analysis
    /// </summary>
    AIMove FindBestMove()
    {
        Debug.Log($"🔍 Finding best move with depth {maxDepth}");
        
        // Generate tất cả possible moves
        List<AIMove> possibleMoves = GeneratePossibleMoves(currentGameState, true);
        Debug.Log($"📋 Generated {possibleMoves.Count} possible moves for AI");
        
        // Phase 1: Decision Tree Classifier để lọc strategy
        if (useDecisionTree && decisionTree != null)
        {
            var strategyDecision = GetDecisionTreeStrategy();
            possibleMoves = FilterMovesByStrategy(possibleMoves, strategyDecision);
            Debug.Log($"🌳 Decision Tree filtered to {possibleMoves.Count} moves (Strategy: {strategyDecision})");
        }
        
        // Phase 2: Đánh giá moves bằng Minimax
        var moveScores = new List<(AIMove move, float score)>();
        float bestScore = float.NegativeInfinity;
        
        foreach (AIMove move in possibleMoves)
        {
            // Apply move tạm thời
            GameState newState = ApplyMove(currentGameState, move, true);
            
            // Minimax với Alpha-Beta Pruning
            float score = Minimax(newState, maxDepth - 1, float.NegativeInfinity, float.PositiveInfinity, false);
            
            moveScores.Add((move, score));
            if (score > bestScore)
            {
                bestScore = score;
            }
            
            Debug.Log($"   Move {move.moveType} to {move.targetPosition} -> Score: {score:F2}");
        }
        
        // Phase 3: Entropy Analysis cho selection
        AIMove selectedMove = null;
        if (useEntropyAnalysis && moveScores.Count > 1)
        {
            selectedMove = SelectMoveWithEntropyAnalysis(moveScores, bestScore);
            Debug.Log($"🎲 Entropy Analysis selected move with diversity consideration");
        }
        else
        {
            // Fallback: chọn move có score cao nhất
            selectedMove = moveScores.OrderByDescending(ms => ms.score).FirstOrDefault().move;
        }
        
        // Cập nhật history cho entropy analysis
        UpdateMoveHistory(selectedMove, bestScore);
        
        Debug.Log($"🎯 Final selected move: {selectedMove?.moveType} with score {bestScore:F2}");
        return selectedMove;
    }
    
    /// <summary>
    /// Minimax algorithm với Alpha-Beta Pruning
    /// </summary>
    float Minimax(GameState state, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        // Base case: hết độ sâu hoặc game kết thúc
        if (depth == 0 || IsGameOver(state))
        {
            return EvaluateGameState(state);
        }
        
        if (maximizingPlayer) // AI turn
        {
            float maxEval = float.NegativeInfinity;
            List<AIMove> moves = GeneratePossibleMoves(state, true);
            
            foreach (AIMove move in moves)
            {
                GameState newState = ApplyMove(state, move, true);
                float eval = Minimax(newState, depth - 1, alpha, beta, false);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                
                if (beta <= alpha) break; // Alpha-Beta Pruning
            }
            
            return maxEval;
        }
        else // Human turn
        {
            float minEval = float.PositiveInfinity;
            List<AIMove> moves = GeneratePossibleMoves(state, false);
            
            foreach (AIMove move in moves)
            {
                GameState newState = ApplyMove(state, move, false);
                float eval = Minimax(newState, depth - 1, alpha, beta, true);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                
                if (beta <= alpha) break; // Alpha-Beta Pruning
            }
            
            return minEval;
        }
    }
    
    /// <summary>
    /// Đánh giá trạng thái game (heuristic function)
    /// </summary>
    float EvaluateGameState(GameState state)
    {
        float score = 0f;
        
        // 1. Khoảng cách đến đích
        int aiDistanceToGoal = GetShortestPathLength(state.aiPosition, GetAIGoalRow(), state.placedWalls);
        int humanDistanceToGoal = GetShortestPathLength(state.humanPosition, GetHumanGoalRow(), state.placedWalls);
        
        // AI muốn giảm khoảng cách của mình và tăng khoảng cách của đối thủ
        score += (humanDistanceToGoal - aiDistanceToGoal) * 100f;
        
        // 2. Mobility (số nước đi có thể)
        int aiMobility = GetValidMoveCount(state.aiPosition, state.placedWalls);
        int humanMobility = GetValidMoveCount(state.humanPosition, state.placedWalls);
        score += (aiMobility - humanMobility) * 10f;
        
        // 3. Wall advantage
        score += (state.aiWallsLeft - state.humanWallsLeft) * 5f;
        
        // 4. Position evaluation
        score += EvaluatePosition(state.aiPosition, true) - EvaluatePosition(state.humanPosition, false);
        
        // 5. Win condition
        if (state.aiPosition.y == GetAIGoalRow())
        {
            score += 10000f; // AI wins
        }
        else if (state.humanPosition.y == GetHumanGoalRow())
        {
            score -= 10000f; // AI loses
        }
        
        return score;
    }
    
    /// <summary>
    /// Tính đường đi ngắn nhất bằng A*
    /// </summary>
    int GetShortestPathLength(Vector2Int start, int goalRow, List<WallInfo> walls)
    {
        // Sử dụng cache nếu có
        Vector2Int cacheKey = new Vector2Int(start.x * 1000 + start.y, goalRow);
        if (pathCache.ContainsKey(cacheKey))
        {
            return pathCache[cacheKey].Count;
        }
        
        List<Vector2Int> path = FindPathAStar(start, goalRow, walls);
        pathCache[cacheKey] = path;
        
        return path != null ? path.Count : 999; // Return large number if no path
    }
    
    /// <summary>
    /// A* Pathfinding algorithm
    /// </summary>
    List<Vector2Int> FindPathAStar(Vector2Int start, int goalRow, List<WallInfo> walls)
    {
        var openSet = new List<AStarNode>();
        var closedSet = new HashSet<Vector2Int>();
        
        var startNode = new AStarNode(start, 0, Mathf.Abs(start.y - goalRow));
        openSet.Add(startNode);
        
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        
        while (openSet.Count > 0)
        {
            // Tìm node có f-cost thấp nhất
            var current = openSet.OrderBy(n => n.fCost).First();
            openSet.Remove(current);
            closedSet.Add(current.position);
            
            // Đã đến đích
            if (current.position.y == goalRow)
            {
                return ReconstructPath(cameFrom, current.position);
            }
            
            // Kiểm tra các neighbor
            Vector2Int[] neighbors = {
                current.position + Vector2Int.up,
                current.position + Vector2Int.down,
                current.position + Vector2Int.left,
                current.position + Vector2Int.right
            };
            
            foreach (var neighbor in neighbors)
            {
                // Kiểm tra bounds
                if (neighbor.x < 0 || neighbor.x >= 9 || neighbor.y < 0 || neighbor.y >= 9)
                    continue;
                
                // Đã được explore
                if (closedSet.Contains(neighbor))
                    continue;
                
                // Kiểm tra wall blocking
                if (IsBlockedByWallInfo(current.position, neighbor, walls))
                    continue;
                
                float tentativeGCost = current.gCost + 1;
                
                var existingNode = openSet.FirstOrDefault(n => n.position == neighbor);
                if (existingNode == null)
                {
                    var newNode = new AStarNode(neighbor, tentativeGCost, Mathf.Abs(neighbor.y - goalRow));
                    openSet.Add(newNode);
                    cameFrom[neighbor] = current.position;
                }
                else if (tentativeGCost < existingNode.gCost)
                {
                    existingNode.gCost = tentativeGCost;
                    existingNode.fCost = existingNode.gCost + existingNode.hCost;
                    cameFrom[neighbor] = current.position;
                }
            }
        }
        
        return null; // No path found
    }
    
    /// <summary>
    /// Reconstruct path từ A*
    /// </summary>
    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        
        return path;
    }
    
    /// <summary>
    /// Generate tất cả possible moves cho một player
    /// </summary>
    List<AIMove> GeneratePossibleMoves(GameState state, bool isAIPlayer)
    {
        var moves = new List<AIMove>();
        Vector2Int playerPos = isAIPlayer ? state.aiPosition : state.humanPosition;
        int wallsLeft = isAIPlayer ? state.aiWallsLeft : state.humanWallsLeft;
        
        // 1. Movement moves
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (var direction in directions)
        {
            Vector2Int newPos = playerPos + direction;
            
            if (IsValidPosition(newPos) && !IsBlockedByWallInfo(playerPos, newPos, state.placedWalls))
            {
                moves.Add(new AIMove
                {
                    moveType = MoveType.Movement,
                    targetPosition = newPos
                });
            }
        }
        
        // 2. Wall placement moves (nếu còn wall)
        if (wallsLeft > 0)
        {
            var wallMoves = GenerateWallMoves(state);
            
            // Chỉ thêm một số wall moves tốt nhất để tránh explosion
            wallMoves = wallMoves.OrderByDescending(m => EvaluateWallMove(m, state, isAIPlayer))
                               .Take(8) // Limit số wall moves
                               .ToList();
            
            moves.AddRange(wallMoves);
        }
        
        return moves;
    }
    
    /// <summary>
    /// Generate possible wall moves
    /// </summary>
    List<AIMove> GenerateWallMoves(GameState state)
    {
        var wallMoves = new List<AIMove>();
        
        // Tạo wall moves cho các vị trí strategic
        for (int row = 0; row < 8; row++) // 8 vì wall nằm giữa các ô
        {
            for (int col = 0; col < 8; col++)
            {
                // Horizontal wall
                var hWallMove = new AIMove
                {
                    moveType = MoveType.WallPlacement,
                    targetPosition = new Vector2Int(col, row),
                    isHorizontalWall = true
                };
                
                if (IsValidWallPlacement(hWallMove, state))
                {
                    wallMoves.Add(hWallMove);
                }
                
                // Vertical wall
                var vWallMove = new AIMove
                {
                    moveType = MoveType.WallPlacement,
                    targetPosition = new Vector2Int(col, row),
                    isHorizontalWall = false
                };
                
                if (IsValidWallPlacement(vWallMove, state))
                {
                    wallMoves.Add(vWallMove);
                }
            }
        }
        
        return wallMoves;
    }
    
    /// <summary>
    /// Thực hiện AI move
    /// </summary>
    void ExecuteAIMove(AIMove move)
    {
        Debug.Log($"🤖 AI executing move: {move.moveType}");
        
        if (move.moveType == MoveType.Movement)
        {
            // Di chuyển quân AI
            if (aiPlayer != null)
            {
                aiPlayer.col = move.targetPosition.x;
                aiPlayer.row = move.targetPosition.y;
                
                // Cập nhật vị trí visual
                var updateMethod = aiPlayer.GetType().GetMethod("UpdatePosition", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                updateMethod?.Invoke(aiPlayer, null);
                
                Debug.Log($"🤖 AI moved to [{move.targetPosition.x}, {move.targetPosition.y}]");
            }
        }
        else if (move.moveType == MoveType.WallPlacement)
        {
            // Đặt wall
            PlaceAIWall(move);
        }
        
        // Kiểm tra win condition
        CheckWinCondition();
    }
    
    /// <summary>
    /// Đặt wall cho AI
    /// </summary>
    void PlaceAIWall(AIMove wallMove)
    {
        // Sử dụng reflection để gọi WallPlacer
        var allWallPlacers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var component in allWallPlacers)
        {
            if (component.GetType().Name == "WallPlacer")
            {
                var playerIDField = component.GetType().GetField("playerID");
                if (playerIDField != null)
                {
                    int wallPlayerID = (int)playerIDField.GetValue(component);
                    if (wallPlayerID == playerID)
                    {
                        // Tìm method PlaceWallAt
                        var placeMethod = component.GetType().GetMethod("PlaceWallAt");
                        if (placeMethod != null)
                        {
                            // Convert AI move to world position
                            Vector3 wallWorldPos = CalculateWallWorldPosition(wallMove);
                            
                            placeMethod.Invoke(component, new object[] { wallWorldPos, wallMove.isHorizontalWall });
                            Debug.Log($"🧱 AI placed {(wallMove.isHorizontalWall ? "horizontal" : "vertical")} wall at {wallWorldPos}");
                        }
                        break;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Tính vị trí world cho wall placement
    /// </summary>
    Vector3 CalculateWallWorldPosition(AIMove wallMove)
    {
        Vector3 boardStart = new Vector3(-5.0f, 0f, -4.85f);
        float stepSize = 1.1f; // squareSize + spacing
        
        Vector3 worldPos;
        
        if (wallMove.isHorizontalWall)
        {
            // Horizontal wall giữa 2 hàng
            worldPos = new Vector3(
                boardStart.x + wallMove.targetPosition.x * stepSize + 0.5f,
                0.1f,
                boardStart.z + (wallMove.targetPosition.y + 1) * stepSize - 0.05f
            );
        }
        else
        {
            // Vertical wall giữa 2 cột
            worldPos = new Vector3(
                boardStart.x + (wallMove.targetPosition.x + 1) * stepSize - 0.05f,
                0.1f,
                boardStart.z + wallMove.targetPosition.y * stepSize + 0.5f
            );
        }
        
        return worldPos;
    }
    
    /// <summary>
    /// Kiểm tra điều kiện thắng
    /// </summary>
    void CheckWinCondition()
    {
        if (aiPlayer != null && aiPlayer.row == GetAIGoalRow())
        {
            Debug.Log("🏆 AI WINS!");
            // TODO: Implement win screen
        }
        else if (humanPlayer != null && humanPlayer.row == GetHumanGoalRow())
        {
            Debug.Log("🏆 HUMAN WINS!");
            // TODO: Implement win screen
        }
    }
    
    // ========== HELPER METHODS ==========
    
    int GetAIGoalRow() => 0; // AI (Player 2) muốn về hàng 0
    int GetHumanGoalRow() => 8; // Human (Player 1) muốn về hàng 8
    
    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 9 && pos.y >= 0 && pos.y < 9;
    }
    
    List<WallInfo> GetAllPlacedWalls()
    {
        var walls = new List<WallInfo>();
        
        GameObject[] placedWalls1 = GameObject.FindGameObjectsWithTag("PlacedWall");
        GameObject[] placedWalls2 = GameObject.FindGameObjectsWithTag("Wall");
        
        foreach (var wall in placedWalls1.Concat(placedWalls2))
        {
            if (wall != null)
            {
                walls.Add(new WallInfo
                {
                    position = wall.transform.position,
                    isHorizontal = IsHorizontalWall(wall)
                });
            }
        }
        
        return walls;
    }
    
    bool IsHorizontalWall(GameObject wall)
    {
        if (wall.name.Contains("_H_") || wall.name.EndsWith("_H"))
            return true;
        if (wall.name.Contains("_V_") || wall.name.EndsWith("_V"))
            return false;
        
        Vector3 scale = wall.transform.localScale;
        return scale.x > scale.z;
    }
    
    bool IsBlockedByWallInfo(Vector2Int from, Vector2Int to, List<WallInfo> walls)
    {
        // Implement wall blocking logic similar to ChessPlayer
        // Simplified version for AI
        foreach (var wall in walls)
        {
            if (IsWallBlockingMovement(from, to, wall))
                return true;
        }
        return false;
    }
    
    bool IsWallBlockingMovement(Vector2Int from, Vector2Int to, WallInfo wall)
    {
        // Simplified wall blocking check
        Vector3 boardStart = new Vector3(-5.0f, 0f, -4.85f);
        float stepSize = 1.1f;
        
        int deltaCol = to.x - from.x;
        int deltaRow = to.y - from.y;
        
        Vector3 expectedWallPos = Vector3.zero;
        bool shouldBlock = false;
        
        if (deltaCol == 1) // Moving RIGHT
        {
            expectedWallPos = new Vector3(
                boardStart.x + (from.x + 1) * stepSize - 0.05f,
                0.1f,
                boardStart.z + from.y * stepSize + 0.5f
            );
            shouldBlock = !wall.isHorizontal;
        }
        else if (deltaCol == -1) // Moving LEFT
        {
            expectedWallPos = new Vector3(
                boardStart.x + from.x * stepSize - 0.05f,
                0.1f,
                boardStart.z + from.y * stepSize + 0.5f
            );
            shouldBlock = !wall.isHorizontal;
        }
        else if (deltaRow == 1) // Moving UP
        {
            expectedWallPos = new Vector3(
                boardStart.x + from.x * stepSize + 0.5f,
                0.1f,
                boardStart.z + (from.y + 1) * stepSize - 0.05f
            );
            shouldBlock = wall.isHorizontal;
        }
        else if (deltaRow == -1) // Moving DOWN
        {
            expectedWallPos = new Vector3(
                boardStart.x + from.x * stepSize + 0.5f,
                0.1f,
                boardStart.z + from.y * stepSize - 0.05f
            );
            shouldBlock = wall.isHorizontal;
        }
        
        float distance = Vector3.Distance(wall.position, expectedWallPos);
        return shouldBlock && distance < 0.7f;
    }
    
    int GetValidMoveCount(Vector2Int position, List<WallInfo> walls)
    {
        int count = 0;
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (var direction in directions)
        {
            Vector2Int newPos = position + direction;
            if (IsValidPosition(newPos) && !IsBlockedByWallInfo(position, newPos, walls))
            {
                count++;
            }
        }
        
        return count;
    }
    
    float EvaluatePosition(Vector2Int position, bool isAI)
    {
        float score = 0f;
        
        // Center control
        float centerDistance = Vector2.Distance(position, new Vector2(4, 4));
        score += (9 - centerDistance) * 2f;
        
        // Progress toward goal
        int goalRow = isAI ? GetAIGoalRow() : GetHumanGoalRow();
        float progressScore = (8 - Mathf.Abs(position.y - goalRow)) * 3f;
        score += isAI ? progressScore : -progressScore;
        
        return score;
    }
    
    float EvaluateWallMove(AIMove wallMove, GameState state, bool isAIPlayer)
    {
        // Simple wall evaluation - can be improved
        Vector2Int enemyPos = isAIPlayer ? state.humanPosition : state.aiPosition;
        float distanceToEnemy = Vector2.Distance(wallMove.targetPosition, enemyPos);
        
        // Prefer walls closer to enemy
        return 10f - distanceToEnemy;
    }
    
    bool IsValidWallPlacement(AIMove wallMove, GameState state)
    {
        // Simple validation - check if position is not occupied
        foreach (var wall in state.placedWalls)
        {
            Vector3 wallWorldPos = CalculateWallWorldPosition(wallMove);
            if (Vector3.Distance(wall.position, wallWorldPos) < 0.5f)
            {
                return false; // Too close to existing wall
            }
        }
        
        return true;
    }
    
    GameState ApplyMove(GameState state, AIMove move, bool isAIPlayer)
    {
        GameState newState = state.Clone();
        
        if (move.moveType == MoveType.Movement)
        {
            if (isAIPlayer)
            {
                newState.aiPosition = move.targetPosition;
            }
            else
            {
                newState.humanPosition = move.targetPosition;
            }
        }
        else if (move.moveType == MoveType.WallPlacement)
        {
            newState.placedWalls.Add(new WallInfo
            {
                position = CalculateWallWorldPosition(move),
                isHorizontal = move.isHorizontalWall
            });
            
            if (isAIPlayer)
            {
                newState.aiWallsLeft--;
            }
            else
            {
                newState.humanWallsLeft--;
            }
        }
        
        return newState;
    }
    
    bool IsGameOver(GameState state)
    {
        return state.aiPosition.y == GetAIGoalRow() || state.humanPosition.y == GetHumanGoalRow();
    }
    
    // ========== DECISION TREE & ENTROPY STRUCTURES ==========
    
    /// <summary>
    /// Khởi tạo Decision Tree Classifier
    /// </summary>
    void InitializeDecisionTree()
    {
        decisionTree = new DecisionTreeClassifier();
        decisionTree.Initialize();
        Debug.Log("🌳 Decision Tree Classifier initialized");
    }
    
    /// <summary>
    /// Lấy strategy decision từ Decision Tree
    /// </summary>
    StrategyDecision GetDecisionTreeStrategy()
    {
        if (decisionTree == null) return StrategyDecision.Balanced;
        
        var features = ExtractDecisionFeatures();
        return decisionTree.Predict(features);
    }
    
    /// <summary>
    /// Trích xuất features cho Decision Tree
    /// </summary>
    DecisionFeatures ExtractDecisionFeatures()
    {
        int aiDistanceToGoal = GetShortestPathLength(currentGameState.aiPosition, GetAIGoalRow(), currentGameState.placedWalls);
        int humanDistanceToGoal = GetShortestPathLength(currentGameState.humanPosition, GetHumanGoalRow(), currentGameState.placedWalls);
        
        return new DecisionFeatures
        {
            aiDistanceToGoal = aiDistanceToGoal,
            humanDistanceToGoal = humanDistanceToGoal,
            aiWallsLeft = currentGameState.aiWallsLeft,
            humanWallsLeft = currentGameState.humanWallsLeft,
            aiMobility = GetValidMoveCount(currentGameState.aiPosition, currentGameState.placedWalls),
            humanMobility = GetValidMoveCount(currentGameState.humanPosition, currentGameState.placedWalls),
            distanceAdvantage = humanDistanceToGoal - aiDistanceToGoal,
            wallAdvantage = currentGameState.aiWallsLeft - currentGameState.humanWallsLeft,
            isEarlyGame = (currentGameState.aiWallsLeft + currentGameState.humanWallsLeft) > 14,
            isMidGame = (currentGameState.aiWallsLeft + currentGameState.humanWallsLeft) > 6 && (currentGameState.aiWallsLeft + currentGameState.humanWallsLeft) <= 14,
            isEndGame = (currentGameState.aiWallsLeft + currentGameState.humanWallsLeft) <= 6
        };
    }
    
    /// <summary>
    /// Lọc moves theo strategy từ Decision Tree
    /// </summary>
    List<AIMove> FilterMovesByStrategy(List<AIMove> moves, StrategyDecision strategy)
    {
        switch (strategy)
        {
            case StrategyDecision.Aggressive:
                // Ưu tiên wall placement để block đối thủ
                return moves.Where(m => m.moveType == MoveType.WallPlacement || 
                                      (m.moveType == MoveType.Movement && IsAggressiveMove(m))).ToList();
                
            case StrategyDecision.Defensive:
                // Ưu tiên di chuyển về đích
                return moves.Where(m => m.moveType == MoveType.Movement || 
                                      (m.moveType == MoveType.WallPlacement && IsDefensiveWall(m))).ToList();
                
            case StrategyDecision.Balanced:
                // Giữ nguyên tất cả moves
                return moves;
                
            case StrategyDecision.Blocking:
                // Chỉ wall placement
                var wallMoves = moves.Where(m => m.moveType == MoveType.WallPlacement).ToList();
                return wallMoves.Count > 0 ? wallMoves : moves.Where(m => m.moveType == MoveType.Movement).ToList();
                
            default:
                return moves;
        }
    }
    
    /// <summary>
    /// Kiểm tra move có phải aggressive không
    /// </summary>
    bool IsAggressiveMove(AIMove move)
    {
        if (move.moveType != MoveType.Movement) return false;
        
        // Move hướng về phía đối thủ
        Vector2Int direction = move.targetPosition - currentGameState.aiPosition;
        Vector2Int towardEnemy = currentGameState.humanPosition - currentGameState.aiPosition;
        
        return Vector2.Dot(direction, towardEnemy) > 0;
    }
    
    /// <summary>
    /// Kiểm tra wall có phải defensive không
    /// </summary>
    bool IsDefensiveWall(AIMove move)
    {
        if (move.moveType != MoveType.WallPlacement) return false;
        
        // Wall gần AI position hơn
        float distanceToAI = Vector2.Distance(move.targetPosition, currentGameState.aiPosition);
        float distanceToHuman = Vector2.Distance(move.targetPosition, currentGameState.humanPosition);
        
        return distanceToAI < distanceToHuman;
    }
    
    /// <summary>
    /// Chọn move sử dụng Entropy Analysis
    /// </summary>
    AIMove SelectMoveWithEntropyAnalysis(List<(AIMove move, float score)> moveScores, float bestScore)
    {
        // Lọc moves có score gần với best score
        float threshold = bestScore * 0.95f; // 95% của best score
        var topMoves = moveScores.Where(ms => ms.score >= threshold).ToList();
        
        if (topMoves.Count <= 1)
        {
            return topMoves.FirstOrDefault().move;
        }
        
        // Tính entropy cho selection
        float entropy = CalculateEntropyForMoves(topMoves);
        Debug.Log($"🎲 Move entropy: {entropy:F3}, threshold: {entropyThreshold:F3}");
        
        if (entropy > entropyThreshold)
        {
            // High entropy -> sử dụng softmax distribution
            return SelectMoveWithSoftmax(topMoves);
        }
        else
        {
            // Low entropy -> chọn best move
            return topMoves.OrderByDescending(ms => ms.score).First().move;
        }
    }
    
    /// <summary>
    /// Tính entropy cho danh sách moves
    /// </summary>
    float CalculateEntropyForMoves(List<(AIMove move, float score)> moves)
    {
        if (moves.Count <= 1) return 0f;
        
        // Normalize scores thành probabilities
        float totalScore = moves.Sum(ms => Mathf.Exp(ms.score / softmaxTemperature));
        var probabilities = moves.Select(ms => Mathf.Exp(ms.score / softmaxTemperature) / totalScore).ToList();
        
        // Tính entropy: H = -Σ(p * log(p))
        float entropy = 0f;
        foreach (float p in probabilities)
        {
            if (p > 0f)
            {
                entropy -= p * Mathf.Log(p, 2f);
            }
        }
        
        return entropy;
    }
    
    /// <summary>
    /// Chọn move bằng softmax distribution
    /// </summary>
    AIMove SelectMoveWithSoftmax(List<(AIMove move, float score)> moves)
    {
        // Tính softmax probabilities
        float totalExp = moves.Sum(ms => Mathf.Exp(ms.score / softmaxTemperature));
        var probabilities = moves.Select(ms => Mathf.Exp(ms.score / softmaxTemperature) / totalExp).ToList();
        
        // Random selection theo phân phối
        float randomValue = Random.Range(0f, 1f);
        float cumulativeProbability = 0f;
        
        for (int i = 0; i < moves.Count; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                Debug.Log($"🎯 Softmax selected move {i} with probability {probabilities[i]:F3}");
                return moves[i].move;
            }
        }
        
        // Fallback
        return moves.Last().move;
    }
    
    /// <summary>
    /// Cập nhật history cho entropy analysis
    /// </summary>
    void UpdateMoveHistory(AIMove selectedMove, float score)
    {
        // Cập nhật recent scores
        recentMoveScores.Add(score);
        if (recentMoveScores.Count > 10) // Giữ 10 scores gần nhất
        {
            recentMoveScores.RemoveAt(0);
        }
        
        // Cập nhật move pattern history
        if (selectedMove != null)
        {
            string pattern = $"{selectedMove.moveType}_{selectedMove.targetPosition.x}_{selectedMove.targetPosition.y}";
            if (movePatternHistory.ContainsKey(pattern))
            {
                movePatternHistory[pattern]++;
            }
            else
            {
                movePatternHistory[pattern] = 1;
            }
            
            // Giới hạn size của history
            if (movePatternHistory.Count > 50)
            {
                var oldestPattern = movePatternHistory.OrderBy(kvp => kvp.Value).First().Key;
                movePatternHistory.Remove(oldestPattern);
            }
        }
        
        // Debug logging
        LogAIPerformance(selectedMove, score);
    }
    
    /// <summary>
    /// Log AI performance metrics
    /// </summary>
    void LogAIPerformance(AIMove selectedMove, float score)
    {
        if (recentMoveScores.Count >= 5)
        {
            float avgScore = recentMoveScores.Average();
            float scoreVariance = recentMoveScores.Select(s => (s - avgScore) * (s - avgScore)).Average();
            float entropy = CalculateEntropyForMoves(recentMoveScores.Select(s => (selectedMove, s)).ToList());
            
            Debug.Log($"📊 AI Performance - Avg Score: {avgScore:F2}, Variance: {scoreVariance:F2}, Entropy: {entropy:F3}");
            Debug.Log($"🎯 Current Move: {selectedMove?.moveType} -> {selectedMove?.targetPosition}, Score: {score:F2}");
            
            // Pattern analysis
            if (movePatternHistory.Count > 5)
            {
                var topPatterns = movePatternHistory.OrderByDescending(kvp => kvp.Value).Take(3);
                Debug.Log($"🔄 Top Move Patterns: {string.Join(", ", topPatterns.Select(kvp => $"{kvp.Key}({kvp.Value})"))}");
            }
        }
    }

    // 4. Thêm biến lưu state/action trước đó để cập nhật Q
    private GameState lastState;
    private AIMove lastAction;

    // 5. Sau khi AI thực hiện xong nước đi, cập nhật Q-table (ví dụ trong ExecuteAIMove hoặc sau khi nhận reward)
    void OnAIMoveResult(float reward, GameState nextState)
    {
        if (useQLearning && lastState != null && lastAction != null)
        {
            List<AIMove> nextMoves = GeneratePossibleMoves(nextState, true);
            qAgent.UpdateQ(lastState, lastAction, reward, nextState, nextMoves);
        }
    }

    // 6. Sau mỗi ván, lưu Q-table và decay epsilon
    void OnGameEnd()
    {
        if (useQLearning && allowQTableSaving)
        {
            // Decay epsilon sau mỗi episode (chỉ khi đang training)
            qAgent.DecayEpsilonEpisode();
            
            // Save Q-table to best available path (chỉ khi cho phép)
            string actualQTablePath = GetQTablePath();
            qAgent.SaveQTable(actualQTablePath);
            
            Debug.Log("🎮 Game ended - Epsilon decayed and Q-table saved");
        }
        else if (useQLearning && !allowQTableSaving)
        {
            Debug.Log("🎮 Game ended - Using trained model, no saving to preserve Q-table");
        }
    }

    // ==== STATIC METHODS FOR SELF-PLAY Q-LEARNING ====
    public static List<AIMove> GeneratePossibleMovesStatic(GameState state, bool isAIPlayer)
    {
        // Copy logic từ GeneratePossibleMoves, nhưng chỉ dùng state truyền vào
        var moves = new List<AIMove>();
        Vector2Int pos = isAIPlayer ? state.aiPosition : state.humanPosition;
        int wallsLeft = isAIPlayer ? state.aiWallsLeft : state.humanWallsLeft;
        // Di chuyển: 4 hướng cơ bản
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector2Int next = pos + dir;
            if (IsValidPositionStatic(next) && !IsBlockedByWallInfoStatic(pos, next, state.placedWalls))
            {
                moves.Add(new AIMove { moveType = MoveType.Movement, targetPosition = next });
            }
        }
        // Đặt tường (nếu còn)
        if (wallsLeft > 0)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    foreach (bool isHorizontal in new[] { true, false })
                    {
                        var wallMove = new AIMove { moveType = MoveType.WallPlacement, targetPosition = new Vector2Int(x, y), isHorizontalWall = isHorizontal };
                        if (IsValidWallPlacementStatic(wallMove, state))
                            moves.Add(wallMove);
                    }
                }
            }
        }
        return moves;
    }

    public static GameState ApplyMoveStatic(GameState state, AIMove move, bool isAIPlayer)
    {
        // Tạo bản sao state mới
        GameState newState = state.Clone();
        if (move.moveType == MoveType.Movement)
        {
            if (isAIPlayer)
                newState.aiPosition = move.targetPosition;
            else
                newState.humanPosition = move.targetPosition;
        }
        else if (move.moveType == MoveType.WallPlacement)
        {
            newState.placedWalls.Add(new WallInfo { position = new Vector3(move.targetPosition.x, 0, move.targetPosition.y), isHorizontal = move.isHorizontalWall });
            if (isAIPlayer)
                newState.aiWallsLeft--;
            else
                newState.humanWallsLeft--;
        }
        return newState;
    }

    public static bool IsGameOverStatic(GameState state)
    {
        // AI thắng nếu aiPosition.y == 0, Human thắng nếu humanPosition.y == 8
        if (state.aiPosition.y == 0) return true;
        if (state.humanPosition.y == 8) return true;
        return false;
    }

    // Helper static cho self-play
    public static bool IsValidPositionStatic(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 9 && pos.y >= 0 && pos.y < 9;
    }
    public static bool IsBlockedByWallInfoStatic(Vector2Int from, Vector2Int to, List<WallInfo> walls)
    {
        // Đơn giản: chỉ kiểm tra có tường đúng vị trí giữa from-to không
        foreach (var wall in walls)
        {
            if (IsWallBlockingMovementStatic(from, to, wall))
                return true;
        }
        return false;
    }
    public static bool IsWallBlockingMovementStatic(Vector2Int from, Vector2Int to, WallInfo wall)
    {
        // Logic đơn giản: kiểm tra nếu wall nằm giữa from và to
        if (wall.isHorizontal)
        {
            // Tường ngang chặn di chuyển dọc
            if ((from.y < to.y || to.y < from.y) && from.x == wall.position.x && (from.y == wall.position.z || to.y == wall.position.z))
                return true;
        }
        else
        {
            // Tường dọc chặn di chuyển ngang
            if ((from.x < to.x || to.x < from.x) && from.y == wall.position.z && (from.x == wall.position.x || to.x == wall.position.x))
                return true;
        }
        return false;
    }
    public static bool IsValidWallPlacementStatic(AIMove wallMove, GameState state)
    {
        // Không đặt trùng vị trí tường đã có
        foreach (var wall in state.placedWalls)
        {
            if (wall.position.x == wallMove.targetPosition.x && wall.position.z == wallMove.targetPosition.y && wall.isHorizontal == wallMove.isHorizontalWall)
                return false;
        }
        // Có thể bổ sung kiểm tra chặn đường đi hợp lệ ở đây
        return true;
    }
    
    /// <summary>
    /// Debug Q-Learning system
    /// </summary>
    [ContextMenu("Debug Q-Learning System")]
    public void DebugQLearningSystem()
    {
        Debug.Log("=== QUORIDOR AI Q-LEARNING DEBUG ===");
        Debug.Log($"🎯 Player ID: {playerID}");
        Debug.Log($"🔧 Use Q-Learning: {useQLearning}");
        Debug.Log($"📂 Q-Table Path: {qTablePath}");
        Debug.Log($"📂 Full Q-Table Path: {System.IO.Path.GetFullPath(qTablePath)}");
        
        if (qAgent != null)
        {
            qAgent.DebugQTable();
        }
        else
        {
            Debug.LogError("❌ Q-Learning Agent is null!");
        }
    }

    /// <summary>
    /// Test Q-table serialization
    /// </summary>
    [ContextMenu("Test Q-Table Serialization")]
    public void TestQTableSerialization()
    {
        Debug.Log("🧪 Testing Q-table serialization...");
        
        if (qAgent != null)
        {
            qAgent.TestSerialization();
        }
        else
        {
            Debug.LogError("❌ Q-Learning Agent is null!");
        }
    }

    /// <summary>
    /// Force reload Q-table
    /// </summary>
    [ContextMenu("Reload Q-Table")]
    public void ReloadQTable()
    {
        Debug.Log("🔄 Force reloading Q-table...");
        
        if (qAgent != null)
        {
            string actualQTablePath = GetQTablePath();
            Debug.Log($"🔄 Using path: {actualQTablePath}");
            
            // Load Q-table
            qAgent.LoadQTable(actualQTablePath);
            
            // Check if loaded successfully and update flags
            int qTableSize = qAgent.GetQTableSize();
            Debug.Log($"📊 Q-table loaded with {qTableSize} states");
            
            if (qTableSize > 1000) // If has > 1000 states (trained)
            {
                qAgent.SetTrainedEpsilon(0.1f, 1000, 1000); // 10% exploration, trained
                qAgent.InitializeWithoutEpsilonReset(); // Preserve trained epsilon
                isTrainedModel = true; // Mark as trained model
                allowQTableSaving = false; // Don't save to avoid overwriting
                Debug.Log($"🎓 Detected trained Q-table with exploitation mode (ε=0.1)");
                Debug.Log($"🔒 Q-Learning will NOT retrain - using existing knowledge");
                Debug.Log($"🚫 Q-table saving DISABLED to preserve trained model");
            }
            else
            {
                // If no trained data, initialize normal
                qAgent.Initialize();
                isTrainedModel = false;
                allowQTableSaving = true; // Allow saving when training
                Debug.Log($"🆕 Starting fresh Q-Learning training");
                Debug.Log($"💾 Q-table saving ENABLED for training");
            }
        }
        else
        {
            Debug.LogError("❌ Q-Learning Agent is null!");
        }
    }

    /// <summary>
    /// Save current Q-table
    /// </summary>
    [ContextMenu("Save Q-Table")]
    public void SaveCurrentQTable()
    {
        Debug.Log("💾 Saving Q-table...");
        
        if (qAgent != null)
        {
            string actualQTablePath = GetQTablePath();
            qAgent.SaveQTable(actualQTablePath);
        }
        else
        {
            Debug.LogError("❌ Q-Learning Agent is null!");
        }
    }
    
    /// <summary>
    /// Reset epsilon về initial value
    /// </summary>
    [ContextMenu("Reset Epsilon")]
    public void ResetEpsilon()
    {
        Debug.Log("🔄 Resetting epsilon to initial value...");
        
        if (qAgent != null)
        {
            qAgent.ResetEpsilon();
        }
        else
        {
            Debug.LogError("❌ Q-Learning Agent is null!");
        }
    }
    
    /// <summary>
    /// Force decay epsilon một episode
    /// </summary>
    [ContextMenu("Force Epsilon Decay")]
    public void ForceEpsilonDecay()
    {
        Debug.Log("📉 Forcing epsilon decay...");
        
        if (qAgent != null)
        {
            qAgent.DecayEpsilonEpisode();
        }
        else
        {
            Debug.LogError("❌ Q-Learning Agent is null!");
        }
    }
    
    /// <summary>
    /// Enable training mode (allows Q-table saving)
    /// </summary>
    [ContextMenu("Enable Training Mode")]
    public void EnableTrainingMode()
    {
        allowQTableSaving = true;
        isTrainedModel = false;
        Debug.Log("🎓 Training mode ENABLED - Q-table will be saved");
    }
    
    /// <summary>
    /// Disable training mode (preserve trained model)
    /// </summary>
    [ContextMenu("Disable Training Mode")]
    public void DisableTrainingMode()
    {
        allowQTableSaving = false;
        isTrainedModel = true;
        Debug.Log("🔒 Training mode DISABLED - Q-table will be preserved");
    }
    
    /// <summary>
    /// Check current Q-Learning status
    /// </summary>
    [ContextMenu("Check Q-Learning Status")]
    public void CheckQLearningStatus()
    {
        Debug.Log("=== Q-LEARNING STATUS ===");
        Debug.Log($"🎯 Use Q-Learning: {useQLearning}");
        Debug.Log($"📊 Q-table size: {qAgent?.GetQTableSize() ?? 0} states");
        Debug.Log($"🎓 Is trained model: {isTrainedModel}");
        Debug.Log($"💾 Allow Q-table saving: {allowQTableSaving}");
        
        if (qAgent != null)
        {
            var epsilonInfo = qAgent.GetEpsilonInfo();
            Debug.Log($"🎲 Current epsilon: {epsilonInfo.currentEpsilon:F3}");
            Debug.Log($"📈 Training step: {epsilonInfo.currentStep}");
            Debug.Log($"📊 Training episode: {epsilonInfo.currentEpisode}");
        }
    }
}

// ========== DATA STRUCTURES ==========

/// <summary>
/// Trạng thái game cho AI
/// </summary>
[System.Serializable]
public class GameState
{
    public Vector2Int aiPosition;
    public Vector2Int humanPosition;
    public List<WallInfo> placedWalls = new List<WallInfo>();
    public int aiWallsLeft = 10;
    public int humanWallsLeft = 10;
    
    public GameState Clone()
    {
        var clone = new GameState();
        clone.aiPosition = aiPosition;
        clone.humanPosition = humanPosition;
        clone.placedWalls = new List<WallInfo>(placedWalls);
        clone.aiWallsLeft = aiWallsLeft;
        clone.humanWallsLeft = humanWallsLeft;
        return clone;
    }
}

/// <summary>
/// Thông tin wall cho AI
/// </summary>
[System.Serializable]
public class WallInfo
{
    public Vector3 position;
    public bool isHorizontal;
}

/// <summary>
/// AI move
/// </summary>
[System.Serializable]
public class AIMove
{
    public MoveType moveType;
    public Vector2Int targetPosition;
    public bool isHorizontalWall;
}

/// <summary>
/// Loại nước đi
/// </summary>
public enum MoveType
{
    Movement,
    WallPlacement
}

/// <summary>
/// A* Node
/// </summary>
public class AStarNode
{
    public Vector2Int position;
    public float gCost; // Distance from start
    public float hCost; // Distance to goal
    public float fCost; // Total cost
    
    public AStarNode(Vector2Int pos, float g, float h)
    {
        position = pos;
        gCost = g;
        hCost = h;
        fCost = g + h;
    }
}

// ========== DECISION TREE & ENTROPY STRUCTURES ==========

/// <summary>
/// Strategy decisions từ Decision Tree
/// </summary>
public enum StrategyDecision
{
    Aggressive,    // Tấn công, đặt wall block đối thủ
    Defensive,     // Phòng thủ, di chuyển về đích
    Balanced,      // Cân bằng giữa tấn công và phòng thủ
    Blocking       // Chuyên block bằng wall
}

/// <summary>
/// Features cho Decision Tree
/// </summary>
[System.Serializable]
public class DecisionFeatures
{
    public int aiDistanceToGoal;
    public int humanDistanceToGoal;
    public int aiWallsLeft;
    public int humanWallsLeft;
    public int aiMobility;
    public int humanMobility;
    public int distanceAdvantage; // humanDistance - aiDistance
    public int wallAdvantage;     // aiWalls - humanWalls
    public bool isEarlyGame;
    public bool isMidGame;
    public bool isEndGame;
}

/// <summary>
/// Decision Tree Classifier cho Quoridor AI
/// </summary>
public class DecisionTreeClassifier
{
    private List<DecisionNode> nodes;
    
    public void Initialize()
    {
        // Khởi tạo cây quyết định với các rules
        nodes = new List<DecisionNode>();
        BuildDecisionTree();
    }
    
    void BuildDecisionTree()
    {
        // Rule 1: Early Game - Balanced approach
        nodes.Add(new DecisionNode
        {
            condition = features => features.isEarlyGame,
            decision = StrategyDecision.Balanced,
            priority = 1
        });
        
        // Rule 2: End Game - Focus on reaching goal
        nodes.Add(new DecisionNode
        {
            condition = features => features.isEndGame,
            decision = StrategyDecision.Defensive,
            priority = 3
        });
        
        // Rule 3: AI is ahead - Block opponent
        nodes.Add(new DecisionNode
        {
            condition = features => features.distanceAdvantage > 2,
            decision = StrategyDecision.Blocking,
            priority = 2
        });
        
        // Rule 4: AI is behind - Aggressive push
        nodes.Add(new DecisionNode
        {
            condition = features => features.distanceAdvantage < -1,
            decision = StrategyDecision.Aggressive,
            priority = 2
        });
        
        // Rule 5: Low mobility - Defensive
        nodes.Add(new DecisionNode
        {
            condition = features => features.aiMobility <= 1,
            decision = StrategyDecision.Defensive,
            priority = 2
        });
        
        // Rule 6: High wall advantage - Use walls
        nodes.Add(new DecisionNode
        {
            condition = features => features.wallAdvantage > 3,
            decision = StrategyDecision.Blocking,
            priority = 2
        });
        
        // Rule 7: Low walls left - Conservative
        nodes.Add(new DecisionNode
        {
            condition = features => features.aiWallsLeft <= 2,
            decision = StrategyDecision.Defensive,
            priority = 2
        });
    }
    
    public StrategyDecision Predict(DecisionFeatures features)
    {
        // Tìm rule có priority cao nhất thỏa mãn
        var matchingNodes = nodes.Where(node => node.condition(features))
                                .OrderByDescending(node => node.priority)
                                .ToList();
        
        if (matchingNodes.Count > 0)
        {
            return matchingNodes.First().decision;
        }
        
        // Default: Balanced
        return StrategyDecision.Balanced;
    }
}

/// <summary>
/// Node trong Decision Tree
/// </summary>
public class DecisionNode
{
    public System.Func<DecisionFeatures, bool> condition;
    public StrategyDecision decision;
    public int priority;
}


