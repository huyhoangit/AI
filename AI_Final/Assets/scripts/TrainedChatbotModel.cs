using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Model chatbot tự train với khả năng học từ dữ liệu
/// Hỗ trợ pattern matching và keyword recognition
/// </summary>
[System.Serializable]
public class TrainedChatbotModel
{
    [Header("Training Data")]
    public List<ChatPattern> patterns = new List<ChatPattern>();
    public List<string> defaultResponses = new List<string>();
    
    [Header("Learning Settings")]
    public float confidenceThreshold = 0.6f;
    public int maxLearnedPatterns = 1000;
    
    private Dictionary<string, List<string>> keywordResponses = new Dictionary<string, List<string>>();
    private List<ChatHistory> conversationHistory = new List<ChatHistory>();
    
    public void Initialize()
    {
        LoadDefaultPatterns();
        BuildKeywordIndex();
        Debug.Log($"🤖 Chatbot initialized with {patterns.Count} patterns");
    }
    
    void LoadDefaultPatterns()
    {
        // Quoridor game patterns
        patterns.Add(new ChatPattern("quoridor", new string[] {"quoridor", "trò chơi", "game"}, 
            new string[] {"Quoridor là trò chơi chiến lược thú vị! Mục tiêu là đưa quân cờ về phía đối diện.", 
                         "Trong Quoridor, bạn có thể di chuyển quân hoặc đặt tường để cản đối thủ!"}));
        
        patterns.Add(new ChatPattern("rules", new string[] {"luật", "quy tắc", "cách chơi", "hướng dẫn"}, 
            new string[] {"Mỗi lượt, bạn có thể di chuyển quân 1 ô hoặc đặt tường. Mục tiêu là về đích trước đối thủ.", 
                         "Quân có thể di chuyển lên/xuống/trái/phải. Tường cản đường đi nhưng không được chặn hoàn toàn."}));
        
        patterns.Add(new ChatPattern("strategy", new string[] {"chiến thuật", "strategy", "bí kíp", "mẹo"}, 
            new string[] {"Cân bằng giữa tiến về đích và cản đối thủ. Đừng lãng phí tường!", 
                         "Quan sát nước đi của đối thủ và dự đoán ý định. Kiểm soát trung tâm bàn cờ!"}));
        
        patterns.Add(new ChatPattern("greeting", new string[] {"xin chào", "hello", "hi", "chào"}, 
            new string[] {"Xin chào! Tôi là AI hỗ trợ game Quoridor. Bạn cần giúp gì?", 
                         "Chào bạn! Hãy hỏi tôi về luật chơi hoặc chiến thuật Quoridor nhé!"}));
        
        patterns.Add(new ChatPattern("help", new string[] {"giúp", "help", "hỗ trợ", "trợ giúp"}, 
            new string[] {"Tôi có thể giúp bạn hiểu luật chơi, chiến thuật và phân tích nước đi trong Quoridor!", 
                         "Hãy hỏi tôi về: Luật chơi, Chiến thuật, Phân tích game, hoặc bất kỳ câu hỏi nào!"}));
        
        // Default responses
        defaultResponses.AddRange(new string[] {
            "Tôi chưa hiểu câu hỏi này. Bạn có thể hỏi về Quoridor không?",
            "Có thể bạn muốn hỏi về luật chơi hoặc chiến thuật Quoridor?",
            "Tôi đang học thêm. Hãy thử hỏi về game Quoridor nhé!"
        });
    }
    
    void BuildKeywordIndex()
    {
        keywordResponses.Clear();
        foreach (var pattern in patterns)
        {
            foreach (var keyword in pattern.keywords)
            {
                if (!keywordResponses.ContainsKey(keyword.ToLower()))
                    keywordResponses[keyword.ToLower()] = new List<string>();
                keywordResponses[keyword.ToLower()].AddRange(pattern.responses);
            }
        }
    }
    
    public string GetResponse(string input)
    {
        if (string.IsNullOrEmpty(input)) return GetRandomDefaultResponse();
        
        input = input.ToLower().Trim();
        
        // Record conversation
        conversationHistory.Add(new ChatHistory(input, System.DateTime.Now));
        
        // Try pattern matching first
        var bestMatch = FindBestPattern(input);
        if (bestMatch != null && bestMatch.confidence >= confidenceThreshold)
        {
            return GetRandomResponse(bestMatch.pattern.responses);
        }
        
        // Try keyword matching
        var keywordResponse = FindKeywordResponse(input);
        if (!string.IsNullOrEmpty(keywordResponse))
        {
            return keywordResponse;
        }
        
        // Learn new pattern if confident enough
        LearnFromInput(input);
        
        return GetRandomDefaultResponse();
    }
    
    PatternMatch FindBestPattern(string input)
    {
        PatternMatch bestMatch = null;
        float bestConfidence = 0f;
        
        foreach (var pattern in patterns)
        {
            float confidence = CalculateConfidence(input, pattern);
            if (confidence > bestConfidence)
            {
                bestConfidence = confidence;
                bestMatch = new PatternMatch(pattern, confidence);
            }
        }
        
        return bestMatch;
    }
    
    float CalculateConfidence(string input, ChatPattern pattern)
    {
        int matchCount = 0;
        var inputWords = input.Split(' ');
        
        foreach (var keyword in pattern.keywords)
        {
            if (input.Contains(keyword.ToLower()))
                matchCount++;
        }
        
        // Bonus for exact pattern name match
        if (input.Contains(pattern.patternName.ToLower()))
            matchCount += 2;
        
        return (float)matchCount / pattern.keywords.Length;
    }
    
    string FindKeywordResponse(string input)
    {
        var words = input.Split(' ');
        var responses = new List<string>();
        
        foreach (var word in words)
        {
            if (keywordResponses.ContainsKey(word))
                responses.AddRange(keywordResponses[word]);
        }
        
        return responses.Any() ? GetRandomResponse(responses.ToArray()) : null;
    }
    
    void LearnFromInput(string input)
    {
        if (patterns.Count >= maxLearnedPatterns) return;
        
        // Simple learning: if input contains game-related keywords, create a pattern
        var gameKeywords = new[] {"quoridor", "game", "tường", "quân", "cờ", "chơi"};
        if (gameKeywords.Any(k => input.Contains(k)))
        {
            var newPattern = new ChatPattern($"learned_{patterns.Count}", 
                new string[] {input}, 
                new string[] {"Tôi đã ghi nhận câu hỏi này. Cảm ơn bạn đã giúp tôi học!"});
            patterns.Add(newPattern);
            BuildKeywordIndex();
        }
    }
    
    string GetRandomResponse(string[] responses)
    {
        if (responses == null || responses.Length == 0) return GetRandomDefaultResponse();
        return responses[Random.Range(0, responses.Length)];
    }
    
    string GetRandomDefaultResponse()
    {
        return defaultResponses[Random.Range(0, defaultResponses.Count)];
    }
    
    public ChatAnalytics GetAnalytics()
    {
        return new ChatAnalytics
        {
            totalPatterns = patterns.Count,
            totalConversations = conversationHistory.Count,
            lastInteraction = conversationHistory.LastOrDefault()?.timestamp ?? System.DateTime.MinValue,
            popularKeywords = GetPopularKeywords()
        };
    }
    
    List<string> GetPopularKeywords()
    {
        return keywordResponses.Keys.Take(5).ToList();
    }
}

[System.Serializable]
public class ChatPattern
{
    public string patternName;
    public string[] keywords;
    public string[] responses;
    
    public ChatPattern(string name, string[] keys, string[] resp)
    {
        patternName = name;
        keywords = keys;
        responses = resp;
    }
}

[System.Serializable]
public class PatternMatch
{
    public ChatPattern pattern;
    public float confidence;
    
    public PatternMatch(ChatPattern p, float c)
    {
        pattern = p;
        confidence = c;
    }
}

[System.Serializable]
public class ChatHistory
{
    public string input;
    public System.DateTime timestamp;
    
    public ChatHistory(string inp, System.DateTime time)
    {
        input = inp;
        timestamp = time;
    }
}

[System.Serializable]
public class ChatAnalytics
{
    public int totalPatterns;
    public int totalConversations;
    public System.DateTime lastInteraction;
    public List<string> popularKeywords;
}
