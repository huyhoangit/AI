# 🎮 HƯỚNG DẪN GÁN SCRIPT VÀO GAMEOBJECT

## ⚡ **SETUP NHANH (5 PHÚT):**

### **Bước 1: InputStateManager (QUAN TRỌNG NHẤT)**
```
1. Tạo GameObject mới: Create Empty → đặt tên "InputStateManager"
2. Add Component → InputStateManager.cs
3. ✅ XONG! Script tự động setup làm Singleton
```

### **Bước 2: GameInputManager**
```
1. Tìm GameObject "GameManager" trong Scene
2. Add Component → GameInputManager.cs  
3. ✅ XONG! Tự động kết nối với GameManager
```

### **Bước 3: Chat System**
```
1. Tạo GameObject mới: "ChatSystem"
2. Add Component → MessageBubbleCreator.cs
3. Add Component → ChatUIController.cs
4. ✅ Cả 2 script sẽ làm việc cùng nhau
```

### **Bước 4: Chat Animation**
```
1. Tạo GameObject mới: "ChatToggleController"  
2. Add Component → TogglechatPanel.cs
3. Trong Inspector:
   - Chat Panel: Gán GameObject chứa UI chat
   - Toggle Button: Gán button để mở/đóng chat
4. ✅ Animation sẽ hoạt động ngay!
```

---

## 🔧 **SETUP CHI TIẾT:**

### **A. InputStateManager Setup:**
```
GameObject Name: "InputStateManager"
Location: Root level (không là child)
Scripts: InputStateManager.cs
Settings:
  ✅ Show Debug Info = true (để debug)
  ✅ Auto setup khi Start()
  
📝 Lưu ý: Đây là Singleton - chỉ cần 1 instance duy nhất!
```

### **B. GameInputManager Setup:**
```
GameObject: Existing "GameManager" 
Scripts: GameInputManager.cs (thêm vào GameManager)
Auto Features:
  ✅ Tự động tìm InputStateManager
  ✅ Tự động bảo vệ phím R, C, Escape
  ✅ Tự động kết nối với GameManager methods
```

### **C. Chat System Setup:**
```
GameObject Name: "ChatSystem"
Scripts:
  - MessageBubbleCreator.cs
  - ChatUIController.cs
  
MessageBubbleCreator Settings:
  ✅ Max Bubble Width: 300
  ✅ User/AI colors tự động
  
ChatUIController Settings:
  🔗 Input Field: [Gán TMP_InputField]
  🔗 Send Button: [Gán Button]
  🔗 Chat Content: [Gán Transform chứa messages]
  🔗 Chat Scroll Rect: [Gán ScrollRect]
  🔗 Message Bubble Creator: [Gán component MessageBubbleCreator]
```

### **D. Chat Animation Setup:**
```
GameObject Name: "ChatToggleController"
Scripts: TogglechatPanel.cs

Required Inspector Assignments:
  🔗 Chat Panel: [GameObject chứa chat UI]
  🔗 Toggle Button: [Button để mở/đóng]
  
Animation Settings:
  ⏱️ Duration: 0.3s
  🎭 Type: Scale/Slide/Fade/SlideAndFade
  🎹 Toggle Key: C (có thể đổi)
  📏 Container Width: 430px
```

---

## 🚀 **AUTO SETUP (DỄ NHẤT):**

### **Option 1: SimpleChatUICreator**
```
1. Tạo GameObject: "AutoChatSetup"
2. Add Component: SimpleChatUICreator.cs
3. Right-click → "Create Messenger Chat UI"
4. ✅ Tự động tạo toàn bộ chat system!
```

### **Option 2: SafeChatSystemSetup** 
```
1. Tạo GameObject: "SafeChatSetup"
2. Add Component: SafeChatSystemSetup.cs
3. ✅ Setup On Start = true
4. Press Play → Tự động setup!
```

---

## ⚠️ **LƯU Ý QUAN TRỌNG:**

### **Script Dependencies:**
```
InputStateManager ← GameInputManager (cần InputStateManager)
InputStateManager ← TogglechatPanel (dùng safe input)
MessageBubbleCreator ← ChatUIController (dùng bubble creator)
```

### **Thứ tự Setup:**
```
1. InputStateManager (đầu tiên)
2. GameInputManager 
3. Chat System (MessageBubbleCreator + ChatUIController)
4. Chat Animation (TogglechatPanel)
```

### **Kiểm tra hoạt động:**
```
✅ Console không có lỗi
✅ InputStateManager hiển thị debug info
✅ Nhấn C để toggle chat panel
✅ Typing trong InputField không trigger game shortcuts
✅ Nhấn R để toggle wall placement mode
```

---

## 🔍 **DEBUGGING:**

### **Nếu có lỗi:**
```
1. Kiểm tra Console errors
2. Right-click InputStateManager → "Debug Input State"
3. Right-click TogglechatPanel → "Debug Panel Info"
4. Right-click MessageBubbleCreator → "Test Create Bubble"
```

### **Test InputStateManager:**
```
1. Right-click → "Test Key R"
2. Right-click → "Test Key C"  
3. Kiểm tra input protection hoạt động
```

---

## 🎯 **KẾT QUẢ MONG ĐỢI:**

Sau khi setup xong:
```
✅ Nhấn C → Chat panel hiện/ẩn với animation
✅ Nhấn R → Wall placement mode toggle
✅ Typing trong chat → Không trigger game shortcuts
✅ InputStateManager bảo vệ input conflicts
✅ Chat bubbles tự động resize theo nội dung
✅ Smooth animations cho chat panel
✅ Debug tools hoạt động
```

---

## 📝 **SCRIPTS ĐÃ TÍCH HỢP SẴN:**

Các script này **KHÔNG CẦN** gán thêm (đã có sẵn):
```
❌ WallPlacer.cs (đã có InputStateManager integration)
❌ GameManager.cs (sẽ tự động kết nối với GameInputManager)
❌ BoardManager.cs (không cần thay đổi)
```

---

## 🔥 **QUICK START 2 PHÚT:**

```
1. Tạo "InputStateManager" GameObject + InputStateManager.cs
2. Gán GameInputManager.cs vào GameManager existing
3. Tạo "ChatSystem" + MessageBubbleCreator.cs + ChatUIController.cs  
4. Tạo "ChatToggleController" + TogglechatPanel.cs
5. Press Play → Test với phím C và R!
```

**Xong! Hệ thống input protection hoàn chỉnh!** 🎉
