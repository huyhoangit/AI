using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Configuration")]
    public int boardSize = 9;
    public float squareSize = 1.0f;
    public float squareSpacing = 0.1f;
    
    [Header("Prefabs")]
    public GameObject boardSquarePrefab; // Kéo prefab BoardSquare vào đây
    public GameObject playerPrefab; // Prefab Chess cho người chơi
    public GameObject wallPrefab; // Prefab Wall cho tường
    
    [Header("Board Setup")]
    public Transform boardParent; // Parent chứa toàn bộ board
    public Vector3 boardOffset = Vector3.zero;
    public bool centerBoard = true;
    
    [Header("Visual Settings")]
    public Material lightSquareMaterial;
    public Material darkSquareMaterial;
    public Color highlightColor = Color.yellow;
    
    // Singleton pattern để tránh duplicate
    public static BoardManager Instance { get; private set; }
    
    // Mảng lưu trữ các ô vuông
    private GameObject[,] boardSquares;
    private Vector3 boardStartPosition;
    private bool boardCreated = false;
    
    void Awake()
    {
        // Singleton pattern - chỉ cho phép 1 BoardManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("⚠️ Duplicate BoardManager detected! Destroying this instance.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Kiểm tra xem đã có board nào được tạo chưa
        GameObject existingBoard = GameObject.Find("QuoridorBoard");
        if (existingBoard != null && existingBoard.transform.childCount > 0)
        {
            Debug.LogWarning("⚠️ Board already exists! Skipping board creation.");
            boardCreated = true;
            return;
        }
        
        CreateBoard3D();
    }
    
    /// <summary>
    /// Tạo bàn cờ 3D với 9x9 ô vuông
    /// </summary>
    public void CreateBoard3D()
    {
        // Kiểm tra đã tạo board chưa
        if (boardCreated)
        {
            Debug.LogWarning("⚠️ Board already created! Skipping...");
            return;
        }
        
        Debug.Log("🎯 Bắt đầu tạo bàn cờ 3D...");
        
        // Tạo parent object nếu chưa có
        SetupBoardParent();
        
        // Xóa board cũ nếu có
        ClearExistingBoard();
        
        // Khởi tạo mảng board
        boardSquares = new GameObject[boardSize, boardSize];
        
        // Tính toán vị trí bắt đầu
        CalculateBoardStartPosition();
        
        // Tạo từng ô vuông
        CreateAllSquares();
        
        // Đặt camera nhìn board
        SetupCamera();
        
        // Đánh dấu đã tạo board
        boardCreated = true;
        
        Debug.Log($"✅ Hoàn thành tạo bàn cờ {boardSize}x{boardSize}!");
    }
    
    /// <summary>
    /// Thiết lập parent object cho board
    /// </summary>
    private void SetupBoardParent()
    {
        if (boardParent == null)
        {
            // Kiểm tra xem đã có QuoridorBoard nào chưa
            GameObject existingBoard = GameObject.Find("QuoridorBoard");
            if (existingBoard != null)
            {
                Debug.Log("🔄 Using existing QuoridorBoard parent");
                boardParent = existingBoard.transform;
                return;
            }
            
            GameObject parentObj = new GameObject("QuoridorBoard");
            boardParent = parentObj.transform;
            boardParent.position = boardOffset;
            Debug.Log("🎯 Created new QuoridorBoard parent");
        }
    }
    
    /// <summary>
    /// Xóa board hiện tại
    /// </summary>
    private void ClearExistingBoard()
    {
        if (boardParent != null)
        {
            // Xóa tất cả con của board parent
            int childCount = boardParent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    DestroyImmediate(boardParent.GetChild(i).gameObject);
                }
                else
                {
                    Destroy(boardParent.GetChild(i).gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Tính toán vị trí bắt đầu của board
    /// </summary>
    private void CalculateBoardStartPosition()
    {
        float totalSize = (boardSize - 1) * (squareSize + squareSpacing);
        
        if (centerBoard)
        {
            boardStartPosition = boardParent.position + new Vector3(
                -totalSize / 2f,
                0,
                -totalSize / 2f
            );
        }
        else
        {
            boardStartPosition = boardParent.position;
        }
    }
    
    /// <summary>
    /// Tạo tất cả ô vuông
    /// </summary>
    private void CreateAllSquares()
    {
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                CreateSingleSquare(row, col);
            }
        }
    }
    
    /// <summary>
    /// Tạo một ô vuông tại vị trí chỉ định
    /// </summary>
    private void CreateSingleSquare(int row, int col)
    {
        // Tính vị trí world
        Vector3 worldPosition = boardStartPosition + new Vector3(
            col * (squareSize + squareSpacing),
            0,
            row * (squareSize + squareSpacing)
        );
        
        // Tạo ô vuông
        GameObject square = CreateSquareObject(worldPosition, row, col);
        
        // Cấu hình ô vuông
        ConfigureSquareObject(square, row, col);
        
        // Lưu vào mảng
        boardSquares[row, col] = square;
    }
    
    /// <summary>
    /// Tạo object ô vuông
    /// </summary>
    private GameObject CreateSquareObject(Vector3 position, int row, int col)
    {
        GameObject square;
        
        if (boardSquarePrefab != null)
        {
            // Sử dụng prefab có sẵn
            square = Instantiate(boardSquarePrefab, position, Quaternion.identity, boardParent);
        }
        else
        {
            // Tạo cube mặc định
            square = GameObject.CreatePrimitive(PrimitiveType.Cube);
            square.transform.position = position;
            square.transform.localScale = new Vector3(squareSize, 0.2f, squareSize);
            square.transform.SetParent(boardParent);
            
            Debug.LogWarning("⚠️ Chưa gán BoardSquare prefab, đang sử dụng cube mặc định!");
        }
        
        return square;
    }
    
    /// <summary>
    /// Cấu hình object ô vuông
    /// </summary>
    private void ConfigureSquareObject(GameObject square, int row, int col)
    {
        // Đặt tên
        square.name = $"Square_{row}_{col}";
        
        // Thêm BoardSquare component nếu chưa có
        BoardSquare boardSquareComponent = square.GetComponent<BoardSquare>();
        if (boardSquareComponent == null)
        {
            boardSquareComponent = square.AddComponent<BoardSquare>();
        }
        
        // Cấu hình BoardSquare
        boardSquareComponent.row = row;
        boardSquareComponent.col = col;
        
        // Thêm collider cho tương tác
        if (square.GetComponent<Collider>() == null)
        {
            square.AddComponent<BoxCollider>();
        }
        
        // Áp dụng màu sắc
        ApplySquareColor(square, row, col);
        
        // Thêm tag
        square.tag = "BoardSquare";
    }
    
    /// <summary>
    /// Áp dụng màu sắc pattern cờ vua
    /// </summary>
    private void ApplySquareColor(GameObject square, int row, int col)
    {
        Renderer renderer = square.GetComponent<Renderer>();
        if (renderer == null) return;
        
        bool isLightSquare = (row + col) % 2 == 0;
        
        if (isLightSquare && lightSquareMaterial != null)
        {
            renderer.material = lightSquareMaterial;
        }
        else if (!isLightSquare && darkSquareMaterial != null)
        {
            renderer.material = darkSquareMaterial;
        }
        else
        {
            // Màu mặc định
            Color squareColor = isLightSquare ? new Color(0.9f, 0.9f, 0.8f) : new Color(0.4f, 0.3f, 0.2f);
            renderer.material.color = squareColor;
        }
    }
    
    /// <summary>
    /// Thiết lập camera nhìn board
    /// </summary>
    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Đặt camera ở vị trí nhìn xuống board
            Vector3 boardCenter = boardParent.position;
            mainCamera.transform.position = boardCenter + new Vector3(0, 8, -6);
            mainCamera.transform.LookAt(boardCenter);
        }
    }
    
    /// <summary>
    /// Lấy ô vuông tại vị trí
    /// </summary>
    public GameObject GetSquareAt(int row, int col)
    {
        if (IsValidPosition(row, col) && boardSquares != null)
        {
            return boardSquares[row, col];
        }
        return null;
    }
    
    /// <summary>
    /// Kiểm tra vị trí hợp lệ
    /// </summary>
    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < boardSize && col >= 0 && col < boardSize;
    }
    
    /// <summary>
    /// Lấy vị trí world của ô
    /// </summary>
    public Vector3 GetSquareWorldPosition(int row, int col)
    {
        GameObject square = GetSquareAt(row, col);
        return square != null ? square.transform.position : Vector3.zero;
    }
    
    /// <summary>
    /// Highlight ô vuông
    /// </summary>
    public void HighlightSquare(int row, int col, bool highlight = true)
    {
        GameObject square = GetSquareAt(row, col);
        if (square != null)
        {
            Renderer renderer = square.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (highlight)
                {
                    renderer.material.color = highlightColor;
                }
                else
                {
                    ApplySquareColor(square, row, col);
                }
            }
        }
    }
    
    /// <summary>
    /// Reset tất cả highlight
    /// </summary>
    public void ClearAllHighlights()
    {
        if (boardSquares == null) return;
        
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                ApplySquareColor(boardSquares[row, col], row, col);
            }
        }
    }
    
    /// <summary>
    /// Tìm ô gần nhất với vị trí world
    /// </summary>
    public Vector2Int GetNearestSquarePosition(Vector3 worldPos)
    {
        float minDistance = float.MaxValue;
        Vector2Int nearestPos = Vector2Int.zero;
        
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                Vector3 squarePos = GetSquareWorldPosition(row, col);
                float distance = Vector3.Distance(worldPos, squarePos);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPos = new Vector2Int(col, row);
                }
            }
        }
        
        return nearestPos;
    }
    
    /// <summary>
    /// Lấy vị trí bắt đầu của board (cho WallPlacer sử dụng)
    /// </summary>
    public Vector3 GetBoardStartPosition()
    {
        return boardStartPosition;
    }
    
    // Editor functions
    [ContextMenu("Tạo Board 3D")]
    public void CreateBoardEditor()
    {
        CreateBoard3D();
    }
    
    [ContextMenu("Xóa Board")]
    public void ClearBoardEditor()
    {
        ClearExistingBoard();
    }
    
    [ContextMenu("Test Highlight")]
    public void TestHighlight()
    {
        // Test highlight một số ô
        HighlightSquare(0, 0, true);
        HighlightSquare(4, 4, true);
        HighlightSquare(8, 8, true);
    }
}
