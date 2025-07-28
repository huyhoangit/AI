using UnityEngine;

/// <summary>
/// Script test nhanh Ollama local model
/// Đặt vào GameObject để test
/// </summary>
public class OllamaTestManager : MonoBehaviour
{
    [Header("Test Settings")]
    public bool autoTestOnStart = false;
    public string testQuestion = "Quoridor là gì? Giải thích ngắn gọn.";
    
    private HybridAIManager aiManager;
    
    void Start()
    {
        // Tìm hoặc tạo HybridAIManager
        aiManager = FindFirstObjectByType<HybridAIManager>();
        if (aiManager == null)
        {
            aiManager = gameObject.AddComponent<HybridAIManager>();
            Debug.Log("🔧 Created HybridAIManager for testing");
        }
        
        if (autoTestOnStart)
        {
            Invoke("TestOllama", 2f); // Đợi 2 giây cho AI manager khởi tạo
        }
    }
    
    [ContextMenu("Test Ollama Local")]
    public void TestOllama()
    {
        if (aiManager == null)
        {
            Debug.LogError("❌ HybridAIManager not found!");
            return;
        }
        
        Debug.Log("🚀 Testing Ollama local model...");
        Debug.Log($"Question: {testQuestion}");
        
        aiManager.GetAIResponse(testQuestion, OnTestResponse);
    }
    
    void OnTestResponse(string response, bool success)
    {
        if (success)
        {
            Debug.Log("✅ OLLAMA TEST SUCCESSFUL!");
            Debug.Log($"Response: {response}");
            Debug.Log("🎉 Local model is working correctly!");
        }
        else
        {
            Debug.LogError("❌ OLLAMA TEST FAILED!");
            Debug.LogError($"Error: {response}");
            Debug.LogError("💡 Check Ollama installation and model download");
            ShowTroubleshootingTips();
        }
    }
    
    void ShowTroubleshootingTips()
    {
        Debug.Log("🛠️ TROUBLESHOOTING TIPS:");
        Debug.Log("1. Check if Ollama is running: ollama serve");
        Debug.Log("2. Download model: ollama pull llama2:7b");
        Debug.Log("3. Test Ollama: curl http://localhost:11434/api/version");
        Debug.Log("4. Check model name in HybridAIManager settings");
    }
    
    [ContextMenu("Test Connection Only")]
    public void TestConnection()
    {
        StartCoroutine(TestOllamaConnection());
    }
    
    System.Collections.IEnumerator TestOllamaConnection()
    {
        Debug.Log("🔗 Testing Ollama connection...");
        
        UnityEngine.Networking.UnityWebRequest request = 
            UnityEngine.Networking.UnityWebRequest.Get("http://localhost:11434/api/version");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Ollama server is running!");
            Debug.Log($"Response: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError("❌ Cannot connect to Ollama server!");
            Debug.LogError($"Error: {request.error}");
            Debug.LogError("💡 Make sure Ollama is installed and running");
        }
        
        request.Dispose();
    }
    
    [ContextMenu("Show AI Status")]
    public void ShowAIStatus()
    {
        if (aiManager != null)
        {
            Debug.Log(aiManager.GetAPIStatus());
        }
        else
        {
            Debug.LogWarning("AI Manager not found");
        }
    }
    
    [ContextMenu("Quick Setup Guide")]
    public void ShowQuickSetup()
    {
        Debug.Log("🚀 QUICK OLLAMA SETUP:");
        Debug.Log("1. Download: https://ollama.ai/download");
        Debug.Log("2. Install Ollama");
        Debug.Log("3. Run: ollama pull llama2:7b");
        Debug.Log("4. Run: ollama serve");
        Debug.Log("5. Click 'Test Ollama Local' in this component");
    }
}
