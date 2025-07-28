using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script để tự động sửa các missing script references
/// </summary>
public class AutoMissingScriptFixer : MonoBehaviour
{
    [Header("Auto Fix Settings")]
    public bool autoFixOnStart = true;
    public bool fixPlayerReferences = true;
    public bool fixGameManagerReferences = true;
    public bool createMissingPlayers = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixMissingScripts();
        }
    }
    
    [ContextMenu("Fix Missing Scripts")]
    public void FixMissingScripts()
    {
        Debug.Log("🔧 Starting to fix missing script references...");
        
        if (fixPlayerReferences)
        {
            FixPlayerReferences();
        }
        
        if (fixGameManagerReferences)
        {
            FixGameManagerReferences();
        }
        
        Debug.Log("✅ Missing script fix complete!");
    }
    
    /// <summary>
    /// Sửa references cho Player1 và Player2
    /// </summary>
    void FixPlayerReferences()
    {
        // Tìm Player1 và Player2 GameObjects
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 != null)
        {
            FixPlayerScripts(player1, 1);
        }
        else if (createMissingPlayers)
        {
            Debug.LogWarning("⚠️ Player1 GameObject not found! Creating new Player1...");
            player1 = CreatePlayerGameObject("Player1", 1);
            FixPlayerScripts(player1, 1);
        }
        else
        {
            Debug.LogWarning("⚠️ Player1 GameObject not found!");
        }
        
        if (player2 != null)
        {
            FixPlayerScripts(player2, 2);
        }
        else if (createMissingPlayers)
        {
            Debug.LogWarning("⚠️ Player2 GameObject not found! Creating new Player2...");
            player2 = CreatePlayerGameObject("Player2", 2);
            FixPlayerScripts(player2, 2);
        }
        else
        {
            Debug.LogWarning("⚠️ Player2 GameObject not found!");
        }
    }
    
    /// <summary>
    /// Tạo GameObject cho player nếu không tồn tại
    /// </summary>
    GameObject CreatePlayerGameObject(string playerName, int playerID)
    {
        GameObject playerObj = new GameObject(playerName);
        
        // Set position based on player
        if (playerID == 1)
        {
            playerObj.transform.position = new Vector3(0, -0.24f, -4.4f);
        }
        else
        {
            playerObj.transform.position = new Vector3(0, -0.24f, 4.4f);
        }
        
        Debug.Log($"✅ Created {playerName} GameObject at position {playerObj.transform.position}");
        return playerObj;
    }
    
    /// <summary>
    /// Sửa scripts cho một player
    /// </summary>
    void FixPlayerScripts(GameObject playerObj, int playerID)
    {
        Debug.Log($"🔧 Fixing scripts for {playerObj.name} (Player {playerID})");
        
        // Remove any missing script components first
        RemoveMissingScripts(playerObj);
        
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
            
            DestroyImmediate(primitive);
            Debug.Log($"✅ Added Renderer to {playerObj.name}");
        }
        
        Debug.Log($"✅ {playerObj.name} scripts fixed successfully");
    }
    
    /// <summary>
    /// Xóa các missing script components
    /// </summary>
    void RemoveMissingScripts(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        List<Component> toRemove = new List<Component>();
        
        foreach (Component comp in components)
        {
            if (comp == null)
            {
                toRemove.Add(comp);
            }
        }
        
        foreach (Component comp in toRemove)
        {
            if (comp != null)
            {
                DestroyImmediate(comp);
                Debug.Log($"🗑️ Removed missing script component from {obj.name}");
            }
        }
    }
    
    /// <summary>
    /// Sửa GameManager references
    /// </summary>
    void FixGameManagerReferences()
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
    
    /// <summary>
    /// Reset tất cả references
    /// </summary>
    [ContextMenu("Reset All References")]
    public void ResetAllReferences()
    {
        Debug.Log("🔄 Resetting all references...");
        
        // Reset GameManager references
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
    
    /// <summary>
    /// Force restart game
    /// </summary>
    [ContextMenu("Force Restart Game")]
    public void ForceRestartGame()
    {
        Debug.Log("🔄 Force restarting game...");
        
        // Reset all references first
        ResetAllReferences();
        
        // Fix scripts
        FixMissingScripts();
        
        // Restart GameManager
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            // Force reinitialize
            gameManager.InitializeGame();
        }
        
        Debug.Log("✅ Game force restarted");
    }
} 