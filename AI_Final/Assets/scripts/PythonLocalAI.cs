using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Python Local AI Manager - Kết nối với Python AI server
/// Không cần tải model lớn, chỉ cần Python + Flask
/// </summary>
public class PythonLocalAI : MonoBehaviour
{
    [Header("Python AI Settings")]
    public string pythonServerURL = "http://localhost:5000";
    public float timeoutSeconds = 10f;
    public bool autoStartPython = false;
    
    [Header("Debug")]
    public bool logResponses = true;
    
    public delegate void ResponseCallback(string response, bool success, float confidence);
    
    void Start()
    {
        if (autoStartPython)
        {
            StartPythonServer();
        }
        
        StartCoroutine(CheckServerStatus());
    }
    
    void StartPythonServer()
    {
        try 
        {
            // Try to start Python server
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python",
                Arguments = "simple_quoridor_ai.py",
                WorkingDirectory = Application.dataPath.Replace("/Assets", ""),
                UseShellExecute = false,
                CreateNoWindow = true
            });
            Debug.Log("🚀 Attempting to start Python AI server...");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Could not auto-start Python server: {e.Message}");
            Debug.Log("💡 Please manually run: python simple_quoridor_ai.py");
        }
    }
    
    IEnumerator CheckServerStatus()
    {
        yield return new WaitForSeconds(2f); // Wait for server startup
        
        UnityWebRequest request = UnityWebRequest.Get($"{pythonServerURL}/status");
        request.timeout = 5;
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Python AI server is running!");
            if (logResponses)
            {
                Debug.Log($"Server status: {request.downloadHandler.text}");
            }
        }
        else
        {
            Debug.LogWarning("❌ Python AI server not found!");
            Debug.Log("💡 Please run: python simple_quoridor_ai.py");
            ShowSetupInstructions();
        }
        
        request.Dispose();
    }
    
    void ShowSetupInstructions()
    {
        Debug.Log("🔧 PYTHON AI SETUP INSTRUCTIONS:");
        Debug.Log("1. Install Python (if not installed)");
        Debug.Log("2. Install Flask: pip install flask");
        Debug.Log("3. Run server: python simple_quoridor_ai.py");
        Debug.Log("4. Server should start at http://localhost:5000");
    }
    
    public void GetAIResponse(string userMessage, ResponseCallback callback)
    {
        StartCoroutine(SendChatRequest(userMessage, callback));
    }
    
    IEnumerator SendChatRequest(string message, ResponseCallback callback)
    {
        var requestData = new ChatRequest { message = message };
        string jsonData = JsonUtility.ToJson(requestData);
        
        UnityWebRequest request = new UnityWebRequest($"{pythonServerURL}/chat", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = Mathf.RoundToInt(timeoutSeconds);
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                
                if (logResponses)
                {
                    Debug.Log($"🤖 Python AI Response: {response.response}");
                    Debug.Log($"📊 Confidence: {response.confidence:F2}");
                }
                
                callback?.Invoke(response.response, true, response.confidence);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error parsing Python AI response: {e.Message}");
                callback?.Invoke("Lỗi xử lý phản hồi từ AI", false, 0f);
            }
        }
        else
        {
            Debug.LogError($"❌ Python AI request failed: {request.error}");
            callback?.Invoke($"Lỗi kết nối Python AI: {request.error}", false, 0f);
        }
        
        request.Dispose();
    }
    
    public void TrainAI(string userInput, string aiResponse, string feedback = null)
    {
        StartCoroutine(SendTrainingData(userInput, aiResponse, feedback));
    }
    
    IEnumerator SendTrainingData(string input, string response, string feedback)
    {
        var trainingData = new TrainingRequest 
        { 
            input = input, 
            response = response, 
            feedback = feedback 
        };
        
        string jsonData = JsonUtility.ToJson(trainingData);
        
        UnityWebRequest request = new UnityWebRequest($"{pythonServerURL}/train", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ AI training data sent successfully");
        }
        else
        {
            Debug.LogWarning($"⚠️ Failed to send training data: {request.error}");
        }
        
        request.Dispose();
    }
    
    [ContextMenu("Test Python AI")]
    public void TestPythonAI()
    {
        GetAIResponse("Quoridor là gì?", (response, success, confidence) =>
        {
            if (success)
            {
                Debug.Log($"✅ PYTHON AI TEST SUCCESSFUL!");
                Debug.Log($"Response: {response}");
                Debug.Log($"Confidence: {confidence:F2}");
            }
            else
            {
                Debug.LogError($"❌ PYTHON AI TEST FAILED: {response}");
            }
        });
    }
    
    [ContextMenu("Check Server Status")]
    public void CheckServer()
    {
        StartCoroutine(CheckServerStatus());
    }
    
    [ContextMenu("Reset AI Memory")]
    public void ResetAIMemory()
    {
        StartCoroutine(ResetMemory());
    }
    
    IEnumerator ResetMemory()
    {
        UnityWebRequest request = new UnityWebRequest($"{pythonServerURL}/reset", "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ AI memory reset successfully");
        }
        else
        {
            Debug.LogError($"❌ Failed to reset AI memory: {request.error}");
        }
        
        request.Dispose();
    }
    
    [ContextMenu("Export AI Data")]
    public void ExportAIData()
    {
        StartCoroutine(ExportData());
    }
    
    IEnumerator ExportData()
    {
        UnityWebRequest request = UnityWebRequest.Get($"{pythonServerURL}/export");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("📊 AI Data Export:");
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"❌ Failed to export AI data: {request.error}");
        }
        
        request.Dispose();
    }
}

// Data classes for JSON serialization
[System.Serializable]
public class ChatRequest
{
    public string message;
}

[System.Serializable]
public class ChatResponse
{
    public string response;
    public float confidence;
    public string source;
    public string timestamp;
}

[System.Serializable]
public class TrainingRequest
{
    public string input;
    public string response;
    public string feedback;
}
