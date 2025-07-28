using UnityEngine;
using UnityEditor;

/// <summary>
/// Auto-setup script for Local TTS integration (using ChatService)
/// Automatically configures TTS components in the scene
/// </summary>
public class LocalTTSAutoSetup : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createIntegrationIfMissing = true;
    [SerializeField] private bool enableVietnameseByDefault = true;
    
    [Header("TTS Server Settings")]
    [SerializeField] private string ttsServerUrl = "http://localhost:8001/tts";
    [SerializeField] private bool checkServerConnection = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupLocalTTS();
        }
    }
    
    [ContextMenu("Setup Local TTS System")]
    public void SetupLocalTTS()
    {
        Debug.Log("🎤 Setting up Local TTS system...");
        
        // 1. Check ChatService
        SetupChatService();
        
        // 2. Setup TTS Integration
        SetupTTSIntegration();
        
        // 3. Configure settings
        ConfigureTTSSettings();
        
        // 4. Test connection
        if (checkServerConnection)
        {
            StartCoroutine(TestServerConnection());
        }
        
        Debug.Log("✅ Local TTS system setup complete!");
    }
    
    void SetupChatService()
    {
        ChatService chatService = FindFirstObjectByType<ChatService>();
        
        if (chatService == null)
        {
            Debug.LogWarning("⚠️ ChatService not found! Please add ChatService component to your scene.");
            Debug.LogWarning("💡 ChatService is required for local TTS to work.");
            return;
        }
        
        Debug.Log("✅ ChatService found and ready");
        
        // Update TTS URL if needed
        if (!string.IsNullOrEmpty(ttsServerUrl) && chatService.ttsUrl != ttsServerUrl)
        {
            chatService.ttsUrl = ttsServerUrl;
            Debug.Log($"🎤 Updated TTS URL to: {ttsServerUrl}");
        }
    }
    
    void SetupTTSIntegration()
    {
        QuoridorLocalTTSIntegration ttsIntegration = FindFirstObjectByType<QuoridorLocalTTSIntegration>();
        
        if (ttsIntegration == null && createIntegrationIfMissing)
        {
            Debug.Log("🎤 Creating QuoridorLocalTTSIntegration...");
            
            GameObject integrationGO = new GameObject("QuoridorLocalTTSIntegration");
            ttsIntegration = integrationGO.AddComponent<QuoridorLocalTTSIntegration>();
            
            // Find and assign ChatService
            ChatService chatService = FindFirstObjectByType<ChatService>();
            if (chatService != null)
            {
                // Use reflection to set the private field
                var chatServiceField = typeof(QuoridorLocalTTSIntegration).GetField("chatService", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (chatServiceField != null)
                {
                    chatServiceField.SetValue(ttsIntegration, chatService);
                }
            }
            
            Debug.Log("✅ QuoridorLocalTTSIntegration created successfully");
        }
        else if (ttsIntegration != null)
        {
            Debug.Log("✅ QuoridorLocalTTSIntegration already exists");
        }
    }
    
    void ConfigureTTSSettings()
    {
        // Configure TTS Integration settings
        QuoridorLocalTTSIntegration ttsIntegration = FindFirstObjectByType<QuoridorLocalTTSIntegration>();
        if (ttsIntegration != null)
        {
            // Use reflection to set private fields
            var vietnameseField = typeof(QuoridorLocalTTSIntegration).GetField("enableVietnameseMessages", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (vietnameseField != null)
            {
                vietnameseField.SetValue(ttsIntegration, enableVietnameseByDefault);
            }
            
            Debug.Log($"🎤 TTS configured with Vietnamese: {(enableVietnameseByDefault ? "ON" : "OFF")}");
        }
    }
    
    System.Collections.IEnumerator TestServerConnection()
    {
        Debug.Log("🔍 Testing TTS server connection...");
        
        ChatService chatService = FindFirstObjectByType<ChatService>();
        if (chatService == null)
        {
            Debug.LogError("❌ ChatService not found for server test");
            yield break;
        }
        
        // Test TTS request
        bool testCompleted = false;
        string testMessage = "Test connection";
        
        chatService.RequestTTS(testMessage, (audioData) =>
        {
            if (audioData != null && audioData.Length > 0)
            {
                Debug.Log($"✅ TTS server connection successful! Received {audioData.Length} bytes");
            }
            else
            {
                Debug.LogWarning("⚠️ TTS server connection failed - no audio data received");
            }
            testCompleted = true;
        });
        
        // Wait for test to complete
        float timeout = 10f;
        float elapsed = 0f;
        
        while (!testCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!testCompleted)
        {
            Debug.LogWarning("⚠️ TTS server connection test timed out");
        }
    }
    
    [ContextMenu("Test Local TTS Setup")]
    public void TestLocalTTSSetup()
    {
        Debug.Log("🎤 Testing Local TTS setup...");
        
        ChatService chatService = FindFirstObjectByType<ChatService>();
        QuoridorLocalTTSIntegration ttsIntegration = FindFirstObjectByType<QuoridorLocalTTSIntegration>();
        
        if (chatService != null)
        {
            Debug.Log("✅ ChatService found");
            Debug.Log($"   TTS URL: {chatService.ttsUrl}");
            Debug.Log($"   Current Language: {chatService.currentLang}");
        }
        else
        {
            Debug.LogError("❌ ChatService not found");
        }
        
        if (ttsIntegration != null)
        {
            Debug.Log("✅ QuoridorLocalTTSIntegration found");
            ttsIntegration.TestLocalTTS();
        }
        else
        {
            Debug.LogError("❌ QuoridorLocalTTSIntegration not found");
        }
    }
    
    [ContextMenu("Check Local TTS Status")]
    public void CheckLocalTTSStatus()
    {
        Debug.Log("🔍 Checking Local TTS system status...");
        
        ChatService chatService = FindFirstObjectByType<ChatService>();
        QuoridorLocalTTSIntegration ttsIntegration = FindFirstObjectByType<QuoridorLocalTTSIntegration>();
        
        if (chatService != null)
        {
            Debug.Log("✅ ChatService: OK");
            Debug.Log($"   TTS URL: {chatService.ttsUrl}");
            Debug.Log($"   STT URL: {chatService.sttUrl}");
            Debug.Log($"   Current Language: {chatService.currentLang}");
        }
        else
        {
            Debug.LogWarning("⚠️ ChatService: Missing");
        }
        
        if (ttsIntegration != null)
        {
            Debug.Log("✅ QuoridorLocalTTSIntegration: OK");
        }
        else
        {
            Debug.LogWarning("⚠️ QuoridorLocalTTSIntegration: Missing");
        }
        
        // Check AI components
        QuoridorAI[] aiComponents = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        Debug.Log($"🤖 Found {aiComponents.Length} AI components");
        
        foreach (var ai in aiComponents)
        {
            Debug.Log($"🤖 AI '{ai.name}': Trained={ai.isTrainedModel}, QLearning={ai.useQLearning}");
        }
        
        // Check TTS server status
        Debug.Log($"🌐 TTS Server URL: {ttsServerUrl}");
        Debug.Log($"🔍 Server connection check: {(checkServerConnection ? "Enabled" : "Disabled")}");
    }
    
    [ContextMenu("Start TTS Server Test")]
    public void StartTTSServerTest()
    {
        StartCoroutine(TestServerConnection());
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor menu for Local TTS setup
/// </summary>
public static class LocalTTSAutoSetupEditor
{
    [MenuItem("Quoridor/🎤 Setup Local TTS")]
    public static void SetupLocalTTSSystem()
    {
        Debug.Log("🎤 Setting up Local TTS system from menu...");
        
        // Create temporary component to execute setup
        GameObject tempGO = new GameObject("TempLocalTTSSetup");
        LocalTTSAutoSetup component = tempGO.AddComponent<LocalTTSAutoSetup>();
        
        component.SetupLocalTTS();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
        
        Debug.Log("✅ Local TTS setup complete from menu!");
    }
    
    [MenuItem("Quoridor/🎤 Test Local TTS System")]
    public static void TestLocalTTSSystem()
    {
        Debug.Log("🎤 Testing Local TTS system from menu...");
        
        // Create temporary component to execute test
        GameObject tempGO = new GameObject("TempLocalTTSTest");
        LocalTTSAutoSetup component = tempGO.AddComponent<LocalTTSAutoSetup>();
        
        component.TestLocalTTSSetup();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
    }
    
    [MenuItem("Quoridor/🎤 Check Local TTS Status")]
    public static void CheckLocalTTSStatus()
    {
        Debug.Log("🔍 Checking Local TTS status from menu...");
        
        // Create temporary component to execute check
        GameObject tempGO = new GameObject("TempLocalTTSCheck");
        LocalTTSAutoSetup component = tempGO.AddComponent<LocalTTSAutoSetup>();
        
        component.CheckLocalTTSStatus();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
    }
    
    [MenuItem("Quoridor/🎤 Test TTS Server Connection")]
    public static void TestTTSServerConnection()
    {
        Debug.Log("🔍 Testing TTS server connection from menu...");
        
        // Create temporary component to execute test
        GameObject tempGO = new GameObject("TempTTSServerTest");
        LocalTTSAutoSetup component = tempGO.AddComponent<LocalTTSAutoSetup>();
        
        component.StartTTSServerTest();
        
        // Clean up after a delay
        EditorApplication.delayCall += () =>
        {
            if (tempGO != null)
            {
                UnityEngine.Object.DestroyImmediate(tempGO);
            }
        };
    }
}
#endif 