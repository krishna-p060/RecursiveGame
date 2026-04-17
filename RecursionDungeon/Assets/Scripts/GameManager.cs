using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Won, Lost }

    [Header("State")]
    private GameState currentState = GameState.Playing;
    public GameState CurrentState => currentState;

    private float gameTimer = 0f;
    public float GameTimer => gameTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (currentState == GameState.Playing)
            gameTimer += Time.deltaTime;
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        gameTimer = 0f;
    }

    public void WinGame()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Won;

        BombTimer.Instance?.StopBomb();

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        UIManager.Instance?.ShowVictoryScreen(gameTimer);
    }

    public void LoseGame(string reason)
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Lost;

        BombTimer.Instance?.StopBomb();

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        UIManager.Instance?.ShowGameOverScreen(reason);
    }

    public void ReturnToMainMenu()
    {
        currentState = GameState.MainMenu;
        Time.timeScale = 1f;
        try
        {
            SceneManager.LoadScene("MainMenu");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not load MainMenu scene. Make sure it is added to Build Settings. Error: {e.Message}");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        try
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not reload scene. Make sure it is added to Build Settings. Error: {e.Message}");
        }
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(gameTimer / 60f);
        int seconds = Mathf.FloorToInt(gameTimer % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
