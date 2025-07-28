using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Demo đơn giản cho hệ thống chat AI
/// Dễ setup và test nhanh
/// </summary>
public class HybridChatDemo : MonoBehaviour
{
    [Header("Simple UI Setup")]
    public GameObject chatWindow;
    public TMP_InputField messageInput;
    public TMP_Text chatDisplay;
    public Button sendButton;
    public Button toggleButton;
    public ScrollRect scrollRect;
    
    [Header("Settings")]
    public bool isVisible = false;
    public int maxDisplayLines = 20;
    public string aiName = "QuoridorAI";
    
    private Queue<string> chatLines = new Queue<string>();
    private SimpleAI simpleAI;
    private bool isProcessing = false;
    
    void Start()
    {
        // Initialize
        simpleAI = new SimpleAI();
        
        // Setup UI
        if (chatWindow != null)
            chatWindow.SetActive(isVisible);
            
        // Setup buttons
        if (sendButton != null)
            sendButton.onClick.AddListener(SendMessage);
            
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleChat);
            
        // Setup input
        if (messageInput != null)
        {
            messageInput.onEndEdit.AddListener(OnEnterPressed);
        }
        
        // Welcome message
        AddChatLine($"[{aiName}]: Chào bạn! Tôi là AI hỗ trợ Quoridor. Hỏi tôi bất cứ điều gì!");
    }
    
    void OnEnterPressed(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessage();
        }
    }
    
    public void SendMessage()
    {
        if (messageInput == null || string.IsNullOrEmpty(messageInput.text.Trim()) || isProcessing)
            return;
            
        string userMessage = messageInput.text.Trim();
        messageInput.text = "";
        
        // Add user message
        AddChatLine($"[Bạn]: {userMessage}");
        
        // Get AI response
        StartCoroutine(ProcessAIResponse(userMessage));
    }
    
    IEnumerator ProcessAIResponse(string userMessage)
    {
        isProcessing = true;
        
        // Show typing indicator
        AddChatLine($"[{aiName}]: đang suy nghĩ...");
        
        // Simulate thinking time
        yield return new WaitForSeconds(1f);
        
        // Remove typing indicator
        RemoveLastLine();
        
        // Get AI response
        string aiResponse = simpleAI.GetResponse(userMessage);
        AddChatLine($"[{aiName}]: {aiResponse}");
        
        isProcessing = false;
    }
    
    void AddChatLine(string line)
    {
        chatLines.Enqueue(line);
        
        // Keep only recent messages
        while (chatLines.Count > maxDisplayLines)
        {
            chatLines.Dequeue();
        }
        
        UpdateChatDisplay();
    }
    
    void RemoveLastLine()
    {
        if (chatLines.Count > 0)
        {
            var lines = new List<string>(chatLines);
            lines.RemoveAt(lines.Count - 1);
            chatLines.Clear();
            foreach (var line in lines)
            {
                chatLines.Enqueue(line);
            }
            UpdateChatDisplay();
        }
    }
    
    void UpdateChatDisplay()
    {
        if (chatDisplay != null)
        {
            chatDisplay.text = string.Join("\n", chatLines);
            
            // Auto scroll to bottom
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
    
    public void ToggleChat()
    {
        isVisible = !isVisible;
        if (chatWindow != null)
            chatWindow.SetActive(isVisible);
    }
    
    public void ClearChat()
    {
        chatLines.Clear();
        UpdateChatDisplay();
        AddChatLine($"[{aiName}]: Chat đã được xóa. Bạn cần hỗ trợ gì không?");
    }
}

/// <summary>
/// Simple AI system cho demo
/// </summary>
public class SimpleAI
{
    private Dictionary<string, string[]> knowledgeBase;
    
    public SimpleAI()
    {
        InitializeKnowledge();
    }
    
    void InitializeKnowledge()
    {
        knowledgeBase = new Dictionary<string, string[]>();
        
        // Quoridor rules
        knowledgeBase["luật"] = new string[]
        {
            "Quoridor có luật đơn giản: Di chuyển quân cờ hoặc đặt tường mỗi lượt để đến đích trước đối thủ!",
            "Bạn có 10 quân tường, sử dụng khôn ngoan để cản đối thủ nhưng không được chặn hoàn toàn đường đi!",
            "Quân cờ di chuyển 1 ô mỗi lượt (hoặc nhảy qua đối thủ nếu có thể)."
        };
        
        knowledgeBase["chiến thuật"] = new string[]
        {
            "Cân bằng giữa tấn công và phòng thủ! Đừng chỉ chạy thẳng mà hãy quan sát đối thủ.",
            "Đặt tường ở giữa bàn cờ hiệu quả hơn! Kiểm soát trung tâm là chìa khóa.",
            "Quan sát pattern di chuyển của đối thủ để dự đoán ý định!"
        };
        
        knowledgeBase["tường"] = new string[]
        {
            "Tường rất quan trọng! Dùng để làm chậm đối thủ hoặc tạo đường đi thuận lợi cho mình.",
            "Mỗi người có 10 tường, suy nghĩ kỹ trước khi đặt!",
            "Tường không được chặn hoàn toàn đường đi của đối thủ - luật quan trọng!"
        };
        
        knowledgeBase["chào"] = new string[]
        {
            "Xin chào! Tôi sẵn sàng giúp bạn master game Quoridor!",
            "Hi! Bạn muốn học luật chơi, chiến thuật hay cần lời khuyên gì?",
            "Chào bạn! Hôm nay chúng ta sẽ chinh phục Quoridor nhé!"
        };
        
        knowledgeBase["help"] = new string[]
        {
            "Tôi có thể giúp bạn về: luật chơi, chiến thuật, cách đặt tường, và tips để thắng!",
            "Hỏi tôi về bất cứ điều gì liên quan đến Quoridor! Ví dụ: 'luật chơi như thế nào?'",
            "Tôi ở đây để làm bạn trở thành cao thủ Quoridor! Cần gì cứ hỏi!"
        };
    }
    
    public string GetResponse(string input)
    {
        string lowerInput = input.ToLower();
        
        // Check for keywords
        foreach (var key in knowledgeBase.Keys)
        {
            if (lowerInput.Contains(key))
            {
                string[] responses = knowledgeBase[key];
                return responses[Random.Range(0, responses.Length)];
            }
        }
        
        // Check for greetings
        if (lowerInput.Contains("xin chào") || lowerInput.Contains("hello") || 
            lowerInput.Contains("hi") || lowerInput.Contains("chào"))
        {
            return knowledgeBase["chào"][Random.Range(0, knowledgeBase["chào"].Length)];
        }
        
        // Check for help
        if (lowerInput.Contains("giúp") || lowerInput.Contains("help") || 
            lowerInput.Contains("hỗ trợ"))
        {
            return knowledgeBase["help"][Random.Range(0, knowledgeBase["help"].Length)];
        }
        
        // Default responses
        string[] defaultResponses = {
            "Thú vị! Bạn có thể hỏi tôi về luật chơi, chiến thuật Quoridor, hoặc cách sử dụng tường hiệu quả!",
            "Tôi chưa hiểu rõ ý bạn. Thử hỏi về 'luật chơi', 'chiến thuật', hay 'tường' xem!",
            "Hmm, có thể bạn muốn biết về Quoridor? Tôi có thể giải thích luật và chiến thuật!",
            "Bạn có thể hỏi cụ thể hơn không? Ví dụ: 'Làm sao để thắng?' hoặc 'Cách đặt tường?'"
        };
        
        return defaultResponses[Random.Range(0, defaultResponses.Length)];
    }
}