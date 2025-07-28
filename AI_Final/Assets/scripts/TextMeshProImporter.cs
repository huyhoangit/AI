using UnityEngine;
using UnityEditor;

/// <summary>
/// Tự động import TextMeshPro khi cần thiết
/// Chạy script này để setup TextMeshPro package
/// </summary>
public class TextMeshProImporter : MonoBehaviour
{
    [Header("Auto Import Settings")]
    public bool autoImportOnStart = true;
    public bool showImportDialog = true;
    
    void Start()
    {
        if (autoImportOnStart)
        {
            CheckAndImportTextMeshPro();
        }
    }
    
    [ContextMenu("Check TextMeshPro")]
    public void CheckAndImportTextMeshPro()
    {
        Debug.Log("🔍 Checking TextMeshPro package...");
        
        // Kiểm tra xem có TMPro types không
        System.Type tmpTextType = System.Type.GetType("TMPro.TMP_Text");
        
        if (tmpTextType == null)
        {
            Debug.LogWarning("⚠️ TextMeshPro not found! Please import TextMeshPro package.");
            ShowImportInstructions();
        }
        else
        {
            Debug.Log("✅ TextMeshPro is available!");
        }
    }
    
    void ShowImportInstructions()
    {
        Debug.Log("📋 TO IMPORT TEXTMESHPRO:");
        Debug.Log("1. Open Unity Editor");
        Debug.Log("2. Go to Window → Package Manager");
        Debug.Log("3. Search for 'TextMeshPro'");
        Debug.Log("4. Click 'Install' or 'Update'");
        Debug.Log("5. After installation, go to Window → TextMeshPro → Import TMP Essential Resources");
        Debug.Log("6. Restart Unity if needed");
    }
    
    [ContextMenu("Test TMPro Components")]
    public void TestTMProComponents()
    {
        Debug.Log("🧪 Testing TMPro components...");
        
        // Test TMP_Text
        System.Type tmpTextType = System.Type.GetType("TMPro.TMP_Text");
        if (tmpTextType != null)
        {
            Debug.Log("✅ TMP_Text is available");
        }
        else
        {
            Debug.LogError("❌ TMP_Text not found");
        }
        
        // Test TMP_InputField
        System.Type tmpInputType = System.Type.GetType("TMPro.TMP_InputField");
        if (tmpInputType != null)
        {
            Debug.Log("✅ TMP_InputField is available");
        }
        else
        {
            Debug.LogError("❌ TMP_InputField not found");
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor script để tự động import TextMeshPro
/// </summary>
public class TextMeshProEditorImporter : EditorWindow
{
    [MenuItem("Tools/Import TextMeshPro")]
    public static void ShowWindow()
    {
        GetWindow<TextMeshProEditorImporter>("TextMeshPro Importer");
    }
    
    void OnGUI()
    {
        GUILayout.Label("TextMeshPro Package Importer", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Check TextMeshPro Status"))
        {
            CheckTextMeshProStatus();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Open Package Manager (Window → Package Manager)");
        GUILayout.Label("2. Search for 'TextMeshPro'");
        GUILayout.Label("3. Click 'Install' or 'Update'");
        GUILayout.Label("4. Import TMP Essential Resources");
        GUILayout.Label("5. Restart Unity if needed");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Open Package Manager"))
        {
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
        }
        
        if (GUILayout.Button("Import TMP Essential Resources"))
        {
            ImportTMPResources();
        }
    }
    
    void CheckTextMeshProStatus()
    {
        System.Type tmpTextType = System.Type.GetType("TMPro.TMP_Text");
        
        if (tmpTextType != null)
        {
            EditorUtility.DisplayDialog("TextMeshPro Status", "✅ TextMeshPro is properly installed!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("TextMeshPro Status", "❌ TextMeshPro is NOT installed!\n\nPlease install it via Package Manager.", "OK");
        }
    }
    
    void ImportTMPResources()
    {
        try
        {
            // Try to import TMP Essential Resources
            System.Type tmpImporterType = System.Type.GetType("TMPro.TMP_Importer");
            if (tmpImporterType != null)
            {
                var method = tmpImporterType.GetMethod("ImportEssentialResources");
                if (method != null)
                {
                    method.Invoke(null, null);
                    EditorUtility.DisplayDialog("Import Success", "TMP Essential Resources imported successfully!", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Import Failed", "TMP Importer not found. Please install TextMeshPro first.", "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Import Error", "Error importing TMP resources: " + e.Message, "OK");
        }
    }
}
#endif 