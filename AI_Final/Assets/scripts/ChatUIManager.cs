using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Advanced Chat UI Manager với model tự train và API hybrid
/// Tự động chuyển đổi giữa local model và cloud API
/// </summary>
public class ChatUIManager : MonoBehaviour
{
    [Header("UI Components")]
    public InputField inputField;
    public Button sendButton;
    public Transform chatContent;
    public GameObject chatMessagePrefab;
    public ScrollRect chatScrollRect;
    
    [Header("AI Components")]
    public bool useTrainedModel = true;
    public bool useAPIFallback = true;
    public float apiTimeout = 5f;
    
    [Header("Chat Settings")]
    public int maxChatHistory = 50;
    public bool autoScroll = true;
    public bool showTypingIndicator = true;
    
    private TrainedChatbotModel trainedModel;
    private HybridAIManager apiManager;
    private List<UIChatMessage> chatHistory = new List<UIChatMessage>();
    private bool isProcessing = false;
    
    void Start()
    {
        InitializeChatSystem();
        SetupUIEvents();
        ShowWelcomeMessage();
    }
    
    void InitializeChatSystem()
    {
        // Initialize trained model
        trainedModel = new TrainedChatbotModel();
        trainedModel.Initialize();
        
        // Get or add API manager
        apiManager = GetComponent<HybridAIManager>();
        if (apiManager == null && useAPIFallback)
        {
            apiManager = gameObject.AddComponent<HybridAIManager>();
        }
        
        Debug.Log("🤖 Advanced Chat System initialized");
        Debug.Log($"Trained Model: {useTrainedModel}, API Fallback: {useAPIFallback}");
    }
    
    void SetupUIEvents()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendClicked);
            
        if (inputField != null)
        {
            inputField.onEndEdit.AddListener(OnInputEndEdit);
        }
    }
    
    void ShowWelcomeMessage()
    {
        AddMessage("Xin chào! Tôi là AI hỗ trợ Quoridor với khả năng học và API thông minh. Hãy hỏi tôi bất kỳ điều gì!", false);
        AddMessage("💡 Tôi có thể: Giải thích luật chơi, đưa ra chiến thuật, phân tích nước đi, và học từ cuộc trò chuyện của chúng ta!", false);
    }
    
    void OnInputEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSendClicked();
        }
    }
    
    void OnSendClicked()
    {
        if (isProcessing) return;
        
        string userInput = inputField.text.Trim();
        if (string.IsNullOrEmpty(userInput)) return;
        
        // Add user message
        AddMessage(userInput, true);
        inputField.text = "";
        
        // Process response
        StartCoroutine(ProcessUserInput(userInput));
    }
    
    IEnumerator ProcessUserInput(string userInput)
    {
        isProcessing = true;
        
        // Show typing indicator
        GameObject typingIndicator = null;
        if (showTypingIndicator)
        {
            typingIndicator = AddTypingIndicator();
        }
        
        yield return new WaitForSeconds(0.5f); // Simulate thinking time
        
        string response = "";
        bool foundResponse = false;
        
        // Try trained model first
        if (useTrainedModel)
        {
            response = trainedModel.GetResponse(userInput);
            if (!IsDefaultResponse(response))
            {
                foundResponse = true;
                Debug.Log("📚 Using trained model response");
            }
        }
        
        // If no good response from trained model, try API
        if (!foundResponse && useAPIFallback && apiManager != null)
        {
            Debug.Log("🌐 Falling back to API...");
            bool apiCompleted = false;
            
            apiManager.GetAIResponse(userInput, (apiResponse, success) => {
                if (success && !string.IsNullOrEmpty(apiResponse))
                {
                    response = apiResponse;
                    foundResponse = true;
                    Debug.Log("✅ API response received");
                }
                else
                {
                    Debug.LogWarning("❌ API failed, using default response");
                }
                apiCompleted = true;
            });
            
            // Wait for API with timeout
            float waitTime = 0f;
            while (!apiCompleted && waitTime < apiTimeout)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }
        }
        
        // Remove typing indicator
        if (typingIndicator != null)
        {
            DestroyImmediate(typingIndicator);
        }
        
        // Add final response
        if (string.IsNullOrEmpty(response))
        {
            response = "Xin lỗi, tôi gặp khó khăn trong việc xử lý câu hỏi này. Bạn có thể thử hỏi lại không?";
        }
        
        AddMessage(response, false);
        
        // Record interaction
        RecordChatHistory(userInput, response);
        
        isProcessing = false;
    }
    
    bool IsDefaultResponse(string response)
    {
        // Check if response is from default responses (low confidence)
        string[] defaultIndicators = {"chưa hiểu", "chưa học", "thử hỏi", "không hiểu"};
        foreach (var indicator in defaultIndicators)
        {
            if (response.ToLower().Contains(indicator))
                return true;
        }
        return false;
    }
    
    GameObject AddTypingIndicator()
    {
        var typingObj = Instantiate(chatMessagePrefab, chatContent);
        var textComp = typingObj.GetComponentInChildren<Text>();
        
        if (textComp != null)
        {
            textComp.text = "🤖 AI đang suy nghĩ...";
            textComp.color = Color.yellow;
            textComp.fontStyle = FontStyle.Italic;
        }
        
        if (autoScroll)
            StartCoroutine(ScrollToBottom());
            
        return typingObj;
    }
    
    void AddMessage(string message, bool isUser)
    {
        // Manage chat history limit
        if (chatContent.childCount >= maxChatHistory)
        {
            DestroyImmediate(chatContent.GetChild(0).gameObject);
        }
        
        var msgObj = Instantiate(chatMessagePrefab, chatContent);
        var textComp = msgObj.GetComponentInChildren<Text>();
        
        if (textComp != null)
        {
            string prefix = isUser ? "👤 Bạn: " : "🤖 AI: ";
            textComp.text = prefix + message;
            textComp.color = isUser ? new Color(0.3f, 0.8f, 0.3f) : Color.white;
            
            if (isUser)
            {
                textComp.fontStyle = FontStyle.Bold;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No Text component found in chat message prefab");
        }
        
        if (autoScroll)
            StartCoroutine(ScrollToBottom());
    }
    
    IEnumerator ScrollToBottom()
    {
        yield return null; // Wait one frame
        yield return null; // Wait another frame for layout
        
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    void RecordChatHistory(string userInput, string aiResponse)
    {
        chatHistory.Add(new UIChatMessage
        {
            userMessage = userInput,
            aiResponse = aiResponse,
            timestamp = System.DateTime.Now,
            source = isProcessing ? "API" : "Local"
        });
        
        // Keep history manageable
        if (chatHistory.Count > 100)
        {
            chatHistory.RemoveAt(0);
        }
    }
    
    [ContextMenu("Show Analytics")]
    public void ShowAnalytics()
    {
        if (trainedModel != null)
        {
            var analytics = trainedModel.GetAnalytics();
            Debug.Log($"📊 Chat Analytics:");
            Debug.Log($"  Total Patterns: {analytics.totalPatterns}");
            Debug.Log($"  Total Conversations: {analytics.totalConversations}");
            Debug.Log($"  Chat History: {chatHistory.Count} messages");
        }
        
        if (apiManager != null)
        {
            Debug.Log(apiManager.GetAPIStatus());
        }
    }
    
    [ContextMenu("Clear Chat")]
    public void ClearChat()
    {
        foreach (Transform child in chatContent)
        {
            DestroyImmediate(child.gameObject);
        }
        
        chatHistory.Clear();
        ShowWelcomeMessage();
        Debug.Log("🗑️ Chat cleared");
    }
    
    [ContextMenu("Export Chat History")]
    public void ExportChatHistory()
    {
        string export = "=== CHAT HISTORY EXPORT ===\n";
        foreach (var chat in chatHistory)
        {
            export += $"[{chat.timestamp:HH:mm:ss}] User: {chat.userMessage}\n";
            export += $"[{chat.timestamp:HH:mm:ss}] AI ({chat.source}): {chat.aiResponse}\n\n";
        }
        
        Debug.Log(export);
        // Could save to file here if needed
    }

    [ContextMenu("Speak Last AI Message")]
    public void OnTTSButtonClicked()
    {
        // Lấy message cuối cùng của AI
        string lastAIMessage = null;
        for (int i = chatHistory.Count - 1; i >= 0; i--)
        {
            if (!string.IsNullOrEmpty(chatHistory[i].aiResponse))
            {
                lastAIMessage = chatHistory[i].aiResponse;
                break;
            }
        }
        if (!string.IsNullOrEmpty(lastAIMessage))
        {
            ChatService chatService = FindFirstObjectByType<ChatService>();
            if (chatService != null)
            {
                chatService.RequestTTS(lastAIMessage, (audio) =>
                {
                    if (audio != null && audio.Length > 0)
                    {
                        Debug.Log($"🔊 Đã nhận audio từ TTS ({audio.Length} bytes)");
                        // Phát audio bằng AudioSource tạm thời
                        var audioClip = WavUtility.ToAudioClip(audio, 0, "TTSClip");
                        AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Không nhận được audio từ TTS");
                    }
                });
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy ChatService trong scene!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Không có message AI nào để đọc!");
        }
    }
}

/// <summary>
/// Chat message data structure for UI
/// </summary>
[System.Serializable]
public class UIChatMessage
{
    public string userMessage;
    public string aiResponse;
    public System.DateTime timestamp;
    public string source; // "Local" or "API"
} 