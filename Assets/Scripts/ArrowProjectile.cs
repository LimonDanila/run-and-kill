using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Arrow Settings")]
    public float speed = 10f;
    public int damage = 5;
    public float lifetime = 3f;

    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    private Vector2 direction;
    private Rigidbody2D rb;
    private bool hasHit = false;
    private GameObject sourceEnemy;
    private Collider2D arrowCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        arrowCollider = GetComponent<Collider2D>();
        if (arrowCollider == null)
            arrowCollider = gameObject.AddComponent<BoxCollider2D>();

        // Настройка Rigidbody2D
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Настройка Collider2D
        arrowCollider.isTrigger = false; // Важно: не триггер для столкновений
        arrowCollider.enabled = true;
    }

    public void Initialize(Vector2 dir, float spd, int dmg, float life, GameObject source = null)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        lifetime = life;
        sourceEnemy = source;

        // Поворачиваем стрелу
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Задаём скорость
        if (rb != null)
            rb.linearVelocity = direction * speed;

        // Уничтожаем через время
        Destroy(gameObject, lifetime);

        // Игнорируем коллизию с источником (лучником)
        if (sourceEnemy != null)
        {
            Collider2D sourceCollider = sourceEnemy.GetComponent<Collider2D>();
            if (sourceCollider != null && arrowCollider != null)
            {
                Physics2D.IgnoreCollision(arrowCollider, sourceCollider, true);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        Debug.Log($"Стрела столкнулась с: {collision.gameObject.name}, тег: {collision.gameObject.tag}");

        // Попадание в игрока
        if (collision.gameObject.CompareTag("Player"))
        {
            HeroMove hero = collision.gameObject.GetComponent<HeroMove>();
            if (hero != null && !hero.IsDead)
            {
                hero.TakeHit(damage, transform.position.x);
                Debug.Log($"Стрела нанесла {damage} урона герою!");
            }

            HitEffect();
            Destroy(gameObject);
        }

        // Попадание в стену, землю или любой объект с коллайдером
        else
        {
            HitEffect();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        Debug.Log($"Стрела (триггер) столкнулась с: {other.name}, тег: {other.tag}");

        // Попадание в игрока
        if (other.CompareTag("Player"))
        {
            HeroMove hero = other.GetComponent<HeroMove>();
            if (hero != null && !hero.IsDead)
            {
                hero.TakeHit(damage, transform.position.x);
                Debug.Log($"Стрела нанесла {damage} урона герою!");
            }

            HitEffect();
            Destroy(gameObject);
        }

        // Попадание в землю или стену
        else if (other.CompareTag("Ground") ||
                 other.gameObject.layer == LayerMask.NameToLayer("Wall") ||
                 other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                 other.gameObject.layer == LayerMask.NameToLayer("SpikeWall"))
        {
            HitEffect();
            Destroy(gameObject);
        }
    }

    void HitEffect()
    {
        hasHit = true;

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }

    // Визуализация для отладки
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}