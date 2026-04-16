using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.2f;
    public LayerMask monsterLayer;

    private PlayerInventory inventory;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Attack();
    }

    void Attack()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position + transform.up * 0.7f,
            attackRange,
            monsterLayer
        );

        foreach (var hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster == null || monster.IsDead) continue;

            if (inventory.HasWeapon(monster.requiredWeapon))
            {
                monster.Defeat();
            }
            else
            {
                string needed = monster.requiredWeapon;
                UIManager.Instance?.ShowDialog($"You need the {needed}!");
            }
            return;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.up * 0.7f, attackRange);
    }
}
