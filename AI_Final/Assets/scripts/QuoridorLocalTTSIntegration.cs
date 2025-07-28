using UnityEngine;
using System.Collections;

/// <summary>
/// Integrates existing ChatService TTS with Quoridor game events
/// Uses local TTS server instead of ElevenLabs
/// </summary>
public class QuoridorLocalTTSIntegration : MonoBehaviour
{
    [Header("TTS Integration")]
    [SerializeField] private ChatService chatService;
    [SerializeField] private bool enableGameEvents = true;
    [SerializeField] private bool enableAITrainingEvents = true;
    [SerializeField] private bool enableDebugMessages = false;
    
    [Header("Message Settings")]
    [SerializeField] private float minMessageInterval = 1.0f;
    [SerializeField] private bool enableVietnameseMessages = true; // Default to Vietnamese
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float volume = 1.0f;
    
    private float lastMessageTime = 0f;
    private GameManager gameManager;
    private QuoridorAI[] aiComponents;
    private bool isProcessing = false;
    
    void Start()
    {
        InitializeTTSIntegration();
        StartCoroutine(SetupGameEventListeners());
    }
    
    void InitializeTTSIntegration()
    {
        // Find ChatService
        if (chatService == null)
        {
            chatService = FindFirstObjectByType<ChatService>();
            if (chatService == null)
            {
                Debug.LogWarning("⚠️ ChatService not found! TTS will not work.");
                return;
            }
        }
        
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
        
        // Find game components
        gameManager = FindFirstObjectByType<GameManager>();
        aiComponents = FindObjectsByType<QuoridorAI>(FindObjectsSortMode.None);
        
        Debug.Log($"🎤 Local TTS Integration initialized with {aiComponents.Length} AI components");
        
        // Test TTS
        if (enableDebugMessages)
        {
            StartCoroutine(TestTTSAfterDelay());
        }
    }
    
    IEnumerator TestTTSAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        SpeakMessage("Hệ thống TTS đã sẵn sàng cho game Quoridor");
    }
    
    IEnumerator SetupGameEventListeners()
    {
        yield return new WaitForSeconds(1f);
        
        // Monitor AI training status
        if (enableAITrainingEvents)
        {
            StartCoroutine(MonitorAITrainingStatus());
        }
        
        // Monitor game state changes
        if (enableGameEvents)
        {
            StartCoroutine(MonitorGameState());
        }
    }
    
    /// <summary>
    /// Monitor AI training status and speak updates
    /// </summary>
    IEnumerator MonitorAITrainingStatus()
    {
        bool lastTrainingState = false;
        bool lastTrainedModelState = false;
        
        while (true)
        {
            yield return new WaitForSeconds(2f);
            
            foreach (var ai in aiComponents)
            {
                if (ai == null) continue;
                
                // Check if AI is training
                bool isCurrentlyTraining = IsAITraining(ai);
                if (isCurrentlyTraining != lastTrainingState)
                {
                    if (isCurrentlyTraining)
                    {
                        SpeakMessage("AI đã bắt đầu huấn luyện");
                    }
                    lastTrainingState = isCurrentlyTraining;
                }
                
                // Check if AI is using trained model
                bool isUsingTrainedModel = ai.isTrainedModel;
                if (isUsingTrainedModel != lastTrainedModelState)
                {
                    if (isUsingTrainedModel)
                    {
                        SpeakMessage("AI đang sử dụng kiến thức đã được huấn luyện");
                    }
                    lastTrainedModelState = isUsingTrainedModel;
                }
            }
        }
    }
    
    /// <summary>
    /// Monitor game state changes
    /// </summary>
    IEnumerator MonitorGameState()
    {
        int lastTurn = -1;
        bool lastGameActive = false;
        
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if (gameManager != null)
            {
                // Check turn changes
                int currentTurn = GetCurrentTurn();
                if (currentTurn != lastTurn && currentTurn > 0)
                {
                    if (currentTurn % 2 == 1) // Player turn
                    {
                        SpeakMessage("Đến lượt bạn chơi");
                    }
                    else // AI turn
                    {
                        SpeakMessage("AI đang suy nghĩ");
                    }
                    lastTurn = currentTurn;
                }
                
                // Check game state
                bool isGameActive = IsGameActive();
                if (isGameActive != lastGameActive)
                {
                    if (isGameActive)
                    {
                        SpeakMessage("Trò chơi đã bắt đầu");
                    }
                    else
                    {
                        SpeakMessage("Trò chơi đã kết thúc");
                    }
                    lastGameActive = isGameActive;
                }
            }
        }
    }
    
    /// <summary>
    /// Check if AI is currently training
    /// </summary>
    bool IsAITraining(QuoridorAI ai)
    {
        // Check for self-play trainer
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null)
        {
            return trainer.autoStartTraining;
        }
        
        // Check if AI is in training mode
        return !ai.isTrainedModel && ai.allowQTableSaving;
    }
    
    /// <summary>
    /// Get current turn number
    /// </summary>
    int GetCurrentTurn()
    {
        if (gameManager != null)
        {
            // Try to get turn from GameManager
            var turnField = typeof(GameManager).GetField("currentTurn", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (turnField != null)
            {
                return (int)turnField.GetValue(gameManager);
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Check if game is currently active
    /// </summary>
    bool IsGameActive()
    {
        if (gameManager != null)
        {
            // Try to get game state from GameManager
            var gameActiveField = typeof(GameManager).GetField("gameActive", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameActiveField != null)
            {
                return (bool)gameActiveField.GetValue(gameManager);
            }
        }
        return false;
    }
    
    /// <summary>
    /// Speak a message using local TTS server
    /// </summary>
    public void SpeakMessage(string message)
    {
        if (chatService == null || !enableGameEvents || isProcessing) return;
        
        // Rate limiting
        if (Time.time - lastMessageTime < minMessageInterval)
        {
            return;
        }
        
        lastMessageTime = Time.time;
        
        // Convert to Vietnamese if needed
        if (enableVietnameseMessages)
        {
            message = TranslateToVietnamese(message);
        }
        
        // Set language for TTS
        chatService.ChuyenNgonNgu(enableVietnameseMessages ? "vi" : "en");
        
        // Request TTS
        StartCoroutine(SpeakMessageCoroutine(message));
    }
    
    IEnumerator SpeakMessageCoroutine(string message)
    {
        isProcessing = true;
        
        chatService.RequestTTS(message, (audioData) =>
        {
            if (audioData != null && audioData.Length > 0)
            {
                // Convert audio data to AudioClip
                AudioClip audioClip = CreateAudioClipFromBytes(audioData);
                if (audioClip != null)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                    Debug.Log($"🎤 Playing audio: {message}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ TTS failed for message: {message}");
            }
            
            isProcessing = false;
        });
        
        yield return null;
    }
    
    /// <summary>
    /// Create AudioClip from byte array (WAV format)
    /// </summary>
    AudioClip CreateAudioClipFromBytes(byte[] audioData)
    {
        try
        {
            // Create a temporary WAV file and load it
            string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "tts_audio.wav");
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
                Debug.LogError($"❌ Failed to load audio: {www.error}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error creating audio clip: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Translate common game messages to Vietnamese
    /// </summary>
    string TranslateToVietnamese(string englishMessage)
    {
        switch (englishMessage.ToLower())
        {
            case "your turn to play":
                return "Đến lượt bạn chơi";
            case "ai is thinking":
                return "AI đang suy nghĩ";
            case "game started":
                return "Trò chơi đã bắt đầu";
            case "game ended":
                return "Trò chơi đã kết thúc";
            case "ai training has begun":
                return "AI đã bắt đầu huấn luyện";
            case "ai is now using trained knowledge":
                return "AI đang sử dụng kiến thức đã được huấn luyện";
            case "invalid move. please try again":
                return "Nước đi không hợp lệ. Vui lòng thử lại";
            case "wall placed successfully":
                return "Đã đặt tường thành công";
            case "congratulations! you won the game!":
                return "Chúc mừng! Bạn đã thắng!";
            case "game over. better luck next time!":
                return "Trò chơi kết thúc. Chúc may mắn lần sau!";
            case "the ai has won this game":
                return "AI đã thắng trò chơi này";
            case "ai training completed successfully":
                return "AI đã hoàn thành huấn luyện thành công";
            case "ai knowledge loaded from previous training":
                return "Đã tải kiến thức AI từ lần huấn luyện trước";
            default:
                return englishMessage;
        }
    }
    
    #region Public Interface Methods
    
    [ContextMenu("Test Local TTS")]
    public void TestLocalTTS()
    {
        SpeakMessage("Test hệ thống TTS local thành công");
    }
    
    [ContextMenu("Speak AI Status")]
    public void SpeakAIStatus()
    {
        if (aiComponents.Length > 0)
        {
            var ai = aiComponents[0];
            if (ai.isTrainedModel)
            {
                SpeakMessage("AI đang sử dụng model đã được huấn luyện");
            }
            else
            {
                SpeakMessage("AI đang ở chế độ huấn luyện");
            }
        }
    }
    
    [ContextMenu("Toggle Vietnamese Messages")]
    public void ToggleVietnameseMessages()
    {
        enableVietnameseMessages = !enableVietnameseMessages;
        Debug.Log($"🎤 Vietnamese messages: {(enableVietnameseMessages ? "ON" : "OFF")}");
    }
    
    #endregion
    
    #region Event Handlers for Manual Integration
    
    /// <summary>
    /// Call this when player makes a move
    /// </summary>
    public void OnPlayerMove()
    {
        SpeakMessage("Người chơi đã hoàn thành lượt đi");
    }
    
    /// <summary>
    /// Call this when AI makes a move
    /// </summary>
    public void OnAIMove()
    {
        SpeakMessage("AI đã hoàn thành lượt đi");
    }
    
    /// <summary>
    /// Call this when wall is placed
    /// </summary>
    public void OnWallPlaced()
    {
        SpeakMessage("Đã đặt tường thành công");
    }
    
    /// <summary>
    /// Call this when invalid move is attempted
    /// </summary>
    public void OnInvalidMove()
    {
        SpeakMessage("Nước đi không hợp lệ. Vui lòng thử lại");
    }
    
    /// <summary>
    /// Call this when game ends
    /// </summary>
    public void OnGameEnd(bool playerWon)
    {
        if (playerWon)
        {
            SpeakMessage("Chúc mừng! Bạn đã thắng!");
        }
        else
        {
            SpeakMessage("Trò chơi kết thúc. Chúc may mắn lần sau!");
        }
    }
    
    /// <summary>
    /// Call this when AI training starts
    /// </summary>
    public void OnAITrainingStart()
    {
        SpeakMessage("AI đã bắt đầu huấn luyện");
    }
    
    /// <summary>
    /// Call this when AI training completes
    /// </summary>
    public void OnAITrainingComplete()
    {
        SpeakMessage("AI đã hoàn thành huấn luyện thành công");
    }
    
    /// <summary>
    /// Call this when Q-table is loaded
    /// </summary>
    public void OnQTableLoaded()
    {
        SpeakMessage("Đã tải kiến thức AI từ lần huấn luyện trước");
    }
    
    #endregion
} 