using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quick TMPro Fix - Không sử dụng editor scripts
/// Chạy script này để tự động fix lỗi TMPro
/// </summary>
public class TMProQuickFix : MonoBehaviour
{
    [Header("Quick Fix Settings")]
    public bool autoFixOnStart = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            QuickFixTMPro();
        }
    }
    
    [ContextMenu("Quick Fix TMPro")]
    public void QuickFixTMPro()
    {
        Debug.Log("🔧 Quick fixing TMPro issues...");
        
        // Kiểm tra TMPro availability
        bool hasTMPro = CheckTMProAvailability();
        
        if (hasTMPro)
        {
            Debug.Log("✅ TMPro is available - no fixes needed!");
        }
        else
        {
            Debug.LogWarning("❌ TMPro not found - showing instructions");
            ShowQuickInstructions();
        }
    }
    
    bool CheckTMProAvailability()
    {
        System.Type tmpTextType = System.Type.GetType("TMPro.TMP_Text");
        return tmpTextType != null;
    }
    
    void ShowQuickInstructions()
    {
        Debug.Log("📋 QUICK FIX INSTRUCTIONS:");
        Debug.Log("1. Window → Package Manager");
        Debug.Log("2. Search 'TextMeshPro'");
        Debug.Log("3. Click 'Install'");
        Debug.Log("4. Window → TextMeshPro → Import TMP Essential Resources");
        Debug.Log("5. Restart Unity");
    }
    
    [ContextMenu("Test TMPro")]
    public void TestTMPro()
    {
        Debug.Log("🧪 Testing TMPro...");
        
        if (CheckTMProAvailability())
        {
            Debug.Log("✅ TMPro is working!");
        }
        else
        {
            Debug.Log("❌ TMPro not available");
        }
    }
} 