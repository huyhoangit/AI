using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class ChatUIController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] public TMPro.TMP_InputField inputField;
    [SerializeField] public Button sendButton;
    [SerializeField] public Button voiceButton;
    [SerializeField] public Button ttsButton;
    [SerializeField] public Button langButton;
    [SerializeField] public Transform chatContent;
    [SerializeField] public ScrollRect chatScrollRect;
    [SerializeField] public GameObject userBubblePrefab;
    [SerializeField] public GameObject aiBubblePrefab;

    [Header("Service")]
    public ChatService chatService;

    [Header("Bubble Creator")]
    public MessageBubbleCreator messageBubbleCreator;

    private string currentLang = "vi";
    private string lastAIReply = "";
    public AudioSource audioSource; // Thêm biến này vào class
    private Button testTTSButton;

    void Awake()
    {
        // Tự động tìm các thành phần UI nếu chưa gán
        if (inputField == null) inputField = GameObject.Find("ChatInputField")?.GetComponent<TMPro.TMP_InputField>();
        if (sendButton == null) sendButton = GameObject.Find("SendButton")?.GetComponent<Button>();
        if (voiceButton == null) voiceButton = GameObject.Find("VoiceButton")?.GetComponent<Button>();
        if (ttsButton == null) ttsButton = GameObject.Find("TTSButton")?.GetComponent<Button>();
        if (langButton == null) langButton = GameObject.Find("LangButton")?.GetComponent<Button>();
        if (chatContent == null) chatContent = GameObject.Find("ChatContent")?.transform;
        if (chatScrollRect == null) chatScrollRect = GameObject.Find("ChatScrollView")?.GetComponent<ScrollRect>();
        if (chatService == null) chatService = FindFirstObjectByType<ChatService>();
    }

    void Start()
    {
        if (sendButton != null) sendButton.onClick.AddListener(OnSendClicked);
        if (voiceButton != null) voiceButton.onClick.AddListener(OnVoiceClicked);
        if (ttsButton != null) ttsButton.onClick.AddListener(OnTTSClicked);
        if (langButton != null) langButton.onClick.AddListener(OnLangClicked);
        ShowWelcomeMessage();
        // CreateTestTTSButton();
    }

    public void OnSendClicked()
    {
        // Kiểm tra null safety
        if (inputField == null)
        {
            Debug.LogError("❌ Input Field is null! Make sure UI is properly set up.");
            return;
        }
        
        if (chatService == null)
        {
            Debug.LogError("❌ Chat Service is null! Make sure ChatService component is added.");
            return;
        }
        
        string userInput = inputField.text.Trim();
        if (string.IsNullOrEmpty(userInput)) return;
        
        AddMessage(userInput, true);
        inputField.text = "";
        
        chatService.SendMessageToBot(userInput, (reply) => {
            lastAIReply = reply;
            AddMessage(reply, false);
        });
    }

    public void OnVoiceClicked()
    {
        // Demo: giả lập voice bằng text (có thể tích hợp Microphone sau)
        AddMessage("[Voice] Tính năng ghi âm demo", true);
        // Gửi file audio thực tế: chatService.SendVoiceToBot(audioBytes, ...)
    }

    public void OnTTSClicked()
    {
        if (!string.IsNullOrEmpty(lastAIReply))
        {
            chatService.RequestTTS(lastAIReply, (audio) => {
                if (audio != null)
                {
                    // Phát audio thực sự
                    AudioClip clip = WavUtility.ToAudioClip(audio, 0, "TTSClip");
                    if (audioSource == null)
                        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("[TTS] Đã nhận audio và phát lại!");
                }
                else
                {
                    Debug.LogWarning("[TTS] Không nhận được audio!");
                }
            });
        }
    }

    public void OnLangClicked()
    {
        currentLang = (currentLang == "vi") ? "en" : "vi";
        chatService.ChuyenNgonNgu(currentLang);
        if (langButton != null)
        {
            var txt = langButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = currentLang == "vi" ? "🌐 VI/EN" : "🌐 EN/VI";
        }
        AddMessage($"[Đã chuyển ngôn ngữ sang {(currentLang == "vi" ? "Tiếng Việt" : "English")}]", false);
    }

    void AddMessage(string message, bool isUser)
    {
        if (messageBubbleCreator != null && chatContent != null)
        {
            // Sử dụng method tạo bubble với kích thước cố định
            messageBubbleCreator.CreateFixedSizeBubble(message, chatContent, isUser);
        }
        else
        {
            GameObject prefab = isUser ? userBubblePrefab : aiBubblePrefab;
            GameObject bubble = null;
            if (prefab != null)
            {
                bubble = Instantiate(prefab, chatContent);
                var txt = bubble.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = message;
                // Căn chỉnh bubble prefab nếu có RectTransform
                RectTransform rectTransform = bubble.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = isUser ? new Vector2(1, 0) : new Vector2(0, 0);
                    rectTransform.anchorMax = isUser ? new Vector2(1, 1) : new Vector2(0, 1);
                    rectTransform.pivot = isUser ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
                }
                // Đảm bảo alignment đúng kiểu
                if (txt != null)
                    txt.alignment = isUser ? TMPro.TextAlignmentOptions.MidlineRight : TMPro.TextAlignmentOptions.MidlineLeft;
            }
            else
            {
                GameObject msgObj = new GameObject(isUser ? "UserMsg" : "AIMsg");
                msgObj.transform.SetParent(chatContent, false);
                var txt = msgObj.AddComponent<TextMeshProUGUI>();
                txt.text = message;
                txt.fontSize = 18;
                txt.color = isUser ? new Color(0.2f, 0.6f, 1f, 1f) : Color.black;
                txt.alignment = (TMPro.TextAlignmentOptions)(isUser ? TextAlignmentOptions.Right : TextAlignmentOptions.Left);
                // Thêm điều chỉnh vị trí bong bóng
                RectTransform rectTransform = msgObj.GetComponent<RectTransform>();
                rectTransform.anchorMin = isUser ? new Vector2(1, 0) : new Vector2(0, 0);
                rectTransform.anchorMax = isUser ? new Vector2(1, 1) : new Vector2(0, 1);
                rectTransform.pivot = isUser ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
            }
        }
        if (chatScrollRect != null)
            chatScrollRect.verticalNormalizedPosition = 0f;
    }

    void ShowWelcomeMessage()
    {
        AddMessage("Xin chào! Đây là demo Chatbot Messenger-style, hỗ trợ đa ngôn ngữ, voice, TTS, LLM, Rasa.", false);
    }

    private void CreateTestTTSButton()
    {
        // Tìm Canvas
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas trong scene!");
            return;
        }

        GameObject btnObj = new GameObject("TestTTSButton");
        btnObj.transform.SetParent(canvas.transform, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.sizeDelta = new Vector2(120, 40);
        rect.anchoredPosition = new Vector2(-20, 60);

        Button btn = btnObj.AddComponent<Button>();
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f, 1f);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        var txt = txtObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = "Test TTS";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.color = Color.white;
        txt.fontSize = 18;

        btn.onClick.AddListener(OnTestTTSButtonClicked);
        testTTSButton = btn;
    }

    private void OnTestTTSButtonClicked()
    {
        Debug.Log("[TestTTS] Đã ấn nút test TTS!");
        string testText = "Xin chào! Đây là test phát âm thanh từ AI.";
        if (chatService != null)
        {
            chatService.RequestTTS(testText, (audio) => {
                Debug.Log("[TestTTS] Callback nhận audio: " + (audio != null));
                if (audio != null)
                {
                    AudioClip clip = WavUtility.ToAudioClip(audio, 0, "TTSClipTest");
                    if (audioSource == null)
                        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("[TestTTS] Đã phát audio test!");
                }
                else
                {
                    Debug.LogWarning("[TestTTS] Không nhận được audio!");
                }
            });
        }
        else
        {
            Debug.LogError("[TestTTS] ChatService bị null!");
        }
    }

    [ContextMenu("Debug UI Components")]
    public void DebugUIComponents()
    {
        Debug.Log("=== DEBUG UI COMPONENTS ===");
        Debug.Log($"Input Field: {(inputField != null ? "✅ Found" : "❌ NULL")}");
        Debug.Log($"Send Button: {(sendButton != null ? "✅ Found" : "❌ NULL")}");
        Debug.Log($"Chat Content: {(chatContent != null ? "✅ Found" : "❌ NULL")}");
        Debug.Log($"Chat Service: {(chatService != null ? "✅ Found" : "❌ NULL")}");
        Debug.Log($"Voice Button: {(voiceButton != null ? "✅ Found" : "❌ NULL")}");
        Debug.Log($"TTS Button: {(ttsButton != null ? "✅ Found" : "❌ NULL")}");
        Debug.Log($"Lang Button: {(langButton != null ? "✅ Found" : "❌ NULL")}");
        Debug.Log($"Scroll Rect: {(chatScrollRect != null ? "✅ Found" : "❌ NULL")}");
        
        if (inputField == null)
        {
            Debug.LogWarning("⚠️ Input Field is missing! You need to:");
            Debug.LogWarning("1. Create UI using SimpleChatUICreator script");
            Debug.LogWarning("2. Or manually assign TMP_InputField in Inspector");
        }
    }
    
    [ContextMenu("Auto Setup UI")]
    public void AutoSetupUI()
    {
        // Tìm hoặc tạo SimpleChatUICreator
        var uiCreator = FindFirstObjectByType<SimpleChatUICreator>();
        if (uiCreator == null)
        {
            GameObject creatorObj = new GameObject("SimpleChatUICreator");
            uiCreator = creatorObj.AddComponent<SimpleChatUICreator>();
            Debug.Log("✅ Created SimpleChatUICreator");
        }
        
        // Tạo UI
        uiCreator.CreateMessengerChatUI();
        Debug.Log("✅ UI Created! Now re-run Awake to find components...");
        
        // Re-run Awake logic
        Awake();
        DebugUIComponents();
    }
}