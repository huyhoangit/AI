using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor API Migration Tool - Runs in Editor mode without needing Play
/// </summary>
public class EditorAPIMigrationTool
{
    [MenuItem("Quoridor/Fix Deprecated APIs")]
    public static void FixAllDeprecatedAPIs()
    {
        Debug.Log("🔧 FIXING ALL DEPRECATED API USAGE (EDITOR MODE)");
        
        FixLeanTweenTestingUnitTests();
        FixQuoridorAIAPIs();
        ScanForDeprecatedAPIs();
        
        Debug.Log("✅ All deprecated APIs fixed!");
        
        // Refresh Unity to apply changes
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Quoridor/Scan Deprecated APIs")]
    public static void ScanForDeprecatedAPIs()
    {
        Debug.Log("🔍 SCANNING FOR DEPRECATED API USAGE");
        
        string[] scriptPaths = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);
        
        int deprecatedCount = 0;
        
        foreach (string path in scriptPaths)
        {
            string content = File.ReadAllText(path);
            
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
    
    static void FixLeanTweenTestingUnitTests()
    {
        Debug.Log("🔧 Fixing LeanTween TestingUnitTests deprecated APIs...");
        
        string filePath = "Assets/LeanTween/Examples/Scripts/TestingUnitTests.cs";
        
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            
            // Replace deprecated FindObjectsOfType - fix the replacement logic
            if (content.Contains("FindObjectsOfType(typeof("))
            {
                // More specific replacement for LeanTween
                content = content.Replace("FindObjectsOfType(typeof(", "FindObjectsByType(typeof(");
                content = content.Replace("FindObjectsOfType(type", "FindObjectsByType(type, FindObjectsSortMode.None");
                
                File.WriteAllText(filePath, content);
                Debug.Log("✅ Fixed LeanTween deprecated API");
            }
            else
            {
                Debug.Log("ℹ️ LeanTween file already uses new API or no deprecated usage found");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ LeanTween TestingUnitTests.cs not found");
        }
    }
    
    static void FixQuoridorAIAPIs()
    {
        Debug.Log("🔧 Checking QuoridorAI for deprecated APIs...");
        
        string filePath = "Assets/scripts/QuoridorAI.cs";
        
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            string originalContent = content;
            
            // Fix deprecated FindObjectsOfType usage (but keep existing FindObjectsByType)
            if (content.Contains("FindObjectsOfType<") && !content.Contains("FindObjectsByType<"))
            {
                content = content.Replace("FindObjectsOfType<", "FindObjectsByType<");
                content = content.Replace("FindObjectOfType<", "FindFirstObjectByType<");
            }
            
            // Add missing FindObjectsSortMode.None parameter where needed
            if (content.Contains("FindObjectsByType<") && !content.Contains("FindObjectsSortMode"))
            {
                // Use regex for more precise replacement
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    @"FindObjectsByType<(\w+)>\(\)",
                    "FindObjectsByType<$1>(FindObjectsSortMode.None)"
                );
            }
            
            if (content != originalContent)
            {
                File.WriteAllText(filePath, content);
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
    
    [MenuItem("Quoridor/Auto Fix All Project APIs")]
    public static void AutoFixAllProjectAPIs()
    {
        Debug.Log("🚀 AUTO-FIXING ALL PROJECT DEPRECATED APIS");
        
        string[] scriptPaths = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);
        
        int fixedCount = 0;
        
        foreach (string path in scriptPaths)
        {
            try
            {
                string content = File.ReadAllText(path);
                string originalContent = content;
                
                // Fix FindObjectsOfType
                content = content.Replace("FindObjectsOfType<", "FindObjectsByType<");
                content = content.Replace("FindObjectOfType<", "FindFirstObjectByType<");
                
                // Add FindObjectsSortMode.None parameter using regex
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    @"FindObjectsByType<(\w+)>\(\)",
                    "FindObjectsByType<$1>(FindObjectsSortMode.None)"
                );
                
                if (content != originalContent)
                {
                    File.WriteAllText(path, content);
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
        AssetDatabase.Refresh();
    }
}
