using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Script để fix các vấn đề với Q-table
/// </summary>
public class QTableFixer : MonoBehaviour
{
    [Header("Q-Table Settings")]
    public string qTablePath = "Assets/qtable.json";
    public bool autoFixOnStart = true;
    public bool deleteCorruptedFile = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixQTable();
        }
    }
    
    [ContextMenu("Fix Q-Table")]
    public void FixQTable()
    {
        Debug.Log("🔧 Starting to fix Q-table issues...");
        
        if (deleteCorruptedFile)
        {
            DeleteCorruptedQTable();
        }
        
        CreateFreshQTable();
        
        Debug.Log("✅ Q-table fix complete!");
    }
    
    /// <summary>
    /// Xóa file Q-table bị lỗi
    /// </summary>
    void DeleteCorruptedQTable()
    {
        if (File.Exists(qTablePath))
        {
            try
            {
                File.Delete(qTablePath);
                Debug.Log($"🗑️ Deleted corrupted Q-table file: {qTablePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error deleting Q-table file: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"ℹ️ Q-table file not found: {qTablePath}");
        }
    }
    
    /// <summary>
    /// Tạo Q-table mới
    /// </summary>
    void CreateFreshQTable()
    {
        try
        {
            // Tạo Q-table rỗng nhưng hợp lệ
            var emptyQTable = new Dictionary<string, Dictionary<string, float>>();
            
            // Thêm một số state mẫu
            string sampleState = "4,0-4,8-10-10-";
            emptyQTable[sampleState] = new Dictionary<string, float>();
            emptyQTable[sampleState]["M:4,1"] = 0.1f;
            emptyQTable[sampleState]["M:3,0"] = 0.1f;
            emptyQTable[sampleState]["M:5,0"] = 0.1f;
            
            // Serialize và lưu
            var serialization = new Serialization<string, Dictionary<string, float>>(emptyQTable);
            string jsonContent = JsonUtility.ToJson(serialization, true);
            
            // Đảm bảo thư mục tồn tại
            string directory = Path.GetDirectoryName(qTablePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Ghi file
            File.WriteAllText(qTablePath, jsonContent);
            Debug.Log($"✅ Created fresh Q-table at: {qTablePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error creating fresh Q-table: {e.Message}");
        }
    }
    
    /// <summary>
    /// Reset Q-table về trạng thái ban đầu
    /// </summary>
    [ContextMenu("Reset Q-Table")]
    public void ResetQTable()
    {
        Debug.Log("🔄 Resetting Q-table...");
        
        DeleteCorruptedQTable();
        CreateFreshQTable();
        
        Debug.Log("✅ Q-table reset complete!");
    }
    
    /// <summary>
    /// Kiểm tra Q-table có hợp lệ không
    /// </summary>
    [ContextMenu("Validate Q-Table")]
    public void ValidateQTable()
    {
        if (!File.Exists(qTablePath))
        {
            Debug.LogWarning("⚠️ Q-table file does not exist!");
            return;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(qTablePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogWarning("⚠️ Q-table file is empty!");
                return;
            }
            
            var serialization = JsonUtility.FromJson<Serialization<string, Dictionary<string, float>>>(jsonContent);
            if (serialization != null && serialization.keys != null && serialization.values != null)
            {
                Debug.Log("✅ Q-table is valid!");
            }
            else
            {
                Debug.LogWarning("⚠️ Q-table is corrupted!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Q-table validation error: {e.Message}");
        }
    }
} 