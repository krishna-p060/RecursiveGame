using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Weapon Display")]
    public SpriteRenderer weaponIconRenderer;

    private string currentWeapon = "";
    private GameObject weaponArtRoot;

    public string CurrentWeapon => currentWeapon;

    public void EquipWeapon(string weaponName, Color weaponColor)
    {
        currentWeapon = weaponName;

        if (weaponIconRenderer != null)
        {
            // Hide the placeholder circle — we render a proper composite icon instead.
            weaponIconRenderer.enabled = false;

            // Rebuild the visual composite weapon attached to the icon anchor.
            Transform anchor = weaponIconRenderer.transform;

            if (weaponArtRoot != null)
                Destroy(weaponArtRoot);

            weaponArtRoot = new GameObject("WeaponArt");
            weaponArtRoot.transform.SetParent(anchor, false);
            weaponArtRoot.transform.localPosition = Vector3.zero;
            weaponArtRoot.transform.localRotation = Quaternion.identity;
            weaponArtRoot.transform.localScale = Vector3.one;

            ArtBuilder.BuildWeaponArt(weaponArtRoot.transform, weaponName, weaponColor);
        }

        UIManager.Instance?.UpdateWeaponDisplay(weaponName, weaponColor);
        UIManager.Instance?.ShowDialog($"Picked up: {weaponName}");

        // Reaching the base case arms a bomb — player must now race back up and escape.
        if (weaponName == "Dagger" && BombTimer.Instance != null && !BombTimer.Instance.IsActive)
        {
            BombTimer.Instance.StartBomb();
        }
    }

    public bool HasWeapon(string weaponName)
    {
        return currentWeapon == weaponName;
    }

    public void ClearWeapon()
    {
        currentWeapon = "";
        if (weaponIconRenderer != null)
            weaponIconRenderer.enabled = false;
        if (weaponArtRoot != null)
        {
            Destroy(weaponArtRoot);
            weaponArtRoot = null;
        }
        UIManager.Instance?.UpdateWeaponDisplay("None", Color.gray);
    }
}
