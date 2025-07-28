using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

/// <summary>
/// Flexible TTS Manager supporting multiple APIs
/// Supports Google TTS, Azure Speech, OpenAI TTS, etc.
/// </summary>
public class APITTSManager : MonoBehaviour
{
    public enum TTSProvider
    {
        GoogleTTS,
        AzureSpeech,
        OpenAITTS,
        ElevenLabs,
        CustomAPI
    }
    
    [Header("API Configuration")]
    [SerializeField] private TTSProvider selectedProvider = TTSProvider.GoogleTTS;
    
    [Header("Google TTS Settings")]
    [SerializeField] private string googleApiKey = "";
    [SerializeField] private string googleVoice = "vi-VN-Standard-A";
    
    [Header("Azure Speech Settings")]
    [SerializeField] private string azureSubscriptionKey = "";
    [SerializeField] private string azureRegion = "eastus";
    [SerializeField] private string azureVoice = "vi-VN-HoaiMyNeural";
    
    [Header("OpenAI TTS Settings")]
    [SerializeField] private string openaiApiKey = "";
    [SerializeField] private string openaiVoice = "alloy";
    
    [Header("ElevenLabs Settings")]
    [SerializeField] private string elevenLabsApiKey = "";
    [SerializeField] private string elevenLabsVoiceId = "21m00Tcm4TlvDq8ikWAM";
    
    [Header("Custom API Settings")]
    [SerializeField] private string customApiUrl = "";
    [SerializeField] private string customApiKey = "";
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float volume = 1.0f;
    [SerializeField] private bool autoPlay = true;
    
    [Header("General Settings")]
    [SerializeField] private string defaultLanguage = "vi";
    [SerializeField] private bool enableDebugLogs = true;
    
    private bool isProcessing = false;
    
    void Start()
    {
        InitializeTTS();
    }
    
    void InitializeTTS()
    {
        // Get or create AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        audioSource.volume = volume;
        
        LogDebug($"🎤 API TTS Manager initialized with provider: {selectedProvider}");
    }
    
    /// <summary>
    /// Convert text to speech using selected API
    /// </summary>
    public void SpeakText(string text, string language = null)
    {
        if (isProcessing)
        {
            LogDebug("⚠️ TTS already processing, skipping request");
            return;
        }
        
        string lang = language ?? defaultLanguage;
        StartCoroutine(ConvertTextToSpeech(text, lang));
    }
    
    IEnumerator ConvertTextToSpeech(string text, string language)
    {
        isProcessing = true;
        LogDebug($"🎤 Converting to speech: '{text}' (lang: {language})");
        
        switch (selectedProvider)
        {
            case TTSProvider.GoogleTTS:
                yield return StartCoroutine(GoogleTTS(text, language));
                break;
            case TTSProvider.AzureSpeech:
                yield return StartCoroutine(AzureTTS(text, language));
                break;
            case TTSProvider.OpenAITTS:
                yield return StartCoroutine(OpenAITTS(text, language));
                break;
            case TTSProvider.ElevenLabs:
                yield return StartCoroutine(ElevenLabsTTS(text, language));
                break;
            case TTSProvider.CustomAPI:
                yield return StartCoroutine(CustomAPITTS(text, language));
                break;
        }
        
        isProcessing = false;
    }
    
    #region Google TTS
    
    IEnumerator GoogleTTS(string text, string language)
    {
        if (string.IsNullOrEmpty(googleApiKey))
        {
            LogError("❌ Google API Key is missing!");
            yield break;
        }
        
        string url = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={googleApiKey}";
        
        var requestBody = new GoogleTTSRequest
        {
            input = new GoogleTTSInput { text = text },
            voice = new GoogleTTSVoice { languageCode = language, name = googleVoice },
            audioConfig = new GoogleTTSAudioConfig { audioEncoding = "MP3" }
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GoogleTTSResponse>(request.downloadHandler.text);
                if (!string.IsNullOrEmpty(response.audioContent))
                {
                    byte[] audioData = Convert.FromBase64String(response.audioContent);
                    PlayAudio(audioData, "Google TTS");
                }
            }
            else
            {
                LogError($"❌ Google TTS failed: {request.error}");
            }
        }
    }
    
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
    
    #endregion
    
    #region Azure Speech
    
    IEnumerator AzureTTS(string text, string language)
    {
        if (string.IsNullOrEmpty(azureSubscriptionKey))
        {
            LogError("❌ Azure Subscription Key is missing!");
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
                    
                    string ssml = $@"<speak version='1.0' xml:lang='{language}'>
                        <voice xml:lang='{language}' xml:gender='Female' name='{azureVoice}'>
                            {text}
                        </voice>
                    </speak>";
                    
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(ssml);
                    ttsRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    
                    yield return ttsRequest.SendWebRequest();
                    
                    if (ttsRequest.result == UnityWebRequest.Result.Success)
                    {
                        byte[] audioData = ttsRequest.downloadHandler.data;
                        PlayAudio(audioData, "Azure TTS");
                    }
                    else
                    {
                        LogError($"❌ Azure TTS failed: {ttsRequest.error}");
                    }
                }
            }
            else
            {
                LogError($"❌ Azure token request failed: {tokenRequest.error}");
            }
        }
    }
    
    #endregion
    
    #region OpenAI TTS
    
    IEnumerator OpenAITTS(string text, string language)
    {
        if (string.IsNullOrEmpty(openaiApiKey))
        {
            LogError("❌ OpenAI API Key is missing!");
            yield break;
        }
        
        string url = "https://api.openai.com/v1/audio/speech";
        
        var requestBody = new OpenAITTSRequest
        {
            model = "tts-1",
            input = text,
            voice = openaiVoice,
            response_format = "mp3"
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {openaiApiKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                PlayAudio(audioData, "OpenAI TTS");
            }
            else
            {
                LogError($"❌ OpenAI TTS failed: {request.error}");
            }
        }
    }
    
    [System.Serializable]
    public class OpenAITTSRequest
    {
        public string model;
        public string input;
        public string voice;
        public string response_format;
    }
    
    #endregion
    
    #region ElevenLabs TTS
    
    IEnumerator ElevenLabsTTS(string text, string language)
    {
        if (string.IsNullOrEmpty(elevenLabsApiKey))
        {
            LogError("❌ ElevenLabs API Key is missing!");
            yield break;
        }
        
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{elevenLabsVoiceId}";
        
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
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", elevenLabsApiKey);
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                PlayAudio(audioData, "ElevenLabs TTS");
            }
            else
            {
                LogError($"❌ ElevenLabs TTS failed: {request.error}");
            }
        }
    }
    
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
    
    #endregion
    
    #region Custom API
    
    IEnumerator CustomAPITTS(string text, string language)
    {
        if (string.IsNullOrEmpty(customApiUrl))
        {
            LogError("❌ Custom API URL is missing!");
            yield break;
        }
        
        string jsonPayload = $"{{\"text\":\"{text}\",\"lang\":\"{language}\"}}";
        
        using (UnityWebRequest request = new UnityWebRequest(customApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(customApiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {customApiKey}");
            }
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                PlayAudio(audioData, "Custom API TTS");
            }
            else
            {
                LogError($"❌ Custom API TTS failed: {request.error}");
            }
        }
    }
    
    #endregion
    
    /// <summary>
    /// Play audio data
    /// </summary>
    void PlayAudio(byte[] audioData, string source)
    {
        if (audioData != null && audioData.Length > 0)
        {
            AudioClip audioClip = CreateAudioClipFromBytes(audioData);
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                
                if (autoPlay)
                {
                    audioSource.Play();
                    LogDebug($"🎵 Playing audio from {source}: {audioData.Length} bytes");
                }
            }
        }
        else
        {
            LogWarning($"⚠️ No audio data received from {source}");
        }
    }
    
    /// <summary>
    /// Create AudioClip from byte array
    /// </summary>
    AudioClip CreateAudioClipFromBytes(byte[] audioData)
    {
        try
        {
            // Create a temporary file and load it
            string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "tts_audio.mp3");
            System.IO.File.WriteAllBytes(tempPath, audioData);
            
            // Load audio file
            WWW www = new WWW("file://" + tempPath);
            while (!www.isDone) { }
            
            if (string.IsNullOrEmpty(www.error))
            {
                AudioClip clip = www.GetAudioClip(false, false);
                return clip;
            }
            else
            {
                LogError($"❌ Failed to load audio: {www.error}");
                return null;
            }
        }
        catch (Exception e)
        {
            LogError($"❌ Error creating audio clip: {e.Message}");
            return null;
        }
    }
    
    #region Utility Methods
    
    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }
    
    void LogError(string message)
    {
        Debug.LogError(message);
    }
    
    [ContextMenu("Test Current TTS Provider")]
    public void TestCurrentProvider()
    {
        SpeakText("Test TTS system");
    }
    
    [ContextMenu("Switch to Google TTS")]
    public void SwitchToGoogleTTS()
    {
        selectedProvider = TTSProvider.GoogleTTS;
        LogDebug("🎤 Switched to Google TTS");
    }
    
    [ContextMenu("Switch to Azure Speech")]
    public void SwitchToAzureSpeech()
    {
        selectedProvider = TTSProvider.AzureSpeech;
        LogDebug("🎤 Switched to Azure Speech");
    }
    
    [ContextMenu("Switch to OpenAI TTS")]
    public void SwitchToOpenAITTS()
    {
        selectedProvider = TTSProvider.OpenAITTS;
        LogDebug("🎤 Switched to OpenAI TTS");
    }
    
    [ContextMenu("Switch to ElevenLabs")]
    public void SwitchToElevenLabs()
    {
        selectedProvider = TTSProvider.ElevenLabs;
        LogDebug("🎤 Switched to ElevenLabs");
    }
    
    #endregion
} 