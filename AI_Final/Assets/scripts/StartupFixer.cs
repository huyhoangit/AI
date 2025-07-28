using UnityEngine;

/// <summary>
/// Startup component fixer - automatically fixes missing components on game start
/// Add this to the GameManager or any persistent GameObject in the scene
/// </summary>
[System.Serializable]
public class StartupFixer : MonoBehaviour
{
    [Header("Startup Fix Settings")]
    public bool enableAutoFix = true;
    public bool verboseLogging = true;
    
    void Awake()
    {
        if (enableAutoFix)
        {
            if (verboseLogging)
                Debug.Log("🔧 StartupFixer: Running automatic component fix...");
            
            FixMissingPlayerComponents();
        }
    }
    
    /// <summary>
    /// Fix missing components on Player GameObjects
    /// </summary>
    void FixMissingPlayerComponents()
    {
        bool anyFixed = false;
        
        // Find and fix Player1
        GameObject player1 = FindPlayerByName("Player1");
        if (player1 != null)
        {
            if (EnsurePlayerComponents(player1, 1))
                anyFixed = true;
        }
        else if (verboseLogging)
        {
            Debug.LogWarning("⚠️ StartupFixer: Player1 GameObject not found");
        }
        
        // Find and fix Player2
        GameObject player2 = FindPlayerByName("Player2");
        if (player2 != null)
        {
            if (EnsurePlayerComponents(player2, 2))
                anyFixed = true;
        }
        else if (verboseLogging)
        {
            Debug.LogWarning("⚠️ StartupFixer: Player2 GameObject not found");
        }
        
        if (anyFixed && verboseLogging)
        {
            Debug.Log("✅ StartupFixer: Components were added/fixed");
        }
        else if (verboseLogging)
        {
            Debug.Log("ℹ️ StartupFixer: All components already present");
        }
    }
    
    /// <summary>
    /// Find player GameObject by name with fallback search
    /// </summary>
    GameObject FindPlayerByName(string playerName)
    {
        // First try direct find
        GameObject player = GameObject.Find(playerName);
        if (player != null) return player;
        
        // Fallback: search all GameObjects
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(playerName) || obj.name.Equals(playerName))
            {
                if (verboseLogging)
                    Debug.Log($"🔍 StartupFixer: Found {playerName} as {obj.name}");
                return obj;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Ensure a player GameObject has all required components
    /// </summary>
    bool EnsurePlayerComponents(GameObject playerObj, int playerID)
    {
        bool wasModified = false;
        
        // Ensure ChessPlayer component
        ChessPlayer chessPlayer = playerObj.GetComponent<ChessPlayer>();
        if (chessPlayer == null)
        {
            chessPlayer = playerObj.AddComponent<ChessPlayer>();
            chessPlayer.playerID = playerID;
            
            // Set initial positions
            if (playerID == 1)
            {
                chessPlayer.col = 4;
                chessPlayer.row = 0;
            }
            else if (playerID == 2)
            {
                chessPlayer.col = 4;
                chessPlayer.row = 8;
            }
            
            wasModified = true;
            if (verboseLogging)
                Debug.Log($"✅ StartupFixer: Added ChessPlayer to {playerObj.name} (ID: {playerID})");
        }
        else
        {
            // Ensure playerID is set correctly
            if (chessPlayer.playerID != playerID)
            {
                chessPlayer.playerID = playerID;
                wasModified = true;
                if (verboseLogging)
                    Debug.Log($"🔧 StartupFixer: Fixed playerID for {playerObj.name}");
            }
        }
        
        // For Player2, ensure QuoridorAI component
        if (playerID == 2)
        {
            QuoridorAI aiComponent = playerObj.GetComponent<QuoridorAI>();
            if (aiComponent == null)
            {
                aiComponent = playerObj.AddComponent<QuoridorAI>();
                aiComponent.playerID = 2;
                wasModified = true;
                if (verboseLogging)
                    Debug.Log($"✅ StartupFixer: Added QuoridorAI to {playerObj.name}");
            }
        }
        
        // Ensure WallPlacer component
        WallPlacer wallPlacer = playerObj.GetComponent<WallPlacer>();
        if (wallPlacer == null)
        {
            wallPlacer = playerObj.AddComponent<WallPlacer>();
            
            // Set playerID using reflection (WallPlacer might have private fields)
            try
            {
                var playerIDField = wallPlacer.GetType().GetField("playerID", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerIDField != null)
                {
                    playerIDField.SetValue(wallPlacer, playerID);
                }
            }
            catch (System.Exception e)
            {
                if (verboseLogging)
                    Debug.LogWarning($"⚠️ StartupFixer: Could not set WallPlacer playerID: {e.Message}");
            }
            
            wasModified = true;
            if (verboseLogging)
                Debug.Log($"✅ StartupFixer: Added WallPlacer to {playerObj.name}");
        }
        
        return wasModified;
    }
    
    /// <summary>
    /// Manual trigger for component fixing (can be called from Inspector)
    /// </summary>
    [ContextMenu("Fix Components Now")]
    public void FixComponentsNow()
    {
        Debug.Log("🔧 StartupFixer: Manual fix triggered");
        FixMissingPlayerComponents();
    }
    
    /// <summary>
    /// Check if all required components are present
    /// </summary>
    [ContextMenu("Check Component Status")]
    public void CheckComponentStatus()
    {
        Debug.Log("🔍 StartupFixer: Checking component status...");
        
        GameObject player1 = FindPlayerByName("Player1");
        if (player1 != null)
        {
            LogPlayerStatus(player1, 1);
        }
        
        GameObject player2 = FindPlayerByName("Player2");
        if (player2 != null)
        {
            LogPlayerStatus(player2, 2);
        }
    }
    
    void LogPlayerStatus(GameObject player, int expectedID)
    {
        ChessPlayer chess = player.GetComponent<ChessPlayer>();
        QuoridorAI ai = player.GetComponent<QuoridorAI>();
        WallPlacer wall = player.GetComponent<WallPlacer>();
        
        Debug.Log($"📊 {player.name} Status:");
        Debug.Log($"   ChessPlayer: {(chess != null ? "✅" : "❌")} {(chess != null ? $"(ID: {chess.playerID})" : "")}");
        Debug.Log($"   QuoridorAI: {(ai != null ? "✅" : (expectedID == 2 ? "❌" : "➖"))} {(ai != null ? $"(ID: {ai.playerID})" : "")}");
        Debug.Log($"   WallPlacer: {(wall != null ? "✅" : "❌")}");
    }
}
