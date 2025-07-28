using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Test script cho Decision Tree và Entropy Analysis trong QuoridorAI
/// </summary>
public class AITestScript : MonoBehaviour
{
    [Header("Test Settings")]
    public bool runTests = false;
    public QuoridorAI aiToTest;
    
    void Start()
    {
        if (runTests && aiToTest != null)
        {
            StartCoroutine(RunAITests());
        }
    }
    
    System.Collections.IEnumerator RunAITests()
    {
        yield return new UnityEngine.WaitForSeconds(2f);
        
        Debug.Log("🧪 Starting AI Decision Tree & Entropy Tests...");
        
        // Test 1: Decision Tree Strategy
        TestDecisionTreeStrategies();
        
        yield return new UnityEngine.WaitForSeconds(1f);
        
        // Test 2: Entropy Analysis
        TestEntropyAnalysis();
        
        yield return new UnityEngine.WaitForSeconds(1f);
        
        // Test 3: Feature Extraction
        TestFeatureExtraction();
        
        Debug.Log("✅ AI Tests Completed!");
    }
    
    void TestDecisionTreeStrategies()
    {
        Debug.Log("🌳 Testing Decision Tree Strategies...");
        
        // Test Early Game
        var earlyGameFeatures = new DecisionFeatures
        {
            aiDistanceToGoal = 8,
            humanDistanceToGoal = 8,
            aiWallsLeft = 10,
            humanWallsLeft = 10,
            aiMobility = 3,
            humanMobility = 3,
            distanceAdvantage = 0,
            wallAdvantage = 0,
            isEarlyGame = true,
            isMidGame = false,
            isEndGame = false
        };
        
        var decisionTree = new DecisionTreeClassifier();
        decisionTree.Initialize();
        var strategy = decisionTree.Predict(earlyGameFeatures);
        Debug.Log($"Early Game Strategy: {strategy} (Expected: Balanced)");
        
        // Test AI Leading
        var leadingFeatures = new DecisionFeatures
        {
            aiDistanceToGoal = 3,
            humanDistanceToGoal = 6,
            aiWallsLeft = 5,
            humanWallsLeft = 5,
            aiMobility = 2,
            humanMobility = 2,
            distanceAdvantage = 3, // AI leading
            wallAdvantage = 0,
            isEarlyGame = false,
            isMidGame = true,
            isEndGame = false
        };
        
        strategy = decisionTree.Predict(leadingFeatures);
        Debug.Log($"AI Leading Strategy: {strategy} (Expected: Blocking)");
        
        // Test AI Behind
        var behindFeatures = new DecisionFeatures
        {
            aiDistanceToGoal = 6,
            humanDistanceToGoal = 4,
            aiWallsLeft = 3,
            humanWallsLeft = 3,
            aiMobility = 2,
            humanMobility = 3,
            distanceAdvantage = -2, // AI behind
            wallAdvantage = 0,
            isEarlyGame = false,
            isMidGame = true,
            isEndGame = false
        };
        
        strategy = decisionTree.Predict(behindFeatures);
        Debug.Log($"AI Behind Strategy: {strategy} (Expected: Aggressive)");
    }
    
    void TestEntropyAnalysis()
    {
        Debug.Log("🎲 Testing Entropy Analysis...");
        
        // Test với scores có entropy cao
        var highEntropyScores = new List<float> { 100f, 98f, 97f, 95f, 94f };
        float entropy = CalculateTestEntropy(highEntropyScores);
        Debug.Log($"High Entropy Scores: {entropy:F3} (Should be > 0.5)");
        
        // Test với scores có entropy thấp
        var lowEntropyScores = new List<float> { 100f, 100f, 100f, 99f, 99f };
        entropy = CalculateTestEntropy(lowEntropyScores);
        Debug.Log($"Low Entropy Scores: {entropy:F3} (Should be < 0.5)");
        
        // Test Softmax Distribution
        TestSoftmaxDistribution();
    }
    
    void TestSoftmaxDistribution()
    {
        Debug.Log("📊 Testing Softmax Distribution...");
        
        var testScores = new List<(int moveId, float score)>
        {
            (1, 100f), (2, 98f), (3, 95f), (4, 90f)
        };
        
        float temperature = 1.0f;
        float totalExp = testScores.Sum(ms => Mathf.Exp(ms.score / temperature));
        
        Debug.Log("Softmax Probabilities:");
        for (int i = 0; i < testScores.Count; i++)
        {
            float probability = Mathf.Exp(testScores[i].score / temperature) / totalExp;
            Debug.Log($"  Move {testScores[i].moveId}: {probability:F3} (Score: {testScores[i].score})");
        }
    }
    
    void TestFeatureExtraction()
    {
        Debug.Log("📋 Testing Feature Extraction...");
        
        // Simulate game state
        var testFeatures = new DecisionFeatures
        {
            aiDistanceToGoal = 5,
            humanDistanceToGoal = 6,
            aiWallsLeft = 7,
            humanWallsLeft = 4,
            aiMobility = 3,
            humanMobility = 2,
            distanceAdvantage = 1, // Human distance - AI distance
            wallAdvantage = 3,     // AI walls - Human walls
            isEarlyGame = false,
            isMidGame = true,
            isEndGame = false
        };
        
        Debug.Log($"Test Features:");
        Debug.Log($"  Distance Advantage: {testFeatures.distanceAdvantage} (positive = AI closer)");
        Debug.Log($"  Wall Advantage: {testFeatures.wallAdvantage} (positive = AI has more)");
        Debug.Log($"  Game Phase: {(testFeatures.isEarlyGame ? "Early" : testFeatures.isMidGame ? "Mid" : "End")}");
        Debug.Log($"  AI Mobility: {testFeatures.aiMobility}, Human Mobility: {testFeatures.humanMobility}");
    }
    
    float CalculateTestEntropy(List<float> values)
    {
        if (values == null || values.Count == 0) return 0f;
        
        // Normalize to probabilities
        float total = values.Sum();
        var probabilities = values.Select(v => v / total).ToList();
        
        // Calculate entropy
        float entropy = 0f;
        foreach (float p in probabilities)
        {
            if (p > 0f)
            {
                entropy -= p * Mathf.Log(p, 2f);
            }
        }
        
        return entropy;
    }
}
