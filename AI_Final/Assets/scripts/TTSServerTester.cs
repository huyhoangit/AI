using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;

/// <summary>
/// Simple TTS Server Tester
/// Helps debug TTS server connection issues
/// </summary>
public class TTSServerTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private string ttsUrl = "http://localhost:8001/tts";
    [SerializeField] private string testMessage = "Test message";
    [SerializeField] private string language = "vi";
    
    [ContextMenu("Test TTS Server")]
    public void TestTTSServer()
    {
        StartCoroutine(TestTTSCoroutine());
    }
    
    IEnumerator TestTTSCoroutine()
    {
        Debug.Log($"🔍 Testing TTS server: {ttsUrl}");
        Debug.Log($"📝 Test message: {testMessage}");
        Debug.Log($"🌐 Language: {language}");
        
        // Create JSON payload
        string jsonPayload = $"{{\"text\":\"{testMessage}\",\"lang\":\"{language}\"}}";
        Debug.Log($"📦 JSON payload: {jsonPayload}");
        
        using (UnityWebRequest request = new UnityWebRequest(ttsUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            Debug.Log("📤 Sending request...");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Request successful!");
                Debug.Log($"📊 Response code: {request.responseCode}");
                Debug.Log($"📏 Response size: {request.downloadHandler.data?.Length ?? 0} bytes");
                
                if (request.downloadHandler.data != null && request.downloadHandler.data.Length > 0)
                {
                    Debug.Log($"🎵 Audio data received: {request.downloadHandler.data.Length} bytes");
                    
                    // Try to save audio file for inspection
                    string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "test_tts_response.wav");
                    System.IO.File.WriteAllBytes(tempPath, request.downloadHandler.data);
                    Debug.Log($"💾 Audio saved to: {tempPath}");
                }
                else
                {
                    Debug.LogWarning("⚠️ No audio data in response");
                    Debug.Log($"📄 Response text: {request.downloadHandler.text}");
                }
            }
            else
            {
                Debug.LogError($"❌ Request failed: {request.error}");
                Debug.LogError($"📊 Response code: {request.responseCode}");
                Debug.LogError($"📄 Response text: {request.downloadHandler.text}");
            }
        }
    }
    
    [ContextMenu("Test Different URLs")]
    public void TestDifferentUrls()
    {
        StartCoroutine(TestMultipleUrls());
    }
    
    IEnumerator TestMultipleUrls()
    {
        string[] testUrls = {
            "http://localhost:8001/tts",
            "http://127.0.0.1:8001/tts",
            "http://localhost:8001/",
            "http://localhost:8001"
        };
        
        foreach (string url in testUrls)
        {
            Debug.Log($"🔍 Testing URL: {url}");
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✅ {url} - Server is reachable");
                }
                else
                {
                    Debug.LogWarning($"⚠️ {url} - {request.error}");
                }
            }
            
            yield return new WaitForSeconds(1f);
        }
    }
    
    [ContextMenu("Test Simple GET Request")]
    public void TestSimpleGet()
    {
        StartCoroutine(TestSimpleGetCoroutine());
    }
    
    IEnumerator TestSimpleGetCoroutine()
    {
        Debug.Log("🔍 Testing simple GET request to TTS server...");
        
        using (UnityWebRequest request = UnityWebRequest.Get(ttsUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ GET request successful to {ttsUrl}");
                Debug.Log($"📄 Response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ GET request failed: {request.error}");
            }
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor menu for TTS server testing
/// </summary>
public static class TTSServerTesterEditor
{
    [MenuItem("Quoridor/🔍 Test TTS Server Connection")]
    public static void TestTTSServerConnection()
    {
        Debug.Log("🔍 Testing TTS server connection from menu...");
        
        // Create temporary component to execute test
        GameObject tempGO = new GameObject("TempTTSServerTester");
        TTSServerTester component = tempGO.AddComponent<TTSServerTester>();
        
        component.TestTTSServer();
        
        // Clean up after a delay
        EditorApplication.delayCall += () =>
        {
            if (tempGO != null)
            {
                UnityEngine.Object.DestroyImmediate(tempGO);
            }
        };
    }
    
    [MenuItem("Quoridor/🔍 Test Multiple TTS URLs")]
    public static void TestMultipleTTSUrls()
    {
        Debug.Log("🔍 Testing multiple TTS URLs from menu...");
        
        // Create temporary component to execute test
        GameObject tempGO = new GameObject("TempTTSServerTester");
        TTSServerTester component = tempGO.AddComponent<TTSServerTester>();
        
        component.TestDifferentUrls();
        
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