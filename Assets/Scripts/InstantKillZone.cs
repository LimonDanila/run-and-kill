using UnityEngine;

public class InstantKillZone : MonoBehaviour
{
    [Header("Kill Settings")]
    public bool killHero = true;              // Убивать ли героя
    public bool killEnemy = false;            // Убивать ли врагов
    public bool killPlayer = true;            // Убивать ли игрока

    [Header("Damage Fallback")]
    public int fallbackDamage = 9999;         // Урон при касании (если не мгновенная смерть)
    public bool instantDeath = true;          // Мгновенная смерть или просто большой урон

    [Header("Effects")]
    public GameObject deathEffectPrefab;      // Эффект при смерти
    public AudioClip deathSound;              // Звук при смерти

    [Header("Layer Settings")]
    public LayerMask targetLayers;            // Какие слои убивать (оставьте пустым для всех)

    private Collider2D killZoneCollider;

    void Start()
    {
        killZoneCollider = GetComponent<Collider2D>();

        // Если не заданы целевые слои, убиваем всех
        if (targetLayers == 0)
        {
            targetLayers = ~0; // Все слои
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleKill(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleKill(collision.gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        HandleKill(other.gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        HandleKill(collision.gameObject);
    }

    void HandleKill(GameObject obj)
    {
        // Проверяем слой
        if (((1 << obj.layer) & targetLayers) == 0)
            return;

        // Герой
        if (killHero && (obj.CompareTag("Player")))
        {
            KillHero(obj);
        }

        // Враг
        if (killEnemy && obj.CompareTag("Enemy"))
        {
            KillEnemy(obj);
        }
    }

    void KillHero(GameObject heroObject)
    {
        HeroMove hero = heroObject.GetComponent<HeroMove>();
        if (hero == null) return;

        // Если уже мёртв - не трогаем
        if (hero.IsDead) return;

        Debug.Log("Instant Kill Zone: Герой убит!");

        if (instantDeath)
        {
            // Мгновенная смерть (устанавливаем здоровье в 0)
            // Наносим огромный урон, который гарантированно убьёт
            hero.TakeHit(fallbackDamage, transform.position.x);
        }
        else
        {
            // Обычный урон
            hero.TakeHit(fallbackDamage, transform.position.x);
        }

        // Эффекты
        SpawnEffects(heroObject.transform.position);
    }

    void KillEnemy(GameObject enemyObject)
    {
        MeleeEnemy enemy = enemyObject.GetComponent<MeleeEnemy>();
        if (enemy == null) return;

        // Если уже мёртв - не трогаем
        if (enemy.IsDead) return;

        Debug.Log("Instant Kill Zone: Враг убит!");

        if (instantDeath)
        {
            enemy.TakeDamage(9999, transform.position.x);
        }
        else
        {
            enemy.TakeDamage(fallbackDamage, transform.position.x);
        }

        // Эффекты
        SpawnEffects(enemyObject.transform.position);
    }

    void SpawnEffects(Vector3 position)
    {
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, position);
        }
    }

    // Визуализация зоны убийства в редакторе
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // Рисуем красную полупрозрачную область
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(transform.position, col.bounds.size);

            // Рисуем красную рамку
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else
        {
            // Если нет коллайдера, рисуем маленькую сферу
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}