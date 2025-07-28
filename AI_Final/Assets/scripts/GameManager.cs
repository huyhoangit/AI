using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[System.Serializable]
public class SimpleGameState
{
    public int player1X = 4;
    public int player1Y = 0;
    public int player2X = 4;
    public int player2Y = 8;
    public int player1WallsLeft = 10;
    public int player2WallsLeft = 10;
    public int currentPlayer = 1;
    public List<SimpleWallInfo> walls = new List<SimpleWallInfo>();
}

[System.Serializable]
public class SimpleWallInfo
{
    public int x, y;
    public bool horizontal;
    
    public SimpleWallInfo(int wallX, int wallY, bool isHorizontal)
    {
        x = wallX;
        y = wallY;
        horizontal = isHorizontal;
    }
}

[System.Serializable]
public class SimpleAIMove
{
    public bool IsWallPlacement;
    public int WallX, WallY;
    public bool IsHorizontalWall;
    public int ToX, ToY;
    
    public SimpleAIMove(int toX, int toY)
    {
        IsWallPlacement = false;
        ToX = toX;
        ToY = toY;
    }
    
    public SimpleAIMove(int wallX, int wallY, bool isHorizontal)
    {
        IsWallPlacement = true;
        WallX = wallX;
        WallY = wallY;
        IsHorizontalWall = isHorizontal;
    }
}

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public bool playerVsAI = true;
    public int aiDepth = 3;
    
    [Header("UI References")]
    public Text currentPlayerText;
    public Text gameStatusText;
    public Text player1WallCountText;
    public Text player2WallCountText;
    public Button wallModeButton;
    public Button resetGameButton;
    
    [Header("Player References")]
    public ChessPlayer player1;
    public ChessPlayer player2;
    public WallPlacer wallPlacer1;
    public WallPlacer wallPlacer2;
    
    // Game state
    private int currentPlayer = 1; // 1 for player 1, 2 for player 2
    private bool gameEnded = false;
    private bool waitingForAI = false;
    
    // Singleton
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    public void InitializeGame()
    {
        // Setup UI
        SetupUI();
        
        // Initialize players
        if (player1 == null)
            player1 = Object.FindFirstObjectByType<ChessPlayer>();
        if (player2 == null)
        {
            ChessPlayer[] players = Object.FindObjectsByType<ChessPlayer>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p != player1)
                {
                    player2 = p;
                    break;
                }
            }
        }
        
        // Initialize wall placers
        if (wallPlacer1 == null || wallPlacer2 == null)
        {
            WallPlacer[] placers = Object.FindObjectsByType<WallPlacer>(FindObjectsSortMode.None);
            if (placers.Length >= 2)
            {
                wallPlacer1 = placers[0];
                wallPlacer2 = placers[1];
            }
        }
        
        // DEBUG: Log starting positions with more detail
        if (player1 != null)
        {
            Debug.Log($"🎮 GAME INIT: Player 1 starting position: X={player1.currentX}, Y={player1.currentY} (col={player1.col}, row={player1.row})");
            Debug.Log($"🎮 Player 1 Transform Position: {player1.transform.position}");
            Debug.Log($"🎮 Player 1 Goal: Reach row 8 (positive Z in world)");
        }
        if (player2 != null)
        {
            Debug.Log($"🤖 GAME INIT: Player 2 (AI) starting position: X={player2.currentX}, Y={player2.currentY} (col={player2.col}, row={player2.row})");
            Debug.Log($"🤖 Player 2 Transform Position: {player2.transform.position}");
            Debug.Log($"🤖 Player 2 Goal: Reach row 0 (negative Z in world)");
        }
        
        // Set initial game state
        currentPlayer = 1;
        gameEnded = false;
        waitingForAI = false;
        
        // Clear board occupancy first
        ClearBoardOccupancy();
        
        // Reset all wall slots to ensure clean start
        ResetAllWallSlots();
        
        // FORCE CORRECT STARTING POSITIONS if needed
        if (player1 != null && player1.currentY != 0)
        {
            Debug.LogWarning($"⚠️ Player 1 not at row 0! Current: {player1.currentY}. Fixing...");
            player1.SetInitialPosition(4, 0);
        }
        if (player2 != null && player2.currentY != 8)
        {
            Debug.LogWarning($"⚠️ Player 2 (AI) not at row 8! Current: {player2.currentY}. Fixing...");
            player2.SetInitialPosition(4, 8);
        }
        
        // Update board occupancy after setting positions
        UpdateBoardOccupancy();
        
        UpdateUI();
        
        LogGameState("Game Initialization");
        
        Debug.Log("GameManager initialized. Player vs AI: " + playerVsAI);
    }
    
    private void SetupUI()
    {
        // Setup wall mode button
        if (wallModeButton != null)
        {
            wallModeButton.onClick.RemoveAllListeners();
            wallModeButton.onClick.AddListener(ToggleWallMode);
        }
        
        // Setup reset button
        if (resetGameButton != null)
        {
            resetGameButton.onClick.RemoveAllListeners();
            resetGameButton.onClick.AddListener(Replay); // Dùng Replay để reset game
        }
    }
    
    public void OnPlayerMove(int playerId)
    {
        if (gameEnded || waitingForAI) return;
        
        if (playerId == currentPlayer)
        {
            Debug.Log($"Player {playerId} made a move");
            
            // Check win condition
            if (CheckWinCondition(playerId))
            {
                EndGame(playerId);
                return;
            }
            
            // Switch turns
            SwitchTurn();
        }
    }
    
    public void OnWallPlaced(int playerId)
    {
        if (gameEnded || waitingForAI) return;
        
        if (playerId == currentPlayer)
        {
            Debug.Log($"Player {playerId} placed a wall");
            
            // Switch turns
            SwitchTurn();
        }
    }
    
    private void SwitchTurn()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        UpdateUI();
        
        // DEBUG: Log turn switch
        Debug.Log($"🔄 Turn switched to Player {currentPlayer}");
        
        // If it's AI's turn and we're in player vs AI mode
        if (currentPlayer == 2 && playerVsAI && !gameEnded)
        {
            // SAFETY CHECK: Ensure AI isn't already at winning position
            if (player2 != null && player2.currentY == 0)
            {
                Debug.LogError($"❌ CRITICAL BUG: AI is already at winning position [{player2.currentX}, {player2.currentY}] at start of turn!");
                Debug.LogError($"❌ NOTE: Win condition should check Z=0 for AI, not Y=0!");
                EndGame(2);
                return;
            }
            
            StartCoroutine(ExecuteAITurn());
        }
    }
    
    private IEnumerator ExecuteAITurn()
    {
        waitingForAI = true;
        if (gameStatusText != null)
            SetUIText(gameStatusText, "AI is thinking...");
        
        LogGameState("AI Turn Start");
        
        // Log current AI position before thinking
        if (player2 != null)
        {
            Debug.Log($"🤖 AI TURN START: AI is at position [{player2.currentX}, {player2.currentY}]");
        }
        
        // Give a small delay for visual feedback
        yield return new WaitForSeconds(0.5f);
        
        try
        {
            // Get AI move using simple logic
            var aiMove = GetSimpleAIMove();
            
            if (aiMove != null)
            {
                // Execute AI move
                if (aiMove.IsWallPlacement)
                {
                    // Validate wall placement before attempting
                    if (wallPlacer2 != null && IsValidWallPlacement(aiMove.WallX, aiMove.WallY, aiMove.IsHorizontalWall))
                    {
                        Debug.Log($"🤖 AI attempting wall at ({aiMove.WallX}, {aiMove.WallY}), horizontal: {aiMove.IsHorizontalWall}");
                        wallPlacer2.PlaceWallForAI(aiMove.WallY, aiMove.WallX, aiMove.IsHorizontalWall);
                        Debug.Log($"✅ AI placed wall at ({aiMove.WallX}, {aiMove.WallY}), horizontal: {aiMove.IsHorizontalWall}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ AI tried invalid wall placement at ({aiMove.WallX}, {aiMove.WallY}), falling back to movement");
                        // Fallback to simple movement towards goal
                        if (player2 != null)
                        {
                            var fallbackMove = GetAnyValidMove(player2.currentX, player2.currentY);
                            if (fallbackMove != null)
                            {
                                player2.MoveTo(fallbackMove.ToX, fallbackMove.ToY);
                                Debug.Log($"🔄 AI fallback move to ({fallbackMove.ToX}, {fallbackMove.ToY})");
                            }
                        }
                    }
                }
                else
                {
                    // Move piece
                    if (player2 != null)
                    {
                        Debug.Log($"🤖 AI BEFORE MOVE: Position [{player2.currentX}, {player2.currentY}]");
                        player2.SetPosition(aiMove.ToX, aiMove.ToY);
                        Debug.Log($"🤖 AI AFTER MOVE: Position [{player2.currentX}, {player2.currentY}]");
                        Debug.Log($"🤖 AI moved to [{aiMove.ToX}, {aiMove.ToY}]");
                    }
                }
                
                // Check win condition AFTER move
                LogGameState("After AI Move");
                if (player2 != null)
                {
                    Debug.Log($"🏆 Checking win condition for AI at position [{player2.currentX}, {player2.currentY}]");
                    if (CheckWinCondition(2))
                    {
                        Debug.Log($"🏆 AI WINS! Final position: [{player2.currentX}, {player2.currentY}]");
                        EndGame(2);
                    }
                    else
                    {
                        Debug.Log($"🔄 AI turn complete, switching to Player 1");
                        // Switch back to player 1
                        currentPlayer = 1;
                        UpdateUI();
                    }
                }
                else
                {
                    currentPlayer = 1;
                    UpdateUI();
                }
            }
            else
            {
                Debug.LogError("AI failed to generate a move!");
                currentPlayer = 1;
                UpdateUI();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AI turn failed: {e.Message}");
            currentPlayer = 1;
            UpdateUI();
        }
        
        waitingForAI = false;
    }
    
    /// <summary>
    /// Smart AI logic with enhanced strategic thinking
    /// </summary>
    private SimpleAIMove GetSimpleAIMove()
    {
        if (player2 == null) return null;
        
        int currentX = player2.currentX;
        int currentY = player2.currentY;
        
        Debug.Log($"🤖 AI thinking from position [{currentX}, {currentY}]");
        
        // Store last positions to prevent oscillation
        Vector2Int currentPos = new Vector2Int(currentX, currentY);
        AddToAIHistory(currentPos);
        
        // PRIORITY 1: Check if AI can win immediately
        var winningMove = GetWinningMove();
        if (winningMove != null)
        {
            Debug.Log("🏆 AI found winning move!");
            return winningMove;
        }
        
        // PRIORITY 2: Check if need to block player from winning
        var criticalBlock = GetCriticalBlockingMove();
        if (criticalBlock != null)
        {
            Debug.Log("🚨 AI making critical blocking move!");
            return criticalBlock;
        }
        
        // PRIORITY 3: Try to use advanced QuoridorAI with minimax
        var minimaxMove = TryGetMinimaxMove();
        if (minimaxMove != null)
        {
            Debug.Log("🧠 Using Minimax strategic move");
            return minimaxMove;
        }
        
        // PRIORITY 4: Enhanced strategic decision making
        return GetEnhancedStrategicMove();
    }
    
    // AI History to prevent oscillation
    private List<Vector2Int> aiMoveHistory = new List<Vector2Int>();
    private const int MAX_HISTORY = 4;
    
    private void AddToAIHistory(Vector2Int pos)
    {
        aiMoveHistory.Add(pos);
        if (aiMoveHistory.Count > MAX_HISTORY)
        {
            aiMoveHistory.RemoveAt(0);
        }
    }
    
    private bool IsOscillating(Vector2Int targetPos)
    {
        if (aiMoveHistory.Count < 3) return false;
        
        // Check if we're moving back and forth between 2 positions
        var recent = aiMoveHistory.GetRange(aiMoveHistory.Count - 3, 3);
        return recent[0] == targetPos && recent[1] != targetPos && recent[2] == recent[0];
    }
    
    /// <summary>
    /// Try to get move from advanced QuoridorAI component - Updated for better integration
    /// </summary>
    private SimpleAIMove TryGetAdvancedAIMove()
    {
        var quoridorAI = FindFirstObjectByType<QuoridorAI>();
        if (quoridorAI != null)
        {
            try
            {
                Debug.Log("🧠 Using advanced QuoridorAI with Minimax for move selection");
                
                // Use reflection to call FindBestMove
                var findBestMoveMethod = quoridorAI.GetType().GetMethod("FindBestMove");
                if (findBestMoveMethod != null)
                {
                    var aiMove = findBestMoveMethod.Invoke(quoridorAI, null);
                    if (aiMove != null)
                    {
                        // Convert AIMove to SimpleAIMove using reflection
                        var moveTypeField = aiMove.GetType().GetField("moveType");
                        if (moveTypeField != null)
                        {
                            var moveTypeValue = moveTypeField.GetValue(aiMove);
                            
                            // Check if it's a movement or wall placement
                            if (moveTypeValue.ToString() == "Movement")
                            {
                                var targetPosField = aiMove.GetType().GetField("targetPosition");
                                if (targetPosField != null)
                                {
                                    Vector2Int targetPos = (Vector2Int)targetPosField.GetValue(aiMove);
                                    Debug.Log($"🧠 QuoridorAI Minimax suggests movement to [{targetPos.x}, {targetPos.y}]");
                                    return new SimpleAIMove(targetPos.x, targetPos.y);
                                }
                            }
                            else if (moveTypeValue.ToString() == "WallPlacement")
                            {
                                var targetPosField = aiMove.GetType().GetField("targetPosition");
                                var isHorizontalField = aiMove.GetType().GetField("isHorizontalWall");
                                
                                if (targetPosField != null && isHorizontalField != null)
                                {
                                    Vector2Int wallPos = (Vector2Int)targetPosField.GetValue(aiMove);
                                    bool isHorizontal = (bool)isHorizontalField.GetValue(aiMove);
                                    
                                    Debug.Log($"� QuoridorAI Minimax suggests strategic wall at [{wallPos.x}, {wallPos.y}], horizontal: {isHorizontal}");
                                    return new SimpleAIMove(wallPos.x, wallPos.y, isHorizontal);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Advanced QuoridorAI failed: {e.Message}");
                Debug.LogWarning($"⚠️ Stack trace: {e.StackTrace}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ QuoridorAI component not found! Make sure it's added to the scene.");
        }
        
        return null;
    }
    
    /// <summary>
    /// Pathfinding-based AI movement with enhanced wall strategy
    /// </summary>
    private SimpleAIMove GetPathfindingAIMove()
    {
        // Calculate strategic priorities
        bool playerCloseToGoal = IsPlayerCloseToGoal();
        bool aiHasWallAdvantage = HasWallAdvantage();
        
        // Enhanced wall placement probability based on game state
        float wallPlacementChance = CalculateWallPlacementProbability(playerCloseToGoal, aiHasWallAdvantage);
        
        Debug.Log($"🎲 Wall placement probability: {wallPlacementChance:F2} (player close: {playerCloseToGoal}, wall advantage: {aiHasWallAdvantage})");
        
        // Try strategic wall placement first if conditions are right
        if (wallPlacer2 != null && wallPlacer2.wallsLeft > 0 && Random.Range(0f, 1f) < wallPlacementChance)
        {
            var wallMove = GetStrategicWallMove();
            if (wallMove != null)
            {
                Debug.Log("🧱 AI prioritizing strategic wall placement");
                return wallMove;
            }
        }
        
        // Try enhanced pathfinding movement
        var enhancedMove = GetPathfindingAIMoveEnhanced();
        if (enhancedMove != null) return enhancedMove;
        
        // Fallback to strategic wall placement if movement failed
        if (wallPlacer2 != null && wallPlacer2.wallsLeft > 0)
        {
            var wallMove = GetStrategicWallMove();
            if (wallMove != null)
            {
                Debug.Log("🧱 AI using fallback wall placement");
                return wallMove;
            }
        }
        
        // Last resort: any valid move
        return GetAnyValidMove(player2.currentX, player2.currentY);
    }
    
    /// <summary>
    /// Check if player is close to winning
    /// </summary>
    private bool IsPlayerCloseToGoal()
    {
        if (player1 == null) return false;
        
        int distanceToGoal = 8 - player1.currentY; // Distance to row 8
        return distanceToGoal <= 3; // Close if 3 steps or less
    }
    
    /// <summary>
    /// Check if AI is close to winning
    /// </summary>
    private bool IsAICloseToGoal()
    {
        if (player2 == null) return false;
        return player2.currentY <= 2; // AI's goal is Y=0
    }
    
    /// <summary>
    /// Check if AI has wall advantage
    /// </summary>
    private bool HasWallAdvantage()
    {
        if (wallPlacer1 == null || wallPlacer2 == null) return false;
        
        return wallPlacer2.wallsLeft >= wallPlacer1.wallsLeft;
    }
    
    /// <summary>
    /// Calculate dynamic wall placement probability
    /// </summary>
    private float CalculateWallPlacementProbability(bool playerCloseToGoal, bool aiHasWallAdvantage)
    {
        float baseProbability = 0.4f; // Base 40% chance
        
        if (playerCloseToGoal)
            baseProbability += 0.3f; // +30% if player is close to winning
        
        if (aiHasWallAdvantage)
            baseProbability += 0.2f; // +20% if AI has more walls
        
        // Check if AI is also close to goal (reduce wall placement)
        if (player2 != null && player2.currentY <= 2)
            baseProbability -= 0.2f; // -20% if AI is close to winning
        
        return Mathf.Clamp01(baseProbability);
    }
    
    /// <summary>
    /// Enhanced pathfinding with direction preference
    /// </summary>
    private SimpleAIMove GetPathfindingAIMoveEnhanced()
    {
        int currentX = player2.currentX;
        int currentY = player2.currentY;
        Vector2Int currentPos = new Vector2Int(currentX, currentY);
        Vector2Int goalPos = new Vector2Int(4, 0); // AI goal
        
        // First try direct approach (preferred direction)
        Vector2Int directMove = GetDirectMoveTowardsGoal(currentPos, goalPos);
        if (directMove != Vector2Int.zero && !IsOscillating(directMove))
        {
            if (IsValidAIMove(currentX, currentY, directMove.x, directMove.y))
            {
                Debug.Log($"🎯 AI direct move to [{directMove.x}, {directMove.y}]");
                return new SimpleAIMove(directMove.x, directMove.y);
            }
        }
        
        // If direct move failed, use A* pathfinding
        var path = FindPathToGoal(currentPos, goalPos);
        if (path != null && path.Count > 1)
        {
            Vector2Int nextStep = path[1];
            
            if (!IsOscillating(nextStep) && IsValidAIMove(currentX, currentY, nextStep.x, nextStep.y))
            {
                Debug.Log($"🗺️ AI pathfinding move to [{nextStep.x}, {nextStep.y}]");
                return new SimpleAIMove(nextStep.x, nextStep.y);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get direct move towards goal
    /// </summary>
    private Vector2Int GetDirectMoveTowardsGoal(Vector2Int from, Vector2Int to)
    {
        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;
        
        // Prioritize Y movement (towards goal row 0)
        if (deltaY < 0)
        {
            return new Vector2Int(from.x, from.y - 1); // Move down
        }
        else if (deltaY > 0)
        {
            return new Vector2Int(from.x, from.y + 1); // Move up (away from goal, low priority)
        }
        
        // If Y is aligned, move in X direction
        if (deltaX < 0)
        {
            return new Vector2Int(from.x - 1, from.y); // Move left
        }
        else if (deltaX > 0)
        {
            return new Vector2Int(from.x + 1, from.y); // Move right
        }
        
        return Vector2Int.zero; // Already at goal
    }
    
    /// <summary>
    /// Debug AI state
    /// </summary>
    [ContextMenu("Debug AI State")]
    public void DebugAIState()
    {
        if (player2 != null)
        {
            Debug.Log($"🤖 AI State Debug:");
            Debug.Log($"   Position: [{player2.currentX}, {player2.currentY}]");
            Debug.Log($"   Move History: {string.Join(", ", aiMoveHistory)}");
            
            // Show available moves
            Vector2Int[] directions = {
                new Vector2Int(0, -1), new Vector2Int(-1, 0), 
                new Vector2Int(1, 0), new Vector2Int(0, 1)
            };
            
            foreach (var dir in directions)
            {
                int newX = player2.currentX + dir.x;
                int newY = player2.currentY + dir.y;
                bool valid = IsValidAIMove(player2.currentX, player2.currentY, newX, newY);
                string status = valid ? "✅" : "❌";
                Debug.Log($"   {status} Move to [{newX}, {newY}]");
            }
        }
    }

    // Helper methods for UI interaction without direct UI dependencies
    void SetUIText(Component textComponent, string text)
    {
        if (textComponent == null) return;
        
        // Use reflection to set text property
        var textProperty = textComponent.GetType().GetProperty("text");
        if (textProperty != null)
        {
            textProperty.SetValue(textComponent, text);
        }
    }
    
    void SetupButtonListener(Component button, System.Action action)
    {
        if (button == null || action == null) return;
        
        // Use reflection to setup button click listeners
        var onClickField = button.GetType().GetField("onClick");
        if (onClickField != null)
        {
            var onClick = onClickField.GetValue(button);
            var addListenerMethod = onClick.GetType().GetMethod("AddListener", new[] { typeof(UnityEngine.Events.UnityAction) });
            if (addListenerMethod != null)
            {
                UnityEngine.Events.UnityAction unityAction = () => action();
                addListenerMethod.Invoke(onClick, new object[] { unityAction });
            }
        }
    }

    /// <summary>
    /// Find alternative move when oscillation detected
    /// </summary>
    private SimpleAIMove GetAlternativeMove(int currentX, int currentY, Vector2Int blockedMove)
    {
        Vector2Int[] allDirections = {
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left  
            new Vector2Int(1, 0),   // Right
            new Vector2Int(0, 1)    // Up
        };
        
        // Try all directions except the blocked one
        foreach (var dir in allDirections)
        {
            Vector2Int newPos = new Vector2Int(currentX + dir.x, currentY + dir.y);
            
            if (newPos == blockedMove) continue; // Skip the oscillating move
            
            if (IsValidAIMove(currentX, currentY, newPos.x, newPos.y))
            {
                // Prefer moves that get closer to goal
                int goalDistance = Mathf.Abs(newPos.y - 0); // Distance to row 0
                int currentDistance = Mathf.Abs(currentY - 0);
                
                if (goalDistance <= currentDistance)
                {
                    Debug.Log($"🔀 AI found alternative move to [{newPos.x}, {newPos.y}]");
                    return new SimpleAIMove(newPos.x, newPos.y);
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get any valid move as last resort
    /// </summary>
    private SimpleAIMove GetAnyValidMove(int currentX, int currentY)
    {
        Vector2Int[] directions = {
            new Vector2Int(0, -1),  // Down (towards goal)
            new Vector2Int(-1, 0),  // Left  
            new Vector2Int(1, 0),   // Right
            new Vector2Int(0, 1)    // Up (away from goal)
        };
        
        foreach (var dir in directions)
        {
            int newX = currentX + dir.x;
            int newY = currentY + dir.y;
            
            if (IsValidAIMove(currentX, currentY, newX, newY))
            {
                Debug.Log($"🆘 AI last resort move to [{newX}, {newY}]");
                return new SimpleAIMove(newX, newY);
            }
        }
        
        Debug.LogWarning("❌ AI has no valid moves!");
        return null;
    }
    
    /// <summary>
    /// Validate AI move
    /// </summary>
    private bool IsValidAIMove(int fromX, int fromY, int toX, int toY)
    {
        // Check bounds
        if (toX < 0 || toX >= 9 || toY < 0 || toY >= 9) return false;
        
        // Check if only one step
        int deltaX = Mathf.Abs(toX - fromX);
        int deltaY = Mathf.Abs(toY - fromY);
        if (deltaX + deltaY != 1) return false;
        
        // Check if occupied by player 1
        if (player1 != null && player1.currentX == toX && player1.currentY == toY) return false;
        
        // Check if blocked by walls
        if (IsBlockedByWallAI(fromX, fromY, toX, toY)) return false;
        
        return true;
    }
    
    /// <summary>
    /// Get move priority (higher number = better move)
    /// </summary>
    private int GetMovePriority(int fromX, int fromY, int toX, int toY)
    {
        int deltaY = toY - fromY;
        
        // AI wants to reach row 0, so moving down (negative Y) is best
        if (deltaY == -1) return 10; // Down - highest priority
        if (deltaY == 0) return 5;   // Left/Right - medium priority  
        if (deltaY == 1) return 1;   // Up - lowest priority
        
        return 0;
    }
    
    /// <summary>
    /// Try to place a wall - now uses strategic placement
    /// </summary>
    private SimpleAIMove TryPlaceWall()
    {
        // Use strategic wall placement instead of random
        return GetStrategicWallMove();
    }
    /// <summary>
    /// Check if wall placement is valid - Enhanced version
    /// </summary>
    private bool IsValidWallPlacement(int x, int y, bool isHorizontal)
    {
        // Basic bounds check
        if (isHorizontal)
        {
            if (x < 0 || x >= 8 || y < 0 || y >= 8) return false;
        }
        else
        {
            if (x < 0 || x >= 8 || y < 0 || y >= 8) return false;
        }
        
        // Check if AI has walls left
        if (wallPlacer2 == null || wallPlacer2.wallsLeft <= 0)
        {
            Debug.LogWarning($"❌ AI has no walls left: {(wallPlacer2?.wallsLeft ?? 0)}");
            return false;
        }
        
        // Check if wall already exists using WallPlacer's validation
        if (IsWallAlreadyPlacedEnhanced(x, y, isHorizontal))
        {
            Debug.LogWarning($"❌ Wall already placed at ({x}, {y}), horizontal: {isHorizontal}");
            return false;
        }
        
        // Check if wall would completely block a player's path
        if (!WillPlayersHavePathAfterWall(x, y, isHorizontal))
        {
            Debug.LogWarning($"❌ Wall would block player path completely at ({x}, {y})");
            return false;
        }
        
        Debug.Log($"✅ Wall placement valid at ({x}, {y}), horizontal: {isHorizontal}");
        return true;
    }
    
    /// <summary>
    /// Enhanced check if wall already exists at position using WallPlacer's data
    /// </summary>
    private bool IsWallAlreadyPlacedEnhanced(int x, int y, bool isHorizontal)
    {
        // Method 1: Use WallPlacer's IsWallPlacedAt method
        if (wallPlacer2 != null)
        {
            try
            {
                var wallPlacerType = wallPlacer2.GetType();
                var isWallPlacedMethod = wallPlacerType.GetMethod("IsWallPlacedAt");
                if (isWallPlacedMethod != null)
                {
                    // WallPlacer uses (row, col) format, so (y, x)
                    var isAlreadyPlaced = (bool)isWallPlacedMethod.Invoke(wallPlacer2, new object[] { y, x, isHorizontal });
                    if (isAlreadyPlaced)
                    {
                        Debug.Log($"🔍 WallPlacer confirms wall exists at ({x}, {y})");
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ WallPlacer validation failed: {e.Message}");
            }
        }
        
        // Method 2: Check wallPlaced array directly
        var wallSlotIndex = CalculateWallSlotIndex(x, y, isHorizontal);
        if (wallSlotIndex != -1 && wallPlacer2 != null)
        {
            try
            {
                var wallPlacerType = wallPlacer2.GetType();
                var wallPlacedField = wallPlacerType.GetField("wallPlaced", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (wallPlacedField != null)
                {
                    var wallPlacedArray = wallPlacedField.GetValue(wallPlacer2) as bool[];
                    if (wallPlacedArray != null && wallSlotIndex < wallPlacedArray.Length)
                    {
                        if (wallPlacedArray[wallSlotIndex])
                        {
                            Debug.Log($"🔍 Wall slot {wallSlotIndex} is occupied at ({x}, {y})");
                            return true;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Direct wall slot check failed: {e.Message}");
            }
        }
        
        // Method 3: Fallback to checking physical walls (original method)
        return IsWallAlreadyPlacedPhysical(x, y, isHorizontal);
    }
    
    /// <summary>
    /// Check physical walls in scene
    /// </summary>
    private bool IsWallAlreadyPlacedPhysical(int x, int y, bool isHorizontal)
    {
        // Find all placed walls
        GameObject[] placedWalls1 = GameObject.FindGameObjectsWithTag("PlacedWall");
        GameObject[] placedWalls2 = GameObject.FindGameObjectsWithTag("Wall");
        
        // Combine arrays
        GameObject[] allWalls = new GameObject[placedWalls1.Length + placedWalls2.Length];
        placedWalls1.CopyTo(allWalls, 0);
        placedWalls2.CopyTo(allWalls, placedWalls1.Length);
        
        foreach (GameObject wall in allWalls)
        {
            if (wall == null) continue;
            
            // Convert world position back to grid position
            Vector3 wallPos = wall.transform.position;
            bool wallIsHorizontal = IsHorizontalWallAI(wall);
            
            // Calculate expected grid position for this wall
            Vector3 boardStart = new Vector3(-5.0f, 0f, -4.85f);
            float stepSize = 1.1f;
            
            int wallGridX = Mathf.RoundToInt((wallPos.x - boardStart.x) / stepSize);
            int wallGridY = Mathf.RoundToInt((wallPos.z - boardStart.z) / stepSize);
            
            // Check if this matches our intended placement
            if (wallGridX == x && wallGridY == y && wallIsHorizontal == isHorizontal)
            {
                Debug.Log($"🔍 Physical wall found at ({x}, {y}): {wall.name}");
                return true; // Wall already exists here
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Calculate wall slot index using same logic as WallPlacer
    /// </summary>
    private int CalculateWallSlotIndex(int wallX, int wallY, bool isHorizontal)
    {
        try
        {
            // Use same calculation as WallPlacer.GetSlotIndex
            int slotsPerRow = 16; // 8 horizontal + 8 vertical per row
            int rowIndex = wallY;
            int slotInRow;
            
            if (isHorizontal)
            {
                slotInRow = wallX; // Horizontal walls: 0-7
            }
            else
            {
                slotInRow = 8 + wallX; // Vertical walls: 8-15
            }
            
            int slotIndex = rowIndex * slotsPerRow + slotInRow;
            return slotIndex;
        }
        catch
        {
            return -1;
        }
    }
    
    /// <summary>
    /// Check if both players will have path to goal after placing wall
    /// </summary>
    private bool WillPlayersHavePathAfterWall(int wallX, int wallY, bool isHorizontal)
    {
        // Simple check: if we're trying to place too many walls in one area, it might block paths
        int nearbyWalls = CountNearbyWalls(wallX, wallY, 2);
        if (nearbyWalls >= 4)
        {
            Debug.LogWarning($"⚠️ Too many walls nearby ({nearbyWalls}) - might block paths");
            return false;
        }
        
        // For now, allow most placements - full pathfinding check would be expensive
        return true;
    }
    
    /// <summary>
    /// Count walls within a radius
    /// </summary>
    private int CountNearbyWalls(int centerX, int centerY, int radius)
    {
        int count = 0;
        
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x >= 0 && x < 8 && y >= 0 && y < 8)
                {
                    if (IsWallAlreadyPlacedPhysical(x, y, true) || IsWallAlreadyPlacedPhysical(x, y, false))
                    {
                        count++;
                    }
                }
            }
        }
        
        return count;
    }
    
    private bool CheckWinCondition(int playerId)
    {
        if (playerId == 1 && player1 != null)
        {
            // Player 1 wins by reaching the top row (y = 8)
            Debug.Log($"🏆 Checking win for Player 1 at position [{player1.currentX}, {player1.currentY}]. Goal: Y = 8");
            return player1.currentY == 8;
        }
        else if (playerId == 2 && player2 != null)
        {
            // Player 2 wins by reaching the bottom row (y = 0)
            Debug.Log($"🏆 Checking win for Player 2 (AI) at position [{player2.currentX}, {player2.currentY}]. Goal: Y = 0");
            return player2.currentY == 0;
        }
        
        return false;
    }
    
    private void EndGame(int winner)
    {
        gameEnded = true;
        
        if (gameStatusText != null)
        {
            if (winner == 1)
                SetUIText(gameStatusText, "Player 1 Wins!");
            else if (winner == 2)
                SetUIText(gameStatusText, playerVsAI ? "AI Wins!" : "Player 2 Wins!");
        }
        
        Debug.Log($"Game ended. Winner: Player {winner}");
        
        // Disable further interactions
        if (player1 != null) player1.enabled = false;
        if (player2 != null) player2.enabled = false;
        if (wallPlacer1 != null) wallPlacer1.enabled = false;
        if (wallPlacer2 != null) wallPlacer2.enabled = false;
    }
    
    private void UpdateUI()
    {
        if (currentPlayerText != null)
        {
            if (currentPlayer == 1)
                SetUIText(currentPlayerText, "Current Player: Player 1");
            else
                SetUIText(currentPlayerText, playerVsAI ? "Current Player: AI" : "Current Player: Player 2");
        }
        
        if (gameStatusText != null && !gameEnded)
        {
            if (waitingForAI)
                SetUIText(gameStatusText, "AI is thinking...");
            else
                SetUIText(gameStatusText, "Game in Progress");
        }
        
        // Update wall counts
        if (player1WallCountText != null && wallPlacer1 != null)
            SetUIText(player1WallCountText, $"Player 1 Walls: {wallPlacer1.wallsLeft}");
        
        if (player2WallCountText != null && wallPlacer2 != null)
            SetUIText(player2WallCountText, $"{(playerVsAI ? "AI" : "Player 2")} Walls: {wallPlacer2.wallsLeft}");
    }
    
    private void ToggleWallMode()
    {
        if (gameEnded || waitingForAI) return;
        
        if (currentPlayer == 1 && wallPlacer1 != null)
        {
            wallPlacer1.ToggleWallPlacementMode();
        }
        else if (currentPlayer == 2 && !playerVsAI && wallPlacer2 != null)
        {
            wallPlacer2.ToggleWallPlacementMode();
        }
    }
    
    private void ResetGame()
    {
        Debug.Log("🔄 Resetting game...");
        
        // Reset game state
        gameEnded = false;
        waitingForAI = false;
        currentPlayer = 1;
        
        // Clear AI move history
        aiMoveHistory.Clear();
        
        // Clear board occupancy first
        ClearBoardOccupancy();
        
        // Reset all wall slots to ensure clean start
        ResetAllWallSlots();
        
        // Reset wall placers BEFORE resetting positions
        if (wallPlacer1 != null)
        {
            wallPlacer1.ResetWalls();
            wallPlacer1.enabled = true;
            Debug.Log("✅ Reset Player 1 walls");
        }
        if (wallPlacer2 != null)
        {
            wallPlacer2.ResetWalls();
            wallPlacer2.enabled = true;
            Debug.Log("✅ Reset Player 2 walls");
        }
        
        // Reset player positions using SetInitialPosition instead of SetPosition
        if (player1 != null)
        {
            player1.SetInitialPosition(4, 0); // Bottom center - use SetInitialPosition!
            player1.enabled = true;
            Debug.Log($"✅ Reset Player 1 to position [{player1.currentX}, {player1.currentY}]");
        }
        if (player2 != null)
        {
            player2.SetInitialPosition(4, 8); // Top center - use SetInitialPosition!
            player2.enabled = true;
            Debug.Log($"✅ Reset Player 2 to position [{player2.currentX}, {player2.currentY}]");
        }
        
        // Update board occupancy after setting positions
        UpdateBoardOccupancy();
        
        // Remove all physical walls from scene
        RemoveAllPhysicalWalls();
        
        UpdateUI();
        
        Debug.Log("✅ Game reset complete");
        LogGameState("Game Reset");
    }
    
    /// <summary>
    /// Reset all wall slots to clear state
    /// </summary>
    private void ResetAllWallSlots()
    {
        Debug.Log("🧹 Resetting all wall slots...");
        
        // Reset both wall placers' wall arrays
        ResetWallPlacerSlots(wallPlacer1, "Player 1");
        ResetWallPlacerSlots(wallPlacer2, "Player 2");
    }
    
    /// <summary>
    /// Reset a specific wall placer's slots
    /// </summary>
    private void ResetWallPlacerSlots(WallPlacer wallPlacer, string playerName)
    {
        if (wallPlacer == null) return;
        
        try
        {
            var wallPlacerType = wallPlacer.GetType();
            
            // Reset wallPlaced array
            var wallPlacedField = wallPlacerType.GetField("wallPlaced", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (wallPlacedField != null)
            {
                var wallPlacedArray = wallPlacedField.GetValue(wallPlacer) as bool[];
                if (wallPlacedArray != null)
                {
                    for (int i = 0; i < wallPlacedArray.Length; i++)
                    {
                        wallPlacedArray[i] = false;
                    }
                    Debug.Log($"✅ Reset {wallPlacedArray.Length} wall slots for {playerName}");
                }
            }
            
            // Reset walls left count
            var wallsLeftField = wallPlacerType.GetField("wallsLeft");
            if (wallsLeftField != null)
            {
                wallsLeftField.SetValue(wallPlacer, 10);
                Debug.Log($"✅ Reset walls count to 10 for {playerName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Failed to reset wall slots for {playerName}: {e.Message}");
        }
    }
    
    /// <summary>
    /// Remove all physical wall objects from the scene
    /// </summary>
    private void RemoveAllPhysicalWalls()
    {
        Debug.Log("🧹 Removing all physical walls from scene...");
        
        // Find all placed walls
        GameObject[] placedWalls1 = GameObject.FindGameObjectsWithTag("PlacedWall");
        GameObject[] placedWalls2 = GameObject.FindGameObjectsWithTag("Wall");
        
        int removedCount = 0;
        
        // Remove PlacedWall tagged objects
        foreach (GameObject wall in placedWalls1)
        {
            if (wall != null)
            {
                Destroy(wall);
                removedCount++;
            }
        }
        
        // Remove Wall tagged objects that were placed during game
        foreach (GameObject wall in placedWalls2)
        {
            if (wall != null && wall.name.Contains("Wall_"))
            {
                Destroy(wall);
                removedCount++;
            }
        }
        
        Debug.Log($"✅ Removed {removedCount} physical walls from scene");
    }
    
    // Public methods for other scripts to call
    public bool IsCurrentPlayer(int playerId)
    {
        return currentPlayer == playerId && !gameEnded && !waitingForAI;
    }
    
    public bool IsGameEnded()
    {
        return gameEnded;
    }
    
    public bool IsWaitingForAI()
    {
        return waitingForAI;
    }
    
    /// <summary>
    /// Check if AI move is blocked by walls (similar to ChessPlayer logic)
    /// </summary>
    private bool IsBlockedByWallAI(int fromCol, int fromRow, int toCol, int toRow)
    {
        // Find all placed walls
        GameObject[] placedWalls1 = GameObject.FindGameObjectsWithTag("PlacedWall");
        GameObject[] placedWalls2 = GameObject.FindGameObjectsWithTag("Wall");
        
        // Combine arrays
        GameObject[] allWalls = new GameObject[placedWalls1.Length + placedWalls2.Length];
        placedWalls1.CopyTo(allWalls, 0);
        placedWalls2.CopyTo(allWalls, placedWalls1.Length);
        
        if (allWalls.Length == 0) return false; // No walls
        
        Debug.Log($"🔍 AI checking wall blocking from [{fromCol},{fromRow}] to [{toCol},{toRow}]. Found {allWalls.Length} walls");
        
        // Determine movement direction
        int deltaCol = toCol - fromCol;
        int deltaRow = toRow - fromRow;
        
        foreach (GameObject wall in allWalls)
        {
            if (wall == null) continue;
            
            Vector3 wallPos = wall.transform.position;
            bool isHorizontalWall = IsHorizontalWallAI(wall);
            
            // Check if this wall blocks the movement
            if (IsWallBlockingAI(fromCol, fromRow, deltaCol, deltaRow, wallPos, isHorizontalWall))
            {
                Debug.Log($"🚫 AI movement blocked by wall '{wall.name}' at {wallPos}");
                return true; // Wall blocks movement
            }
        }
        
        return false; // No walls block movement
    }
    
    /// <summary>
    /// Check if wall is horizontal (for AI)
    /// </summary>
    private bool IsHorizontalWallAI(GameObject wall)
    {
        // Check name first
        if (wall.name.Contains("_H_") || wall.name.EndsWith("_H"))
        {
            return true;
        }
        if (wall.name.Contains("_V_") || wall.name.EndsWith("_V"))
        {
            return false;
        }
        
        // Fallback: check scale
        Vector3 scale = wall.transform.localScale;
        return scale.x > scale.z; // Horizontal wall: longer in X direction
    }
    
    /// <summary>
    /// Check if a specific wall blocks AI movement
    /// </summary>
    private bool IsWallBlockingAI(int fromCol, int fromRow, int deltaCol, int deltaRow, Vector3 wallPos, bool isHorizontalWall)
    {
        // Use same board start position as WallPlacer
        Vector3 boardStart = new Vector3(-5.0f, 0f, -4.85f);
        float squareSize = 1.0f;
        float squareSpacing = 0.1f;
        float stepSize = squareSize + squareSpacing; // 1.1f
        
        float tolerance = 0.7f;
        
        Vector3 expectedWallPos = Vector3.zero;
        bool shouldBeBlocked = false;
        
        if (deltaCol == 1) // Moving RIGHT
        {
            // Check vertical wall on right edge of current square
            expectedWallPos = new Vector3(
                boardStart.x + (fromCol + 1) * stepSize - squareSpacing * 0.5f,
                0.1f,
                boardStart.z + fromRow * stepSize + squareSize * 0.5f
            );
            shouldBeBlocked = !isHorizontalWall && Vector3.Distance(wallPos, expectedWallPos) < tolerance;
        }
        else if (deltaCol == -1) // Moving LEFT
        {
            // Check vertical wall on left edge of current square
            expectedWallPos = new Vector3(
                boardStart.x + fromCol * stepSize - squareSpacing * 0.5f,
                0.1f,
                boardStart.z + fromRow * stepSize + squareSize * 0.5f
            );
            shouldBeBlocked = !isHorizontalWall && Vector3.Distance(wallPos, expectedWallPos) < tolerance;
        }
        else if (deltaRow == 1) // Moving UP
        {
            // Check horizontal wall on top edge of current square
            expectedWallPos = new Vector3(
                boardStart.x + fromCol * stepSize + squareSize * 0.5f,
                0.1f,
                boardStart.z + (fromRow + 1) * stepSize - squareSpacing * 0.5f
            );
            shouldBeBlocked = isHorizontalWall && Vector3.Distance(wallPos, expectedWallPos) < tolerance;
        }
        else if (deltaRow == -1) // Moving DOWN
        {
            // Check horizontal wall on bottom edge of current square
            expectedWallPos = new Vector3(
                boardStart.x + fromCol * stepSize + squareSize * 0.5f,
                0.1f,
                boardStart.z + fromRow * stepSize - squareSpacing * 0.5f
            );
            shouldBeBlocked = isHorizontalWall && Vector3.Distance(wallPos, expectedWallPos) < tolerance;
        }
        
        return shouldBeBlocked;
    }
    
    /// <summary>
    /// Check if a move is valid (simplified version)
    /// </summary>
    private bool IsValidMoveSimplified(int fromX, int fromY, int toX, int toY)
    {
        // Check bounds
        if (toX < 0 || toX >= 9 || toY < 0 || toY >= 9)
            return false;
        
        // Check if destination is occupied by other player
        if (player1 != null && player1.currentX == toX && player1.currentY == toY)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Log complete game state for debugging
    /// </summary>
    private void LogGameState(string context)
    {
        Debug.Log($"🎮 GAME STATE [{context}]:");
        Debug.Log($"   Current Player: {currentPlayer}");
        Debug.Log($"   Game Ended: {gameEnded}");
        Debug.Log($"   Waiting for AI: {waitingForAI}");
        
        if (player1 != null)
        {
            Debug.Log($"   Player 1: [{player1.currentX}, {player1.currentY}]");
        }
        else
        {
            Debug.Log($"   Player 1: NULL");
        }
        
        if (player2 != null)
        {
            Debug.Log($"   Player 2 (AI): [{player2.currentX}, {player2.currentY}]");
        }
        else
        {
            Debug.Log($"   Player 2 (AI): NULL");
        }
        
        Debug.Log($"🎮 END GAME STATE [{context}]");
    }
    
    /// <summary>
    /// Clear all board squares of occupying pieces before initialization
    /// </summary>
    private void ClearBoardOccupancy()
    {
        Debug.Log("🧹 Clearing board occupancy...");
        
        // Find all BoardSquare objects and clear their occupancy
        var allBoardSquares = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        int clearedCount = 0;
        
        foreach (var component in allBoardSquares)
        {
            if (component.GetType().Name == "BoardSquare")
            {
                var clearMethod = component.GetType().GetMethod("ClearOccupyingPiece");
                if (clearMethod != null)
                {
                    clearMethod.Invoke(component, null);
                    clearedCount++;
                }
            }
        }
        
        Debug.Log($"✅ Cleared {clearedCount} board squares");
    }
    
    /// <summary>
    /// Update board square occupancy after setting player positions
    /// </summary>
    private void UpdateBoardOccupancy()
    {
        Debug.Log("📋 Updating board occupancy...");
        
        // Set Player 1 occupancy
        if (player1 != null)
        {
            SetSquareOccupancy(player1.currentY, player1.currentX, player1.gameObject);
        }
        
        // Set Player 2 occupancy  
        if (player2 != null)
        {
            SetSquareOccupancy(player2.currentY, player2.currentX, player2.gameObject);
        }
    }
    
    /// <summary>
    /// Set occupancy for a specific board square
    /// </summary>
    private void SetSquareOccupancy(int row, int col, GameObject occupyingPiece)
    {
        var allBoardSquares = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        
        foreach (var component in allBoardSquares)
        {
            if (component.GetType().Name == "BoardSquare")
            {
                // Check if this is the correct square
                var rowField = component.GetType().GetField("row");
                var colField = component.GetType().GetField("col");
                
                if (rowField != null && colField != null)
                {
                    int squareRow = (int)rowField.GetValue(component);
                    int squareCol = (int)colField.GetValue(component);
                    
                    if (squareRow == row && squareCol == col)
                    {
                        var setOccupyMethod = component.GetType().GetMethod("SetOccupyingPiece");
                        if (setOccupyMethod != null)
                        {
                            setOccupyMethod.Invoke(component, new object[] { occupyingPiece });
                            Debug.Log($"✅ Set occupancy for square [{row}, {col}] with {occupyingPiece.name}");
                        }
                        break;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// A* pathfinding to find optimal route to goal
    /// </summary>
    private List<Vector2Int> FindPathToGoal(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();
        
        // Initialize starting node
        var startNode = new PathNode(start, 0, GetHeuristic(start, goal));
        openSet.Add(startNode);
        gScore[start] = 0;
        fScore[start] = GetHeuristic(start, goal);
        
        while (openSet.Count > 0)
        {
            // Get node with lowest fScore
            var current = openSet.OrderBy(n => n.fCost).First();
            openSet.Remove(current);
            
            if (current.position == goal)
            {
                // Reconstruct path
                return ReconstructPath(cameFrom, current.position);
            }
            
            closedSet.Add(current.position);
            
            // Check all neighbors
            var neighbors = GetNeighbors(current.position);
            foreach (var neighbor in neighbors)
            {
                if (closedSet.Contains(neighbor)) continue;
                
                // Check if move is valid (not blocked by walls or players)
                if (!IsValidAIMove(current.position.x, current.position.y, neighbor.x, neighbor.y))
                    continue;
                
                float tentativeGScore = gScore.GetValueOrDefault(current.position, float.MaxValue) + 1;
                
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current.position;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + GetHeuristic(neighbor, goal);
                    
                    if (!openSet.Any(n => n.position == neighbor))
                    {
                        openSet.Add(new PathNode(neighbor, tentativeGScore, fScore[neighbor]));
                    }
                }
            }
        }
        
        // No path found
        Debug.LogWarning("🚫 AI: No path to goal found!");
        return null;
    }
    
    private class PathNode
    {
        public Vector2Int position;
        public float gCost;
        public float fCost;
        
        public PathNode(Vector2Int pos, float g, float f)
        {
            position = pos;
            gCost = g;
            fCost = f;
        }
    }
    
    private float GetHeuristic(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
    
    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            new Vector2Int(0, -1), // Down
            new Vector2Int(0, 1),  // Up
            new Vector2Int(-1, 0), // Left
            new Vector2Int(1, 0)   // Right
        };
        
        foreach (var dir in directions)
        {
            var neighbor = pos + dir;
            if (neighbor.x >= 0 && neighbor.x < 9 && neighbor.y >= 0 && neighbor.y < 9)
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    
    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
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
    /// Strategic wall placement to block player's progress - Enhanced version
    /// </summary>
    private SimpleAIMove GetStrategicWallMove()
    {
        if (player1 == null) return null;
        
        Debug.Log("🧱 AI analyzing strategic wall placement...");
        
        // Priority 1: Emergency blocking if player is about to win
        var emergencyBlock = GetEmergencyBlockingMove();
        if (emergencyBlock != null)
        {
            Debug.Log("🚨 AI making emergency blocking move!");
            return emergencyBlock;
        }
        
        // Priority 2: Advanced path analysis - block optimal route
        var pathAnalysisBlock = GetAdvancedPathBlockingMove();
        if (pathAnalysisBlock != null)
        {
            Debug.Log("🧠 AI using advanced path analysis for wall placement");
            return pathAnalysisBlock;
        }
        
        // Priority 3: Block player's immediate high-value moves
        var immediateBlockMove = GetSmartImmediateBlockingMove();
        if (immediateBlockMove != null)
        {
            Debug.Log("🚧 AI blocking immediate high-value player moves");
            return immediateBlockMove;
        }
        
        // Priority 4: Create strategic choke points
        var chokePointMove = CreateStrategicChokePoint();
        if (chokePointMove != null)
        {
            Debug.Log("🎯 AI creating strategic choke point");
            return chokePointMove;
        }
        
        // Priority 5: Defensive positioning
        return GetDefensiveWallMove();
    }
    
    /// <summary>
    /// Emergency wall placement when player is very close to winning
    /// </summary>
    private SimpleAIMove GetEmergencyBlockingMove()
    {
        if (player1 == null) return null;
        
        int playerY = player1.currentY;
        int playerX = player1.currentX;
        
        // If player is at Y=7 (one move from goal), this is critical
        if (playerY == 7)
        {
            Debug.Log("🚨 EMERGENCY: Player is one move from winning!");
            
            // Try multiple wall positions to block goal access
            var emergencyPositions = new[]
            {
                new { x = playerX, y = 7, horizontal = true },      // Directly above player
                new { x = playerX - 1, y = 7, horizontal = true }, // Left of player
                new { x = playerX + 1, y = 7, horizontal = true }, // Right of player
                new { x = playerX, y = 6, horizontal = true },     // One row back
            };
            
            foreach (var pos in emergencyPositions)
            {
                if (IsValidWallPlacement(pos.x, pos.y, pos.horizontal))
                {
                    Debug.Log($"🚨 Emergency wall placement at [{pos.x}, {pos.y}], horizontal: {pos.horizontal}");
                    return new SimpleAIMove(pos.x, pos.y, pos.horizontal);
                }
            }
        }
        
        // If player is at Y=6 (two moves from goal), start preparing
        if (playerY == 6)
        {
            Debug.Log("⚠️ Player is getting close to goal, preparing defensive walls");
            
            // Place walls in goal area to slow down player
            for (int x = Mathf.Max(0, playerX - 1); x <= Mathf.Min(7, playerX + 1); x++)
            {
                if (IsValidWallPlacement(x, 7, true))
                {
                    Debug.Log($"⚠️ Preparatory wall at [{x}, 7]");
                    return new SimpleAIMove(x, 7, true);
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Advanced path analysis to find best wall placement
    /// </summary>
    private SimpleAIMove GetAdvancedPathBlockingMove()
    {
        if (player1 == null) return null;
        
        // Find player's optimal path to goal using A*
        Vector2Int playerPos = new Vector2Int(player1.currentX, player1.currentY);
        Vector2Int goalPos = new Vector2Int(player1.currentX, 8); // Player's goal is Y=8
        
        var playerPath = FindPathToGoal(playerPos, goalPos);
        
        if (playerPath != null && playerPath.Count > 1)
        {
            Debug.Log($"� Player's optimal path has {playerPath.Count} steps");
            
            // Analyze each step in the path to find best blocking position
            for (int i = 1; i < Mathf.Min(playerPath.Count, 4); i++) // Check first 3 steps
            {
                Vector2Int currentStep = playerPath[i - 1];
                Vector2Int nextStep = playerPath[i];
                
                // Calculate wall position to block this movement
                var blockingWall = CalculateWallToBlockMovement(currentStep, nextStep);
                if (blockingWall != null && IsValidWallPlacement(blockingWall.WallX, blockingWall.WallY, blockingWall.IsHorizontalWall))
                {
                    // Evaluate the impact of this wall placement
                    float impact = EvaluateWallImpact(blockingWall, playerPos);
                    
                    if (impact > 50f) // High impact threshold
                    {
                        Debug.Log($"📊 High-impact wall placement (impact: {impact:F1}) at [{blockingWall.WallX}, {blockingWall.WallY}]");
                        return blockingWall;
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Smart immediate blocking that considers player's strategic options
    /// </summary>
    private SimpleAIMove GetSmartImmediateBlockingMove()
    {
        if (player1 == null) return null;
        
        int p1X = player1.currentX;
        int p1Y = player1.currentY;
        
        // Evaluate each direction the player might move
        var directionValues = new[]
        {
            new { dir = new Vector2Int(0, 1), value = 100f, name = "Up (Goal)" },     // Towards goal - highest value
            new { dir = new Vector2Int(-1, 1), value = 80f, name = "Up-Left" },      // Diagonal towards goal
            new { dir = new Vector2Int(1, 1), value = 80f, name = "Up-Right" },      // Diagonal towards goal
            new { dir = new Vector2Int(-1, 0), value = 40f, name = "Left" },         // Sideways
            new { dir = new Vector2Int(1, 0), value = 40f, name = "Right" },         // Sideways
            new { dir = new Vector2Int(0, -1), value = 10f, name = "Down" }          // Away from goal
        };
        
        // Try to block the most valuable moves first
        foreach (var option in directionValues.OrderByDescending(o => o.value))
        {
            Vector2Int targetPos = new Vector2Int(p1X + option.dir.x, p1Y + option.dir.y);
            
            // Check if this is a valid position for player to move to
            if (IsValidPlayerPosition(targetPos))
            {
                var blockingWall = CalculateWallToBlockMovement(
                    new Vector2Int(p1X, p1Y), targetPos);
                
                if (blockingWall != null && IsValidWallPlacement(blockingWall.WallX, blockingWall.WallY, blockingWall.IsHorizontalWall))
                {
                    Debug.Log($"🎯 Blocking high-value move: {option.name} (value: {option.value})");
                    return blockingWall;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Create strategic choke points on the board
    /// </summary>
    private SimpleAIMove CreateStrategicChokePoint()
    {
        if (player1 == null) return null;
        
        int p1X = player1.currentX;
        int p1Y = player1.currentY;
        
        // Create choke points in key strategic areas
        var chokePointAreas = new[]
        {
            new { centerX = 4, centerY = 6, priority = 100f, name = "Center-High" },  // Center area near goal
            new { centerX = p1X, centerY = p1Y + 2, priority = 80f, name = "Ahead of Player" }, // Ahead of player
            new { centerX = 2, centerY = 5, priority = 60f, name = "Left Corridor" },  // Left corridor
            new { centerX = 6, centerY = 5, priority = 60f, name = "Right Corridor" }, // Right corridor
        };
        
        foreach (var area in chokePointAreas.OrderByDescending(a => a.priority))
        {
            // Try to place walls around this strategic area
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int wallX = area.centerX + dx;
                    int wallY = area.centerY + dy;
                    
                    // Try both horizontal and vertical walls
                    if (IsValidWallPlacement(wallX, wallY, true))
                    {
                        Debug.Log($"🌐 Creating choke point in {area.name} area with horizontal wall");
                        return new SimpleAIMove(wallX, wallY, true);
                    }
                    
                    if (IsValidWallPlacement(wallX, wallY, false))
                    {
                        Debug.Log($"🌐 Creating choke point in {area.name} area with vertical wall");
                        return new SimpleAIMove(wallX, wallY, false);
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Calculate wall needed to block specific movement
    /// </summary>
    private SimpleAIMove CalculateWallToBlockMovement(Vector2Int from, Vector2Int to)
    {
        Vector2Int direction = to - from;
        
        if (direction.y == 1) // Moving up
        {
            return new SimpleAIMove(from.x, from.y, true); // Horizontal wall above current position
        }
        else if (direction.y == -1) // Moving down
        {
            return new SimpleAIMove(from.x, from.y - 1, true); // Horizontal wall below current position
        }
        else if (direction.x == 1) // Moving right
        {
            return new SimpleAIMove(from.x, from.y, false); // Vertical wall to the right
        }
        else if (direction.x == -1) // Moving left
        {
            return new SimpleAIMove(from.x - 1, from.y, false); // Vertical wall to the left
        }
        
        return null;
    }
    
    /// <summary>
    /// Evaluate the impact of a wall placement
    /// </summary>
    private float EvaluateWallImpact(SimpleAIMove wallMove, Vector2Int playerPos)
    {
        float impact = 0f;
        
        // Higher impact if wall is closer to player's goal
        float distanceToGoal = 8 - wallMove.WallY;
        impact += (8 - distanceToGoal) * 10f; // Closer to goal = higher impact
        
        // Higher impact if wall is near player's current position
        float distanceToPlayer = Vector2Int.Distance(new Vector2Int(wallMove.WallX, wallMove.WallY), playerPos);
        impact += Mathf.Max(0, 5 - distanceToPlayer) * 15f; // Closer to player = higher impact
        
        // Higher impact for horizontal walls (they block forward movement)
        if (wallMove.IsHorizontalWall)
        {
            impact += 20f;
        }
        
        return impact;
    }
    
    /// <summary>
    /// Check if position is valid for player movement
    /// </summary>
    private bool IsValidPlayerPosition(Vector2Int pos)
    {
        // Check bounds
        if (pos.x < 0 || pos.x >= 9 || pos.y < 0 || pos.y >= 9) return false;
        
        // Check if occupied by AI
        if (player2 != null && player2.currentX == pos.x && player2.currentY == pos.y) return false;
        
        return true;
    }
    
    /// <summary>
    /// Block player's immediate next possible moves
    /// </summary>
    private SimpleAIMove BlockImmediatePlayerMove()
    {
        int p1X = player1.currentX;
        int p1Y = player1.currentY;
        
        // Identify player's preferred direction (towards goal Y=8)
        Vector2Int[] priorityDirections = {
            new Vector2Int(0, 1),   // Up (towards goal) - highest priority to block
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0),   // Right  
            new Vector2Int(0, -1)   // Down (away from goal)
        };
        
        foreach (var dir in priorityDirections)
        {
            int targetX = p1X + dir.x;
            int targetY = p1Y + dir.y;
            
            // Check if player can move to this position
            if (targetX >= 0 && targetX < 9 && targetY >= 0 && targetY < 9)
            {
                // Try to place wall to block this movement
                if (dir.y == 1) // Blocking upward movement
                {
                    // Place horizontal wall above current position
                    if (IsValidWallPlacement(p1X, p1Y, true))
                    {
                        Debug.Log($"🚧 Blocking player's upward movement with horizontal wall at [{p1X}, {p1Y}]");
                        return new SimpleAIMove(p1X, p1Y, true);
                    }
                    // Try adjacent positions
                    if (p1X > 0 && IsValidWallPlacement(p1X - 1, p1Y, true))
                    {
                        return new SimpleAIMove(p1X - 1, p1Y, true);
                    }
                }
                else if (dir.x == 1) // Blocking rightward movement
                {
                    // Place vertical wall to the right
                    if (IsValidWallPlacement(p1X, p1Y, false))
                    {
                        Debug.Log($"🚧 Blocking player's rightward movement with vertical wall at [{p1X}, {p1Y}]");
                        return new SimpleAIMove(p1X, p1Y, false);
                    }
                }
                else if (dir.x == -1) // Blocking leftward movement
                {
                    // Place vertical wall to the left
                    if (p1X > 0 && IsValidWallPlacement(p1X - 1, p1Y, false))
                    {
                        Debug.Log($"🚧 Blocking player's leftward movement with vertical wall at [{p1X - 1}, {p1Y}]");
                        return new SimpleAIMove(p1X - 1, p1Y, false);
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Block player's optimal path to goal
    /// </summary>
    private SimpleAIMove BlockPlayerOptimalPath()
    {
        // Find player 1's best path to goal (row 8)
        var playerPath = FindPathToGoal(new Vector2Int(player1.currentX, player1.currentY), 
                                       new Vector2Int(4, 8)); // Player 1 goal
        
        if (playerPath == null || playerPath.Count < 3) return null;
        
        Debug.Log($"🛤️ Player's path has {playerPath.Count} steps");
        
        // Try to block at critical points in the path (first 3-4 steps)
        for (int i = 1; i < Mathf.Min(playerPath.Count, 5); i++)
        {
            var pathPoint = playerPath[i];
            var previousPoint = playerPath[i - 1];
            
            // Determine movement direction to block
            Vector2Int direction = pathPoint - previousPoint;
            var wallMove = PlaceWallToBlockDirection(previousPoint, direction);
            
            if (wallMove != null)
            {
                Debug.Log($"🛤️ Blocking path at step {i}: [{pathPoint.x}, {pathPoint.y}]");
                return wallMove;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Place wall to block specific movement direction
    /// </summary>
    private SimpleAIMove PlaceWallToBlockDirection(Vector2Int fromPos, Vector2Int direction)
    {
        if (direction.y == 1) // Moving up
        {
            // Place horizontal wall above fromPos
            if (IsValidWallPlacement(fromPos.x, fromPos.y, true))
                return new SimpleAIMove(fromPos.x, fromPos.y, true);
        }
        else if (direction.y == -1) // Moving down  
        {
            // Place horizontal wall below fromPos
            if (fromPos.y > 0 && IsValidWallPlacement(fromPos.x, fromPos.y - 1, true))
                return new SimpleAIMove(fromPos.x, fromPos.y - 1, true);
        }
        else if (direction.x == 1) // Moving right
        {
            // Place vertical wall to the right
            if (IsValidWallPlacement(fromPos.x, fromPos.y, false))
                return new SimpleAIMove(fromPos.x, fromPos.y, false);
        }
        else if (direction.x == -1) // Moving left
        {
            // Place vertical wall to the left
            if (fromPos.x > 0 && IsValidWallPlacement(fromPos.x - 1, fromPos.y, false))
                return new SimpleAIMove(fromPos.x - 1, fromPos.y, false);
        }
        
        return null;
    }
    
    /// <summary>
    /// Create obstacles near player's goal area
    /// </summary>
    private SimpleAIMove CreateObstaclesNearGoal()
    {
        // Player 1's goal is row 8, so create obstacles in rows 6-7
        int[] targetRows = { 7, 6 };
        
        foreach (int row in targetRows)
        {
            // Try to place horizontal walls across the path to goal
            for (int col = 2; col <= 6; col++) // Focus on center area
            {
                if (IsValidWallPlacement(col, row, true))
                {
                    Debug.Log($"🎯 Creating obstacle near goal at row {row}, col {col}");
                    return new SimpleAIMove(col, row, true);
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Find best wall placement to block a specific position
    /// </summary>
    private SimpleAIMove FindBestWallToBlock(Vector2Int targetPos)
    {
        // Try horizontal walls above and below the target
        var horizontalWalls = new[]
        {
            new { x = targetPos.x, y = targetPos.y, horizontal = true },
            new { x = targetPos.x, y = targetPos.y - 1, horizontal = true }
        };
        
        foreach (var wall in horizontalWalls)
        {
            if (IsValidWallPlacement(wall.x, wall.y, wall.horizontal))
            {
                return new SimpleAIMove(wall.x, wall.y, wall.horizontal);
            }
        }
        
        // Try vertical walls left and right of the target
        var verticalWalls = new[]
        {
            new { x = targetPos.x, y = targetPos.y, horizontal = false },
            new { x = targetPos.x - 1, y = targetPos.y, horizontal = false }
        };
        
        foreach (var wall in verticalWalls)
        {
            if (IsValidWallPlacement(wall.x, wall.y, wall.horizontal))
            {
                return new SimpleAIMove(wall.x, wall.y, wall.horizontal);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Place defensive wall near AI position
    /// </summary>
    private SimpleAIMove GetDefensiveWallMove()
    {
        if (player2 == null) return null;
        
        int aiX = player2.currentX;
        int aiY = player2.currentY;
        
        // Try to place walls around AI to create escape routes
        var defensivePositions = new[]
        {
            new { x = aiX - 1, y = aiY, horizontal = false },
            new { x = aiX + 1, y = aiY, horizontal = false },
            new { x = aiX, y = aiY - 1, horizontal = true },
            new { x = aiX, y = aiY + 1, horizontal = true }
        };
        
        foreach (var pos in defensivePositions)
        {
            if (IsValidWallPlacement(pos.x, pos.y, pos.horizontal))
            {
                Debug.Log($"🛡️ AI placing defensive wall at [{pos.x}, {pos.y}], horizontal: {pos.horizontal}");
                return new SimpleAIMove(pos.x, pos.y, pos.horizontal);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if AI can win in one move
    /// </summary>
    private SimpleAIMove GetWinningMove()
    {
        if (player2 == null) return null;
        
        // Check if AI can move to winning position (Y = 0)
        int aiX = player2.currentX;
        int aiY = player2.currentY;
        
        // If AI is at Y = 1, check if it can move to Y = 0
        if (aiY == 1)
        {
            // Check if move down is valid
            if (IsValidAIMove(aiX, aiY, aiX, aiY - 1))
            {
                Debug.Log("🏆 AI found winning move: moving to goal!");
                return new SimpleAIMove(aiX, aiY - 1);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if player is about to win and AI must block
    /// </summary>
    private SimpleAIMove GetCriticalBlockingMove()
    {
        if (player1 == null || player2 == null) return null;
        
        // Check if player is one move away from winning (Y = 8)
        int playerX = player1.currentX;
        int playerY = player1.currentY;
        
        if (playerY == 7) // Player is one move away from goal
        {
            // Try to place a wall to block player's path to goal
            Debug.Log("🚨 Player is one move away from winning! AI must block!");
            
            // Try to place horizontal wall above player's goal line
            if (IsValidWallPlacement(playerX, 7, true))
            {
                Debug.Log("🛡️ AI blocking player with horizontal wall!");
                return new SimpleAIMove(playerX, 7, true);
            }
            
            // Try nearby positions
            for (int dx = -1; dx <= 1; dx++)
            {
                int wallX = playerX + dx;
                if (IsValidWallPlacement(wallX, 7, true))
                {
                    Debug.Log($"🛡️ AI blocking player with horizontal wall at {wallX}, 7!");
                    return new SimpleAIMove(wallX, 7, true);
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Enhanced minimax integration with better error handling
    /// </summary>
    private SimpleAIMove TryGetMinimaxMove()
    {
        var quoridorAI = FindFirstObjectByType<QuoridorAI>();
        if (quoridorAI == null) 
        {
            Debug.LogWarning("⚠️ QuoridorAI component not found!");
            return null;
        }
        
        try
        {
            // Update QuoridorAI's game state before asking for move
            UpdateQuoridorAIGameState(quoridorAI);
            
            // Get the best move using either Q-Learning or Minimax (based on useQLearning flag)
            var getBestMoveMethod = quoridorAI.GetType().GetMethod("GetBestMove", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (getBestMoveMethod != null)
            {
                var aiMove = getBestMoveMethod.Invoke(quoridorAI, null);
                
                if (aiMove != null)
                {
                    return ConvertAIMoveToSimpleAIMove(aiMove);
                }
            }
            else
            {
                Debug.LogWarning("⚠️ GetBestMove method not found in QuoridorAI!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Minimax integration failed: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Update QuoridorAI's internal game state
    /// </summary>
    private void UpdateQuoridorAIGameState(QuoridorAI quoridorAI)
    {
        try
        {
            // Use reflection to update QuoridorAI's game state
            var updateMethod = quoridorAI.GetType().GetMethod("UpdateGameState", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (updateMethod != null)
            {
                updateMethod.Invoke(quoridorAI, null);
                Debug.Log("✅ Updated QuoridorAI game state");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Failed to update QuoridorAI game state: {e.Message}");
        }
    }
    
    /// <summary>
    /// Convert AIMove to SimpleAIMove
    /// </summary>
    private SimpleAIMove ConvertAIMoveToSimpleAIMove(object aiMove)
    {
        try
        {
            var moveTypeField = aiMove.GetType().GetField("moveType");
            if (moveTypeField == null) return null;
            
            var moveTypeValue = moveTypeField.GetValue(aiMove);
            
            if (moveTypeValue.ToString() == "Movement")
            {
                var targetPosField = aiMove.GetType().GetField("targetPosition");
                if (targetPosField != null)
                {
                    Vector2Int targetPos = (Vector2Int)targetPosField.GetValue(aiMove);
                    Debug.Log($"🧠 Minimax suggests movement to [{targetPos.x}, {targetPos.y}]");
                    return new SimpleAIMove(targetPos.x, targetPos.y);
                }
            }
            else if (moveTypeValue.ToString() == "WallPlacement")
            {
                var targetPosField = aiMove.GetType().GetField("targetPosition");
                var isHorizontalField = aiMove.GetType().GetField("isHorizontalWall");
                
                if (targetPosField != null && isHorizontalField != null)
                {
                    Vector2Int wallPos = (Vector2Int)targetPosField.GetValue(aiMove);
                    bool isHorizontal = (bool)isHorizontalField.GetValue(aiMove);
                    
                    Debug.Log($"🧠 Minimax suggests wall at [{wallPos.x}, {wallPos.y}], horizontal: {isHorizontal}");
                    return new SimpleAIMove(wallPos.x, wallPos.y, isHorizontal);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to convert AIMove: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Calculate dynamic wall placement probability based on game state
    /// </summary>
    private float CalculateDynamicWallProbability()
    {
        float baseChance = 0.3f; // 30% base chance
        
        // Increase chance if player is close to goal
        if (player1 != null && player1.currentY >= 6)
        {
            baseChance += 0.4f; // +40% if player is close
            Debug.Log("📈 Player close to goal, increasing wall chance");
        }
        
        // Increase chance if AI has wall advantage
        if (HasWallAdvantage())
        {
            baseChance += 0.2f; // +20% if AI has more walls
            Debug.Log("📈 AI has wall advantage, increasing wall chance");
        }
        
        // Decrease chance if AI is very close to goal
        if (player2 != null && player2.currentY <= 2)
        {
            baseChance -= 0.3f; // -30% if AI is close to winning
            Debug.Log("📉 AI close to goal, decreasing wall chance");
        }
        
        // Decrease chance if AI has very few walls left
        int aiWalls = GetAIWallsLeft();
        if (aiWalls <= 2)
        {
            baseChance -= 0.2f; // -20% if low on walls
            Debug.Log("📉 AI low on walls, decreasing wall chance");
        }
        
        return Mathf.Clamp01(baseChance);
    }
    
    /// <summary>
    /// Get number of walls left for AI
    /// </summary>
    private int GetAIWallsLeft()
    {
        if (wallPlacer2 != null)
        {
            return wallPlacer2.wallsLeft;
        }
        return 0;
    }
    
    /// <summary>
    /// Enhanced strategic move selection
    /// </summary>
    private SimpleAIMove GetEnhancedStrategicMove()
    {
        // Calculate game situation factors
        bool playerCloseToGoal = IsPlayerCloseToGoal();
        bool aiCloseToGoal = IsAICloseToGoal();
        bool hasWallAdvantage = HasWallAdvantage();
        int aiWalls = GetAIWallsLeft();
        
        Debug.Log($"📊 Game Analysis: Player close to goal: {playerCloseToGoal}, AI close: {aiCloseToGoal}, Wall advantage: {hasWallAdvantage}, AI walls left: {aiWalls}");
        
        // Strategic decision tree
        
        // If AI is close to goal and player is not threatening, focus on movement
        if (aiCloseToGoal && !playerCloseToGoal)
        {
            Debug.Log("🎯 AI prioritizing movement - close to goal");
            var moveToGoal = GetOptimalMovementToGoal();
            if (moveToGoal != null) return moveToGoal;
        }
        
        // If player is close to goal, prioritize blocking
        if (playerCloseToGoal && aiWalls > 0)
        {
            Debug.Log("🛡️ AI prioritizing wall placement - player close to goal");
            var blockingWall = GetStrategicWallMove();
            if (blockingWall != null) return blockingWall;
        }
        
        // Calculate wall placement probability based on situation
        float wallProbability = CalculateAdvancedWallProbability(playerCloseToGoal, aiCloseToGoal, hasWallAdvantage, aiWalls);
        
        Debug.Log($"🎲 Strategic wall probability: {wallProbability:F2}");
        
        // Make strategic decision
        if (aiWalls > 0 && UnityEngine.Random.Range(0f, 1f) < wallProbability)
        {
            var strategicWall = GetStrategicWallMove();
            if (strategicWall != null)
            {
                Debug.Log("🧱 AI choosing strategic wall placement");
                return strategicWall;
            }
        }
        
        // Default to enhanced pathfinding movement
        return GetPathfindingAIMoveEnhanced();
    }
    
    /// <summary>
    /// Calculate advanced wall placement probability
    /// </summary>
    private float CalculateAdvancedWallProbability(bool playerClose, bool aiClose, bool wallAdvantage, int aiWalls)
    {
        float baseProbability = 0.25f;
        
        // Situation-based adjustments
        if (playerClose && !aiClose)
        {
            baseProbability += 0.5f; // Very high priority to block player
            Debug.Log("📈 Player close but AI not - high blocking priority");
        }
        else if (playerClose && aiClose)
        {
            baseProbability += 0.3f; // Both close - still prioritize blocking
            Debug.Log("📈 Both players close - moderate blocking priority");
        }
        else if (!playerClose && aiClose)
        {
            baseProbability -= 0.2f; // AI close, player not - focus on movement
            Debug.Log("📉 AI close, player not - focus on movement");
        }
        
        // Wall resource management
        if (aiWalls <= 2)
        {
            baseProbability -= 0.3f; // Save limited walls
            Debug.Log("📉 Low on walls - conserving resources");
        }
        else if (aiWalls >= 7 && wallAdvantage)
        {
            baseProbability += 0.2f; // Plenty of walls and advantage
            Debug.Log("📈 Wall advantage - use it");
        }
        
        // Game phase adjustments
        int totalMoves = GetTotalMoveCount();
        if (totalMoves < 10) // Early game
        {
            baseProbability -= 0.1f; // Less wall focus early
            Debug.Log("📉 Early game - less wall focus");
        }
        else if (totalMoves > 20) // Late game
        {
            baseProbability += 0.2f; // More tactical late game
            Debug.Log("📈 Late game - more tactical play");
        }
        
        return Mathf.Clamp01(baseProbability);
    }
    
    /// <summary>
    /// Get optimal movement toward goal using A*
    /// </summary>
    private SimpleAIMove GetOptimalMovementToGoal()
    {
        if (player2 == null) return null;
        
        Vector2Int aiPos = new Vector2Int(player2.currentX, player2.currentY);
        Vector2Int goal = new Vector2Int(player2.currentX, 0); // AI's goal is Y=0
        
        // Use A* to find path to goal
        var pathToGoal = FindPathToGoal(aiPos, goal);
        
        if (pathToGoal != null && pathToGoal.Count > 1)
        {
            Vector2Int nextStep = pathToGoal[1]; // First step in optimal path
            
            // Validate the move
            if (IsValidAIMove(aiPos.x, aiPos.y, nextStep.x, nextStep.y) && !IsOscillating(nextStep))
            {
                Debug.Log($"🎯 Optimal movement to goal: [{nextStep.x}, {nextStep.y}]");
                return new SimpleAIMove(nextStep.x, nextStep.y);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get total number of moves made in game (rough estimate)
    /// </summary>
    private int GetTotalMoveCount()
    {
        // Estimate based on wall usage (rough approximation)
        int player1WallsUsed = (wallPlacer1 != null) ? (10 - wallPlacer1.wallsLeft) : 0;
        int player2WallsUsed = (wallPlacer2 != null) ? (10 - wallPlacer2.wallsLeft) : 0;
        
        // Assume roughly 1 wall per 3-4 moves, so estimate total moves
        int estimatedMoves = (player1WallsUsed + player2WallsUsed) * 3;
        
        // Add position-based estimate
        if (player1 != null && player2 != null)
        {
            int player1Progress = player1.currentY; // Player starts at Y=0, moves to Y=8
            int player2Progress = 8 - player2.currentY; // AI starts at Y=8, moves to Y=0
            estimatedMoves += player1Progress + player2Progress;
        }
        
        return estimatedMoves;
    }

    public void Replay()
    {
        Debug.Log("🔄 Replay button pressed");
        RemoveAllPhysicalWalls(); // Xóa hết tường vật lý trước khi reset
        InitializeGame();
    }
}
