using UnityEngine;
using System.Collections;

public class HeroCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.2f;
    public int lightAttackDamage = 10;
    public int heavyAttackDamage = 25;

    [Header("Stamina Costs")]
    public int lightAttackStaminaCost = 1;
    public int heavyAttackStaminaCost = 2;

    [Header("Hit Effect")]
    public GameObject hitEffectPrefab;

    private Animator anim;
    private HeroMove movement;
    private SpriteRenderer sprite;

    private bool isAttacking = false;
    private bool canAct = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<HeroMove>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        ApplyShopUpgrades();
    }

    void Update()
    {
        if (movement == null || movement.IsDead) return;
        if (movement.IsStunned || movement.isTakingDamage) return;

        // Быстрая атака (левая кнопка мыши)
        if (Input.GetMouseButtonDown(0) && !isAttacking && !movement.IsStunned)
        {
            if (movement.HasEnoughStamina(lightAttackStaminaCost))
            {
                StartCoroutine(LightAttack());
            }
            else
            {
                Debug.Log("Недостаточно стамины для быстрой атаки!");
            }
        }

        // Сильная атака (правая кнопка мыши)
        if (Input.GetMouseButtonDown(1) && !isAttacking && !movement.IsStunned)
        {
            if (movement.HasEnoughStamina(heavyAttackStaminaCost))
            {
                StartCoroutine(HeavyAttack());
            }
            else
            {
                Debug.Log("Недостаточно стамины для сильной атаки!");
            }
        }
    }

    IEnumerator LightAttack()
    {
        isAttacking = true;
        movement.SetAttacking(true);
        movement.UseStamina(lightAttackStaminaCost);

        anim.SetTrigger("Attack1");
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(0.4f);

        anim.SetTrigger("EndAttack");

        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        movement.SetAttacking(false);
        anim.SetBool("isAttacking", false);
    }

    IEnumerator HeavyAttack()
    {
        isAttacking = true;
        movement.SetAttacking(true);
        movement.UseStamina(heavyAttackStaminaCost);

        anim.SetTrigger("Attack2");
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(0.7f);

        anim.SetTrigger("EndAttack");

        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        movement.SetAttacking(false);
        anim.SetBool("isAttacking", false);
    }

    public void DealLightDamage()
    {
        if (movement != null && movement.IsDead) return;

        float direction = movement != null ? movement.GetFacingDirection() : (sprite.flipX ? -1f : 1f);
        Vector2 attackOrigin = new Vector2(transform.position.x + direction * attackRange, transform.position.y);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, attackRange);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                MeleeEnemy enemyScript = enemy.GetComponent<MeleeEnemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(lightAttackDamage, transform.position.x);

                    if (hitEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(hitEffectPrefab, enemy.transform.position, Quaternion.identity);
                        Destroy(effect, 0.5f);
                    }
                }
            }
        }
    }

    public void DealHeavyDamage()
    {
        if (movement != null && movement.IsDead) return;

        float direction = movement != null ? movement.GetFacingDirection() : (sprite.flipX ? -1f : 1f);
        Vector2 attackOrigin = new Vector2(transform.position.x + direction * attackRange, transform.position.y);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, attackRange);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                MeleeEnemy enemyScript = enemy.GetComponent<MeleeEnemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(heavyAttackDamage, transform.position.x);

                    if (hitEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(hitEffectPrefab, enemy.transform.position, Quaternion.identity);
                        Destroy(effect, 0.5f);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float direction = 1f;
        if (movement != null)
            direction = movement.GetFacingDirection();
        else if (sprite != null)
            direction = sprite.flipX ? -1f : 1f;
        Vector2 attackOrigin = new Vector2(transform.position.x + direction * attackRange, transform.position.y);
        Gizmos.DrawWireSphere(attackOrigin, attackRange);
    }

    public void ApplyShopUpgrades()
    {
        // Загружаем уровень улучшений
        int damageLevel = PlayerPrefs.GetInt("DamageLevel", 0);

        // Базовый урон
        int baseLightDamage = 10;
        int baseHeavyDamage = 25;

        // Применяем улучшения урона (каждое улучшение +5 к легкой атаке, +10 к тяжелой)
        if (damageLevel > 0)
        {
            int additionalDamage = damageLevel * 5;
            lightAttackDamage = baseLightDamage + additionalDamage;
            heavyAttackDamage = baseHeavyDamage + (additionalDamage * 2);

            Debug.Log($"Применено улучшение урона: +{additionalDamage} к легкой атаке, +{additionalDamage * 2} к тяжелой");
        }
    }
}