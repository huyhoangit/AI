using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One-Click Chat Setup - Sets up everything in one go
/// Add this to any GameObject and it will create the complete chat system
/// </summary>
public class OneClickChatSetup : MonoBehaviour
{
    [Header("Setup Options")]
    public bool createCompleteFixer = true;
    public bool createChatSystem = true;
    public bool createTTSServer = true;
    public bool createTTSTest = true;
    public bool autoStartTTSServer = true;
    
    [Header("Debug")]
    public bool verboseLogging = true;
    
    void Start()
    {
        Debug.Log("🚀 OneClickChatSetup: Starting complete setup...");
        
        if (createCompleteFixer)
            CreateCompleteFixer();
            
        if (createChatSystem)
            CreateChatSystem();
            
        if (createTTSServer)
            CreateTTSServer();
            
        if (createTTSTest)
            CreateTTSTest();
            
        Debug.Log("✅ OneClickChatSetup: Complete setup finished!");
    }
    
    void CreateCompleteFixer()
    {
        Debug.Log("🔧 Creating CompleteFixer...");
        
        CompleteFixer existingFixer = FindFirstObjectByType<CompleteFixer>();
        if (existingFixer == null)
        {
            GameObject fixerObj = new GameObject("CompleteFixer");
            CompleteFixer fixer = fixerObj.AddComponent<CompleteFixer>();
            fixer.fixMissingScripts = true;
            fixer.fixTTSButton = true;
            fixer.setupChatUI = true;
            fixer.verboseLogging = verboseLogging;
            
            Debug.Log("✅ Created CompleteFixer");
        }
        else
        {
            Debug.Log("✅ CompleteFixer already exists");
        }
    }
    
    void CreateChatSystem()
    {
        Debug.Log("🔧 Creating Chat System...");
        
        SimpleChatSystemSetup existingSetup = FindFirstObjectByType<SimpleChatSystemSetup>();
        if (existingSetup == null)
        {
            GameObject setupObj = new GameObject("SimpleChatSystemSetup");
            SimpleChatSystemSetup setup = setupObj.AddComponent<SimpleChatSystemSetup>();
            
            Debug.Log("✅ Created SimpleChatSystemSetup");
        }
        else
        {
            Debug.Log("✅ SimpleChatSystemSetup already exists");
        }
    }
    
    void CreateTTSServer()
    {
        Debug.Log("🔧 Creating TTS Server...");
        
        TTSServerStarter existingStarter = FindFirstObjectByType<TTSServerStarter>();
        if (existingStarter == null)
        {
            GameObject starterObj = new GameObject("TTSServerStarter");
            TTSServerStarter starter = starterObj.AddComponent<TTSServerStarter>();
            starter.autoStartOnPlay = autoStartTTSServer;
            starter.verboseLogging = verboseLogging;
            
            Debug.Log("✅ Created TTSServerStarter");
        }
        else
        {
            Debug.Log("✅ TTSServerStarter already exists");
        }
    }
    
    void CreateTTSTest()
    {
        Debug.Log("🔧 Creating TTS Test...");
        
        TTSTestScript existingTest = FindFirstObjectByType<TTSTestScript>();
        if (existingTest == null)
        {
            GameObject testObj = new GameObject("TTSTestScript");
            TTSTestScript test = testObj.AddComponent<TTSTestScript>();
            test.testOnStart = true;
            test.verboseLogging = verboseLogging;
            
            Debug.Log("✅ Created TTSTestScript");
        }
        else
        {
            Debug.Log("✅ TTSTestScript already exists");
        }
    }
    
    [ContextMenu("Run Complete Setup")]
    public void RunCompleteSetup()
    {
        Debug.Log("🚀 Running complete setup manually...");
        
        CreateCompleteFixer();
        CreateChatSystem();
        CreateTTSServer();
        CreateTTSTest();
        
        // Wait a bit then test everything
        Invoke(nameof(TestEverything), 3f);
    }
    
    void TestEverything()
    {
        Debug.Log("🧪 Testing everything...");
        
        // Test CompleteFixer
        CompleteFixer fixer = FindFirstObjectByType<CompleteFixer>();
        if (fixer != null)
        {
            Debug.Log("✅ CompleteFixer found");
        }
        else
        {
            Debug.LogError("❌ CompleteFixer not found");
        }
        
        // Test ChatUIController
        ChatUIController chatController = FindFirstObjectByType<ChatUIController>();
        if (chatController != null)
        {
            Debug.Log("✅ ChatUIController found");
        }
        else
        {
            Debug.LogError("❌ ChatUIController not found");
        }
        
        // Test ChatService
        ChatService chatService = FindFirstObjectByType<ChatService>();
        if (chatService != null)
        {
            Debug.Log("✅ ChatService found");
        }
        else
        {
            Debug.LogError("❌ ChatService not found");
        }
        
        // Test TTS Button
        Button ttsButton = GameObject.Find("TTSButton")?.GetComponent<Button>();
        if (ttsButton != null)
        {
            Debug.Log("✅ TTS Button found");
            Debug.Log($"TTS Button listeners: {ttsButton.onClick.GetPersistentEventCount()}");
        }
        else
        {
            Debug.LogError("❌ TTS Button not found");
        }
        
        // Test TTSServerStarter
        TTSServerStarter ttsStarter = FindFirstObjectByType<TTSServerStarter>();
        if (ttsStarter != null)
        {
            Debug.Log("✅ TTSServerStarter found");
            Debug.Log($"TTS Server running: {ttsStarter.IsServerRunning()}");
        }
        else
        {
            Debug.LogError("❌ TTSServerStarter not found");
        }
        
        Debug.Log("✅ Testing completed!");
    }
    
    [ContextMenu("Test TTS Button Click")]
    public void TestTTSButtonClick()
    {
        Button ttsButton = GameObject.Find("TTSButton")?.GetComponent<Button>();
        if (ttsButton != null)
        {
            Debug.Log("🔍 Testing TTS Button click...");
            ttsButton.onClick.Invoke();
            Debug.Log("✅ TTS Button click test completed");
        }
        else
        {
            Debug.LogError("❌ TTS Button not found for testing");
        }
    }
    
    [ContextMenu("Force Fix Missing Scripts")]
    public void ForceFixMissingScripts()
    {
        Debug.Log("🔧 Force fixing missing scripts...");
        
        CompleteFixer fixer = FindFirstObjectByType<CompleteFixer>();
        if (fixer != null)
        {
            fixer.RunCompleteFix();
        }
        else
        {
            Debug.LogError("❌ CompleteFixer not found");
        }
    }
    
    void OnGUI()
    {
        if (verboseLogging)
        {
            GUILayout.BeginArea(new Rect(10, 120, 300, 200));
            GUILayout.Label("OneClickChatSetup Controls:");
            
            if (GUILayout.Button("Run Complete Setup"))
            {
                RunCompleteSetup();
            }
            
            if (GUILayout.Button("Test Everything"))
            {
                TestEverything();
            }
            
            if (GUILayout.Button("Test TTS Button"))
            {
                TestTTSButtonClick();
            }
            
            if (GUILayout.Button("Force Fix Scripts"))
            {
                ForceFixMissingScripts();
            }
            
            GUILayout.EndArea();
        }
    }
}
