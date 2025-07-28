using UnityEngine;

/// <summary>
/// Debug utility to check Q-Learning status and file paths
/// Helps diagnose why Q-table might be retraining
/// </summary>
public class QLearningStatusChecker : MonoBehaviour
{
    [Header("Check Settings")]
    public bool checkOnStart = true;
    
    void Start()
    {
        if (checkOnStart)
        {
            CheckCompleteStatus();
        }
    }
    
    [ContextMenu("Check Q-Learning Status")]
    public void CheckCompleteStatus()
    {
        Debug.Log("=== Q-LEARNING STATUS CHECKER ===");
        
        CheckQTableFile();
        CheckAIComponents();
        CheckTrainerComponents();
        CheckPathSettings();
        
        Debug.Log("=== STATUS CHECK COMPLETE ===");
    }
    
    void CheckQTableFile()
    {
        Debug.Log("\n📁 Q-TABLE FILE CHECK:");
        
        // Check StreamingAssets path
        string streamingPath = Application.streamingAssetsPath + "/qtable.json";
        if (System.IO.File.Exists(streamingPath))
        {
            var fileInfo = new System.IO.FileInfo(streamingPath);
            Debug.Log($"✅ StreamingAssets Q-table found: {fileInfo.Length / 1024f:F1} KB");
            Debug.Log($"📅 Last modified: {fileInfo.LastWriteTime}");
        }
        else
        {
            Debug.LogWarning("❌ No Q-table in StreamingAssets");
        }
        
        // Check other common paths
        string dataPath = Application.dataPath + "/qtable.json";
        if (System.IO.File.Exists(dataPath))
        {
            var fileInfo = new System.IO.FileInfo(dataPath);
            Debug.Log($"ℹ️ Assets Q-table found: {fileInfo.Length / 1024f:F1} KB");
        }
        
        string persistentPath = Application.persistentDataPath + "/qtable.json";
        if (System.IO.File.Exists(persistentPath))
        {
            var fileInfo = new System.IO.FileInfo(persistentPath);
            Debug.Log($"ℹ️ PersistentData Q-table found: {fileInfo.Length / 1024f:F1} KB");
        }
    }
    
    void CheckAIComponents()
    {
        Debug.Log("\n🤖 AI COMPONENTS CHECK:");
        
        QuoridorAI[] ais = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        Debug.Log($"Found {ais.Length} QuoridorAI components");
        
        for (int i = 0; i < ais.Length; i++)
        {
            var ai = ais[i];
            Debug.Log($"  AI[{i}] - {ai.gameObject.name}:");
            Debug.Log($"    🎯 Use Q-Learning: {ai.useQLearning}");
            Debug.Log($"    💾 Allow Q-Table Saving: {ai.allowQTableSaving}");
            Debug.Log($"    🎓 Is Trained Model: {ai.isTrainedModel}");
            Debug.Log($"    📁 Q-Table Path: {ai.qTablePath}");
            
            // Use reflection to access private qAgent field
            var qAgentField = typeof(QuoridorAI).GetField("qAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (qAgentField != null)
            {
                var qAgent = qAgentField.GetValue(ai);
                if (qAgent != null)
                {
                    var getQTableSizeMethod = qAgent.GetType().GetMethod("GetQTableSize");
                    var epsilonField = qAgent.GetType().GetField("epsilon", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                    if (getQTableSizeMethod != null)
                    {
                        int qTableSize = (int)getQTableSizeMethod.Invoke(qAgent, null);
                        Debug.Log($"    📊 Q-States: {qTableSize}");
                    }
                    
                    if (epsilonField != null)
                    {
                        float epsilon = (float)epsilonField.GetValue(qAgent);
                        Debug.Log($"    🎯 Epsilon: {epsilon:F3}");
                    }
                }
            }
        }
    }
    
    void CheckTrainerComponents()
    {
        Debug.Log("\n🏋️ TRAINER COMPONENTS CHECK:");
        
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null)
        {
            Debug.Log($"✅ QuoridorSelfPlayTrainer found on: {trainer.gameObject.name}");
            Debug.Log($"  🎯 GameObject Active: {trainer.gameObject.activeInHierarchy}");
            Debug.Log($"  🚀 Auto Start Training: {trainer.autoStartTraining}");
            Debug.Log($"  🔒 Preserve Existing Q-Table: {trainer.preserveExistingQTable}");
            Debug.Log($"  📊 Episodes: {trainer.numEpisodes}");
            
            // Use reflection to access private isTraining field
            var isTrainingField = typeof(QuoridorSelfPlayTrainer).GetField("isTraining", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (isTrainingField != null)
            {
                bool isTraining = (bool)isTrainingField.GetValue(trainer);
                Debug.Log($"  🔄 Is Training: {isTraining}");
            }
            else
            {
                Debug.Log($"  🔄 Is Training: Unable to access (private field)");
            }
        }
        else
        {
            Debug.Log("ℹ️ No QuoridorSelfPlayTrainer found");
        }
    }
    
    void CheckPathSettings()
    {
        Debug.Log("\n📂 PATH SETTINGS:");
        Debug.Log($"  📁 Application.dataPath: {Application.dataPath}");
        Debug.Log($"  💾 Application.persistentDataPath: {Application.persistentDataPath}");
        Debug.Log($"  🎮 Application.streamingAssetsPath: {Application.streamingAssetsPath}");
        Debug.Log($"  📍 Current working directory: {System.IO.Directory.GetCurrentDirectory()}");
    }
    
    [ContextMenu("Quick Fix - Disable Trainer")]
    public void QuickFixDisableTrainer()
    {
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null)
        {
            trainer.autoStartTraining = false;
            trainer.gameObject.SetActive(false);
            Debug.Log("🔒 QUICK FIX: Trainer disabled to protect Q-table");
        }
    }
    
    [ContextMenu("Quick Fix - Copy Q-Table to StreamingAssets")]
    public void QuickFixCopyQTable()
    {
        string sourcePath = Application.dataPath + "/qtable.json";
        string targetPath = Application.streamingAssetsPath + "/qtable.json";
        
        if (System.IO.File.Exists(sourcePath))
        {
            // Create StreamingAssets directory if it doesn't exist
            string dir = System.IO.Path.GetDirectoryName(targetPath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            
            System.IO.File.Copy(sourcePath, targetPath, true);
            Debug.Log("✅ Q-table copied to StreamingAssets");
        }
        else
        {
            Debug.LogWarning("❌ No source Q-table found in Assets folder");
        }
    }
}
