using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text; // Added for StringBuilder
using System.Text.RegularExpressions; // Added for Regex

/// <summary>
/// Simple Chatbot System - Hệ thống chatbot hoàn chỉnh với UI dễ dùng
/// Model tự train cho Quoridor + API cho tán gẫu
/// </summary>
public class SimpleChatbotSystem : MonoBehaviour
{
    [Header("🎯 SIMPLE CHATBOT SYSTEM")]
    [Tooltip("Nhấn để tạo hệ thống chatbot hoàn chỉnh")]
    public bool setupChatbot = false;

    [Header("🎨 UI Settings")]
    public Vector2 chatWindowSize = new Vector2(450, 600);
    public Vector2 chatWindowPosition = new Vector2(-250, 0);
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    public Color userMessageColor = new Color(0.2f, 0.6f, 1f, 0.9f);
    public Color aiMessageColor = new Color(0.3f, 0.3f, 0.4f, 0.9f);

    [Header("🤖 AI Settings")]
    public bool useTrainedModel = true;
    public bool useAPIFallback = true;
    public float apiTimeout = 5f;

    private bool isSettingUp = false;
    private SimpleTrainedModel trainedModel;
    private SimpleAPIManager apiManager;

    void OnValidate()
    {
        if (setupChatbot && !isSettingUp)
        {
            setupChatbot = false;
            StartCoroutine(SetupCompleteChatbot());
        }
    }

    [ContextMenu("🚀 Setup Complete Chatbot")]
    public void SetupChatbotMenu()
    {
        if (!isSettingUp)
        {
            StartCoroutine(SetupCompleteChatbot());
        }
    }

    public IEnumerator SetupCompleteChatbot()
    {
        isSettingUp = true;
        Debug.Log("🚀 Setting up Simple Chatbot System...");

        // 1. Xóa hệ thống cũ
        RemoveOldSystem();
        yield return new WaitForSeconds(0.1f);

        // 2. Tạo UI
        CreateCanvasIfNeeded();
        yield return new WaitForEndOfFrame();
        CreateEventSystemIfNeeded();
        yield return new WaitForEndOfFrame();
        GameObject chatPanel = CreateChatPanel();
        yield return new WaitForEndOfFrame();
        CreateChatHeader(chatPanel.transform);
        yield return new WaitForEndOfFrame();
        GameObject scrollView = CreateChatScrollView(chatPanel.transform);
        yield return new WaitForEndOfFrame();
        GameObject inputArea = CreateInputArea(chatPanel.transform);
        yield return new WaitForEndOfFrame();
        CreateToggleButton(chatPanel);
        yield return new WaitForEndOfFrame();

        // 3. Setup AI Components
        SetupAIComponents();
        yield return new WaitForEndOfFrame();

        // 4. Setup Chat Manager
        SetupChatManager(chatPanel, scrollView, inputArea);
        yield return new WaitForEndOfFrame();

        Debug.Log("✅ Simple Chatbot System ready!");
        isSettingUp = false;
    }

    void RemoveOldSystem()
    {
        string[] oldObjects = { "SimpleChatPanel", "ChatToggleButton", "SimpleChatManager" };
        foreach (var name in oldObjects)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null) DestroyImmediate(obj);
        }
        var managers = Object.FindObjectsByType<SimpleChatManager>(FindObjectsSortMode.None);
        foreach (var m in managers) DestroyImmediate(m);
    }

    void CreateCanvasIfNeeded()
    {
        if (FindFirstObjectByType<Canvas>() == null)
        {
            GameObject canvasObj = new GameObject("ChatCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    void CreateEventSystemIfNeeded()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    GameObject CreateChatPanel()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject panel = new GameObject("SimpleChatPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        Image image = panel.AddComponent<Image>();
        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();

        rect.anchorMin = new Vector2(1, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(1, 0.5f);
        rect.sizeDelta = chatWindowSize;
        rect.anchoredPosition = chatWindowPosition;

        image.color = backgroundColor;
        image.raycastTarget = true;

        // Bo góc và shadow
        var outline = panel.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.3f);
        outline.effectDistance = new Vector2(3, -3);

        panel.SetActive(true);
        canvasGroup.alpha = 1f;

        return panel;
    }

    void CreateChatHeader(Transform parent)
    {
        GameObject header = new GameObject("Header");
        header.transform.SetParent(parent, false);

        RectTransform rect = header.AddComponent<RectTransform>();
        Image image = header.AddComponent<Image>();

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(0, 50);
        rect.anchoredPosition = Vector2.zero;

        image.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // Header Text
        GameObject textObj = new GameObject("HeaderText");
        textObj.transform.SetParent(header.transform, false);
        Text text = textObj.AddComponent<Text>();

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 0);
        textRect.offsetMax = new Vector2(-15, 0);

        text.text = "🤖 Quoridor AI Assistant";
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.fontStyle = FontStyle.Bold;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    GameObject CreateChatScrollView(Transform parent)
    {
        GameObject scrollView = new GameObject("ChatScrollView");
        scrollView.transform.SetParent(parent, false);

        RectTransform rect = scrollView.AddComponent<RectTransform>();
        Image image = scrollView.AddComponent<Image>();
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        Mask mask = scrollView.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = new Vector2(10, 60); // Space for input
        rect.offsetMax = new Vector2(-10, -50); // Space for header

        image.color = new Color(0.08f, 0.08f, 0.12f, 0.8f);

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollView.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();

        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = Vector2.zero;
        contentRect.anchoredPosition = Vector2.zero;

        layout.spacing = 8;
        layout.padding = new RectOffset(15, 15, 15, 15);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.viewport = rect;
        scrollRect.scrollSensitivity = 30f;

        return scrollView;
    }

    GameObject CreateInputArea(Transform parent)
    {
        GameObject inputArea = new GameObject("InputArea");
        inputArea.transform.SetParent(parent, false);

        RectTransform rect = inputArea.AddComponent<RectTransform>();
        Image image = inputArea.AddComponent<Image>();
        HorizontalLayoutGroup layout = inputArea.AddComponent<HorizontalLayoutGroup>();

        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.sizeDelta = new Vector2(0, 60);
        rect.anchoredPosition = Vector2.zero;

        image.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);

        layout.spacing = 10;
        layout.padding = new RectOffset(15, 15, 10, 10);
        layout.childAlignment = TextAnchor.MiddleLeft;

        // Input Field
        CreateInputField(inputArea.transform);
        CreateSendButton(inputArea.transform);

        return inputArea;
    }

    void CreateInputField(Transform parent)
    {
        GameObject inputObj = new GameObject("MessageInput");
        inputObj.transform.SetParent(parent, false);

        RectTransform rect = inputObj.AddComponent<RectTransform>();
        Image image = inputObj.AddComponent<Image>();
        InputField input = inputObj.AddComponent<InputField>();
        LayoutElement layout = inputObj.AddComponent<LayoutElement>();

        rect.sizeDelta = new Vector2(300, 40);
        image.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        layout.flexibleWidth = 1;
        layout.minWidth = 200;

        // Placeholder
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(inputObj.transform, false);
        RectTransform placeholderRect = placeholder.AddComponent<RectTransform>();
        Text placeholderText = placeholder.AddComponent<Text>();

        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);

        placeholderText.text = "Hỏi về Quoridor hoặc tán gẫu...";
        placeholderText.fontSize = 13;
        placeholderText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        placeholderText.alignment = TextAnchor.MiddleLeft;
        placeholderText.fontStyle = FontStyle.Italic;
        placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Text Area
        GameObject textArea = new GameObject("Text");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textRect = textArea.AddComponent<RectTransform>();
        Text textComp = textArea.AddComponent<Text>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);

        textComp.text = "";
        textComp.fontSize = 13;
        textComp.color = Color.white;
        textComp.alignment = TextAnchor.MiddleLeft;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        input.targetGraphic = image;
        input.textComponent = textComp;
        input.placeholder = placeholderText;
        input.characterLimit = 500;
    }

    void CreateSendButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("SendButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        Image image = buttonObj.AddComponent<Image>();
        Button button = buttonObj.AddComponent<Button>();
        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();

        rect.sizeDelta = new Vector2(80, 40);
        image.color = new Color(0.2f, 0.6f, 1f, 1f);

        layout.minWidth = 80;
        layout.preferredWidth = 80;

        // Button Text
        GameObject buttonText = new GameObject("Text");
        buttonText.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = buttonText.AddComponent<RectTransform>();
        Text text = buttonText.AddComponent<Text>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        text.text = "💬 Gửi";
        text.fontSize = 13;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        button.targetGraphic = image;
    }

    void CreateToggleButton(GameObject chatPanel)
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject toggleObj = new GameObject("ChatToggleButton");
        toggleObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = toggleObj.AddComponent<RectTransform>();
        Image image = toggleObj.AddComponent<Image>();
        Button button = toggleObj.AddComponent<Button>();

        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(120, 45);
        rect.anchoredPosition = new Vector2(-15, -15);

        image.color = new Color(0.2f, 0.6f, 1f, 0.95f);
        image.raycastTarget = true;

        // Bo góc
        var outline = toggleObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.3f);
        outline.effectDistance = new Vector2(2, -2);

        // Button Text
        GameObject buttonText = new GameObject("Text");
        buttonText.transform.SetParent(toggleObj.transform, false);
        RectTransform textRect = buttonText.AddComponent<RectTransform>();
        Text text = buttonText.AddComponent<Text>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        text.text = "🤖 AI Chat";
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        button.targetGraphic = image;

        // Toggle functionality
        CanvasGroup panelGroup = chatPanel.GetComponent<CanvasGroup>();
        button.onClick.AddListener(() => {
            bool isActive = chatPanel.activeSelf;
            if (panelGroup != null) {
                if (isActive) {
                    panelGroup.alpha = 0f;
                    chatPanel.SetActive(false);
                } else {
                    chatPanel.SetActive(true);
                    panelGroup.alpha = 1f;
                }
            } else {
                chatPanel.SetActive(!isActive);
            }
            text.text = (!isActive) ? "❌ Đóng" : "🤖 AI Chat";
            Debug.Log($"🔘 Chat panel toggled: {(!isActive ? "OPEN" : "CLOSED")}");
        });
    }

    void SetupAIComponents()
    {
        trainedModel = new SimpleTrainedModel();
        trainedModel.Initialize();
        apiManager = new SimpleAPIManager();
        Debug.Log("🤖 AI Components initialized");
    }

    void SetupChatManager(GameObject chatPanel, GameObject scrollView, GameObject inputArea)
    {
        SimpleChatManager chatManager = gameObject.AddComponent<SimpleChatManager>();
        chatManager.trainedModel = trainedModel;
        chatManager.apiManager = apiManager;
        chatManager.inputField = inputArea.transform.Find("MessageInput").GetComponent<InputField>();
        chatManager.sendButton = inputArea.transform.Find("SendButton").GetComponent<Button>();
        chatManager.chatContent = scrollView.transform.Find("Content");
        chatManager.chatScrollRect = scrollView.GetComponent<ScrollRect>();
        chatManager.userMessageColor = userMessageColor;
        chatManager.aiMessageColor = aiMessageColor;
        Debug.Log("✅ Chat Manager setup complete");
    }
}

/// <summary>
/// Simple Trained Model - Model tự train cho Quoridor
/// </summary>
[System.Serializable]
public class SimpleTrainedModel
{
    private Dictionary<string, List<string>> responses = new Dictionary<string, List<string>>();
    private float confidenceThreshold = 0.6f;

    public void Initialize()
    {
        // Quoridor Game Responses
        responses[NormalizeInput("hướng dẫn")] = new List<string> {
            "🎮 **HƯỚNG DẪN CHƠI QUORIDOR:**\n\n" +
            "🎯 **Mục tiêu:** Đưa quân cờ đến đầu bên kia trước đối thủ!\n" +
            "🚶 **Di chuyển:** Click vào ô bạn muốn đi\n" +
            "🧱 **Đặt tường:** Click giữa các ô để chặn đường\n" +
            "💡 **Mẹo:** Mỗi lượt chỉ được di chuyển HOẶC đặt tường!"
        };

        responses[NormalizeInput("cách chơi")] = responses[NormalizeInput("hướng dẫn")];
        responses[NormalizeInput("luật chơi")] = responses[NormalizeInput("hướng dẫn")];
        responses[NormalizeInput("game")] = responses[NormalizeInput("hướng dẫn")];

        responses[NormalizeInput("chiến thuật")] = new List<string> {
            "🧠 **CHIẾN THUẬT QUORIDOR:**\n\n" +
            "• Tập trung về đích ở giai đoạn đầu\n" +
            "• Dùng tường để chặn đường ngắn nhất của đối thủ\n" +
            "• Đừng lãng phí tường quá sớm!\n" +
            "• Luôn đảm bảo mình có đường đi về đích 😉"
        };

        responses[NormalizeInput("strategy")] = responses[NormalizeInput("chiến thuật")];
        responses[NormalizeInput("tactic")] = responses[NormalizeInput("chiến thuật")];

        responses[NormalizeInput("gợi ý")] = new List<string> {
            "💡 **GỢI Ý CHO BẠN:**\n\n" +
            "• Hãy tính toán đường đi ngắn nhất\n" +
            "• Dùng tường để chặn đối thủ\n" +
            "• Đừng bị mắc kẹt!\n" +
            "• Luôn có kế hoạch dự phòng 😊"
        };

        responses[NormalizeInput("hint")] = responses[NormalizeInput("gợi ý")];
        responses[NormalizeInput("tip")] = responses[NormalizeInput("gợi ý")];

        responses[NormalizeInput("help")] = new List<string> {
            "🆘 **TÔI CÓ THỂ GIÚP:**\n\n" +
            "• 'hướng dẫn' - Luật chơi cơ bản\n" +
            "• 'chiến thuật' - Mẹo chơi hay\n" +
            "• 'gợi ý' - Lời khuyên cho nước đi\n" +
            "• Hoặc tán gẫu với tôi! 😄"
        };

        responses[NormalizeInput("xin chào")] = new List<string> {
            "Chào bạn! Tôi là AI hỗ trợ Quoridor. Hãy hỏi tôi bất cứ điều gì! 😊",
            "Hi! Sẵn sàng thách thức tôi chưa? 🎮",
            "Xin chào! Hy vọng bạn đã chuẩn bị chiến thuật tốt! 😏"
        };

        responses[NormalizeInput("hello")] = responses[NormalizeInput("xin chào")];
        responses[NormalizeInput("hi")] = responses[NormalizeInput("xin chào")];

        Debug.Log($"🤖 Simple Trained Model initialized with {responses.Count} patterns");
        Debug.Log("[SimpleChatbot] All response keys: " + string.Join(", ", responses.Keys));
    }

    public string GetResponse(string input)
    {
        if (string.IsNullOrEmpty(input)) return GetDefaultResponse();
        string normalizedInput = NormalizeInput(input);
        Debug.Log($"[SimpleChatbot] Normalized input: '{normalizedInput}'");
        var words = normalizedInput.Split(' ');
        // Ưu tiên trả về hướng dẫn nếu input chứa bất kỳ từ nào trong nhóm này
        string[] guideKeywords = { "hướng dẫn", "cách chơi", "luật chơi", "game" };
        foreach (var kw in guideKeywords)
        {
            var kwNorm = NormalizeInput(kw);
            var key = NormalizeInput("hướng dẫn");
            if ((normalizedInput.Contains(kwNorm) || words.Any(w => w == kwNorm)) && responses.ContainsKey(key))
                return GetRandomResponse(responses[key]);
        }
        float bestScore = 0f;
        string bestResponse = null;
        foreach (var pattern in responses.Keys)
        {
            var patternNorm = NormalizeInput(pattern);
            float score = CalculateSimilarity(normalizedInput, patternNorm);
            if (score > bestScore && score >= confidenceThreshold)
            {
                bestScore = score;
                if (responses.ContainsKey(pattern))
                    bestResponse = GetRandomResponse(responses[pattern]);
            }
        }
        Debug.Log($"[SimpleChatbot] bestScore: {bestScore}, bestResponse: {bestResponse}");
        return bestResponse ?? GetDefaultResponse();
    }

    private string RemoveVietnameseDiacritics(string input)
    {
        string[] vietnameseChars = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };
        string[] replaceChars = new string[]
        {
            "aAeEoOuUiIdDyY",
            "a",
            "A",
            "e",
            "E",
            "o",
            "O",
            "u",
            "U",
            "i",
            "I",
            "d",
            "D",
            "y",
            "Y"
        };
        for (int i = 1; i < vietnameseChars.Length; i++)
        {
            for (int j = 0; j < vietnameseChars[i].Length; j++)
            {
                input = input.Replace(vietnameseChars[i][j], replaceChars[i][0]);
            }
        }
        return input;
    }

    private string NormalizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var normalized = System.Text.RegularExpressions.Regex.Replace(input.ToLower(), @"[^\p{L}\p{N}\s]", "");
        normalized = RemoveVietnameseDiacritics(normalized);
        return normalized.Trim();
    }

    private float CalculateSimilarity(string input, string pattern)
    {
        var inputWords = input.Split(' ').ToHashSet();
        var patternWords = pattern.Split(' ').ToHashSet();
        int intersection = inputWords.Intersect(patternWords).Count();
        int union = inputWords.Union(patternWords).Count();
        return union > 0 ? (float)intersection / union : 0f;
    }

    string GetRandomResponse(List<string> responseList)
    {
        return responseList[Random.Range(0, responseList.Count)];
    }

    string GetDefaultResponse()
    {
        return "Tôi chưa hiểu rõ. Bạn có thể hỏi về 'hướng dẫn', 'chiến thuật', hoặc tán gẫu với tôi! 😊";
    }
}

/// <summary>
/// Simple API Manager - Xử lý câu hỏi ngoài phạm vi
/// </summary>
public class SimpleAPIManager
{
    public string GetResponse(string input)
    {
        // Simulate API response for general conversation
        string lowerInput = input.ToLower();
        
        if (lowerInput.Contains("thời tiết") || lowerInput.Contains("weather"))
        {
            return "🌤️ Hôm nay thời tiết đẹp! Phù hợp để chơi Quoridor nhỉ? 😄";
        }
        
        if (lowerInput.Contains("sức khỏe") || lowerInput.Contains("health"))
        {
            return "💪 Tôi luôn khỏe mạnh và sẵn sàng chơi! Bạn thế nào? 😊";
        }
        
        if (lowerInput.Contains("tên") || lowerInput.Contains("name"))
        {
            return "🤖 Tôi là AI Assistant, chuyên về game Quoridor! Rất vui được gặp bạn! 😄";
        }
        
        if (lowerInput.Contains("tuổi") || lowerInput.Contains("age"))
        {
            return "🎂 Tôi là AI nên không có tuổi thật, nhưng tôi rất trẻ và năng động! 😄";
        }
        
        // Default casual response
        string[] casualResponses = {
            "Thú vị! Bạn có muốn chơi Quoridor không? 🎮",
            "Haha, bạn thật vui tính! Có muốn thử thách tôi không? 😏",
            "Tôi thích trò chuyện với bạn! Có muốn chơi game không? 😊",
            "Bạn thật thú vị! Hãy chơi Quoridor với tôi nhé! 🎯"
        };
        
        return casualResponses[Random.Range(0, casualResponses.Length)];
    }
}

/// <summary>
/// Simple Chat Manager - Quản lý UI và logic chat
/// </summary>
public class SimpleChatManager : MonoBehaviour
{
    [Header("UI References")]
    public InputField inputField;
    public Button sendButton;
    public Transform chatContent;
    public ScrollRect chatScrollRect;
    
    [Header("AI References")]
    public SimpleTrainedModel trainedModel;
    public SimpleAPIManager apiManager;
    
    [Header("Styling")]
    public Color userMessageColor = new Color(0.2f, 0.6f, 1f, 0.9f);
    public Color aiMessageColor = new Color(0.3f, 0.3f, 0.4f, 0.9f);
    
    private bool isProcessing = false;
    
    void Start()
    {
        SetupEvents();
        ShowWelcomeMessage();
    }
    
    void SetupEvents()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendClicked);
            
        if (inputField != null)
            inputField.onEndEdit.AddListener(OnInputEndEdit);
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
        
        AddMessage(userInput, true);
        inputField.text = "";
        
        StartCoroutine(ProcessResponse(userInput));
    }
    
    IEnumerator ProcessResponse(string userInput)
    {
        isProcessing = true;
        
        // Show typing indicator
        AddTypingIndicator();
        yield return new WaitForSeconds(0.5f);
        
        string response = "";
        
        // Try trained model first
        if (trainedModel != null)
        {
            response = trainedModel.GetResponse(userInput);
            if (!IsDefaultResponse(response))
            {
                Debug.Log("📚 Using trained model response");
            }
        }
        
        // If no good response, try API
        if (IsDefaultResponse(response) && apiManager != null)
        {
            Debug.Log("🌐 Using API response");
            response = apiManager.GetResponse(userInput);
        }
        
        // Remove typing indicator
        RemoveTypingIndicator();
        
        AddMessage(response, false);
        
        isProcessing = false;
    }
    
    bool IsDefaultResponse(string response)
    {
        return response.Contains("chưa hiểu") || response.Contains("có thể hỏi");
    }
    
    void AddMessage(string message, bool isUser)
    {
        if (chatContent == null) return;
        
        GameObject messageObj = new GameObject("Message");
        messageObj.transform.SetParent(chatContent, false);
        
        RectTransform rect = messageObj.AddComponent<RectTransform>();
        Image image = messageObj.AddComponent<Image>();
        LayoutElement layout = messageObj.AddComponent<LayoutElement>();
        ContentSizeFitter fitter = messageObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        rect.sizeDelta = new Vector2(0, 0);
        image.color = isUser ? userMessageColor : aiMessageColor;
        layout.preferredHeight = 60;
        
        // Message Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(messageObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        Text text = textObj.AddComponent<Text>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        text.text = (isUser ? "👤 Bạn: " : "🤖 AI: ") + message;
        text.fontSize = 13;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        // Auto scroll
        StartCoroutine(ScrollToBottom());
        // Force layout update to fix message overlap
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent.GetComponent<RectTransform>());
    }
    
    void AddTypingIndicator()
    {
        if (chatContent == null) return;
        
        GameObject typingObj = new GameObject("TypingIndicator");
        typingObj.transform.SetParent(chatContent, false);
        
        RectTransform rect = typingObj.AddComponent<RectTransform>();
        Image image = typingObj.AddComponent<Image>();
        LayoutElement layout = typingObj.AddComponent<LayoutElement>();
        
        rect.sizeDelta = new Vector2(0, 0);
        image.color = aiMessageColor;
        layout.preferredHeight = 40;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(typingObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        Text text = textObj.AddComponent<Text>();
        
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        
        text.text = "🤖 AI đang suy nghĩ...";
        text.fontSize = 12;
        text.color = Color.yellow;
        text.alignment = TextAnchor.MiddleLeft;
        text.fontStyle = FontStyle.Italic;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
    
    void RemoveTypingIndicator()
    {
        if (chatContent == null) return;
        
        for (int i = chatContent.childCount - 1; i >= 0; i--)
        {
            Transform child = chatContent.GetChild(i);
            if (child.name == "TypingIndicator")
            {
                DestroyImmediate(child.gameObject);
                break;
            }
        }
    }
    
    IEnumerator ScrollToBottom()
    {
        yield return null;
        yield return null;
        
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    void ShowWelcomeMessage()
    {
        AddMessage("Xin chào! Tôi là AI Assistant chuyên về Quoridor. Hãy hỏi tôi về 'hướng dẫn', 'chiến thuật', hoặc tán gẫu với tôi! 😊", false);
    }
} 