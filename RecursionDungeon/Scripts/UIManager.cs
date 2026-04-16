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

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgain);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);

        UpdateWeaponDisplay("None", Color.gray);
    }

    void Update()
    {
        if (timerText != null && GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            timerText.text = GameManager.Instance.GetFormattedTime();
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
                victoryTimeText.text = $"You Escaped the Dungeon!\nTime: {minutes:00}:{seconds:00}";
            }
        }
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
