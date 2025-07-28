using UnityEngine;

/// <summary>
/// Emergency Q-Table Reloader - Forces all AI components to reload their Q-tables
/// Use this when Q-tables exist but are not being loaded properly
/// </summary>
public class EmergencyQTableReloader : MonoBehaviour
{
    [ContextMenu("Emergency Reload All Q-Tables")]
    public void EmergencyReloadAllQTables()
    {
        Debug.Log("🚨 EMERGENCY Q-TABLE RELOAD INITIATED 🚨");
        
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        if (allAIs.Length == 0)
        {
            Debug.LogWarning("⚠️ No QuoridorAI components found!");
            return;
        }
        
        foreach (var ai in allAIs)
        {
            Debug.Log($"🔄 Reloading Q-table for: {ai.gameObject.name}");
            
            // Force reload Q-table
            ai.ReloadQTable();
            
            // Wait a frame
            StartCoroutine(ValidateReloadAfterFrame(ai));
        }
    }
    
    System.Collections.IEnumerator ValidateReloadAfterFrame(QuoridorAI ai)
    {
        yield return new WaitForEndOfFrame();
        
        // Validate reload success
        ai.CheckQLearningStatus();
    }
    
    [ContextMenu("Quick Debug All AI Components")]
    public void QuickDebugAllAI()
    {
        Debug.Log("🔍 QUICK DEBUG ALL AI COMPONENTS");
        
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        for (int i = 0; i < allAIs.Length; i++)
        {
            var ai = allAIs[i];
            Debug.Log($"\n🤖 AI Component [{i}]: {ai.gameObject.name}");
            Debug.Log($"   🎯 Use Q-Learning: {ai.useQLearning}");
            Debug.Log($"   🎓 Is Trained Model: {ai.isTrainedModel}");
            Debug.Log($"   💾 Allow Q-Table Saving: {ai.allowQTableSaving}");
            Debug.Log($"   📁 Q-Table Path: {ai.qTablePath}");
            
            // Try to get Q-agent info via reflection
            var qAgentField = typeof(QuoridorAI).GetField("qAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (qAgentField != null)
            {
                var qAgent = qAgentField.GetValue(ai);
                if (qAgent != null)
                {
                    var getQTableSizeMethod = qAgent.GetType().GetMethod("GetQTableSize");
                    if (getQTableSizeMethod != null)
                    {
                        int qTableSize = (int)getQTableSizeMethod.Invoke(qAgent, null);
                        Debug.Log($"   📊 Q-Table Size: {qTableSize} states");
                        
                        if (qTableSize == 0)
                        {
                            Debug.LogWarning($"   ⚠️ Q-Table is EMPTY for {ai.gameObject.name}!");
                        }
                        else if (qTableSize > 1000)
                        {
                            Debug.Log($"   ✅ TRAINED MODEL detected ({qTableSize} states)");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"   ❌ Q-Agent is NULL for {ai.gameObject.name}");
                }
            }
        }
    }
    
    [ContextMenu("Force All AI to Trained Mode")]
    public void ForceAllAIToTrainedMode()
    {
        Debug.Log("🎓 FORCING ALL AI TO TRAINED MODE");
        
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        foreach (var ai in allAIs)
        {
            Debug.Log($"🎓 Setting trained mode for: {ai.gameObject.name}");
            
            // Force trained mode settings
            ai.isTrainedModel = true;
            ai.allowQTableSaving = false;
            
            // Force reload with trained settings
            ai.ReloadQTable();
        }
    }
    
    [ContextMenu("Check Q-Table Files")]
    public void CheckQTableFiles()
    {
        Debug.Log("📁 CHECKING Q-TABLE FILES");
        
        string[] pathsToCheck = {
            System.IO.Path.Combine(Application.streamingAssetsPath, "qtable.json"),
            System.IO.Path.Combine(Application.dataPath, "StreamingAssets", "qtable.json"),
            System.IO.Path.Combine(Application.dataPath, "qtable.json"),
            System.IO.Path.Combine(Application.persistentDataPath, "qtable.json")
        };
        
        bool foundAny = false;
        
        for (int i = 0; i < pathsToCheck.Length; i++)
        {
            string path = pathsToCheck[i];
            Debug.Log($"🔍 Checking path {i+1}: {path}");
            
            if (System.IO.File.Exists(path))
            {
                var fileInfo = new System.IO.FileInfo(path);
                Debug.Log($"   ✅ FOUND: Size {fileInfo.Length / 1024f:F1} KB, Modified: {fileInfo.LastWriteTime}");
                foundAny = true;
                
                // Try to read first few characters to validate JSON
                try
                {
                    string firstChars = System.IO.File.ReadAllText(path).Substring(0, Mathf.Min(100, (int)fileInfo.Length));
                    Debug.Log($"   📄 Content preview: {firstChars}...");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"   ⚠️ Cannot read file: {e.Message}");
                }
            }
            else
            {
                Debug.Log($"   ❌ NOT FOUND");
            }
        }
        
        if (!foundAny)
        {
            Debug.LogError("❌ NO Q-TABLE FILES FOUND IN ANY LOCATION!");
        }
    }
    
    void Start()
    {
        // Auto-run checks on start
        Invoke("AutoCheckAndReload", 2f);
    }
    
    void AutoCheckAndReload()
    {
        Debug.Log("🔄 Auto-checking Q-table status...");
        QuickDebugAllAI();
        
        // If any AI has empty Q-table, try emergency reload
        QuoridorAI[] allAIs = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        bool needsReload = false;
        
        foreach (var ai in allAIs)
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
                        int qTableSize = (int)getQTableSizeMethod.Invoke(qAgent, null);
                        if (qTableSize == 0)
                        {
                            needsReload = true;
                            break;
                        }
                    }
                }
            }
        }
        
        if (needsReload)
        {
            Debug.LogWarning("⚠️ Detected empty Q-tables, attempting emergency reload...");
            EmergencyReloadAllQTables();
        }
    }
}
