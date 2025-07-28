using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject squarePrefab;
    public GameObject wallPrefab;
    public int boardSize = 9;
    private GameObject[,] squares;
    private GameObject[,] walls;

    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        squares = new GameObject[boardSize, boardSize];
        walls = new GameObject[boardSize - 1, boardSize - 1];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                Vector3 position = new Vector3(x, 0, y);
                squares[x, y] = Instantiate(squarePrefab, position, Quaternion.identity);
                squares[x, y].name = $"Square_{x}_{y}";
            }
        }

        for (int x = 0; x < boardSize - 1; x++)
        {
            for (int y = 0; y < boardSize - 1; y++)
            {
                Vector3 wallPosition = new Vector3(x + 0.5f, 0, y + 0.5f);
                walls[x, y] = Instantiate(wallPrefab, wallPosition, Quaternion.identity);
                walls[x, y].name = $"Wall_{x}_{y}";
            }
        }
    }

    public void PlaceWall(int x, int y, bool isVertical)
    {
        if (isVertical && x < boardSize - 1)
        {
            Destroy(walls[x, y]);
            walls[x, y] = Instantiate(wallPrefab, new Vector3(x + 0.5f, 0, y + 0.5f), Quaternion.identity);
        }
        else if (!isVertical && y < boardSize - 1)
        {
            Destroy(walls[x, y]);
            walls[x, y] = Instantiate(wallPrefab, new Vector3(x + 0.5f, 0, y + 0.5f), Quaternion.Euler(0, 90, 0));
        }
    }

    public GameObject GetSquare(int x, int y)
    {
        return squares[x, y];
    }

    public GameObject GetWall(int x, int y)
    {
        return walls[x, y];
    }
}