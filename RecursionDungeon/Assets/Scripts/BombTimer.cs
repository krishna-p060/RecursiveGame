using UnityEngine;

/// <summary>
/// Bomb countdown that arms when the player picks up the Dagger (base case reached).
/// If the player fails to defeat every monster and escape the dungeon before the timer
/// hits zero, the bomb explodes and the game is lost.
/// </summary>
public class BombTimer : MonoBehaviour
{
    public static BombTimer Instance { get; private set; }

    [Tooltip("How many seconds the player has once the Dagger is grabbed.")]
    public float fuseSeconds = 20f;

    private float timeRemaining = 0f;
    private bool active = false;

    public bool IsActive => active;
    public float TimeRemaining => timeRemaining;
    public float FuseSeconds => fuseSeconds;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Arms the bomb — starts the countdown if it isn't already running.</summary>
    public void StartBomb(float? overrideSeconds = null)
    {
        if (active) return;
        timeRemaining = overrideSeconds ?? fuseSeconds;
        active = true;
        UIManager.Instance?.ShowBombTimer(timeRemaining);
    }

    /// <summary>Defuse the bomb (call this when the player wins or the game ends).</summary>
    public void StopBomb()
    {
        active = false;
        UIManager.Instance?.HideBombTimer();
    }

    void Update()
    {
        if (!active) return;
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        timeRemaining -= Time.deltaTime;
        UIManager.Instance?.UpdateBombTimer(timeRemaining, fuseSeconds);

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            active = false;
            Explode();
        }
    }

    void Explode()
    {
        UIManager.Instance?.UpdateBombTimer(0f, fuseSeconds);
        UIManager.Instance?.HideBombTimer();
        GameManager.Instance?.LoseGame("THE BOMB EXPLODED!");
    }
}
