using UnityEngine;
using UnityEngine.UI;

public class StandaloneTTSPanel : MonoBehaviour
{
    public ChatService chatService;
    private InputField inputField;
    private AudioSource audioSource;

    void Start()
    {
        // Tìm hoặc tạo Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // Tạo panel
        GameObject panelObj = new GameObject("StandaloneTTSPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(400, 100);
        panelRect.anchoredPosition = new Vector2(0, 0);
        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // Tạo InputField
        GameObject inputObj = new GameObject("TTSInputField", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
        inputObj.transform.SetParent(panelObj.transform, false);
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 0.5f);
        inputRect.anchorMax = new Vector2(0.7f, 0.5f);
        inputRect.pivot = new Vector2(0, 0.5f);
        inputRect.sizeDelta = new Vector2(260, 40);
        inputRect.anchoredPosition = new Vector2(20, 0);
        Image inputImg = inputObj.GetComponent<Image>();
        inputImg.color = Color.white;
        inputField = inputObj.GetComponent<InputField>();
        inputField.textComponent = CreateText(inputObj.transform, "InputText", 18, TextAnchor.MiddleLeft, new Vector2(10, 0), new Vector2(250, 40));
        inputField.placeholder = CreateText(inputObj.transform, "Placeholder", 18, TextAnchor.MiddleLeft, new Vector2(10, 0), new Vector2(250, 40), "Nhập nội dung...");
        inputField.placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        // Tạo nút TTS
        GameObject btnObj = new GameObject("TTSButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(panelObj.transform, false);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.75f, 0.5f);
        btnRect.anchorMax = new Vector2(0.95f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(80, 40);
        btnRect.anchoredPosition = new Vector2(0, 0);
        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 1f, 1f);
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(OnTTSButtonClicked);
        var btnText = CreateText(btnObj.transform, "BtnText", 18, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(80, 40), "TTS");
        btnText.color = Color.white;

        // Tạo AudioSource riêng
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private Text CreateText(Transform parent, string name, int fontSize, TextAnchor anchor, Vector2 anchoredPos, Vector2 size, string text = "")
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObj.transform.SetParent(parent, false);
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;
        Text txt = textObj.GetComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.alignment = anchor;
        txt.text = text;
        txt.color = Color.black;
        return txt;
    }

    private void OnTTSButtonClicked()
    {
        string userInput = inputField != null ? inputField.text.Trim() : null;
        if (!string.IsNullOrEmpty(userInput))
        {
            chatService.RequestTTS(userInput, (audio) => {
                if (audio != null)
                {
                    AudioClip clip = WavUtility.ToAudioClip(audio, 0, "TTSClipStandalone");
                    audioSource.clip = clip;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogWarning("[StandaloneTTSPanel] Không nhận được audio!");
                }
            });
        }
        else
        {
            Debug.LogWarning("[StandaloneTTSPanel] InputField rỗng, không có gì để đọc!");
        }
    }
} 