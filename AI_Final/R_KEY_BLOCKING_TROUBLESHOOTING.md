# 🐛 PHÍM R KHÔNG CHẶN ĐƯỢC - TROUBLESHOOTING GUIDE

## 🔍 **VẤN ĐỀ PHÁT HIỆN:**
- Phím C đã được chặn khi typing
- Phím R vẫn chưa được chặn khi typing

## 🔧 **CÁC THAY ĐỔI ĐÃ THỰC HIỆN:**

### **1. GameInputManager.cs:**
```csharp
// ĐÃ SỬA: resetKey từ Tab về R
[SerializeField] private KeyCode resetKey = KeyCode.R; // Trước đây là KeyCode.Tab
```

### **2. WallPlacer.cs Update():**
```csharp
// ĐÃ LOẠI BỎ: Xử lý KeyCode.R trực tiếp
void Update()
{
    // KeyCode.R đã được chuyển sang GameInputManager để xử lý tập trung
    // GameInputManager sẽ gọi ToggleWallPlaceMode() khi cần thiết
    // Không xử lý input trực tiếp ở đây nữa để tránh xung đột
}
```

### **3. ChessPlayer.cs:**
```csharp
// ĐÃ THÊM: Input protection cho phím R
if (InputStateManager.Instance != null ? 
    InputStateManager.Instance.GetKeyDownSafe(KeyCode.R) : 
    Input.GetKeyDown(KeyCode.R))
{
    ToggleWallPlacement();
}
```

---

## 🧪 **TESTING STEPS:**

### **Step 1: Kiểm tra InputStateManager Debug:**
```
1. Right-click InputStateManager trong Inspector
2. Click "Test All Key Protection"
3. Kiểm tra Console output:
   - Input Field Focused: true/false
   - Key R Blocked: true/false
   - Key R Safe Check: true/false
```

### **Step 2: Kiểm tra GameInputManager Debug:**
```
1. Right-click GameInputManager trong Inspector  
2. Click "Enable Debug Logging"
3. Click "Debug Input State"
4. Test với chat input focus:
   - Click vào chat InputField
   - Nhấn R → Console should show "Key R blocked due to input focus"
   - Click ra ngoài chat
   - Nhấn R → Console should show "Safe key press: R"
```

### **Step 3: Manual Testing:**
```
1. Mở game
2. Click vào chat InputField (để focus)
3. Nhấn R → Không được trigger wall toggle
4. Click ra ngoài chat
5. Nhấn R → Được trigger wall toggle
```

---

## 🔍 **POTENTIAL ISSUES & SOLUTIONS:**

### **Issue 1: Multiple Scripts xử lý cùng phím R**
```
❌ Problem: WallPlacer, ChessPlayer, GameInputManager cùng xử lý
✅ Solution: Chỉ GameInputManager xử lý, các script khác bị disable input
```

### **Issue 2: InputStateManager không detect InputField focus**
```
❌ Problem: IsAnyInputFieldFocused() return false khi đang typing
✅ Solution: Kiểm tra EventSystem và TMP_InputField.isFocused
```

### **Issue 3: GameInputManager resetKey sai**
```
❌ Problem: resetKey = KeyCode.Tab thay vì KeyCode.R
✅ Solution: Đổi về KeyCode.R
```

### **Issue 4: Script execution order**
```
❌ Problem: InputStateManager chạy sau GameInputManager
✅ Solution: Đảm bảo InputStateManager chạy trước
```

---

## 🛠️ **DEBUG COMMANDS:**

### **InputStateManager:**
```
Right-click trong Inspector:
- "Debug Input State" - Xem trạng thái general
- "Test Key R" - Test riêng phím R
- "Test Key C" - Test riêng phím C  
- "Test All Key Protection" - Test comprehensive
```

### **GameInputManager:**
```
Right-click trong Inspector:
- "Enable Debug Logging" - Bật log chi tiết
- "Debug Input State" - Xem trạng thái + current player
- "Test Wall Toggle (R Key)" - Test wall toggle logic
- "Test All Keys Safety" - Test tất cả phím safety
```

---

## 📊 **EXPECTED DEBUG OUTPUT:**

### **Khi typing trong chat (phím R bị chặn):**
```
Console Output:
[InputStateManager] Input focus changed: FOCUSED - Object: ChatInputField
[GameInputManager] Key R blocked due to input focus
```

### **Khi không typing (phím R hoạt động):**
```
Console Output:
[InputStateManager] Input focus changed: UNFOCUSED - Object: None  
[GameInputManager] Safe key press: R
[GameInputManager] Reset key (R) pressed
✅ Wall placement mode toggled for Player 1
```

---

## 🎯 **VERIFICATION CHECKLIST:**

### **Setup Verification:**
- [ ] ✅ InputStateManager GameObject exists
- [ ] ✅ GameInputManager added to GameManager  
- [ ] ✅ resetKey = KeyCode.R (not Tab)
- [ ] ✅ WallPlacer.Update() không xử lý KeyCode.R
- [ ] ✅ ChessPlayer có input protection

### **Runtime Verification:**
- [ ] ✅ InputStateManager.Instance != null
- [ ] ✅ IsAnyInputFieldFocused() works correctly
- [ ] ✅ GetKeyDownSafe(KeyCode.R) blocks when typing
- [ ] ✅ GameInputManager receives R key events
- [ ] ✅ No multiple scripts processing same R key

### **Behavior Verification:**
- [ ] ✅ R key blocked when typing in chat
- [ ] ✅ R key works when not typing
- [ ] ✅ C key blocked when typing (working)
- [ ] ✅ No console errors

---

## 🚨 **COMMON MISTAKES:**

### **1. Forgotten Key Binding:**
```
❌ resetKey vẫn là Tab
✅ resetKey phải là R
```

### **2. Multiple Input Handlers:**
```
❌ WallPlacer + GameInputManager cùng xử lý R
✅ Chỉ GameInputManager xử lý R
```

### **3. InputStateManager Singleton Missing:**
```
❌ InputStateManager.Instance == null
✅ InputStateManager GameObject phải exist trong scene
```

### **4. EventSystem Missing:**
```
❌ EventSystem.current == null
✅ Scene phải có EventSystem để detect UI focus
```

---

## 🔄 **NEXT STEPS:**

1. **Test comprehensive**: Chạy all debug commands
2. **Verify console logs**: Đảm bảo R key được log correctly
3. **Manual test**: Test typing + R key behavior
4. **Check execution order**: InputStateManager trước GameInputManager
5. **Verify singleton**: InputStateManager.Instance != null

**Sau khi thực hiện các bước trên, phím R sẽ được chặn đúng cách khi typing!** 🎯
