using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class SimpleChatbotModel
{
    [System.Serializable]
    public class QA
    {
        public string question;
        public string answer;
        public List<string> keywords; // Thêm keywords để matching linh hoạt hơn
    }

    public List<QA> faqList = new List<QA>();

    // Khởi tạo với một số câu hỏi/trả lời mẫu
    public SimpleChatbotModel()
    {
        // Luật chơi
        faqList.Add(new QA { 
            question = "luật chơi quoridor", 
            answer = "Mỗi người chơi phải đưa quân của mình sang phía đối diện bàn cờ, có thể đặt tường để cản đối thủ.",
            keywords = new List<string> { "luật", "chơi", "quoridor", "hướng", "dẫn", "cách", "chơi", "luật chơi", "hướng dẫn", "cách chơi", "chơi sao", "bắt đầu", "game", "chơi như thế nào" }
        });
        // Hướng dẫn (tổng quát)
        faqList.Add(new QA {
            question = "hướng dẫn chơi quoridor",
            answer = "Để chơi Quoridor, bạn cần đưa quân của mình sang phía đối diện bàn cờ, có thể đặt tường để cản đối thủ. Mỗi lượt bạn có thể di chuyển quân hoặc đặt tường. Không được chặn hoàn toàn đường đi của đối thủ.",
            keywords = new List<string> { "hướng dẫn", "cách chơi", "bắt đầu", "chơi như thế nào", "hướng dẫn chơi", "hướng dẫn quoridor", "cách chơi quoridor" }
        });
        
        // Di chuyển quân
        faqList.Add(new QA { 
            question = "cách di chuyển quân", 
            answer = "Quân di chuyển một ô theo chiều ngang hoặc dọc, không được đi xuyên qua tường.",
            keywords = new List<string> { "di", "chuyển", "quân", "đi", "di chuyển", "cách di chuyển", "di chuyển quân" }
        });
        
        // Đặt tường
        faqList.Add(new QA { 
            question = "cách đặt tường", 
            answer = "Mỗi lượt bạn có thể đặt một tường để cản đường đối thủ, nhưng không được chặn hoàn toàn đường đi.",
            keywords = new List<string> { "đặt", "tường", "wall", "cách đặt", "đặt tường", "cách đặt tường" }
        });
        
        // Số lượng tường
        faqList.Add(new QA { 
            question = "bao nhiêu tường", 
            answer = "Mỗi người chơi có 10 tường để sử dụng trong suốt ván đấu.",
            keywords = new List<string> { "bao", "nhiêu", "tường", "số", "lượng", "mấy", "cs", "bn", "mấy tường", "mấy cái tường", "mấy bức tường", "bao nhiêu", "số lượng tường" }
        });
        
        // Cách thắng
        faqList.Add(new QA { 
            question = "làm sao để thắng", 
            answer = "Đưa quân của bạn sang hàng đối diện trước đối thủ để giành chiến thắng.",
            keywords = new List<string> { "làm", "sao", "để", "thắng", "chiến", "thắng", "làm sao", "cách thắng", "làm sao để thắng" }
        });
        
        // Thêm các câu hỏi/trả lời khác tuỳ ý
    }

    // Normalize input mạnh mẽ
    private string NormalizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        
        // Từ điển viết tắt/thông dụng
        Dictionary<string, string> slangDict = new Dictionary<string, string> {
            { "cs", "có" },
            { "bn", "bao nhiêu" },
            { "mấy", "bao nhiêu" },
            { "tg", "tường" },
            { "tường", "tường" }, // để chắc chắn
            // Thêm các từ khác nếu cần
        };
        foreach (var kv in slangDict)
        {
            input = Regex.Replace(input, $@"\\b{kv.Key}\\b", kv.Value);
        }
        
        // Chuyển về Unicode chuẩn
        input = input.Normalize(NormalizationForm.FormC);
        
        // Loại bỏ dấu câu và ký tự đặc biệt, chỉ giữ lại chữ cái, số và khoảng trắng
        input = Regex.Replace(input, @"[^\w\s]", " ");
        
        // Loại bỏ khoảng trắng thừa và trim
        input = Regex.Replace(input, @"\s+", " ").Trim();
        
        // Chuyển về chữ thường
        input = input.ToLowerInvariant();
        
        // Loại bỏ các từ không quan trọng (stop words)
        string[] stopWords = { "là", "của", "và", "hoặc", "nhưng", "mà", "thì", "là", "có", "được", "cho", "với", "từ", "đến", "trong", "ngoài", "trên", "dưới", "trước", "sau", "bên", "cạnh", "giữa", "xung", "quanh" };
        foreach (string stopWord in stopWords)
        {
            input = Regex.Replace(input, $@"\b{stopWord}\b", " ");
        }
        
        // Loại bỏ khoảng trắng thừa lần nữa
        input = Regex.Replace(input, @"\s+", " ").Trim();
        
        return input;
    }

    // Tách từ và normalize
    private List<string> TokenizeAndNormalize(string input)
    {
        string normalized = NormalizeInput(input);
        if (string.IsNullOrEmpty(normalized)) return new List<string>();
        
        return normalized.Split(' ')
                       .Where(word => !string.IsNullOrEmpty(word) && word.Length > 1)
                       .ToList();
    }

    // Trả về câu trả lời gần đúng nhất, hoặc null nếu không tìm được
    public string GetAnswer(string userInput)
    {
        if (string.IsNullOrEmpty(userInput)) return null;
        
        var userTokens = TokenizeAndNormalize(userInput);
        if (userTokens.Count == 0) return null;
        
        float bestScore = 0.0f;
        string bestAnswer = null;
        
        foreach (var qa in faqList)
        {
            // Tính điểm dựa trên keywords
            float keywordScore = CalculateKeywordScore(userTokens, qa.keywords);
            
            // Tính điểm dựa trên câu hỏi gốc
            var questionTokens = TokenizeAndNormalize(qa.question);
            float questionScore = CalculateSimilarity(userTokens, questionTokens);
            
            // Lấy điểm cao nhất
            float totalScore = Mathf.Max(keywordScore, questionScore);
            
            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestAnswer = qa.answer;
            }
        }
        
        // Ngưỡng: chỉ trả lời nếu giống nhau > 0.3 (30%) - giảm ngưỡng để dễ match hơn
        if (bestScore > 0.3f)
            return bestAnswer;
            
        return null;
    }

    // Tính điểm dựa trên keywords
    private float CalculateKeywordScore(List<string> userTokens, List<string> keywords)
    {
        if (keywords == null || keywords.Count == 0) return 0f;
        
        int matches = 0;
        foreach (string userToken in userTokens)
        {
            foreach (string keyword in keywords)
            {
                if (userToken.Contains(keyword) || keyword.Contains(userToken))
                {
                    matches++;
                    break; // Mỗi userToken chỉ match với 1 keyword
                }
            }
        }
        
        if (userTokens.Count == 0) return 0f;
        return (float)matches / userTokens.Count;
    }

    // Đo độ tương đồng giữa 2 danh sách từ (Jaccard similarity)
    private float CalculateSimilarity(List<string> tokensA, List<string> tokensB)
    {
        if (tokensA.Count == 0 || tokensB.Count == 0) return 0f;
        
        var setA = new HashSet<string>(tokensA);
        var setB = new HashSet<string>(tokensB);
        
        int intersect = setA.Intersect(setB).Count();
        int union = setA.Union(setB).Count();
        
        if (union == 0) return 0f;
        return (float)intersect / union;
    }
} 