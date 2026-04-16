using UnityEngine;

/// <summary>
/// Attach this to each Room parent object to auto-configure its visual depth cues.
/// Makes deeper rooms smaller and darker, reinforcing the recursion metaphor.
/// </summary>
public class RoomSetup : MonoBehaviour
{
    [Header("Room Depth")]
    public int depth = 0;

    [Header("Visual Scaling")]
    public float baseScale = 1.0f;
    public float scaleReductionPerDepth = 0.08f;

    [Header("Darkness")]
    public float baseBrightness = 1.0f;
    public float darknessPerDepth = 0.15f;

    [Header("Floor Renderer")]
    public SpriteRenderer floorRenderer;

    void Awake()
    {
        float scale = baseScale - (depth * scaleReductionPerDepth);
        transform.localScale = Vector3.one * Mathf.Max(scale, 0.6f);

        if (floorRenderer != null)
        {
            float brightness = baseBrightness - (depth * darknessPerDepth);
            brightness = Mathf.Max(brightness, 0.25f);
            floorRenderer.color = new Color(brightness, brightness, brightness, 1f);
        }
    }
}
