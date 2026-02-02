using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Menu, Playing, GameOver }
    public GameState State { get; private set; }
    [SerializeField] private int gameDuration = 60;

    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private UIManager uiManager;

    public int Score { get; private set; }
    public int ChainMultiplier;
    public int TimeRemaining { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowMenu();
    }

    public void ShowMenu()
    {
        State = GameState.Menu;
        board.ClearBoard();
        if (uiManager != null)
            uiManager.ShowMenu();
    }

    public void StartGame(int gridSize)
    {
        GeneralGameInfo.Instance.SelectedGridSize = gridSize;
        Score = 0;
        ChainMultiplier = 1;
        TimeRemaining = gameDuration;
        State = GameState.Playing;

        if (uiManager != null)
        {
            uiManager.ShowHUD();
            uiManager.UpdateScore(Score);
            uiManager.UpdateTimer(TimeRemaining);
        }

        board.InitializeBoard(gridSize);
        StartCoroutine(TimeRunning());
    }
    private IEnumerator TimeRunning()
    {
        while (TimeRemaining > 0)
        {
            yield return new WaitForSeconds(1);
            TimeRemaining--;
            if (uiManager != null)
                uiManager.UpdateTimer(TimeRemaining);
        }
        EndGame();
    }

    public void AddScore(int tilesCleared)
    {
        int points = tilesCleared * ChainMultiplier;
        Score += points;
        if (uiManager != null)
            uiManager.UpdateScore(Score);
    }

    public void EndGame()
    {
        State = GameState.GameOver;
        GeneralGameInfo.Instance.FinalScore = Score;
        if (uiManager != null)
            uiManager.ShowGameOver(Score);
    }
}
