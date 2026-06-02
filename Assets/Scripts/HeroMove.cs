using UnityEngine;

public class HeroMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float minJumpForce = 8f;
    public float maxHoldTime = 0.25f;

    [Header("Gravity Settings")]
    public float fallGravityMultiplier = 2f;

    [Header("Landing Settings")]
    public float landingAnimationTime = 0.15f;

    [Header("Wall Settings")]
    public float wallCheckDistance = 0.1f;
    public float wallCheckHeightOffset = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask rightWallLayer;

    [Header("Hit Settings")]
    public float hitKnockbackForce = 5f;
    public float hitStunDuration = 0.25f;
    public float invincibilityDuration = 1f;

    [Header("Health Settings")]
    public int maxHealth = 100;               // Максимальное здоровье
    public int currentHealth;                 // Текущее здоровье

    [Header("Stamina Settings")]
    public int maxStamina = 4;                // Максимальная стамина
    public float staminaRegenRate = 0.5f;     // Восстановление стамины в секунду
    public float staminaRegenDelay = 1f;      // Задержка перед восстановлением после траты

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    public SpriteRenderer Sprite
    {
        get { return sprite; }
    }
    private Animator anim;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFalling;
    private bool wasFalling = false;

    private bool isJumping = false;
    private bool justLanded = false;
    private float landingTimer = 0f;
    private float jumpHoldTimer = 0f;
    private float originalGravity;

    private bool wallOnLeft;
    private bool wallOnRight;

    private bool isStunned = false;
    private float stunTimer = 0f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    private bool isBlockingRotation = false;
    private float lastDirection = 1f;

    public bool isTakingDamage = false;

    // Stamina variables
    public int currentStamina;
    private float staminaRegenTimer = 0f;
    private float staminaRegenTickTimer = 0f;  // Таймер для пульсирующего восстановления
    private bool isDead = false;
    private bool isAttacking = false;

    private bool isSimulatingMovement = false;
    private float simulatedHorizontalInput = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
        originalGravity = rb.gravityScale;

        ApplyShopUpgrades();

        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    void Update()
    {
        if (isDead) return;

        isBlockingRotation = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Обновление таймеров
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
            }
        }

        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (sprite != null)
                    sprite.color = Color.white;
            }
            else
            {
                float alpha = Mathf.PingPong(Time.time * 15f, 1f);
                if (sprite != null)
                    sprite.color = new Color(1f, 1f, 1f, alpha);
            }
        }

        // ========== ИСПРАВЛЕННОЕ ВОССТАНОВЛЕНИЕ СТАМИНЫ ==========
        // Если стамина не максимальна и мы не атакуем и не получаем урон
        if (currentStamina < maxStamina && !isAttacking && !isTakingDamage && !isStunned)
        {
            // Уменьшаем таймер задержки
            if (staminaRegenTimer > 0)
            {
                staminaRegenTimer -= Time.deltaTime;
            }
            else
            {
                // Восстанавливаем стамину (пульсирующее восстановление)
                staminaRegenTickTimer += Time.deltaTime;
                if (staminaRegenTickTimer >= 0.1f) // Каждые 0.1 секунды
                {
                    staminaRegenTickTimer = 0f;
                    currentStamina = Mathf.Min(maxStamina, currentStamina + Mathf.CeilToInt(staminaRegenRate * 0.1f));

                    // Обновляем UI если есть
                    OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                }
            }
        }
        else if (currentStamina >= maxStamina)
        {
            // Если стамина полная, сбрасываем таймер восстановления
            staminaRegenTimer = 0;
            staminaRegenTickTimer = 0;
        }
        // ==========================================================

        if (!isStunned && !isTakingDamage)
        {
            if (isSimulatingMovement)
            {
                // Используем эмулированный ввод
                horizontalInput = simulatedHorizontalInput;
            }
            else
            {
                // Нормальный ввод с клавиатуры
                horizontalInput = Input.GetAxisRaw("Horizontal");
            }

            if (!isBlockingRotation && horizontalInput != 0)
            {
                lastDirection = horizontalInput > 0 ? 1f : -1f;
            }
        }
        else
        {
            horizontalInput = 0;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        CheckWalls();

        isFalling = !isGrounded && rb.linearVelocity.y < -0.1f;

        if (!isGrounded)
        {
            wasFalling = true;
        }

        if (wasFalling && isGrounded && !justLanded)
        {
            justLanded = true;
            landingTimer = landingAnimationTime;
            wasFalling = false;
        }

        if (justLanded)
        {
            landingTimer -= Time.deltaTime;
            if (landingTimer <= 0f)
            {
                justLanded = false;
            }
        }

        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("verticalSpeed", rb.linearVelocity.y);
        anim.SetBool("isFalling", isFalling);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("justLanded", justLanded);
        anim.SetBool("isTakingDamage", isTakingDamage);
        anim.SetBool("isDead", isDead);

        if (Input.GetButtonDown("Jump") && isGrounded && !isStunned && !isTakingDamage)
        {
            isJumping = true;
            jumpHoldTimer = 0f;
            justLanded = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetBool("isJumping", true);
        }

        if (Input.GetButton("Jump") && isJumping && jumpHoldTimer < maxHoldTime && !isStunned && !isTakingDamage)
        {
            jumpHoldTimer += Time.deltaTime;

            if (rb.linearVelocity.y < jumpForce * 0.8f && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.85f);
            }
        }

        if (Input.GetButtonUp("Jump") && isJumping)
        {
            isJumping = false;
            anim.SetBool("isJumping", false);

            if (rb.linearVelocity.y > minJumpForce)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, minJumpForce);
            }
        }

        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            isJumping = false;
            anim.SetBool("isJumping", false);
        }

        if (sprite != null && !isStunned && !isTakingDamage)
        {
            if (isBlockingRotation)
            {
                // Не поворачиваем
            }
            else
            {
                if (horizontalInput > 0)
                    sprite.flipX = false;
                else if (horizontalInput < 0)
                    sprite.flipX = true;
            }
        }
    }

    public void StartMovingRight()
    {
        isSimulatingMovement = true;
        simulatedHorizontalInput = 1f;
        Debug.Log("HeroMove: начата эмуляция движения вправо");
    }

    // Вызывается из кат-сцены для остановки эмуляции
    public void StopMovingRight()
    {
        isSimulatingMovement = false;
        simulatedHorizontalInput = 0f;
        Debug.Log("HeroMove: эмуляция движения остановлена");
    }

    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;

        // Когда атака заканчивается, запускаем таймер восстановления стамины
        if (!attacking && currentStamina < maxStamina)
        {
            staminaRegenTimer = staminaRegenDelay;
            staminaRegenTickTimer = 0;
        }
    }

    public bool HasEnoughStamina(int amount)
    {
        return currentStamina >= amount;
    }

    public void UseStamina(int amount)
    {
        currentStamina -= amount;
        staminaRegenTimer = staminaRegenDelay;  // Сбрасываем таймер восстановления
        staminaRegenTickTimer = 0;               // Сбрасываем тик-таймер

        // Обновляем UI если есть
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        // Если стамина кончилась, возможно блокируем действия
        if (currentStamina <= 0)
        {
            Debug.Log("Статична кончилась!");
        }
    }

    public int GetCurrentStamina()
    {
        return currentStamina;
    }

    public int GetMaxStamina()
    {
        return maxStamina;
    }

    // События для UI
    public System.Action<int, int> OnHealthChanged;
    public System.Action<int, int> OnStaminaChanged;

    public void TakeHit(int damage, float attackerX)
    {
        if (isInvincible || isDead) return;
        if (isStunned) return;

        // Уменьшаем здоровье
        currentHealth -= damage;
        Debug.Log($"Герой получил урон: {damage}. Осталось здоровья: {currentHealth}");

        // Обновляем UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Проверяем смерть
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        isTakingDamage = true;

        anim.SetTrigger("Hit");

        isStunned = true;
        stunTimer = hitStunDuration;

        float knockbackDirection = (transform.position.x - attackerX) > 0 ? 1f : -1f;
        rb.linearVelocity = new Vector2(knockbackDirection * hitKnockbackForce, 3f);

        isJumping = false;
        anim.SetBool("isJumping", false);
    }

    public void OnDamageAnimationEnd()
    {
        isTakingDamage = false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        isTakingDamage = false;
        isStunned = false;
        isAttacking = false;

        // Меняем слой трупа на "PlayerDead"
        int deadLayer = LayerMask.NameToLayer("PlayerDead");
        gameObject.layer = deadLayer;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = deadLayer;
        }

        // ПОЛНОСТЬЮ ОБНУЛЯЕМ СКОРОСТЬ по всем осям
        rb.linearVelocity = Vector2.zero;  // Обнуляем и X, и Y скорость
        rb.angularVelocity = 0f;           // Обнуляем вращение

        // Устанавливаем нормальную гравитацию
        rb.gravityScale = originalGravity;

        // Запрещаем вращение (чтобы труп не крутился)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        horizontalInput = 0;

        // Запускаем анимацию смерти
        anim.SetTrigger("Death");
        anim.SetBool("isDead", true);

        // Отключаем скрипты
        HeroCombat combat = GetComponent<HeroCombat>();
        if (combat != null) combat.enabled = false;

        Debug.Log("Герой погиб!");

        OnHealthChanged?.Invoke(0, maxHealth);
    }

    // Вспомогательный метод для восстановления здоровья (например, аптечки)
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Герой восстановил {amount} здоровья. Теперь: {currentHealth}");
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetFacingDirection()
    {
        if (sprite != null)
        {
            return sprite.flipX ? -1f : 1f;
        }
        return 1f;
    }

    public bool IsBlockingRotation
    {
        get { return isBlockingRotation; }
    }

    public bool IsInvincible
    {
        get { return isInvincible; }
    }

    public bool IsStunned
    {
        get { return isStunned; }
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    void CheckWalls()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        float colliderHeight = boxCollider != null ? boxCollider.size.y : 1f;

        Vector2 topPoint = new Vector2(transform.position.x, transform.position.y + colliderHeight / 2f);
        Vector2 bottomPoint = new Vector2(transform.position.x, transform.position.y - colliderHeight / 2f);
        Vector2 middlePoint = transform.position;

        bool leftTop = CheckWallAtPoint(topPoint, Vector2.left);
        bool leftMiddle = CheckWallAtPoint(middlePoint, Vector2.left);
        bool leftBottom = CheckWallAtPoint(bottomPoint, Vector2.left);

        bool rightTop = CheckWallAtPoint(topPoint, Vector2.right);
        bool rightMiddle = CheckWallAtPoint(middlePoint, Vector2.right);
        bool rightBottom = CheckWallAtPoint(bottomPoint, Vector2.right);

        wallOnLeft = leftTop || leftMiddle || leftBottom;
        wallOnRight = rightTop || rightMiddle || rightBottom;
    }

    bool CheckWallAtPoint(Vector2 point, Vector2 direction)
    {
        RaycastHit2D hit1 = Physics2D.Raycast(point, direction, wallCheckDistance, groundLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(point, direction, wallCheckDistance, rightWallLayer);
        return hit1.collider != null || hit2.collider != null;
    }

    void FixedUpdate()
    {
        // НЕ ДВИГАЕМСЯ ЕСЛИ В СТАНЕ (но можем отлететь от удара)
        float horizontalMovement = horizontalInput * moveSpeed;

        if (!isGrounded && !isStunned)
        {
            if (horizontalInput < 0 && wallOnLeft)
            {
                horizontalMovement = 0;
            }
            else if (horizontalInput > 0 && wallOnRight)
            {
                horizontalMovement = 0;
            }
        }

        rb.linearVelocity = new Vector2(horizontalMovement, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = originalGravity * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = originalGravity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            float colliderHeight = boxCollider.size.y;

            Vector3 topPoint = new Vector3(transform.position.x, transform.position.y + colliderHeight / 2f, 0);
            Vector3 middlePoint = transform.position;
            Vector3 bottomPoint = new Vector3(transform.position.x, transform.position.y - colliderHeight / 2f, 0);

            Gizmos.color = Color.blue;

            Gizmos.DrawRay(topPoint, Vector3.left * wallCheckDistance);
            Gizmos.DrawRay(topPoint, Vector3.right * wallCheckDistance);

            Gizmos.DrawRay(middlePoint, Vector3.left * wallCheckDistance);
            Gizmos.DrawRay(middlePoint, Vector3.right * wallCheckDistance);
            Gizmos.DrawRay(bottomPoint, Vector3.left * wallCheckDistance);
            Gizmos.DrawRay(bottomPoint, Vector3.right * wallCheckDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(topPoint, 0.05f);
            Gizmos.DrawWireSphere(middlePoint, 0.05f);
            Gizmos.DrawWireSphere(bottomPoint, 0.05f);
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth; // Полное восстановление при улучшении
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Здоровье увеличено до {maxHealth}");
    }

    public void IncreaseMaxStamina(int amount)
    {
        maxStamina += amount;
        currentStamina = maxStamina; // Полное восстановление при улучшении
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        Debug.Log($"Стамина увеличена до {maxStamina}");
    }

    public void ApplyShopUpgrades()
    {
        // Загружаем уровень улучшений из PlayerPrefs
        int healthLevel = PlayerPrefs.GetInt("HealthLevel", 0);
        int staminaLevel = PlayerPrefs.GetInt("StaminaLevel", 0);

        Debug.Log($"ApplyShopUpgrades: HealthLevel={healthLevel}, StaminaLevel={staminaLevel}");

        // Применяем улучшения здоровья
        if (healthLevel > 0)
        {
            int additionalHealth = healthLevel * 40;
            maxHealth = 100 + additionalHealth;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            Debug.Log($"Применено улучшение здоровья: +{additionalHealth} HP (всего {maxHealth})");
        }

        // Применяем улучшения стамины
        if (staminaLevel > 0)
        {
            int additionalStamina = staminaLevel;
            maxStamina = 4 + additionalStamina;
            currentStamina = maxStamina;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            Debug.Log($"Применено улучшение стамины: +{additionalStamina} стамины (всего {maxStamina})");
        }
    }
}