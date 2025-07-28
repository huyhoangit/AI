using UnityEngine;

public class AIController : MonoBehaviour
{
    public int playerId; // ID of the AI player
    private BoardManager boardManager;

    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
    }

    void Update()
    {
        if (GameManager.Instance.currentPlayer == playerId)
        {
            MakeMove();
        }
    }

    void MakeMove()
    {
        // Implement the logic to choose the best move using Minimax or A* algorithm
        Vector2Int bestMove = MinimaxAI.GetBestMove(boardManager, playerId);
        if (bestMove != Vector2Int.zero)
        {
            boardManager.MovePlayer(playerId, bestMove);
        }
    }
}