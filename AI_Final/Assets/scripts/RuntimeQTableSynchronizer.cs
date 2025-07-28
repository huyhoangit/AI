using UnityEngine;

/// <summary>
/// Runtime Q-Table Synchronizer - Forces all AI components to reload Q-table in runtime
/// Use this when Q-table is loaded but some AI components still have empty tables
/// </summary>
public class RuntimeQTableSynchronizer : MonoBehaviour
{
    [Header("Synchronization Settings")]
    public bool autoSyncOnStart = true;
    public float syncInterval = 5f; // Sync every 5 seconds
    
    void Start()
    {
        if (autoSyncOnStart)
        {
            InvokeRepeating("CheckAndSyncAllAI", 2f, syncInterval);
        }
    }
    
    [ContextMenu("Force Sync All AI Q-Tables")]
    public void ForceSyncAllAI()
    {
        Debug.Log("🔄 FORCE SYNCING ALL AI Q-TABLES");
        
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        if (allAIs.Length == 0)
        {
            Debug.LogWarning("⚠️ No QuoridorAI components found!");
            return;
        }
        
        // Find the AI with the largest Q-table (reference)
        QuoridorAI referenceAI = null;
        int maxQTableSize = 0;
        
        foreach (var ai in allAIs)
        {
            int qTableSize = GetQTableSize(ai);
            Debug.Log($"📊 AI '{ai.gameObject.name}': {qTableSize} states");
            
            if (qTableSize > maxQTableSize)
            {
                maxQTableSize = qTableSize;
                referenceAI = ai;
            }
        }
        
        if (referenceAI != null && maxQTableSize > 1000)
        {
            Debug.Log($"✅ Reference AI found: '{referenceAI.gameObject.name}' with {maxQTableSize} states");
            
            // Sync all other AIs to match the reference
            foreach (var ai in allAIs)
            {
                if (ai != referenceAI)
                {
                    SyncAIToReference(ai, referenceAI);
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No trained AI found to use as reference. Forcing reload from file...");
            
            // Force all AIs to reload from file
            foreach (var ai in allAIs)
            {
                ForceReloadFromFile(ai);
            }
        }
    }
    
    void CheckAndSyncAllAI()
    {
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        bool needsSync = false;
        
        foreach (var ai in allAIs)
        {
            int qTableSize = GetQTableSize(ai);
            if (qTableSize == 0) // Empty Q-table detected
            {
                needsSync = true;
                break;
            }
        }
        
        if (needsSync)
        {
            Debug.LogWarning("⚠️ Detected empty Q-tables, auto-syncing...");
            ForceSyncAllAI();
        }
    }
    
    void SyncAIToReference(QuoridorAI targetAI, QuoridorAI referenceAI)
    {
        Debug.Log($"🔄 Syncing '{targetAI.gameObject.name}' to reference '{referenceAI.gameObject.name}'");
        
        // Copy settings from reference
        targetAI.useQLearning = referenceAI.useQLearning;
        targetAI.isTrainedModel = referenceAI.isTrainedModel;
        targetAI.allowQTableSaving = referenceAI.allowQTableSaving;
        
        // Force reload Q-table
        targetAI.ReloadQTable();
        
        // Verify sync
        int targetSize = GetQTableSize(targetAI);
        int referenceSize = GetQTableSize(referenceAI);
        
        if (targetSize > 1000)
        {
            Debug.Log($"✅ Sync successful: '{targetAI.gameObject.name}' now has {targetSize} states");
        }
        else
        {
            Debug.LogWarning($"⚠️ Sync failed: '{targetAI.gameObject.name}' still has {targetSize} states");
        }
    }
    
    void ForceReloadFromFile(QuoridorAI ai)
    {
        Debug.Log($"📁 Force reloading Q-table from file for: '{ai.gameObject.name}'");
        
        // Set to trained model settings
        ai.isTrainedModel = true;
        ai.allowQTableSaving = false;
        ai.useQLearning = true;
        
        // Force reload
        ai.ReloadQTable();
        
        // Verify
        int qTableSize = GetQTableSize(ai);
        if (qTableSize > 1000)
        {
            Debug.Log($"✅ File reload successful: {qTableSize} states loaded");
        }
        else
        {
            Debug.LogError($"❌ File reload failed: Only {qTableSize} states loaded");
        }
    }
    
    int GetQTableSize(QuoridorAI ai)
    {
        try
        {
            var qAgentField = typeof(QuoridorAI).GetField("qAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (qAgentField != null)
            {
                var qAgent = qAgentField.GetValue(ai);
                if (qAgent != null)
                {
                    var getQTableSizeMethod = qAgent.GetType().GetMethod("GetQTableSize");
                    if (getQTableSizeMethod != null)
                    {
                        return (int)getQTableSizeMethod.Invoke(qAgent, null);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error getting Q-table size for {ai.gameObject.name}: {e.Message}");
        }
        
        return 0;
    }
    
    [ContextMenu("Debug All AI Status")]
    public void DebugAllAIStatus()
    {
        Debug.Log("=== ALL AI STATUS DEBUG ===");
        
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        for (int i = 0; i < allAIs.Length; i++)
        {
            var ai = allAIs[i];
            int qTableSize = GetQTableSize(ai);
            
            Debug.Log($"\n🤖 AI[{i}]: {ai.gameObject.name}");
            Debug.Log($"   🎯 Use Q-Learning: {ai.useQLearning}");
            Debug.Log($"   🎓 Is Trained Model: {ai.isTrainedModel}");
            Debug.Log($"   💾 Allow Q-Table Saving: {ai.allowQTableSaving}");
            Debug.Log($"   📊 Q-Table Size: {qTableSize} states");
            
            // Get epsilon info
            try
            {
                var qAgentField = typeof(QuoridorAI).GetField("qAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (qAgentField != null)
                {
                    var qAgent = qAgentField.GetValue(ai);
                    if (qAgent != null)
                    {
                        var getEpsilonInfoMethod = qAgent.GetType().GetMethod("GetEpsilonInfo");
                        if (getEpsilonInfoMethod != null)
                        {
                            var epsilonInfo = getEpsilonInfoMethod.Invoke(qAgent, null);
                            var currentEpsilonField = epsilonInfo.GetType().GetField("currentEpsilon");
                            if (currentEpsilonField != null)
                            {
                                float epsilon = (float)currentEpsilonField.GetValue(epsilonInfo);
                                Debug.Log($"   🎲 Current Epsilon: {epsilon:F3}");
                                
                                if (qTableSize > 1000 && epsilon > 0.2f)
                                {
                                    Debug.LogWarning($"   ⚠️ MISMATCH: Large Q-table but high epsilon (should be ~0.1)");
                                }
                                else if (qTableSize > 1000 && epsilon <= 0.15f)
                                {
                                    Debug.Log($"   ✅ CORRECT: Trained model with exploitation epsilon");
                                }
                                else if (qTableSize == 0)
                                {
                                    Debug.LogError($"   ❌ PROBLEM: Empty Q-table!");
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"   ❌ Error getting epsilon info: {e.Message}");
            }
        }
        
        Debug.Log("=== DEBUG COMPLETE ===");
    }
    
    [ContextMenu("Fix GameManager AI")]
    public void FixGameManagerAI()
    {
        Debug.Log("🔧 FIXING GAMEMANAGER AI");
        
        // Find GameManager
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<MonoBehaviour>()?.gameObject;
            Debug.LogWarning("⚠️ GameManager not found by name, searching for components...");
        }
        
        if (gameManager != null)
        {
            QuoridorAI gmAI = gameManager.GetComponent<QuoridorAI>();
            if (gmAI != null)
            {
                Debug.Log($"🎯 Found GameManager AI: {gmAI.gameObject.name}");
                
                // Force fix this specific AI
                ForceReloadFromFile(gmAI);
                
                Debug.Log("✅ GameManager AI fixed");
            }
            else
            {
                Debug.LogWarning("⚠️ No QuoridorAI component found on GameManager");
            }
        }
        else
        {
            Debug.LogError("❌ GameManager not found!");
        }
    }
    
    [ContextMenu("Emergency Fix All")]
    public void EmergencyFixAll()
    {
        Debug.Log("🚨 EMERGENCY FIX ALL AI COMPONENTS");
        
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        foreach (var ai in allAIs)
        {
            Debug.Log($"🔧 Emergency fixing: {ai.gameObject.name}");
            
            // Reset to trained model settings
            ai.useQLearning = true;
            ai.isTrainedModel = true;
            ai.allowQTableSaving = false;
            
            // Force multiple reload attempts
            for (int i = 0; i < 3; i++)
            {
                ai.ReloadQTable();
                
                int qTableSize = GetQTableSize(ai);
                if (qTableSize > 1000)
                {
                    Debug.Log($"✅ Emergency fix successful on attempt {i + 1}: {qTableSize} states");
                    break;
                }
                else if (i == 2)
                {
                    Debug.LogError($"❌ Emergency fix failed after 3 attempts: {qTableSize} states");
                }
            }
        }
        
        Debug.Log("🚨 Emergency fix complete!");
    }
}
