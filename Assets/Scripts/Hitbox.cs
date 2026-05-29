using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Settings")]
    public bool isPlayerHitbox = true;
    public int damage = 10;
    public float knockbackForce = 5f;
    public float damageCooldown = 0.5f;  // Задержка между уроном

    private float lastDamageTime = -999f;
    private HeroMove hero;
    private MeleeEnemy enemy;

    void Start()
    {
        if (isPlayerHitbox)
        {
            hero = GetComponentInParent<HeroMove>();
            // Устанавливаем слой
            gameObject.layer = LayerMask.NameToLayer("PlayerHitbox");
        }
        else
        {
            enemy = GetComponentInParent<MeleeEnemy>();
            gameObject.layer = LayerMask.NameToLayer("EnemyHitbox");
        }

        // Делаем коллайдер триггером
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {

       Debug.Log($"Hitbox {gameObject.name} коснулся {other.name}");

        if (Time.time - lastDamageTime < damageCooldown) return;

        if (isPlayerHitbox)
        {
            // Хитбокс игрока: получаем урон от врага
            if (other.CompareTag("EnemyHitbox") || other.CompareTag("Enemy"))
            {
                MeleeEnemy hitEnemy = other.GetComponentInParent<MeleeEnemy>();
                if (hitEnemy != null && !hitEnemy.IsDead)
                {
                    if (hero != null && !hero.IsInvincible && !hero.IsDead)
                    {
                        lastDamageTime = Time.time;
                        hero.TakeHit(hitEnemy.damage, hitEnemy.transform.position.x);
                        Debug.Log($"Игрок получил урон от {hitEnemy.name}!");
                    }
                }
            }
        }
        //else
        //{
        //    // Хитбокс врага: наносим урон игроку
        //    if (other.CompareTag("PlayerHitbox") || other.CompareTag("Player"))
        //    {
        //        HeroMove hitHero = other.GetComponentInParent<HeroMove>();
        //        if (hitHero != null && !hitHero.IsDead && !hitHero.IsInvincible)
        //        {
        //            lastDamageTime = Time.time;
        //            hitHero.TakeHit(damage, transform.position.x);
        //            Debug.Log($"Враг {gameObject.name} нанёс урон игроку!");
        //        }
        //    }
        //}
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerStay2D(other);
    }
}