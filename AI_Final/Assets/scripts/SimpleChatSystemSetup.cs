using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple Chat System Setup - Creates basic chat UI without complex TMP_InputField setup
/// </summary>
public class SimpleChatSystemSetup : MonoBehaviour
{
    [Header("UI Settings")]
    public Vector2 canvasSize = new Vector2(400, 600);
    public Color backgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    
    void Start()
    {
        CreateSimpleChatSystem();
    }
    
    [ContextMenu("Create Simple Chat System")]
    public void CreateSimpleChatSystem()
    {
        Debug.Log("🚀 Creating simple chat system...");
        
        // Create Canvas
        Canvas canvas = CreateCanvas();
        
        // Create Chat Panel
        GameObject chatPanel = CreateChatPanel(canvas.transform);
        
        // Create Input Panel
        GameObject inputPanel = CreateInputPanel(canvas.transform);
        
        // Create ChatUIController
        ChatUIController chatController = CreateChatUIController();
        
        // Create ChatService
        ChatService chatService = CreateChatService();
        
        // Create MessageBubbleCreator
        MessageBubbleCreator bubbleCreator = CreateMessageBubbleCreator();
        
        // Connect everything
        ConnectComponents(chatController, chatService, bubbleCreator, chatPanel, inputPanel);
        
        Debug.Log("✅ Simple chat system created!");
    }
    
    Canvas CreateCanvas()
    {
        // Find existing canvas or create new one
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("✅ Using existing Canvas");
            return existingCanvas;
        }
        
        GameObject canvasObj = new GameObject("ChatCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("✅ Created new Canvas");
        return canvas;
    }
    
    GameObject CreateChatPanel(Transform parent)
    {
        GameObject chatPanel = new GameObject("ChatPanel");
        chatPanel.transform.SetParent(parent, false);
        
        // Add Image component for background
        Image panelImage = chatPanel.AddComponent<Image>();
        panelImage.color = backgroundColor;
        
        // Set RectTransform
        RectTransform rectTransform = chatPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0.2f);
        rectTransform.anchorMax = new Vector2(1, 0.9f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Create ScrollView
        GameObject scrollView = CreateScrollView(chatPanel.transform);
        
        Debug.Log("✅ Created Chat Panel");
        return chatPanel;
    }
    
    GameObject CreateScrollView(Transform parent)
    {
        GameObject scrollView = new GameObject("ChatScrollView");
        scrollView.transform.SetParent(parent, false);
        
        // Add ScrollRect
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        
        // Set RectTransform
        RectTransform rectTransform = scrollView.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Create Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.clear;
        
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        // Create ChatContent
        GameObject chatContent = new GameObject("ChatContent");
        chatContent.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = chatContent.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        
        // Connect ScrollRect
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        
        Debug.Log("✅ Created ScrollView with ChatContent");
        return scrollView;
    }
    
    GameObject CreateInputPanel(Transform parent)
    {
        GameObject inputPanel = new GameObject("InputPanel");
        inputPanel.transform.SetParent(parent, false);
        
        // Add Image component for background
        Image panelImage = inputPanel.AddComponent<Image>();
        panelImage.color = backgroundColor;
        
        // Set RectTransform
        RectTransform rectTransform = inputPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 0.2f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Create Button Panel
        GameObject buttonPanel = CreateButtonPanel(inputPanel.transform);
        
        Debug.Log("✅ Created Input Panel");
        return inputPanel;
    }
    
    GameObject CreateButtonPanel(Transform parent)
    {
        GameObject buttonPanel = new GameObject("ButtonPanel");
        buttonPanel.transform.SetParent(parent, false);
        
        // Set RectTransform
        RectTransform rectTransform = buttonPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Create Send Button
        GameObject sendButton = CreateButton("SendButton", "Gửi", buttonPanel.transform, new Vector2(0, 0.5f), new Vector2(0.3f, 0.8f));
        
        // Create TTS Button
        GameObject ttsButton = CreateButton("TTSButton", "🔊 TTS", buttonPanel.transform, new Vector2(0.35f, 0.5f), new Vector2(0.65f, 0.8f));
        
        // Create Voice Button
        GameObject voiceButton = CreateButton("VoiceButton", "🎤", buttonPanel.transform, new Vector2(0.7f, 0.5f), new Vector2(0.85f, 0.8f));
        
        // Create Lang Button
        GameObject langButton = CreateButton("LangButton", "🌐", buttonPanel.transform, new Vector2(0.9f, 0.5f), new Vector2(1, 0.8f));
        
        Debug.Log("✅ Created Button Panel");
        return buttonPanel;
    }
    
    GameObject CreateButton(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);
        
        // Add Button component
        Button buttonComponent = button.AddComponent<Button>();
        
        // Add Image component
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        // Set RectTransform
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = new Vector2(2, 2);
        rectTransform.offsetMax = new Vector2(-2, -2);
        
        // Create Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 14;
        textComponent.color = Color.white;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log($"✅ Created {name} button");
        return button;
    }
    
    ChatUIController CreateChatUIController()
    {
        GameObject controllerObj = new GameObject("ChatUIController");
        ChatUIController controller = controllerObj.AddComponent<ChatUIController>();
        Debug.Log("✅ Created ChatUIController");
        return controller;
    }
    
    ChatService CreateChatService()
    {
        GameObject serviceObj = new GameObject("ChatService");
        ChatService service = serviceObj.AddComponent<ChatService>();
        Debug.Log("✅ Created ChatService");
        return service;
    }
    
    MessageBubbleCreator CreateMessageBubbleCreator()
    {
        GameObject creatorObj = new GameObject("MessageBubbleCreator");
        MessageBubbleCreator creator = creatorObj.AddComponent<MessageBubbleCreator>();
        Debug.Log("✅ Created MessageBubbleCreator");
        return creator;
    }
    
    void ConnectComponents(ChatUIController controller, ChatService service, MessageBubbleCreator creator, GameObject chatPanel, GameObject inputPanel)
    {
        // Connect ChatService
        controller.chatService = service;
        
        // Connect MessageBubbleCreator
        controller.messageBubbleCreator = creator;
        
        // Find UI components
        controller.sendButton = inputPanel.transform.Find("ButtonPanel/SendButton")?.GetComponent<Button>();
        controller.ttsButton = inputPanel.transform.Find("ButtonPanel/TTSButton")?.GetComponent<Button>();
        controller.voiceButton = inputPanel.transform.Find("ButtonPanel/VoiceButton")?.GetComponent<Button>();
        controller.langButton = inputPanel.transform.Find("ButtonPanel/LangButton")?.GetComponent<Button>();
        controller.chatContent = chatPanel.transform.Find("ChatScrollView/Viewport/ChatContent");
        controller.chatScrollRect = chatPanel.transform.Find("ChatScrollView")?.GetComponent<ScrollRect>();
        
        // Connect button events
        if (controller.sendButton != null)
            controller.sendButton.onClick.AddListener(controller.OnSendClicked);
        if (controller.ttsButton != null)
            controller.ttsButton.onClick.AddListener(controller.OnTTSClicked);
        if (controller.voiceButton != null)
            controller.voiceButton.onClick.AddListener(controller.OnVoiceClicked);
        if (controller.langButton != null)
            controller.langButton.onClick.AddListener(controller.OnLangClicked);
        
        Debug.Log("✅ Connected all components");
    }
    
    [ContextMenu("Test TTS Button Connection")]
    public void TestTTSButtonConnection()
    {
        Button ttsButton = GameObject.Find("TTSButton")?.GetComponent<Button>();
        if (ttsButton != null)
        {
            Debug.Log("🔍 TTS Button found!");
            Debug.Log($"Button interactable: {ttsButton.interactable}");
            Debug.Log($"Button onClick listeners count: {ttsButton.onClick.GetPersistentEventCount()}");
            
            // Test click
            ttsButton.onClick.Invoke();
            Debug.Log("✅ TTS Button click test completed");
        }
        else
        {
            Debug.LogError("❌ TTS Button not found!");
        }
    }
} 