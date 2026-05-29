using UnityEngine;

public class BossArrowProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 8;
    public float lifetime = 3f;

    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    private Vector2 direction;
    private Rigidbody2D rb;
    private bool hasHit = false;
    private GameObject sourceBoss;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(Vector2 dir, float spd, int dmg, float life, GameObject source = null)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        lifetime = life;
        sourceBoss = source;

        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        // Попадание в игрока
        if (collision.gameObject.CompareTag("Player"))
        {
            HeroMove hero = collision.gameObject.GetComponent<HeroMove>();
            if (hero != null && !hero.IsDead)
            {
                hero.TakeHit(damage, transform.position.x);
                Debug.Log($"Стрела босса нанесла {damage} урона!");
            }

            HitEffect();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            HeroMove hero = other.GetComponent<HeroMove>();
            if (hero != null && !hero.IsDead)
            {
                hero.TakeHit(damage, transform.position.x);
                Debug.Log($"Стрела босса нанесла {damage} урона!");
            }

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
}