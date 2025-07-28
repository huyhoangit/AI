# 🧠 AI DECISION TREE & ENTROPY ANALYSIS INTEGRATION REPORT

## 📋 TỔNG QUAN

QuoridorAI đã được nâng cấp với **Decision Tree Classifier** và **Entropy Analysis** để tạo ra AI có hành vi thông minh và đa dạng hơn.

---

## 🌳 DECISION TREE CLASSIFIER

### Mục đích
- **Quyết định chiến lược**: AI sẽ quyết định khi nào nên "di chuyển", "đặt wall", hay "tấn công/phòng thủ"
- **Giảm tính máy móc**: Thay vì luôn chọn nước đi có điểm cao nhất, AI sẽ có logic strategy

### Cách hoạt động
```
INPUT FEATURES:
├── aiDistanceToGoal (khoảng cách AI đến đích)
├── humanDistanceToGoal (khoảng cách người chơi đến đích)
├── aiWallsLeft (số wall còn lại của AI)
├── humanWallsLeft (số wall còn lại của người chơi)
├── aiMobility (số nước đi có thể của AI)
├── humanMobility (số nước đi có thể của người chơi)
├── distanceAdvantage (lợi thế khoảng cách)
├── wallAdvantage (lợi thế số wall)
└── Game Phase (Early/Mid/End game)

DECISION RULES:
├── Early Game → Balanced (cân bằng)
├── End Game → Defensive (tập trung về đích)
├── AI Leading → Blocking (chặn đối thủ)
├── AI Behind → Aggressive (tấn công)
├── Low Mobility → Defensive (phòng thủ)
├── High Wall Advantage → Blocking (dùng wall)
└── Low Walls Left → Conservative (tiết kiệm wall)

OUTPUT STRATEGY:
├── Aggressive: Ưu tiên wall placement để block
├── Defensive: Ưu tiên movement về đích
├── Balanced: Cân bằng giữa move và wall
└── Blocking: Chuyên focus vào wall placement
```

### Ưu điểm
- ✅ **Contextual Decision Making**: AI hiểu được tình huống game
- ✅ **Dynamic Strategy**: Thay đổi chiến lược theo game phase
- ✅ **Reduced Predictability**: Khó đoán hành vi của AI

---

## 🎲 ENTROPY ANALYSIS

### Mục đích
- **Tăng tính đa dạng**: Khi có nhiều nước đi tốt, AI sẽ chọn ngẫu nhiên thay vì luôn chọn cái đầu tiên
- **Softmax Selection**: Sử dụng phân phối xác suất để chọn move
- **Anti-Pattern**: Tránh việc AI bị "bắt bài" bởi người chơi

### Cách hoạt động
```
ENTROPY CALCULATION:
├── Lọc top moves (95% của best score)
├── Tính entropy của danh sách moves
├── So sánh với entropy threshold
└── Quyết định selection method

HIGH ENTROPY (> threshold):
├── Sử dụng Softmax Distribution
├── Random selection theo xác suất
└── Tăng tính bất ngờ

LOW ENTROPY (< threshold):
├── Chọn best move truyền thống
├── Đảm bảo performance
└── Tối ưu cho tình huống rõ ràng
```

### Softmax Temperature
- **Cao (>1.0)**: Tăng randomness, AI "creative" hơn
- **Thấp (<1.0)**: Giảm randomness, AI "focused" hơn
- **Default: 1.0**: Cân bằng giữa performance và diversity

---

## ⚙️ THAM SỐ ĐIỀU CHỈNH

### Trong Unity Inspector:
```
[Decision Tree & Entropy Settings]
├── Entropy Threshold: 0.7 (0-1)
│   └── Ngưỡng để kích hoạt random selection
├── Softmax Temperature: 1.0 (0-2)
│   └── Độ "nóng" của phân phối xác suất
├── Use Decision Tree: true
│   └── Bật/tắt Decision Tree
└── Use Entropy Analysis: true
    └── Bật/tắt Entropy Analysis
```

### Tuning Recommendations:
- **Entropy Threshold = 0.5**: AI conservative, ít random
- **Entropy Threshold = 0.9**: AI creative, nhiều random
- **Softmax Temperature = 0.5**: AI focused, ít bất ngờ
- **Softmax Temperature = 1.5**: AI experimental, nhiều bất ngờ

---

## 🔧 INTEGRATION WORKFLOW

### Phase 1: Decision Tree Filtering
```csharp
// Trích xuất features từ game state
DecisionFeatures features = ExtractDecisionFeatures();

// Decision Tree quyết định strategy
StrategyDecision strategy = decisionTree.Predict(features);

// Lọc moves theo strategy
List<AIMove> filteredMoves = FilterMovesByStrategy(allMoves, strategy);
```

### Phase 2: Minimax Evaluation
```csharp
// Đánh giá từng move bằng Minimax
foreach (AIMove move in filteredMoves)
{
    float score = Minimax(ApplyMove(state, move), depth, alpha, beta, false);
    moveScores.Add((move, score));
}
```

### Phase 3: Entropy-Based Selection
```csharp
// Tính entropy và chọn move
float entropy = CalculateEntropyForMoves(topMoves);

if (entropy > entropyThreshold)
{
    // Softmax selection
    selectedMove = SelectMoveWithSoftmax(topMoves);
}
else
{
    // Traditional best move
    selectedMove = topMoves.OrderByDescending(score).First();
}
```

---

## 📊 PERFORMANCE METRICS

### Debug Logs:
```
🌳 Decision Tree filtered to X moves (Strategy: Aggressive)
🎲 Move entropy: 0.876, threshold: 0.700
🎯 Softmax selected move 2 with probability 0.324
📊 AI Performance - Avg Score: 150.25, Variance: 45.30, Entropy: 0.821
🔄 Top Move Patterns: Movement_4_3(8), WallPlacement_2_5(6), Movement_3_4(4)
```

### Analytics:
- **Average Score**: Hiệu suất trung bình của AI
- **Score Variance**: Độ ổn định trong quyết định
- **Entropy**: Mức độ đa dạng trong behavior
- **Move Patterns**: Thống kê pattern phổ biến

---

## 🎯 EXPECTED BENEFITS

### Gameplay Improvements:
- ✅ **Smarter AI**: Context-aware decision making
- ✅ **Diverse Behavior**: Khó predict, tăng replay value
- ✅ **Balanced Challenge**: Vừa thông minh vừa không quá rigid
- ✅ **Learning Curve**: AI adapt theo game phase

### Technical Benefits:
- ✅ **Modular Design**: Có thể tắt/bật từng tính năng
- ✅ **Tunable Parameters**: Điều chỉnh behavior dễ dàng
- ✅ **Debug Friendly**: Extensive logging cho analysis
- ✅ **Performance Monitoring**: Real-time metrics

---

## 🧪 TESTING SCENARIOS

### Để test AI mới:
1. **Early Game**: AI nên có strategy "Balanced"
2. **AI Leading**: AI nên chuyển sang "Blocking"
3. **AI Behind**: AI nên "Aggressive" hơn
4. **Low Walls**: AI nên "Conservative" với wall usage
5. **High Entropy**: AI nên có move selection đa dạng

### Debug Commands:
```csharp
// Kiểm tra Decision Tree
Debug.Log($"Current Strategy: {GetDecisionTreeStrategy()}");

// Kiểm tra Entropy
Debug.Log($"Move Entropy: {CalculateEntropyForMoves(moves)}");

// Kiểm tra Features
var features = ExtractDecisionFeatures();
Debug.Log($"Features: Distance={features.distanceAdvantage}, Walls={features.wallAdvantage}");
```

---

## 📝 IMPLEMENTATION STATUS

### ✅ COMPLETED:
- [x] Decision Tree Classifier với 7 rules
- [x] Entropy Analysis với Softmax selection
- [x] Integration vào FindBestMove workflow
- [x] Debug logging và performance metrics
- [x] Tunable parameters trong Inspector
- [x] Feature extraction từ game state
- [x] Strategy-based move filtering

### 🚀 READY FOR TESTING:
- AI hiện đã có khả năng:
  - Quyết định strategy dựa vào tình huống
  - Chọn move đa dạng khi có nhiều lựa chọn tốt
  - Thích ứng với game phase khác nhau
  - Tránh bị predict quá dễ dàng

---

*Tài liệu này được tạo tự động bởi AI Assistant - Cập nhật: 2025-06-30*
