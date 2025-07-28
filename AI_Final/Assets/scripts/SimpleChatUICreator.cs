using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tạo UI chat Messenger-style tự động trong Editor hoặc Playmode
/// </summary>
public class SimpleChatUICreator : MonoBehaviour
{
    [ContextMenu("Create Messenger Chat UI")]
    public void CreateMessengerChatUI()
    {
        // Tạo Canvas nếu chưa có
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MessengerCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Tạo Panel nền chat
        GameObject panel = new GameObject("ChatPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(500, 700);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.95f, 0.95f, 0.98f, 1f);

        // Tạo ScrollView cho chat
        GameObject scrollObj = new GameObject("ChatScrollView");
        scrollObj.transform.SetParent(panel.transform, false);
        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0.15f);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        Image scrollImg = scrollObj.AddComponent<Image>();
        scrollImg.color = new Color(1, 1, 1, 0.0f);

        // Content cho ScrollView
        GameObject contentObj = new GameObject("ChatContent");
        contentObj.transform.SetParent(scrollObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = new Vector2(10, 10);
        contentRect.offsetMax = new Vector2(-10, -10);
        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 8f;
        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRect;

        // Tạo input field
        GameObject inputObj = new GameObject("ChatInputField");
        inputObj.transform.SetParent(panel.transform, false);
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 0);
        inputRect.anchorMax = new Vector2(0.7f, 0.13f);
        inputRect.offsetMin = new Vector2(10, 10);
        inputRect.offsetMax = new Vector2(-10, -10);
        inputObj.AddComponent<CanvasRenderer>();
        Image inputImg = inputObj.AddComponent<Image>();
        inputImg.color = new Color(1, 1, 1, 1f);
        TMPro.TMP_InputField inputField = inputObj.AddComponent<TMPro.TMP_InputField>();
        
        // Tạo viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(inputObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);
        viewportObj.AddComponent<CanvasRenderer>();
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        Image viewportImg = viewportObj.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0f);
        // Gán viewport cho TMP_InputField (sử dụng thuộc tính đúng)
        System.Reflection.FieldInfo viewportField = typeof(TMPro.TMP_InputField).GetField("m_TextViewport", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (viewportField != null) viewportField.SetValue(inputField, viewportRect);
        
        // Tạo Text (child của viewport)
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(viewportObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 18;
        text.color = Color.black;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        // Gán text component cho TMP_InputField (sử dụng thuộc tính đúng)
        System.Reflection.FieldInfo textField = typeof(TMPro.TMP_InputField).GetField("m_TextComponent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (textField != null) textField.SetValue(inputField, text);
        // Tạo Placeholder (child của viewport)
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(viewportObj.transform, false);
        TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Nhập tin nhắn...";
        placeholderText.fontSize = 18;
        placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        // Gán placeholder cho TMP_InputField (sử dụng reflection hoặc thuộc tính đúng)
        System.Reflection.FieldInfo placeholderField = typeof(TMPro.TMP_InputField).GetField("m_Placeholder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (placeholderField != null) placeholderField.SetValue(inputField, placeholderText);

        // Nút gửi
        GameObject sendBtnObj = new GameObject("SendButton");
        sendBtnObj.transform.SetParent(panel.transform, false);
        RectTransform sendBtnRect = sendBtnObj.AddComponent<RectTransform>();
        sendBtnRect.anchorMin = new Vector2(0.72f, 0);
        sendBtnRect.anchorMax = new Vector2(0.82f, 0.13f);
        sendBtnRect.offsetMin = new Vector2(0, 10);
        sendBtnRect.offsetMax = new Vector2(-10, -10);
        Button sendBtn = sendBtnObj.AddComponent<Button>();
        Image sendBtnImg = sendBtnObj.AddComponent<Image>();
        sendBtnImg.color = new Color(0.2f, 0.6f, 1f, 1f);
        GameObject sendTextObj = new GameObject("SendText");
        sendTextObj.transform.SetParent(sendBtnObj.transform, false);
        TextMeshProUGUI sendText = sendTextObj.AddComponent<TextMeshProUGUI>();
        sendText.text = "Gửi";
        sendText.fontSize = 18;
        sendText.color = Color.white;
        sendText.alignment = TMPro.TextAlignmentOptions.Center;
        RectTransform sendTextRect = sendTextObj.GetComponent<RectTransform>();
        sendTextRect.anchorMin = Vector2.zero;
        sendTextRect.anchorMax = Vector2.one;
        sendTextRect.offsetMin = Vector2.zero;
        sendTextRect.offsetMax = Vector2.zero;

        // Nút ghi âm (voice)
        GameObject voiceBtnObj = new GameObject("VoiceButton");
        voiceBtnObj.transform.SetParent(panel.transform, false);
        RectTransform voiceBtnRect = voiceBtnObj.AddComponent<RectTransform>();
        voiceBtnRect.anchorMin = new Vector2(0.84f, 0);
        voiceBtnRect.anchorMax = new Vector2(0.92f, 0.13f);
        voiceBtnRect.offsetMin = new Vector2(0, 10);
        voiceBtnRect.offsetMax = new Vector2(-10, -10);
        Button voiceBtn = voiceBtnObj.AddComponent<Button>();
        Image voiceBtnImg = voiceBtnObj.AddComponent<Image>();
        voiceBtnImg.color = new Color(0.1f, 0.7f, 0.4f, 1f);
        GameObject voiceTextObj = new GameObject("VoiceText");
        voiceTextObj.transform.SetParent(voiceBtnObj.transform, false);
        TextMeshProUGUI voiceText = voiceTextObj.AddComponent<TextMeshProUGUI>();
        voiceText.text = "🎤";
        voiceText.fontSize = 22;
        voiceText.color = Color.white;
        voiceText.alignment = TMPro.TextAlignmentOptions.Center;
        RectTransform voiceTextRect = voiceTextObj.GetComponent<RectTransform>();
        voiceTextRect.anchorMin = Vector2.zero;
        voiceTextRect.anchorMax = Vector2.one;
        voiceTextRect.offsetMin = Vector2.zero;
        voiceTextRect.offsetMax = Vector2.zero;

        // Nút nghe lại (TTS)
        GameObject ttsBtnObj = new GameObject("TTSButton");
        ttsBtnObj.transform.SetParent(panel.transform, false);
        RectTransform ttsBtnRect = ttsBtnObj.AddComponent<RectTransform>();
        ttsBtnRect.anchorMin = new Vector2(0.94f, 0);
        ttsBtnRect.anchorMax = new Vector2(1f, 0.13f);
        ttsBtnRect.offsetMin = new Vector2(0, 10);
        ttsBtnRect.offsetMax = new Vector2(-10, -10);
        Button ttsBtn = ttsBtnObj.AddComponent<Button>();
        Image ttsBtnImg = ttsBtnObj.AddComponent<Image>();
        ttsBtnImg.color = new Color(0.8f, 0.5f, 0.1f, 1f);
        GameObject ttsTextObj = new GameObject("TTSText");
        ttsTextObj.transform.SetParent(ttsBtnObj.transform, false);
        TextMeshProUGUI ttsText = ttsTextObj.AddComponent<TextMeshProUGUI>();
        ttsText.text = "🔊";
        ttsText.fontSize = 22;
        ttsText.color = Color.white;
        ttsText.alignment = TMPro.TextAlignmentOptions.Center;
        RectTransform ttsTextRect = ttsTextObj.GetComponent<RectTransform>();
        ttsTextRect.anchorMin = Vector2.zero;
        ttsTextRect.anchorMax = Vector2.one;
        ttsTextRect.offsetMin = Vector2.zero;
        ttsTextRect.offsetMax = Vector2.zero;

        // Nút chọn ngôn ngữ
        GameObject langBtnObj = new GameObject("LangButton");
        langBtnObj.transform.SetParent(panel.transform, false);
        RectTransform langBtnRect = langBtnObj.AddComponent<RectTransform>();
        langBtnRect.anchorMin = new Vector2(0.84f, 0.14f);
        langBtnRect.anchorMax = new Vector2(1f, 0.18f);
        langBtnRect.offsetMin = new Vector2(0, 0);
        langBtnRect.offsetMax = new Vector2(-10, -10);
        Button langBtn = langBtnObj.AddComponent<Button>();
        Image langBtnImg = langBtnObj.AddComponent<Image>();
        langBtnImg.color = new Color(0.2f, 0.2f, 0.8f, 1f);
        GameObject langTextObj = new GameObject("LangText");
        langTextObj.transform.SetParent(langBtnObj.transform, false);
        TextMeshProUGUI langText = langTextObj.AddComponent<TextMeshProUGUI>();
        langText.text = "🌐 VI/EN";
        langText.fontSize = 16;
        langText.color = Color.white;
        langText.alignment = TMPro.TextAlignmentOptions.Center;
        RectTransform langTextRect = langTextObj.GetComponent<RectTransform>();
        langTextRect.anchorMin = Vector2.zero;
        langTextRect.anchorMax = Vector2.one;
        langTextRect.offsetMin = Vector2.zero;
        langTextRect.offsetMax = Vector2.zero;
    }
}
