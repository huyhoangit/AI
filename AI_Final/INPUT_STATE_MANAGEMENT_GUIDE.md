# 🎮 Input State Management System

## 📋 Tổng Quan
Hệ thống quản lý trạng thái nhập liệu toàn cục, đảm bảo các phím tắt game (như R, C, Escape) không hoạt động khi đang nhập text trong InputField.

## ✨ Tính Năng

### 🔒 **InputStateManager:**
- ✅ Detect focus trên TMP_InputField và Legacy InputField
- ✅ Singleton pattern - available ở mọi nơi  
- ✅ Events khi input state thay đổi
- ✅ Safe input checking methods
- ✅ Debug tools và real-time monitoring

### 🎮 **GameInputManager:**
- ✅ Quản lý tất cả game input keys (R, C, Escape)
- ✅ Tích hợp với InputStateManager để block inputs
- ✅ Auto-find GameManager và ChatPanel references
- ✅ Fallback mechanism nếu không có InputStateManager

### 🎯 **TogglechatPanel Integration:**
- ✅ Sử dụng InputStateManager cho phím C
- ✅ Fallback về local check nếu cần
- ✅ Enhanced debug info

## 🚀 Setup Instructions

### 1. Tạo InputStateManager
```
1. Tạo empty GameObject tên "InputStateManager"
2. Gắn script InputStateManager.cs
3. Check "Don't Destroy On Load" (script tự động làm)
4. Optionally enable "Show Debug Info" trong Inspector
```

### 2. Tạo GameInputManager  
```
1. Tạo empty GameObject tên "GameInputManager"
2. Gắn script GameInputManager.cs
3. Script sẽ tự động tìm GameManager và ChatPanel
4. Hoặc assign manual trong Inspector nếu cần
```

### 3. Verify Integration
```
- TogglechatPanel sẽ tự động sử dụng InputStateManager
- Không cần thay đổi gì thêm trong existing scripts
```

## 🎮 Key Bindings Default

```csharp
Reset Game: KeyCode.R
Chat Toggle: KeyCode.C  
Pause Game: KeyCode.Escape
```

## 🔧 Programming API

### InputStateManager:
```csharp
// Check nếu đang focus input
bool focused = InputStateManager.Instance.IsInputFieldFocused;

// Safe input checking
bool keyPressed = InputStateManager.Instance.GetKeyDownSafe(KeyCode.R);

// Force block/unblock game inputs
InputStateManager.Instance.SetBlockGameInputs(true, "Menu open");

// Check nếu key bị block
bool blocked = InputStateManager.Instance.IsKeyBlocked(KeyCode.R);

// Listen for input state changes
InputStateManager.Instance.OnInputStateChanged += (focused) => {
    Debug.Log($"Input focus changed: {focused}");
};
```

### GameInputManager:
```csharp
// Change key bindings
gameInputManager.SetKeyBindings(KeyCode.F5, KeyCode.T, KeyCode.P);

// Manual trigger actions
gameInputManager.TestResetKey();
gameInputManager.TestChatToggleKey();
```

## 🧪 Testing & Debug

### Context Menu Commands:

**InputStateManager:**
- `Debug Input State` - Xem trạng thái hiện tại
- `Test Key R` - Test phím R có bị block không  
- `Test Key C` - Test phím C có bị block không

**GameInputManager:**
- `Test Reset Key` - Test reset game
- `Test Chat Toggle Key` - Test toggle chat
- `Debug Input State` - Xem input state
- `Test All Keys Safety` - Test tất cả keys

**TogglechatPanel:**
- `Debug Panel Info` - Xem InputStateManager integration

### Manual Testing:
```
1. Click vào InputField để focus
2. Try nhấn R hoặc C → Should NOT work
3. Click outside InputField để unfocus  
4. Try nhấn R hoặc C → Should work normally
5. Check Console logs để verify behavior
```

## 📊 Debug Output Examples

### Normal State (No Input Focus):
```
[InputStateManager] Input focus changed: UNFOCUSED - Object: None
[GameInputManager] Safe key press: R
✅ Game reset via GameManager.ResetGame()
```

### Blocked State (Input Focused):
```
[InputStateManager] Input focus changed: FOCUSED - Object: ChatInputField
[GameInputManager] Key R blocked due to input focus
[InputStateManager] Key R blocked - Input field focused: True, Force blocked: False
```

## 🔄 Integration với Existing Code

### Automatic Integration:
- **TogglechatPanel**: Tự động detect và sử dụng InputStateManager
- **GameManager**: GameInputManager sẽ tự động gọi reset methods
- **No Breaking Changes**: Existing code vẫn hoạt động bình thường

### GameManager Methods Support:
```csharp
// GameInputManager sẽ tự động tìm và gọi:
gameManager.ResetGame()     // Primary
gameManager.RestartGame()   // Fallback  
gameManager.PauseGame()     // For Escape key
```

## 🎯 Use Cases

### 1. Chat System:
- User đang gõ tin nhắn trong chat
- Nhấn R để reset game → **BLOCKED** ✅
- Click outside chat, nhấn R → Reset normally ✅

### 2. Settings Menu:
```csharp
// Khi mở settings menu
InputStateManager.Instance.SetBlockGameInputs(true, "Settings menu open");

// Khi đóng settings menu  
InputStateManager.Instance.SetBlockGameInputs(false);
```

### 3. Pause Game:
- Nhấn Escape → Pause game
- Nếu đang trong InputField → **BLOCKED** ✅

## 🔧 Advanced Configuration

### Custom Key Bindings:
```csharp
void Start()
{
    // Change key bindings at runtime
    var gameInput = FindFirstObjectByType<GameInputManager>();
    gameInput.SetKeyBindings(KeyCode.F5, KeyCode.T, KeyCode.P);
}
```

### Event Listening:
```csharp
void Start()
{
    if (InputStateManager.Instance != null)
    {
        InputStateManager.Instance.OnInputStateChanged += OnInputFocusChanged;
    }
}

void OnInputFocusChanged(bool focused)
{
    if (focused)
    {
        // User started typing - maybe show typing indicator
        ShowTypingIndicator();
    }
    else
    {
        // User finished typing - hide indicator
        HideTypingIndicator();
    }
}
```

## ✅ Verification Checklist

After setup, verify:

- [ ] ✅ InputStateManager GameObject exists in scene
- [ ] ✅ GameInputManager GameObject exists in scene  
- [ ] ✅ R key blocked when typing in chat
- [ ] ✅ C key blocked when typing in chat
- [ ] ✅ R key works normally when not typing
- [ ] ✅ C key works normally when not typing
- [ ] ✅ No console errors
- [ ] ✅ Debug commands work in Context Menu

## 🎉 Hoàn Thành!

Bây giờ game của bạn có:
- 🔒 **Smart input blocking** khi đang typing
- 🎮 **Centralized key management** 
- 🐛 **Comprehensive debugging tools**
- 🔄 **Backward compatibility**
- ⚡ **Performance optimized**

**Professional game input handling achieved!** 🚀
