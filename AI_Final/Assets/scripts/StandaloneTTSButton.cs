using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class StandaloneTTSButton : MonoBehaviour
{
    public ChatService chatService;
    public InputField tmpInputField; // Kéo thả TMP_InputField từ Inspector
    private AudioSource audioSource;

    void Start()
    {
        // Tìm ChatPanel trước
        GameObject chatPanel = GameObject.Find("ChatPanel");
        if (chatPanel == null)
        {
            // Nếu không có ChatPanel, tìm Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas", typeof(Canvas));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            chatPanel = canvas.gameObject;
        }

        // Tạo nút
        GameObject btnObj = new GameObject("StandaloneTTSButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(chatPanel.transform, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.sizeDelta = new Vector2(140, 50);
        rect.anchoredPosition = new Vector2(-10, 10);

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0.1f, 0.7f, 0.2f, 1f);

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(OnStandaloneTTSButtonClicked);

        // Thêm text cho nút
        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        var txt = txtObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = "TTS Độc lập";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.color = Color.white;
        txt.fontSize = 20;

        // Tạo AudioSource riêng
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnStandaloneTTSButtonClicked()
    {
        string userInput = tmpInputField != null ? tmpInputField.text.Trim() : null;
        if (!string.IsNullOrEmpty(userInput))
        {
            Debug.Log($"[StandaloneTTS] User input: {userInput}");
            
            // FAST OPTION: Xử lý tất cả trong Python server
            StartCoroutine(SendToFastIntelligentTTS(userInput));
        }
        else
        {
            Debug.LogWarning("TMP_InputField rỗng, không có gì để đọc!");
        }
    }

    [System.Serializable]
    public class TTSRequest
    {
        public string text;
    }

    private System.Collections.IEnumerator SendToFastIntelligentTTS(string userInput)
    {
        string fastTTSUrl = "http://localhost:8001/fast_intelligent_tts";
        TTSRequest requestData = new TTSRequest { text = userInput };
        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(fastTTSUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            float startTime = Time.time;
            yield return request.SendWebRequest();
            float totalTime = Time.time - startTime;

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                Debug.Log($"[StandaloneTTS] ✅ Fast response in {totalTime:F2}s - Audio: {audioData.Length} bytes");

                if (audioData != null && audioData.Length > 0)
                {
                    AudioClip clip = WavUtility.ToAudioClip(audioData, 0, "FastTTSClip");
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("[StandaloneTTS] 🔊 Playing AI response audio!");
                }
                else
                {
                    Debug.LogWarning("[StandaloneTTS] ⚠️ No audio data received!");
                }
            }
            else
            {
                Debug.LogError($"[StandaloneTTS] ❌ Error in {totalTime:F2}s: {request.error}");
                
                // Fallback to original TTS
                Debug.Log("[StandaloneTTS] 🔄 Falling back to basic TTS...");
                chatService.RequestTTS(userInput, (audio) => {
                    if (audio != null)
                    {
                        AudioClip clip = WavUtility.ToAudioClip(audio, 0, "FallbackTTSClip");
                        audioSource.clip = clip;
                        audioSource.Play();
                    }
                });
            }
        }
    }

    // BƯỚC XỬ LÝ AI MỚI
    private void ProcessWithAI(string userInput, System.Action<string> callback)
    {
        // Có thể chọn AI provider dựa trên logic
        bool useRasa = ShouldUseRasa(userInput);
        
        if (useRasa)
        {
            Debug.Log("[StandaloneTTS] Using Rasa AI...");
            ProcessWithRasa(userInput, callback);
        }
        else
        {
            Debug.Log("[StandaloneTTS] Using Gemini AI...");
            ProcessWithGemini(userInput, callback);
        }
    }

    private void ProcessWithRasa(string userInput, System.Action<string> callback)
    {
        // Gọi Rasa API
        StartCoroutine(SendToRasa(userInput, callback));
    }

    private void ProcessWithGemini(string userInput, System.Action<string> callback)
    {
        // Gọi Gemini API
        StartCoroutine(SendToGemini(userInput, callback));
    }

    private bool ShouldUseRasa(string input)
    {
        // Logic chọn AI: Rasa cho game commands, Gemini cho general chat
        string[] gameKeywords = {"di chuyển", "đặt tường", "AI", "quoridor", "game", "chơi"};
        string lowerInput = input.ToLower();
        
        foreach (string keyword in gameKeywords)
        {
            if (lowerInput.Contains(keyword.ToLower()))
                return true;
        }
        return false; // Default to Gemini
    }

    private System.Collections.IEnumerator SendToRasa(string message, System.Action<string> callback)
    {
        string rasaUrl = "http://localhost:5005/webhooks/rest/webhook";
        
        var requestData = new {
            sender = "user",
            message = message
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(rasaUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"[Rasa] Raw response: {response}");
                
                // Parse Rasa response
                string aiResponse = ParseRasaResponse(response);
                callback(aiResponse);
            }
            else
            {
                Debug.LogError($"[Rasa] Error: {request.error}");
                callback("Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này.");
            }
        }
    }

    private System.Collections.IEnumerator SendToGemini(string message, System.Action<string> callback)
    {
        string geminiUrl = "YOUR_GEMINI_API_ENDPOINT";
        
        var requestData = new {
            prompt = message,
            max_tokens = 150
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(geminiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer YOUR_API_KEY");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"[Gemini] Raw response: {response}");
                
                // Parse Gemini response
                string aiResponse = ParseGeminiResponse(response);
                callback(aiResponse);
            }
            else
            {
                Debug.LogError($"[Gemini] Error: {request.error}");
                callback("Tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.");
            }
        }
    }

    private string ParseRasaResponse(string jsonResponse)
    {
        // Parse JSON response từ Rasa
        try
        {
            // Rasa trả về array of messages
            // Example: [{"text": "AI response here"}]
            if (jsonResponse.Contains("\"text\""))
            {
                int start = jsonResponse.IndexOf("\"text\":\"") + 8;
                int end = jsonResponse.IndexOf("\"", start);
                if (start < end)
                {
                    return jsonResponse.Substring(start, end - start);
                }
            }
            return "Tôi đã hiểu yêu cầu của bạn.";
        }
        catch
        {
            return "Có lỗi xảy ra khi xử lý phản hồi.";
        }
    }

    private string ParseGeminiResponse(string jsonResponse)
    {
        // Parse JSON response từ Gemini
        try
        {
            // Gemini response format may vary
            if (jsonResponse.Contains("\"text\""))
            {
                int start = jsonResponse.IndexOf("\"text\":\"") + 8;
                int end = jsonResponse.IndexOf("\"", start);
                if (start < end)
                {
                    return jsonResponse.Substring(start, end - start);
                }
            }
            return "Đây là phản hồi từ Gemini AI.";
        }
        catch
        {
            return "Có lỗi xảy ra khi xử lý phản hồi AI.";
        }
    }
} 