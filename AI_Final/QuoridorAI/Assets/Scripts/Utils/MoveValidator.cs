using UnityEngine;

public static class MoveValidator
{
    public static bool IsValidMove(Vector2Int currentPos, Vector2Int targetPos, bool[,] wallGrid)
    {
        int dx = Mathf.Abs(targetPos.x - currentPos.x);
        int dy = Mathf.Abs(targetPos.y - currentPos.y);

        // Check if the move is adjacent
        if (dx + dy != 1)
            return false;

        // Check for walls blocking the path
        if (dx == 1 && dy == 0) // Horizontal move
        {
            if (targetPos.x > currentPos.x) // Moving right
                return !wallGrid[currentPos.x, currentPos.y + 1]; // Check right wall
            else // Moving left
                return !wallGrid[currentPos.x - 1, currentPos.y + 1]; // Check left wall
        }
        else if (dx == 0 && dy == 1) // Vertical move
        {
            if (targetPos.y > currentPos.y) // Moving up
                return !wallGrid[currentPos.x + 1, currentPos.y]; // Check upper wall
            else // Moving down
                return !wallGrid[currentPos.x + 1, currentPos.y - 1]; // Check lower wall
        }

        return false;
    }
}