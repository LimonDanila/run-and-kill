using UnityEngine;
using System.Collections;

public class BossEnemy : MeleeEnemy
{
    [Header("Boss Specific Settings")]
    public float rangedAttackRange = 7f;
    public float rangedAttackCooldown = 2.5f;
    public int rangedDamage = 8;

    [Header("Ranged Attack Settings")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float arrowSpeed = 5f;
    public float arrowLifetime = 3f;

    [Header("Portal Settings")]
    public GameObject portalPrefab;
    public int currentLevelNumber = 4;

    private bool canRangedAttack = true;
    private bool isRangedAttacking = false;
    private float facingCooldown = 0f;
    private float currentTargetDirection = 0f;

    private FallingSpikes fallingSpikes;

    protected override void Start()
    {
        base.Start();

        if (arrowSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("ArrowSpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = new Vector3(0.8f, 0.3f, 0);
            arrowSpawnPoint = spawnPoint.transform;
        }

        // Босс не отбрасывается
        knockbackForce = 0f;

        // Увеличиваем дальность обнаружения
        detectionRange = Mathf.Max(detectionRange, rangedAttackRange + 2f);

        fallingSpikes = FindObjectOfType<FallingSpikes>();
    }

    protected override void Update()
    {
        if (isDead) return;

        // Обновляем кулдаун поворота
        if (facingCooldown > 0)
            facingCooldown -= Time.deltaTime;

        // Неуязвимость и мигание
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

        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        UpdateAnimations();

        // Проверка атак
        if (!isAttacking && !isRangedAttacking && !isHitting && IsPlayerAlive() && IsPlayerDetected())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Ближняя атака
            if (canAttack && distanceToPlayer <= attackRange)
            {
                StartCoroutine(PerformAttack());
            }
            // Дальняя атака
            else if (canRangedAttack && distanceToPlayer <= rangedAttackRange && distanceToPlayer > attackRange)
            {
                StartCoroutine(RangedAttack());
            }
        }
    }

    protected override IEnumerator PerformAttack()
    {
        isAttacking = true;
        canAttack = false;

        anim.SetTrigger("Attack");
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(1.2f);

        isAttacking = false;
        anim.SetBool("isAttacking", false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    protected override void FixedUpdate()
    {
        if (isDead || isAttacking || isRangedAttacking || isHitting) return;

        if (!isGrounded)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Получаем направление движения
        float direction = GetMovementDirection();

        // Двигаемся
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // Обновляем направление спрайта (только если есть значительное движение)
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            UpdateSpriteDirectionFromVelocity();
        }
    }

    protected override float GetMovementDirection()
    {
        // Если игрока нет или он мёртв - патрулируем
        if (!IsPlayerAlive() || !IsPlayerDetected())
        {
            return GetPatrolDirection();
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Если игрок в радиусе ближней атаки - стоим на месте
        if (distanceToPlayer <= attackRange)
        {
            // Медленно поворачиваемся к игроку
            UpdateFacingToPlayer();
            return 0f;
        }

        // Игрок в зоне видимости - двигаемся к нему
        if (distanceToPlayer <= detectionRange)
        {
            // Направление к игроку
            float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);

            // Поворачиваемся к игроку
            UpdateFacingToPlayer();

            return directionToPlayer;
        }

        return GetPatrolDirection();
    }

    void UpdateFacingToPlayer()
    {
        if (player == null) return;
        if (facingCooldown > 0) return;

        bool shouldFaceRight = player.position.x > transform.position.x;
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            if (sprite != null)
                sprite.flipX = !facingRight;
            facingCooldown = 0.3f;
        }
    }

    void UpdateSpriteDirectionFromVelocity()
    {
        if (sprite == null) return;

        if (rb.linearVelocity.x > 0.1f && !facingRight)
        {
            facingRight = true;
            sprite.flipX = false;
        }
        else if (rb.linearVelocity.x < -0.1f && facingRight)
        {
            facingRight = false;
            sprite.flipX = true;
        }
    }

    IEnumerator RangedAttack()
    {
        isRangedAttacking = true;
        canRangedAttack = false;
        canAttack = false;

        // Поворачиваемся к игроку (мгновенно)
        if (player != null)
        {
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != facingRight)
            {
                facingRight = shouldFaceRight;
                if (sprite != null) sprite.flipX = !facingRight;
                facingCooldown = 0.5f;
            }
        }

        // Запускаем анимацию дальнобойной атаки
        anim.SetTrigger("RangedAttack");
        anim.SetBool("isRangedAttacking", true);

        // Ждём момент выстрела
        yield return new WaitForSeconds(0.7f);

        ShootArrow();

        yield return new WaitForSeconds(0.4f);

        isRangedAttacking = false;
        anim.SetBool("isRangedAttacking", false);

        yield return new WaitForSeconds(rangedAttackCooldown);
        canRangedAttack = true;
        canAttack = true;
    }

    void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("BossEnemy: не назначен префаб стрелы!");
            return;
        }

        if (!IsPlayerAlive() || player == null) return;

        // Направление к игроку
        Vector2 direction = (player.position - arrowSpawnPoint.position).normalized;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

        BossArrowProjectile arrowScript = arrow.GetComponent<BossArrowProjectile>();
        if (arrowScript != null)
        {
            arrowScript.Initialize(direction, arrowSpeed, rangedDamage, arrowLifetime, gameObject);
        }
        else
        {
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * arrowSpeed;
            }
            Destroy(arrow, arrowLifetime);
        }

        Debug.Log($"Босс выстрелил!");
    }

    // Переопределяем получение урона - без отбрасывания
    public override void TakeDamage(int damage, float attackerX)
    {
        if (isDead) return;
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Босс получил урон: {damage}. Осталось: {currentHealth}/{maxHealth}");

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Только анимация попадания (без отбрасывания)
        anim.SetTrigger("Hit");
        anim.SetBool("isHitting", true);
        StartCoroutine(ResetHitAnimation());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator ResetHitAnimation()
    {
        yield return new WaitForSeconds(0.3f);
        if (anim != null)
            anim.SetBool("isHitting", false);
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        canAttack = false;
        canRangedAttack = false;

        int deadLayer = LayerMask.NameToLayer("EnemyDead");
        if (deadLayer != -1)
        {
            gameObject.layer = deadLayer;
            foreach (Transform child in transform)
            {
                child.gameObject.layer = deadLayer;
            }
        }

        if (fallingSpikes != null)
        {
            fallingSpikes.StopSpawning();
            fallingSpikes.ClearAllSpikes();
        }

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1f;

        anim.SetTrigger("Death");
        anim.SetBool("isDead", true);

        // ========== СПАВН ПОРТАЛА ПОСЛЕ СМЕРТИ ==========
        SpawnPortalAfterDeath();
        // ================================================

        this.enabled = false;
        Destroy(gameObject, 5f);

        Debug.Log("Босс повержен!");
    }

    void SpawnPortalAfterDeath()
    {
        if (portalPrefab == null)
        {
            Debug.LogWarning("BossEnemy: portalPrefab не назначен!");
            return;
        }

        // Позиция портала - там где умер босс
        Vector3 portalPosition = transform.position;

        // Небольшое смещение вверх, чтобы портал был над землёй
        portalPosition.y += 1f;

        // Создаём портал
        GameObject portal = Instantiate(portalPrefab, portalPosition, Quaternion.identity);

        // Настраиваем портал для завершения уровня
        Portal portalScript = portal.GetComponent<Portal>();
        if (portalScript != null)
        {
            // Устанавливаем номер уровня (можно передать из LevelGenerator)
            portalScript.levelNumber = currentLevelNumber;
        }

        // Добавляем эффект появления портала (опционально)
        SpawnPortalEffect(portalPosition);

        Debug.Log($"Портал спавнен на позиции {portalPosition}");
    }

    void SpawnPortalEffect(Vector3 position)
    {
        // Эффект появления портала (частицы, вспышка и т.д.)
        GameObject effectPrefab = null; // Замените на ваш префаб эффекта
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    protected override void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.2f && isGrounded && !isAttacking && !isRangedAttacking && !isHitting;

        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isGrounded", isGrounded);
    }
}