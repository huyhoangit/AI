using UnityEngine;

public static class GridUtils
{
    public static Vector3 GridToWorld(Vector2Int gridPos, float cellSize, float gapSize, float boardSize, float cellHeight)
    {
        float totalCellSize = cellSize + gapSize;
        float offset = (boardSize - (8 * totalCellSize + cellSize)) / 2;
        float posX = (gridPos.x * totalCellSize) - (boardSize / 2) + offset + (cellSize / 2);
        float posZ = (gridPos.y * totalCellSize) - (boardSize / 2) + offset + (cellSize / 2);
        return new Vector3(posX, cellHeight / 2, posZ);
    }

    public static Vector2Int WorldToGrid(Vector3 worldPos, float cellSize, float gapSize, float boardSize)
    {
        float totalCellSize = cellSize + gapSize;
        float offset = (boardSize - (8 * totalCellSize + cellSize)) / 2;
        return new Vector2Int(
            Mathf.RoundToInt(((worldPos.x + boardSize / 2) - offset) / totalCellSize),
            Mathf.RoundToInt(((worldPos.z + boardSize / 2) - offset) / totalCellSize)
        );
    }

    public static bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < 9 && position.y >= 0 && position.y < 9;
    }
}