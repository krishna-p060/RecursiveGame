using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Won }

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
        currentState = GameState.Won;

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        UIManager.Instance?.ShowVictoryScreen(gameTimer);
    }

    public void ReturnToMainMenu()
    {
        currentState = GameState.MainMenu;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(gameTimer / 60f);
        int seconds = Mathf.FloorToInt(gameTimer % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
