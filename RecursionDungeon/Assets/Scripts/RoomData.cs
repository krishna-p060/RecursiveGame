using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomData", menuName = "RecursionDungeon/RoomData")]
public class RoomData : ScriptableObject
{
    public int depth;
    public string roomName;
    public bool hasMonster = true;
    public string monsterName;
    public Color monsterColor = Color.red;
    public string requiredWeapon;
    public string rewardWeapon;
    public string basePickupWeapon;
    public Color roomTint = Color.white;
    public float roomScale = 1f;
}
