# 🚀 HƯỚNG DẪN TEST MODEL LOCAL OLLAMA

## 📥 BƯỚC 1: CÀI ĐẶT OLLAMA

### **Download & Install:**
1. **Truy cập:** https://ollama.ai/download
2. **Download** phiên bản cho Windows
3. **Cài đặt** file .exe đã tải về
4. **Khởi động** Ollama (tự động chạy background)

### **Kiểm tra cài đặt:**
```bash
# Mở Command Prompt hoặc PowerShell
ollama --version
```

## 🤖 BƯỚC 2: TẢI MODEL

### **Tải model nhẹ (khuyến nghị):**
```bash
# Model nhỏ (~3.8GB) - phù hợp máy yếu
ollama pull llama2:7b

# Hoặc model rất nhỏ (~1.9GB)
ollama pull llama2:chat

# Model tiếng Việt (nếu có)
ollama pull vinallama
```

### **Test model:**
```bash
# Chạy thử model
ollama run llama2:7b

# Test câu hỏi
>>> Bạn là AI hỗ trợ game Quoridor. Quoridor là gì?
```

## 🎮 BƯỚC 3: CẤU HÌNH UNITY

### **Trong HybridAIManager:**
1. **Đảm bảo cấu hình đúng:**
   - `ollamaURL = "http://localhost:11434/api/generate"`
   - `ollamaModel = "llama2:7b"` (hoặc model bạn đã tải)
   - `primaryAPI = "ollama"`

### **Test trong Unity:**
1. **Tạo GameObject mới**
2. **Add component CompleteChatSystemSetup**
3. **Tick "Setup Complete System"**
4. **Play game và test chat**

## 🔍 BƯỚC 4: DEBUG & KIỂM TRA

### **Kiểm tra Ollama đang chạy:**
```bash
# Check service
curl http://localhost:11434/api/version

# Hoặc trong browser
http://localhost:11434
```

### **Debug trong Unity Console:**
- Tìm messages: `🔄 Trying ollama API...`
- Nếu thành công: `✅ ollama API success`
- Nếu lỗi: `❌ ollama API failed`

## ⚡ QUICK TEST SCRIPT

### **Test nhanh Ollama từ Unity:**
```csharp
[ContextMenu("Test Ollama")]
public void TestOllama()
{
    GetAIResponse("Quoridor là gì?", (response, success) => {
        if (success)
            Debug.Log($"✅ Ollama works: {response}");
        else
            Debug.LogError($"❌ Ollama failed: {response}");
    });
}
```

## 🛠️ TROUBLESHOOTING

### **Lỗi thường gặp:**

**1. Connection refused:**
- Ollama chưa được start
- Chạy: `ollama serve`

**2. Model not found:**
- Model chưa được pull
- Chạy: `ollama pull llama2:7b`

**3. Timeout:**
- Tăng `timeoutSeconds` trong HybridAIManager
- Model lần đầu chạy chậm

**4. Empty response:**
- Check model có đúng format không
- Thử model khác: `ollama pull llama2:chat`

## 📊 KIỂM TRA HOẠT ĐỘNG

### **Trong Unity Console sẽ thấy:**
```
🔧 Hybrid AI Manager initialized
Primary: ollama, Fallback: huggingface
🔄 Trying ollama API...
✅ ollama API success
```

### **Nếu thành công:**
- Chat bot sẽ trả lời bằng model local
- Không cần internet sau khi model đã tải
- Tốc độ phụ thuộc vào cấu hình máy

---

**🎯 READY TO TEST!** 
**Sau khi hoàn thành các bước trên, model local Ollama sẽ hoạt động trong game!**
