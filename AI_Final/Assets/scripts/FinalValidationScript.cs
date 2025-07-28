using UnityEngine;

/// <summary>
/// Final validation script to ensure all systems are working correctly
/// Run this to validate Q-Learning preservation and compilation status
/// </summary>
public class FinalValidationScript : MonoBehaviour
{
    [Header("Validation Settings")]
    public bool runOnStart = true;
    
    void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(RunCompleteValidation());
        }
    }
    
    System.Collections.IEnumerator RunCompleteValidation()
    {
        yield return new WaitForSeconds(1f); // Wait for initialization
        
        Debug.Log("🔍 =========================");
        Debug.Log("🔍 FINAL SYSTEM VALIDATION");
        Debug.Log("🔍 =========================");
        
        ValidateQLearningSystem();
        yield return new WaitForSeconds(0.5f);
        
        ValidateTrainerStatus();
        yield return new WaitForSeconds(0.5f);
        
        ValidateFilePaths();
        yield return new WaitForSeconds(0.5f);
        
        ValidateComponents();
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("✅ =========================");
        Debug.Log("✅ VALIDATION COMPLETE");
        Debug.Log("✅ =========================");
    }
    
    [ContextMenu("Run Complete Validation")]
    public void RunCompleteValidationManual()
    {
        StartCoroutine(RunCompleteValidation());
    }
    
    void ValidateQLearningSystem()
    {
        Debug.Log("\n🧠 Q-LEARNING SYSTEM VALIDATION:");
        
        QuoridorAI[] ais = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        if (ais.Length == 0)
        {
            Debug.LogWarning("⚠️ No QuoridorAI components found!");
            return;
        }
        
        foreach (var ai in ais)
        {
            Debug.Log($"📱 AI Component: {ai.gameObject.name}");
            Debug.Log($"   🎯 Use Q-Learning: {ai.useQLearning}");
            Debug.Log($"   💾 Allow Q-Table Saving: {ai.allowQTableSaving}");
            Debug.Log($"   🎓 Is Trained Model: {ai.isTrainedModel}");
            
            // Check Q-agent via reflection
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
                        
                        if (qTableSize > 1000)
                        {
                            Debug.Log("   ✅ TRAINED MODEL DETECTED - Should use exploitation mode");
                        }
                        else
                        {
                            Debug.Log("   🆕 NEW MODEL - Training mode");
                        }
                    }
                }
            }
        }
    }
    
    void ValidateTrainerStatus()
    {
        Debug.Log("\n🏋️ TRAINER STATUS VALIDATION:");
        
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null)
        {
            Debug.Log($"📱 Trainer found: {trainer.gameObject.name}");
            Debug.Log($"   🎯 GameObject Active: {trainer.gameObject.activeInHierarchy}");
            Debug.Log($"   🚀 Auto Start Training: {trainer.autoStartTraining}");
            Debug.Log($"   🔒 Preserve Existing Q-Table: {trainer.preserveExistingQTable}");
            
            if (!trainer.autoStartTraining)
            {
                Debug.Log("   ✅ TRAINER DISABLED - Q-table will be preserved");
            }
            else
            {
                Debug.LogWarning("   ⚠️ TRAINER ENABLED - May overwrite Q-table!");
            }
        }
        else
        {
            Debug.Log("   ✅ No trainer found - Q-table safe");
        }
    }
    
    void ValidateFilePaths()
    {
        Debug.Log("\n📁 FILE PATH VALIDATION:");
        
        string[] pathsToCheck = {
            System.IO.Path.Combine(Application.streamingAssetsPath, "qtable.json"),
            System.IO.Path.Combine(Application.dataPath, "qtable.json"),
            System.IO.Path.Combine(Application.persistentDataPath, "qtable.json"),
            System.IO.Path.Combine(Application.dataPath, "StreamingAssets", "qtable.json")
        };
        
        bool foundQTable = false;
        
        foreach (string path in pathsToCheck)
        {
            if (System.IO.File.Exists(path))
            {
                var fileInfo = new System.IO.FileInfo(path);
                Debug.Log($"   ✅ Found: {path}");
                Debug.Log($"      📊 Size: {fileInfo.Length / 1024f:F1} KB");
                Debug.Log($"      📅 Modified: {fileInfo.LastWriteTime}");
                foundQTable = true;
            }
        }
        
        if (!foundQTable)
        {
            Debug.LogWarning("   ⚠️ No Q-table file found!");
        }
    }
    
    void ValidateComponents()
    {
        Debug.Log("\n🔧 COMPONENT VALIDATION:");
        
        // Check for missing scripts
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int missingScripts = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Player1" || obj.name == "Player2")
            {
                Component[] components = obj.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        missingScripts++;
                        Debug.LogWarning($"   ⚠️ Missing script on: {obj.name}");
                    }
                }
                
                // Check specific components
                if (obj.GetComponent<ChessPlayer>() == null)
                {
                    Debug.LogWarning($"   ⚠️ Missing ChessPlayer on: {obj.name}");
                }
            }
        }
        
        if (missingScripts == 0)
        {
            Debug.Log("   ✅ No missing scripts detected");
        }
        else
        {
            Debug.LogWarning($"   ⚠️ Found {missingScripts} missing scripts");
        }
    }
    
    [ContextMenu("Quick Fix - Disable All Trainers")]
    public void QuickFixDisableAllTrainers()
    {
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null)
        {
            trainer.autoStartTraining = false;
            trainer.gameObject.SetActive(false);
            Debug.Log("🔒 QUICK FIX: All trainers disabled");
        }
        
        QuickTrainerDisabler disabler = FindFirstObjectByType<QuickTrainerDisabler>();
        if (disabler != null)
        {
            disabler.DisableSelfPlayTrainer();
        }
    }
    
    [ContextMenu("Force Q-Learning Exploitation Mode")]
    public void ForceExploitationMode()
    {
        QuoridorAI[] ais = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        foreach (var ai in ais)
        {
            ai.allowQTableSaving = false;
            ai.isTrainedModel = true;
            
            // Force epsilon to exploitation via reflection
            var qAgentField = typeof(QuoridorAI).GetField("qAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (qAgentField != null)
            {
                var qAgent = qAgentField.GetValue(ai);
                if (qAgent != null)
                {
                    var setTrainedEpsilonMethod = qAgent.GetType().GetMethod("SetTrainedEpsilon");
                    if (setTrainedEpsilonMethod != null)
                    {
                        setTrainedEpsilonMethod.Invoke(qAgent, new object[] { 0.1f, 1000, 1000 });
                        Debug.Log($"🎯 Forced exploitation mode (ε=0.1) on {ai.gameObject.name}");
                    }
                }
            }
        }
    }
}
