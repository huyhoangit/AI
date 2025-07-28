using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using TMPro;

/// <summary>
/// Enhanced AI Chat Manager với UI đẹp và hybrid system
/// Hỗ trợ cả local model và free API
/// </summary>
public class EnhancedAIChatManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatPanel;
    public TMP_InputField chatInput;
    public ScrollRect chatScrollRect;
    public Transform chatContent;
    public GameObject messagePrefab;
    public Button sendButton;
    public Button clearChatButton;
    public Button toggleChatButton;
    public Image aiStatusIndicator;
    
    [Header("Chat Settings")]
    public bool isVisible = false;
    public int maxMessages = 50;
    public float typingSpeed = 0.05f;
    public bool autoScroll = true;
    
    [Header("API Settings")]
    public bool useAPI = true;
    public string apiProvider = "ollama"; // "ollama", "huggingface", "cohere"
    [SerializeField] private string ollamaURL = "http://localhost:11434/api/generate";
    [SerializeField] private string huggingfaceAPI = "YOUR_HF_API_KEY";
    [SerializeField] private string cohereAPI = "YOUR_COHERE_API_KEY";
    public string modelName = "llama2";
    public float temperature = 0.7f;
    public int maxTokens = 150;
    
    [Header("Local AI Settings")]
    public bool useLocalAI = true;
    public bool preferLocal = true;
    
    [Header("Game Integration")]
    public GameManager gameManager;
    public QuoridorAI quoridorAI;
    public bool provideGameAdvice = true;
    
    [Header("AI Personality")]
    public string aiName = "QuoridorBot";
    public string personality = "Tôi là một AI thông minh chuyên về game Quoridor. Tôi thân thiện, hỗ trợ và luôn sẵn sàng giúp đỡ!";
    
    private List<ChatMessage> chatHistory = new List<ChatMessage>();
    private bool isTyping = false;
    private LocalAIResponder localAI;
    
    [System.Serializable]
    public class ChatMessage
    {
        public string sender;
        public string message;
        public System.DateTime timestamp;
        public bool isUser;
        
        public ChatMessage(string sender, string message, bool isUser)
        {
            this.sender = sender;
            this.message = message;
            this.timestamp = System.DateTime.Now;
            this.isUser = isUser;
        }
    }
    
    void Start()
    {
        InitializeChat();
        SetupEventListeners();
        ShowWelcomeMessage();
    }
    
    void InitializeChat()
    {
        if (chatPanel != null)
            chatPanel.SetActive(isVisible);
            
        localAI = new LocalAIResponder();
        
        // Setup UI colors and styling
        SetupChatStyling();
    }
    
    void SetupEventListeners()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(SendMessage);
            
        if (clearChatButton != null)
            clearChatButton.onClick.AddListener(ClearChat);
            
        if (toggleChatButton != null)
            toggleChatButton.onClick.AddListener(ToggleChat);
            
        if (chatInput != null)
        {
            chatInput.onEndEdit.AddListener(OnInputEndEdit);
        }
    }
    
    void OnInputEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessage();
        }
    }
    
    void SetupChatStyling()
    {
        // Set modern colors and styling
        if (aiStatusIndicator != null)
            aiStatusIndicator.color = Color.green; // Online indicator
    }
    
    void ShowWelcomeMessage()
    {
        string welcomeMsg = $"Xin chào! Tôi là {aiName}. Tôi có thể giúp bạn chơi Quoridor tốt hơn. Hãy hỏi tôi bất cứ điều gì!";
        AddMessage(aiName, welcomeMsg, false);
    }
    
    public void SendMessage()
    {
        if (chatInput == null || string.IsNullOrEmpty(chatInput.text.Trim()) || isTyping)
            return;
            
        string userMessage = chatInput.text.Trim();
        chatInput.text = "";
        
        // Add user message
        AddMessage("Bạn", userMessage, true);
        
        // Get AI response
        StartCoroutine(GetAIResponse(userMessage));
    }
    
    IEnumerator GetAIResponse(string userMessage)
    {
        isTyping = true;
        UpdateAIStatus(false); // Show thinking
        
        string response = "";
        
        // Try local AI first if enabled and preferred
        if (useLocalAI && preferLocal)
        {
            response = localAI.GetResponse(userMessage, GetGameContext());
        }
        
        // If no local response or API is preferred, try API
        if (string.IsNullOrEmpty(response) && useAPI)
        {
            yield return StartCoroutine(GetAPIResponse(userMessage, (apiResponse) => {
                response = apiResponse;
            }));
        }
        
        // Fallback to local if API failed
        if (string.IsNullOrEmpty(response) && useLocalAI)
        {
            response = localAI.GetResponse(userMessage, GetGameContext());
        }
        
        // Ultimate fallback
        if (string.IsNullOrEmpty(response))
        {
            response = "Xin lỗi, tôi đang gặp một chút vấn đề. Bạn có thể thử lại sau không?";
        }
        
        // Simulate typing delay
        yield return new WaitForSeconds(0.5f);
        
        // Type out the response
        yield return StartCoroutine(TypeMessage(aiName, response, false));
        
        isTyping = false;
        UpdateAIStatus(true); // Show ready
    }
    
    IEnumerator GetAPIResponse(string message, System.Action<string> callback)
    {
        switch (apiProvider.ToLower())
        {
            case "ollama":
                yield return StartCoroutine(CallOllamaAPI(message, callback));
                break;
            case "huggingface":
                yield return StartCoroutine(CallHuggingFaceAPI(message, callback));
                break;
            case "cohere":
                yield return StartCoroutine(CallCohereAPI(message, callback));
                break;
            default:
                callback("API provider không được hỗ trợ.");
                break;
        }
    }
    
    IEnumerator CallOllamaAPI(string message, System.Action<string> callback)
    {
        var requestData = new
        {
            model = modelName,
            prompt = BuildPrompt(message),
            stream = false,
            options = new
            {
                temperature = temperature,
                num_predict = maxTokens
            }
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(ollamaURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);
                    callback(response.response);
                }
                catch
                {
                    callback("Không thể xử lý phản hồi từ Ollama.");
                }
            }
            else
            {
                Debug.LogError($"Ollama API Error: {request.error}");
                callback("");
            }
        }
    }
    
    IEnumerator CallHuggingFaceAPI(string message, System.Action<string> callback)
    {
        // Implement HuggingFace API call here
        if (string.IsNullOrEmpty(huggingfaceAPI) || huggingfaceAPI == "YOUR_HF_API_KEY")
        {
            callback("HuggingFace API key chưa được cấu hình. Vui lòng nhập API key trong Inspector.");
        }
        else
        {
            // TODO: Implement actual HuggingFace API call
            yield return new WaitForSeconds(1f);
            callback("HuggingFace API chưa được triển khai hoàn chỉnh.");
        }
    }
    
    IEnumerator CallCohereAPI(string message, System.Action<string> callback)
    {
        // Implement Cohere API call here
        if (string.IsNullOrEmpty(cohereAPI) || cohereAPI == "YOUR_COHERE_API_KEY")
        {
            callback("Cohere API key chưa được cấu hình. Vui lòng nhập API key trong Inspector.");
        }
        else
        {
            // TODO: Implement actual Cohere API call
            yield return new WaitForSeconds(1f);
            callback("Cohere API chưa được triển khai hoàn chỉnh.");
        }
    }
    
    string BuildPrompt(string userMessage)
    {
        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine(personality);
        prompt.AppendLine();
        
        if (provideGameAdvice && gameManager != null)
        {
            prompt.AppendLine("Thông tin game hiện tại:");
            prompt.AppendLine(GetGameContext());
            prompt.AppendLine();
        }
        
        prompt.AppendLine($"Người chơi hỏi: {userMessage}");
        prompt.AppendLine("Trả lời ngắn gọn và hữu ích:");
        
        return prompt.ToString();
    }
    
    string GetGameContext()
    {
        if (gameManager == null) return "Không có thông tin game.";
        
        // Get current game state info
        return $"Game đang chạy. Tham khảo GameManager để lấy thông tin chi tiết.";
    }
    
    IEnumerator TypeMessage(string sender, string message, bool isUser)
    {
        GameObject messageObj = CreateMessageBubble(sender, "", isUser);
        TMP_Text messageText = messageObj.GetComponentInChildren<TMP_Text>();
        
        for (int i = 0; i <= message.Length; i++)
        {
            messageText.text = message.Substring(0, i);
            if (autoScroll && chatScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
            yield return new WaitForSeconds(typingSpeed);
        }
        
        // Add to history
        chatHistory.Add(new ChatMessage(sender, message, isUser));
    }
    
    void AddMessage(string sender, string message, bool isUser)
    {
        StartCoroutine(TypeMessage(sender, message, isUser));
    }
    
    GameObject CreateMessageBubble(string sender, string message, bool isUser)
    {
        GameObject messageObj = Instantiate(messagePrefab, chatContent);
        
        // Configure message bubble appearance
        ConfigureMessageBubble(messageObj, sender, message, isUser);
        
        return messageObj;
    }
    
    void ConfigureMessageBubble(GameObject messageObj, string sender, string message, bool isUser)
    {
        // Configure colors, alignment, etc. based on sender
        TMP_Text senderText = messageObj.transform.Find("SenderText")?.GetComponent<TMP_Text>();
        TMP_Text messageText = messageObj.transform.Find("MessageText")?.GetComponent<TMP_Text>();
        Image bubble = messageObj.GetComponent<Image>();
        
        if (senderText != null)
            senderText.text = sender;
            
        if (messageText != null)
            messageText.text = message;
            
        if (bubble != null)
        {
            bubble.color = isUser ? new Color(0.2f, 0.6f, 1f, 0.8f) : new Color(0.8f, 0.8f, 0.8f, 0.8f);
        }
        
        // Align message based on sender
        RectTransform rectTransform = messageObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = isUser ? new Vector2(0.3f, 0) : new Vector2(0, 0);
            rectTransform.anchorMax = isUser ? new Vector2(1, 1) : new Vector2(0.7f, 1);
        }
    }
    
    void UpdateAIStatus(bool isReady)
    {
        if (aiStatusIndicator != null)
        {
            aiStatusIndicator.color = isReady ? Color.green : Color.yellow;
        }
    }
    
    public void ToggleChat()
    {
        isVisible = !isVisible;
        if (chatPanel != null)
            chatPanel.SetActive(isVisible);
    }
    
    public void ClearChat()
    {
        chatHistory.Clear();
        
        if (chatContent != null)
        {
            foreach (Transform child in chatContent)
            {
                Destroy(child.gameObject);
            }
        }
        
        ShowWelcomeMessage();
    }
    
    // Data structures for API responses
    [System.Serializable]
    public class OllamaResponse
    {
        public string response;
        public bool done;
    }
}

/// <summary>
/// Local AI Responder cho responses offline
/// </summary>
public class LocalAIResponder
{
    private Dictionary<string, List<string>> responses;
    
    public LocalAIResponder()
    {
        InitializeResponses();
    }
    
    void InitializeResponses()
    {
        responses = new Dictionary<string, List<string>>();
        
        // Game rules responses
        responses["luật chơi"] = new List<string>
        {
            "Quoridor là game đua đến đích! Mục tiêu là đưa quân cờ của bạn đến hàng đối diện trước đối thủ.",
            "Bạn có thể di chuyển quân cờ hoặc đặt tường để cản đối thủ. Mỗi lượt chỉ được làm một hành động!",
            "Nhớ là phải luôn để đường cho đối thủ đến đích nhé!"
        };
        
        responses["chiến thuật"] = new List<string>
        {
            "Hãy thử cân bằng giữa tiến tới và cản đối thủ!",
            "Đặt tường ở giữa bàn cờ thường hiệu quả hơn là ở rìa.",
            "Quan sát động thái đối thủ để dự đoán ý định của họ!"
        };
        
        responses["chào"] = new List<string>
        {
            "Xin chào! Sẵn sàng cho một ván Quoridor thú vị chưa?",
            "Chào bạn! Tôi ở đây để giúp bạn chơi Quoridor giỏi hơn!",
            "Hi! Bạn cần tôi giải thích luật chơi hay đưa ra lời khuyên?"
        };
        
        responses["default"] = new List<string>
        {
            "Thú vị! Bạn có thể hỏi tôi về luật chơi, chiến thuật, hoặc bất cứ điều gì về Quoridor!",
            "Tôi không chắc hiểu ý bạn, nhưng tôi luôn sẵn sàng giúp đỡ về game Quoridor!",
            "Hmm, bạn có thể diễn đạt khác được không? Hoặc hỏi tôi về game nhé!"
        };
    }
    
    public string GetResponse(string input, string gameContext = "")
    {
        string lowerInput = input.ToLower();
        
        foreach (var category in responses.Keys)
        {
            if (lowerInput.Contains(category))
            {
                var responseList = responses[category];
                return responseList[Random.Range(0, responseList.Count)];
            }
        }
        
        // Default response
        var defaultList = responses["default"];
        return defaultList[Random.Range(0, defaultList.Count)];
    }
}