using UnityEngine;

/// <summary>
/// Quản lý input cho game, tích hợp với InputStateManager để block inputs khi cần
/// </summary>
public class GameInputManager : MonoBehaviour
{
    [Header("Key Bindings")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private KeyCode chatToggleKey = KeyCode.C;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TogglechatPanel chatPanel;
    
    [Header("Debug")]
    [SerializeField] private bool showKeyPressLog = false;
    
    void Awake()
    {
        // Auto-find references nếu chưa gán
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
            
        if (chatPanel == null)
            chatPanel = FindFirstObjectByType<TogglechatPanel>();
    }
    
    void Update()
    {
        HandleGameInputs();
    }
    
    /// <summary>
    /// Xử lý các input của game
    /// </summary>
    private void HandleGameInputs()
    {
        // Kiểm tra reset key (R)
        if (IsKeyDownSafe(resetKey))
        {
            HandleResetKey();
        }
        
        // Kiểm tra chat toggle key (C) - backup nếu TogglechatPanel không hoạt động
        if (IsKeyDownSafe(chatToggleKey))
        {
            HandleChatToggleKey();
        }
        
        // Kiểm tra pause key (Escape)
        if (IsKeyDownSafe(pauseKey))
        {
            HandlePauseKey();
        }
    }
    
    /// <summary>
    /// Safe input check sử dụng InputStateManager
    /// </summary>
    private bool IsKeyDownSafe(KeyCode key)
    {
        // Nếu có InputStateManager, sử dụng safe check
        if (InputStateManager.Instance != null)
        {
            bool result = InputStateManager.Instance.GetKeyDownSafe(key);
            
            if (showKeyPressLog && result)
            {
                Debug.Log($"[GameInputManager] Safe key press: {key}");
            }
            else if (showKeyPressLog && Input.GetKeyDown(key) && !result)
            {
                Debug.Log($"[GameInputManager] Key {key} blocked due to input focus");
            }
            
            return result;
        }
        
        // Fallback: check input focus manually
        bool isInputFocused = IsInputFieldFocusedFallback();
        bool keyPressed = Input.GetKeyDown(key);
        
        if (showKeyPressLog && keyPressed)
        {
            Debug.Log($"[GameInputManager] Key {key} - Input focused: {isInputFocused}, Allowed: {!isInputFocused}");
        }
        
        return keyPressed && !isInputFocused;
    }
    
    /// <summary>
    /// Fallback check cho input focus nếu không có InputStateManager
    /// </summary>
    private bool IsInputFieldFocusedFallback()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            
            if (selectedObject != null)
            {
                var tmpInputField = selectedObject.GetComponent<TMPro.TMP_InputField>();
                if (tmpInputField != null && tmpInputField.isFocused)
                    return true;
                
                var legacyInputField = selectedObject.GetComponent<UnityEngine.UI.InputField>();
                if (legacyInputField != null && legacyInputField.isFocused)
                    return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Xử lý phím Reset (R)
    /// </summary>
    private void HandleResetKey()
    {
        Debug.Log("[GameInputManager] Reset key (R) pressed");
        
        if (gameManager != null)
        {
            // Kiểm tra xem GameManager có method reset không
            var resetMethod = gameManager.GetType().GetMethod("ResetGame");
            if (resetMethod != null)
            {
                resetMethod.Invoke(gameManager, null);
                Debug.Log("✅ Game reset via GameManager.ResetGame()");
            }
            else
            {
                // Tìm các method reset khác
                var restartMethod = gameManager.GetType().GetMethod("RestartGame");
                if (restartMethod != null)
                {
                    restartMethod.Invoke(gameManager, null);
                    Debug.Log("✅ Game reset via GameManager.RestartGame()");
                }
                else
                {
                    Debug.LogWarning("⚠️ No reset method found in GameManager");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager not found for reset");
        }
    }
    
    /// <summary>
    /// Xử lý phím Chat Toggle (C) - backup
    /// </summary>
    private void HandleChatToggleKey()
    {
        Debug.Log("[GameInputManager] Chat toggle key (C) pressed");
        
        if (chatPanel != null)
        {
            chatPanel.ToggleChatPanel();
            Debug.Log("✅ Chat toggled via GameInputManager backup");
        }
        else
        {
            Debug.LogWarning("⚠️ ChatPanel not found for toggle");
        }
    }
    
    /// <summary>
    /// Xử lý phím Pause (Escape)
    /// </summary>
    private void HandlePauseKey()
    {
        Debug.Log("[GameInputManager] Pause key (Escape) pressed");
        
        if (gameManager != null)
        {
            // Tìm pause method
            var pauseMethod = gameManager.GetType().GetMethod("PauseGame");
            if (pauseMethod != null)
            {
                pauseMethod.Invoke(gameManager, null);
                Debug.Log("✅ Game paused via GameManager.PauseGame()");
            }
            else
            {
                Debug.LogWarning("⚠️ No pause method found in GameManager");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager not found for pause");
        }
    }
    
    /// <summary>
    /// Set key bindings programmatically
    /// </summary>
    public void SetKeyBindings(KeyCode reset, KeyCode chat, KeyCode pause)
    {
        resetKey = reset;
        chatToggleKey = chat;
        pauseKey = pause;
        
        Debug.Log($"[GameInputManager] Key bindings updated - Reset: {reset}, Chat: {chat}, Pause: {pause}");
    }
    
    /// <summary>
    /// Bật/tắt debug logging cho key presses
    /// </summary>
    public void SetDebugLogging(bool enabled)
    {
        showKeyPressLog = enabled;
        Debug.Log($"[GameInputManager] Debug logging {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    [ContextMenu("Enable Debug Logging")]
    public void EnableDebugLogging()
    {
        SetDebugLogging(true);
    }
    
    [ContextMenu("Disable Debug Logging")]
    public void DisableDebugLogging()
    {
        SetDebugLogging(false);
    }
    
    [ContextMenu("Test Reset Key")]
    public void TestResetKey()
    {
        HandleResetKey();
    }
    
    [ContextMenu("Test Chat Toggle Key")]
    public void TestChatToggleKey()
    {
        HandleChatToggleKey();
    }
    
    [ContextMenu("Debug Input State")]
    public void DebugInputState()
    {
        Debug.Log("=== GAME INPUT MANAGER DEBUG ===");
        Debug.Log($"Reset Key: {resetKey}");
        Debug.Log($"Chat Toggle Key: {chatToggleKey}");
        Debug.Log($"Pause Key: {pauseKey}");
        Debug.Log($"GameManager: {(gameManager != null ? "✅ Found" : "❌ Missing")}");
        Debug.Log($"ChatPanel: {(chatPanel != null ? "✅ Found" : "❌ Missing")}");
        Debug.Log($"InputStateManager: {(InputStateManager.Instance != null ? "✅ Available" : "❌ Not available")}");
        
        if (InputStateManager.Instance != null)
        {
            Debug.Log($"  - Input Focus: {InputStateManager.Instance.IsInputFieldFocused}");
            Debug.Log($"  - Game Blocked: {InputStateManager.Instance.IsGameInputBlocked}");
            Debug.Log($"  - Reset Key Blocked: {InputStateManager.Instance.IsKeyBlocked(resetKey)}");
            Debug.Log($"  - Chat Key Blocked: {InputStateManager.Instance.IsKeyBlocked(chatToggleKey)}");
        }
        else
        {
            bool fallbackFocus = IsInputFieldFocusedFallback();
            Debug.Log($"  - Fallback Input Focus: {fallbackFocus}");
        }
    }
    
    [ContextMenu("Test All Keys Safety")]
    public void TestAllKeysSafety()
    {
        Debug.Log("=== TESTING KEY SAFETY ===");
        
        bool resetSafe = IsKeyDownSafe(resetKey);
        bool chatSafe = IsKeyDownSafe(chatToggleKey);
        bool pauseSafe = IsKeyDownSafe(pauseKey);
        
        Debug.Log($"Reset Key ({resetKey}) Safe: {resetSafe}");
        Debug.Log($"Chat Key ({chatToggleKey}) Safe: {chatSafe}");
        Debug.Log($"Pause Key ({pauseKey}) Safe: {pauseSafe}");
        
        // Raw input checks
        Debug.Log($"Raw Reset Input: {Input.GetKeyDown(resetKey)}");
        Debug.Log($"Raw Chat Input: {Input.GetKeyDown(chatToggleKey)}");
        Debug.Log($"Raw Pause Input: {Input.GetKeyDown(pauseKey)}");
    }
}
