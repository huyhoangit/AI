# 🎯 Q-TABLE FOCUSED DEBUG GUIDE

## 🔍 Tập trung chỉ vào Q-table loading issue

### ✅ System hiện tại đã sẵn sàng:
- QLearningAgent.cs có debug methods hoàn chỉnh
- QuoridorAI.cs có context menu methods 
- Enhanced logging cho LoadQTable() và SaveQTable()

## 🚀 QUICK TEST - 3 BƯỚC

### Bước 1: Kiểm tra Q-table hiện tại
```
Unity → Chọn GameObject có QuoridorAI → Right-click → "Debug Q-Learning System"
```
**Kết quả mong đợi:**
```
=== QUORIDOR AI Q-LEARNING DEBUG ===
🎯 Player ID: 2
🔧 Use Q-Learning: true/false
📂 Q-Table Path: Assets/qtable.json
📂 Full Q-Table Path: d:\unity\AI\AI_Final\Assets\qtable.json
```

### Bước 2: Test serialization format
```
Unity → Right-click → "Test Q-Table Serialization"
```
**Kết quả mong đợi:**
```
🧪 Testing Q-table serialization...
✅ Serialization successful. JSON: {...}
✅ Deserialization successful. States: 2
```

### Bước 3: Force reload với detailed logging
```
Unity → Right-click → "Reload Q-Table"
```
**Kết quả mong đợi:**
```
🔄 Force reloading Q-table...
🔍 Attempting to load Q-table from: Assets/qtable.json
🔍 Full path: d:\unity\AI\AI_Final\Assets\qtable.json
✅ File exists: Assets/qtable.json
```

## 🔧 QLearningAgent.cs Key Methods

### LoadQTable() - Enhanced với detailed logging:
```csharp
public void LoadQTable(string path)
{
    Debug.Log($"🔍 Attempting to load Q-table from: {path}");
    Debug.Log($"🔍 Full path: {Path.GetFullPath(path)}");
    
    if (File.Exists(path))
    {
        Debug.Log($"✅ File exists: {path}");
        string jsonContent = File.ReadAllText(path);
        Debug.Log($"📄 File size: {jsonContent.Length} characters");
        Debug.Log($"📝 JSON content preview: {jsonContent.Substring(0, Mathf.Min(100, jsonContent.Length))}...");
        
        // Parse JSON với error handling
        var serialization = JsonUtility.FromJson<Serialization<string, Dictionary<string, float>>>(jsonContent);
        QTable = serialization.ToDictionary();
        Debug.Log($"✅ Q-table loaded successfully - Total states: {QTable.Count}");
    }
    else
    {
        Debug.Log($"ℹ️ Q-table file {path} not found. Starting with empty Q-table.");
        QTable = new Dictionary<string, Dictionary<string, float>>();
    }
}
```

### TestSerialization() - Verify JSON format:
```csharp
public void TestSerialization()
{
    // Tạo test data
    var testQTable = new Dictionary<string, Dictionary<string, float>>();
    testQTable["test_state_1"] = new Dictionary<string, float>
    {
        {"M:1,2", 0.5f},
        {"W:2,3,true", -0.2f}
    };
    
    // Test serialize/deserialize
    var serialization = new Serialization<string, Dictionary<string, float>>(testQTable);
    string json = JsonUtility.ToJson(serialization, true);
    var deserialization = JsonUtility.FromJson<Serialization<string, Dictionary<string, float>>>(json);
}
```

## 🎯 Common Q-table Issues & Solutions

### Issue 1: "Q-table file not found"
**Debug steps:**
1. Check file path: `Debug.Log($"Full path: {Path.GetFullPath(path)}")` 
2. Check directory exists: `Directory.Exists(Path.GetDirectoryName(path))`
3. List files in directory: `Directory.GetFiles(directory, "*.json")`

**Solution:** 
- Ensure game đã chạy ít nhất 1 lần để tạo Q-table
- Hoặc force save: Right-click → "Save Q-Table"

### Issue 2: "JSON parsing failed"
**Debug steps:**
1. Check file content: `File.ReadAllText(path)`
2. Verify JSON format với TestSerialization()
3. Check serialization structure

**Solution:**
- Delete corrupted qtable.json
- Run TestSerialization() để verify format
- Let system recreate file

### Issue 3: "Empty Q-table"
**Debug steps:**
1. Check `QTable.Count` trong DebugQTable()
2. Verify UpdateQ() được gọi trong game
3. Check learning parameters: alpha, gamma, epsilon

**Solution:**
- Play game vài turn để generate Q-data
- Ensure useQLearning = true
- Check reward system working

## 📊 Expected Q-table Structure

### JSON Format:
```json
{
  "keys": [
    "4,8-4,0-10-10-",
    "4,7-4,0-10-10-"
  ],
  "values": [
    {
      "keys": ["M:4,7"],
      "values": [0.1]
    },
    {
      "keys": ["M:4,6", "W:4,7,true"],
      "values": [0.5, -0.2]
    }
  ]
}
```

### State Encoding:
- Format: `{aiPos.x},{aiPos.y}-{humanPos.x},{humanPos.y}-{aiWalls}-{humanWalls}-{wallList}`
- Example: `"4,8-4,0-10-10-"` (no walls placed)

### Action Encoding:
- Movement: `"M:{x},{y}"` 
- Wall: `"W:{x},{y},{isHorizontal}"`

## 🎮 Practical Testing Steps

### 1. Fresh Start Test:
```
1. Delete Assets/qtable.json (if exists)
2. Open Unity, start game
3. Right-click QuoridorAI → "Debug Q-Learning System"
4. Should show: "Q-table file not found. Starting with empty Q-table."
```

### 2. Generate Q-data Test:
```
1. Play 2-3 game moves in Unity
2. Right-click → "Save Q-Table" 
3. Right-click → "Debug Q-Learning System"
4. Should show: "Q-table loaded successfully - Total states: X"
```

### 3. Reload Test:
```
1. Right-click → "Reload Q-Table"
2. Check Console for detailed loading process
3. Verify states count > 0
```

## 🔍 Console Output Analysis

### ✅ Successful Loading:
```
🔍 Attempting to load Q-table from: Assets/qtable.json
🔍 Full path: d:\unity\AI\AI_Final\Assets\qtable.json
✅ File exists: Assets/qtable.json
📄 File size: 1543 characters
📝 JSON content preview: {"keys":["4,8-4,0-10-10-"],"values":[...
✅ Q-table loaded successfully - Total states: 15
```

### ❌ File Not Found:
```
🔍 Attempting to load Q-table from: Assets/qtable.json
🔍 Full path: d:\unity\AI\AI_Final\Assets\qtable.json
ℹ️ Q-table file Assets/qtable.json not found. Starting with empty Q-table.
🔍 Checking directory: d:\unity\AI\AI_Final\Assets
📁 Directory exists. Files in directory:
   📄 (other files, no qtable.json)
```

### ⚠️ Parsing Error:
```
🔍 Attempting to load Q-table from: Assets/qtable.json
✅ File exists: Assets/qtable.json
📄 File size: 234 characters
📝 JSON content preview: {corrupted json...
⚠️ JSON parsing error: JsonException: ...
```

## 🎯 Goal: Identify exact reason tại sao "không load được q-table.json"

Run through these tests và báo cáo kết quả để pinpoint chính xác issue!
