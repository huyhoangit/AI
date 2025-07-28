using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Quản lý trạng thái nhập liệu toàn cục trong game
/// </summary>
public class InputStateManager : MonoBehaviour
{
    // Singleton pattern
    public static InputStateManager Instance { get; private set; }
    
    [Header("Input State")]
    [SerializeField] private bool isInputFieldFocused = false;
    [SerializeField] private bool blockGameInputs = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private string currentFocusedObject = "None";
    
    // Events
    public System.Action<bool> OnInputStateChanged;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Update()
    {
        CheckInputFocusState();
    }
    
    /// <summary>
    /// Kiểm tra trạng thái focus của input fields
    /// </summary>
    private void CheckInputFocusState()
    {
        bool previousState = isInputFieldFocused;
        isInputFieldFocused = IsAnyInputFieldFocused();
        
        // Update current focused object name for debugging
        UpdateCurrentFocusedObjectName();
        
        // Trigger event nếu trạng thái thay đổi
        if (previousState != isInputFieldFocused)
        {
            OnInputStateChanged?.Invoke(isInputFieldFocused);
            
            if (showDebugInfo)
            {
                Debug.Log($"[InputStateManager] Input focus changed: {(isInputFieldFocused ? "FOCUSED" : "UNFOCUSED")} - Object: {currentFocusedObject}");
            }
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có InputField nào đang được focus không
    /// </summary>
    private bool IsAnyInputFieldFocused()
    {
        if (EventSystem.current != null)
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
            
            if (selectedObject != null)
            {
                // Kiểm tra TMP_InputField
                var tmpInputField = selectedObject.GetComponent<TMPro.TMP_InputField>();
                if (tmpInputField != null && tmpInputField.isFocused)
                {
                    return true;
                }
                
                // Kiểm tra Legacy InputField
                var legacyInputField = selectedObject.GetComponent<UnityEngine.UI.InputField>();
                if (legacyInputField != null && legacyInputField.isFocused)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Cập nhật tên object đang được focus để debug
    /// </summary>
    private void UpdateCurrentFocusedObjectName()
    {
        if (EventSystem.current != null)
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
            currentFocusedObject = selectedObject != null ? selectedObject.name : "None";
        }
        else
        {
            currentFocusedObject = "No EventSystem";
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có nên block game inputs không
    /// </summary>
    public bool ShouldBlockGameInputs()
    {
        return isInputFieldFocused || blockGameInputs;
    }
    
    /// <summary>
    /// Force block/unblock game inputs
    /// </summary>
    public void SetBlockGameInputs(bool block, string reason = "")
    {
        blockGameInputs = block;
        
        if (showDebugInfo)
        {
            Debug.Log($"[InputStateManager] Game inputs {(block ? "BLOCKED" : "UNBLOCKED")} - Reason: {reason}");
        }
    }
    
    /// <summary>
    /// Kiểm tra xem key có bị block không
    /// </summary>
    public bool IsKeyBlocked(KeyCode key)
    {
        if (ShouldBlockGameInputs())
        {
            if (showDebugInfo)
            {
                Debug.Log($"[InputStateManager] Key {key} blocked - Input field focused: {isInputFieldFocused}, Force blocked: {blockGameInputs}");
            }
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Safe input check - trả về true nếu key được nhấn VÀ không bị block
    /// </summary>
    public bool GetKeyDownSafe(KeyCode key)
    {
        if (IsKeyBlocked(key))
            return false;
            
        return Input.GetKeyDown(key);
    }
    
    /// <summary>
    /// Safe input check cho key hold
    /// </summary>
    public bool GetKeySafe(KeyCode key)
    {
        if (IsKeyBlocked(key))
            return false;
            
        return Input.GetKey(key);
    }
    
    // Properties
    public bool IsInputFieldFocused => isInputFieldFocused;
    public bool IsGameInputBlocked => blockGameInputs;
    public string CurrentFocusedObject => currentFocusedObject;
    
    [ContextMenu("Debug Input State")]
    public void DebugInputState()
    {
        Debug.Log("=== INPUT STATE MANAGER DEBUG ===");
        Debug.Log($"Is Input Field Focused: {isInputFieldFocused}");
        Debug.Log($"Is Game Input Blocked: {blockGameInputs}");
        Debug.Log($"Should Block Game Inputs: {ShouldBlockGameInputs()}");
        Debug.Log($"Current Focused Object: {currentFocusedObject}");
        Debug.Log($"EventSystem Available: {(EventSystem.current != null)}");
        
        if (EventSystem.current != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                Debug.Log($"Selected Object Details:");
                Debug.Log($"  Name: {selected.name}");
                Debug.Log($"  Has TMP_InputField: {selected.GetComponent<TMPro.TMP_InputField>() != null}");
                Debug.Log($"  Has Legacy InputField: {selected.GetComponent<UnityEngine.UI.InputField>() != null}");
            }
        }
    }
    
    [ContextMenu("Test Key R")]
    public void TestKeyR()
    {
        bool blocked = IsKeyBlocked(KeyCode.R);
        bool safeCheck = GetKeyDownSafe(KeyCode.R);
        
        Debug.Log($"Key R Status:");
        Debug.Log($"  Is Blocked: {blocked}");
        Debug.Log($"  Safe Check Result: {safeCheck}");
        Debug.Log($"  Raw Input.GetKeyDown(R): {Input.GetKeyDown(KeyCode.R)}");
    }
    
    [ContextMenu("Test Key C")]
    public void TestKeyC()
    {
        bool blocked = IsKeyBlocked(KeyCode.C);
        bool safeCheck = GetKeyDownSafe(KeyCode.C);
        
        Debug.Log($"Key C Status:");
        Debug.Log($"  Is Blocked: {blocked}");
        Debug.Log($"  Safe Check Result: {safeCheck}");
        Debug.Log($"  Raw Input.GetKeyDown(C): {Input.GetKeyDown(KeyCode.C)}");
    }
    
    [ContextMenu("Test All Key Protection")]
    public void TestAllKeyProtection()
    {
        Debug.Log("=== TESTING ALL KEY PROTECTION ===");
        
        // Test key R
        bool rBlocked = IsKeyBlocked(KeyCode.R);
        bool rSafe = GetKeyDownSafe(KeyCode.R);
        bool rRaw = Input.GetKeyDown(KeyCode.R);
        
        Debug.Log($"KEY R:");
        Debug.Log($"  - Blocked: {rBlocked}");
        Debug.Log($"  - Safe Check: {rSafe}");
        Debug.Log($"  - Raw Input: {rRaw}");
        
        // Test key C
        bool cBlocked = IsKeyBlocked(KeyCode.C);
        bool cSafe = GetKeyDownSafe(KeyCode.C);
        bool cRaw = Input.GetKeyDown(KeyCode.C);
        
        Debug.Log($"KEY C:");
        Debug.Log($"  - Blocked: {cBlocked}");
        Debug.Log($"  - Safe Check: {cSafe}");
        Debug.Log($"  - Raw Input: {cRaw}");
        
        // Test current input state
        Debug.Log($"INPUT STATE:");
        Debug.Log($"  - Input Field Focused: {isInputFieldFocused}");
        Debug.Log($"  - Should Block: {ShouldBlockGameInputs()}");
        Debug.Log($"  - Current Focus: {currentFocusedObject}");
    }
}
