using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor Q-Table Synchronizer - Runs in Editor mode without needing Play
/// </summary>
public class EditorQTableSynchronizer
{
    [MenuItem("Quoridor/Setup Q-Table Synchronizer")]
    public static void SetupQTableSynchronizer()
    {
        Debug.Log("🔧 SETTING UP Q-TABLE SYNCHRONIZER (EDITOR MODE)");
        
        // Check if synchronizer already exists in scene
        RuntimeQTableSynchronizer existingSyncer = Object.FindFirstObjectByType<RuntimeQTableSynchronizer>();
        
        if (existingSyncer == null)
        {
            // Create new GameObject for synchronizer
            GameObject syncerObj = new GameObject("RuntimeQTableSynchronizer");
            
            // Add the synchronizer component
            RuntimeQTableSynchronizer syncer = syncerObj.AddComponent<RuntimeQTableSynchronizer>();
            
            // Configure settings
            syncer.autoSyncOnStart = true;
            
            Debug.Log("✅ Runtime Q-Table Synchronizer created and configured");
            
            // Mark scene as dirty so it can be saved
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
        else
        {
            Debug.Log("ℹ️ Runtime Q-Table Synchronizer already exists");
        }
    }
    
    [MenuItem("Quoridor/Quick Fix All AI (Editor)")]
    public static void QuickFixAllAIEditor()
    {
        Debug.Log("⚡ QUICK FIX ALL AI - EDITOR MODE");
        
        // Setup synchronizer first
        SetupQTableSynchronizer();
        
        // If in Play mode, run the actual synchronizer
        if (Application.isPlaying)
        {
            RuntimeQTableSynchronizer syncer = Object.FindFirstObjectByType<RuntimeQTableSynchronizer>();
            
            if (syncer != null)
            {
                syncer.DebugAllAIStatus();
                syncer.EmergencyFixAll();
                syncer.FixGameManagerAI();
                syncer.ForceSyncAllAI();
                
                Debug.Log("✅ Quick fix complete!");
            }
            else
            {
                Debug.LogError("❌ Failed to find synchronizer!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Press Play to run AI synchronization");
            Debug.Log("💡 Synchronizer has been set up and will auto-run when you Press Play");
        }
    }
    
    [MenuItem("Quoridor/Debug All AI Components (Editor)")]
    public static void DebugAllAIComponentsEditor()
    {
        Debug.Log("🔍 DEBUGGING ALL AI COMPONENTS (EDITOR MODE)");
        
        QuoridorAI[] allAIs = Object.FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        if (allAIs.Length == 0)
        {
            Debug.LogWarning("⚠️ No QuoridorAI components found in scene!");
            return;
        }
        
        for (int i = 0; i < allAIs.Length; i++)
        {
            var ai = allAIs[i];
            
            Debug.Log($"\n🤖 AI[{i}]: {ai.gameObject.name}");
            Debug.Log($"   🎯 Use Q-Learning: {ai.useQLearning}");
            Debug.Log($"   🎓 Is Trained Model: {ai.isTrainedModel}");
            Debug.Log($"   💾 Allow Q-Table Saving: {ai.allowQTableSaving}");
            Debug.Log($"   📂 Q-Table Path: {ai.qTablePath}");
            
            // Check if Q-table file exists
            string fullPath = ai.GetType().GetMethod("GetQTablePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(ai, null)?.ToString();
            
            if (!string.IsNullOrEmpty(fullPath))
            {
                bool fileExists = System.IO.File.Exists(fullPath);
                Debug.Log($"   📁 Q-Table File: {(fileExists ? "✅ EXISTS" : "❌ NOT FOUND")}");
                
                if (fileExists)
                {
                    var fileInfo = new System.IO.FileInfo(fullPath);
                    Debug.Log($"   📊 File Size: {fileInfo.Length / 1024f:F1} KB");
                    Debug.Log($"   🕒 Modified: {fileInfo.LastWriteTime}");
                }
            }
        }
        
        Debug.Log("\n🔍 Debug complete!");
    }
    
    [MenuItem("Quoridor/Check Q-Table Files")]
    public static void CheckQTableFiles()
    {
        Debug.Log("📁 CHECKING Q-TABLE FILES");
        
        string[] searchPaths = {
            System.IO.Path.Combine(Application.streamingAssetsPath, "qtable.json"),
            System.IO.Path.Combine(Application.dataPath, "StreamingAssets", "qtable.json"),
            System.IO.Path.Combine(Application.dataPath, "qtable.json"),
            System.IO.Path.Combine(Application.persistentDataPath, "qtable.json")
        };
        
        bool foundAny = false;
        
        foreach (string path in searchPaths)
        {
            if (System.IO.File.Exists(path))
            {
                var fileInfo = new System.IO.FileInfo(path);
                Debug.Log($"✅ Found: {path}");
                Debug.Log($"   📊 Size: {fileInfo.Length / 1024f:F1} KB");
                Debug.Log($"   🕒 Modified: {fileInfo.LastWriteTime}");
                foundAny = true;
            }
            else
            {
                Debug.Log($"❌ Not found: {path}");
            }
        }
        
        if (!foundAny)
        {
            Debug.LogWarning("⚠️ No Q-table files found! Make sure to train the AI first.");
        }
        else
        {
            Debug.Log("✅ Q-table files detected!");
        }
    }
    
    [MenuItem("Quoridor/Force Reload Q-Tables (Editor)")]
    public static void ForceReloadQTablesEditor()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ Q-Table reload only works in Play mode");
            Debug.Log("💡 Press Play first, then use this command");
            return;
        }
        
        Debug.Log("🔄 FORCE RELOADING ALL Q-TABLES");
        
        QuoridorAI[] allAIs = Object.FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        foreach (var ai in allAIs)
        {
            Debug.Log($"🔄 Reloading Q-table for: {ai.gameObject.name}");
            ai.ReloadQTable();
        }
        
        Debug.Log("✅ All Q-tables reloaded!");
    }
}
