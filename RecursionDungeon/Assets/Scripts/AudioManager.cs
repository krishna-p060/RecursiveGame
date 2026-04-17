using UnityEngine;

/// <summary>
/// Loads sound effects from Assets/Resources/Audio/SFX/ and plays them on demand.
/// Missing files are silently ignored so the game still runs if you haven't added
/// them yet.
///
/// To add a new SFX:
///   1. Drop the audio file into: Assets/Resources/Audio/SFX/&lt;name&gt;.wav
///   2. Call AudioManager.Instance.Play("&lt;name&gt;") from anywhere.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixing")]
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private AudioSource source;

    // Well-known SFX names — keep these in sync with filenames in Resources/Audio/SFX/.
    public const string SFX_WEAPON_PICKUP = "weapon_pickup";
    public const string SFX_MONSTER_DEATH = "monster_death";
    public const string SFX_BOMB_ARMED    = "bomb_armed";
    public const string SFX_BOMB_TICK     = "bomb_tick";
    public const string SFX_BOMB_EXPLODE  = "bomb_explode";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D
    }

    /// <summary>Plays a one-shot SFX by filename (without extension).</summary>
    public void Play(string clipName, float volumeScale = 1f)
    {
        if (string.IsNullOrEmpty(clipName) || source == null) return;
        AudioClip clip = Resources.Load<AudioClip>("Audio/SFX/" + clipName);
        if (clip == null) return; // SFX file not present — fine, just stay silent.
        source.PlayOneShot(clip, sfxVolume * volumeScale);
    }
}
