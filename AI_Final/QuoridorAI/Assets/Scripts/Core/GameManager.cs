using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int currentPlayer; // 1 or 2
    public GameState gameState;
    public GameObject selectedObject;
    public bool isSelecting;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        gameState = GameState.WaitingForInput;
        currentPlayer = 1; // Start with player 1
    }

    public void SwitchTurn()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        CheckWinCondition();
    }

    public Vector2Int GetPlayerPosition(int playerId)
    {
        return playerId == 1 ? new Vector2Int(4, 0) : new Vector2Int(4, 8);
    }

    public void UpdatePlayerPosition(int playerId, Vector2Int newPos)
    {
        // Logic to update player position in the game
    }

    private void CheckWinCondition()
    {
        // Logic to check if a player has won
    }
}