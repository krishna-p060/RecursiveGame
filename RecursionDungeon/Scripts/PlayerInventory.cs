using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Weapon Display")]
    public SpriteRenderer weaponIconRenderer;

    private string currentWeapon = "";

    public string CurrentWeapon => currentWeapon;

    public void EquipWeapon(string weaponName, Color weaponColor)
    {
        currentWeapon = weaponName;
        if (weaponIconRenderer != null)
        {
            weaponIconRenderer.color = weaponColor;
            weaponIconRenderer.enabled = true;
        }
        UIManager.Instance?.UpdateWeaponDisplay(weaponName, weaponColor);
        UIManager.Instance?.ShowDialog($"Picked up: {weaponName}");
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
        UIManager.Instance?.UpdateWeaponDisplay("None", Color.gray);
    }
}
