using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Component cho việc đặt thanh chắn (wall) trong game Quoridor
/// Xử lý click và đặt wall vào các rãnh giữa ô vuông
/// Tích hợp với InputStateManager để bảo vệ phím tắt khi đang nhập text
/// </summary>
public class WallPlacer : MonoBehaviour
{
    [Header("Wall Settings")]
    public int playerID = 1; // Player nào đang đặt wall
    public int wallsRemaining = 10; // Số wall còn lại (mỗi player có 10 wall)
    public int wallsLeft { get { return wallsRemaining; } } // Property for GameManager
    public bool isPlacingWall = false; // Có đang ở chế độ đặt wall không
    
    [Header("Visual")]
    public Color validWallColor = Color.red; // Màu highlight vị trí có thể đặt wall
    public Color placedWallColor = new Color(0.6f, 0.3f, 0.1f); // Màu wall đã đặt (brown color)
    public Material wallMaterial; // Material cho wall
    
    [Header("Wall Prefab")]
    public GameObject wallPrefab; // Prefab cho wall
    
    private BoardManager boardManager;
    private MonoBehaviour gameManager; // Reference to GameManager
    private GameObject[] wallSlots; // Các vị trí có thể đặt wall
    private bool[] wallPlaced; // Đánh dấu vị trí nào đã đặt wall
    private GameObject wallParent; // Parent chứa tất cả wall
    
    // Public list for GameManager to access - using simple data structure
    [System.Serializable]
    public class WallData
    {
        public int x, y;
        public bool isHorizontal;
        
        public WallData(int x, int y, bool isHorizontal)
        {
            this.x = x;
            this.y = y;
            this.isHorizontal = isHorizontal;
        }
    }
    
    public List<WallData> placedWalls = new List<WallData>();
    
    void Start()
    {
        // Tìm BoardManager
        boardManager = BoardManager.Instance;
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }
        
        // Tạo các vị trí wall
        CreateWallSlots();
        
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
    }
    
    /// <summary>
    /// Toggle wall placement mode (for GameManager)
    /// </summary>
    public void ToggleWallPlacementMode()
    {
        ToggleWallPlaceMode();
    }
    
    /// <summary>
    /// Bật/tắt chế độ đặt wall
    /// </summary>
    public void ToggleWallPlaceMode()
    {
        if (wallsRemaining <= 0)
        {
            Debug.LogWarning($"❌ Player {playerID} has no walls remaining!");
            return;
        }
        
        isPlacingWall = !isPlacingWall;
        
        if (isPlacingWall)
        {
            Debug.Log($"🧱 Player {playerID} entering wall placement mode. Walls remaining: {wallsRemaining}");
            Debug.Log($"   Input protection: {(InputStateManager.Instance != null ? "✅ Active (R key safe)" : "⚠️ Manual fallback")}");
            ShowValidWallPositions();
        }
        else
        {
            Debug.Log($"🔄 Player {playerID} exiting wall placement mode");
            HideValidWallPositions();
        }
    }
    
    /// <summary>
    /// Tạo các vị trí có thể đặt wall
    /// </summary>
    void CreateWallSlots()
    {
        if (boardManager == null) return;
        
        // Debug log vị trí board
        Vector3 boardStart = GetBoardStartPosition();
        Debug.Log($"🎯 Board start position (FIXED): {boardStart}");
        Debug.Log($"🎯 Board settings: squareSize={boardManager.squareSize}, spacing={boardManager.squareSpacing}");
        
        // Tạo parent cho wall
        wallParent = GameObject.Find("WallParent");
        if (wallParent == null)
        {
            wallParent = new GameObject("WallParent");
        }
        
        // Tính toán số lượng wall slot
        // Horizontal walls: 8x9 (giữa các hàng)
        // Vertical walls: 9x8 (giữa các cột)
        int horizontalSlots = 8 * 9; // 72 slots
        int verticalSlots = 9 * 8;   // 72 slots
        int totalSlots = horizontalSlots + verticalSlots; // 144 slots
        
        wallSlots = new GameObject[totalSlots];
        wallPlaced = new bool[totalSlots];
        
        int slotIndex = 0;
        
        // Tạo horizontal wall slots (giữa các hàng)
        for (int row = 0; row < 8; row++) // 8 khoảng giữa 9 hàng
        {
            for (int col = 0; col < 9; col++)
            {
                CreateWallSlot(slotIndex, row, col, true); // true = horizontal
                slotIndex++;
            }
        }
        
        // Tạo vertical wall slots (giữa các cột)
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 8; col++) // 8 khoảng giữa 9 cột
            {
                CreateWallSlot(slotIndex, row, col, false); // false = vertical
                slotIndex++;
            }
        }
        
        Debug.Log($"✅ Created {totalSlots} wall slots ({horizontalSlots} horizontal + {verticalSlots} vertical)");
    }
    
    /// <summary>
    /// Tạo một wall slot tại vị trí cụ thể
    /// </summary>
    void CreateWallSlot(int slotIndex, int row, int col, bool isHorizontal)
    {
        if (boardManager == null) return;
        
        // Tính vị trí wall slot dựa trên logic board
        Vector3 wallPosition = CalculateWallSlotPosition(row, col, isHorizontal);
        Vector3 wallScale;
        
        // Điều chỉnh kích thước wall dựa trên squareSize
        float squareSize = boardManager.squareSize;
        
        if (isHorizontal)
        {
            // Wall nằm ngang (dài theo X, mỏng theo Z)
            wallScale = new Vector3(squareSize * 0.9f, 0.2f, 0.1f);
        }
        else
        {
            // Wall dọc (mỏng theo X, dài theo Z)
            wallScale = new Vector3(0.1f, 0.2f, squareSize * 0.9f);
        }
        
        // Tạo wall slot (invisible collider)
        GameObject wallSlot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallSlot.name = $"WallSlot_{slotIndex}_{(isHorizontal ? "H" : "V")}_{row}_{col}";
        wallSlot.transform.position = wallPosition;
        wallSlot.transform.localScale = wallScale;
        wallSlot.transform.SetParent(wallParent.transform);
        
        // Debug log để kiểm tra vị trí
        Debug.Log($"🔧 Created wall slot {slotIndex}: {(isHorizontal ? "HORIZONTAL" : "VERTICAL")} at Row:{row}, Col:{col}, Position:{wallPosition}");
        
        // Làm trong suốt và tắt collider
        Renderer slotRenderer = wallSlot.GetComponent<Renderer>();
        Collider slotCollider = wallSlot.GetComponent<Collider>();
        
        if (slotRenderer != null)
        {
            slotRenderer.material.color = new Color(1, 1, 1, 0.1f); // Gần như trong suốt
            slotRenderer.enabled = false; // Ẩn đi ban đầu
        }
        
        if (slotCollider != null)
        {
            slotCollider.enabled = false; // Tắt collider ban đầu để không va chạm với quân cờ
        }
        
        // Thêm WallSlot component
        WallSlot slotComponent = wallSlot.AddComponent<WallSlot>();
        slotComponent.Initialize(this, slotIndex, row, col, isHorizontal);
        
        // Thêm tag
        wallSlot.tag = "WallSlot";
        
        wallSlots[slotIndex] = wallSlot;
    }
    
    /// <summary>
    /// Tính toán vị trí chính xác của wall slot
    /// </summary>
    Vector3 CalculateWallSlotPosition(int row, int col, bool isHorizontal)
    {
        // Lấy thông tin từ BoardManager
        float squareSize = boardManager.squareSize;
        float squareSpacing = boardManager.squareSpacing;
        float stepSize = squareSize + squareSpacing; // 1.1
        
        // Lấy vị trí bắt đầu của board (góc trái dưới ô [0,0])
        Vector3 boardStartPos = GetBoardStartPosition();
        
        Vector3 wallPosition;
        
        if (isHorizontal)
        {
            // Wall nằm ngang: nằm trên cạnh trên của ô [row, col]
            // Tức là giữa ô [row, col] và ô [row+1, col]
            wallPosition = new Vector3(
                boardStartPos.x + col * stepSize + squareSize * 0.5f,  // X: center của cột
                0.1f,                                                   // Y: nâng lên
                boardStartPos.z + (row + 1) * stepSize - squareSpacing * 0.5f  // Z: cạnh trên của ô
            );
        }
        else
        {
            // Wall dọc: nằm trên cạnh phải của ô [row, col]
            // Tức là giữa ô [row, col] và ô [row, col+1]
            wallPosition = new Vector3(
                boardStartPos.x + (col + 1) * stepSize - squareSpacing * 0.5f,  // X: cạnh phải của ô
                0.1f,                                                           // Y: nâng lên
                boardStartPos.z + row * stepSize + squareSize * 0.5f            // Z: center của hàng
            );
        }
        
        return wallPosition;
    }
    
    /// <summary>
    /// Lấy vị trí bắt đầu của board (cố định)
    /// </summary>
    Vector3 GetBoardStartPosition()
    {
        // Cố định vị trí bắt đầu tại x = -4.5, z = -3.8
        return new Vector3(-5.0f, 0f, -4.85f);
    }
    
    /// <summary>
    /// Hiển thị các vị trí có thể đặt wall
    /// </summary>
    void ShowValidWallPositions()
    {
        for (int i = 0; i < wallSlots.Length; i++)
        {
            if (wallSlots[i] != null && !wallPlaced[i])
            {
                Renderer renderer = wallSlots[i].GetComponent<Renderer>();
                Collider collider = wallSlots[i].GetComponent<Collider>();
                
                if (renderer != null)
                {
                    renderer.enabled = true;
                    renderer.material.color = validWallColor;
                }
                
                if (collider != null)
                {
                    collider.enabled = true; // Bật collider để có thể click
                }
            }
        }
    }
    
    /// <summary>
    /// Ẩn các vị trí có thể đặt wall
    /// </summary>
    void HideValidWallPositions()
    {
        for (int i = 0; i < wallSlots.Length; i++)
        {
            if (wallSlots[i] != null && !wallPlaced[i])
            {
                Renderer renderer = wallSlots[i].GetComponent<Renderer>();
                Collider collider = wallSlots[i].GetComponent<Collider>();
                
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
                
                if (collider != null)
                {
                    collider.enabled = false; // Tắt collider để không va chạm với quân cờ
                }
            }
        }
    }
      /// <summary>
    /// Đặt wall tại vị trí slot (handles both player clicks and GameManager calls)
    /// </summary>
    public void PlaceWall(int slotIndex, int row, int col, bool isHorizontal)
    {
        // If not in placement mode, this might be a GameManager/AI call
        // Recalculate the correct slot index to ensure accuracy
        int correctSlotIndex = GetSlotIndex(row, col, isHorizontal);
        
        if (correctSlotIndex == -1)
        {
            Debug.LogWarning($"❌ Invalid wall position: row={row}, col={col}, horizontal={isHorizontal}");
            return;
        }
        
        // Use the correct slot index instead of the passed one
        if (correctSlotIndex != slotIndex && slotIndex >= 0)
        {
            Debug.LogWarning($"⚠️ Slot index mismatch! Passed: {slotIndex}, Calculated: {correctSlotIndex}. Using calculated value.");
        }

        Debug.Log($"🖱️ PlaceWall called: slot {correctSlotIndex} at [{row}, {col}], horizontal: {isHorizontal}");

        // For player clicks, require placement mode
        // For AI/GameManager calls, allow placement regardless of mode
        bool isPlayerClick = (slotIndex == correctSlotIndex && isPlacingWall);
        bool isAICall = (slotIndex != correctSlotIndex || !isPlacingWall);
        
        if (isPlayerClick || isAICall)
        {
            // Execute the wall placement
            ExecuteWallPlacement(correctSlotIndex, row, col, isHorizontal);

            // Exit wall placement mode only for player interactions
            if (isPlayerClick)
            {
                isPlacingWall = false;
                HideValidWallPositions();
            }
        }
        else
        {
            Debug.LogWarning("❌ Not in wall placement mode!");
        }
    }
    
    /// <summary>
    /// Tạo wall thực tế (visual)
    /// </summary>
    void CreateActualWall(int slotIndex, int row, int col, bool isHorizontal)
    {
        GameObject actualWall;
        
        if (wallPrefab != null)
        {
            // Sử dụng prefab
            actualWall = Instantiate(wallPrefab, wallSlots[slotIndex].transform.position, 
                isHorizontal ? Quaternion.identity : Quaternion.Euler(0, 90, 0), 
                wallParent.transform);
        }
        else
        {
            // Tạo wall đơn giản
            actualWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            actualWall.transform.position = wallSlots[slotIndex].transform.position;
            actualWall.transform.localScale = wallSlots[slotIndex].transform.localScale;
            actualWall.transform.SetParent(wallParent.transform);
        }
        
        actualWall.name = $"PlacedWall_P{playerID}_{slotIndex}_{(isHorizontal ? "H" : "V")}_{row}_{col}";
        
        // Đặt màu
        Renderer wallRenderer = actualWall.GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            if (wallMaterial != null)
            {
                wallRenderer.material = wallMaterial;
            }
            else
            {
                wallRenderer.material.color = placedWallColor;
            }
        }
        
        // Thêm tag
        actualWall.tag = "PlacedWall";
        
        // Debug log để kiểm tra wall được tạo
        Debug.Log($"🧱 Created actual wall: {actualWall.name}");
    }
    
    /// <summary>
    /// Kiểm tra có thể đặt wall không (không chặn đường đi)
    /// </summary>
    bool CanPlaceWall(int slotIndex, int row, int col, bool isHorizontal)
    {
        // TODO: Implement pathfinding check để đảm bảo cả 2 player vẫn có đường đến đích
        // Tạm thời cho phép đặt tất cả
        return true;
    }
    
    void Update()
    {
        // KeyCode.R đã được chuyển sang GameInputManager để xử lý tập trung
        // GameInputManager sẽ gọi ToggleWallPlaceMode() khi cần thiết
        // Không xử lý input trực tiếp ở đây nữa để tránh xung đột
    }
    
    /// <summary>
    /// Test method để kiểm tra wall slots trong editor
    /// </summary>
    [ContextMenu("Test Wall Positions")]
    public void TestWallPositions()
    {
        if (boardManager == null)
        {
            Debug.LogWarning("❌ BoardManager not found!");
            return;
        }
        
        Debug.Log("🧪 Testing wall slot positions...");
        
        Vector3 boardStart = GetBoardStartPosition();
        Debug.Log($"📍 Board Start Position (FIXED): {boardStart}");
        Debug.Log($"📏 Square Size: {boardManager.squareSize}, Spacing: {boardManager.squareSpacing}");
        
        // Test first square position (should be around x=-4.4, z=-4)
        Vector3 firstSquare = boardManager.GetSquareWorldPosition(0, 0);
        Debug.Log($"🔷 First square [0,0]: {firstSquare}");
        
        // Test horizontal wall at (0,0) - should be between row 0 and 1, at column 0
        Vector3 testHorizontal = CalculateWallSlotPosition(0, 0, true);
        Debug.Log($"📏 Horizontal wall [0,0]: {testHorizontal}");
        
        // Test vertical wall at (0,0) - should be between column 0 and 1, at row 0
        Vector3 testVertical = CalculateWallSlotPosition(0, 0, false);
        Debug.Log($"📏 Vertical wall [0,0]: {testVertical}");
        
        // Test a few more positions
        Vector3 testH_1_1 = CalculateWallSlotPosition(1, 1, true);
        Vector3 testV_1_1 = CalculateWallSlotPosition(1, 1, false);
        Debug.Log($"📏 Horizontal wall [1,1]: {testH_1_1}");
        Debug.Log($"📏 Vertical wall [1,1]: {testV_1_1}");
        
        // Test center positions
        Vector3 testHorizontalCenter = CalculateWallSlotPosition(4, 4, true);
        Vector3 testVerticalCenter = CalculateWallSlotPosition(4, 4, false);
        Debug.Log($"📏 Horizontal wall [4,4]: {testHorizontalCenter}");
        Debug.Log($"📏 Vertical wall [4,4]: {testVerticalCenter}");
    }
    
    /// <summary>
    /// Reset all walls for game restart
    /// </summary>
    public void ResetWalls()
    {
        // Clear placed walls list
        placedWalls.Clear();
        
        // Reset wall count
        wallsRemaining = 10;
        
        // Reset wall placed array
        if (wallPlaced != null)
        {
            for (int i = 0; i < wallPlaced.Length; i++)
            {
                wallPlaced[i] = false;
            }
        }
        
        // Destroy all placed wall GameObjects
        if (wallParent != null)
        {
            foreach (Transform child in wallParent.transform)
            {
                if (child.gameObject.tag == "PlacedWall" || child.gameObject.tag == "Wall")
                {
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }
        }
        
        // Reset wall placement mode
        isPlacingWall = false;
        HideValidWallPositions();
        
        Debug.Log($"🔄 Player {playerID} walls reset. Input protection: {(InputStateManager.Instance != null ? "✅ Active" : "❌ Inactive")}");
    }
    
    /// <summary>
    /// Place wall for AI (called by GameManager)
    /// </summary>
    public void PlaceWallForAI(int row, int col, bool isHorizontal)
    {
        // Find the correct slot index for this position
        int slotIndex = GetSlotIndex(row, col, isHorizontal);
        
        if (slotIndex == -1)
        {
            Debug.LogWarning($"❌ Invalid wall position for AI: row={row}, col={col}, horizontal={isHorizontal}");
            return;
        }
        
        // Place the wall without requiring placement mode
        bool wasPlacingWall = isPlacingWall;
        isPlacingWall = true; // Temporarily enable placement mode
        
        PlaceWall(slotIndex, row, col, isHorizontal);
        
        isPlacingWall = wasPlacingWall; // Restore original state
    }
    
    /// <summary>
    /// Get slot index for a given row, col, and orientation
    /// </summary>
    private int GetSlotIndex(int row, int col, bool isHorizontal)
    {
        if (isHorizontal)
        {
            // Horizontal walls: 8 rows x 9 cols = 72 slots
            if (row < 0 || row >= 8 || col < 0 || col >= 9)
                return -1;
            return row * 9 + col;
        }
        else
        {
            // Vertical walls: 9 rows x 8 cols = 72 slots, starting after horizontal slots
            if (row < 0 || row >= 9 || col < 0 || col >= 8)
                return -1;
            return 72 + row * 8 + col; // 72 = total horizontal slots
        }
    }
    
    /// <summary>
    /// Execute wall placement (for internal use and AI)
    /// </summary>
    public void ExecuteWallPlacement(int slotIndex, int row, int col, bool isHorizontal)
    {
        if (wallsRemaining <= 0)
        {
            Debug.LogWarning($"❌ Player {playerID} has no walls remaining!");
            return;
        }
        
        if (slotIndex < 0 || slotIndex >= wallPlaced.Length || wallPlaced[slotIndex])
        {
            Debug.LogWarning($"❌ Invalid or already placed wall at slot {slotIndex}!");
            return;
        }
        
        // IMPORTANT: Check if wall would block any player's path completely
        if (!ValidateWallPlacementWithGameManager(row, col, isHorizontal))
        {
            Debug.LogWarning($"❌ Wall at ({col}, {row}) would block player path - placement denied!");
            return;
        }
        
        // Mark wall as placed
        wallPlaced[slotIndex] = true;
        wallsRemaining--;
        
        // Create actual wall visual
        CreateActualWall(slotIndex, row, col, isHorizontal);
        
        // Hide wall slot
        if (wallSlots[slotIndex] != null)
        {
            Renderer renderer = wallSlots[slotIndex].GetComponent<Renderer>();
            Collider collider = wallSlots[slotIndex].GetComponent<Collider>();
            
            if (renderer != null)
                renderer.enabled = false;
            
            if (collider != null)
                collider.enabled = false;
        }
        
        Debug.Log($"✅ Player {playerID} placed wall at slot {slotIndex}. Walls remaining: {wallsRemaining}");
        
        // Add to placed walls list
        WallData newWall = new WallData(col, row, isHorizontal);
        placedWalls.Add(newWall);
        
        // Notify GameManager
        if (gameManager != null)
        {
            var method = gameManager.GetType().GetMethod("OnWallPlaced");
            if (method != null)
            {
                method.Invoke(gameManager, new object[] { playerID });
            }
        }
    }
    
    /// <summary>
    /// Check if a wall exists at the specified position
    /// </summary>
    public bool IsWallPlacedAt(int row, int col, bool isHorizontal)
    {
        int slotIndex = GetSlotIndex(row, col, isHorizontal);
        return slotIndex != -1 && slotIndex < wallPlaced.Length && wallPlaced[slotIndex];
    }
    
    /// <summary>
    /// Get all placed walls (for GameManager access)
    /// </summary>
    public List<WallData> GetPlacedWalls()
    {
        return new List<WallData>(placedWalls);
    }
    
    /// <summary>
    /// Validate wall placement with GameManager pathfinding
    /// </summary>
    private bool ValidateWallPlacementWithGameManager(int row, int col, bool isHorizontal)
    {
        try
        {
            // Find GameManager
            var gameManagerComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            MonoBehaviour gameManager = null;
            
            foreach (var component in gameManagerComponents)
            {
                if (component.GetType().Name == "GameManager")
                {
                    gameManager = component;
                    break;
                }
            }
            
            if (gameManager == null)
            {
                Debug.LogWarning("⚠️ GameManager not found for wall validation");
                return true; // Allow placement if can't validate
            }
            
            // Use reflection to call WillPlayersHavePathAfterWall
            var validationMethod = gameManager.GetType().GetMethod("WillPlayersHavePathAfterWall", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (validationMethod != null)
            {
                // GameManager expects (wallX, wallY, isHorizontal) format  
                var result = validationMethod.Invoke(gameManager, new object[] { col, row, isHorizontal });
                return (bool)result;
            }
            else
            {
                Debug.LogWarning("⚠️ WillPlayersHavePathAfterWall method not found");
                return true; // Allow placement if method not found
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Wall validation failed: {e.Message}");
            return true; // Allow placement on error to avoid breaking game
        }
    }
    
    /// <summary>
    /// Fallback check cho input focus nếu không có InputStateManager
    /// </summary>
    private bool IsInputFieldFocusedFallback()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            
            if (selectedObject != null)
            {
                // Kiểm tra TMP_InputField
                var tmpInputField = selectedObject.GetComponent<TMPro.TMP_InputField>();
                if (tmpInputField != null && tmpInputField.isFocused)
                    return true;
                
                // Kiểm tra Legacy InputField
                var legacyInputField = selectedObject.GetComponent<UnityEngine.UI.InputField>();
                if (legacyInputField != null && legacyInputField.isFocused)
                    return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Debug input protection status
    /// </summary>
    [ContextMenu("Debug Input Protection")]
    public void DebugInputProtection()
    {
        Debug.Log("=== WALL PLACER INPUT PROTECTION DEBUG ===");
        Debug.Log($"Player ID: {playerID}");
        Debug.Log($"Walls Remaining: {wallsRemaining}");
        Debug.Log($"Is Placing Wall: {isPlacingWall}");
        
        // Check InputStateManager
        if (InputStateManager.Instance != null)
        {
            Debug.Log($"InputStateManager: ✅ Available");
            Debug.Log($"  - Input Field Focused: {InputStateManager.Instance.IsInputFieldFocused}");
            Debug.Log($"  - Game Input Blocked: {InputStateManager.Instance.IsGameInputBlocked}");
            Debug.Log($"  - Should Block Inputs: {InputStateManager.Instance.ShouldBlockGameInputs()}");
            Debug.Log($"  - Key R Blocked: {InputStateManager.Instance.IsKeyBlocked(KeyCode.R)}");
            Debug.Log($"  - Current Focused Object: {InputStateManager.Instance.CurrentFocusedObject}");
        }
        else
        {
            Debug.Log($"InputStateManager: ❌ Not available");
            bool fallbackFocus = IsInputFieldFocusedFallback();
            Debug.Log($"  - Fallback Input Focus: {fallbackFocus}");
        }
        
        // Test current input state
        bool rawInput = Input.GetKeyDown(KeyCode.R);
        bool safeInput = InputStateManager.Instance != null ? 
            InputStateManager.Instance.GetKeyDownSafe(KeyCode.R) : 
            (rawInput && !IsInputFieldFocusedFallback());
            
        Debug.Log($"Key R Status:");
        Debug.Log($"  - Raw Input: {rawInput}");
        Debug.Log($"  - Safe Input: {safeInput}");
        Debug.Log($"  - Would Toggle Wall: {safeInput && wallsRemaining > 0}");
    }
    
    /// <summary>
    /// Test wall placement protection
    /// </summary>
    [ContextMenu("Test Wall Toggle Protection")]
    public void TestWallToggleProtection()
    {
        Debug.Log("🧪 Testing wall toggle protection...");
        
        bool shouldToggle = false;
        
        if (InputStateManager.Instance != null)
        {
            shouldToggle = InputStateManager.Instance.GetKeyDownSafe(KeyCode.R);
            Debug.Log($"InputStateManager result: {shouldToggle}");
        }
        else
        {
            bool fallbackResult = Input.GetKeyDown(KeyCode.R) && !IsInputFieldFocusedFallback();
            shouldToggle = fallbackResult;
            Debug.Log($"Fallback result: {fallbackResult}");
        }
        
        if (shouldToggle && wallsRemaining > 0)
        {
            Debug.Log("✅ Would execute wall toggle");
            // Don't actually toggle in test
        }
        else if (!shouldToggle)
        {
            Debug.Log("🔒 Wall toggle blocked (input protection active)");
        }
        else if (wallsRemaining <= 0)
        {
            Debug.Log("❌ Wall toggle blocked (no walls remaining)");
        }
    }
}
