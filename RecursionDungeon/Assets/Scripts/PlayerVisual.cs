using UnityEngine;

/// <summary>
/// Builds a detailed composite hero/knight visual (helmet, plume, armor, boots) on
/// the Player GameObject at runtime. The root SpriteRenderer (plain circle) is hidden.
/// </summary>
public class PlayerVisual : MonoBehaviour
{
    [Tooltip("Tint applied to the armor plate / main body color.")]
    public Color bodyColor = new Color(0.35f, 0.75f, 1f);

    private bool built = false;

    void Awake()
    {
        if (built) return;
        SpriteRenderer rootSR = GetComponent<SpriteRenderer>();
        if (rootSR != null)
        {
            // Use whatever color the editor assigned as the armor plate tint.
            bodyColor = rootSR.color;
            rootSR.enabled = false;
        }
        ArtBuilder.BuildHeroArt(transform, bodyColor);
        built = true;
    }
}
