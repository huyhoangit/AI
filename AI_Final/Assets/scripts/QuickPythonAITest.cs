using UnityEngine;

/// <summary>
/// Quick Test for Python AI - Add to GameObject and test
/// </summary>
public class QuickPythonAITest : MonoBehaviour
{
    void Start()
    {
        // Test ngay khi start
        Invoke("TestPythonAI", 1f);
    }
    
    void TestPythonAI()
    {
        Debug.Log("🚀 Testing Python AI connection...");
        
        // Tìm hoặc tạo PythonLocalAI component
        PythonLocalAI pythonAI = GetComponent<PythonLocalAI>();
        if (pythonAI == null)
        {
            pythonAI = gameObject.AddComponent<PythonLocalAI>();
            Debug.Log("🔧 Added PythonLocalAI component");
        }
        
        // Test với câu hỏi về Quoridor
        pythonAI.GetAIResponse("Quoridor là gì? Giải thích ngắn gọn.", OnTestResponse);
    }
    
    void OnTestResponse(string response, bool success, float confidence)
    {
        if (success)
        {
            Debug.Log("✅ PYTHON AI TEST THÀNH CÔNG!");
            Debug.Log($"🤖 AI Response: {response}");
            Debug.Log($"📊 Confidence: {confidence:F2}");
            Debug.Log("🎉 Python AI đã sẵn sàng sử dụng!");
            
            // Test thêm câu hỏi
            TestMoreQuestions();
        }
        else
        {
            Debug.LogError("❌ PYTHON AI TEST THẤT BẠI!");
            Debug.LogError($"Error: {response}");
            ShowTroubleshootingTips();
        }
    }
    
    void TestMoreQuestions()
    {
        PythonLocalAI pythonAI = GetComponent<PythonLocalAI>();
        
        // Test câu hỏi về chiến thuật
        pythonAI.GetAIResponse("Chiến thuật thắng trong Quoridor là gì?", (response, success, confidence) => {
            if (success)
            {
                Debug.Log($"🧠 Strategy Response: {response}");
            }
        });
        
        // Test câu hỏi về luật chơi
        pythonAI.GetAIResponse("Làm sao để đặt tường trong Quoridor?", (response, success, confidence) => {
            if (success)
            {
                Debug.Log($"📋 Rules Response: {response}");
            }
        });
    }
    
    void ShowTroubleshootingTips()
    {
        Debug.Log("🛠️ TROUBLESHOOTING:");
        Debug.Log("1. Check if Python server is running at http://localhost:5000");
        Debug.Log("2. Make sure Flask is installed: py -m pip install flask");
        Debug.Log("3. Check firewall settings");
        Debug.Log("4. Verify simple_quoridor_ai.py is running");
    }
    
    [ContextMenu("Test Python AI Manual")]
    public void ManualTest()
    {
        TestPythonAI();
    }
    
    [ContextMenu("Test Server Connection")]
    public void TestConnection()
    {
        PythonLocalAI pythonAI = GetComponent<PythonLocalAI>();
        if (pythonAI == null)
        {
            pythonAI = gameObject.AddComponent<PythonLocalAI>();
        }
        
        pythonAI.CheckServer();
    }
}
