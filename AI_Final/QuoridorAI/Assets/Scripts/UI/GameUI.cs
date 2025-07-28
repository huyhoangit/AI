using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text player1ScoreText;
    public Text player2ScoreText;
    public GameObject gameOverPanel;
    public Button restartButton;

    private void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
        gameOverPanel.SetActive(false);
    }

    public void UpdateScore(int player1Score, int player2Score)
    {
        player1ScoreText.text = "Player 1 Score: " + player1Score;
        player2ScoreText.text = "Player 2 Score: " + player2Score;
    }

    public void ShowGameOver(string winner)
    {
        gameOverPanel.SetActive(true);
        gameOverPanel.GetComponentInChildren<Text>().text = winner + " Wins!";
    }

    private void RestartGame()
    {
        // Logic to restart the game
        GameManager.Instance.RestartGame();
        gameOverPanel.SetActive(false);
    }
}