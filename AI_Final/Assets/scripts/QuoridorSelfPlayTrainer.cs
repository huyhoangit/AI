using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuoridorSelfPlayTrainer : MonoBehaviour
{
    [Header("Training Settings")]
    public bool autoStartTraining = true; // BẬT tự động training
    public int numEpisodes = 1000;
    public int maxMovesPerGame = 200;
    public string qTablePath = "Assets/qtable.json";
    
    [Header("Control")]
    public bool preserveExistingQTable = true; // Giữ Q-table hiện tại
    
    private QLearningAgent ai1;
    private QLearningAgent ai2;
    private bool isTraining = false;

    void Start()
    {
        if (autoStartTraining)
        {
            Debug.Log("🚀 Auto-starting self-play training...");
            StartCoroutine(SelfPlayLoop());
        }
        else
        {
            Debug.Log("⏸️ Self-play trainer ready but not auto-starting (autoStartTraining = false)");
        }
    }
    
    /// <summary>
    /// Manually start training (can be called from Inspector or code)
    /// </summary>
    [ContextMenu("Start Training")]
    public void StartTraining()
    {
        if (!isTraining)
        {
            Debug.Log("🎯 Starting manual self-play training...");
            StartCoroutine(SelfPlayLoop());
        }
        else
        {
            Debug.LogWarning("⚠️ Training already in progress!");
        }
    }
    
    /// <summary>
    /// Stop training
    /// </summary>
    [ContextMenu("Stop Training")]
    public void StopTraining()
    {
        if (isTraining)
        {
            isTraining = false;
            Debug.Log("🛑 Self-play training stopped");
        }
    }

    IEnumerator SelfPlayLoop()
    {
        isTraining = true;
        Debug.Log($"🤖 Starting self-play training for {numEpisodes} episodes...");
        
        ai1 = new QLearningAgent();
        ai2 = new QLearningAgent();
        
        // Load existing Q-table if preserveExistingQTable is true
        if (preserveExistingQTable)
        {
            ai1.LoadQTable(qTablePath);
            ai2.LoadQTable(qTablePath);
            
            // Check if Q-table has trained data
            if (ai1.GetQTableSize() > 1000)
            {
                Debug.Log($"📚 Loaded existing Q-table with {ai1.GetQTableSize()} states - continuing training");
                // Set trained epsilon for continued training
                ai1.SetTrainedEpsilon(0.1f, 1000, 1000);
                ai2.SetTrainedEpsilon(0.1f, 1000, 1000);
            }
            else
            {
                Debug.Log($"🆕 Q-table has limited data ({ai1.GetQTableSize()} states) - starting fresh training");
                ai1.Initialize();
                ai2.Initialize();
            }
        }
        else
        {
            Debug.Log("🆕 Starting fresh training (preserveExistingQTable = false)");
            ai1.Initialize();
            ai2.Initialize();
        }

        for (int episode = 1; episode <= numEpisodes && isTraining; episode++)
        {
            // Khởi tạo trạng thái game mới
            GameState state = new GameState();
            state.aiPosition = new Vector2Int(4, 8); // AI1 (Player 2)
            state.humanPosition = new Vector2Int(4, 0); // AI2 (Player 1)
            state.aiWallsLeft = 10;
            state.humanWallsLeft = 10;
            state.placedWalls = new List<WallInfo>();

            bool isAI1Turn = true;
            int moveCount = 0;
            bool gameEnded = false;
            float rewardAI1 = 0f, rewardAI2 = 0f;

            GameState lastStateAI1 = null, lastStateAI2 = null;
            AIMove lastActionAI1 = null, lastActionAI2 = null;

            while (!gameEnded && moveCount < maxMovesPerGame)
            {
                List<AIMove> possibleMoves = (isAI1Turn)
                    ? QuoridorAI.GeneratePossibleMovesStatic(state, true)
                    : QuoridorAI.GeneratePossibleMovesStatic(state, false);

                if (possibleMoves.Count == 0)
                {
                    // Không còn nước đi hợp lệ, hòa
                    rewardAI1 = 0f;
                    rewardAI2 = 0f;
                    break;
                }

                AIMove move = (isAI1Turn)
                    ? ai1.ChooseAction(state, possibleMoves)
                    : ai2.ChooseAction(state, possibleMoves);

                // Lưu lại state/action trước đó để cập nhật Q
                if (isAI1Turn)
                {
                    lastStateAI1 = state.Clone();
                    lastActionAI1 = move;
                }
                else
                {
                    lastStateAI2 = state.Clone();
                    lastActionAI2 = move;
                }

                // Thực hiện nước đi
                GameState nextState = QuoridorAI.ApplyMoveStatic(state, move, isAI1Turn);

                // Kiểm tra thắng/thua
                if (QuoridorAI.IsGameOverStatic(nextState))
                {
                    gameEnded = true;
                    if (isAI1Turn)
                    {
                        rewardAI1 = 1f;  // AI1 thắng
                        rewardAI2 = -1f; // AI2 thua
                    }
                    else
                    {
                        rewardAI1 = -1f; // AI1 thua
                        rewardAI2 = 1f;  // AI2 thắng
                    }
                }

                // Cập nhật Q cho người vừa đi
                if (isAI1Turn && lastStateAI1 != null && lastActionAI1 != null)
                {
                    List<AIMove> nextMoves = QuoridorAI.GeneratePossibleMovesStatic(nextState, false);
                    ai1.UpdateQ(lastStateAI1, lastActionAI1, rewardAI1, nextState, nextMoves);
                }
                else if (!isAI1Turn && lastStateAI2 != null && lastActionAI2 != null)
                {
                    List<AIMove> nextMoves = QuoridorAI.GeneratePossibleMovesStatic(nextState, true);
                    ai2.UpdateQ(lastStateAI2, lastActionAI2, rewardAI2, nextState, nextMoves);
                }

                // Chuyển lượt
                state = nextState;
                isAI1Turn = !isAI1Turn;
                moveCount++;
            }

            // Lưu Q-table sau mỗi ván
            ai1.SaveQTable(qTablePath);
            ai2.SaveQTable(qTablePath);

            Debug.Log($"[Self-Play] Episode {episode}/{numEpisodes} - Moves: {moveCount} - Reward AI1: {rewardAI1}, AI2: {rewardAI2}");

            // Đợi 1 frame để không khóa Unity
            yield return null;
        }

        isTraining = false;
        Debug.Log($"✅ Self-play training completed! Final Q-table size: {ai1.GetQTableSize()} states");
    }
}