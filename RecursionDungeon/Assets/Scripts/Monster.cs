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
    private Vector3 baseScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = monsterColor;
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (isDead) return;
        float pulse = 1f + Mathf.Sin(Time.time * 2.5f) * 0.08f;
        transform.localScale = baseScale * pulse;

        if (spriteRenderer != null)
        {
            float glow = 0.85f + Mathf.Sin(Time.time * 3f) * 0.15f;
            spriteRenderer.color = monsterColor * glow;
        }
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

        float duration = 0.6f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        // Flash white first
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(Color.white, new Color(1f, 0f, 0f, 0f), t);
            transform.Rotate(0, 0, Time.deltaTime * 720f);
            yield return null;
        }

        SpawnReward();

        if (exitDoor != null)
            exitDoor.Unlock();

        // Hide label too
        Transform label = transform.Find("Label");
        if (label != null) label.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    void SpawnReward()
    {
        if (string.IsNullOrEmpty(rewardWeaponName)) return;

        Vector3 spawnPos = rewardSpawnPoint != null
            ? rewardSpawnPoint.position
            : transform.position;

        // Create reward item directly (doesn't rely on any prefab template)
        GameObject reward = new GameObject($"Reward_{rewardWeaponName}");
        reward.transform.SetParent(transform.parent);
        reward.transform.position = spawnPos;
        reward.transform.localScale = Vector3.one * 0.6f;

        Sprite circleSprite = FindCircleSprite();

        // Glow behind
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(reward.transform, false);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.6f;
        SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = circleSprite;
        Color gc = rewardWeaponColor; gc.a = 0.25f;
        glowSR.color = gc;
        glowSR.sortingOrder = 4;

        // Main sprite
        SpriteRenderer sr = reward.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite;
        sr.color = rewardWeaponColor;
        sr.sortingOrder = 6;

        // Collider
        CircleCollider2D col = reward.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        // Item component
        Item item = reward.AddComponent<Item>();
        item.weaponName = rewardWeaponName;
        item.weaponColor = rewardWeaponColor;

        // Name label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(reward.transform, false);
        labelGO.transform.localPosition = new Vector3(0, -0.9f, 0);
        TextMesh tm = labelGO.AddComponent<TextMesh>();
        tm.text = rewardWeaponName;
        tm.fontSize = 26;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1f, 1f, 0.6f);
        tm.fontStyle = FontStyle.Bold;
        labelGO.GetComponent<MeshRenderer>().sortingOrder = 15;

        reward.SetActive(true);
    }

    Sprite FindCircleSprite()
    {
        // Try to grab the sprite from an existing Item/Monster in the scene
        Monster anyMonster = FindFirstObjectByType<Monster>();
        if (anyMonster != null)
        {
            SpriteRenderer sr = anyMonster.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null) return sr.sprite;
        }
        Item anyItem = FindFirstObjectByType<Item>();
        if (anyItem != null)
        {
            SpriteRenderer sr = anyItem.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null) return sr.sprite;
        }
        // Fallback: use our own sprite
        if (spriteRenderer != null) return spriteRenderer.sprite;
        return null;
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
