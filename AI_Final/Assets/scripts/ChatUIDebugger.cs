using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tool hỗ trợ debug và sửa lỗi ChatUI
/// </summary>
public class ChatUIDebugger : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    
    [ContextMenu("Fix TMP InputField Issue")]
    public void FixTMPInputFieldIssue()
    {
        Debug.Log("🔧 FIXING TMP_InputField INSPECTOR ISSUE...");
        
        // 1. Kiểm tra TextMeshPro package
        var tmpAssembly = System.Type.GetType("TMPro.TMP_InputField, Unity.TextMeshPro");
        if (tmpAssembly == null)
        {
            Debug.LogError("❌ TextMeshPro package not found! Install it via Window > Package Manager");
            return;
        }
        
        Debug.Log("✅ TextMeshPro package found");
        
        // 2. Tìm ChatUIController
        var chatController = FindFirstObjectByType<ChatUIController>();
        if (chatController == null)
        {
            Debug.LogError("❌ ChatUIController not found in scene!");
            return;
        }
        
        Debug.Log("✅ ChatUIController found");
        
        // 3. Tạo UI nếu chưa có
        var inputField = GameObject.Find("ChatInputField");
        if (inputField == null)
        {
            Debug.LogWarning("⚠️ ChatInputField not found. Creating UI...");
            var uiCreator = FindFirstObjectByType<SimpleChatUICreator>();
            if (uiCreator == null)
            {
                GameObject creatorObj = new GameObject("SimpleChatUICreator_Temp");
                uiCreator = creatorObj.AddComponent<SimpleChatUICreator>();
            }
            uiCreator.CreateMessengerChatUI();
            Debug.Log("✅ UI Created!");
        }
        
        // 4. Force refresh
        chatController.DebugUIComponents();
        
        Debug.Log("🎯 Fix completed! Check Inspector now.");
    }
    
    [ContextMenu("Force Refresh Inspector")]
    public void ForceRefreshInspector()
    {
        // Force Unity to refresh
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
        
        var chatController = FindFirstObjectByType<ChatUIController>();
        if (chatController != null)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(chatController);
            #endif
        }
        
        Debug.Log("🔄 Inspector refreshed!");
    }
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("🔧 ChatUIDebugger ready! Use context menu to fix issues.");
        }
    }
}
