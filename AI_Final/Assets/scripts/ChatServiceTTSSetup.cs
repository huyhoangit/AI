using UnityEngine;
using UnityEditor;

/// <summary>
/// Setup script for enhanced ChatService with multiple TTS options
/// </summary>
public class ChatServiceTTSSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private ChatService.TTSType defaultTTSType = ChatService.TTSType.LocalServer;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupChatServiceTTS();
        }
    }
    
    [ContextMenu("Setup ChatService TTS")]
    public void SetupChatServiceTTS()
    {
        Debug.Log("🎤 Setting up ChatService TTS...");
        
        ChatService chatService = FindFirstObjectByType<ChatService>();
        if (chatService == null)
        {
            Debug.LogWarning("⚠️ ChatService not found! Please add ChatService component to your scene.");
            return;
        }
        
        // Set default TTS type
        var ttsTypeField = typeof(ChatService).GetField("ttsType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (ttsTypeField != null)
        {
            ttsTypeField.SetValue(chatService, defaultTTSType);
        }
        
        Debug.Log($"✅ ChatService TTS configured with type: {defaultTTSType}");
        ShowSetupInstructions(defaultTTSType);
    }
    
    void ShowSetupInstructions(ChatService.TTSType ttsType)
    {
        Debug.Log($"📋 Setup instructions for {ttsType}:");
        
        switch (ttsType)
        {
            case ChatService.TTSType.LocalServer:
                Debug.Log("1. Run SimpleTTSServer.py: python SimpleTTSServer.py");
                Debug.Log("2. Ensure server is running on http://localhost:8001");
                Debug.Log("3. Test connection using TTSServerTester");
                break;
                
            case ChatService.TTSType.GoogleTTS:
                Debug.Log("1. Get Google Cloud API Key from: https://console.cloud.google.com/");
                Debug.Log("2. Enable Text-to-Speech API");
                Debug.Log("3. Add API key to ChatService component (Google Api Key field)");
                break;
                
            case ChatService.TTSType.AzureSpeech:
                Debug.Log("1. Get Azure Speech Service key from: https://portal.azure.com/");
                Debug.Log("2. Create Speech Service resource");
                Debug.Log("3. Add subscription key and region to ChatService component");
                break;
                
            case ChatService.TTSType.OpenAITTS:
                Debug.Log("1. Get OpenAI API key from: https://platform.openai.com/");
                Debug.Log("2. Add API key to ChatService component (OpenAI Api Key field)");
                break;
                
            case ChatService.TTSType.ElevenLabs:
                Debug.Log("1. Get ElevenLabs API key from: https://elevenlabs.io/");
                Debug.Log("2. Add API key to ChatService component (ElevenLabs Api Key field)");
                break;
        }
    }
    
    [ContextMenu("Switch to Local TTS")]
    public void SwitchToLocalTTS()
    {
        SwitchTTSType(ChatService.TTSType.LocalServer);
    }
    
    [ContextMenu("Switch to Google TTS")]
    public void SwitchToGoogleTTS()
    {
        SwitchTTSType(ChatService.TTSType.GoogleTTS);
    }
    
    [ContextMenu("Switch to Azure Speech")]
    public void SwitchToAzureSpeech()
    {
        SwitchTTSType(ChatService.TTSType.AzureSpeech);
    }
    
    [ContextMenu("Switch to OpenAI TTS")]
    public void SwitchToOpenAITTS()
    {
        SwitchTTSType(ChatService.TTSType.OpenAITTS);
    }
    
    [ContextMenu("Switch to ElevenLabs")]
    public void SwitchToElevenLabs()
    {
        SwitchTTSType(ChatService.TTSType.ElevenLabs);
    }
    
    void SwitchTTSType(ChatService.TTSType ttsType)
    {
        Debug.Log($"🎤 Switching ChatService TTS to: {ttsType}");
        
        ChatService chatService = FindFirstObjectByType<ChatService>();
        if (chatService == null)
        {
            Debug.LogError("❌ ChatService not found!");
            return;
        }
        
        var ttsTypeField = typeof(ChatService).GetField("ttsType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (ttsTypeField != null)
        {
            ttsTypeField.SetValue(chatService, ttsType);
        }
        
        Debug.Log($"✅ ChatService TTS switched to: {ttsType}");
        ShowSetupInstructions(ttsType);
    }
    
    [ContextMenu("Test ChatService TTS")]
    public void TestChatServiceTTS()
    {
        Debug.Log("🎤 Testing ChatService TTS...");
        
        ChatService chatService = FindFirstObjectByType<ChatService>();
        if (chatService == null)
        {
            Debug.LogError("❌ ChatService not found!");
            return;
        }
        
        // Get current TTS type
        var ttsTypeField = typeof(ChatService).GetField("ttsType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ChatService.TTSType currentType = ChatService.TTSType.LocalServer;
        if (ttsTypeField != null)
        {
            currentType = (ChatService.TTSType)ttsTypeField.GetValue(chatService);
        }
        
        Debug.Log($"🎤 Current TTS type: {currentType}");
        
        // Test TTS
        chatService.RequestTTS("Test ChatService TTS", (audioData) =>
        {
            if (audioData != null && audioData.Length > 0)
            {
                Debug.Log($"✅ TTS test successful! Received {audioData.Length} bytes");
            }
            else
            {
                Debug.LogWarning("⚠️ TTS test failed - no audio data received");
            }
        });
    }
    
    [ContextMenu("Check ChatService Status")]
    public void CheckChatServiceStatus()
    {
        Debug.Log("🔍 Checking ChatService status...");
        
        ChatService chatService = FindFirstObjectByType<ChatService>();
        if (chatService == null)
        {
            Debug.LogWarning("⚠️ ChatService: Missing");
            return;
        }
        
        Debug.Log("✅ ChatService: Found");
        Debug.Log($"   TTS URL: {chatService.ttsUrl}");
        Debug.Log($"   STT URL: {chatService.sttUrl}");
        Debug.Log($"   Current Language: {chatService.currentLang}");
        
        // Get current TTS type
        var ttsTypeField = typeof(ChatService).GetField("ttsType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (ttsTypeField != null)
        {
            var ttsType = ttsTypeField.GetValue(chatService);
            Debug.Log($"   TTS Type: {ttsType}");
        }
        
        // Check API keys
        var googleApiKeyField = typeof(ChatService).GetField("googleApiKey", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (googleApiKeyField != null)
        {
            var googleKey = googleApiKeyField.GetValue(chatService) as string;
            Debug.Log($"   Google API Key: {(string.IsNullOrEmpty(googleKey) ? "Missing" : "Set")}");
        }
        
        var azureKeyField = typeof(ChatService).GetField("azureSubscriptionKey", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (azureKeyField != null)
        {
            var azureKey = azureKeyField.GetValue(chatService) as string;
            Debug.Log($"   Azure API Key: {(string.IsNullOrEmpty(azureKey) ? "Missing" : "Set")}");
        }
        
        var openaiKeyField = typeof(ChatService).GetField("openaiApiKey", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (openaiKeyField != null)
        {
            var openaiKey = openaiKeyField.GetValue(chatService) as string;
            Debug.Log($"   OpenAI API Key: {(string.IsNullOrEmpty(openaiKey) ? "Missing" : "Set")}");
        }
        
        var elevenLabsKeyField = typeof(ChatService).GetField("elevenLabsApiKey", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (elevenLabsKeyField != null)
        {
            var elevenLabsKey = elevenLabsKeyField.GetValue(chatService) as string;
            Debug.Log($"   ElevenLabs API Key: {(string.IsNullOrEmpty(elevenLabsKey) ? "Missing" : "Set")}");
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor menu for ChatService TTS setup
/// </summary>
public static class ChatServiceTTSSetupEditor
{
    [MenuItem("Quoridor/🎤 Setup ChatService TTS")]
    public static void SetupChatServiceTTSSystem()
    {
        Debug.Log("🎤 Setting up ChatService TTS from menu...");
        
        // Create temporary component to execute setup
        GameObject tempGO = new GameObject("TempChatServiceTTSSetup");
        ChatServiceTTSSetup component = tempGO.AddComponent<ChatServiceTTSSetup>();
        
        component.SetupChatServiceTTS();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
        
        Debug.Log("✅ ChatService TTS setup complete from menu!");
    }
    
    [MenuItem("Quoridor/🎤 Switch to Local TTS")]
    public static void SwitchToLocalTTS()
    {
        Debug.Log("🎤 Switching to Local TTS from menu...");
        
        // Create temporary component to execute switch
        GameObject tempGO = new GameObject("TempLocalTTSSwitch");
        ChatServiceTTSSetup component = tempGO.AddComponent<ChatServiceTTSSetup>();
        
        component.SwitchToLocalTTS();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
    }
    
    [MenuItem("Quoridor/🎤 Switch to Google TTS")]
    public static void SwitchToGoogleTTS()
    {
        Debug.Log("🎤 Switching to Google TTS from menu...");
        
        // Create temporary component to execute switch
        GameObject tempGO = new GameObject("TempGoogleTTSSwitch");
        ChatServiceTTSSetup component = tempGO.AddComponent<ChatServiceTTSSetup>();
        
        component.SwitchToGoogleTTS();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
    }
    
    [MenuItem("Quoridor/🎤 Switch to Azure Speech")]
    public static void SwitchToAzureSpeech()
    {
        Debug.Log("🎤 Switching to Azure Speech from menu...");
        
        // Create temporary component to execute switch
        GameObject tempGO = new GameObject("TempAzureSpeechSwitch");
        ChatServiceTTSSetup component = tempGO.AddComponent<ChatServiceTTSSetup>();
        
        component.SwitchToAzureSpeech();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
    }
    
    [MenuItem("Quoridor/🎤 Switch to OpenAI TTS")]
    public static void SwitchToOpenAITTS()
    {
        Debug.Log("🎤 Switching to OpenAI TTS from menu...");
        
        // Create temporary component to execute switch
        GameObject tempGO = new GameObject("TempOpenAITTSSwitch");
        ChatServiceTTSSetup component = tempGO.AddComponent<ChatServiceTTSSetup>();
        
        component.SwitchToOpenAITTS();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
    }
    
    [MenuItem("Quoridor/🎤 Switch to ElevenLabs")]
    public static void SwitchToElevenLabs()
    {
        Debug.Log("🎤 Switching to ElevenLabs from menu...");
        
        // Create temporary component to execute switch
        GameObject tempGO = new GameObject("TempElevenLabsSwitch");
        ChatServiceTTSSetup component = tempGO.AddComponent<ChatServiceTTSSetup>();
        
        component.SwitchToElevenLabs();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
    }
    
    [MenuItem("Quoridor/🎤 Test ChatService TTS")]
    public static void TestChatServiceTTSSystem()
    {
        Debug.Log("🎤 Testing ChatService TTS from menu...");
        
        // Create temporary component to execute test
        GameObject tempGO = new GameObject("TempChatServiceTTSTest");
        ChatServiceTTSSetup component = tempGO.AddComponent<ChatServiceTTSSetup>();
        
        component.TestChatServiceTTS();
        
        // Clean up after a delay
        EditorApplication.delayCall += () =>
        {
            if (tempGO != null)
            {
                UnityEngine.Object.DestroyImmediate(tempGO);
            }
        };
    }
    
    [MenuItem("Quoridor/🎤 Check ChatService Status")]
    public static void CheckChatServiceStatus()
    {
        Debug.Log("🔍 Checking ChatService status from menu...");
        
        // Create temporary component to execute check
        GameObject tempGO = new GameObject("TempChatServiceCheck");
        ChatServiceTTSSetup component = tempGO.AddComponent<ChatServiceTTSSetup>();
        
        component.CheckChatServiceStatus();
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(tempGO);
    }
}
#endif 