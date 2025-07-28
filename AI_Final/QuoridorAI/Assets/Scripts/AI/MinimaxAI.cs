using UnityEngine;
using System.Collections.Generic;

public class MinimaxAI : MonoBehaviour
{
    public int playerId; // ID of the AI player
    public int maxDepth = 3; // Maximum depth for the Minimax algorithm

    public Vector2Int GetBestMove(BoardManager boardManager)
    {
        List<Vector2Int> possibleMoves = boardManager.GetPossibleMoves(playerId);
        Vector2Int bestMove = possibleMoves[0];
        float bestValue = float.NegativeInfinity;

        foreach (Vector2Int move in possibleMoves)
        {
            boardManager.MakeMove(move, playerId);
            float moveValue = Minimax(boardManager, maxDepth, false);
            boardManager.UndoMove(move);

            if (moveValue > bestValue)
            {
                bestValue = moveValue;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private float Minimax(BoardManager boardManager, int depth, bool isMaximizing)
    {
        if (depth == 0 || boardManager.IsGameOver())
        {
            return EvaluateBoard(boardManager);
        }

        if (isMaximizing)
        {
            float maxEval = float.NegativeInfinity;
            List<Vector2Int> possibleMoves = boardManager.GetPossibleMoves(playerId);

            foreach (Vector2Int move in possibleMoves)
            {
                boardManager.MakeMove(move, playerId);
                float eval = Minimax(boardManager, depth - 1, false);
                boardManager.UndoMove(move);
                maxEval = Mathf.Max(maxEval, eval);
            }
            return maxEval;
        }
        else
        {
            float minEval = float.PositiveInfinity;
            List<Vector2Int> possibleMoves = boardManager.GetPossibleMoves(3 - playerId); // Assuming two players with IDs 1 and 2

            foreach (Vector2Int move in possibleMoves)
            {
                boardManager.MakeMove(move, 3 - playerId);
                float eval = Minimax(boardManager, depth - 1, true);
                boardManager.UndoMove(move);
                minEval = Mathf.Min(minEval, eval);
            }
            return minEval;
        }
    }

    private float EvaluateBoard(BoardManager boardManager)
    {
        // Implement your board evaluation logic here
        // For example, you can return a score based on the distance to the goal or the number of walls available
        return 0f; // Placeholder
    }
}