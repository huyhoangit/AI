using UnityEngine;

/// <summary>
/// Component cho quân chess trong game Quoridor
/// Xử lý click và di chuyển quân cờ
/// </summary>
public class ChessPlayer : MonoBehaviour
{
    [Header("Player Info")]
    public int playerID = 1; // 1 hoặc 2
    public int row = 0; // Vị trí hiện tại - hàng
    public int col = 0; // Vị trí hiện tại - cột (giữa bàn cờ)
    public bool isInitializing = false; // Flag to indicate initialization mode
    
    // Properties for easier access
    public int currentX { get { return col; } }
    public int currentY { get { return row; } }
    
    [Header("Movement")]
    public bool isSelected = false;
    public Color selectedColor = Color.blue;
    public Color validMoveColor = Color.green;
    
    [Header("Wall Placement")]
    public WallPlacer wallPlacer; // Reference đến WallPlacer
    
    private BoardManager boardManager;
    private MonoBehaviour gameManager; // Reference to GameManager
    private Renderer chessRenderer;
    private Material originalMaterial;
    private Vector2Int[] validMoves = new Vector2Int[4]; // 4 ô xung quanh
    
    void Start()
    {
        // Tìm BoardManager
        boardManager = BoardManager.Instance;
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }
        
        // Tìm GameManager
        var gameManagerComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var component in gameManagerComponents)
        {
            if (component.GetType().Name == "GameManager")
            {
                gameManager = component;
                break;
            }
        }
        
        // Tìm WallPlacer nếu chưa assign
        if (wallPlacer == null)
        {
            // Tìm WallPlacer có cùng playerID
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
                            // Tìm thấy WallPlacer cùng playerID
                            Debug.Log($"✅ Found WallPlacer for Player {playerID}");
                            break;
                        }
                    }
                }
            }
        }
        
        // Lấy renderer
        chessRenderer = GetComponent<Renderer>();
        if (chessRenderer != null)
        {
            originalMaterial = chessRenderer.material;
        }
        
        // Đặt vị trí ban đầu
        // UpdatePosition();
    }
    
    void Update()
    {
        // Chỉ xử lý input nếu player này được chọn
        //if (!isSelected) return;
        
        // Phím R để toggle wall placement mode - với input protection
        if (InputStateManager.Instance != null ? 
            InputStateManager.Instance.GetKeyDownSafe(KeyCode.R) : 
            Input.GetKeyDown(KeyCode.R))
        {
            ToggleWallPlacement();
        }
        
        // Phím Escape để bỏ chọn
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectPiece();
        }
    }
    
    /// <summary>
    /// Toggle chế độ đặt wall
    /// </summary>
    void ToggleWallPlacement()
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
                        // Gọi ToggleWallPlaceMode
                        var method = component.GetType().GetMethod("ToggleWallPlaceMode");
                        if (method != null)
                        {
                            method.Invoke(component, null);
                            Debug.Log($"🧱 Player {playerID} toggled wall placement mode");
                        }
                        break;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Cập nhật vị trí thực tế trên board
    /// </summary>
    void UpdatePosition()
    {
        if (boardManager != null)
        {
            Vector3 worldPos = boardManager.GetSquareWorldPosition(row, col);
            transform.position = worldPos + Vector3.up; // Nâng lên trên ô một chút
        }
    }
    
    /// <summary>
    /// Xử lý khi click vào quân chess
    /// </summary>
    void OnMouseDown()
    {
        // Kiểm tra có phải lượt của player này không
        if (!IsMyTurn())
        {
            Debug.Log($"⏳ Not Player {playerID}'s turn");
            return;
        }
        
        if (isSelected)
        {
            // Nếu đã chọn rồi thì bỏ chọn
            DeselectPiece();
        }
        else
        {
            // Chọn quân và hiển thị nước đi hợp lệ
            SelectPiece();
        }
    }
    
    /// <summary>
    /// Chọn quân cờ và highlight các ô có thể di chuyển
    /// </summary>
    public void SelectPiece()
    {
        Debug.Log($"🎯 Selected Player {playerID} at [{row}, {col}]");
        
        isSelected = true;
        
        // Đổi màu quân cờ để hiển thị đã chọn
        if (chessRenderer != null)
        {
            chessRenderer.material.color = selectedColor;
        }
        
        // Tính toán và highlight các ô có thể di chuyển
        CalculateValidMoves();
        HighlightValidMoves();
    }
    
    /// <summary>
    /// Bỏ chọn quân cờ
    /// </summary>
    public void DeselectPiece()
    {
        Debug.Log($"🔄 Deselected Player {playerID}");
        
        isSelected = false;
        
        // Trở lại màu gốc
        if (chessRenderer != null && originalMaterial != null)
        {
            chessRenderer.material = originalMaterial;
        }
        
        // Xóa highlight
        ClearValidMoveHighlights();
    }
    
    /// <summary>
    /// Tính toán 4 ô xung quanh có thể di chuyển (kiểm tra cả wall)
    /// </summary>
    void CalculateValidMoves()
    {
        // 4 hướng: lên, xuống, trái, phải
        Vector2Int[] directions = {
            Vector2Int.up,      // Lên (row + 1)
            Vector2Int.down,    // Xuống (row - 1)
            Vector2Int.left,    // Trái (col - 1)
            Vector2Int.right    // Phải (col + 1)
        };
        
        for (int i = 0; i < 4; i++)
        {
            Vector2Int newPos = new Vector2Int(col, row) + directions[i];
            
            // Kiểm tra trong bounds của bàn cờ (0-8)
            if (IsValidPosition(newPos.x, newPos.y))
            {
                // Kiểm tra xem có wall chặn đường đi không
                if (!IsBlockedByWall(col, row, newPos.x, newPos.y))
                {
                    validMoves[i] = newPos;
                }
                else
                {
                    validMoves[i] = new Vector2Int(-1, -1); // Bị wall chặn
                    Debug.Log($"🚫 Movement from [{col},{row}] to [{newPos.x},{newPos.y}] blocked by wall");
                }
            }
            else
            {
                validMoves[i] = new Vector2Int(-1, -1); // Không hợp lệ
            }
        }
    }
    
    /// <summary>
    /// Kiểm tra vị trí có hợp lệ không (trong bounds 9x9)
    /// </summary>
    bool IsValidPosition(int col, int row)
    {
        return col >= 0 && col < 9 && row >= 0 && row < 9;
    }
    
    /// <summary>
    /// Highlight các ô có thể di chuyển bằng màu xanh lá
    /// </summary>
    void HighlightValidMoves()
    {
        if (boardManager == null) return;
        
        for (int i = 0; i < validMoves.Length; i++)
        {
            Vector2Int move = validMoves[i];
            if (move.x >= 0 && move.y >= 0) // Nước đi hợp lệ
            {
                GameObject square = boardManager.GetSquareAt(move.y, move.x);
                if (square != null)
                {
                    Renderer squareRenderer = square.GetComponent<Renderer>();
                    if (squareRenderer != null)
                    {
                        squareRenderer.material.color = validMoveColor; // Màu xanh lá
                    }
                    
                    // Thêm component để xử lý click vào ô di chuyển
                    ChessMoveTarget moveTarget = square.GetComponent<ChessMoveTarget>();
                    if (moveTarget == null)
                    {
                        moveTarget = square.AddComponent<ChessMoveTarget>();
                    }
                    moveTarget.Initialize(move.x, move.y, this);
                }
            }
        }
    }
    
    /// <summary>
    /// Xóa highlight các ô di chuyển
    /// </summary>
    void ClearValidMoveHighlights()
    {
        if (boardManager == null) return;
        
        for (int i = 0; i < validMoves.Length; i++)
        {
            Vector2Int move = validMoves[i];
            if (move.x >= 0 && move.y >= 0)
            {
                GameObject square = boardManager.GetSquareAt(move.y, move.x);
                if (square != null)
                {
                    // Reset màu ô về mặc định
                    boardManager.HighlightSquare(move.y, move.x, false);
                    
                    // Xóa component di chuyển
                    ChessMoveTarget moveTarget = square.GetComponent<ChessMoveTarget>();
                    if (moveTarget != null)
                    {
                        Destroy(moveTarget);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Di chuyển quân cờ đến vị trí mới
    /// </summary>
    public void MoveTo(int newCol, int newRow)
    {
        // Kiểm tra nước đi có hợp lệ không
        bool validMove = false;
        for (int i = 0; i < validMoves.Length; i++)
        {
            if (validMoves[i].x == newCol && validMoves[i].y == newRow)
            {
                validMove = true;
                break;
            }
        }
        
        if (!validMove)
        {
            Debug.LogWarning($"❌ Invalid move to [{newCol}, {newRow}]");
            return;
        }
        
        Debug.Log($"✅ Player {playerID} moved from [{col}, {row}] to [{newCol}, {newRow}]");
        
        // Cập nhật vị trí
        col = newCol;
        row = newRow;
        
        // Di chuyển đến vị trí mới
        UpdatePosition();
        
        // Bỏ chọn quân
        DeselectPiece();
        
        // Thông báo cho GameManager về lượt đi
        if (gameManager != null)
        {
            var method = gameManager.GetType().GetMethod("OnPlayerMove");
            if (method != null)
            {
                method.Invoke(gameManager, new object[] { playerID });
            }
        }
    }
    
    /// <summary>
    /// Set position for AI moves (public method for GameManager)
    /// </summary>
    public void SetPosition(int newCol, int newRow)
    {
        Debug.Log($"🤖 SETPOSITION: Player {playerID} moving from [{col}, {row}] to [{newCol}, {newRow}]");
        
        // Skip validation during initialization
        if (!isInitializing)
        {
            // VALIDATION: Ensure move is only one step
            int deltaX = Mathf.Abs(newCol - col);
            int deltaY = Mathf.Abs(newRow - row);
            
            if (deltaX + deltaY > 1)
            {
                Debug.LogError($"❌ CRITICAL: Player {playerID} trying to make invalid move! Distance: {deltaX + deltaY}");
                return; // Don't allow the move
            }
        }
        else
        {
            Debug.Log($"🔧 INITIALIZATION: Player {playerID} teleporting to starting position [{newCol}, {newRow}]");
        }
        
        col = newCol;
        row = newRow;
        UpdatePosition();
        
        Debug.Log($"🤖 SETPOSITION COMPLETE: Player {playerID} now at [{col}, {row}]");
    }
    
    /// <summary>
    /// Set initial position (for game initialization only)
    /// </summary>
    public void SetInitialPosition(int newCol, int newRow)
    {
        Debug.Log($"🎮 INITIAL POSITION SET: Player {playerID} at [{newCol}, {newRow}]");
        
        col = newCol;
        row = newRow;
        
        // Force update transform position immediately
        UpdatePosition();
        
        // Double-check the values are set correctly
        Debug.Log($"🔍 After SetInitialPosition: Player {playerID} - col={col}, row={row}, currentX={currentX}, currentY={currentY}");
        Debug.Log($"✅ Player {playerID} initialized at [{col}, {row}] (World pos: {transform.position})");
    }
    
    /// <summary>
    /// Kiểm tra có phải lượt của player này không
    /// </summary>
    public bool IsMyTurn()
    {
        // Kiểm tra lượt chơi dựa trên playerID với reflection
        if (gameManager != null)
        {
            var method = gameManager.GetType().GetMethod("IsCurrentPlayer");
            if (method != null)
            {
                return (bool)method.Invoke(gameManager, new object[] { playerID });
            }
        }
        return true; // Default to true if no GameManager found
    }
    
    /// <summary>
    /// Kiểm tra xem có wall nào chặn đường đi từ vị trí A đến vị trí B không
    /// </summary>
    bool IsBlockedByWall(int fromCol, int fromRow, int toCol, int toRow)
    {
        // Tìm tất cả các wall đã được đặt (thử cả 2 tag)
        GameObject[] placedWalls1 = GameObject.FindGameObjectsWithTag("PlacedWall");
        GameObject[] placedWalls2 = GameObject.FindGameObjectsWithTag("Wall");
        
        // Gộp 2 mảng
        GameObject[] allWalls = new GameObject[placedWalls1.Length + placedWalls2.Length];
        placedWalls1.CopyTo(allWalls, 0);
        placedWalls2.CopyTo(allWalls, placedWalls1.Length);
        
        if (allWalls.Length == 0) 
        {
            Debug.Log($"✅ No walls found - movement from [{fromCol},{fromRow}] to [{toCol},{toRow}] is FREE");
            return false; // Không có wall nào
        }
        
        Debug.Log($"🔍 Checking wall blocking from [{fromCol},{fromRow}] to [{toCol},{toRow}]. Found {allWalls.Length} walls");
        
        // Xác định hướng di chuyển
        int deltaCol = toCol - fromCol;
        int deltaRow = toRow - fromRow;
        
        // Validate movement (should be exactly 1 step)
        if (Mathf.Abs(deltaCol) + Mathf.Abs(deltaRow) != 1)
        {
            Debug.LogError($"❌ Invalid movement: [{fromCol},{fromRow}] to [{toCol},{toRow}] - not a single step!");
            return true; // Block invalid moves
        }
        
        string movementDirection = "";
        if (deltaCol == 1) movementDirection = "RIGHT";
        else if (deltaCol == -1) movementDirection = "LEFT"; 
        else if (deltaRow == 1) movementDirection = "UP";
        else if (deltaRow == -1) movementDirection = "DOWN";
        
        Debug.Log($"🎯 Movement direction: {movementDirection}");
        
        foreach (GameObject wall in allWalls)
        {
            if (wall == null) continue;
            
            Vector3 wallPos = wall.transform.position;
            bool isHorizontalWall = IsHorizontalWall(wall);
            
            Debug.Log($"🧱 Analyzing wall '{wall.name}' at {wallPos}, horizontal: {isHorizontalWall}");
            
            // Kiểm tra xem wall này có chặn đường đi không
            if (IsWallBlocking(fromCol, fromRow, deltaCol, deltaRow, wallPos, isHorizontalWall))
            {
                Debug.Log($"🚫 MOVEMENT BLOCKED! Wall '{wall.name}' blocks {movementDirection} movement");
                return true; // Có wall chặn
            }
        }
        
        Debug.Log($"✅ No walls blocking {movementDirection} movement from [{fromCol},{fromRow}] to [{toCol},{toRow}]");
        return false; // Không có wall chặn
    }
    
    /// <summary>
    /// Kiểm tra xem wall là horizontal hay vertical dựa trên scale và tên - ENHANCED VERSION
    /// </summary>
    bool IsHorizontalWall(GameObject wall)
    {
        // PRIORITY 1: Kiểm tra tên wall trước (reliable nhất)
        string wallName = wall.name.ToLower();
        
        // Horizontal wall indicators
        if (wallName.Contains("_h_") || wallName.EndsWith("_h") || 
            wallName.Contains("horizontal") || wallName.Contains("horiz"))
        {
            Debug.Log($"🧱 Wall '{wall.name}' detected as HORIZONTAL by name");
            return true;
        }
        
        // Vertical wall indicators  
        if (wallName.Contains("_v_") || wallName.EndsWith("_v") || 
            wallName.Contains("vertical") || wallName.Contains("vert"))
        {
            Debug.Log($"🧱 Wall '{wall.name}' detected as VERTICAL by name");
            return false;
        }
        
        // PRIORITY 2: Kiểm tra scale/rotation
        Vector3 scale = wall.transform.localScale;
        Vector3 rotation = wall.transform.eulerAngles;
        
        // Kiểm tra rotation trước (nếu wall được rotate)
        float yRotation = Mathf.Abs(rotation.y);
        if (Mathf.Abs(yRotation - 90f) < 30f || Mathf.Abs(yRotation - 270f) < 30f)
        {
            // Wall được rotate 90 độ -> có thể đổi orientation
            bool isHorizontalByRotation = scale.z > scale.x; // Sau khi rotate, Z trở thành chiều dài ngang
            Debug.Log($"� Wall '{wall.name}' rotated {yRotation}° -> horizontal: {isHorizontalByRotation}");
            return isHorizontalByRotation;
        }
        
        // PRIORITY 3: Fallback - kiểm tra scale thông thường
        bool isHorizontalByScale = scale.x > scale.z;
        Debug.Log($"🔧 Wall '{wall.name}' scale check: {scale} -> horizontal: {isHorizontalByScale}");
        
        return isHorizontalByScale;
    }
    
    /// <summary>
    /// Kiểm tra xem một wall cụ thể có chặn đường đi không
    /// </summary>
    bool IsWallBlocking(int fromCol, int fromRow, int deltaCol, int deltaRow, Vector3 wallPos, bool isHorizontalWall)
    {
        // CRITICAL: Sử dụng chính xác cùng logic tính toán với WallPlacer và GameManager
        Vector3 boardStart = new Vector3(-5.0f, 0f, -4.85f); // ĐỒNG BỘ với WallPlacer.GetBoardStartPosition()
        float squareSize = 1.0f; // Từ BoardManager
        float squareSpacing = 0.1f; // Từ BoardManager
        float stepSize = squareSize + squareSpacing; // 1.1f
        
        float tolerance = 0.7f; // Tolerance để detect wall
        
        string direction = "";
        Vector3 expectedWallPos = Vector3.zero;
        bool shouldBeBlocked = false;
        
        if (deltaCol == 1) // Di chuyển sang PHẢI (col tăng)
        {
            direction = "RIGHT";
            // Kiểm tra vertical wall bên phải ô hiện tại
            // Wall này nằm giữa [fromRow, fromCol] và [fromRow, fromCol+1]
            expectedWallPos = new Vector3(
                boardStart.x + (fromCol + 1) * stepSize - squareSpacing * 0.5f, // Cạnh phải ô hiện tại
                0.1f,
                boardStart.z + fromRow * stepSize + squareSize * 0.5f // Center của hàng
            );
            
            shouldBeBlocked = !isHorizontalWall && Vector3.Distance(wallPos, expectedWallPos) < tolerance;
        }
        else if (deltaCol == -1) // Di chuyển sang TRÁI (col giảm)
        {
            direction = "LEFT";
            // Kiểm tra vertical wall bên trái ô hiện tại
            // Wall này nằm giữa [fromRow, fromCol-1] và [fromRow, fromCol]
            expectedWallPos = new Vector3(
                boardStart.x + fromCol * stepSize - squareSpacing * 0.5f, // Cạnh trái ô hiện tại
                0.1f,
                boardStart.z + fromRow * stepSize + squareSize * 0.5f // Center của hàng
            );
            
            shouldBeBlocked = !isHorizontalWall && Vector3.Distance(wallPos, expectedWallPos) < tolerance;
        }
        else if (deltaRow == 1) // Di chuyển LÊN TRÊN (row tăng)
        {
            direction = "UP";
            // Kiểm tra horizontal wall phía trên ô hiện tại
            expectedWallPos = new Vector3(
                boardStart.x + fromCol * stepSize + squareSize * 0.5f, // Center của cột
                0.1f,
                boardStart.z + (fromRow + 1) * stepSize - squareSpacing * 0.5f // Cạnh trên ô hiện tại
            );
            
            shouldBeBlocked = isHorizontalWall && Vector3.Distance(wallPos, expectedWallPos) < tolerance;
        }
        else if (deltaRow == -1) // Di chuyển XUỐNG DƯỚI (row giảm)
        {
            direction = "DOWN";
            // Kiểm tra horizontal wall phía dưới ô hiện tại
            expectedWallPos = new Vector3(
                boardStart.x + fromCol * stepSize + squareSize * 0.5f, // Center của cột
                0.1f,
                boardStart.z + fromRow * stepSize - squareSpacing * 0.5f // Cạnh dưới ô hiện tại
            );
            
            shouldBeBlocked = isHorizontalWall && Vector3.Distance(wallPos, expectedWallPos) < tolerance;
        }
        
        float distance = Vector3.Distance(wallPos, expectedWallPos);
        Debug.Log($"🧱 {direction} movement check: Wall at {wallPos} vs Expected {expectedWallPos}, Distance: {distance:F3}, IsHorizontal: {isHorizontalWall}, Tolerance: {tolerance}");
        
        if (shouldBeBlocked)
        {
            Debug.Log($"🚫 {direction} movement BLOCKED by {(isHorizontalWall ? "horizontal" : "vertical")} wall!");
        }
        else
        {
            Debug.Log($"✅ {direction} movement NOT blocked - {(isHorizontalWall ? "horizontal" : "vertical")} wall too far or wrong type");
        }
        
        return shouldBeBlocked;
    }
    
    /// <summary>
    /// Test method để debug wall blocking logic
    /// </summary>
    [ContextMenu("Test Wall Blocking")]
    public void TestWallBlocking()
    {
        Debug.Log($"🧪 Testing wall blocking for Player {playerID} at [{col}, {row}]");
        
        // Test 4 hướng
        string[] directions = { "RIGHT", "LEFT", "UP", "DOWN" };
        Vector2Int[] deltas = {
            new Vector2Int(1, 0),   // RIGHT
            new Vector2Int(-1, 0),  // LEFT
            new Vector2Int(0, 1),   // UP
            new Vector2Int(0, -1)   // DOWN
        };
        
        for (int i = 0; i < 4; i++)
        {
            Vector2Int delta = deltas[i];
            int toCol = col + delta.x;
            int toRow = row + delta.y;
            
            if (IsValidPosition(toCol, toRow))
            {
                bool blocked = IsBlockedByWall(col, row, toCol, toRow);
                string status = blocked ? "🚫 BLOCKED" : "✅ FREE";
                Debug.Log($"   {directions[i]} to [{toCol}, {toRow}]: {status}");
            }
            else
            {
                Debug.Log($"   {directions[i]} to [{toCol}, {toRow}]: ⚠️ OUT OF BOUNDS");
            }
        }
    }
    
    /// <summary>
    /// Enhanced debug method with real-time wall analysis
    /// </summary>
    [ContextMenu("Debug Wall Blocking Detailed")]
    public void DebugWallBlockingDetailed()
    {
        Debug.Log($"🔍 === DETAILED WALL BLOCKING ANALYSIS for Player {playerID} at [{col}, {row}] ===");
        
        // Find all walls first
        GameObject[] placedWalls1 = GameObject.FindGameObjectsWithTag("PlacedWall");
        GameObject[] placedWalls2 = GameObject.FindGameObjectsWithTag("Wall");
        GameObject[] allWalls = new GameObject[placedWalls1.Length + placedWalls2.Length];
        placedWalls1.CopyTo(allWalls, 0);
        placedWalls2.CopyTo(allWalls, placedWalls1.Length);
        
        Debug.Log($"📊 Found {allWalls.Length} total walls on board");
        
        // Analyze each wall
        for (int i = 0; i < allWalls.Length; i++)
        {
            if (allWalls[i] != null)
            {
                Vector3 wallPos = allWalls[i].transform.position;
                bool isHorizontal = IsHorizontalWall(allWalls[i]);
                Debug.Log($"   Wall {i}: '{allWalls[i].name}' at {wallPos}, horizontal: {isHorizontal}");
            }
        }
        
        Debug.Log($"");
        
        // Test 4 hướng với detailed analysis
        string[] directions = { "RIGHT", "LEFT", "UP", "DOWN" };
        Vector2Int[] deltas = {
            new Vector2Int(1, 0),   // RIGHT
            new Vector2Int(-1, 0),  // LEFT
            new Vector2Int(0, 1),   // UP
            new Vector2Int(0, -1)   // DOWN
        };
        
        for (int i = 0; i < 4; i++)
        {
            Vector2Int delta = deltas[i];
            int toCol = col + delta.x;
            int toRow = row + delta.y;
            
            Debug.Log($"🎯 Testing {directions[i]} movement to [{toCol}, {toRow}]:");
            
            if (!IsValidPosition(toCol, toRow))
            {
                Debug.Log($"   ❌ OUT OF BOUNDS");
                continue;
            }
            
            bool blocked = IsBlockedByWall(col, row, toCol, toRow);
            string status = blocked ? "🚫 BLOCKED" : "✅ FREE";
            Debug.Log($"   {status}");
            Debug.Log($"");
        }
        
        Debug.Log($"🔍 === END WALL BLOCKING ANALYSIS ===");
    }
}
