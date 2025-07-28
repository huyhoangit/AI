using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class QLearningAgent
{
    private Dictionary<string, Dictionary<string, float>> QTable = new Dictionary<string, Dictionary<string, float>>();
    private float alpha = 0.1f; // learning rate
    private float gamma = 0.9f; // discount factor
    private float epsilon = 1.0f; // current exploration rate
    
    [Header("Epsilon Decay Settings")]
    public float initialEpsilon = 1.0f;      // Bắt đầu với 100% exploration
    public float minEpsilon = 0.01f;         // Minimum exploration rate (1%)
    public float epsilonDecayRate = 0.995f;  // Decay factor mỗi episode
    public int decaySteps = 1000;            // Số steps để decay
    
    // Epsilon decay tracking
    private int currentStep = 0;
    private int currentEpisode = 0;
    
    // Initialize epsilon decay system
    public void Initialize()
    {
        epsilon = initialEpsilon;
        currentStep = 0;
        currentEpisode = 0;
        
        Debug.Log($"🎯 QLearning initialized - Initial epsilon: {epsilon:F3}");
    }
    
    // Initialize mà không reset epsilon (để preserve trained state)
    public void InitializeWithoutEpsilonReset()
    {
        // Không reset epsilon, giữ nguyên trained value
        Debug.Log($"🎯 QLearning initialized preserving epsilon: {epsilon:F3}");
    }
    
    // Method để decay epsilon sau mỗi action/step
    public void DecayEpsilonStep()
    {
        currentStep++;
        
        // Linear decay theo steps
        float decayProgress = Mathf.Min(1.0f, (float)currentStep / decaySteps);
        epsilon = Mathf.Lerp(initialEpsilon, minEpsilon, decayProgress);
        
        // Đảm bảo không đi dưới minimum
        epsilon = Mathf.Max(epsilon, minEpsilon);
        
        if (currentStep % 100 == 0) // Log mỗi 100 steps
        {
            Debug.Log($"📉 Epsilon decay - Step: {currentStep}, Epsilon: {epsilon:F3}, Progress: {decayProgress:F2}");
        }
    }
    
    // Method để decay epsilon sau mỗi episode (game kết thúc)
    public void DecayEpsilonEpisode()
    {
        currentEpisode++;
        
        // Exponential decay theo episodes
        epsilon = Mathf.Max(minEpsilon, epsilon * epsilonDecayRate);
        
        Debug.Log($"🎮 Episode {currentEpisode} completed - New epsilon: {epsilon:F3}");
    }
    
    // Method để reset epsilon về initial value
    public void ResetEpsilon()
    {
        epsilon = initialEpsilon;
        currentStep = 0;
        currentEpisode = 0;
        
        Debug.Log($"🔄 Epsilon reset to initial value: {epsilon:F3}");
    }
    
    // Method để get current exploration info
    public EpsilonInfo GetEpsilonInfo()
    {
        return new EpsilonInfo
        {
            currentEpsilon = epsilon,
            currentStep = currentStep,
            currentEpisode = currentEpisode,
            decayProgress = Mathf.Min(1.0f, (float)currentStep / decaySteps),
            isExplorationPhase = epsilon > 0.1f,
            isExploitationPhase = epsilon <= 0.1f
        };
    }
    
    // Method để get Q-table size
    public int GetQTableSize()
    {
        return QTable?.Count ?? 0;
    }
    
    // Method để set epsilon (cho trained models)
    public void SetTrainedEpsilon(float trainedEpsilon, int trainedSteps = 0, int trainedEpisodes = 0)
    {
        epsilon = trainedEpsilon;
        currentStep = trainedSteps;
        currentEpisode = trainedEpisodes;
        
        Debug.Log($"🧠 Epsilon set to trained value: {epsilon:F3} (Steps: {currentStep}, Episodes: {currentEpisode})");
    }

    // Encode GameState thành string
    public string EncodeState(GameState state)
    {
        // Đơn giản hóa: chỉ dùng vị trí, số tường, các tường đã đặt
        string pos = $"{state.aiPosition.x},{state.aiPosition.y}-{state.humanPosition.x},{state.humanPosition.y}";
        string walls = string.Join("|", state.placedWalls.Select(w => $"{w.position.x},{w.position.z},{w.isHorizontal}"));
        return $"{pos}-{state.aiWallsLeft}-{state.humanWallsLeft}-{walls}";
    }

    // Encode AIMove thành string
    public string EncodeAction(AIMove move)
    {
        if (move.moveType == MoveType.Movement)
            return $"M:{move.targetPosition.x},{move.targetPosition.y}";
        else
            return $"W:{move.targetPosition.x},{move.targetPosition.y},{move.isHorizontalWall}";
    }

    // Chọn action theo epsilon-greedy với decay
    public AIMove ChooseAction(GameState state, List<AIMove> possibleMoves)
    {
        if (possibleMoves == null || possibleMoves.Count == 0) return null;
        
        // Decay epsilon mỗi khi chọn action
        DecayEpsilonStep();
        
        // Epsilon-greedy với current epsilon
        if (Random.value < epsilon)
        {
            // Exploration: chọn random
            var randomMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
            if (currentStep % 50 == 0) // Log occasional exploration
            {
                Debug.Log($"🎲 Exploration (ε={epsilon:F3}): Random action selected");
            }
            return randomMove;
        }
        
        // Exploitation: chọn best action từ Q-table
        string s = EncodeState(state);
        if (!QTable.ContainsKey(s)) 
        {
            return possibleMoves[Random.Range(0, possibleMoves.Count)];
        }
        
        var qActions = QTable[s];
        string bestAction = qActions.OrderByDescending(kv => kv.Value).First().Key;
        var bestMove = possibleMoves.FirstOrDefault(m => EncodeAction(m) == bestAction);
        
        if (currentStep % 50 == 0) // Log occasional exploitation
        {
            Debug.Log($"🧠 Exploitation (ε={epsilon:F3}): Best Q-value action selected");
        }
        
        return bestMove ?? possibleMoves[0];
    }

    // Cập nhật Q-table
    public void UpdateQ(GameState state, AIMove action, float reward, GameState nextState, List<AIMove> nextMoves)
    {
        string s = EncodeState(state);
        string a = EncodeAction(action);
        if (!QTable.ContainsKey(s)) QTable[s] = new Dictionary<string, float>();
        if (!QTable[s].ContainsKey(a)) QTable[s][a] = 0f;

        float maxQNext = 0f;
        if (nextMoves != null && nextMoves.Count > 0)
        {
            string sNext = EncodeState(nextState);
            if (QTable.ContainsKey(sNext))
                maxQNext = QTable[sNext].Values.DefaultIfEmpty(0f).Max();
        }

        QTable[s][a] = QTable[s][a] + alpha * (reward + gamma * maxQNext - QTable[s][a]);
    }

    // Enhanced Debug Q-table information với epsilon info
    public void DebugQTable()
    {
        Debug.Log("=== Q-TABLE DEBUG INFO ===");
        Debug.Log($"📊 Total states: {QTable.Count}");
        Debug.Log($"⚙️ Learning rate (alpha): {alpha}");
        Debug.Log($"⚙️ Discount factor (gamma): {gamma}");
        
        // Enhanced epsilon debug info
        Debug.Log("=== EPSILON DECAY INFO ===");
        Debug.Log($"🎯 Current epsilon: {epsilon:F3}");
        Debug.Log($"📈 Initial epsilon: {initialEpsilon:F3}");
        Debug.Log($"📉 Minimum epsilon: {minEpsilon:F3}");
        Debug.Log($"🔄 Decay rate: {epsilonDecayRate:F3}");
        Debug.Log($"👣 Current step: {currentStep}");
        Debug.Log($"🎮 Current episode: {currentEpisode}");
        Debug.Log($"📊 Decay progress: {(Mathf.Min(1.0f, (float)currentStep / decaySteps) * 100):F1}%");
        Debug.Log($"🧭 Phase: {(epsilon > 0.1f ? "EXPLORATION" : "EXPLOITATION")}");
        
        if (QTable.Count > 0)
        {
            int totalActions = QTable.Values.Sum(dict => dict.Count);
            Debug.Log($"📊 Total state-action pairs: {totalActions}");
            Debug.Log($"📊 Average actions per state: {(float)totalActions / QTable.Count:F2}");
            
            // Show top 5 states by number of actions
            var topStates = QTable.OrderByDescending(kvp => kvp.Value.Count).Take(5);
            Debug.Log("🏆 Top 5 states by action count:");
            foreach (var state in topStates)
            {
                string statePreview = state.Key.Length > 50 ? state.Key.Substring(0, 50) + "..." : state.Key;
                Debug.Log($"   📋 {statePreview} -> {state.Value.Count} actions");
            }
            
            // Show some Q-values
            var firstState = QTable.First();
            Debug.Log($"📈 Sample Q-values for state: {firstState.Key.Substring(0, Mathf.Min(50, firstState.Key.Length))}...");
            foreach (var action in firstState.Value.Take(3))
            {
                Debug.Log($"   🎯 Action: {action.Key} -> Q-value: {action.Value:F4}");
            }
        }
        else
        {
            Debug.Log("📊 Q-table is empty");
        }
    }
    
    // Test serialization/deserialization
    public void TestSerialization()
    {
        Debug.Log("🧪 Testing Q-table serialization...");
        
        // Create a small test Q-table
        var testQTable = new Dictionary<string, Dictionary<string, float>>();
        testQTable["test_state_1"] = new Dictionary<string, float>
        {
            {"M:1,2", 0.5f},
            {"W:2,3,true", -0.2f}
        };
        testQTable["test_state_2"] = new Dictionary<string, float>
        {
            {"M:3,4", 1.0f}
        };
        
        try
        {
            // Test serialization
            var serialization = new QTableSerialization(testQTable);
            string json = JsonUtility.ToJson(serialization, true);
            Debug.Log($"✅ Serialization successful. JSON preview: {json.Substring(0, Mathf.Min(200, json.Length))}...");
            
            // Test deserialization
            var deserialization = JsonUtility.FromJson<QTableSerialization>(json);
            var rebuiltTable = deserialization.ToDictionary();
            
            Debug.Log($"✅ Deserialization successful. States: {rebuiltTable.Count}");
            foreach (var kvp in rebuiltTable)
            {
                Debug.Log($"   📋 {kvp.Key} -> {kvp.Value.Count} actions");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Serialization test failed: {e.Message}");
        }
    }

    // Lưu Q-table ra file
    public void SaveQTable(string path)
    {
        try
        {
            Debug.Log($"💾 Attempting to save Q-table to: {path}");
            Debug.Log($"💾 Full path: {Path.GetFullPath(path)}");
            Debug.Log($"💾 Q-table states count: {QTable.Count}");
            
            // Tạo directory nếu chưa tồn tại
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Debug.Log($"📁 Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }
            
            // Serialize Q-table
            var serialization = new QTableSerialization(QTable);
            string jsonContent = JsonUtility.ToJson(serialization, true);
            
            Debug.Log($"📝 JSON content size: {jsonContent.Length} characters");
            Debug.Log($"📝 JSON preview: {jsonContent.Substring(0, Mathf.Min(200, jsonContent.Length))}...");
            
            // Ghi file
            File.WriteAllText(path, jsonContent);
            
            // Verify file đã được tạo
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                Debug.Log($"✅ Q-table saved successfully to {path}");
                Debug.Log($"✅ File size: {fileInfo.Length} bytes");
            }
            else
            {
                Debug.LogError($"❌ Failed to create file: {path}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error saving Q-table to {path}: {e.Message}");
            Debug.LogError($"❌ Stack trace: {e.StackTrace}");
        }
    }

    // Đọc Q-table từ file
    public void LoadQTable(string path)
    {
        Debug.Log($"🔍 Attempting to load Q-table from: {path}");
        Debug.Log($"🔍 Full path: {Path.GetFullPath(path)}");
        
        try
        {
            if (File.Exists(path))
            {
                Debug.Log($"✅ File exists: {path}");
                string jsonContent = File.ReadAllText(path);
                Debug.Log($"📄 File size: {jsonContent.Length} characters");
                
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    Debug.Log($"📝 JSON content preview: {jsonContent.Substring(0, Mathf.Min(100, jsonContent.Length))}...");
                    
                    try
                    {
                        var serialization = JsonUtility.FromJson<QTableSerialization>(jsonContent);
                        
                        if (serialization != null && serialization.stateKeys != null && serialization.stateValues != null)
                        {
                            Debug.Log($"🔧 Serialization success - States count: {serialization.stateKeys.Count}, Values count: {serialization.stateValues.Count}");
                            QTable = serialization.ToDictionary();
                            Debug.Log($"✅ Q-table loaded successfully from {path} - Total states: {QTable.Count}");
                            
                            // Tự động detect trained model và set epsilon
                            if (QTable.Count > 1000)
                            {
                                SetTrainedEpsilon(0.1f, 1000, 1000);
                                Debug.Log($"🎓 Auto-detected trained model - set exploitation mode (ε=0.1)");
                            }
                            
                            // Debug một vài entries đầu tiên
                            int debugCount = 0;
                            foreach (var kvp in QTable)
                            {
                                if (debugCount < 3)
                                {
                                    Debug.Log($"📊 State: {kvp.Key.Substring(0, Mathf.Min(50, kvp.Key.Length))}... -> {kvp.Value.Count} actions");
                                    debugCount++;
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ Failed to deserialize Q-table from {path} - null serialization");
                            Debug.LogWarning($"   Serialization: {(serialization != null ? "Not null" : "NULL")}");
                            Debug.LogWarning($"   StateKeys: {(serialization?.stateKeys != null ? "Not null" : "NULL")}");
                            Debug.LogWarning($"   StateValues: {(serialization?.stateValues != null ? "Not null" : "NULL")}");
                            QTable = new Dictionary<string, Dictionary<string, float>>();
                        }
                    }
                    catch (System.Exception jsonEx)
                    {
                        Debug.LogWarning($"⚠️ JSON parsing error in Q-table file {path}: {jsonEx.Message}");
                        Debug.LogWarning($"⚠️ JSON Stack trace: {jsonEx.StackTrace}");
                        Debug.LogWarning($"⚠️ Problematic JSON content: {jsonContent}");
                        QTable = new Dictionary<string, Dictionary<string, float>>();
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Q-table file {path} is empty");
                    QTable = new Dictionary<string, Dictionary<string, float>>();
                }
            }
            else
            {
                Debug.Log($"ℹ️ Q-table file {path} not found. Starting with empty Q-table.");
                Debug.Log($"🔍 Checking directory: {Path.GetDirectoryName(path)}");
                
                string directory = Path.GetDirectoryName(path);
                if (Directory.Exists(directory))
                {
                    Debug.Log($"📁 Directory exists. Files in directory:");
                    string[] files = Directory.GetFiles(directory, "*.json");
                    foreach (string file in files)
                    {
                        Debug.Log($"   📄 {Path.GetFileName(file)}");
                    }
                }
                else
                {
                    Debug.LogWarning($"📁 Directory does not exist: {directory}");
                }
                
                QTable = new Dictionary<string, Dictionary<string, float>>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error loading Q-table from {path}: {e.Message}");
            Debug.LogError($"❌ Stack trace: {e.StackTrace}");
            QTable = new Dictionary<string, Dictionary<string, float>>();
        }
    }
}

// Helper để serialize Dictionary
[System.Serializable]
public class Serialization<TKey, TValue>
{
    [SerializeField]
    public List<TKey> keys;
    [SerializeField]
    public List<TValue> values;

    public Serialization(Dictionary<TKey, TValue> dict)
    {
        keys = new List<TKey>(dict.Keys);
        values = new List<TValue>(dict.Values);
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < keys.Count && i < values.Count; i++)
            dict[keys[i]] = values[i];
        return dict;
    }
}

// Custom serialization cho Q-table nested dictionary
[System.Serializable]
public class QTableSerialization
{
    [SerializeField]
    public List<string> stateKeys;
    [SerializeField]
    public List<QActionSerialization> stateValues;

    public QTableSerialization(Dictionary<string, Dictionary<string, float>> qTable)
    {
        stateKeys = new List<string>();
        stateValues = new List<QActionSerialization>();
        
        foreach (var kvp in qTable)
        {
            stateKeys.Add(kvp.Key);
            stateValues.Add(new QActionSerialization(kvp.Value));
        }
    }

    public Dictionary<string, Dictionary<string, float>> ToDictionary()
    {
        var dict = new Dictionary<string, Dictionary<string, float>>();
        for (int i = 0; i < stateKeys.Count && i < stateValues.Count; i++)
        {
            dict[stateKeys[i]] = stateValues[i].ToDictionary();
        }
        return dict;
    }
}

[System.Serializable]
public class QActionSerialization
{
    [SerializeField]
    public List<string> actionKeys;
    [SerializeField]
    public List<float> actionValues;

    public QActionSerialization(Dictionary<string, float> actions)
    {
        actionKeys = new List<string>(actions.Keys);
        actionValues = new List<float>(actions.Values);
    }

    public Dictionary<string, float> ToDictionary()
    {
        var dict = new Dictionary<string, float>();
        for (int i = 0; i < actionKeys.Count && i < actionValues.Count; i++)
        {
            dict[actionKeys[i]] = actionValues[i];
        }
        return dict;
    }
}

// Helper class để return epsilon information
[System.Serializable]
public class EpsilonInfo
{
    public float currentEpsilon;
    public int currentStep;
    public int currentEpisode;
    public float decayProgress;
    public bool isExplorationPhase;
    public bool isExploitationPhase;
} 