using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple TTS Test Script - Tests TTS button functionality
/// </summary>
public class TTSTestScript : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    public bool verboseLogging = true;
    
    void Start()
    {
        if (testOnStart)
        {
            Invoke(nameof(TestTTSButton), 2f);
        }
    }
    
    [ContextMenu("Test TTS Button")]
    public void TestTTSButton()
    {
        Debug.Log("🧪 Testing TTS Button...");
        
        // Find TTS Button
        Button ttsButton = GameObject.Find("TTSButton")?.GetComponent<Button>();
        if (ttsButton == null)
        {
            Debug.LogError("❌ TTS Button not found!");
            return;
        }
        
        Debug.Log("✅ TTS Button found!");
        Debug.Log($"Button interactable: {ttsButton.interactable}");
        Debug.Log($"Button onClick listeners count: {ttsButton.onClick.GetPersistentEventCount()}");
        
        // Test click
        ttsButton.onClick.Invoke();
        Debug.Log("✅ TTS Button click test completed");
    }
    
    [ContextMenu("Test ChatUIController")]
    public void TestChatUIController()
    {
        Debug.Log("🧪 Testing ChatUIController...");
        
        ChatUIController controller = FindFirstObjectByType<ChatUIController>();
        if (controller == null)
        {
            Debug.LogError("❌ ChatUIController not found!");
            return;
        }
        
        Debug.Log("✅ ChatUIController found!");
        Debug.Log($"ChatService: {(controller.chatService != null ? "✅ Connected" : "❌ NULL")}");
        Debug.Log($"TTS Button: {(controller.ttsButton != null ? "✅ Connected" : "❌ NULL")}");
        
        // Test TTS directly
        if (controller.chatService != null)
        {
            Debug.Log("🧪 Testing TTS directly...");
            controller.OnTTSClicked();
        }
    }
    
    [ContextMenu("Test ChatService")]
    public void TestChatService()
    {
        Debug.Log("🧪 Testing ChatService...");
        
        ChatService service = FindFirstObjectByType<ChatService>();
        if (service == null)
        {
            Debug.LogError("❌ ChatService not found!");
            return;
        }
        
        Debug.Log("✅ ChatService found!");
        
        // Test TTS request
        service.RequestTTS("Hello World", (audio) => {
            if (audio != null)
            {
                Debug.Log($"✅ TTS test successful! Received {audio.Length} bytes");
            }
            else
            {
                Debug.LogError("❌ TTS test failed - no audio received");
            }
        });
    }
    
    void OnGUI()
    {
        if (verboseLogging)
        {
            GUILayout.BeginArea(new Rect(10, 330, 300, 150));
            GUILayout.Label("TTS Test Controls:");
            
            if (GUILayout.Button("Test TTS Button"))
            {
                TestTTSButton();
            }
            
            if (GUILayout.Button("Test ChatUIController"))
            {
                TestChatUIController();
            }
            
            if (GUILayout.Button("Test ChatService"))
            {
                TestChatService();
            }
            
            GUILayout.EndArea();
        }
    }
} 