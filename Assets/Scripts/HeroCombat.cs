using UnityEngine;
using System.Collections;

public class HeroCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1f;
    public float lightAttackCooldown = 0.4f;
    public float heavyAttackCooldown = 1f;
    public int lightAttackDamage = 10;
    public int heavyAttackDamage = 25;

    [Header("Hit Effect")]
    public GameObject hitEffectPrefab;

    private Animator anim;
    private HeroMove movement;
    private SpriteRenderer sprite;

    private bool isAttacking = false;
    private bool canLightAttack = true;
    private bool canHeavyAttack = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<HeroMove>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (movement != null && movement.IsStunned) return;

        // Быстрая атака (левая кнопка мыши)
        if (Input.GetMouseButtonDown(0) && canLightAttack && !isAttacking && movement)
        {
            StartCoroutine(LightAttack());
        }

        // Сильная атака (правая кнопка мыши)
        if (Input.GetMouseButtonDown(1) && canHeavyAttack && !isAttacking && movement)
        {
            StartCoroutine(HeavyAttack());
        }
    }

    IEnumerator LightAttack()
    {
        isAttacking = true;
        canLightAttack = false;

        Debug.Log("Быстрая атака!");

        anim.SetTrigger("Attack1");
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(0.4f);

        anim.SetTrigger("EndAttack");

        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        anim.SetBool("isAttacking", false);

        yield return new WaitForSeconds(lightAttackCooldown);
        canLightAttack = true;
    }

    IEnumerator HeavyAttack()
    {
        isAttacking = true;
        canHeavyAttack = false;

        Debug.Log("Сильная атака!");

        anim.SetTrigger("Attack2");
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(0.8f);

        anim.SetTrigger("EndAttack");

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        anim.SetBool("isAttacking", false);

        yield return new WaitForSeconds(heavyAttackCooldown);
        canHeavyAttack = true;
    }

    // Метод для быстрой атаки из Animation Event
    public void DealLightDamage()
    {
        // Определяем направление атаки (куда смотрит персонаж)
        float direction = 1f;
        if (movement != null)
        {
            direction = movement.GetFacingDirection();
        }
        else if (sprite != null)
        {
            direction = sprite.flipX ? -1f : 1f;
        }

        Vector2 attackOrigin = new Vector2(transform.position.x + direction * attackRange, transform.position.y);

        Debug.Log($"Быстрая атака! Урон: {lightAttackDamage}, Направление: {(direction > 0 ? "Вправо" : "Влево")}");

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

    // Метод для сильной атаки из Animation Event
    public void DealHeavyDamage()
    {
        // Определяем направление атаки (куда смотрит персонаж)
        float direction = 1f;
        if (movement != null)
        {
            direction = movement.GetFacingDirection();
        }
        else if (sprite != null)
        {
            direction = sprite.flipX ? -1f : 1f;
        }

        Vector2 attackOrigin = new Vector2(transform.position.x + direction * attackRange, transform.position.y);

        Debug.Log($"Сильная атака! Урон: {heavyAttackDamage}, Направление: {(direction > 0 ? "Вправо" : "Влево")}");

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

    public void TakeDamage(int damage, float attackerX)
    {
        if (movement != null)
        {
            movement.TakeHit(damage, attackerX);
        }

        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float direction = 1f;
        if (movement != null)
        {
            direction = movement.GetFacingDirection();
        }
        else if (sprite != null)
        {
            direction = sprite.flipX ? -1f : 1f;
        }

        Vector2 attackOrigin = new Vector2(transform.position.x + direction * attackRange, transform.position.y);
        Gizmos.DrawWireSphere(attackOrigin, attackRange);
    }
}