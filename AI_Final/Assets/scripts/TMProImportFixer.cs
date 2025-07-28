using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// TMPro Import Fixer - Tự động sửa import statements
/// Script này sẽ tự động fix các lỗi import TMPro
/// </summary>
public class TMProImportFixer : MonoBehaviour
{
    [Header("Fix Settings")]
    public bool autoFixOnStart = false;
    public bool createBackup = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixTMProImports();
        }
    }
    
    [ContextMenu("Fix TMPro Imports")]
    public void FixTMProImports()
    {
        Debug.Log("🔧 Fixing TMPro imports...");
        
        // Kiểm tra xem có TMPro không
        if (!CheckTMProAvailability())
        {
            Debug.LogWarning("❌ TMPro not available - cannot fix imports");
            return;
        }
        
        // Fix các file có lỗi TMPro
        FixSpecificFiles();
        
        Debug.Log("✅ TMPro import fixes completed");
    }
    
    bool CheckTMProAvailability()
    {
        System.Type tmpTextType = System.Type.GetType("TMPro.TMP_Text");
        return tmpTextType != null;
    }
    
    void FixSpecificFiles()
    {
        Debug.Log("📁 Fixing specific files with TMPro issues...");
        
        // Danh sách các file cần fix
        string[] filesToFix = {
            "ChatUIManager.cs",
            "EnhancedAIChatManager.cs", 
            "HybridChatDemo.cs",
            "MessageBubbleCreator.cs",
            "OneClickChatSetup.cs",
            "SimpleChatUICreator.cs"
        };
        
        foreach (string fileName in filesToFix)
        {
            FixFile(fileName);
        }
    }
    
    void FixFile(string fileName)
    {
        string filePath = Path.Combine(Application.dataPath, "scripts", fileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"⚠️ File not found: {fileName}");
            return;
        }
        
        try
        {
            string content = File.ReadAllText(filePath);
            string originalContent = content;
            
            // Fix using statements
            content = FixUsingStatements(content);
            
            // Fix component references
            content = FixComponentReferences(content);
            
            // Chỉ write nếu có thay đổi
            if (content != originalContent)
            {
                if (createBackup)
                {
                    string backupPath = filePath + ".backup";
                    File.WriteAllText(backupPath, originalContent);
                    Debug.Log($"💾 Created backup: {backupPath}");
                }
                
                File.WriteAllText(filePath, content);
                Debug.Log($"✅ Fixed: {fileName}");
            }
            else
            {
                Debug.Log($"ℹ️ No changes needed: {fileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error fixing {fileName}: {e.Message}");
        }
    }
    
    string FixUsingStatements(string content)
    {
        // Thêm using TMPro nếu chưa có
        if (!content.Contains("using TMPro;"))
        {
            // Tìm vị trí để thêm using statement
            int insertIndex = content.IndexOf("using UnityEngine;");
            if (insertIndex != -1)
            {
                int endIndex = content.IndexOf('\n', insertIndex);
                if (endIndex != -1)
                {
                    content = content.Insert(endIndex + 1, "using TMPro;\n");
                }
            }
        }
        
        return content;
    }
    
    string FixComponentReferences(string content)
    {
        // Fix TMP_Text references
        content = Regex.Replace(content, @"TMP_Text\s+(\w+)", "TMP_Text $1");
        
        // Fix TMP_InputField references  
        content = Regex.Replace(content, @"TMP_InputField\s+(\w+)", "TMP_InputField $1");
        
        // Fix TextAlignmentOptions
        content = Regex.Replace(content, @"TextAlignmentOptions\.(\w+)", "TextAlignmentOptions.$1");
        
        return content;
    }
    
    [ContextMenu("Check TMPro Status")]
    public void CheckTMProStatus()
    {
        Debug.Log("🔍 Checking TMPro status...");
        
        bool hasTMPro = CheckTMProAvailability();
        
        if (hasTMPro)
        {
            Debug.Log("✅ TMPro is available");
            Debug.Log("📋 You can now use TMPro components");
        }
        else
        {
            Debug.LogWarning("❌ TMPro not available");
            Debug.Log("📋 Please install TextMeshPro package first");
        }
    }
    
    [ContextMenu("Create Test TMPro Component")]
    public void CreateTestTMProComponent()
    {
        if (!CheckTMProAvailability())
        {
            Debug.LogError("❌ TMPro not available - cannot create test component");
            return;
        }
        
        // Tạo test GameObject với TMPro component
        GameObject testObj = new GameObject("TMProTest");
        
        try
        {
            System.Type tmpTextType = System.Type.GetType("TMPro.TMP_Text");
            var tmpText = testObj.AddComponent(tmpTextType);
            
            // Set text using reflection
            var textProperty = tmpTextType.GetProperty("text");
            if (textProperty != null)
            {
                textProperty.SetValue(tmpText, "TMPro Test - Working!");
            }
            
            Debug.Log("✅ Successfully created TMPro test component");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error creating TMPro component: {e.Message}");
        }
    }
} 