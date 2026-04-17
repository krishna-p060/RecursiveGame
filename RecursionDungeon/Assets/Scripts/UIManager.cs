using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD - Top Left")]
    public Text depthText;
    public Text roomNameText;

    [Header("HUD - Top Right")]
    public Text weaponNameText;
    public Image weaponIconImage;

    [Header("Dialog")]
    public GameObject dialogPanel;
    public Text dialogText;
    private Coroutine dialogCoroutine;

    [Header("Victory Screen")]
    public GameObject victoryPanel;
    public Text victoryTimeText;
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Timer Display")]
    public Text timerText;

    [Header("Bomb Timer")]
    public GameObject bombPanel;
    public Text bombTimerText;
    public Text bombLabelText;
    public Image bombFuseBar;
    public Image bombPanelBackground;

    [Header("Game Over Screen")]
    public GameObject gameOverPanel;
    public Text gameOverReasonText;
    public Button gameOverRestartButton;
    public Button gameOverMenuButton;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (bombPanel != null) bombPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgain);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
        if (gameOverRestartButton != null)
            gameOverRestartButton.onClick.AddListener(OnPlayAgain);
        if (gameOverMenuButton != null)
            gameOverMenuButton.onClick.AddListener(OnMainMenu);

        UpdateWeaponDisplay("None", Color.gray);
    }

    void Update()
    {
        if (timerText != null && GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            timerText.text = GameManager.Instance.GetFormattedTime();
        }

        // Keyboard shortcuts on the Victory screen, in case mouse input doesn't work
        if (victoryPanel != null && victoryPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                OnPlayAgain();
            else if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
                OnMainMenu();
        }

        // Same shortcuts on the Game Over screen
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                OnPlayAgain();
            else if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
                OnMainMenu();
        }
    }

    public void UpdateRoomDisplay(int depth, string roomName)
    {
        if (depthText != null)
            depthText.text = $"Depth: {depth}";
        if (roomNameText != null)
            roomNameText.text = roomName;
    }

    public void UpdateWeaponDisplay(string weaponName, Color color)
    {
        if (weaponNameText != null)
            weaponNameText.text = $"Weapon: {weaponName}";
        if (weaponIconImage != null)
            weaponIconImage.color = color;
    }

    public void ShowDialog(string message)
    {
        if (dialogCoroutine != null)
            StopCoroutine(dialogCoroutine);
        dialogCoroutine = StartCoroutine(ShowDialogCoroutine(message));
    }

    IEnumerator ShowDialogCoroutine(string message)
    {
        if (dialogPanel != null) dialogPanel.SetActive(true);
        if (dialogText != null) dialogText.text = message;
        yield return new WaitForSeconds(2f);
        if (dialogPanel != null) dialogPanel.SetActive(false);
        dialogCoroutine = null;
    }

    public void ShowVictoryScreen(float time)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryTimeText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                victoryTimeText.text = $"TIME: {minutes:00}:{seconds:00}";
            }
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //                            BOMB TIMER UI
    // ═════════════════════════════════════════════════════════════════════════
    public void ShowBombTimer(float initialSeconds)
    {
        if (bombPanel == null) return;
        bombPanel.SetActive(true);
        UpdateBombTimer(initialSeconds, initialSeconds);
        ShowDialog("⚠ BOMB ARMED!  Escape in " + Mathf.CeilToInt(initialSeconds) + "s!");
    }

    public void UpdateBombTimer(float remaining, float total)
    {
        if (bombPanel == null || !bombPanel.activeSelf) return;

        float r = Mathf.Max(0f, remaining);
        if (bombTimerText != null)
            bombTimerText.text = r.ToString("0.00");

        if (bombFuseBar != null)
            bombFuseBar.fillAmount = total > 0f ? Mathf.Clamp01(r / total) : 0f;

        // Colour + pulse: yellow > 5s, orange 2-5s, red flashing < 2s.
        Color fuseColor;
        float pulse = 1f;
        if (r > 5f)
        {
            fuseColor = new Color(1f, 0.85f, 0.25f);
        }
        else if (r > 2f)
        {
            fuseColor = new Color(1f, 0.55f, 0.1f);
            pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.06f;
        }
        else
        {
            float flash = (Mathf.Sin(Time.time * 18f) + 1f) * 0.5f;
            fuseColor = Color.Lerp(new Color(1f, 0.2f, 0.2f), new Color(1f, 0.95f, 0.4f), flash);
            pulse = 1f + Mathf.Sin(Time.time * 14f) * 0.12f;
        }

        if (bombTimerText != null) bombTimerText.color = fuseColor;
        if (bombLabelText != null) bombLabelText.color = fuseColor;
        if (bombFuseBar != null) bombFuseBar.color = fuseColor;
        if (bombPanelBackground != null)
        {
            Color bg = bombPanelBackground.color;
            bombPanelBackground.color = new Color(
                Mathf.Lerp(bg.r, fuseColor.r * 0.3f, 0.4f),
                Mathf.Lerp(bg.g, fuseColor.g * 0.1f, 0.4f),
                Mathf.Lerp(bg.b, fuseColor.b * 0.1f, 0.4f),
                bg.a);
        }
        bombPanel.transform.localScale = new Vector3(pulse, pulse, 1f);
    }

    public void HideBombTimer()
    {
        if (bombPanel == null) return;
        bombPanel.SetActive(false);
        bombPanel.transform.localScale = Vector3.one;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //                          GAME OVER SCREEN
    // ═════════════════════════════════════════════════════════════════════════
    public void ShowGameOverScreen(string reason)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);
        if (gameOverReasonText != null)
            gameOverReasonText.text = reason;
    }

    void OnPlayAgain()
    {
        GameManager.Instance?.RestartGame();
    }

    void OnMainMenu()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }
}
