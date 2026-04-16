using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Item : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName;
    public Color weaponColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private bool pickedUp = false;
    private Vector3 startPos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;
        SetupVisuals();
    }

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Floating bob animation
        float bob = Mathf.Sin(Time.time * 3f) * 0.15f;
        transform.position = startPos + new Vector3(0, bob, 0);

        // Gentle glow pulse
        if (spriteRenderer != null)
        {
            float glow = 0.8f + Mathf.Sin(Time.time * 4f) * 0.2f;
            spriteRenderer.color = weaponColor * glow;
        }
    }

    public void SetupVisuals()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = weaponColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            pickedUp = true;
            inventory.EquipWeapon(weaponName, weaponColor);
            Destroy(gameObject);
        }
    }
}
