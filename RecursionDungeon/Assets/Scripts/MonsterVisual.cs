using UnityEngine;

/// <summary>
/// Attaches to a Monster GameObject and builds a detailed composite pixel-art
/// visual (face, horns, wings, etc.) as children of the monster transform.
/// The root SpriteRenderer is kept as the "body" circle; overlay parts layer on top.
/// </summary>
[RequireComponent(typeof(Monster))]
public class MonsterVisual : MonoBehaviour
{
    private bool built = false;

    void Awake()
    {
        BuildIfNeeded();
    }

    void BuildIfNeeded()
    {
        if (built) return;
        Monster m = GetComponent<Monster>();
        if (m == null || string.IsNullOrEmpty(m.monsterName)) return;
        ArtBuilder.BuildMonsterArt(transform, m.monsterName, m.monsterColor);
        built = true;
    }
}
