using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour
{
    public enum DoorType { EnterDeeper, ExitToParent }

    [Header("Door Config")]
    public DoorType doorType;
    public int targetRoomDepth;
    public bool isLocked = false;

    [Header("Visuals")]
    public SpriteRenderer doorRenderer;
    public Color unlockedColor = new Color(0.2f, 0.8f, 0.2f);
    public Color lockedColor = new Color(0.8f, 0.2f, 0.2f);

    [Header("Lock Indicator")]
    public GameObject lockIndicator;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        UpdateVisuals();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (isLocked)
        {
            UIManager.Instance?.ShowDialog("This door is locked! Defeat the monster first.");
            return;
        }

        if (doorType == DoorType.EnterDeeper)
            RoomManager.Instance?.EnterRoom(targetRoomDepth);
        else
            RoomManager.Instance?.ExitRoom(targetRoomDepth);
    }

    public void Lock()
    {
        isLocked = true;
        UpdateVisuals();
    }

    public void Unlock()
    {
        isLocked = false;
        UpdateVisuals();
        UIManager.Instance?.ShowDialog("The exit door is now unlocked!");
    }

    void UpdateVisuals()
    {
        if (doorRenderer != null)
            doorRenderer.color = isLocked ? lockedColor : unlockedColor;
        if (lockIndicator != null)
            lockIndicator.SetActive(isLocked);
    }
}
