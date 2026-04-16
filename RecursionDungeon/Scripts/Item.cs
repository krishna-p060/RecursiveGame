using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Item : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName;
    public Color weaponColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private bool pickedUp = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;
        SetupVisuals();
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
