using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Linq; // Added for .Any()

public class ChatService : MonoBehaviour
{
    public string rasaUrl = "http://localhost:5005/webhooks/rest/webhook";
    [Header("Gemini API Settings")]
    [Tooltip("API key lấy từ Google AI Studio")]
    public string geminiApiKey = "";
    [Tooltip("Model Gemini, ví dụ: gemini-1.5-flash")]
    public string geminiModel = "gemini-1.5-flash";
    
    public enum TTSType
    {
        LocalServer,
        GoogleTTS,
        AzureSpeech,
        OpenAITTS,
        ElevenLabs
    }
    
    [Header("TTS Settings")]
    
    [SerializeField] private TTSType ttsType = TTSType.LocalServer;
    
    [Header("Local TTS Settings")]
    public string ttsUrl = "http://localhost:8001/tts";
    public string sttUrl = "http://localhost:8002/stt";
    
    [Header("API TTS Settings")]
    [SerializeField] private string googleApiKey = "";
    [SerializeField] private string azureSubscriptionKey = "";
    [SerializeField] private string azureRegion = "eastus";
    [SerializeField] private string openaiApiKey = "";
    [SerializeField] private string elevenLabsApiKey = "";
    
    public string currentLang = "vi";

    public enum ChatModelType { Rasa, Gemini }
    [Header("Chatbot Model Switch")]
    public ChatModelType chatModelType = ChatModelType.Rasa;

    // Gửi message text lên bot (Rasa trước, fallback Gemini)
    public void SendMessageToBot(string message, Action<string> onReply)
    {
        if (chatModelType == ChatModelType.Rasa)
        {
            StartCoroutine(SendMessageToRasa(message, onReply));
        }
        else
        {
            StartCoroutine(SendMessageToGemini(message, onReply));
        }
    }

    // Tách riêng coroutine cho Rasa
    private IEnumerator SendMessageToRasa(string message, Action<string> onReply)
    {
        string rasaPayload = $"{{\"sender\":\"unity\",\"message\":\"{EscapeJson(message)}\"}}";
        using (UnityWebRequest req = new UnityWebRequest(rasaUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(rasaPayload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success && req.downloadHandler.text.Contains("text"))
            {
                string reply = ParseRasaReply(req.downloadHandler.text);
                onReply?.Invoke(reply);
            }
            else
            {
                Debug.LogError($"[Rasa] Error: {req.error} | Response: {req.downloadHandler.text}");
                onReply?.Invoke("[Rasa] Không nhận được phản hồi!");
            }
        }
    }

    // Tách riêng coroutine cho Gemini
    private IEnumerator SendMessageToGemini(string message, Action<string> onReply)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent?key={geminiApiKey}";
        string json = "{\n" +
                      "  \"contents\": [{\n" +
                      "    \"parts\": [{\n" +
                      "      \"text\": \"" + EscapeJson(message) + "\"\n" +
                      "    }]\n" +
                      "  }]\n" +
                      "}";
        byte[] bodyRawGemini = System.Text.Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRawGemini);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(req.downloadHandler.text);
                if (response.candidates != null && response.candidates.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;
                    onReply?.Invoke(reply);
                }
                else
                {
                    onReply?.Invoke("[Gemini] Không nhận được phản hồi rõ ràng!");
                }
            }
            else
            {
                Debug.LogError($"[Gemini] Error: {req.error} | Response: {req.downloadHandler.text}");
                onReply?.Invoke("[Gemini] Không thể kết nối Gemini API!");
            }
        }
    }

    // Gửi audio lên STT, nhận text
    public void SendVoiceToBot(byte[] audioData, Action<string> onText)
    {
        StartCoroutine(SendVoiceCoroutine(audioData, onText));
    }
    private IEnumerator SendVoiceCoroutine(byte[] audioData, Action<string> onText)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(sttUrl, ""))
        {
            req.uploadHandler = new UploadHandlerRaw(audioData);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "audio/wav");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.ConnectionError && req.result != UnityWebRequest.Result.ProtocolError)
            {
                string text = req.downloadHandler.text;
                onText?.Invoke(text);
            }
            else
            {
                onText?.Invoke("[Lỗi STT]");
            }
        }
    }

    // Gửi text lên TTS, nhận audio (WAV bytes)
    public void RequestTTS(string text, Action<byte[]> onAudio)
    {
        StartCoroutine(RequestTTSCoroutine(text, onAudio));
    }
    
    private IEnumerator RequestTTSCoroutine(string text, Action<byte[]> onAudio)
    {
        switch (ttsType)
        {
            case TTSType.LocalServer:
                yield return StartCoroutine(RequestLocalTTS(text, onAudio));
                break;
            case TTSType.GoogleTTS:
                yield return StartCoroutine(RequestGoogleTTS(text, onAudio));
                break;
            case TTSType.AzureSpeech:
                yield return StartCoroutine(RequestAzureTTS(text, onAudio));
                break;
            case TTSType.OpenAITTS:
                yield return StartCoroutine(RequestOpenAITTS(text, onAudio));
                break;
            case TTSType.ElevenLabs:
                yield return StartCoroutine(RequestElevenLabsTTS(text, onAudio));
                break;
        }
    }
    
    // Local TTS Server
    private IEnumerator RequestLocalTTS(string text, Action<byte[]> onAudio)
    {
        string json = $"{{\"text\":\"{text}\",\"lang\":\"{currentLang}\"}}";
        using (UnityWebRequest req = new UnityWebRequest(ttsUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.ConnectionError && req.result != UnityWebRequest.Result.ProtocolError)
            {
                byte[] audio = req.downloadHandler.data;
                onAudio?.Invoke(audio);
            }
            else
            {
                onAudio?.Invoke(null);
            }
        }
    }
    
    // Google TTS
    private IEnumerator RequestGoogleTTS(string text, Action<byte[]> onAudio)
    {
        if (string.IsNullOrEmpty(googleApiKey))
        {
            Debug.LogError("❌ Google API Key is missing!");
            onAudio?.Invoke(null);
            yield break;
        }
        
        string url = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={googleApiKey}";
        
        var requestBody = new GoogleTTSRequest
        {
            input = new GoogleTTSInput { text = text },
            voice = new GoogleTTSVoice { languageCode = currentLang, name = "vi-VN-Standard-A" },
            audioConfig = new GoogleTTSAudioConfig { audioEncoding = "MP3" }
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GoogleTTSResponse>(request.downloadHandler.text);
                if (!string.IsNullOrEmpty(response.audioContent))
                {
                    byte[] audioData = System.Convert.FromBase64String(response.audioContent);
                    onAudio?.Invoke(audioData);
                }
                else
                {
                    onAudio?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"❌ Google TTS failed: {request.error}");
                onAudio?.Invoke(null);
            }
        }
    }
    
    // Azure Speech
    private IEnumerator RequestAzureTTS(string text, Action<byte[]> onAudio)
    {
        if (string.IsNullOrEmpty(azureSubscriptionKey))
        {
            Debug.LogError("❌ Azure Subscription Key is missing!");
            onAudio?.Invoke(null);
            yield break;
        }
        
        // Get access token first
        string tokenUrl = $"https://{azureRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
        
        using (UnityWebRequest tokenRequest = new UnityWebRequest(tokenUrl, "POST"))
        {
            tokenRequest.SetRequestHeader("Ocp-Apim-Subscription-Key", azureSubscriptionKey);
            tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            tokenRequest.downloadHandler = new DownloadHandlerBuffer();
            
            yield return tokenRequest.SendWebRequest();
            
            if (tokenRequest.result == UnityWebRequest.Result.Success)
            {
                string accessToken = tokenRequest.downloadHandler.text;
                
                // Use token for TTS request
                string ttsUrl = $"https://{azureRegion}.tts.speech.microsoft.com/cognitiveservices/v1";
                
                using (UnityWebRequest ttsRequest = new UnityWebRequest(ttsUrl, "POST"))
                {
                    ttsRequest.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                    ttsRequest.SetRequestHeader("Content-Type", "application/ssml+xml");
                    ttsRequest.SetRequestHeader("X-Microsoft-OutputFormat", "audio-16khz-128kbitrate-mono-mp3");
                    ttsRequest.downloadHandler = new DownloadHandlerBuffer();
                    
                    string ssml = $@"<speak version='1.0' xml:lang='{currentLang}'>
                        <voice xml:lang='{currentLang}' xml:gender='Female' name='vi-VN-HoaiMyNeural'>
                            {text}
                        </voice>
                    </speak>";
                    
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(ssml);
                    ttsRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    
                    yield return ttsRequest.SendWebRequest();
                    
                    if (ttsRequest.result == UnityWebRequest.Result.Success)
                    {
                        byte[] audioData = ttsRequest.downloadHandler.data;
                        onAudio?.Invoke(audioData);
                    }
                    else
                    {
                        Debug.LogError($"❌ Azure TTS failed: {ttsRequest.error}");
                        onAudio?.Invoke(null);
                    }
                }
            }
            else
            {
                Debug.LogError($"❌ Azure token request failed: {tokenRequest.error}");
                onAudio?.Invoke(null);
            }
        }
    }
    
    // OpenAI TTS
    private IEnumerator RequestOpenAITTS(string text, Action<byte[]> onAudio)
    {
        if (string.IsNullOrEmpty(openaiApiKey))
        {
            Debug.LogError("❌ OpenAI API Key is missing!");
            onAudio?.Invoke(null);
            yield break;
        }
        
        string url = "https://api.openai.com/v1/audio/speech";
        
        var requestBody = new OpenAITTSRequest
        {
            model = "tts-1",
            input = text,
            voice = "alloy",
            response_format = "mp3"
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {openaiApiKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                onAudio?.Invoke(audioData);
            }
            else
            {
                Debug.LogError($"❌ OpenAI TTS failed: {request.error}");
                onAudio?.Invoke(null);
            }
        }
    }
    
    // ElevenLabs TTS
    private IEnumerator RequestElevenLabsTTS(string text, Action<byte[]> onAudio)
    {
        if (string.IsNullOrEmpty(elevenLabsApiKey))
        {
            Debug.LogError("❌ ElevenLabs API Key is missing!");
            onAudio?.Invoke(null);
            yield break;
        }
        
        string url = "https://api.elevenlabs.io/v1/text-to-speech/21m00Tcm4TlvDq8ikWAM";
        
        var requestBody = new ElevenLabsTTSRequest
        {
            text = text,
            voice_settings = new ElevenLabsVoiceSettings
            {
                stability = 0.5f,
                similarity_boost = 0.75f
            }
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", elevenLabsApiKey);
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                onAudio?.Invoke(audioData);
            }
            else
            {
                Debug.LogError($"❌ ElevenLabs TTS failed: {request.error}");
                onAudio?.Invoke(null);
            }
        }
    }

    // Đổi ngôn ngữ
    public void ChuyenNgonNgu(string lang)
    {
        currentLang = lang;
    }

    // Hàm public để gán vào nút switch
    public void SwitchChatModel()
    {
        chatModelType = (chatModelType == ChatModelType.Rasa) ? ChatModelType.Gemini : ChatModelType.Rasa;
        Debug.Log($"[ChatService] Đã chuyển model: {(chatModelType == ChatModelType.Rasa ? "Rasa" : "Gemini")}");
    }

    // Parse JSON trả lời từ Rasa
    private string ParseRasaReply(string json)
    {
        // Rasa trả về dạng [{"recipient_id":"unity","text":"..."}, ...]
        var texts = new System.Collections.Generic.List<string>();
        int idx = 0;
        while ((idx = json.IndexOf("\"text\":", idx)) >= 0)
        {
            int start = json.IndexOf('"', idx + 7) + 1;
            int end = json.IndexOf('"', start);
            if (start > 0 && end > start)
                texts.Add(json.Substring(start, end - start));
            idx = end;
        }
        return string.Join(" ", texts);
    }
    // Parse JSON trả lời từ Gemini
    private string ParseGeminiReply(string json)
    {
        // Gemini trả về {"reply":"..."}
        int idx = json.IndexOf("\"reply\":");
        if (idx >= 0)
        {
            int start = json.IndexOf('"', idx + 8) + 1;
            int end = json.IndexOf('"', start);
            if (start > 0 && end > start)
                return json.Substring(start, end - start);
        }
        return null;
    }

    string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    // Kiểm tra câu trả lời mặc định/không hiểu của Rasa, so sánh cả câu hỏi và câu trả lời
    private bool IsDefaultRasaReply(string reply, string userMessage)
    {
        string[] quoridorKeywords = {
            "quoridor", "bàn cờ", "quân cờ", "tường", "chiến thuật", "luật chơi", "mục tiêu", "đối thủ", "di chuyển",
            "luật", "hướng dẫn", "game", "trò chơi", "cách chơi", "quy tắc", "thắng", "kết thúc", "đặt tường", "chiến thắng"
        };
        string replyLower = reply.ToLower();
        string userLower = userMessage.ToLower();

        // Nếu câu hỏi không liên quan đến quoridor mà câu trả lời lại nhắc đến quoridor => mặc định
        bool userRelated = quoridorKeywords.Any(kw => userLower.Contains(kw));
        bool replyRelated = quoridorKeywords.Any(kw => replyLower.Contains(kw));
        if (!userRelated && replyRelated)
            return true;

        // Nếu câu trả lời không chứa từ khóa nào liên quan đến quoridor => mặc định
        if (!replyRelated)
            return true;

        // Có thể bổ sung thêm các mẫu câu mặc định khác ở đây...

        return false;
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
    
    // Google TTS Classes
    [System.Serializable]
    public class GoogleTTSRequest
    {
        public GoogleTTSInput input;
        public GoogleTTSVoice voice;
        public GoogleTTSAudioConfig audioConfig;
    }
    
    [System.Serializable]
    public class GoogleTTSInput
    {
        public string text;
    }
    
    [System.Serializable]
    public class GoogleTTSVoice
    {
        public string languageCode;
        public string name;
    }
    
    [System.Serializable]
    public class GoogleTTSAudioConfig
    {
        public string audioEncoding;
    }
    
    [System.Serializable]
    public class GoogleTTSResponse
    {
        public string audioContent;
    }
    
    // OpenAI TTS Classes
    [System.Serializable]
    public class OpenAITTSRequest
    {
        public string model;
        public string input;
        public string voice;
        public string response_format;
    }
    
    // ElevenLabs TTS Classes
    [System.Serializable]
    public class ElevenLabsTTSRequest
    {
        public string text;
        public ElevenLabsVoiceSettings voice_settings;
    }
    
    [System.Serializable]
    public class ElevenLabsVoiceSettings
    {
        public float stability;
        public float similarity_boost;
    }
} 