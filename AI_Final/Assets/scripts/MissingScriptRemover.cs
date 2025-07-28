using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingScriptRemover : MonoBehaviour
{
    [Header("Missing Script Removal")]
    [SerializeField] private bool removeOnStart = false;
    [SerializeField] private bool logRemovedScripts = true;
    
    [Header("Manual Controls")]
    [SerializeField] private bool removeFromAllObjects = false;
    [SerializeField] private bool removeFromSelectedObjects = false;
    
    private int totalRemoved = 0;
    private List<string> removedScripts = new List<string>();

    void Start()
    {
        if (removeOnStart)
        {
            RemoveMissingScriptsFromAllObjects();
        }
    }

    void Update()
    {
        // Manual controls through Inspector
        if (removeFromAllObjects)
        {
            removeFromAllObjects = false;
            RemoveMissingScriptsFromAllObjects();
        }
        
        if (removeFromSelectedObjects)
        {
            removeFromSelectedObjects = false;
            RemoveMissingScriptsFromSelectedObjects();
        }
    }

    /// <summary>
    /// Removes missing script components from all GameObjects in the scene
    /// </summary>
    public void RemoveMissingScriptsFromAllObjects()
    {
        totalRemoved = 0;
        removedScripts.Clear();
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            int removed = RemoveMissingScriptsFromObject(obj);
            if (removed > 0)
            {
                totalRemoved += removed;
                removedScripts.Add($"{obj.name}: {removed} scripts");
            }
        }
        
        LogResults("All Objects");
    }

    /// <summary>
    /// Removes missing script components from selected GameObjects
    /// </summary>
    public void RemoveMissingScriptsFromSelectedObjects()
    {
        totalRemoved = 0;
        removedScripts.Clear();
        
        GameObject[] selectedObjects = Selection.gameObjects;
        
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("⚠️ No objects selected! Please select objects first.");
            return;
        }
        
        foreach (GameObject obj in selectedObjects)
        {
            int removed = RemoveMissingScriptsFromObject(obj);
            if (removed > 0)
            {
                totalRemoved += removed;
                removedScripts.Add($"{obj.name}: {removed} scripts");
            }
        }
        
        LogResults("Selected Objects");
    }

    /// <summary>
    /// Removes missing script components from a specific GameObject
    /// </summary>
    public int RemoveMissingScriptsFromObject(GameObject targetObject)
    {
        if (targetObject == null) return 0;
        
        int removedCount = 0;
        Component[] components = targetObject.GetComponents<Component>();
        
        for (int i = components.Length - 1; i >= 0; i--)
        {
            if (components[i] == null)
            {
                // This is a missing script component
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(targetObject);
                removedCount++;
                
                if (logRemovedScripts)
                {
                    Debug.Log($"🗑️ Removed missing script from: {targetObject.name}");
                }
                break; // RemoveMonoBehavioursWithMissingScript removes all missing scripts at once
            }
        }
        
        return removedCount;
    }

    /// <summary>
    /// Counts missing scripts in the scene without removing them
    /// </summary>
    public int CountMissingScripts()
    {
        int count = 0;
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null) count++;
            }
        }
        
        return count;
    }

    /// <summary>
    /// Lists all GameObjects with missing scripts
    /// </summary>
    public void ListObjectsWithMissingScripts()
    {
        Debug.Log("=== OBJECTS WITH MISSING SCRIPTS ===");
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        bool foundAny = false;
        
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            bool hasMissing = false;
            
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    hasMissing = true;
                    break;
                }
            }
            
            if (hasMissing)
            {
                Debug.Log($"⚠️ {obj.name} has missing script(s)");
                foundAny = true;
            }
        }
        
        if (!foundAny)
        {
            Debug.Log("✅ No objects with missing scripts found!");
        }
    }

    private void LogResults(string operation)
    {
        if (totalRemoved > 0)
        {
            Debug.Log($"✅ {operation}: Removed {totalRemoved} missing script(s)");
            
            if (logRemovedScripts && removedScripts.Count > 0)
            {
                Debug.Log("📋 Removed from:");
                foreach (string script in removedScripts)
                {
                    Debug.Log($"   - {script}");
                }
            }
        }
        else
        {
            Debug.Log($"✅ {operation}: No missing scripts found");
        }
    }

    // Context menu methods for easy access
    [ContextMenu("Remove Missing Scripts - All Objects")]
    private void ContextRemoveAll()
    {
        RemoveMissingScriptsFromAllObjects();
    }

    [ContextMenu("Remove Missing Scripts - Selected Objects")]
    private void ContextRemoveSelected()
    {
        RemoveMissingScriptsFromSelectedObjects();
    }

    [ContextMenu("Count Missing Scripts")]
    private void ContextCountMissing()
    {
        int count = CountMissingScripts();
        Debug.Log($"📊 Total missing scripts in scene: {count}");
    }

    [ContextMenu("List Objects With Missing Scripts")]
    private void ContextListMissing()
    {
        ListObjectsWithMissingScripts();
    }

    // Static methods for easy access from other scripts
    public static void RemoveAllMissingScripts()
    {
        MissingScriptRemover remover = FindFirstObjectByType<MissingScriptRemover>();
        if (remover != null)
        {
            remover.RemoveMissingScriptsFromAllObjects();
        }
        else
        {
            Debug.LogError("❌ MissingScriptRemover not found in scene!");
        }
    }

    public static void RemoveMissingScriptsFromGameObject(GameObject obj)
    {
        MissingScriptRemover remover = FindFirstObjectByType<MissingScriptRemover>();
        if (remover != null)
        {
            remover.RemoveMissingScriptsFromObject(obj);
        }
        else
        {
            Debug.LogError("❌ MissingScriptRemover not found in scene!");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MissingScriptRemover))]
public class MissingScriptRemoverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MissingScriptRemover remover = (MissingScriptRemover)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Remove Missing Scripts - All Objects"))
        {
            remover.RemoveMissingScriptsFromAllObjects();
        }
        
        if (GUILayout.Button("Remove Missing Scripts - Selected Objects"))
        {
            remover.RemoveMissingScriptsFromSelectedObjects();
        }
        
        if (GUILayout.Button("Count Missing Scripts"))
        {
            int count = remover.CountMissingScripts();
            EditorUtility.DisplayDialog("Missing Scripts Count", 
                $"Total missing scripts in scene: {count}", "OK");
        }
        
        if (GUILayout.Button("List Objects With Missing Scripts"))
        {
            remover.ListObjectsWithMissingScripts();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This script helps remove missing script components from GameObjects.\n\n" +
            "• Use 'Remove All' to clean the entire scene\n" +
            "• Use 'Remove Selected' to clean only selected objects\n" +
            "• Use 'Count' to see how many missing scripts exist\n" +
            "• Use 'List' to see which objects have missing scripts", 
            MessageType.Info);
    }
}
#endif 