using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject gameUI;
    public GameObject gameOverPanel;
    public Text winnerText;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        gameUI.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void StartGame()
    {
        mainMenu.SetActive(false);
        gameUI.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(string winner)
    {
        gameUI.SetActive(false);
        gameOverPanel.SetActive(true);
        winnerText.text = winner + " Wins!";
    }

    public void RestartGame()
    {
        // Logic to restart the game
        ShowMainMenu();
    }
}