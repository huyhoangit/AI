using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tạo message bubbles cho chat UI
/// </summary>
public class MessageBubbleCreator : MonoBehaviour
{
    [Header("Bubble Settings")]
    public Color userBubbleColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    public Color aiBubbleColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    public float bubblePadding = 10f;
    public float maxBubbleWidth = 250f;
    
    [Header("Text Settings")]
    public float fontSize = 14f;
    public Color userTextColor = Color.white;
    public Color aiTextColor = Color.white;
    
    /// <summary>
    /// Tạo message bubble cho user
    /// </summary>
    public GameObject CreateUserBubble(string message, Transform parent)
    {
        return CreateMessageBubble(message, parent, true);
    }
    
    /// <summary>
    /// Tạo message bubble cho AI
    /// </summary>
    public GameObject CreateAIBubble(string message, Transform parent)
    {
        return CreateMessageBubble(message, parent, false);
    }
    
    /// <summary>
    /// Tạo message bubble chung
    /// </summary>
    private GameObject CreateMessageBubble(string message, Transform parent, bool isUser)
    {
        // Tạo container với kích thước cố định
        GameObject container = new GameObject("BubbleContainer");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        
        // Cấu hình container với kích thước cố định
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.sizeDelta = new Vector2(500f, 100f); // Width = 500, Height = 100
        containerRect.anchoredPosition = Vector2.zero;
        
        HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = isUser ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = true;  // Cho phép control width
        hlg.childControlHeight = true; // Cho phép control height
        hlg.padding = new RectOffset(10, 10, 10, 10);
        
        // Thêm ContentSizeFitter để container giữ kích thước cố định
        ContentSizeFitter containerCSF = container.AddComponent<ContentSizeFitter>();
        containerCSF.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // Giữ width = 500
        containerCSF.verticalFit = ContentSizeFitter.FitMode.Unconstrained; // Giữ height = 100

        // Tạo bubble
        GameObject bubble = new GameObject($"Bubble_{(isUser ? "User" : "AI")}");

        bubble.transform.SetParent(container.transform, false);

        RectTransform bubbleRect = bubble.AddComponent<RectTransform>();
        Image bubbleImage = bubble.AddComponent<Image>();
        LayoutElement bubbleLayout = bubble.AddComponent<LayoutElement>();

        // Cấu hình bubble để fit trong container
        bubbleImage.color = isUser ? userBubbleColor : aiBubbleColor;
        bubbleImage.type = Image.Type.Sliced;

        // Đặt kích thước bubble trực tiếp
        bubbleRect.sizeDelta = new Vector2(450f, 80f); // Kích thước cố định

        // Cấu hình layout cho bubble - kích thước cố định cho cả User và AI
        bubbleLayout.preferredWidth = 450f; // Để lại 50px cho padding
        bubbleLayout.preferredHeight = 80f;  // Để lại 20px cho padding
        bubbleLayout.minWidth = 450f;        // Kích thước tối thiểu
        bubbleLayout.minHeight = 80f;        // Kích thước tối thiểu
        bubbleLayout.flexibleWidth = 0f;     // Không cho phép mở rộng
        bubbleLayout.flexibleHeight = 0f;    // Không cho phép mở rộng

        // Tạo text component
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bubble.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();

        // Cấu hình text rect để fill bubble
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(bubblePadding, bubblePadding);
        textRect.offsetMax = new Vector2(-bubblePadding, -bubblePadding);

        // Cấu hình text
        textComponent.text = message;
        textComponent.fontSize = fontSize;
        textComponent.color = isUser ? userTextColor : aiTextColor;
        textComponent.alignment = isUser ? TMPro.TextAlignmentOptions.MidlineRight : TMPro.TextAlignmentOptions.MidlineLeft;
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        textComponent.overflowMode = TextOverflowModes.Ellipsis; // Cắt text nếu quá dài

        // Cấu hình bubble position trong container
        bubbleRect.anchorMin = Vector2.zero;
        bubbleRect.anchorMax = Vector2.zero; // Thay đổi từ Vector2.one thành Vector2.zero
        bubbleRect.pivot = new Vector2(0.5f, 0.5f);
        bubbleRect.anchoredPosition = new Vector2(225f, 40f); // Đặt ở giữa container (450/2, 80/2)
        bubbleRect.offsetMin = Vector2.zero;
        bubbleRect.offsetMax = Vector2.zero;

        return container;
    }
    
    /// <summary>
    /// Điều chỉnh kích thước bubble dựa trên text
    /// </summary>
    private System.Collections.IEnumerator AdjustBubbleSize(GameObject bubble, TextMeshProUGUI textComponent)
    {
        // Đợi một frame để text được render
        yield return null;
        
        // Tính toán kích thước text
        Vector2 textSize = textComponent.GetPreferredValues();
        
        // Điều chỉnh bubble size
        RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
        LayoutElement bubbleLayout = bubble.GetComponent<LayoutElement>();
        
        float targetWidth = Mathf.Min(textSize.x + bubblePadding * 2, maxBubbleWidth);
        float targetHeight = textSize.y + bubblePadding * 2;
        
        bubbleLayout.preferredWidth = targetWidth;
        bubbleLayout.preferredHeight = targetHeight;
        
        // Force layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
    }
    
    /// <summary>
    /// Tạo bubble với alignment khác nhau
    /// </summary>
    public GameObject CreateBubbleWithAlignment(string message, Transform parent, bool isUser, TMPro.TextAlignmentOptions alignment)
    {
        GameObject bubble = CreateMessageBubble(message, parent, isUser);
        
        // Thay đổi alignment của text
        TextMeshProUGUI textComponent = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.alignment = alignment;
        }
        
        return bubble;
    }
    
    /// <summary>
    /// Tạo bubble với custom style
    /// </summary>
    public GameObject CreateCustomBubble(string message, Transform parent, Color bubbleColor, Color textColor, TextAlignmentOptions alignment)
    {
        // Tạo bubble container
        GameObject bubble = new GameObject("CustomBubble");
        bubble.transform.SetParent(parent, false);
        
        RectTransform bubbleRect = bubble.AddComponent<RectTransform>();
        Image bubbleImage = bubble.AddComponent<Image>();
        LayoutElement bubbleLayout = bubble.AddComponent<LayoutElement>();
        
        // Cấu hình bubble
        bubbleImage.color = bubbleColor;
        bubbleImage.type = Image.Type.Sliced;
        
        // Cấu hình layout
        bubbleLayout.preferredWidth = maxBubbleWidth;
        bubbleLayout.flexibleWidth = 0f;
        bubbleLayout.minHeight = 30f;
        
        // Tạo text component
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bubble.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        
        // Cấu hình text
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(bubblePadding, bubblePadding);
        textRect.offsetMax = new Vector2(-bubblePadding, -bubblePadding);
        
        textComponent.text = message;
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = (TMPro.TextAlignmentOptions)alignment;
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        textComponent.overflowMode = TextOverflowModes.Overflow;
        
        // Cấu hình bubble size
        StartCoroutine(AdjustBubbleSize(bubble, textComponent));
        
        return bubble;
    }
    
    /// <summary>
    /// Tạo bubble với kích thước cố định - phương pháp đơn giản
    /// </summary>
    public GameObject CreateFixedSizeBubble(string message, Transform parent, bool isUser)
    {
        // Tạo container chính
        GameObject container = new GameObject("FixedBubbleContainer");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        
        // Container cố định 480x100
        containerRect.sizeDelta = new Vector2(400f, 100f);
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = Vector2.zero;
        
        // Tạo bubble bên trong - KHÔNG dùng LayoutGroup
        GameObject bubble = new GameObject($"FixedBubble_{(isUser ? "User" : "AI")}");
        bubble.transform.SetParent(container.transform, false);
        RectTransform bubbleRect = bubble.AddComponent<RectTransform>();
        Image bubbleImage = bubble.AddComponent<Image>();
        
        // Đặt kích thước bubble cố định 300x80
        bubbleRect.sizeDelta = new Vector2(300f, 80f);
        
        // Đặt anchor presets và vị trí bubble dựa trên alignment
        if (isUser)
        {
            // User: right-top anchor preset, x = -10
            bubbleRect.anchorMin = new Vector2(1, 1);  // right-top
            bubbleRect.anchorMax = new Vector2(1, 1);  // right-top
            bubbleRect.pivot = new Vector2(1, 1);      // pivot tại right-top
            bubbleRect.anchoredPosition = new Vector2(-10f, 0f); // x = -10 từ bên phải
        }
        else
        {
            // AI: left-top anchor preset, x = 10
            bubbleRect.anchorMin = new Vector2(0, 1);  // left-top
            bubbleRect.anchorMax = new Vector2(0, 1);  // left-top
            bubbleRect.pivot = new Vector2(0, 1);      // pivot tại left-top
            bubbleRect.anchoredPosition = new Vector2(10f, 0f); // x = 10 từ bên trái
        }
        
        // Cấu hình bubble image
        bubbleImage.color = isUser ? userBubbleColor : aiBubbleColor;
        bubbleImage.type = Image.Type.Sliced;
        
        // Tạo text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bubble.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        
        // Text fill bubble
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(bubblePadding, bubblePadding);
        textRect.offsetMax = new Vector2(-bubblePadding, -bubblePadding);
        
        // Cấu hình text
        textComponent.text = message;
        textComponent.fontSize = fontSize;
        textComponent.color = isUser ? userTextColor : aiTextColor;
        textComponent.alignment = isUser ? TMPro.TextAlignmentOptions.MidlineRight : TMPro.TextAlignmentOptions.MidlineLeft;
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        textComponent.overflowMode = TextOverflowModes.Ellipsis;
        
        Debug.Log($"✅ Created fixed size bubble: Container={containerRect.sizeDelta}, Bubble={bubbleRect.sizeDelta}");
        Debug.Log($"   User={isUser}, Anchor={bubbleRect.anchorMin}, Position={bubbleRect.anchoredPosition}");
        
        return container;
    }
    
    [ContextMenu("Test Create User Bubble")]
    public void TestCreateUserBubble()
    {
        Transform testParent = transform;
        if (testParent == null)
        {
            Debug.LogError("❌ No parent transform found!");
            return;
        }
        
        GameObject bubble = CreateUserBubble("Test user message - This is a test message to check bubble sizing", testParent);
        Debug.Log("✅ User bubble created successfully!");
        Debug.Log($"Container size: {bubble.GetComponent<RectTransform>().sizeDelta}");
    }
    
    [ContextMenu("Test Create AI Bubble")]
    public void TestCreateAIBubble()
    {
        Transform testParent = transform;
        if (testParent == null)
        {
            Debug.LogError("❌ No parent transform found!");
            return;
        }
        
        GameObject bubble = CreateAIBubble("Test AI response - This is a test AI response to check bubble sizing and alignment", testParent);
        Debug.Log("✅ AI bubble created successfully!");
        Debug.Log($"Container size: {bubble.GetComponent<RectTransform>().sizeDelta}");
    }
    
    [ContextMenu("Clear All Test Bubbles")]
    public void ClearAllTestBubbles()
    {
        // Xóa tất cả bubble test
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name.Contains("BubbleContainer"))
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        Debug.Log("🗑️ All test bubbles cleared!");
    }
    
    [ContextMenu("Debug Bubble Sizes")]
    public void DebugBubbleSizes()
    {
        Debug.Log("=== BUBBLE SIZE DEBUG ===");
        
        // Tìm tất cả bubble containers
        var containers = FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
        foreach (var container in containers)
        {
            if (container.name.Contains("BubbleContainer"))
            {
                Debug.Log($"Container '{container.name}': Size = {container.sizeDelta}");
                
                // Tìm bubble bên trong
                for (int i = 0; i < container.childCount; i++)
                {
                    var child = container.GetChild(i);
                    if (child.name.Contains("Bubble_"))
                    {
                        var bubbleRect = child.GetComponent<RectTransform>();
                        var layoutElement = child.GetComponent<LayoutElement>();
                        
                        Debug.Log($"  Bubble '{child.name}':");
                        Debug.Log($"    RectTransform.sizeDelta = {bubbleRect.sizeDelta}");
                        Debug.Log($"    RectTransform.rect.size = {bubbleRect.rect.size}");
                        
                        if (layoutElement != null)
                        {
                            Debug.Log($"    LayoutElement.preferredWidth = {layoutElement.preferredWidth}");
                            Debug.Log($"    LayoutElement.preferredHeight = {layoutElement.preferredHeight}");
                            Debug.Log($"    LayoutElement.minWidth = {layoutElement.minWidth}");
                            Debug.Log($"    LayoutElement.minHeight = {layoutElement.minHeight}");
                        }
                    }
                }
            }
        }
    }
    
    [ContextMenu("Force Rebuild Layout")]
    public void ForceRebuildLayout()
    {
        // Tìm tất cả container và force rebuild
        var containers = FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
        foreach (var container in containers)
        {
            if (container.name.Contains("BubbleContainer"))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(container);
                Debug.Log($"Force rebuilt layout for: {container.name}");
            }
        }
    }
    
    [ContextMenu("Test Fixed Size Bubbles")]
    public void TestFixedSizeBubbles()
    {
        if (transform == null)
        {
            Debug.LogError("❌ No transform found!");
            return;
        }
        
        // Test user bubble
        var userBubble = CreateFixedSizeBubble("Test user message với kích thước cố định", transform, true);
        
        // Test AI bubble
        var aiBubble = CreateFixedSizeBubble("Test AI response với kích thước cố định", transform, false);
        
        Debug.Log("✅ Fixed size bubbles created! Check sizes now.");
        
        // Debug sizes
        StartCoroutine(DebugSizesAfterFrame());
    }
    
    private System.Collections.IEnumerator DebugSizesAfterFrame()
    {
        yield return null; // Wait one frame
        
        var containers = GetComponentsInChildren<RectTransform>();
        foreach (var rect in containers)
        {
            if (rect.name.Contains("FixedBubble"))
            {
                Debug.Log($"Bubble '{rect.name}': sizeDelta={rect.sizeDelta}, rect.size={rect.rect.size}");
            }
        }
    }
    
    [ContextMenu("Test New Size Bubbles")]
    public void TestNewSizeBubbles()
    {
        if (transform == null)
        {
            Debug.LogError("❌ No transform found!");
            return;
        }
        
        Debug.Log("🧪 Testing new bubble sizes:");
        Debug.Log("   Container: 480x100px");
        Debug.Log("   Bubble: 300x80px");
        Debug.Log("   AI: left-top anchor, x=10");
        Debug.Log("   User: right-top anchor, x=-10");
        
        // Test user bubble
        var userBubble = CreateFixedSizeBubble("Test user message với kích thước mới", transform, true);
        
        // Test AI bubble  
        var aiBubble = CreateFixedSizeBubble("Test AI response với kích thước mới", transform, false);
        
        Debug.Log("✅ New size bubbles created! Check positions now.");
        
        // Debug sizes and positions
        StartCoroutine(DebugNewSizesAfterFrame());
    }
    
    private System.Collections.IEnumerator DebugNewSizesAfterFrame()
    {
        yield return null; // Wait one frame
        
        Debug.Log("=== NEW BUBBLE SIZE & POSITION DEBUG ===");
        var containers = GetComponentsInChildren<RectTransform>();
        foreach (var rect in containers)
        {
            if (rect.name.Contains("FixedBubble"))
            {
                string type = rect.name.Contains("User") ? "USER" : "AI";
                Debug.Log($"{type} Bubble '{rect.name}':");
                Debug.Log($"   Size: {rect.sizeDelta} (Expected: 300x80)");
                Debug.Log($"   Anchor: {rect.anchorMin} to {rect.anchorMax}");
                Debug.Log($"   Position: {rect.anchoredPosition}");
                Debug.Log($"   World Position: {rect.position}");
            }
            else if (rect.name.Contains("FixedBubbleContainer"))
            {
                Debug.Log($"CONTAINER '{rect.name}':");
                Debug.Log($"   Size: {rect.sizeDelta} (Expected: 480x100)");
            }
        }
    }
    
    [ContextMenu("Test Final Position Bubbles")]
    public void TestFinalPositionBubbles()
    {
        if (transform == null)
        {
            Debug.LogError("❌ No transform found!");
            return;
        }
        
        Debug.Log("🎯 Testing FINAL bubble positions:");
        Debug.Log("   Container: 480x100px");
        Debug.Log("   Bubble: 300x80px");
        Debug.Log("   AI: left-top anchor, x=10 (close to left edge)");
        Debug.Log("   User: right-top anchor, x=-10 (close to right edge)");
        
        // Clear previous test bubbles
        ClearAllTestBubbles();
        
        // Test user bubble
        var userBubble = CreateFixedSizeBubble("User message gần sát mép phải", transform, true);
        
        // Test AI bubble  
        var aiBubble = CreateFixedSizeBubble("AI response gần sát mép trái", transform, false);
        
        Debug.Log("✅ Final position bubbles created!");
        
        // Debug final positions
        StartCoroutine(DebugFinalPositionsAfterFrame());
    }
    
    private System.Collections.IEnumerator DebugFinalPositionsAfterFrame()
    {
        yield return null; // Wait one frame
        
        Debug.Log("=== FINAL BUBBLE POSITIONS DEBUG ===");
        var containers = GetComponentsInChildren<RectTransform>();
        foreach (var rect in containers)
        {
            if (rect.name.Contains("FixedBubble"))
            {
                string type = rect.name.Contains("User") ? "USER" : "AI";
                Debug.Log($"{type} Bubble:");
                Debug.Log($"   Size: {rect.sizeDelta}");
                Debug.Log($"   Anchor: {rect.anchorMin}");
                Debug.Log($"   Position: {rect.anchoredPosition}");
                
                // Tính toán vị trí thực tế trong container
                if (type == "USER")
                {
                    Debug.Log($"   → Distance from RIGHT edge: {Mathf.Abs(rect.anchoredPosition.x)}px");
                }
                else
                {
                    Debug.Log($"   → Distance from LEFT edge: {rect.anchoredPosition.x}px");
                }
            }
        }
    }
}
