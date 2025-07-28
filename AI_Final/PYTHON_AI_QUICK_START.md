# 🐍 PYTHON LOCAL AI - QUICK START GUIDE

## ✨ KHÔNG CẦN TẢI GÌ CẢ - CHỈ CẦN PYTHON!

### **🚀 SETUP 1-CLICK (Khuyến nghị):**
1. **Chạy batch file:**
   ```bash
   start-python-ai.bat
   ```
   - Tự động cài đặt dependencies
   - Khởi động Python AI server
   - Ready for Unity!

### **🔧 SETUP THỦ CÔNG:**
1. **Cài Python** (nếu chưa có): https://python.org
2. **Install dependencies:**
   ```bash
   pip install flask
   ```
3. **Start AI server:**
   ```bash
   python simple_quoridor_ai.py
   ```

## 🎮 UNITY INTEGRATION:

### **Option 1: Sử dụng PythonLocalAI (đơn giản)**
1. Add component `PythonLocalAI` vào GameObject
2. Click "Test Python AI" trong Inspector
3. Xem kết quả trong Console

### **Option 2: Sử dụng HybridAIManager (advanced)**
1. Set `primaryAPI = "python"`
2. Set `pythonAIURL = "http://localhost:5000/chat"`
3. Sử dụng ChatUIManager như bình thường

## 🧠 AI FEATURES:

### **Self-Learning:**
- ✅ Học từ mỗi cuộc hội thoại
- ✅ Lưu patterns và cải thiện responses
- ✅ Auto-save learning data

### **Game Knowledge:**
- ✅ Hiểu luật chơi Quoridor
- ✅ Đưa ra chiến thuật
- ✅ Trả lời về movement và walls

### **API Endpoints:**
- `POST /chat` - Chat với AI
- `POST /train` - Train AI với data mới
- `GET /status` - Kiểm tra AI status
- `POST /reset` - Reset AI memory
- `GET /export` - Export AI data

## 🎯 TESTING:

### **Test Python AI:**
```csharp
// Trong Unity, add PythonLocalAI component
[ContextMenu("Test Python AI")]
public void TestPythonAI()
{
    GetAIResponse("Quoridor là gì?", (response, success, confidence) =>
    {
        Debug.Log($"AI Response: {response}");
        Debug.Log($"Confidence: {confidence}");
    });
}
```

### **Expected Output:**
```
✅ Python AI server is running!
🤖 Python AI Response: Quoridor là game board strategy nổi tiếng cho 2-4 người chơi...
📊 Confidence: 0.90
```

## 💡 ADVANTAGES:

### **Vs Ollama:**
- ✅ **Nhẹ hơn** (không cần tải model GB)
- ✅ **Nhanh hơn** (custom cho game)
- ✅ **Linh hoạt** (dễ customize)
- ✅ **Học được** (improve theo thời gian)

### **Vs Cloud APIs:**
- ✅ **Offline** (không cần internet)
- ✅ **Free** (không giới hạn requests)
- ✅ **Private** (data không ra ngoài)
- ✅ **Low latency** (local processing)

## 🛠️ TROUBLESHOOTING:

### **Server không start:**
```bash
# Check Python
python --version

# Check Flask
pip list | grep flask

# Manual start
python simple_quoridor_ai.py
```

### **Unity không connect:**
- Check server đang chạy: http://localhost:5000/status
- Check firewall settings
- Verify URL trong Unity: `http://localhost:5000/chat`

### **Empty responses:**
- Check Console logs trong Python
- Server learning từ conversations
- Thử train với data mới

## 🎉 READY TO USE!

### **Files cần thiết:**
- ✅ `simple_quoridor_ai.py` - Python AI server
- ✅ `PythonLocalAI.cs` - Unity component
- ✅ `start-python-ai.bat` - Auto setup
- ✅ `requirements.txt` - Dependencies

### **Workflow:**
1. Run `start-python-ai.bat`
2. Add `PythonLocalAI` component trong Unity
3. Test và enjoy!

---

**🎯 HOÀN TOÀN TỰ CHỦ - KHÔNG CẦN EXTERNAL MODELS!**
