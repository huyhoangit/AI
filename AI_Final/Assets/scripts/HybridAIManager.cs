using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/// <summary>
/// Hybrid AI API Manager - Hỗ trợ nhiều API miễn phí
/// Tự động chuyển đổi giữa các dịch vụ khi cần
/// </summary>
public class HybridAIManager : MonoBehaviour
{
    [Header("API Settings")]
    public string primaryAPI = "python";
    public string fallbackAPI = "ollama";
    public float timeoutSeconds = 10f;
    
    [Header("Python Local AI")]
    public string pythonAIURL = "http://localhost:5000/chat";
    public bool usePythonAI = true;
    
    [Header("Ollama (Local AI)")]
    public string ollamaURL = "http://localhost:11434/api/generate";
    public string ollamaModel = "llama2";
    
    [Header("HuggingFace")]
    public string huggingfaceModel = "microsoft/DialoGPT-medium";
    public string huggingfaceToken = ""; // Optional
    
    [Header("Groq (Free)")]
    public string groqAPIKey = ""; // Free tier available
    public string groqModel = "llama2-70b-4096";
    
    [Header("Game Context")]
    public bool includeGameContext = true;
    public string gameContext = "Bạn là AI hỗ trợ game Quoridor chuyên nghiệp. Hãy trả lời ngắn gọn, chính xác và hữu ích về mọi khía cạnh của game này.";
    
    private List<string> apiHistory = new List<string>();
    private Dictionary<string, int> apiUsage = new Dictionary<string, int>();
    
    public delegate void ResponseCallback(string response, bool success);
    
    void Start()
    {
        InitializeAPIs();
    }
    
    void InitializeAPIs()
    {
        apiUsage["python"] = 0;
        apiUsage["ollama"] = 0;
        apiUsage["huggingface"] = 0;
        apiUsage["groq"] = 0;
        
        Debug.Log("🔧 Hybrid AI Manager initialized");
        Debug.Log($"Primary: {primaryAPI}, Fallback: {fallbackAPI}");
    }
    
    public void GetAIResponse(string userMessage, ResponseCallback callback)
    {
        StartCoroutine(ProcessAIRequest(userMessage, callback));
    }
    
    IEnumerator ProcessAIRequest(string userMessage, ResponseCallback callback)
    {
        string enhancedMessage = includeGameContext ? 
            $"{gameContext}\n\nCâu hỏi: {userMessage}" : userMessage;
        
        // Try primary API first
        yield return StartCoroutine(TryAPI(primaryAPI, enhancedMessage, (response, success) => {
            if (success)
            {
                callback?.Invoke(response, true);
                return;
            }
            
            // Try fallback API
            StartCoroutine(TryAPI(fallbackAPI, enhancedMessage, callback));
        }));
    }
    
    IEnumerator TryAPI(string apiName, string message, ResponseCallback callback)
    {
        Debug.Log($"🔄 Trying {apiName} API...");
        apiUsage[apiName]++;
        
        UnityWebRequest request = null;
        
        switch (apiName.ToLower())
        {
            case "python":
                request = CreatePythonAIRequest(message);
                break;
            case "ollama":
                request = CreateOllamaRequest(message);
                break;
            case "huggingface":
                request = CreateHuggingFaceRequest(message);
                break;
            case "groq":
                request = CreateGroqRequest(message);
                break;
            default:
                callback?.Invoke("API không được hỗ trợ", false);
                yield break;
        }
        
        if (request == null)
        {
            callback?.Invoke("Không thể tạo request", false);
            yield break;
        }
        
        // Set timeout
        request.timeout = Mathf.RoundToInt(timeoutSeconds);
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = ParseAPIResponse(apiName, request.downloadHandler.text);
            if (!string.IsNullOrEmpty(response))
            {
                Debug.Log($"✅ {apiName} API success");
                callback?.Invoke(response, true);
            }
            else
            {
                Debug.LogWarning($"⚠️ {apiName} returned empty response");
                callback?.Invoke("Phản hồi trống từ AI", false);
            }
        }
        else
        {
            Debug.LogError($"❌ {apiName} API failed: {request.error}");
            callback?.Invoke($"Lỗi {apiName}: {request.error}", false);
        }
        
        request.Dispose();
    }
    
    UnityWebRequest CreateOllamaRequest(string message)
    {
        var requestData = new {
            model = ollamaModel,
            prompt = message,
            stream = false
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ollamaURL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        return request;
    }
    
    UnityWebRequest CreateHuggingFaceRequest(string message)
    {
        string url = $"https://api-inference.huggingface.co/models/{huggingfaceModel}";
        
        var requestData = new {
            inputs = message,
            parameters = new {
                max_length = 100,
                temperature = 0.7f
            }
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        if (!string.IsNullOrEmpty(huggingfaceToken))
            request.SetRequestHeader("Authorization", $"Bearer {huggingfaceToken}");
        
        return request;
    }
    
    UnityWebRequest CreateGroqRequest(string message)
    {
        if (string.IsNullOrEmpty(groqAPIKey))
        {
            Debug.LogWarning("Groq API key not set");
            return null;
        }
        
        string url = "https://api.groq.com/openai/v1/chat/completions";
        
        var requestData = new {
            model = groqModel,
            messages = new[] {
                new { role = "user", content = message }
            },
            max_tokens = 150,
            temperature = 0.7f
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {groqAPIKey}");
        
        return request;
    }
    
    UnityWebRequest CreatePythonAIRequest(string message)
    {
        var requestData = new {
            message = message
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(pythonAIURL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        return request;
    }
    
    string ParseAPIResponse(string apiName, string jsonResponse)
    {
        try
        {
            switch (apiName.ToLower())
            {
                case "python":
                    var pythonResponse = JsonUtility.FromJson<PythonAIResponse>(jsonResponse);
                    return pythonResponse.response;
                    
                case "ollama":
                    var ollamaResponse = JsonUtility.FromJson<OllamaResponse>(jsonResponse);
                    return ollamaResponse.response;
                    
                case "huggingface":
                    // HuggingFace returns array format
                    if (jsonResponse.StartsWith("["))
                    {
                        jsonResponse = jsonResponse.Substring(1, jsonResponse.Length - 2);
                    }
                    var hfResponse = JsonUtility.FromJson<HuggingFaceResponse>(jsonResponse);
                    return hfResponse.generated_text;
                    
                case "groq":
                    var groqResponse = JsonUtility.FromJson<GroqResponse>(jsonResponse);
                    return groqResponse.choices[0].message.content;
                    
                default:
                    return "Không thể parse response";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Parse error for {apiName}: {e.Message}");
            return "Lỗi xử lý phản hồi AI";
        }
    }
    
    public string GetAPIStatus()
    {
        StringBuilder status = new StringBuilder();
        status.AppendLine("📊 API Usage Statistics:");
        foreach (var kvp in apiUsage)
        {
            status.AppendLine($"  {kvp.Key}: {kvp.Value} requests");
        }
        return status.ToString();
    }
    
    public void ResetAPICounters()
    {
        foreach (var key in apiUsage.Keys.ToList())
        {
            apiUsage[key] = 0;
        }
        Debug.Log("🔄 API counters reset");
    }
}

// Response data classes
[System.Serializable]
public class PythonAIResponse
{
    public string response;
    public float confidence;
    public string source;
}

[System.Serializable]
public class OllamaResponse
{
    public string response;
    public bool done;
}

[System.Serializable]
public class HuggingFaceResponse
{
    public string generated_text;
}

[System.Serializable]
public class GroqResponse
{
    public GroqChoice[] choices;
}

[System.Serializable]
public class GroqChoice
{
    public GroqMessage message;
}

[System.Serializable]
public class GroqMessage
{
    public string content;
}
