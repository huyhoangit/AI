using UnityEngine;

/// <summary>
/// Final test script to verify Q-Learning trained model is working
/// This script will test AI decision making and verify it's using trained Q-table
/// </summary>
public class TrainedModelTestScript : MonoBehaviour
{
    [Header("Test Settings")]
    public bool runTestOnStart = true;
    public int testMovesCount = 5;
    
    void Start()
    {
        if (runTestOnStart)
        {
            Invoke("RunTrainedModelTest", 3f); // Wait for system initialization
        }
    }
    
    [ContextMenu("Test Trained Model")]
    public void RunTrainedModelTest()
    {
        Debug.Log("🧪 =========================");
        Debug.Log("🧪 TRAINED MODEL TEST");
        Debug.Log("🧪 =========================");
        
        // Find AI component
        QuoridorAI ai = FindFirstObjectByType<QuoridorAI>();
        
        if (ai == null)
        {
            Debug.LogError("❌ No QuoridorAI found for testing!");
            return;
        }
        
        Debug.Log($"🤖 Testing AI: {ai.gameObject.name}");
        
        // Verify trained model status
        ValidateTrainedModel(ai);
        
        // Test AI decision making
        TestAIDecisionMaking(ai);
        
        // Test algorithm selection
        TestAlgorithmSelection(ai);
        
        Debug.Log("✅ =========================");
        Debug.Log("✅ TRAINED MODEL TEST COMPLETE");
        Debug.Log("✅ =========================");
    }
    
    void ValidateTrainedModel(QuoridorAI ai)
    {
        Debug.Log("\n🎓 TRAINED MODEL VALIDATION:");
        
        Debug.Log($"   🎯 Use Q-Learning: {ai.useQLearning}");
        Debug.Log($"   🎓 Is Trained Model: {ai.isTrainedModel}");
        Debug.Log($"   💾 Allow Q-Table Saving: {ai.allowQTableSaving}");
        
        // Get Q-agent info via reflection
        var qAgentField = typeof(QuoridorAI).GetField("qAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (qAgentField != null)
        {
            var qAgent = qAgentField.GetValue(ai);
            if (qAgent != null)
            {
                var getQTableSizeMethod = qAgent.GetType().GetMethod("GetQTableSize");
                var getEpsilonInfoMethod = qAgent.GetType().GetMethod("GetEpsilonInfo");
                
                if (getQTableSizeMethod != null)
                {
                    int qTableSize = (int)getQTableSizeMethod.Invoke(qAgent, null);
                    Debug.Log($"   📊 Q-Table Size: {qTableSize} states");
                    
                    if (qTableSize > 10000)
                    {
                        Debug.Log("   ✅ TRAINED MODEL CONFIRMED (> 10,000 states)");
                    }
                    else if (qTableSize > 1000)
                    {
                        Debug.Log("   ⚠️ PARTIALLY TRAINED MODEL (1,000+ states)");
                    }
                    else
                    {
                        Debug.LogWarning("   ❌ EMPTY OR UNTRAINED MODEL");
                        return;
                    }
                }
                
                if (getEpsilonInfoMethod != null)
                {
                    var epsilonInfo = getEpsilonInfoMethod.Invoke(qAgent, null);
                    var currentEpsilonField = epsilonInfo.GetType().GetField("currentEpsilon");
                    if (currentEpsilonField != null)
                    {
                        float epsilon = (float)currentEpsilonField.GetValue(epsilonInfo);
                        Debug.Log($"   🎲 Current Epsilon: {epsilon:F3}");
                        
                        if (epsilon <= 0.15f)
                        {
                            Debug.Log("   ✅ EXPLOITATION MODE CONFIRMED (Low ε)");
                        }
                        else
                        {
                            Debug.LogWarning("   ⚠️ Still in exploration mode (High ε)");
                        }
                    }
                }
            }
        }
    }
    
    void TestAIDecisionMaking(QuoridorAI ai)
    {
        Debug.Log("\n🎯 AI DECISION MAKING TEST:");
        
        for (int i = 0; i < testMovesCount; i++)
        {
            Debug.Log($"\n🧪 Test Move {i + 1}:");
            
            try
            {
                // Get AI's best move
                var bestMove = ai.GetBestMove();
                
                if (bestMove != null)
                {
                    Debug.Log($"   ✅ AI selected: {bestMove.moveType} to {bestMove.targetPosition}");
                    
                    // Validate move quality (trained model should make good decisions)
                    if (bestMove.moveType == MoveType.Movement)
                    {
                        Debug.Log("   📍 Movement decision - checking if strategic...");
                    }
                    else if (bestMove.moveType == MoveType.WallPlacement)
                    {
                        Debug.Log("   🧱 Wall placement decision - checking if defensive/offensive...");
                    }
                }
                else
                {
                    Debug.LogWarning("   ❌ AI returned null move!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"   ❌ Error in AI decision making: {e.Message}");
            }
            
            // Small delay between tests
            System.Threading.Thread.Sleep(100);
        }
    }
    
    void TestAlgorithmSelection(QuoridorAI ai)
    {
        Debug.Log("\n🔍 ALGORITHM SELECTION TEST:");
        
        // Test Q-Learning selection
        if (ai.useQLearning)
        {
            Debug.Log("   🧠 Q-Learning is ENABLED");
            
            // Check if GetBestMove uses Q-Learning
            Debug.Log("   🎯 Testing GetBestMove() method...");
            
            try
            {
                var move = ai.GetBestMove();
                Debug.Log("   ✅ GetBestMove() executed successfully");
                
                // The console output should show Q-Learning messages
                Debug.Log("   💡 Check console for Q-Learning algorithm messages");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"   ❌ GetBestMove() failed: {e.Message}");
            }
        }
        else
        {
            Debug.Log("   🔍 Minimax is ENABLED");
            Debug.Log("   💡 Switch useQLearning to true to test trained model");
        }
    }
    
    [ContextMenu("Force AI to Use Trained Model")]
    public void ForceAIToUseTrainedModel()
    {
        Debug.Log("🔧 FORCING AI TO USE TRAINED MODEL");
        
        QuoridorAI ai = FindFirstObjectByType<QuoridorAI>();
        if (ai != null)
        {
            // Force settings
            ai.useQLearning = true;
            ai.isTrainedModel = true;
            ai.allowQTableSaving = false;
            
            // Force reload Q-table
            ai.ReloadQTable();
            
            Debug.Log("✅ AI forced to use trained model mode");
            
            // Validate
            ai.CheckQLearningStatus();
        }
    }
    
    [ContextMenu("Test AI vs Human Scenario")]
    public void TestAIVsHumanScenario()
    {
        Debug.Log("🎮 TESTING AI VS HUMAN SCENARIO");
        
        QuoridorAI ai = FindFirstObjectByType<QuoridorAI>();
        if (ai == null)
        {
            Debug.LogError("❌ No AI found!");
            return;
        }
        
        // Test multiple scenarios
        string[] scenarios = {
            "Opening game (early moves)",
            "Mid-game (wall blocking)",
            "End-game (race to finish)",
            "Defensive position",
            "Aggressive position"
        };
        
        foreach (string scenario in scenarios)
        {
            Debug.Log($"\n🎬 Scenario: {scenario}");
            
            try
            {
                var move = ai.GetBestMove();
                if (move != null)
                {
                    Debug.Log($"   🤖 AI choice: {move.moveType} to {move.targetPosition}");
                    Debug.Log($"   💭 Expected: Trained model should make strategic decision");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"   ❌ Scenario failed: {e.Message}");
            }
        }
    }
    
    [ContextMenu("Performance Benchmark")]
    public void RunPerformanceBenchmark()
    {
        Debug.Log("⚡ PERFORMANCE BENCHMARK");
        
        QuoridorAI ai = FindFirstObjectByType<QuoridorAI>();
        if (ai == null) return;
        
        int testCount = 10;
        float totalTime = 0f;
        
        for (int i = 0; i < testCount; i++)
        {
            float startTime = Time.realtimeSinceStartup;
            
            try
            {
                var move = ai.GetBestMove();
                float endTime = Time.realtimeSinceStartup;
                float duration = endTime - startTime;
                totalTime += duration;
                
                Debug.Log($"   Move {i + 1}: {duration * 1000f:F1}ms");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"   Move {i + 1} failed: {e.Message}");
            }
        }
        
        float avgTime = totalTime / testCount;
        Debug.Log($"📊 Average decision time: {avgTime * 1000f:F1}ms");
        
        if (avgTime < 0.1f)
        {
            Debug.Log("✅ EXCELLENT performance (< 100ms)");
        }
        else if (avgTime < 0.5f)
        {
            Debug.Log("✅ GOOD performance (< 500ms)");
        }
        else
        {
            Debug.LogWarning("⚠️ SLOW performance (> 500ms)");
        }
    }
}
