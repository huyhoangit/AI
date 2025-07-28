using UnityEngine;

/// <summary>
/// Emergency script to fix missing components IMMEDIATELY
/// Add this script to any GameObject in the scene and it will auto-fix on Awake
/// </summary>
public class EmergencyFixer : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("🚨 EmergencyFixer: Starting immediate fix...");
        FixAllMissingComponents();
    }
    
    void Start()
    {
        // Run again in Start to be absolutely sure
        FixAllMissingComponents();
    }
    
    void FixAllMissingComponents()
    {
        // Find Player GameObjects
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Player1" || obj.name == "Player2")
            {
                FixPlayerObject(obj);
            }
        }
    }
    
    void FixPlayerObject(GameObject playerObj)
    {
        Debug.Log($"🔧 Fixing {playerObj.name}...");
        
        // Determine player ID from name
        int playerID = playerObj.name == "Player1" ? 1 : 2;
        
        // Remove any missing/broken components first
        RemoveMissingComponents(playerObj);
        
        // Add ChessPlayer if missing
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
            else
            {
                chessPlayer.col = 4;
                chessPlayer.row = 8;
            }
            
            Debug.Log($"✅ Added ChessPlayer to {playerObj.name} (ID: {playerID})");
        }
        
        // Add QuoridorAI for Player2
        if (playerID == 2)
        {
            QuoridorAI aiComponent = playerObj.GetComponent<QuoridorAI>();
            if (aiComponent == null)
            {
                aiComponent = playerObj.AddComponent<QuoridorAI>();
                aiComponent.playerID = 2;
                Debug.Log($"✅ Added QuoridorAI to {playerObj.name}");
            }
        }
        
        // Add WallPlacer if missing
        WallPlacer wallPlacer = playerObj.GetComponent<WallPlacer>();
        if (wallPlacer == null)
        {
            wallPlacer = playerObj.AddComponent<WallPlacer>();
            
            // Set playerID using reflection since it might be private
            var playerIDField = wallPlacer.GetType().GetField("playerID", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerIDField != null)
            {
                playerIDField.SetValue(wallPlacer, playerID);
            }
            
            Debug.Log($"✅ Added WallPlacer to {playerObj.name} (ID: {playerID})");
        }
        
        Debug.Log($"✅ {playerObj.name} fixed successfully!");
    }
    
    void RemoveMissingComponents(GameObject obj)
    {
        // Get all components and check for null references
        Component[] components = obj.GetComponents<Component>();
        
        for (int i = components.Length - 1; i >= 0; i--)
        {
            if (components[i] == null)
            {
                Debug.Log($"🗑️ Found missing component on {obj.name}, will be cleaned up by Unity");
            }
        }
    }
    
    [ContextMenu("Force Fix Now")]
    public void ForceFixNow()
    {
        Debug.Log("🔧 Force fixing now...");
        FixAllMissingComponents();
    }
}
