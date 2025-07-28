# 🎮 GAMEINPUTMANAGER.CS - HƯỚNG DẪN SETUP CHI TIẾT

## 📍 **VỊ TRÍ GÁN SCRIPT:**

### **GameObject: GameManager (có sẵn)**
```
1. Tìm GameObject "GameManager" trong Scene Hierarchy
2. Click vào GameManager 
3. Trong Inspector → Add Component → GameInputManager
4. ✅ Script sẽ gán vào GameManager cùng với GameManager.cs
```

---

## ⚙️ **INSPECTOR SETTINGS:**

### **Key Bindings:**
```
🎹 Reset Key: R (để reset game)
🎹 Chat Toggle Key: C (để mở/đóng chat)  
🎹 Pause Key: Escape (để pause game)
```

### **References (Tự động tìm):**
```
🔗 Game Manager: [Tự động tìm GameManager component]
🔗 Chat Panel: [Tự động tìm TogglechatPanel component]
```

### **Debug:**
```
✅ Show Key Press Log: true (để debug input)
```

---

## 🔧 **AUTO-SETUP FEATURES:**

### **Tự động tìm References:**
```csharp
// Trong Awake(), script tự động tìm:
gameManager = FindFirstObjectByType<GameManager>();
chatPanel = FindFirstObjectByType<TogglechatPanel>();
```

### **Input Protection:**
```csharp
// Tự động sử dụng InputStateManager để bảo vệ input
IsKeyDownSafe(resetKey) // Thay vì Input.GetKeyDown()
```

### **Fallback System:**
```csharp
// Nếu không có InputStateManager, sử dụng fallback
IsInputFieldFocusedFallback() // Manual check
```

---

## 🚀 **CHỨC NĂNG CHÍNH:**

### **1. Reset Game (Phím R):**
```
✅ Tự động tìm GameManager.ResetGame() method
✅ Fallback tìm GameManager.RestartGame() method  
✅ Bảo vệ không trigger khi đang typing
```

### **2. Chat Toggle (Phím C):**
```
✅ Backup cho TogglechatPanel nếu có vấn đề
✅ Tự động tìm TogglechatPanel component
✅ Bảo vệ không trigger khi đang typing
```

### **3. Pause Game (Phím Escape):**
```
✅ Tự động tìm GameManager.PauseGame() method
✅ Bảo vệ không trigger khi đang typing
```

---

## 🔍 **DEBUG TOOLS:**

### **Context Menu (Right-click trong Inspector):**
```
🔧 "Test Reset Key" - Test phím R
🔧 "Test Chat Toggle Key" - Test phím C  
🔧 "Debug Input State" - Debug toàn bộ trạng thái
🔧 "Test All Keys Safety" - Test tất cả phím safety
```

### **Console Logs:**
```
[GameInputManager] Reset key (R) pressed
[GameInputManager] Chat toggle key (C) pressed
[GameInputManager] Key R blocked due to input focus
✅ Game reset via GameManager.ResetGame()
```

---

## 📋 **SETUP STEP-BY-STEP:**

### **Bước 1: Gán Script**
```
1. Tìm GameManager GameObject trong Hierarchy
2. Click GameManager
3. Inspector → Add Component → GameInputManager
```

### **Bước 2: Kiểm tra Auto-Setup**
```
1. Press Play
2. Console sẽ hiện: "GameManager: ✅ Found"
3. Console sẽ hiện: "ChatPanel: ✅ Found" (nếu có TogglechatPanel)
```

### **Bước 3: Test Input Protection**
```
1. Right-click GameInputManager → "Debug Input State"
2. Tạo InputField để test
3. Click vào InputField → type text
4. Nhấn R → sẽ KHÔNG trigger reset (input protected)
5. Click ra ngoài InputField
6. Nhấn R → sẽ trigger reset (input safe)
```

---

## ⚠️ **DEPENDENCIES:**

### **Cần có sẵn:**
```
✅ InputStateManager.cs (script đã setup trước)
✅ GameManager.cs (GameObject GameManager)
🔄 TogglechatPanel.cs (optional, để backup chat toggle)
```

### **Methods cần có trong GameManager:**
```csharp
// GameInputManager sẽ tìm các method này:
public void ResetGame() { } // Hoặc RestartGame()
public void PauseGame() { } // Optional
```

---

## 🎯 **KẾT QUẢ SAU KHI SETUP:**

### **Input Protection:**
```
✅ Nhấn R khi typing → KHÔNG reset game
✅ Nhấn R khi không typing → Reset game
✅ Nhấn C khi typing → KHÔNG toggle chat  
✅ Nhấn C khi không typing → Toggle chat
✅ Nhấn Escape khi typing → KHÔNG pause
✅ Nhấn Escape khi không typing → Pause game
```

### **Auto-Integration:**
```
✅ Tự động kết nối với InputStateManager
✅ Tự động tìm GameManager methods
✅ Tự động tìm ChatPanel component
✅ Fallback system nếu thiếu components
```

### **Debug Information:**
```
✅ Key press logging
✅ Input state monitoring  
✅ Component status checking
✅ Safety testing tools
```

---

## 🔧 **TROUBLESHOOTING:**

### **Nếu không hoạt động:**
```
1. Kiểm tra Console errors
2. Right-click → "Debug Input State"
3. Kiểm tra InputStateManager đã setup chưa
4. Kiểm tra GameManager có methods reset chưa
```

### **Nếu không tìm thấy references:**
```
1. Manually gán GameManager reference trong Inspector
2. Manually gán TogglechatPanel reference trong Inspector
3. Hoặc đảm bảo các GameObject có đúng component names
```

---

## 📝 **NOTES:**

### **Script Location:**
```
📁 Assets/scripts/GameInputManager.cs
🎯 GameObject: GameManager (existing)
🔗 Works with: InputStateManager, TogglechatPanel, GameManager
```

### **Key Features:**
```
🛡️ Input protection via InputStateManager
🔄 Auto-finding references via FindFirstObjectByType
📞 Reflection-based method calling
🔧 Comprehensive debug tools
⚡ Fallback systems for robustness
```

### **Integration Status:**
```
✅ Ready to use immediately after adding to GameManager
✅ No manual wiring required
✅ Auto-detects and integrates with existing systems
✅ Production-ready with full error handling
```

---

## 🚀 **QUICK START:**

```
1. Add GameInputManager.cs to GameManager GameObject
2. Press Play
3. Test: Nhấn R để reset, C để chat, Escape để pause
4. Test protection: Type trong InputField → phím không trigger
5. ✅ Done! Input protection system hoạt động!
```

**GameInputManager sẽ làm việc seamlessly với InputStateManager và các script khác!** 🎉
