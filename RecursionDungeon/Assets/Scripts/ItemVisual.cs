using UnityEngine;

/// <summary>
/// Attaches to an Item (weapon pickup) GameObject and builds a detailed composite
/// pixel-art weapon visual. Hides the root circle SpriteRenderer so the composite
/// (blade, guard, handle, etc.) is clearly visible on a transparent background.
/// A separate "Glow" child underneath (optional) can still provide an ambient pulse.
/// </summary>
[RequireComponent(typeof(Item))]
public class ItemVisual : MonoBehaviour
{
    private bool built = false;

    void Awake()
    {
        BuildIfNeeded();
    }

    void BuildIfNeeded()
    {
        if (built) return;
        Item item = GetComponent<Item>();
        if (item == null || string.IsNullOrEmpty(item.weaponName)) return;

        // Hide the root circle — the composite art replaces it.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        ArtBuilder.BuildWeaponArt(transform, item.weaponName, item.weaponColor);
        built = true;
    }
}
