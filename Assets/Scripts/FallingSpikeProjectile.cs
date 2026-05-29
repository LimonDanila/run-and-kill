using UnityEngine;

public class FallingSpikeProjectile : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 15;

    private float fallSpeed = 8f;
    private HeroMove heroMove;
    private bool hasHit = false;
    private Rigidbody2D rb;
    private float bottomBoundary;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        // Настройка физики
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.isKinematic = false;

        // Настройка коллайдера (как триггер, чтобы не сталкиваться с объектами)
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
        }
        collider.isTrigger = true;  // Триггер - пролетает сквозь объекты

        // Устанавливаем слой Projectile
        gameObject.layer = LayerMask.NameToLayer("Projectile");

        // Определяем нижнюю границу экрана
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            bottomBoundary = mainCamera.transform.position.y - 6f;
        }
        else
        {
            bottomBoundary = -6f;
        }
    }

    public void Initialize(float speed, HeroMove hero)
    {
        fallSpeed = speed;
        heroMove = hero;

        // Задаём скорость падения
        rb.linearVelocity = Vector2.down * fallSpeed;

        // Поворачиваем шип остриём вниз
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Уничтожаем через 10 секунд (запас)
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        // Проверка выхода за нижнюю границу экрана
        if (transform.position.y < bottomBoundary)
        {
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
        }
    }

    void HitEffect()
    {
        // Эффект частиц при попадании
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();

        // Небольшая тряска камеры
        StartCoroutine(CameraShake());
    }

    System.Collections.IEnumerator CameraShake()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        Vector3 originalPos = mainCam.transform.position;
        float shakeDuration = 0.1f;
        float shakeAmount = 0.05f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-shakeAmount, shakeAmount);
            float y = Random.Range(-shakeAmount, shakeAmount);
            mainCam.transform.position = originalPos + new Vector3(x, y, 0);
            yield return null;
        }

        mainCam.transform.position = originalPos;
    }
}