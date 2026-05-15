using UnityEngine;

public class SpikeWall : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageToHero = 20;
    public float heroKnockbackForce = 8f;
    public bool instantKillEnemy = true;
    public float damageCooldown = 1f;

    private float lastDamageTime = 0f;

    void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    void HandleCollision(GameObject obj)
    {
        // Игнорируем трупы по слоям
        if (obj.layer == LayerMask.NameToLayer("PlayerDead") ||
            obj.layer == LayerMask.NameToLayer("EnemyDead"))
            return;

        // Урон герою
        if (obj.CompareTag("Player") && Time.time - lastDamageTime >= damageCooldown)
        {
            HeroMove hero = obj.GetComponent<HeroMove>();
            if (hero != null && !hero.IsDead)
            {
                lastDamageTime = Time.time;
                hero.TakeHit(damageToHero, transform.position.x);

                Rigidbody2D heroRb = obj.GetComponent<Rigidbody2D>();
                if (heroRb != null)
                {
                    float direction = (obj.transform.position.x - transform.position.x) > 0 ? 1f : -1f;
                    heroRb.linearVelocity = new Vector2(direction * heroKnockbackForce, heroRb.linearVelocity.y);
                }
            }
        }

        // Мгновенное убийство врага
        if (obj.CompareTag("Enemy"))
        {
            MeleeEnemy enemy = obj.GetComponent<MeleeEnemy>();
            if (enemy != null && !enemy.IsDead)
            {
                if (instantKillEnemy)
                    enemy.TakeDamage(9999, transform.position.x);
                else
                    enemy.TakeDamage(damageToHero, transform.position.x);
            }
        }
    }
}