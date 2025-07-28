# 🤖 TỰ TRAIN MODEL LOCAL BẰNG PYTHON - KHÔNG CẦN OLLAMA

## 🎯 OVERVIEW

Thay vì tải Ollama, bạn có thể:
1. **Tự train model Python** với game data
2. **Export model** sang Unity-compatible format
3. **Sử dụng local Python API** server
4. **Hoặc convert thành C# model**

## 🐍 PHƯƠNG PHÁP 1: PYTHON LOCAL SERVER

### **Tạo Simple AI Model với Python:**

```python
# simple_quoridor_ai.py
import json
import re
from flask import Flask, request, jsonify
from datetime import datetime
import random

class QuoridorAI:
    def __init__(self):
        self.knowledge_base = self.load_training_data()
        self.conversation_history = []
        
    def load_training_data(self):
        # Game knowledge base
        return {
            "quoridor_rules": [
                "Quoridor là game cờ vua 2 người chơi trên bàn cờ 9x9",
                "Mục tiêu là đưa quân cờ về đích đối diện",
                "Mỗi người có 10 bức tường để chặn đối thủ",
                "Tường phải để lại ít nhất 1 đường đi cho đối thủ"
            ],
            "strategies": [
                "Ưu tiên tiến về phía trước khi có thể",
                "Dùng tường để chặn khi đối thủ gần đích",
                "Không lãng phí tường quá sớm",
                "Luôn để lại đường thoát cho mình"
            ],
            "common_questions": {
                "quoridor là gì": "Quoridor là game board strategy cho 2-4 người chơi. Mục tiêu là đưa quân cờ của bạn qua bên kia bàn cờ.",
                "luật chơi": "Mỗi lượt bạn có thể di chuyển quân cờ 1 ô hoặc đặt 1 bức tường để chặn đối thủ.",
                "chiến thuật": "Cân bằng giữa tiến về phía trước và sử dụng tường để chặn đối thủ.",
                "tường": "Mỗi người có 10 bức tường, dùng để chặn đối thủ nhưng phải để lại đường đi."
            }
        }
    
    def train_with_conversation(self, user_input, ai_response, feedback=None):
        """Học từ cuộc hội thoại"""
        self.conversation_history.append({
            "input": user_input.lower(),
            "response": ai_response,
            "timestamp": datetime.now().isoformat(),
            "feedback": feedback
        })
        
        # Simple pattern learning
        words = user_input.lower().split()
        for word in words:
            if word not in self.knowledge_base.get("learned_words", {}):
                if "learned_words" not in self.knowledge_base:
                    self.knowledge_base["learned_words"] = {}
                self.knowledge_base["learned_words"][word] = ai_response
    
    def get_response(self, user_input):
        user_input = user_input.lower().strip()
        
        # Check exact matches first
        for question, answer in self.knowledge_base["common_questions"].items():
            if question in user_input:
                return answer
        
        # Keyword matching
        if any(word in user_input for word in ["quoridor", "game", "trò chơi"]):
            if any(word in user_input for word in ["luật", "rule", "chơi"]):
                return random.choice(self.knowledge_base["quoridor_rules"])
            elif any(word in user_input for word in ["chiến thuật", "strategy", "thắng"]):
                return random.choice(self.knowledge_base["strategies"])
        
        if any(word in user_input for word in ["tường", "wall", "chặn"]):
            return "Tường dùng để chặn đối thủ. Bạn có 10 bức tường và phải đảm bảo đối thủ vẫn có đường đi."
        
        if any(word in user_input for word in ["di chuyển", "move", "đi"]):
            return "Mỗi lượt bạn có thể di chuyển 1 ô theo 4 hướng: lên, xuống, trái, phải."
        
        # Check learned words
        learned_words = self.knowledge_base.get("learned_words", {})
        for word in user_input.split():
            if word in learned_words:
                return learned_words[word]
        
        # Default responses
        defaults = [
            "Tôi chưa hiểu câu hỏi này. Bạn có thể hỏi về luật chơi, chiến thuật, hoặc cách di chuyển không?",
            "Hãy hỏi tôi về Quoridor: luật chơi, chiến thuật, tường, hoặc cách di chuyển.",
            "Tôi đang học thêm! Bạn có thể hỏi về game Quoridor cụ thể hơn không?"
        ]
        return random.choice(defaults)

# Flask API Server
app = Flask(__name__)
ai_model = QuoridorAI()

@app.route('/chat', methods=['POST'])
def chat():
    data = request.get_json()
    user_message = data.get('message', '')
    
    if not user_message:
        return jsonify({'error': 'No message provided'}), 400
    
    response = ai_model.get_response(user_message)
    
    # Learn from conversation
    ai_model.train_with_conversation(user_message, response)
    
    return jsonify({
        'response': response,
        'confidence': 0.8,
        'source': 'local_python_model'
    })

@app.route('/train', methods=['POST'])
def train():
    data = request.get_json()
    user_input = data.get('input', '')
    ai_response = data.get('response', '')
    feedback = data.get('feedback', None)
    
    ai_model.train_with_conversation(user_input, ai_response, feedback)
    
    return jsonify({'status': 'trained', 'message': 'Model updated successfully'})

@app.route('/status', methods=['GET'])
def status():
    return jsonify({
        'status': 'running',
        'conversations': len(ai_model.conversation_history),
        'learned_words': len(ai_model.knowledge_base.get("learned_words", {})),
        'model': 'QuoridorAI v1.0'
    })

if __name__ == '__main__':
    print("🤖 Starting Quoridor AI Local Server...")
    print("📡 Server will run on: http://localhost:5000")
    print("🎯 Endpoints:")
    print("  POST /chat - Chat with AI")
    print("  POST /train - Train AI with new data")
    print("  GET /status - Check AI status")
    app.run(host='0.0.0.0', port=5000, debug=True)
```

### **Requirements.txt:**
```txt
flask==2.3.3
```

### **Cách chạy:**
```bash
# Cài đặt dependencies
pip install flask

# Chạy server
python simple_quoridor_ai.py
```

## 🔗 PHƯƠNG PHÁP 2: UNITY INTEGRATION

### **Update HybridAIManager để hỗ trợ Python API:**

```csharp
[Header("Python Local AI")]
public string pythonAIURL = "http://localhost:5000/chat";
public bool usePythonAI = true;
```

### **Thêm method mới:**

```csharp
UnityWebRequest CreatePythonAIRequest(string message)
{
    var requestData = new {
        message = message
    };
    
    string jsonData = JsonUtility.ToJson(requestData);
    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
    
    UnityWebRequest request = new UnityWebRequest(pythonAIURL, "POST");
    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    
    return request;
}
```

## 🎓 PHƯƠNG PHÁP 3: PURE C# MODEL (NO PYTHON)

### **Tạo C# AI Model hoàn toàn trong Unity:**

```csharp
// AdvancedLocalAI.cs
public class AdvancedLocalAI
{
    private Dictionary<string, List<string>> knowledgeBase;
    private List<ConversationData> learningHistory;
    
    public AdvancedLocalAI()
    {
        InitializeKnowledge();
        learningHistory = new List<ConversationData>();
    }
    
    void InitializeKnowledge()
    {
        knowledgeBase = new Dictionary<string, List<string>>
        {
            ["quoridor"] = new List<string> 
            {
                "Quoridor là game board strategy cho 2-4 người chơi",
                "Mục tiêu là đưa quân cờ qua bên kia bàn cờ"
            },
            ["luật"] = new List<string>
            {
                "Mỗi lượt có thể di chuyển hoặc đặt tường",
                "Có 10 bức tường cho mỗi người chơi",
                "Phải để lại đường đi cho đối thủ"
            }
            // ... thêm nhiều knowledge
        };
    }
    
    public string GetResponse(string input)
    {
        string response = ProcessInput(input);
        LearnFromConversation(input, response);
        return response;
    }
    
    string ProcessInput(string input)
    {
        input = input.ToLower();
        
        // Simple keyword matching với scoring
        var scores = new Dictionary<string, float>();
        
        foreach (var category in knowledgeBase.Keys)
        {
            if (input.Contains(category))
            {
                scores[category] = scores.GetValueOrDefault(category, 0) + 1f;
            }
        }
        
        if (scores.Count > 0)
        {
            var bestMatch = scores.OrderByDescending(x => x.Value).First();
            var responses = knowledgeBase[bestMatch.Key];
            return responses[Random.Range(0, responses.Count)];
        }
        
        return "Tôi chưa hiểu. Hãy hỏi về Quoridor, luật chơi, hoặc chiến thuật.";
    }
    
    void LearnFromConversation(string input, string response)
    {
        learningHistory.Add(new ConversationData
        {
            userInput = input,
            aiResponse = response,
            timestamp = DateTime.Now,
            confidence = CalculateConfidence(input)
        });
        
        // Adaptive learning - update knowledge base
        UpdateKnowledgeFromHistory();
    }
}
```

## ⚡ QUICK START GUIDE

### **Option A: Python Server (Khuyến nghị)**
1. Save Python code vào `simple_quoridor_ai.py`
2. `pip install flask`
3. `python simple_quoridor_ai.py`
4. Update Unity HybridAIManager với Python API URL

### **Option B: Pure C# Model**
1. Implement AdvancedLocalAI trong Unity
2. No external dependencies
3. Faster response, fully integrated

### **Option C: Hybrid Approach**
1. Use Python for training data generation
2. Export trained patterns to JSON
3. Load JSON trong Unity C# model

---

**🎯 ADVANTAGES:**
- ✅ **No external downloads** (Ollama, models)
- ✅ **Full control** over training data
- ✅ **Customizable** for your game
- ✅ **Fast responses** (local processing)
- ✅ **Learning capability** (improves over time)

**💡 Bạn muốn tôi implement phương pháp nào?**
