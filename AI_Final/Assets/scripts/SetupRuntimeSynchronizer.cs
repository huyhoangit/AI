using UnityEngine;

/// <summary>
/// Setup Runtime Q-Table Synchronizer - Automatically creates and configures synchronizer
/// </summary>
public class SetupRuntimeSynchronizer : MonoBehaviour
{
    [Header("Setup Settings")]
    public bool setupOnStart = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupSynchronizer();
        }
    }
    
    [ContextMenu("Setup Runtime Synchronizer")]
    public void SetupSynchronizer()
    {
        Debug.Log("🔧 Setting up Runtime Q-Table Synchronizer...");
        
        // Check if synchronizer already exists
        RuntimeQTableSynchronizer existingSyncer = FindFirstObjectByType<RuntimeQTableSynchronizer>();
        
        if (existingSyncer == null)
        {
            // Create new GameObject for synchronizer
            GameObject syncerObj = new GameObject("RuntimeQTableSynchronizer");
            
            // Add the synchronizer component
            RuntimeQTableSynchronizer syncer = syncerObj.AddComponent<RuntimeQTableSynchronizer>();
            
            // Configure settings
            syncer.autoSyncOnStart = true;
            
            Debug.Log("✅ Runtime Q-Table Synchronizer created and configured");
            
            // Only use DontDestroyOnLoad in Play mode
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(syncerObj);
            }
            
            // Immediate first sync
            syncer.ForceSyncAllAI();
        }
        else
        {
            Debug.Log("ℹ️ Runtime Q-Table Synchronizer already exists");
            
            // Force sync anyway
            existingSyncer.ForceSyncAllAI();
        }
    }
    
    [ContextMenu("Quick Fix All AI")]
    public void QuickFixAllAI()
    {
        Debug.Log("⚡ QUICK FIX ALL AI - SETUP + SYNC");
        
        // Setup synchronizer first
        SetupSynchronizer();
        
        // Get the synchronizer
        RuntimeQTableSynchronizer syncer = FindFirstObjectByType<RuntimeQTableSynchronizer>();
        
        if (syncer != null)
        {
            // Run all fix methods
            syncer.DebugAllAIStatus();
            syncer.EmergencyFixAll();
            syncer.FixGameManagerAI();
            syncer.ForceSyncAllAI();
            
            Debug.Log("✅ Quick fix complete!");
        }
        else
        {
            Debug.LogError("❌ Failed to setup synchronizer!");
        }
    }
    
    [ContextMenu("Test AI After Fix")]
    public void TestAIAfterFix()
    {
        Debug.Log("🧪 TESTING AI AFTER FIX");
        
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        foreach (var ai in allAIs)
        {
            Debug.Log($"\n🤖 Testing AI: {ai.gameObject.name}");
            
            // Test algorithm selection
            if (ai.useQLearning)
            {
                Debug.Log($"   ✅ Using Q-Learning");
                
                // Get Q-table info
                try
                {
                    var qAgentField = typeof(QuoridorAI).GetField("qAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (qAgentField != null)
                    {
                        var qAgent = qAgentField.GetValue(ai);
                        if (qAgent != null)
                        {
                            var getQTableSizeMethod = qAgent.GetType().GetMethod("GetQTableSize");
                            var getEpsilonInfoMethod = qAgent.GetType().GetMethod("GetEpsilonInfo");
                            
                            if (getQTableSizeMethod != null && getEpsilonInfoMethod != null)
                            {
                                int qTableSize = (int)getQTableSizeMethod.Invoke(qAgent, null);
                                var epsilonInfo = getEpsilonInfoMethod.Invoke(qAgent, null);
                                var currentEpsilonField = epsilonInfo.GetType().GetField("currentEpsilon");
                                
                                if (currentEpsilonField != null)
                                {
                                    float epsilon = (float)currentEpsilonField.GetValue(epsilonInfo);
                                    
                                    Debug.Log($"   📊 Q-Table: {qTableSize} states");
                                    Debug.Log($"   🎲 Epsilon: {epsilon:F3}");
                                    
                                    if (qTableSize > 10000 && epsilon < 0.2f)
                                    {
                                        Debug.Log($"   ✅ TRAINED MODEL ACTIVE!");
                                    }
                                    else if (qTableSize > 1000)
                                    {
                                        Debug.Log($"   ⚠️ Has Q-table but high epsilon");
                                    }
                                    else
                                    {
                                        Debug.LogError($"   ❌ Q-table too small!");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"   ❌ Error testing AI: {e.Message}");
                }
            }
            else
            {
                Debug.Log($"   🔄 Using Minimax");
            }
        }
        
        Debug.Log("🧪 Test complete!");
    }
}
