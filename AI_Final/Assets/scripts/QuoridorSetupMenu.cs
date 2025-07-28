using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor menu để setup và fix các vấn đề trong Quoridor game
/// </summary>
public class QuoridorSetupMenu
{
    [MenuItem("Quoridor/Fix Missing Scripts")]
    public static void FixMissingScripts()
    {
        Debug.Log("🔧 Starting to fix missing script references...");
        
        // Tìm và fix Player1
        GameObject player1 = GameObject.Find("Player1");
        if (player1 != null)
        {
            FixPlayerScripts(player1, 1);
        }
        else
        {
            Debug.LogWarning("⚠️ Player1 GameObject not found!");
        }
        
        // Tìm và fix Player2
        GameObject player2 = GameObject.Find("Player2");
        if (player2 != null)
        {
            FixPlayerScripts(player2, 2);
        }
        else
        {
            Debug.LogWarning("⚠️ Player2 GameObject not found!");
        }
        
        // Fix GameManager references
        FixGameManagerReferences();
        
        Debug.Log("✅ Missing script fix complete!");
    }
    
    [MenuItem("Quoridor/Setup Complete Game")]
    public static void SetupCompleteGame()
    {
        Debug.Log("🎮 Setting up complete Quoridor game...");
        
        // 1. Fix Q-table issues first
        FixQTableIssues();
        
        // 2. Fix missing scripts
        FixMissingScripts();
        
        // 3. Setup board if needed
        SetupBoard();
        
        // 4. Setup UI if needed
        SetupUI();
        
        Debug.Log("✅ Complete game setup finished!");
    }
    
    [MenuItem("Quoridor/Fix Q-Table Issues")]
    public static void FixQTableIssues()
    {
        Debug.Log("🔧 Fixing Q-table issues...");
        
        string qTablePath = "Assets/qtable.json";
        
        // Delete corrupted file if exists
        if (System.IO.File.Exists(qTablePath))
        {
            try
            {
                System.IO.File.Delete(qTablePath);
                Debug.Log($"🗑️ Deleted corrupted Q-table file: {qTablePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error deleting Q-table file: {e.Message}");
            }
        }
        
        // Create fresh Q-table
        try
        {
            var emptyQTable = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, float>>();
            
            // Add sample state
            string sampleState = "4,0-4,8-10-10-";
            emptyQTable[sampleState] = new System.Collections.Generic.Dictionary<string, float>();
            emptyQTable[sampleState]["M:4,1"] = 0.1f;
            emptyQTable[sampleState]["M:3,0"] = 0.1f;
            emptyQTable[sampleState]["M:5,0"] = 0.1f;
            
            // Serialize and save
            var serialization = new Serialization<string, System.Collections.Generic.Dictionary<string, float>>(emptyQTable);
            string jsonContent = JsonUtility.ToJson(serialization, true);
            
            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(qTablePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // Write file
            System.IO.File.WriteAllText(qTablePath, jsonContent);
            Debug.Log($"✅ Created fresh Q-table at: {qTablePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error creating fresh Q-table: {e.Message}");
        }
    }
    
    [MenuItem("Quoridor/Reset All References")]
    public static void ResetAllReferences()
    {
        Debug.Log("🔄 Resetting all references...");
        
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.player1 = null;
            gameManager.player2 = null;
            gameManager.wallPlacer1 = null;
            gameManager.wallPlacer2 = null;
        }
        
        Debug.Log("✅ All references reset");
    }
    
    private static void FixPlayerScripts(GameObject playerObj, int playerID)
    {
        Debug.Log($"🔧 Fixing scripts for {playerObj.name} (Player {playerID})");
        
        // Kiểm tra và thêm ChessPlayer component nếu thiếu
        ChessPlayer chessPlayer = playerObj.GetComponent<ChessPlayer>();
        if (chessPlayer == null)
        {
            chessPlayer = playerObj.AddComponent<ChessPlayer>();
            Debug.Log($"✅ Added ChessPlayer component to {playerObj.name}");
        }
        
        // Set player properties
        chessPlayer.playerID = playerID;
        if (playerID == 1)
        {
            chessPlayer.row = 0;
            chessPlayer.col = 4;
        }
        else
        {
            chessPlayer.row = 8;
            chessPlayer.col = 4;
        }
        
        // Kiểm tra và thêm WallPlacer component nếu thiếu
        WallPlacer wallPlacer = playerObj.GetComponent<WallPlacer>();
        if (wallPlacer == null)
        {
            wallPlacer = playerObj.AddComponent<WallPlacer>();
            Debug.Log($"✅ Added WallPlacer component to {playerObj.name}");
        }
        
        // Set wall placer properties
        wallPlacer.playerID = playerID;
        wallPlacer.wallsRemaining = 10;
        
        // Thêm collider nếu thiếu
        if (playerObj.GetComponent<Collider>() == null)
        {
            playerObj.AddComponent<CapsuleCollider>();
            Debug.Log($"✅ Added Collider to {playerObj.name}");
        }
        
        // Thêm renderer nếu thiếu
        if (playerObj.GetComponent<Renderer>() == null)
        {
            // Tạo primitive mesh
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            MeshFilter meshFilter = primitive.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = primitive.GetComponent<MeshRenderer>();
            
            if (meshFilter != null && meshRenderer != null)
            {
                playerObj.AddComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
                playerObj.AddComponent<MeshRenderer>().material = meshRenderer.material;
                
                // Set color based on player
                Material mat = new Material(meshRenderer.material);
                mat.color = playerID == 1 ? Color.blue : Color.red;
                playerObj.GetComponent<MeshRenderer>().material = mat;
            }
            
            Object.DestroyImmediate(primitive);
            Debug.Log($"✅ Added Renderer to {playerObj.name}");
        }
        
        Debug.Log($"✅ {playerObj.name} scripts fixed successfully");
    }
    
    private static void FixGameManagerReferences()
    {
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("⚠️ GameManager not found!");
            return;
        }
        
        Debug.Log("🔧 Fixing GameManager references...");
        
        // Tìm Player1 và Player2
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 != null)
        {
            ChessPlayer chessPlayer1 = player1.GetComponent<ChessPlayer>();
            WallPlacer wallPlacer1 = player1.GetComponent<WallPlacer>();
            
            if (chessPlayer1 != null)
            {
                gameManager.player1 = chessPlayer1;
                Debug.Log("✅ Connected Player1 to GameManager");
            }
            
            if (wallPlacer1 != null)
            {
                gameManager.wallPlacer1 = wallPlacer1;
                Debug.Log("✅ Connected WallPlacer1 to GameManager");
            }
        }
        
        if (player2 != null)
        {
            ChessPlayer chessPlayer2 = player2.GetComponent<ChessPlayer>();
            WallPlacer wallPlacer2 = player2.GetComponent<WallPlacer>();
            
            if (chessPlayer2 != null)
            {
                gameManager.player2 = chessPlayer2;
                Debug.Log("✅ Connected Player2 to GameManager");
            }
            
            if (wallPlacer2 != null)
            {
                gameManager.wallPlacer2 = wallPlacer2;
                Debug.Log("✅ Connected WallPlacer2 to GameManager");
            }
        }
        
        Debug.Log("✅ GameManager references fixed successfully");
    }
    
    private static void SetupBoard()
    {
        BoardManager boardManager = BoardManager.Instance;
        if (boardManager == null)
        {
            boardManager = Object.FindFirstObjectByType<BoardManager>();
        }
        
        if (boardManager == null)
        {
            Debug.LogWarning("⚠️ BoardManager not found! Please add BoardManager to scene.");
        }
        else
        {
            Debug.Log("✅ BoardManager found");
        }
    }
    
    private static void SetupUI()
    {
        // Check if UI components exist
        var uiComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        bool foundUI = false;
        
        foreach (var component in uiComponents)
        {
            if (component.GetType().Name.Contains("UI") || component.GetType().Name.Contains("Chat"))
            {
                foundUI = true;
                break;
            }
        }
        
        if (!foundUI)
        {
            Debug.LogWarning("⚠️ No UI components found. Consider adding chat or UI components.");
        }
        else
        {
            Debug.Log("✅ UI components found");
        }
    }
} 