using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

/// <summary>
/// Free AI API Manager - Hỗ trợ nhiều API miễn phí
/// </summary>
public class FreeAIAPIManager : MonoBehaviour
{
    [Header("API Configuration")]
    public APIProvider currentProvider = APIProvider.Ollama;
    
    [Header("Ollama Settings (Local - Miễn phí)")]
    public string ollamaURL = "http://localhost:11434/api/generate";
    public string ollamaModel = "llama2";
    
    [Header("Hugging Face Settings")]
    public string huggingFaceKey = ""; // Đăng ký miễn phí tại huggingface.co
    public string hfModel = "microsoft/DialoGPT-medium";
    
    [Header("Cohere Settings")]
    public string cohereKey = ""; // Free tier: cohere.ai
    
    [Header("OpenRouter Settings (Có free models)")]
    public string openRouterKey = ""; // openrouter.ai
    public string openRouterModel = "mistralai/mistral-7b-instruct:free";
    
    public enum APIProvider
    {
        Ollama,          // Local, hoàn toàn miễn phí
        HuggingFace,     // Miễn phí với limit
        Cohere,          // Free tier
        OpenRouter       // Có free models
    }
    
    [System.Serializable]
    public class APIResponse
    {
        public string text;
        public bool success;
        public string error;
    }
    
    /// <summary>
    /// Gửi tin nhắn đến AI và nhận phản hồi
    /// </summary>
    public void SendMessage(string message, Action<APIResponse> callback)
    {
        StartCoroutine(ProcessMessage(message, callback));
    }
    
    IEnumerator ProcessMessage(string message, Action<APIResponse> callback)
    {
        APIResponse response = new APIResponse();
        
        switch (currentProvider)
        {
            case APIProvider.Ollama:
                yield return StartCoroutine(CallOllama(message, response));
                break;
            case APIProvider.HuggingFace:
                yield return StartCoroutine(CallHuggingFace(message, response));
                break;
            case APIProvider.Cohere:
                yield return StartCoroutine(CallCohere(message, response));
                break;
            case APIProvider.OpenRouter:
                yield return StartCoroutine(CallOpenRouter(message, response));
                break;
        }
        
        callback?.Invoke(response);
    }
    
    /// <summary>
    /// Ollama - Local AI (Hoàn toàn miễn phí)
    /// Cài đặt: Download Ollama -> ollama pull llama2 -> ollama serve
    /// </summary>
    IEnumerator CallOllama(string message, APIResponse response)
    {
        var requestData = new
        {
            model = ollamaModel,
            prompt = BuildGamePrompt(message),
            stream = false,
            options = new
            {
                temperature = 0.7f,
                num_predict = 100
            }
        };
        
        string json = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(ollamaURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var ollamaResponse = JsonUtility.FromJson<OllamaResponseData>(request.downloadHandler.text);
                    response.text = ollamaResponse.response;
                    response.success = true;
                }
                catch (Exception e)
                {
                    response.success = false;
                    response.error = "Parse error: " + e.Message;
                    response.text = "Không thể xử lý phản hồi từ Ollama.";
                }
            }
            else
            {
                response.success = false;
                response.error = request.error;
                response.text = "Không thể kết nối Ollama. Hãy chắc chắn Ollama đang chạy!";
            }
        }
    }
    
    /// <summary>
    /// Hugging Face - Miễn phí với limit (30k characters/month)
    /// Đăng ký: huggingface.co/settings/tokens
    /// </summary>
    IEnumerator CallHuggingFace(string message, APIResponse response)
    {
        if (string.IsNullOrEmpty(huggingFaceKey))
        {
            response.success = false;
            response.text = "Cần Hugging Face API key để sử dụng.";
            yield break;
        }
        
        string url = $"https://api-inference.huggingface.co/models/{hfModel}";
        
        var requestData = new
        {
            inputs = BuildGamePrompt(message),
            parameters = new
            {
                max_new_tokens = 100,
                temperature = 0.7f,
                return_full_text = false
            }
        };
        
        string json = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {huggingFaceKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // HF response parsing would go here
                    response.text = "Hugging Face API response (cần implement parser)";
                    response.success = true;
                }
                catch (Exception e)
                {
                    response.success = false;
                    response.error = e.Message;
                    response.text = "Lỗi xử lý phản hồi Hugging Face.";
                }
            }
            else
            {
                response.success = false;
                response.error = request.error;
                response.text = "Lỗi kết nối Hugging Face API.";
            }
        }
    }
    
    /// <summary>
    /// Cohere - Free tier (100 calls/month)
    /// Đăng ký: cohere.ai
    /// </summary>
    IEnumerator CallCohere(string message, APIResponse response)
    {
        if (string.IsNullOrEmpty(cohereKey))
        {
            response.success = false;
            response.text = "Cần Cohere API key để sử dụng.";
            yield break;
        }
        
        string url = "https://api.cohere.ai/v1/generate";
        
        var requestData = new
        {
            model = "command",
            prompt = BuildGamePrompt(message),
            max_tokens = 100,
            temperature = 0.7f,
            k = 0,
            stop_sequences = new string[] { "\n" },
            return_likelihoods = "NONE"
        };
        
        string json = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {cohereKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Cohere response parsing would go here
                    response.text = "Cohere API response (cần implement parser)";
                    response.success = true;
                }
                catch (Exception e)
                {
                    response.success = false;
                    response.error = e.Message;
                    response.text = "Lỗi xử lý phản hồi Cohere.";
                }
            }
            else
            {
                response.success = false;
                response.error = request.error;
                response.text = "Lỗi kết nối Cohere API.";
            }
        }
    }
    
    /// <summary>
    /// OpenRouter - Có free models
    /// Đăng ký: openrouter.ai
    /// </summary>
    IEnumerator CallOpenRouter(string message, APIResponse response)
    {
        if (string.IsNullOrEmpty(openRouterKey))
        {
            response.success = false;
            response.text = "Cần OpenRouter API key để sử dụng.";
            yield break;
        }
        
        string url = "https://openrouter.ai/api/v1/chat/completions";
        
        var requestData = new
        {
            model = openRouterModel,
            messages = new object[]
            {
                new { role = "system", content = "You are a helpful AI assistant for Quoridor game." },
                new { role = "user", content = message }
            },
            max_tokens = 100,
            temperature = 0.7f
        };
        
        string json = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {openRouterKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // OpenRouter response parsing would go here
                    response.text = "OpenRouter API response (cần implement parser)";
                    response.success = true;
                }
                catch (Exception e)
                {
                    response.success = false;
                    response.error = e.Message;
                    response.text = "Lỗi xử lý phản hồi OpenRouter.";
                }
            }
            else
            {
                response.success = false;
                response.error = request.error;
                response.text = "Lỗi kết nối OpenRouter API.";
            }
        }
    }
    
    /// <summary>
    /// Build prompt cho game context
    /// </summary>
    string BuildGamePrompt(string userMessage)
    {
        return $"You are an AI assistant for Quoridor game. Help the user with game rules, strategies, and tips. User message: {userMessage}";
    }
    
    [ContextMenu("Test API Connection")]
    public void TestConnection()
    {
        SendMessage("Hello, can you help me with Quoridor game?", (response) =>
        {
            if (response.success)
            {
                Debug.Log($"✅ API Test Success: {response.text}");
            }
            else
            {
                Debug.LogError($"❌ API Test Failed: {response.error}");
            }
        });
    }
    
    [System.Serializable]
    public class OllamaResponseData
    {
        public string response;
        public bool done;
    }
}

public static class FreeAIHelper
{
    public static void SetupOllama()
    {
        Debug.Log("📋 OLLAMA SETUP:");
        Debug.Log("1. Download: https://ollama.ai");
        Debug.Log("2. Install and run: ollama serve");
        Debug.Log("3. Download model: ollama pull llama2");
        Debug.Log("4. Test: ollama run llama2 'Hello'");
    }
    
    public static void SetupHuggingFace()
    {
        Debug.Log("📋 HUGGING FACE SETUP:");
        Debug.Log("1. Register: huggingface.co");
        Debug.Log("2. Get API key: huggingface.co/settings/tokens");
        Debug.Log("3. Add key to FreeAIAPIManager");
    }
}
