using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public enum AnimationType
{
    Scale,
    Slide,
    Fade,
    SlideAndFade
}

public class TogglechatPanel : MonoBehaviour
{
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.C;
    [SerializeField] private GameObject toggleButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationType animationType = AnimationType.Scale;
    
    [Header("Container Settings")]
    [SerializeField] private float containerWidth = 400f;
    [SerializeField] private float containerHeight = 700f;
    [SerializeField] private bool useCustomSize = true;
    
    private bool isChatPanelVisible = false;
    private bool isAnimating = false;
    private RectTransform chatPanelRect;
    private CanvasGroup chatPanelCanvasGroup;
    private Vector3 originalScale;
    private Vector2 originalSize;
    private void Update()
    {
        // Sử dụng InputStateManager để kiểm tra input safety
        if (!isAnimating && 
            (InputStateManager.Instance != null ? InputStateManager.Instance.GetKeyDownSafe(toggleKey) : Input.GetKeyDown(toggleKey) && !IsInputFieldFocused()) || 
            (toggleButton != null && Input.GetMouseButtonDown(0) && 
             RectTransformUtility.RectangleContainsScreenPoint(toggleButton.GetComponent<RectTransform>(), Input.mousePosition)))
        {
            ToggleChatPanel();
        }   
    }
    
    public void ToggleChatPanel()
    {

        if (isAnimating || chatPanel == null) return;
        
        toggleButton.SetActive(isChatPanelVisible);
        isChatPanelVisible = !isChatPanelVisible;
        StartCoroutine(AnimateChatPanel(isChatPanelVisible));
        Debug.Log($"Chat panel animation started: {(isChatPanelVisible ? "Show" : "Hide")}");
    }

    private void Start()
    {
        InitializeComponents();
        SetupInitialState();
    }
    
    private void InitializeComponents()
    {
        if (chatPanel != null)
        {
            chatPanelRect = chatPanel.GetComponent<RectTransform>();
            chatPanelCanvasGroup = chatPanel.GetComponent<CanvasGroup>();
            
            // Thêm CanvasGroup nếu chưa có (cho fade animation)
            if (chatPanelCanvasGroup == null)
            {
                chatPanelCanvasGroup = chatPanel.AddComponent<CanvasGroup>();
            }
            
            // Áp dụng custom size nếu được bật
            if (useCustomSize && chatPanelRect != null)
            {
                chatPanelRect.sizeDelta = new Vector2(containerWidth, containerHeight);
                Debug.Log($"Applied custom size: {containerWidth}x{containerHeight}px");
            }
            
            // Lưu trữ giá trị gốc (sau khi áp dụng custom width)
            if (chatPanelRect != null)
            {
                originalScale = chatPanelRect.localScale;
                originalSize = chatPanelRect.sizeDelta;
            }
        }
    }
    
    private void SetupInitialState()
    {
        // Đặt trạng thái ban đầu (ẩn)
        isChatPanelVisible = false;
        if (chatPanel != null)
        {
            switch (animationType)
            {
                case AnimationType.Scale:
                    chatPanelRect.localScale = Vector3.zero;
                    break;
                case AnimationType.Slide:
                    chatPanelRect.anchoredPosition = new Vector2(chatPanelRect.anchoredPosition.x + originalSize.x, chatPanelRect.anchoredPosition.y);
                    break;
                case AnimationType.Fade:
                    chatPanelCanvasGroup.alpha = 0f;
                    break;
                case AnimationType.SlideAndFade:
                    chatPanelRect.anchoredPosition = new Vector2(chatPanelRect.anchoredPosition.x + originalSize.x, chatPanelRect.anchoredPosition.y);
                    chatPanelCanvasGroup.alpha = 0f;
                    break;
            }
            chatPanel.SetActive(true); // Keep active for animation
        }
    }
    
    private IEnumerator AnimateChatPanel(bool show)
    {
        isAnimating = true;
        float elapsed = 0f;
        
        // Đảm bảo panel active để animation hoạt động
        if (show)
            chatPanel.SetActive(true);
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float curveValue = animationCurve.Evaluate(show ? progress : 1f - progress);
            
            ApplyAnimation(curveValue);
            yield return null;
        }
        
        // Đảm bảo giá trị cuối chính xác
        ApplyAnimation(show ? 1f : 0f);
        
        // Tắt panel nếu đang ẩn để tối ưu hiệu suất
        if (!show)
            chatPanel.SetActive(false);
            
        isAnimating = false;
        Debug.Log($"Chat panel animation completed: {(show ? "Shown" : "Hidden")}");
    }
    
    private void ApplyAnimation(float progress)
    {
        switch (animationType)
        {
            case AnimationType.Scale:
                chatPanelRect.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
                break;
                
            case AnimationType.Slide:
                Vector2 hiddenPos = new Vector2(chatPanelRect.anchoredPosition.x + originalSize.x, chatPanelRect.anchoredPosition.y);
                Vector2 shownPos = new Vector2(chatPanelRect.anchoredPosition.x - originalSize.x, chatPanelRect.anchoredPosition.y);
                chatPanelRect.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, progress);
                break;
                
            case AnimationType.Fade:
                chatPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
                break;
                
            case AnimationType.SlideAndFade:
                // Combine slide và fade
                Vector2 slideHiddenPos = new Vector2(chatPanelRect.anchoredPosition.x + originalSize.x, chatPanelRect.anchoredPosition.y);
                Vector2 slideShownPos = new Vector2(chatPanelRect.anchoredPosition.x - originalSize.x, chatPanelRect.anchoredPosition.y);
                chatPanelRect.anchoredPosition = Vector2.Lerp(slideHiddenPos, slideShownPos, progress);
                chatPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
                break;
        }
    }
    
    /// <summary>
    /// Mở panel với animation
    /// </summary>
    public void ShowPanel()
    {
        if (!isChatPanelVisible && !isAnimating)
        {
            isChatPanelVisible = true;
            StartCoroutine(AnimateChatPanel(true));
        }
    }
    
    /// <summary>
    /// Ẩn panel với animation  
    /// </summary>
    public void HidePanel()
    {
        if (isChatPanelVisible && !isAnimating)
        {
            isChatPanelVisible = false;
            StartCoroutine(AnimateChatPanel(false));
        }
    }
    
    /// <summary>
    /// Đặt loại animation
    /// </summary>
    public void SetAnimationType(AnimationType newType)
    {
        if (!isAnimating)
        {
            animationType = newType;
            SetupInitialState();
        }
    }
    
    /// <summary>
    /// Đặt kích thước cho container
    /// </summary>
    public void SetContainerSize(float newWidth, float newHeight)
    {
        containerWidth = newWidth;
        containerHeight = newHeight;
        
        if (chatPanelRect != null && useCustomSize)
        {
            chatPanelRect.sizeDelta = new Vector2(containerWidth, containerHeight);
            
            // Cập nhật originalSize sau khi thay đổi
            originalSize = chatPanelRect.sizeDelta;
            
            Debug.Log($"Container size updated to: {containerWidth}x{containerHeight}px");
        }
    }
    
    /// <summary>
    /// Đặt width cho container (backward compatibility)
    /// </summary>
    public void SetContainerWidth(float newWidth)
    {
        SetContainerSize(newWidth, containerHeight);
    }
    
    /// <summary>
    /// Bật/tắt custom size
    /// </summary>
    public void SetUseCustomSize(bool enabled)
    {
        useCustomSize = enabled;
        
        if (chatPanelRect != null)
        {
            if (useCustomSize)
            {
                // Áp dụng custom size
                chatPanelRect.sizeDelta = new Vector2(containerWidth, containerHeight);
                Debug.Log($"Custom size enabled: {containerWidth}x{containerHeight}px");
            }
            else
            {
                Debug.Log("Custom size disabled - using original size");
            }
            
            // Cập nhật originalSize
            originalSize = chatPanelRect.sizeDelta;
        }
    }
    
    [ContextMenu("Test Show Panel")]
    public void TestShowPanel()
    {
        ShowPanel();
    }
    
    [ContextMenu("Test Hide Panel")]
    public void TestHidePanel()
    {
        HidePanel();
    }
    
    [ContextMenu("Test Toggle Panel")]
    public void TestTogglePanel()
    {
        ToggleChatPanel();
    }
    
    [ContextMenu("Reset Panel State")]
    public void ResetPanelState()
    {
        if (chatPanel != null && !isAnimating)
        {
            StopAllCoroutines();
            isAnimating = false;
            SetupInitialState();
            Debug.Log("Panel state reset to initial hidden state");
        }
    }
    
    [ContextMenu("Debug Panel Info")]
    public void DebugPanelInfo()
    {
        Debug.Log("=== CHAT PANEL TOGGLE DEBUG ===");
        Debug.Log($"Chat Panel: {(chatPanel != null ? "✅ Found" : "❌ Missing")}");
        Debug.Log($"Toggle Button: {(toggleButton != null ? "✅ Found" : "❌ Missing")}");
        Debug.Log($"Is Visible: {isChatPanelVisible}");
        Debug.Log($"Is Animating: {isAnimating}");
        Debug.Log($"Animation Type: {animationType}");
        Debug.Log($"Animation Duration: {animationDuration}s");
        Debug.Log($"Container Width: {containerWidth}px");
        Debug.Log($"Container Height: {containerHeight}px");
        Debug.Log($"Use Custom Size: {useCustomSize}");
        
        // Input state info
        bool localInputFocus = IsInputFieldFocused();
        Debug.Log($"Local Input Focus Check: {(localInputFocus ? "🔒 FOCUSED" : "✅ Not focused")}");
        
        // InputStateManager info (nếu có)
        if (InputStateManager.Instance != null)
        {
            Debug.Log($"InputStateManager: ✅ Available");
            Debug.Log($"  - Input Field Focused: {InputStateManager.Instance.IsInputFieldFocused}");
            Debug.Log($"  - Game Input Blocked: {InputStateManager.Instance.IsGameInputBlocked}");
            Debug.Log($"  - Should Block Inputs: {InputStateManager.Instance.ShouldBlockGameInputs()}");
            Debug.Log($"  - Current Focused Object: {InputStateManager.Instance.CurrentFocusedObject}");
            Debug.Log($"  - Key {toggleKey} would be blocked: {InputStateManager.Instance.IsKeyBlocked(toggleKey)}");
        }
        else
        {
            Debug.Log($"InputStateManager: ❌ Not available (using local check)");
        }
        
        if (chatPanelRect != null)
        {
            Debug.Log($"Panel Scale: {chatPanelRect.localScale}");
            Debug.Log($"Panel Position: {chatPanelRect.anchoredPosition}");
            Debug.Log($"Panel Size: {chatPanelRect.sizeDelta}");
        }
        
        if (chatPanelCanvasGroup != null)
        {
            Debug.Log($"Panel Alpha: {chatPanelCanvasGroup.alpha}");
        }
    }
    
    [ContextMenu("Test Input Focus Check")]
    public void TestInputFocusCheck()
    {
        bool isFocused = IsInputFieldFocused();
        Debug.Log($"Is InputField Focused: {isFocused}");
        
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            GameObject selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            Debug.Log($"Currently Selected Object: {(selected != null ? selected.name : "None")}");
            
            if (selected != null)
            {
                var tmpInputField = selected.GetComponent<TMPro.TMP_InputField>();
                var legacyInputField = selected.GetComponent<UnityEngine.UI.InputField>();
                
                Debug.Log($"Has TMP_InputField: {tmpInputField != null}");
                Debug.Log($"Has Legacy InputField: {legacyInputField != null}");
                
                if (tmpInputField != null)
                    Debug.Log($"TMP InputField isFocused: {tmpInputField.isFocused}");
                if (legacyInputField != null)
                    Debug.Log($"Legacy InputField isFocused: {legacyInputField.isFocused}");
            }
        }
        else
        {
            Debug.LogWarning("EventSystem not found!");
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có InputField nào đang được focus không
    /// </summary>
    private bool IsInputFieldFocused()
    {
        // Kiểm tra EventSystem có focus vào UI element nào không
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            
            if (selectedObject != null)
            {
                // Kiểm tra nếu object được select là InputField
                var inputField = selectedObject.GetComponent<TMPro.TMP_InputField>();
                if (inputField != null && inputField.isFocused)
                {
                    return true;
                }
                
                // Kiểm tra Legacy InputField (nếu có)
                var legacyInputField = selectedObject.GetComponent<UnityEngine.UI.InputField>();
                if (legacyInputField != null && legacyInputField.isFocused)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    [ContextMenu("Set Size 500x700")]
    public void SetSize500x700()
    {
        SetContainerSize(500f, 700f);
        Debug.Log("✅ Chat panel size set to 500x700px");
    }
    
    [ContextMenu("Set Size 400x600")]
    public void SetSize400x600()
    {
        SetContainerSize(400f, 600f);
        Debug.Log("✅ Chat panel size set to 400x600px");
    }
    
    [ContextMenu("Enable Custom Size")]
    public void EnableCustomSize()
    {
        SetUseCustomSize(true);
    }
    
    [ContextMenu("Disable Custom Size")]
    public void DisableCustomSize()
    {
        SetUseCustomSize(false);
    }
}
