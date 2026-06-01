using UnityEngine;
using System.Collections;

public class ArcherEnemy : MeleeEnemy
{
    [Header("Archer Specific Settings")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;
    public float attackRangeArcher = 7f;

    [Header("Line of Sight")]
    public float lineOfSightHeight = 1.5f;
    public LayerMask lineOfSightMask;
    public bool showDebugGizmos = true;
    public bool enableDebugLogs = true;

    [Header("Retreat Settings")]
    public float retreatDistance = 3f;
    public float minRetreatDistance = 4f;

    private bool isShooting = false;
    private float originalMoveSpeed;
    private bool isAiming = false;

    protected override void Start()
    {
        base.Start();

        originalMoveSpeed = moveSpeed;
        attackRange = attackRangeArcher;

        if (arrowSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("ArrowSpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = new Vector3(0.5f, 0.2f, 0);
            arrowSpawnPoint = spawnPoint.transform;
        }

        if (lineOfSightMask == 0)
        {
            lineOfSightMask = LayerMask.GetMask("Player", "Ground", "Wall");
        }
    }

    protected override void Update()
    {
        if (isDead) return;

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

        if (canAttack && !isShooting && !isHitting && IsPlayerAlive())
        {
            if (CanShootAtPlayer())
            {
                if (enableDebugLogs)
                    Debug.Log("Лучник: начинаю атаку!");
                StartCoroutine(PerformAttack());
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (isShooting || isAiming)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (!isGrounded)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        CheckWall();
        CheckEdge();

        float direction = GetMovementDirection();
        currentDirection = direction;

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        UpdateSpriteDirection();
    }

    float GetMovementDirection()
    {
        if (IsPlayerAlive() && IsPlayerDetected())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer < retreatDistance)
            {
                float directionFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
                return directionFromPlayer;
            }
        }

        return GetPatrolDirection();
    }

    new bool IsPlayerDetected()
    {
        if (player == null) return false;
        if (!IsPlayerAlive()) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= detectionRange;
    }

    bool CanShootAtPlayer()
    {
        if (player == null) return false;
        if (!IsPlayerAlive()) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange || distanceToPlayer < minRetreatDistance || lineOfSightHeight < Mathf.Abs(transform.position.y - player.position.y))
        {
            if (enableDebugLogs) Debug.Log($"CanShootAtPlayer: слишком далеко ({distanceToPlayer} > {attackRange})");
            return false;
        }

        // Определяем направление к игроку
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);

        // ИСПРАВЛЕНО: Используем центр объекта для луча, а не точку спавна стрелы
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + 0.2f);

        // Корректируем направление луча - строго по горизонтали
        Vector2 rayDirection = new Vector2(directionToPlayer, 0);

        // Расстояние для луча
        float rayDistance = Mathf.Abs(player.position.x - transform.position.x);


        // Пускаем луч
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, lineOfSightMask);

        // Рисуем луч для отладки
        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, 0.1f);

        if (hit.collider != null)
        {

            if (hit.collider.CompareTag("Player"))
            {
                // Поворачиваемся к игроку
                bool shouldFaceRight = player.position.x > transform.position.x;
                if (shouldFaceRight != facingRight)
                {
                    facingRight = shouldFaceRight;
                    if (sprite != null) sprite.flipX = !facingRight;
                }

                return true;
            }
        }
        else
        {
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != facingRight)
            {
                facingRight = shouldFaceRight;
                if (sprite != null) sprite.flipX = !facingRight;
            }

            return true;
        }

        return false;
    }

    protected override IEnumerator PerformAttack()
    {
        isShooting = true;
        canAttack = false;
        isAiming = true;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (player != null)
        {
            bool shouldFaceRight = player.position.x > transform.position.x;
            if (shouldFaceRight != facingRight)
            {
                facingRight = shouldFaceRight;
                if (sprite != null)
                    sprite.flipX = !facingRight;
            }
        }

        anim.SetTrigger("Attack");
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(0.35f);

        ShootArrow();

        yield return new WaitForSeconds(0.75f);

        isShooting = false;
        isAiming = false;
        anim.SetBool("isAttacking", false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("ArcherEnemy: не назначен префаб стрелы!");
            return;
        }

        Vector2 direction;

        if (player != null && IsPlayerAlive())
        {
            // Определяем направление к игроку
            float dirX = Mathf.Sign(player.position.x - transform.position.x);
            direction = new Vector2(dirX, 0);
            if (enableDebugLogs)
                Debug.Log($"ShootArrow: выстрел в сторону игрока, dirX = {dirX}");
        }
        else
        {
            direction = facingRight ? Vector2.right : Vector2.left;
            if (enableDebugLogs)
                Debug.Log($"ShootArrow: выстрел по направлению взгляда, direction = {direction}");
        }

        direction.Normalize();

        // Используем точку спавна стрелы или центр объекта
        Vector3 spawnPos = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        // Поворачиваем стрелу
        float angle = direction.x > 0 ? 0 : 180;
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

        ArrowProjectile arrowScript = arrow.GetComponent<ArrowProjectile>();
        if (arrowScript != null)
        {
            arrowScript.Initialize(direction, arrowSpeed, damage, arrowLifetime);
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

        if (enableDebugLogs)
            Debug.Log($"ShootArrow: стрела выпущена! Направление = {direction}");
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        if (arrowSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(arrowSpawnPoint.position, 0.1f);
        }

        if (!showDebugGizmos) return;

        // Зона обнаружения
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Зона атаки
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, attackRangeArcher);

        // Зона отступления
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        // Линия огня
        Gizmos.color = Color.cyan;
        float yPos = transform.position.y;
        Gizmos.DrawLine(new Vector3(transform.position.x - attackRangeArcher, yPos + 0.2f, 0),
                       new Vector3(transform.position.x + attackRangeArcher, yPos + 0.2f, 0));

        // Высота обзора
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawLine(new Vector3(transform.position.x - attackRangeArcher, yPos + lineOfSightHeight, 0),
                       new Vector3(transform.position.x + attackRangeArcher, yPos + lineOfSightHeight, 0));
        Gizmos.DrawLine(new Vector3(transform.position.x - attackRangeArcher, yPos - lineOfSightHeight, 0),
                       new Vector3(transform.position.x + attackRangeArcher, yPos - lineOfSightHeight, 0));

        // Точка для лучей
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + 0.2f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(rayOrigin, Vector2.right * attackRangeArcher);
        Gizmos.DrawRay(rayOrigin, Vector2.left * attackRangeArcher);

        if (Application.isPlaying && player != null && IsPlayerAlive())
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            float distance = Mathf.Abs(player.position.x - transform.position.x);
            Gizmos.color = CanShootAtPlayer() ? Color.green : Color.red;
            Gizmos.DrawRay(rayOrigin, new Vector2(direction, 0) * distance);
        }
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        canAttack = false;

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(2);
            Debug.Log("Лучник убит! +2 монеты");
        }

        // Меняем слой на "EnemyDead" (труп не взаимодействует с игроком)
        int deadLayer = LayerMask.NameToLayer("EnemyDead");
        if (deadLayer != -1)
        {
            gameObject.layer = deadLayer;
            foreach (Transform child in transform)
            {
                child.gameObject.layer = deadLayer;
            }
        }
        else
        {
            Debug.LogWarning("Слой 'EnemyDead' не найден! Создайте его в Project Settings → Tags and Layers");
        }

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1f;

        anim.SetTrigger("Death");
        anim.SetBool("isDead", true);

        // Отключаем скрипт
        this.enabled = false;

        // Удаляем объект через несколько секунд
        Destroy(gameObject, 5f);

        Debug.Log("Лучник погиб!");
    }
}