using UnityEngine;

/// <summary>
/// Script tự động tạo board, chess players, và wall placement system
/// </summary>
public class QuoridorGameSetup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject chessPlayerPrefab; // Prefab cho quân chess
    public GameObject wallPrefab; // Prefab cho wall
    
    [Header("Game Settings")]
    public bool autoSetupOnStart = true;
    public Vector3 player1StartPos = new Vector3(4, 0, 0); // Player 1 start at bottom center
    public Vector3 player2StartPos = new Vector3(4, 0, 8); // Player 2 start at top center
    
    private BoardManager boardManager;
    private GameObject player1Chess, player2Chess;
    private WallPlacer player1WallPlacer, player2WallPlacer;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupGame();
        }
    }
    
    /// <summary>
    /// Setup toàn bộ game
    /// </summary>
    [ContextMenu("Setup Quoridor Game")]
    public void SetupGame()
    {
        Debug.Log("🎮 Setting up Quoridor game...");
        
        // 0. Setup GameManager first
        SetupGameManager();
        
        // 1. Tìm hoặc tạo BoardManager
        SetupBoardManager();
        
        // 2. Tạo chess players
        SetupChessPlayers();
        
        // 3. Tạo wall placement system
        SetupWallPlacementSystem();
        
        // 4. Setup GameManager
        //SetupGameManager();
        
        // 5. Setup UI
        SetupUI();
        
        Debug.Log("✅ Quoridor game setup complete!");
        Debug.Log("🎯 Controls:");
        Debug.Log("   - Click chess piece to select");
        Debug.Log("   - Click green squares to move");
        Debug.Log("   - Press R to place walls (when chess selected)");
        Debug.Log("   - Click red areas to place wall");
        Debug.Log("   - Press Escape to deselect");
    }
    
    /// <summary>
    /// Setup BoardManager
    /// </summary>
    void SetupBoardManager()
    {
        boardManager = BoardManager.Instance;
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }
        
        if (boardManager == null)
        {
            Debug.LogWarning("⚠️ No BoardManager found! Please add BoardManager to scene first.");
            return;
        }
        
        Debug.Log("✅ BoardManager found");
    }
    
    /// <summary>
    /// Setup chess players
    /// </summary>
    void SetupChessPlayers()
    {
        if (boardManager == null) return;
        
        // Tạo Player 1
        if (player1Chess == null)
        {
            player1Chess = CreateChessPlayer(1, 4, 0, player1StartPos);
        }
        
        // Tạo Player 2
        if (player2Chess == null)
        {
            player2Chess = CreateChessPlayer(2, 4, 8, player2StartPos);
        }
        
        Debug.Log("✅ Chess players created");
    }
    
    /// <summary>
    /// Tạo một chess player
    /// </summary>
    GameObject CreateChessPlayer(int playerID, int startCol, int startRow, Vector3 worldPos)
    {
        GameObject chessObj;
        
        if (chessPlayerPrefab != null)
        {
            chessObj = Instantiate(chessPlayerPrefab, worldPos, Quaternion.identity);
        }
        else
        {
            // Tạo chess đơn giản
            chessObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chessObj.transform.position = worldPos;
            chessObj.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        }
        
        chessObj.name = $"ChessPlayer_{playerID}";
        
        // Thêm ChessPlayer component nếu chưa có
        var chessPlayer = chessObj.GetComponent<MonoBehaviour>();
        if (chessPlayer == null || chessPlayer.GetType().Name != "ChessPlayer")
        {
            // Sử dụng reflection để add component
            var chessPlayerType = System.Type.GetType("ChessPlayer");
            if (chessPlayerType != null)
            {
                chessPlayer = (MonoBehaviour)chessObj.AddComponent(chessPlayerType);
                
                // Set properties
                var playerIDField = chessPlayerType.GetField("playerID");
                var rowField = chessPlayerType.GetField("row");
                var colField = chessPlayerType.GetField("col");
                
                if (playerIDField != null) playerIDField.SetValue(chessPlayer, playerID);
                if (rowField != null) rowField.SetValue(chessPlayer, startRow);
                if (colField != null) colField.SetValue(chessPlayer, startCol);
            }
        }
        
        // Đặt màu khác biệt cho 2 player
        Renderer renderer = chessObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = playerID == 1 ? Color.blue : Color.red;
        }
        
        // Thêm collider để có thể click
        if (chessObj.GetComponent<Collider>() == null)
        {
            chessObj.AddComponent<CapsuleCollider>();
        }
        
        // Thêm tag
        chessObj.tag = $"Player{playerID}";
        
        return chessObj;
    }
    
    /// <summary>
    /// Setup wall placement system
    /// </summary>
    void SetupWallPlacementSystem()
    {
        // Tạo WallPlacer cho Player 1
        GameObject wallPlacer1Obj = new GameObject("WallPlacer_Player1");
        var wallPlacer1Type = System.Type.GetType("WallPlacer");
        if (wallPlacer1Type != null)
        {
            var wallPlacer1 = wallPlacer1Obj.AddComponent(wallPlacer1Type) as MonoBehaviour;
            
            // Set properties
            var playerIDField = wallPlacer1Type.GetField("playerID");
            var wallPrefabField = wallPlacer1Type.GetField("wallPrefab");
            
            if (playerIDField != null) playerIDField.SetValue(wallPlacer1, 1);
            if (wallPrefabField != null && wallPrefab != null) wallPrefabField.SetValue(wallPlacer1, wallPrefab);
        }
        
        // Tạo WallPlacer cho Player 2
        GameObject wallPlacer2Obj = new GameObject("WallPlacer_Player2");
        if (wallPlacer1Type != null)
        {
            var wallPlacer2 = wallPlacer2Obj.AddComponent(wallPlacer1Type) as MonoBehaviour;
            
            // Set properties
            var playerIDField = wallPlacer1Type.GetField("playerID");
            var wallPrefabField = wallPlacer1Type.GetField("wallPrefab");
            
            if (playerIDField != null) playerIDField.SetValue(wallPlacer2, 2);
            if (wallPrefabField != null && wallPrefab != null) wallPrefabField.SetValue(wallPlacer2, wallPrefab);
        }
        
        Debug.Log("✅ Wall placement system created");
    }
    
    /// <summary>
    /// Setup GameManager
    /// </summary>
    void SetupGameManager()
    {
        // Tìm hoặc tạo GameManager
        var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        bool foundGameManager = false;
        
        foreach (var component in allComponents)
        {
            if (component.GetType().Name == "GameManager")
            {
                foundGameManager = true;
                break;
            }
        }
        
        if (!foundGameManager)
        {
            Debug.LogWarning("⚠️ GameManager not found! Please add GameManager script to a GameObject in the scene.");
            Debug.LogWarning("💡 Create an empty GameObject and add the GameManager script to it, then run setup again.");
        }
        else
        {
            Debug.Log("✅ GameManager found");
        }
    }
    
    /// <summary>
    /// Setup GameManager references after components are created
    /// </summary>
    void SetupGameManagerAfterComponents()
    {
        // Tìm GameManager bằng reflection
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
            Debug.LogWarning("⚠️ No GameManager found! Please add GameManager to scene first.");
            return;
        }
        
        // Set player references using reflection
        var player1Field = gameManager.GetType().GetField("player1");
        var player2Field = gameManager.GetType().GetField("player2");
        var wallPlacer1Field = gameManager.GetType().GetField("wallPlacer1");
        var wallPlacer2Field = gameManager.GetType().GetField("wallPlacer2");
        
        if (player1Field != null && player1Chess != null)
        {
            var player1Component = player1Chess.GetComponent<ChessPlayer>();
            if (player1Component != null)
                player1Field.SetValue(gameManager, player1Component);
        }
        
        if (player2Field != null && player2Chess != null)
        {
            var player2Component = player2Chess.GetComponent<ChessPlayer>();
            if (player2Component != null)
                player2Field.SetValue(gameManager, player2Component);
        }
        
        if (wallPlacer1Field != null && player1WallPlacer != null)
        {
            wallPlacer1Field.SetValue(gameManager, player1WallPlacer);
        }
        
        if (wallPlacer2Field != null && player2WallPlacer != null)
        {
            wallPlacer2Field.SetValue(gameManager, player2WallPlacer);
        }
        
        Debug.Log("✅ GameManager references setup complete");
    }
    
    /// <summary>
    /// Setup UI instructions
    /// </summary>
    void SetupUI()
    {
        // Tìm QuoridorUI bằng reflection
        var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        bool foundUI = false;
        
        foreach (var component in allComponents)
        {
            if (component.GetType().Name == "QuoridorUI")
            {
                foundUI = true;
                break;
            }
        }
        
        if (!foundUI)
        {
            Debug.LogWarning("⚠️ QuoridorUI not found! Please add QuoridorUI script to a GameObject in the scene for UI.");
            Debug.LogWarning("💡 Create an empty GameObject and add the QuoridorUI script to it for game UI.");
        }
        else
        {
            Debug.Log("✅ QuoridorUI found");
        }
    }
    
    /// <summary>
    /// Reset game
    /// </summary>
    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        // Xóa chess players
        if (player1Chess != null) DestroyImmediate(player1Chess);
        if (player2Chess != null) DestroyImmediate(player2Chess);
        
        // Xóa wall placers
        GameObject[] wallPlacers = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in wallPlacers)
        {
            if (obj.name.Contains("WallPlacer"))
            {
                DestroyImmediate(obj);
            }
        }
        
        // Xóa walls
        GameObject wallParent = GameObject.Find("WallParent");
        if (wallParent != null)
        {
            DestroyImmediate(wallParent);
        }
        
        Debug.Log("🔄 Game reset complete");
    }
}
