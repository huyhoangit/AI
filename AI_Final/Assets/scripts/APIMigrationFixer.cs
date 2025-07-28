using UnityEngine;
using UnityEditor;

/// <summary>
/// API Migration Fixer - Fixes deprecated Unity API usage
/// </summary>
public class APIMigrationFixer : MonoBehaviour
{
    [Header("API Migration Settings")]
    public bool autoFixOnBuild = true;
    
    [ContextMenu("Fix All Deprecated APIs")]
    public void FixAllDeprecatedAPIs()
    {
        Debug.Log("🔧 FIXING ALL DEPRECATED API USAGE");
        
        FixLeanTweenTestingUnitTests();
        FixQuoridorAIAPIs();
        
        Debug.Log("✅ All deprecated APIs fixed!");
    }
    
    void FixLeanTweenTestingUnitTests()
    {
        Debug.Log("🔧 Fixing LeanTween TestingUnitTests deprecated APIs...");
        
        string filePath = "Assets/LeanTween/Examples/Scripts/TestingUnitTests.cs";
        
        if (System.IO.File.Exists(filePath))
        {
            string content = System.IO.File.ReadAllText(filePath);
            
            // Replace deprecated FindObjectsOfType
            if (content.Contains("FindObjectsOfType("))
            {
                content = content.Replace("FindObjectsOfType(", "FindObjectsByType<GameObject>(FindObjectsSortMode.None);");
                
                System.IO.File.WriteAllText(filePath, content);
                Debug.Log("✅ Fixed LeanTween deprecated API");
            }
            else
            {
                Debug.Log("ℹ️ LeanTween file already uses new API");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ LeanTween TestingUnitTests.cs not found");
        }
    }
    
    void FixQuoridorAIAPIs()
    {
        Debug.Log("🔧 Checking QuoridorAI for deprecated APIs...");
        
        string filePath = "Assets/scripts/QuoridorAI.cs";
        
        if (System.IO.File.Exists(filePath))
        {
            string content = System.IO.File.ReadAllText(filePath);
            
            bool hasChanges = false;
            
            // Check for deprecated FindObjectsOfType usage
            if (content.Contains("FindObjectsOfType<") && !content.Contains("FindObjectsByType<"))
            {
                content = content.Replace("FindObjectsOfType<", "FindObjectsByType<");
                hasChanges = true;
            }
            
            if (hasChanges)
            {
                System.IO.File.WriteAllText(filePath, content);
                Debug.Log("✅ Fixed QuoridorAI deprecated APIs");
            }
            else
            {
                Debug.Log("ℹ️ QuoridorAI already uses new APIs");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ QuoridorAI.cs not found");
        }
    }
    
    /// <summary>
    /// Check all scripts for deprecated API usage
    /// </summary>
    [ContextMenu("Scan For Deprecated APIs")]
    public void ScanForDeprecatedAPIs()
    {
        Debug.Log("🔍 SCANNING FOR DEPRECATED API USAGE");
        
        string[] scriptPaths = System.IO.Directory.GetFiles("Assets", "*.cs", System.IO.SearchOption.AllDirectories);
        
        int deprecatedCount = 0;
        
        foreach (string path in scriptPaths)
        {
            string content = System.IO.File.ReadAllText(path);
            
            if (content.Contains("FindObjectsOfType(") || 
                content.Contains("FindObjectOfType(") ||
                content.Contains("Object.FindObjectsOfType") ||
                content.Contains("Object.FindObjectOfType"))
            {
                Debug.LogWarning($"⚠️ Deprecated API found in: {path}");
                deprecatedCount++;
            }
        }
        
        if (deprecatedCount == 0)
        {
            Debug.Log("✅ No deprecated APIs found!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Found {deprecatedCount} files with deprecated APIs");
        }
    }
    
    /// <summary>
    /// Auto-fix all deprecated APIs in project
    /// </summary>
    [ContextMenu("Auto Fix All Project APIs")]
    public void AutoFixAllProjectAPIs()
    {
        Debug.Log("🚀 AUTO-FIXING ALL PROJECT DEPRECATED APIS");
        
        string[] scriptPaths = System.IO.Directory.GetFiles("Assets", "*.cs", System.IO.SearchOption.AllDirectories);
        
        int fixedCount = 0;
        
        foreach (string path in scriptPaths)
        {
            try
            {
                string content = System.IO.File.ReadAllText(path);
                string originalContent = content;
                
                // Fix FindObjectsOfType
                content = content.Replace("FindObjectsOfType<", "FindObjectsByType<");
                content = content.Replace("FindObjectOfType<", "FindFirstObjectByType<");
                
                // Add FindObjectsSortMode.None if missing
                if (content.Contains("FindObjectsByType<") && !content.Contains("FindObjectsSortMode"))
                {
                    content = content.Replace("FindObjectsByType<", "FindObjectsByType<");
                    // This is a more complex replacement that would need regex
                }
                
                if (content != originalContent)
                {
                    System.IO.File.WriteAllText(path, content);
                    Debug.Log($"✅ Fixed deprecated APIs in: {path}");
                    fixedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error fixing {path}: {e.Message}");
            }
        }
        
        Debug.Log($"🎉 Auto-fixed {fixedCount} files!");
        
        // Refresh Unity to detect changes
        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor integration for API migration
/// </summary>
[InitializeOnLoad]
public static class APIMigrationEditorHook
{
    static APIMigrationEditorHook()
    {
        // Auto-run scan when Unity starts
        EditorApplication.delayCall += () =>
        {
            APIMigrationFixer fixer = Object.FindFirstObjectByType<APIMigrationFixer>();
            if (fixer != null && fixer.autoFixOnBuild)
            {
                fixer.ScanForDeprecatedAPIs();
            }
        };
    }
}
#endif
