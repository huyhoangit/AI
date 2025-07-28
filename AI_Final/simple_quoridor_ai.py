"""
Simple Quoridor AI - Local Python Server
Không cần tải model lớn, chỉ cần Python + Flask
"""

import json
import re
from flask import Flask, request, jsonify
from datetime import datetime
import random
import os

class QuoridorAI:
    def __init__(self):
        self.knowledge_base = self.load_training_data()
        self.conversation_history = []
        self.learning_file = "ai_learning_data.json"
        self.load_learning_data()
        
    def load_training_data(self):
        """Load game knowledge base"""
        return {
            "quoridor_rules": [
                "Quoridor là game cờ chiến thuật cho 2-4 người chơi trên bàn cờ 9x9",
                "Mục tiêu là đưa quân cờ của bạn về đích ở phía đối diện",
                "Mỗi người chơi có 10 bức tường để chặn đối thủ",
                "Tường phải đảm bảo còn ít nhất 1 đường đi cho tất cả người chơi",
                "Mỗi lượt có thể di chuyển quân cờ hoặc đặt 1 bức tường"
            ],
            "strategies": [
                "Ưu tiên tiến thẳng về phía trước khi có thể",
                "Sử dụng tường để chặn khi đối thủ gần đích",
                "Không lãng phí tường quá sớm trong game",
                "Luôn giữ đường thoát cho chính mình",
                "Quan sát vị trí đối thủ để đặt tường hiệu quả",
                "Dùng tường tạo mê cung để làm chậm đối thủ"
            ],
            "movement_rules": [
                "Quân cờ có thể di chuyển 1 ô theo 4 hướng: lên, xuống, trái, phải",
                "Không thể đi qua tường hoặc ra ngoài bàn cờ",
                "Nếu có quân đối thủ ở ô kế bên, có thể nhảy qua",
                "Khi nhảy qua đối thủ bị chặn bởi tường, có thể đi chéo"
            ],
            "wall_rules": [
                "Tường có thể đặt ngang hoặc dọc giữa các ô",
                "Mỗi tường chiếm 2 khoảng trống liên tiếp",
                "Không thể đặt tường chồng lên tường khác",
                "Phải đảm bảo tất cả người chơi vẫn có đường đi về đích"
            ],
            "common_questions": {
                "quoridor là gì": "Quoridor là game board strategy nổi tiếng cho 2-4 người chơi. Bạn cần đưa quân cờ qua bên kia bàn cờ trong khi sử dụng tường để chặn đối thủ.",
                "luật chơi": "Mỗi lượt bạn có thể di chuyển quân cờ 1 ô hoặc đặt 1 bức tường. Mục tiêu là về đích trước đối thủ.",
                "chiến thuật": "Cân bằng giữa tiến về phía trước và sử dụng tường để chặn đối thủ. Đừng dùng hết tường quá sớm!",
                "tường": "Bạn có 10 bức tường để chặn đối thủ, nhưng phải đảm bảo họ vẫn có đường đi về đích.",
                "di chuyển": "Quân cờ di chuyển 1 ô mỗi lượt. Có thể nhảy qua đối thủ nếu không bị tường chặn.",
                "thắng": "Người đầu tiên đưa quân cờ về hàng đối diện sẽ thắng.",
                "thua": "Bạn thua nếu đối thủ về đích trước hoặc bạn không còn nước đi hợp lệ."
            }
        }
    
    def load_learning_data(self):
        """Load previously learned data"""
        if os.path.exists(self.learning_file):
            try:
                with open(self.learning_file, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    self.conversation_history = data.get('conversations', [])
                    learned = data.get('learned_patterns', {})
                    if learned:
                        self.knowledge_base['learned_patterns'] = learned
                print(f"📚 Loaded {len(self.conversation_history)} previous conversations")
            except Exception as e:
                print(f"⚠️ Could not load learning data: {e}")
    
    def save_learning_data(self):
        """Save learning data to file"""
        try:
            data = {
                'conversations': self.conversation_history[-100:],  # Keep last 100
                'learned_patterns': self.knowledge_base.get('learned_patterns', {})
            }
            with open(self.learning_file, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
        except Exception as e:
            print(f"⚠️ Could not save learning data: {e}")
    
    def train_with_conversation(self, user_input, ai_response, feedback=None):
        """Learn from conversation"""
        conversation = {
            "input": user_input.lower(),
            "response": ai_response,
            "timestamp": datetime.now().isoformat(),
            "feedback": feedback
        }
        
        self.conversation_history.append(conversation)
        
        # Simple pattern learning
        words = user_input.lower().split()
        keywords = [word for word in words if len(word) > 3]  # Ignore short words
        
        if keywords:
            if 'learned_patterns' not in self.knowledge_base:
                self.knowledge_base['learned_patterns'] = {}
                
            pattern_key = ' '.join(keywords[:3])  # Use first 3 keywords
            if pattern_key not in self.knowledge_base['learned_patterns']:
                self.knowledge_base['learned_patterns'][pattern_key] = []
            
            self.knowledge_base['learned_patterns'][pattern_key].append(ai_response)
        
        # Auto-save every 10 conversations
        if len(self.conversation_history) % 10 == 0:
            self.save_learning_data()
    
    def get_response(self, user_input):
        """Generate response based on input"""
        user_input_clean = user_input.lower().strip()
        confidence = 0.5
        
        # Check exact matches first
        for question, answer in self.knowledge_base["common_questions"].items():
            if question in user_input_clean:
                confidence = 0.9
                return answer, confidence
        
        # Check learned patterns
        learned_patterns = self.knowledge_base.get('learned_patterns', {})
        for pattern, responses in learned_patterns.items():
            if all(word in user_input_clean for word in pattern.split()):
                confidence = 0.8
                return random.choice(responses), confidence
        
        # Keyword-based responses
        response = self.get_keyword_response(user_input_clean)
        if response:
            confidence = 0.7
            return response, confidence
        
        # Default responses
        defaults = [
            "Tôi chưa hiểu câu hỏi này. Bạn có thể hỏi về luật chơi Quoridor, chiến thuật, hoặc cách di chuyển không?",
            "Hãy hỏi tôi về Quoridor: luật chơi, chiến thuật, tường, hoặc cách di chuyển nhé!",
            "Tôi đang học thêm! Bạn có thể hỏi về game Quoridor cụ thể hơn không?",
            "Quoridor có nhiều điều thú vị! Bạn muốn biết về luật chơi hay chiến thuật?"
        ]
        confidence = 0.3
        return random.choice(defaults), confidence
    
    def get_keyword_response(self, user_input):
        """Get response based on keywords"""
        # Game rules
        if any(word in user_input for word in ["quoridor", "game", "trò chơi", "board"]):
            if any(word in user_input for word in ["luật", "rule", "chơi", "cách"]):
                return random.choice(self.knowledge_base["quoridor_rules"])
            elif any(word in user_input for word in ["chiến thuật", "strategy", "thắng", "win"]):
                return random.choice(self.knowledge_base["strategies"])
        
        # Wall-related
        if any(word in user_input for word in ["tường", "wall", "chặn", "block"]):
            return random.choice(self.knowledge_base["wall_rules"])
        
        # Movement-related
        if any(word in user_input for word in ["di chuyển", "move", "đi", "nhảy", "jump"]):
            return random.choice(self.knowledge_base["movement_rules"])
        
        # Specific questions
        if any(word in user_input for word in ["thắng", "win", "chiến thắng"]):
            return "Để thắng Quoridor, bạn cần đưa quân cờ về hàng đối diện trước đối thủ. Cân bằng giữa tiến thẳng và dùng tường chặn!"
        
        if any(word in user_input for word in ["bắt đầu", "start", "khởi đầu"]):
            return "Để bắt đầu chơi Quoridor hiệu quả, hãy tiến thẳng về phía trước và quan sát nước đi của đối thủ. Đừng dùng tường quá sớm!"
        
        return None
    
    def get_stats(self):
        """Get AI statistics"""
        return {
            "total_conversations": len(self.conversation_history),
            "learned_patterns": len(self.knowledge_base.get('learned_patterns', {})),
            "knowledge_categories": len(self.knowledge_base),
            "last_conversation": self.conversation_history[-1]['timestamp'] if self.conversation_history else None
        }

# Flask API Server
app = Flask(__name__)
ai_model = QuoridorAI()

@app.route('/chat', methods=['POST'])
def chat():
    try:
        data = request.get_json()
        user_message = data.get('message', '')
        
        if not user_message:
            return jsonify({'error': 'No message provided'}), 400
        
        response, confidence = ai_model.get_response(user_message)
        
        # Learn from conversation
        ai_model.train_with_conversation(user_message, response)
        
        return jsonify({
            'response': response,
            'confidence': confidence,
            'source': 'local_python_ai',
            'timestamp': datetime.now().isoformat()
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/train', methods=['POST'])
def train():
    try:
        data = request.get_json()
        user_input = data.get('input', '')
        ai_response = data.get('response', '')
        feedback = data.get('feedback', None)
        
        if not user_input or not ai_response:
            return jsonify({'error': 'Input and response required'}), 400
        
        ai_model.train_with_conversation(user_input, ai_response, feedback)
        ai_model.save_learning_data()
        
        return jsonify({
            'status': 'success',
            'message': 'Model trained successfully',
            'conversations': len(ai_model.conversation_history)
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/status', methods=['GET'])
def status():
    try:
        stats = ai_model.get_stats()
        return jsonify({
            'status': 'running',
            'model': 'QuoridorAI v1.0',
            'uptime': datetime.now().isoformat(),
            'stats': stats
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/reset', methods=['POST'])
def reset():
    try:
        ai_model.conversation_history.clear()
        if 'learned_patterns' in ai_model.knowledge_base:
            ai_model.knowledge_base['learned_patterns'].clear()
        
        # Remove learning file
        if os.path.exists(ai_model.learning_file):
            os.remove(ai_model.learning_file)
        
        return jsonify({
            'status': 'success',
            'message': 'AI memory reset successfully'
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/export', methods=['GET'])
def export_data():
    try:
        export_data = {
            'knowledge_base': ai_model.knowledge_base,
            'conversation_history': ai_model.conversation_history[-50:],  # Last 50
            'export_time': datetime.now().isoformat()
        }
        return jsonify(export_data)
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    print("🤖 Starting Quoridor AI Local Server...")
    print("📡 Server will run on: http://localhost:5000")
    print("🎯 Available endpoints:")
    print("  POST /chat - Chat with AI")
    print("  POST /train - Train AI with new data") 
    print("  GET /status - Check AI status")
    print("  POST /reset - Reset AI memory")
    print("  GET /export - Export AI data")
    print("\n✨ No external models needed - Pure Python AI!")
    print("🧠 AI will learn and improve from conversations")
    
    app.run(host='localhost', port=5000, debug=False)
