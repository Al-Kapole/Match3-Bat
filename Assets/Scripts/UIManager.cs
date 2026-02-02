using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;

    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text timerText;

    [Header("Game Over")]
    public TMP_Text finalScoreText;

    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowHUD()
    {
        menuPanel.SetActive(false);
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(int score)
    {
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score;
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, time));
            timerText.text = seconds.ToString();
        }
    }

    public void OnGridSize(int size)
    {
        GameManager.Instance.StartGame(size);
    }
    
    
    public void OnPlayAgain()
    {
        GameManager.Instance.StartGame(GeneralGameInfo.Instance.SelectedGridSize);
    }

    public void OnMenuButton()
    {
        GameManager.Instance.ShowMenu();
    }
}
