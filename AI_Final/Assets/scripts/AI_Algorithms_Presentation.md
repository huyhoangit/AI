# AI Algorithms in Quoridor Game

---

## 1. Tổng quan hệ thống AI
- Kết hợp nhiều thuật toán: Q-Learning, Minimax (Alpha-Beta), Decision Tree, A* Pathfinding, Hybrid AI (Local + API)
- Tối ưu cho cả gameplay và tương tác chat

---

## 2. Q-Learning (Học tăng cường)
- **Ý tưởng:** AI tự học chiến thuật qua trải nghiệm, cập nhật Q-Table dựa trên phần thưởng.
- **Điểm đặc biệt:**
  - Tự động train qua self-play
  - Có thể lưu, reload, đồng bộ Q-Table
  - Học được chiến thuật chặn đường, đặt tường tối ưu
- **Ứng dụng:**
  - AI ngày càng chơi tốt hơn sau mỗi ván

---

## 3. Minimax + Alpha-Beta Pruning
- **Ý tưởng:** Duyệt cây trò chơi, giả lập cả AI và người, chọn nước đi tối ưu cho AI, loại bỏ nhánh không cần thiết.
- **Điểm đặc biệt:**
  - Kết hợp với Decision Tree để lọc chiến lược
  - Đánh giá trạng thái dựa trên nhiều yếu tố: khoảng cách, số tường, khả năng di chuyển
  - Có thể chuyển đổi linh hoạt giữa Minimax và Q-Learning
- **Ứng dụng:**
  - AI phản ứng tốt với các tình huống đặc biệt, không chỉ dựa vào học máy

---

## 4. Decision Tree Classifier
- **Ý tưởng:** Phân tích trạng thái game, chọn chiến lược (tấn công, phòng thủ, chặn, cân bằng)
- **Điểm đặc biệt:**
  - Lọc nước đi trước khi vào Minimax
  - Dựa trên các đặc trưng: số tường, khoảng cách, giai đoạn game
- **Ứng dụng:**
  - AI linh hoạt, thay đổi chiến thuật theo tình huống

---

## 5. A* Pathfinding
- **Ý tưởng:** Tìm đường đi ngắn nhất từ vị trí hiện tại đến đích, tránh tường
- **Điểm đặc biệt:**
  - Đảm bảo AI luôn có đường đi hợp lệ
  - Tối ưu hóa tốc độ tìm đường
- **Ứng dụng:**
  - Cả AI và người chơi đều không bị chặn hoàn toàn

---

## 6. Hybrid AI (Local Model + API)
- **Ý tưởng:** Kết hợp model local (train sẵn) và API (Gemini, Ollama, HuggingFace...) để trả lời chat
- **Điểm đặc biệt:**
  - Ưu tiên local cho câu hỏi về game, fallback API cho câu hỏi mở rộng
  - Có thể tùy chỉnh personality, gợi ý, châm biếm
- **Ứng dụng:**
  - AI trả lời tự nhiên, đa dạng, không bị giới hạn bởi local model

---

## 7. Tích hợp AI với Gameplay & Chat
- **AI vừa chơi game, vừa tương tác chat/voice**
- **Bình luận, gợi ý chiến thuật, cảnh báo, trả lời chat**
- **Tích hợp TTS để AI "nói" ra phản hồi**

---

## 8. Tổng kết
- **Hệ thống AI đa dạng, linh hoạt, mạnh mẽ**
- **Kết hợp học máy, heuristic, và chiến lược truyền thống**
- **Tối ưu cho cả gameplay và trải nghiệm người dùng** 