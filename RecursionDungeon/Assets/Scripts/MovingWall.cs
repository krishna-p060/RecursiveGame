using UnityEngine;

/// <summary>
/// Makes a maze wall oscillate back-and-forth, creating timed openings
/// the player can slip through. Attach to any maze-wall GameObject that
/// already has a SpriteRenderer + BoxCollider2D.
/// </summary>
public class MovingWall : MonoBehaviour
{
    public enum MoveAxis { Horizontal, Vertical }

    [Header("Motion")]
    public MoveAxis axis = MoveAxis.Horizontal;
    public float distance = 3f;        // total peak-to-peak travel distance (world units)
    public float speed = 1.4f;         // oscillation frequency multiplier
    public float phaseOffset = 0f;     // lets multiple walls be out-of-sync

    [Header("Visuals")]
    public float glowPulseSpeed = 3f;
    public float glowPulseAmount = 0.25f;

    private Vector3 startLocalPos;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Rigidbody2D rb;

    void Start()
    {
        startLocalPos = transform.localPosition;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;

        // Use a kinematic Rigidbody2D for efficient moving-collider physics.
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        float t = Mathf.Sin((Time.time + phaseOffset) * speed);
        Vector3 offset = axis == MoveAxis.Horizontal
            ? new Vector3(t * distance * 0.5f, 0f, 0f)
            : new Vector3(0f, t * distance * 0.5f, 0f);

        // Move via Rigidbody2D so colliders update smoothly.
        if (rb != null && transform.parent != null)
            rb.MovePosition(transform.parent.TransformPoint(startLocalPos + offset));
        else
            transform.localPosition = startLocalPos + offset;
    }

    void Update()
    {
        // Pulsing brightness so the player can tell this wall is animated.
        if (spriteRenderer != null)
        {
            float pulse = 1f - glowPulseAmount + glowPulseAmount * Mathf.Sin(Time.time * glowPulseSpeed);
            spriteRenderer.color = baseColor * (1f + pulse * 0.15f);
        }
    }
}
