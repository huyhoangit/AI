using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int playerId; // 1 or 2
    private Vector2Int gridPos;
    private float cellSize = 0.8f; // Size of the cell
    private float gapSize = 0.2f; // Gap between cells
    private float boardSize = 9f; // Size of the board
    private float cellHeight = 0.2f; // Height of the cell

    void Start()
    {
        gridPos = GameManager.Instance.GetPlayerPosition(playerId);
        transform.position = GridToWorld(gridPos);
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material playerMaterial = new Material(Shader.Find("Standard"));
            playerMaterial.color = (playerId == 1) ? Color.red : Color.white;
            renderer.material = playerMaterial;
        }
        else
        {
            Debug.LogWarning("No Renderer found on " + gameObject.name + ". Please add a Renderer component.");
        }
    }

    void Update()
    {
        if (GameManager.Instance.currentPlayer == playerId)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject == gameObject && !GameManager.Instance.isSelecting)
                    {
                        SelectPiece();
                    }
                    else if (GameManager.Instance.isSelecting)
                    {
                        Vector2Int targetPos = WorldToGrid(hit.point);
                        MoveOrCancel(targetPos);
                    }
                }
            }
        }
    }

    void SelectPiece()
    {
        GameManager.Instance.isSelecting = true;
        GameManager.Instance.selectedObject = gameObject;
        transform.position += Vector3.up * 0.5f; // Lift the piece
        ShowPossibleMoves();
    }

    void ShowPossibleMoves()
    {
        Vector2Int current = gridPos;
        int[] dx = { 0, 0, 1, -1, 0 }; // 5 squares: center, up, right, down, left
        int[] dy = { 0, 1, 0, -1, 0 };

        for (int i = 0; i < 5; i++)
        {
            int newX = current.x + dx[i];
            int newY = current.y + dy[i];
            if (newX >= 0 && newX < 9 && newY >= 0 && newY < 9)
            {
                GameObject square = GameObject.Find($"Square_{newX}_{newY}");
                if (square != null)
                {
                    Renderer squareRenderer = square.GetComponent<Renderer>();
                    if (squareRenderer != null)
                    {
                        Material highlightMaterial = new Material(Shader.Find("Standard"));
                        highlightMaterial.color = Color.green; // Change to green
                        squareRenderer.material = highlightMaterial;
                        if (i > 0 && IsPathBlocked(current, new Vector2Int(newX, newY)))
                        {
                            squareRenderer.material.color = Color.gray; // Blocked square in gray
                        }
                    }
                }
            }
        }
    }

    bool IsPathBlocked(Vector2Int from, Vector2Int to)
    {
        Vector3 start = GridToWorld(from);
        Vector3 end = GridToWorld(to);
        RaycastHit hit;
        if (Physics.Linecast(start, end, out hit))
            return hit.collider.CompareTag("Wall");
        return false;
    }

    void MoveOrCancel(Vector2Int targetPos)
    {
        if (targetPos == gridPos)
        {
            CancelSelection();
        }
        else
        {
            if (IsValidMove(targetPos))
            {
                Move(targetPos);
                GameManager.Instance.SwitchTurn();
            }
        }
    }

    bool IsValidMove(Vector2Int target)
    {
        Vector2Int current = gridPos;
        int dx = Mathf.Abs(target.x - current.x);
        int dy = Mathf.Abs(target.y - current.y);
        if (dx + dy != 1 || target.x < 0 || target.x >= 9 || target.y < 0 || target.y >= 9)
            return false;

        Vector3 start = GridToWorld(current);
        Vector3 end = GridToWorld(target);
        RaycastHit hit;
        if (Physics.Linecast(start, end, out hit))
            return !hit.collider.CompareTag("Wall");
        return true;
    }

    void Move(Vector2Int newPos)
    {
        gridPos = newPos;
        GameManager.Instance.UpdatePlayerPosition(playerId, newPos);
        transform.position = GridToWorld(newPos);
        ResetSquareColors();
    }

    void CancelSelection()
    {
        transform.position = GridToWorld(gridPos);
        ResetSquareColors();
        GameManager.Instance.isSelecting = false;
        GameManager.Instance.selectedObject = null;
    }

    void ResetSquareColors()
    {
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                GameObject square = GameObject.Find($"Square_{x}_{y}");
                if (square != null)
                {
                    Renderer squareRenderer = square.GetComponent<Renderer>();
                    if (squareRenderer != null)
                    {
                        Material originalMaterial = new Material(Shader.Find("Standard"));
                        originalMaterial.color = Color.black; // Reset to original color
                        squareRenderer.material = originalMaterial;
                    }
                }
            }
        }
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        float totalCellSize = cellSize + gapSize;
        float offset = (boardSize - (8 * totalCellSize + cellSize)) / 2;
        float posX = (gridPos.x * totalCellSize) - (boardSize / 2) + offset + (cellSize / 2);
        float posZ = (gridPos.y * totalCellSize) - (boardSize / 2) + offset + (cellSize / 2);
        return new Vector3(posX, cellHeight / 2, posZ);
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float totalCellSize = cellSize + gapSize;
        float offset = (boardSize - (8 * totalCellSize + cellSize)) / 2;
        return new Vector2Int(
            Mathf.RoundToInt(((worldPos.x + boardSize / 2) - offset) / totalCellSize),
            Mathf.RoundToInt(((worldPos.z + boardSize / 2) - offset) / totalCellSize)
        );
    }

    private void CheckWinCondition()
    {
        if (playerId == 1 && gridPos.y == 8)
        {
            Debug.Log("Player 1 Wins!");
        }
        else if (playerId == 2 && gridPos.y == 0)
        {
            Debug.Log("Player 2 Wins!");
        }
    }
}