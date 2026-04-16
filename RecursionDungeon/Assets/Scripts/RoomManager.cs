using UnityEngine;
using System.Collections;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("Rooms (ordered by depth 0-3)")]
    public GameObject[] rooms;

    [Header("Player Spawn Points (one per room)")]
    public Transform[] entryPoints;

    [Header("Player Reference")]
    public Transform player;

    [Header("Room Info")]
    public string[] roomNames = { "Grand Hall", "Dark Cave", "Crypt", "Tiny Chamber" };
    public string[] monsterNames = { "Dragon", "Wolf", "Bat", "" };
    public string[] requiredWeapons = { "Fire Sword", "Bow", "Dagger", "" };

    private int currentDepth = 0;
    public int CurrentDepth => currentDepth;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        ActivateRoom(0);
    }

    /// <summary>
    /// Recursive enter: push a new frame onto the call stack.
    /// Conceptually: SolveDungeon(depth) calls SolveDungeon(depth+1)
    /// </summary>
    public void EnterRoom(int targetDepth)
    {
        if (targetDepth < 0 || targetDepth >= rooms.Length) return;
        StartCoroutine(TransitionToRoom(targetDepth, true));
    }

    /// <summary>
    /// Recursive exit: pop a frame off the call stack (unwinding).
    /// </summary>
    public void ExitRoom(int targetDepth)
    {
        if (targetDepth < 0 || targetDepth >= rooms.Length) return;

        if (targetDepth == 0 && GameManager.Instance != null)
        {
            Door exitDoor = GetExitDoorForRoom(0);
            if (exitDoor != null && !exitDoor.isLocked)
            {
                GameManager.Instance.WinGame();
                return;
            }
        }

        StartCoroutine(TransitionToRoom(targetDepth, false));
    }

    IEnumerator TransitionToRoom(int targetDepth, bool entering)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        yield return CameraTransition.Instance?.FadeOut();

        rooms[currentDepth].SetActive(false);
        currentDepth = targetDepth;
        rooms[currentDepth].SetActive(true);

        if (entryPoints[currentDepth] != null)
            player.position = entryPoints[currentDepth].position;

        UIManager.Instance?.UpdateRoomDisplay(currentDepth, GetCurrentRoomName());

        if (entering)
        {
            CallStackUI.Instance?.PushFrame(currentDepth, GetCurrentRoomName(),
                monsterNames[currentDepth], requiredWeapons[currentDepth]);
        }
        else
        {
            CallStackUI.Instance?.PopFrame();
        }

        CallStackUI.Instance?.HighlightFrame(currentDepth);

        yield return CameraTransition.Instance?.FadeIn();

        if (pc != null) pc.SetCanMove(true);
    }

    void ActivateRoom(int depth)
    {
        for (int i = 0; i < rooms.Length; i++)
            rooms[i].SetActive(i == depth);

        currentDepth = depth;

        if (player != null && entryPoints[depth] != null)
            player.position = entryPoints[depth].position;

        UIManager.Instance?.UpdateRoomDisplay(depth, GetCurrentRoomName());
        CallStackUI.Instance?.PushFrame(depth, GetCurrentRoomName(),
            monsterNames[depth], requiredWeapons[depth]);
        CallStackUI.Instance?.HighlightFrame(depth);
    }

    public string GetCurrentRoomName()
    {
        if (currentDepth >= 0 && currentDepth < roomNames.Length)
            return roomNames[currentDepth];
        return "Unknown";
    }

    Door GetExitDoorForRoom(int depth)
    {
        Door[] doors = rooms[depth].GetComponentsInChildren<Door>(true);
        foreach (var d in doors)
        {
            if (d.doorType == Door.DoorType.ExitToParent)
                return d;
        }
        return null;
    }

    public void ResetRooms()
    {
        currentDepth = 0;
        for (int i = 0; i < rooms.Length; i++)
            rooms[i].SetActive(false);
    }
}
