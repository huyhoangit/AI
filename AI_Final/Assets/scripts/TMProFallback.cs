using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fallback cho TextMeshPro - Sử dụng UI.Text thông thường
/// Chỉ sử dụng khi không có TextMeshPro package
/// </summary>
public static class TMProFallback
{
    // Tạo TMP_Text component từ UI.Text
    public static TMP_Text CreateTMPText(GameObject obj)
    {
        Text text = obj.GetComponent<Text>();
        if (text == null)
        {
            text = obj.AddComponent<Text>();
        }
        
        // Wrap Text component để hoạt động như TMP_Text
        return new TMP_TextWrapper(text);
    }
    
    // Tạo TMP_InputField component
    public static TMP_InputField CreateTMPInputField(GameObject obj)
    {
        InputField inputField = obj.GetComponent<InputField>();
        if (inputField == null)
        {
            inputField = obj.AddComponent<InputField>();
        }
        
        return new TMP_InputFieldWrapper(inputField);
    }
}

// Wrapper cho TMP_Text
public class TMP_Text
{
    private Text textComponent;
    
    public TMP_Text(Text text)
    {
        textComponent = text;
    }
    
    public string text
    {
        get { return textComponent.text; }
        set { textComponent.text = value; }
    }
    
    public float fontSize
    {
        get { return textComponent.fontSize; }
        set { textComponent.fontSize = (int)value; }
    }
    
    public Color color
    {
        get { return textComponent.color; }
        set { textComponent.color = value; }
    }
    
    public TextAlignmentOptions alignment
    {
        get { return (TextAlignmentOptions)textComponent.alignment; }
        set { textComponent.alignment = (TextAnchor)value; }
    }
    
    public FontWeight fontWeight
    {
        get { return FontWeight.Normal; }
        set { /* Font weight not supported in regular Text */ }
    }
}

// Wrapper cho TMP_InputField
public class TMP_InputField
{
    private InputField inputFieldComponent;
    
    public TMP_InputField(InputField inputField)
    {
        inputFieldComponent = inputField;
    }
    
    public string text
    {
        get { return inputFieldComponent.text; }
        set { inputFieldComponent.text = value; }
    }
    
    public UnityEngine.Events.UnityEvent<string> onEndEdit
    {
        get { return inputFieldComponent.onEndEdit; }
    }
    
    public string placeholder
    {
        get { return inputFieldComponent.placeholder?.GetComponent<Text>()?.text ?? ""; }
        set 
        { 
            if (inputFieldComponent.placeholder != null)
            {
                inputFieldComponent.placeholder.GetComponent<Text>().text = value;
            }
        }
    }
}

// Enum để tương thích
public enum TextAlignmentOptions
{
    Left = 0,
    Center = 1,
    Right = 2,
    Justified = 3
}

public enum FontWeight
{
    Normal = 400,
    Bold = 700
}

// Wrapper classes
public class TMP_TextWrapper : TMP_Text
{
    public TMP_TextWrapper(Text text) : base(text) { }
}

public class TMP_InputFieldWrapper : TMP_InputField
{
    public TMP_InputFieldWrapper(InputField inputField) : base(inputField) { }
} 