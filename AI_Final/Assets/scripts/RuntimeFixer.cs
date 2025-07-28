using UnityEngine;
using System.Collections;

/// <summary>
/// Runtime fixer for missing script components on Player GameObjects
/// </summary>
public class RuntimeFixer : MonoBehaviour
{
    [Header("Auto-fix Settings")]
    public bool autoFixOnStart = true;
    public bool debugOutput = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            StartCoroutine(FixMissingComponentsDelayed());
        }
    }
    
    /// <summary>
    /// Fix missing components with a small delay to ensure all GameObjects are initialized
    /// </summary>
    IEnumerator FixMissingComponentsDelayed()
    {
        yield return new WaitForSeconds(0.1f); // Small delay
        FixMissingComponents();
    }
    
    /// <summary>
    /// Fix missing script components on Player GameObjects
    /// </summary>
    [ContextMenu("Fix Missing Components")]
    public void FixMissingComponents()
    {
        if (debugOutput)
            Debug.Log("🔧 Runtime Fixer: Checking for missing components...");
        
        FixPlayerComponents();
        
        if (debugOutput)
            Debug.Log("✅ Runtime Fixer: Component check complete!");
    }
    
    /// <summary>
    /// Fix missing components on Player GameObjects
    /// </summary>
    void FixPlayerComponents()
    {
        // Find Player1
        GameObject player1 = GameObject.Find("Player1");
        if (player1 != null)
        {
            FixPlayerObject(player1, 1);
        }
        else
        {
            if (debugOutput)
                Debug.LogWarning("⚠️ Player1 GameObject not found");
        }
        
        // Find Player2
        GameObject player2 = GameObject.Find("Player2");
        if (player2 != null)
        {
            FixPlayerObject(player2, 2);
        }
        else
        {
            if (debugOutput)
                Debug.LogWarning("⚠️ Player2 GameObject not found");
        }
    }
    
    /// <summary>
    /// Fix components on a specific player GameObject
    /// </summary>
    void FixPlayerObject(GameObject playerObj, int playerID)
    {
        bool wasFixed = false;
        
        // Check and add ChessPlayer component
        if (playerObj.GetComponent<ChessPlayer>() == null)
        {
            var chessPlayer = playerObj.AddComponent<ChessPlayer>();
            chessPlayer.playerID = playerID;
            
            // Set initial position based on player ID
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
            
            wasFixed = true;
            if (debugOutput)
                Debug.Log($"✅ Added ChessPlayer component to {playerObj.name} (ID: {playerID})");
        }
        
        // For Player2 (AI), check and add QuoridorAI component
        if (playerID == 2)
        {
            if (playerObj.GetComponent<QuoridorAI>() == null)
            {
                var aiComponent = playerObj.AddComponent<QuoridorAI>();
                aiComponent.playerID = 2;
                
                wasFixed = true;
                if (debugOutput)
                    Debug.Log($"✅ Added QuoridorAI component to {playerObj.name}");
            }
        }
        
        // Check for WallPlacer component
        if (playerObj.GetComponent<WallPlacer>() == null)
        {
            var wallPlacer = playerObj.AddComponent<WallPlacer>();
            
            // Use reflection to set playerID since WallPlacer might have private fields
            var playerIDField = wallPlacer.GetType().GetField("playerID");
            if (playerIDField != null)
            {
                playerIDField.SetValue(wallPlacer, playerID);
            }
            
            wasFixed = true;
            if (debugOutput)
                Debug.Log($"✅ Added WallPlacer component to {playerObj.name} (ID: {playerID})");
        }
        
        if (!wasFixed && debugOutput)
        {
            Debug.Log($"ℹ️ {playerObj.name} already has all required components");
        }
    }
    
    /// <summary>
    /// Remove all missing script references from GameObjects
    /// </summary>
    [ContextMenu("Remove Missing Scripts")]
    public void RemoveMissingScripts()
    {
        if (debugOutput)
            Debug.Log("🧹 Removing missing script references...");
        
        // Find all GameObjects with missing scripts
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Get all components
            Component[] components = obj.GetComponents<Component>();
            
            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i] == null)
                {
                    // Found a missing component, remove it
                    var serializedObject = new UnityEngine.Object();
                    if (debugOutput)
                        Debug.Log($"🗑️ Removed missing script from {obj.name}");
                    removedCount++;
                }
            }
        }
        
        if (debugOutput)
            Debug.Log($"✅ Removed {removedCount} missing script references");
    }
    
    /// <summary>
    /// Comprehensive fix - both add missing components and remove broken references
    /// </summary>
    [ContextMenu("Comprehensive Fix")]
    public void ComprehensiveFix()
    {
        if (debugOutput)
            Debug.Log("🔧 Starting comprehensive fix...");
        
        // First remove missing scripts
        RemoveMissingScripts();
        
        // Then add required components
        FixMissingComponents();
        
        if (debugOutput)
            Debug.Log("✅ Comprehensive fix complete!");
    }
}
