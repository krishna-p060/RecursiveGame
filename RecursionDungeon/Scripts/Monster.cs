using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour
{
    [Header("Monster Config")]
    public string monsterName;
    public string requiredWeapon;
    public Color monsterColor = Color.red;

    [Header("Reward")]
    public GameObject rewardItemPrefab;
    public string rewardWeaponName;
    public Color rewardWeaponColor = Color.white;
    public Transform rewardSpawnPoint;

    [Header("Room Reference")]
    public Door exitDoor;

    private bool isDead = false;
    public bool IsDead => isDead;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = monsterColor;
    }

    public void Defeat()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        UIManager.Instance?.ShowDialog($"{monsterName} defeated!");

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(monsterColor.r, monsterColor.g, monsterColor.b, 1f - t);
            yield return null;
        }

        SpawnReward();

        if (exitDoor != null)
            exitDoor.Unlock();

        gameObject.SetActive(false);
    }

    void SpawnReward()
    {
        if (string.IsNullOrEmpty(rewardWeaponName)) return;

        Vector3 spawnPos = rewardSpawnPoint != null ? rewardSpawnPoint.position : transform.position + Vector3.down * 0.5f;

        if (rewardItemPrefab != null)
        {
            GameObject reward = Instantiate(rewardItemPrefab, spawnPos, Quaternion.identity, transform.parent);
            Item item = reward.GetComponent<Item>();
            if (item != null)
            {
                item.weaponName = rewardWeaponName;
                item.weaponColor = rewardWeaponColor;
                item.SetupVisuals();
            }
        }
    }

    public void Setup(string name, Color color, string required, string reward, Color rewardColor)
    {
        monsterName = name;
        monsterColor = color;
        requiredWeapon = required;
        rewardWeaponName = reward;
        rewardWeaponColor = rewardColor;
        if (spriteRenderer != null)
            spriteRenderer.color = monsterColor;
    }
}
