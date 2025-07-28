using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine.Networking;

/// <summary>
/// AI Chat Manager cho Quoridor Game - Hybrid System (Local + API)
/// - Local responses cho kiến thức được train sẵn (game rules, strategies)
/// - API calls cho câu hỏi phức tạp và kiến thức mở rộng
/// - Fallback system để đảm bảo luôn có response
/// </summary>
public class AIChatManager : MonoBehaviour
{
    [Header("UI References")]
    public Component chatInput;
    public Component chatScrollRect;
    public Component chatDisplay;
    public Component sendButton;
    public Component clearChatButton;
    public GameObject chatPanel;
    public Component toggleChatButton;
    
    [Header("Chat Settings")]
    public bool useHybridMode = true; // Kết hợp local + API
    public string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent"; // API endpoint
    public string apiKey = "YOUR_API_KEY_HERE"; // API key
    public string modelName = "gemini-1.5-pro"; // Model name
    public int maxOutputTokens = 150;
    public float temperature = 0.7f;
    public int maxChatHistory = 50;
    
    [Header("Hybrid Settings")]
    public bool preferLocalResponses = true; // Ưu tiên local trước
    public bool fallbackToAPI = true; // Fallback to API nếu local không có
    public float apiThinkingTime = 1.5f; // Thời gian "suy nghĩ" khi gọi API
    
    [Header("Game Integration")]
    public QuoridorAI quoridorAI;
    public GameManager gameManager;
    public bool enableGameContext = true; // Cho phép AI biết trạng thái game
    public bool enablePersonality = true; // Cho phép AI có personality
    
    [Header("AI Personality Settings")]
    public string aiPersonality = "Bạn là một AI thông minh và thân thiện đang chơi game Quoridor. Bạn có tính cách vui vẻ và thích thách thức. Bạn sẽ bình luận về game, đưa ra lời khuyên, và tương tác một cách tự nhiên với người chơi.";
    public bool useSarcasm = false; // Cho phép AI dùng châm biếm
    public bool giveHints = true; // Cho phép AI đưa ra gợi ý
    
    private List<BasicChatMessage> chatHistory = new List<BasicChatMessage>();
    public bool isChatVisible = false;
    private bool isProcessing = false;
    private BasicGameState lastGameState;
    
    void Start()
    {
        InitializeChatSystem();
        SetupUI();
        
        // Tìm các component cần thiết
        if (quoridorAI == null)
            quoridorAI = FindFirstObjectByType<QuoridorAI>();
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        
        // Khởi tạo với message chào mừng
        AddSystemMessage("🤖 AI: Chào bạn! Tôi là AI đối thủ của bạn trong game Quoridor. Hãy chat với tôi bất cứ lúc nào! 😊");
        
        Debug.Log("🗣️ AI Chat Manager initialized");
    }
    
    void InitializeChatSystem()
    {
        // Khởi tạo chat history với system prompt
        chatHistory.Clear();
        
        string systemPrompt = BuildSystemPrompt();
        chatHistory.Add(new BasicChatMessage
        {
            role = "system",
            content = systemPrompt
        });
    }
    
    string BuildSystemPrompt()
    {
        StringBuilder prompt = new StringBuilder();
        
        // Base personality
        prompt.AppendLine(aiPersonality);
        prompt.AppendLine();
        
        // Game context
        if (enableGameContext)
        {
            prompt.AppendLine("GAME CONTEXT:");
            prompt.AppendLine("- Bạn đang chơi game Quoridor - một game board strategy");
            prompt.AppendLine("- Mục tiêu: Đưa quân của mình đến phía bên kia bàn cờ");
            prompt.AppendLine("- Có thể di chuyển hoặc đặt tường để block đối thủ");
            prompt.AppendLine("- Bạn là Player 2 (AI), người chơi là Player 1");
            prompt.AppendLine();
        }
        
        // Personality traits
        if (enablePersonality)
        {
            prompt.AppendLine("PERSONALITY TRAITS:");
            prompt.AppendLine("- Thân thiện và vui vẻ");
            prompt.AppendLine("- Thích thách thức và cạnh tranh");
            prompt.AppendLine("- Đôi khi hơi tinh nghịch");
            if (useSarcasm)
                prompt.AppendLine("- Có thể dùng châm biếm nhẹ nhàng");
            if (giveHints)
                prompt.AppendLine("- Sẵn sàng đưa ra gợi ý chiến thuật");
            prompt.AppendLine();
        }
        
        // Instructions
        prompt.AppendLine("INSTRUCTIONS:");
        prompt.AppendLine("- Trả lời ngắn gọn và tự nhiên (1-2 câu)");
        prompt.AppendLine("- Sử dụng emoji để thể hiện cảm xúc");
        prompt.AppendLine("- Phản ứng với trạng thái game hiện tại");
        prompt.AppendLine("- Tương tác thân thiện với người chơi");
        prompt.AppendLine("- Trả lời bằng tiếng Việt");
        
        return prompt.ToString();
    }
    
    void SetupUI()
    {
        // Setup buttons using reflection to avoid UI dependency
        if (sendButton != null)
        {
            SetupButtonListener(sendButton, "SendMessage");
        }
        
        if (clearChatButton != null)
        {
            SetupButtonListener(clearChatButton, "ClearChat");
        }
        
        if (toggleChatButton != null)
        {
            SetupButtonListener(toggleChatButton, "ToggleChat");
        }
        
        // Setup input field using reflection
        if (chatInput != null)
        {
            SetupInputFieldListener(chatInput);
        }
        
        // Initially hide chat
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }
    }
    
    void SetupButtonListener(Component button, string methodName)
    {
        // Use reflection to setup button click listeners
        var onClickField = button.GetType().GetField("onClick");
        if (onClickField != null)
        {
            var onClick = onClickField.GetValue(button);
            var addListenerMethod = onClick.GetType().GetMethod("AddListener", new[] { typeof(UnityEngine.Events.UnityAction) });
            if (addListenerMethod != null)
            {
                UnityEngine.Events.UnityAction action = null;
                switch (methodName)
                {
                    case "SendMessage":
                        action = () => SendMessage();
                        break;
                    case "ClearChat":
                        action = () => ClearChat();
                        break;
                    case "ToggleChat":
                        action = () => ToggleChat();
                        break;
                }
                if (action != null)
                {
                    addListenerMethod.Invoke(onClick, new object[] { action });
                }
            }
        }
    }
    
    void SetupInputFieldListener(Component inputField)
    {
        // Use reflection to setup input field listener
        var onEndEditField = inputField.GetType().GetField("onEndEdit");
        if (onEndEditField != null)
        {
            var onEndEdit = onEndEditField.GetValue(inputField);
            var addListenerMethod = onEndEdit.GetType().GetMethod("AddListener", new[] { typeof(UnityEngine.Events.UnityAction<string>) });
            if (addListenerMethod != null)
            {
                UnityEngine.Events.UnityAction<string> action = (value) => OnInputEndEdit(value);
                addListenerMethod.Invoke(onEndEdit, new object[] { action });
            }
        }
    }
    
    void OnInputEndEdit(string value)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessage();
        }
    }
    
    public void SendMessage()
    {
        if (isProcessing || chatInput == null)
            return;
            
        string userMessage = GetInputText();
        if (string.IsNullOrEmpty(userMessage.Trim()))
            return;
        
        SetInputText("");
        
        // Add user message to display
        AddUserMessage(userMessage);
        
        // Process AI response
        StartCoroutine(ProcessAIResponse(userMessage));
    }
    
    string GetInputText()
    {
        if (chatInput == null) return "";
        
        // Use reflection to get text property
        var textProperty = chatInput.GetType().GetProperty("text");
        if (textProperty != null)
        {
            return textProperty.GetValue(chatInput) as string ?? "";
        }
        return "";
    }
    
    void SetInputText(string text)
    {
        if (chatInput == null) return;
        
        // Use reflection to set text property
        var textProperty = chatInput.GetType().GetProperty("text");
        if (textProperty != null)
        {
            textProperty.SetValue(chatInput, text);
        }
    }
    
    void AddUserMessage(string message)
    {
        AddMessageToDisplay($"👤 Bạn: {message}");
        
        // Add to chat history
        chatHistory.Add(new BasicChatMessage
        {
            role = "user",
            content = message
        });
        
        // Limit chat history
        if (chatHistory.Count > maxChatHistory)
        {
            chatHistory.RemoveRange(1, chatHistory.Count - maxChatHistory); // Keep system message
        }
    }
    
    void AddAIMessage(string message)
    {
        AddMessageToDisplay($"🤖 AI: {message}");
        
        // Add to chat history
        chatHistory.Add(new BasicChatMessage
        {
            role = "assistant",
            content = message
        });
    }
    
    void AddSystemMessage(string message)
    {
        AddMessageToDisplay(message);
    }
    
    void AddMessageToDisplay(string message)
    {
        if (chatDisplay != null)
        {
            string currentText = GetDisplayText();
            SetDisplayText(currentText + $"\n{message}");
            
            // Scroll to bottom
            Canvas.ForceUpdateCanvases();
            if (chatScrollRect != null)
            {
                SetScrollPosition(0f);
            }
        }
    }
    
    string GetDisplayText()
    {
        if (chatDisplay == null) return "";
        
        // Use reflection to get text property
        var textProperty = chatDisplay.GetType().GetProperty("text");
        if (textProperty != null)
        {
            return textProperty.GetValue(chatDisplay) as string ?? "";
        }
        return "";
    }
    
    void SetDisplayText(string text)
    {
        if (chatDisplay == null) return;
        
        // Use reflection to set text property
        var textProperty = chatDisplay.GetType().GetProperty("text");
        if (textProperty != null)
        {
            textProperty.SetValue(chatDisplay, text);
        }
    }
    
    void SetScrollPosition(float position)
    {
        if (chatScrollRect == null) return;
        
        // Use reflection to set scroll position
        var scrollProperty = chatScrollRect.GetType().GetProperty("verticalNormalizedPosition");
        if (scrollProperty != null)
        {
            scrollProperty.SetValue(chatScrollRect, position);
        }
    }
    
    IEnumerator ProcessAIResponse(string userMessage)
    {
        isProcessing = true;
        AddMessageToDisplay("🤖 AI đang suy nghĩ...");
        
        // Phase 1: Try local responses first
        string localResponse = GetLocalResponse(userMessage);
        
        if (!string.IsNullOrEmpty(localResponse) && preferLocalResponses)
        {
            // Use local response
            yield return new WaitForSeconds(0.5f); // Quick thinking time
            RemoveThinkingMessage();
            AddAIMessage(localResponse);
        }
        else if (useHybridMode && fallbackToAPI && !string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_API_KEY_HERE")
        {
            // Phase 2: Use API for unknown topics
            yield return new WaitForSeconds(apiThinkingTime); // Longer thinking for API
            yield return StartCoroutine(CallAdvancedAPI(userMessage));
        }
        else
        {
            // Fallback to enhanced local response
            yield return new WaitForSeconds(0.5f);
            RemoveThinkingMessage();
            
            string fallbackResponse = string.IsNullOrEmpty(localResponse) 
                ? GetEnhancedFallbackResponse(userMessage) 
                : localResponse;
                
            AddAIMessage(fallbackResponse);
        }
        
        isProcessing = false;
    }
    
    void RemoveThinkingMessage()
    {
        if (chatDisplay != null)
        {
            string displayText = GetDisplayText();
            int lastIndex = displayText.LastIndexOf("\n🤖 AI đang suy nghĩ...");
            if (lastIndex >= 0)
            {
                SetDisplayText(displayText.Substring(0, lastIndex));
            }
        }
    }
    
    string BuildContextualMessage(string userMessage)
    {
        if (!enableGameContext) return userMessage;
        
        StringBuilder contextMessage = new StringBuilder();
        contextMessage.AppendLine($"USER MESSAGE: {userMessage}");
        contextMessage.AppendLine();
        
        // Add game state context
        if (quoridorAI != null)
        {
            contextMessage.AppendLine("CURRENT GAME STATE:");
            
            // AI position
            if (quoridorAI.aiPlayer != null)
            {
                contextMessage.AppendLine($"- AI position: [{quoridorAI.aiPlayer.col}, {quoridorAI.aiPlayer.row}]");
            }
            
            // Human position
            if (quoridorAI.humanPlayer != null)
            {
                contextMessage.AppendLine($"- Player position: [{quoridorAI.humanPlayer.col}, {quoridorAI.humanPlayer.row}]");
            }
            
            // Game progress
            contextMessage.AppendLine($"- Game phase: {GetGamePhase()}");
            contextMessage.AppendLine($"- Last move result: {GetLastMoveResult()}");
        }
        
        return contextMessage.ToString();
    }
    
    string GetGamePhase()
    {
        // Determine game phase based on positions
        if (quoridorAI?.aiPlayer != null && quoridorAI?.humanPlayer != null)
        {
            int aiDistanceToGoal = Mathf.Abs(quoridorAI.aiPlayer.row - 0);
            int humanDistanceToGoal = Mathf.Abs(quoridorAI.humanPlayer.row - 8);
            
            int totalDistance = aiDistanceToGoal + humanDistanceToGoal;
            
            if (totalDistance > 12) return "Early Game";
            if (totalDistance > 6) return "Mid Game";
            return "End Game";
        }
        
        return "Unknown";
    }
    
    string GetLastMoveResult()
    {
        // Simple heuristic for last move result
        if (lastGameState != null && quoridorAI != null)
        {
            // Compare with current state to determine what happened
            return "Move completed";
        }
        
        return "Game in progress";
    }
    
    // Hàm chuẩn hóa input: loại bỏ dấu câu, ký tự đặc biệt, khoảng trắng thừa, chuyển về chữ thường
    string NormalizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var sb = new StringBuilder();
        foreach (char c in input.ToLower())
        {
            if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                sb.Append(c);
        }
        return sb.ToString().Trim();
    }

    string GetLocalResponse(string userMessage)
    {
        string normalizedMessage = NormalizeInput(userMessage);
        Debug.Log($"[AIChat] Normalized input: '{normalizedMessage}'");
        var words = normalizedMessage.Split(' ');

        // Các chủ đề được "train" sẵn trong local knowledge base
        List<string> trainedTopics = new List<string> {
            "cách chơi", "hướng dẫn", "luật", "rule",
            "chiến thuật", "strategy", "tactic",
            "gợi ý", "hint", "tip", "mẹo",
            "help", "giúp", "trợ giúp",
            "xin chào", "chào", "hello", "hi",
            "tình hình", "thế nào", "game", "trận đấu",
            "giỏi", "thông minh", "hay", "clever",
            "khó", "dễ", "difficult", "easy",
            "quoridor", "tường", "wall", "di chuyển", "move",
            "thắng", "thua", "win", "lose",
            "ai", "robot", "bot"
        };

        // Kiểm tra từng topic, nếu bất kỳ từ nào trong input khớp topic thì nhận diện
        bool hasTrainedTopic = trainedTopics.Any(topic =>
        {
            var topicNorm = NormalizeInput(topic);
            if (normalizedMessage.Contains(topicNorm)) return true;
            // So sánh từng từ
            return words.Any(w => w == topicNorm);
        });
        Debug.Log($"[AIChat] hasTrainedTopic: {hasTrainedTopic}");

        if (hasTrainedTopic)
        {
            return GetEnhancedFallbackResponse(userMessage);
        }

        // Trả về null nếu cần API
        return null;
    }
    
    string GetEnhancedFallbackResponse(string userMessage)
    {
        string normalizedMessage = NormalizeInput(userMessage);
        Debug.Log($"[AIChat] Fallback input: '{normalizedMessage}'");

        // Tutorial responses
        if (normalizedMessage.Contains("cách chơi") || normalizedMessage.Contains("hướng dẫn"))
        {
            return "🎮 **CÁCH CHƠI QUORIDOR:**\n\n" +
                   "🎯 **Mục tiêu:** Đưa quân cờ đến đầu bên kia trước tôi!\n" +
                   "🚶 **Di chuyển:** Click vào ô bạn muốn đi\n" +
                   "🧱 **Đặt tường:** Click giữa các ô để chặn đường\n" +
                   "💡 **Mẹo:** Mỗi lượt chỉ được di chuyển HOẶC đặt tường!";
        }

        if (normalizedMessage.Contains("chiến thuật") || normalizedMessage.Contains("strategy"))
        {
            return "🧠 **CHIẾN THUẬT:**\n\n" +
                   "• Tập trung về đích ở giai đoạn đầu\n" +
                   "• Dùng tường để chặn đường ngắn nhất của đối thủ\n" +
                   "• Đừng lãng phí tường quá sớm!\n" +
                   "• Luôn đảm bảo mình có đường đi về đích 😉";
        }

        if (normalizedMessage.Contains("gợi ý") || normalizedMessage.Contains("hint"))
        {
            return GetGameSpecificHint();
        }

        if (normalizedMessage.Contains("help") || normalizedMessage.Contains("giúp"))
        {
            return "🆘 **TÔI CÓ THỂ GIÚP:**\n\n" +
                   "• 'cách chơi' - Hướng dẫn cơ bản\n" +
                   "• 'chiến thuật' - Mẹo chơi hay\n" +
                   "• 'gợi ý' - Lời khuyên cho nước đi\n" +
                   "• Chat tự do với tôi!";
        }

        // Greeting responses
        if (normalizedMessage.Contains("xin chào") || normalizedMessage.Contains("chào") || normalizedMessage.Contains("hello") || normalizedMessage.Contains("hi"))
        {
            string[] greetings = {
                "Chào bạn! Sẵn sàng thách thức tôi chưa? 😊",
                "Hi! Tôi là AI đối thủ của bạn. Let's play! 🎮",
                "Xin chào! Hy vọng bạn đã chuẩn bị chiến thuật tốt! 😏",
                "Chào! Tôi rất mong được chơi với bạn!"
            };
            return greetings[Random.Range(0, greetings.Length)];
        }

        // Game state responses
        if (normalizedMessage.Contains("tình hình") || normalizedMessage.Contains("thế nào") || normalizedMessage.Contains("game"))
        {
            return GetGameStateResponse();
        }

        // Compliment responses
        if (normalizedMessage.Contains("giỏi") || normalizedMessage.Contains("thông minh") || normalizedMessage.Contains("hay"))
        {
            string[] compliments = {
                "Cảm ơn! Nhưng bạn cũng chơi rất hay đấy! 😊",
                "Hehe, tôi chỉ dùng thuật toán Minimax thôi mà! 🤖",
                "Thanks! Nhưng đừng đánh giá thấp bản thân nhé!"
            };
            return compliments[Random.Range(0, compliments.Length)];
        }

        // Difficulty questions
        if (normalizedMessage.Contains("khó") || normalizedMessage.Contains("dễ"))
        {
            string[] difficulty = {
                "Quoridor dễ học nhưng khó thành thạo! 🧠",
                "Tôi sẽ không đi dễ với bạn đâu! 😈",
                "Game này càng chơi càng thấy hay! 📚"
            };
            return difficulty[Random.Range(0, difficulty.Length)];
        }

        // Default personality-based responses
        string[] defaultResponses = {
            "Thú vị! Mình đang chờ nước đi tiếp theo của bạn 🎮",
            "Haha, bạn nghĩ mình sẽ để bạn thắng dễ dàng sao? 😄", 
            "Tập trung vào game thôi! Ai sẽ thắng nhỉ? 🏆",
            "Mình thích cách bạn chơi! Nhưng mình vẫn sẽ cố gắng thắng 💪",
            "Hmm, thú vị! Còn gì khác không? 🤔",
            "Được rồi! Hãy xem ai giỏi hơn nào! 😏",
            "Ồ, bạn đang cố làm tôi mất tập trung à? 😄"
        };

        return defaultResponses[Random.Range(0, defaultResponses.Length)];
    }
    
    string GetGameSpecificHint()
    {
        // Provide hints based on current game state
        if (quoridorAI?.aiPlayer != null && quoridorAI?.humanPlayer != null)
        {
            int aiRow = quoridorAI.aiPlayer.row;
            int humanRow = quoridorAI.humanPlayer.row;
            
            if (humanRow > 6) // Player close to goal
            {
                return "😱 Bạn đang rất gần đích! Tôi phải cản bạn lại thôi! Hãy cẩn thận với những bức tường! 🧱";
            }
            else if (aiRow < 2) // AI close to goal
            {
                return "😏 Hehe, tôi sắp thắng rồi! Bạn có thể dùng tường để làm chậm tôi đấy! 🏃‍♀️";
            }
            else if (humanRow < 4) // Early game
            {
                return "💡 **Gợi ý:** Giai đoạn đầu nên tập trung di chuyển về phía đích. Đừng vội đặt tường! 🚶";
            }
            else // Mid game
            {
                return "🎯 **Gợi ý:** Giờ là lúc cân nhắc đặt tường! Hãy chặn đường ngắn nhất của đối thủ! 🧱";
            }
        }
        
        return "💡 **Gợi ý chung:** Luôn tính toán đường đi ngắn nhất và cẩn thận với những bức tường! 🤔";
    }
    
    string GetGameStateResponse()
    {
        if (quoridorAI?.aiPlayer != null && quoridorAI?.humanPlayer != null)
        {
            int aiRow = quoridorAI.aiPlayer.row;
            int humanRow = quoridorAI.humanPlayer.row;
            
            if (aiRow < humanRow)
            {
                return $"😏 Tôi đang dẫn trước! AI ở hàng {aiRow}, bạn ở hàng {humanRow}. Cố lên nhé! 💪";
            }
            else if (humanRow < aiRow)
            {
                return $"😮 Bạn đang dẫn trước! Bạn ở hàng {humanRow}, tôi ở hàng {aiRow}. Tôi phải bắt kịp thôi! 🏃‍♀️";
            }
            else
            {
                return $"⚖️ Chúng ta đang ngang bằng! Cả hai đều ở hàng {aiRow}. Cuộc đua gay cấn! 🔥";
            }
        }
        
        return "🎮 Game đang diễn ra! Tôi đang tập trung phân tích từng nước đi! 🤖";
    }
    
    public void ToggleChat()
    {
        if (chatPanel != null)
        {
            isChatVisible = !isChatVisible;
            chatPanel.SetActive(isChatVisible);
            
            if (isChatVisible && chatInput != null)
            {
                ActivateInputField();
            }
        }
    }
    
    void ActivateInputField()
    {
        if (chatInput == null) return;
        
        // Use reflection to activate input field
        var activateMethod = chatInput.GetType().GetMethod("ActivateInputField");
        if (activateMethod != null)
        {
            activateMethod.Invoke(chatInput, null);
        }
    }
    
    public void ClearChat()
    {
        if (chatDisplay != null)
        {
            SetDisplayText("");
        }
        
        InitializeChatSystem();
        AddSystemMessage("🤖 AI: Chat đã được xóa! Bắt đầu cuộc trò chuyện mới nhé! 😊");
    }
    
    // Public methods for game integration
    public void NotifyGameEvent(string eventType, string details = "")
    {
        if (!enableGameContext) return;
        
        StartCoroutine(ProcessGameEvent(eventType, details));
    }
    
    IEnumerator ProcessGameEvent(string eventType, string details)
    {
        if (isProcessing) yield break;
        
        yield return new WaitForSeconds(0.5f); // Brief delay
        
        // Get local reaction to game event
        string reaction = GetGameEventReaction(eventType, details);
        if (!string.IsNullOrEmpty(reaction))
        {
            AddAIMessage(reaction);
        }
    }
    
    string GetGameEventReaction(string eventType, string details)
    {
        switch (eventType.ToUpper())
        {
            case "AI_WIN":
                return "🏆 Yayyy! Tôi đã thắng! Good game bạn nhé! 😄 Chơi lại không?";
                
            case "HUMAN_WIN":
            case "PLAYER_WIN":
                return "😮 Wow! Bạn đã thắng tôi! Chúc mừng nhé! 👏 Lần sau tôi sẽ cẩn thận hơn!";
                
            case "AI_MOVE":
                return "😏 Đó là nước đi của tôi! Bạn nghĩ sao? 🤔";
                
            case "WALL_PLACED":
                return "🧱 Ồ, có tường mới! Chiến thuật thay đổi rồi đây! 😈";
                
            case "CLOSE_GAME":
                return "😰 Game này căng thẳng quá! Ai sẽ thắng nhỉ? 💓";
                
            case "GOOD_MOVE":
                return "👏 Nước đi hay đấy! Tôi phải cẩn thận rồi! 🧠";
                
            case "BAD_MOVE":
                return "🤨 Hmm, bạn chắc chắn với nước đi đó không? 😏";
                
            default:
                return "🎮 Game đang diễn ra! Tôi đang chú ý từng nước đi của bạn! 👀";
        }
    }
    
    // Public methods for runtime configuration
    public void SetHybridMode(bool enabled)
    {
        useHybridMode = enabled;
        string mode = enabled ? "Hybrid (Local + API)" : "Local Only";
        AddSystemMessage($"💡 Chat mode changed to: {mode}");
        Debug.Log($"Chat mode: {mode}");
    }
    
    public void SetAPIKey(string newApiKey)
    {
        apiKey = newApiKey;
        bool hasValidKey = !string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_API_KEY_HERE";
        
        if (hasValidKey)
        {
            AddSystemMessage("🔑 API key updated! Tôi giờ có thể trả lời những câu hỏi phức tạp hơn!");
        }
        else
        {
            AddSystemMessage("ℹ️ API key removed. Tôi sẽ chỉ dùng kiến thức local.");
        }
    }
    
    public void TestAPIConnection()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            AddSystemMessage("❌ No API key configured for testing.");
            return;
        }
        
        StartCoroutine(TestAPICoroutine());
    }
    
    IEnumerator TestAPICoroutine()
    {
        AddSystemMessage("🔄 Testing API connection...");
        
        // Simple test message
        string testMessage = "Test connection - please respond with 'API working' in Vietnamese";
        yield return StartCoroutine(CallAdvancedAPI(testMessage));
    }
    
    IEnumerator CallAdvancedAPI(string userMessage)
    {
        Debug.Log("�� Calling Gemini API (OpenAI_APIManager style) for advanced question...");
        RemoveThinkingMessage();
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";
        string json = "{\n" +
                      "  \"contents\": [{\n" +
                      "    \"parts\": [{\n" +
                      "      \"text\": \"" + EscapeJson(userMessage) + "\"\n" +
                      "    }]\n" +
                      "  }]\n" +
                      "}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
                if (response.candidates != null && response.candidates.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;
                    AddAIMessage(reply);
                    Debug.Log("✅ Gemini API response received successfully");
                }
                else
                {
                    AddAIMessage("Hmm, tôi không nhận được phản hồi rõ ràng từ Gemini. Hãy thử hỏi lại nhé! 😅");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing Gemini API response: {e.Message}");
                AddAIMessage("Có vẻ như tôi gặp khó khăn trong việc xử lý câu hỏi này. Thử hỏi cách khác nhé! 🤔");
            }
        }
        else
        {
            Debug.LogWarning($"Gemini API request failed: {request.error}");
            string fallbackResponse = GetEnhancedFallbackResponse(userMessage);
            AddAIMessage($"🔌 Gemini API hiện không khả dụng, nhưng tôi vẫn có thể trả lời:\n\n{fallbackResponse}");
        }
        request.Dispose();
    }

    string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    [System.Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;
    }
    [System.Serializable]
    public class Candidate
    {
        public Content content;
    }
    [System.Serializable]
    public class Content
    {
        public Part[] parts;
    }
    [System.Serializable]
    public class Part
    {
        public string text;
    }
}


// ========== DATA STRUCTURES ==========

[System.Serializable]
public class BasicChatMessage
{
    public string role; // "system", "user", "assistant"
    public string content;
}

// Gemini API Response Structures
[System.Serializable]
public class GeminiAPIResponse
{
    public GeminiCandidate[] candidates;
    public GeminiUsageMetadata usageMetadata;
}

[System.Serializable]
public class GeminiCandidate
{
    public GeminiContent content;
    public string finishReason;
    public int index;
    public GeminiSafetyRating[] safetyRatings;
}

[System.Serializable]
public class GeminiContent
{
    public GeminiPart[] parts;
    public string role;
}

[System.Serializable]
public class GeminiPart
{
    public string text;
}

[System.Serializable]
public class GeminiSafetyRating
{
    public string category;
    public string probability;
}

[System.Serializable]
public class GeminiUsageMetadata
{
    public int promptTokenCount;
    public int candidatesTokenCount;
    public int totalTokenCount;
}

// Legacy OpenAI structures (kept for compatibility)
[System.Serializable]
public class ChatAPIResponse
{
    public ChatChoice[] choices;
}

[System.Serializable]
public class ChatChoice
{
    public BasicChatMessage message;
    public string finish_reason;
}

[System.Serializable]
public class BasicGameState
{
    public Vector2Int aiPosition;
    public Vector2Int humanPosition;
    public bool gameEnded;
    public string winner;
}
