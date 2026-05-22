using UnityEngine;
using System.Collections;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;

    [Header("Detection Settings")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;

    [Header("Combat Settings")]
    public int damage = 5;
    public int maxHealth = 30;
    public float knockbackForce = 5f;
    public float invincibilityDuration = 0.5f;

    [Header("Contact Damage Settings")]
    public float contactDamageCooldown = 1f;
    public float contactKnockbackForce = 3f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.3f;

    [Header("Edge Check")]
    public float edgeCheckDistance = 0.5f;
    public Transform edgeCheckPoint;

    [Header("Hero Check")]
    public Transform player;

    protected Rigidbody2D rb;
    protected SpriteRenderer sprite;
    protected Animator anim;

    protected float startX;
    protected float patrolDirection = 1f;
    protected bool isGrounded;
    protected bool isTouchingWall;
    protected bool isNearEdge;
    protected bool facingRight = true;
    protected float currentDirection = 1f;

    protected int currentHealth;
    protected bool isAttacking = false;
    protected bool canAttack = true;
    protected bool isDead = false;
    protected bool isHitting = false;
    protected bool isInvincible = false;
    protected float invincibilityTimer = 0f;

    protected float lastContactDamageTime = 0f;

    // NEW: Кешируем компонент героя
    protected HeroMove heroMove;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();

        startX = transform.position.x;
        currentHealth = maxHealth;

        if (edgeCheckPoint == null)
        {
            GameObject edgeObj = new GameObject("EdgeCheck");
            edgeObj.transform.SetParent(transform);
            edgeObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            edgeCheckPoint = edgeObj.transform;
        }

        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
                heroMove = playerObject.GetComponent<HeroMove>();
                Debug.Log("Скелет: игрок найден автоматически!");
            }
            else
            {
                Debug.LogWarning("Скелет: не удалось найти игрока на сцене!");
            }
        }
        else
        {
            heroMove = player.GetComponent<HeroMove>();
        }

        // Устанавливаем слой для живого скелета
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
    }

    protected virtual void Update()
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

        // Проверка атаки ТОЛЬКО если герой жив
        if (canAttack && !isAttacking && !isHitting && IsPlayerAlive() && IsPlayerDetected())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(PerformAttack());
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || isAttacking || isHitting) return;

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

    protected virtual IEnumerator PerformAttack()
    {
        isAttacking = true;
        canAttack = false;

        anim.SetTrigger("Attack");
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(0.48f);

        isAttacking = false;
        anim.SetBool("isAttacking", false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // НОВЫЙ МЕТОД: Проверка жив ли герой
    protected bool IsPlayerAlive()
    {
        if (player == null) return false;
        if (heroMove == null)
        {
            heroMove = player.GetComponent<HeroMove>();
        }
        // Если герой мёртв или компонент отсутствует - возвращаем false
        if (heroMove == null) return false;
        return !heroMove.IsDead;
    }

    public void DealFirstHit()
    {
        // Наносим урон только если герой жив
        if (player == null) return;
        if (!IsPlayerAlive()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            HeroMove heroMovement = player.GetComponent<HeroMove>();
            if (heroMovement != null)
            {
                heroMovement.TakeHit(damage, transform.position.x);
                Debug.Log("Скелет нанёс УДАР 1");
            }
        }
    }

    public void DealSecondHit()
    {
        // Наносим урон только если герой жив
        if (player == null) return;
        if (!IsPlayerAlive()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            HeroMove heroMovement = player.GetComponent<HeroMove>();
            if (heroMovement != null)
            {
                heroMovement.TakeHit(damage, transform.position.x);
                Debug.Log("Скелет нанёс УДАР 2");
            }
        }
    }

    protected bool IsPlayerDetected()
    {
        if (player == null) return false;
        // НЕ обнаруживаем героя, если он мёртв
        if (!IsPlayerAlive()) return false;

        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    public void TakeDamage(int damage, float attackerX)
    {
        if (isDead) return;

        if (isInvincible)
        {
            Debug.Log("Скелет неуязвим, урон проигнорирован");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Скелет получил урон: {damage}. Осталось здоровья: {currentHealth}");

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        StartCoroutine(HandleHit(attackerX));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual IEnumerator HandleHit(float attackerX)
    {
        isHitting = true;
        isAttacking = false;
        canAttack = false;

        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Hit");
        anim.SetBool("isHitting", true);

        float knockbackDir = (transform.position.x - attackerX) > 0 ? 1 : -1;
        rb.linearVelocity = new Vector2(knockbackDir * knockbackForce, 3f);

        yield return new WaitForSeconds(0.4f);

        isHitting = false;
        anim.SetBool("isHitting", false);

        yield return new WaitForSeconds(0.3f);
        canAttack = true;
    }

    protected void Die()
    {
        if (isDead) return;

        isDead = true;
        canAttack = false;

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

        // Разрешаем вращение для реалистичного падения (опционально)
        // rb.constraints = RigidbodyConstraints2D.None;
        // rb.AddTorque(Random.Range(-3f, 3f));

        anim.SetTrigger("Death");
        anim.SetBool("isDead", true);

        // Отключаем коллайдер или оставляем его? Оставляем для земли
        // GetComponent<Collider2D>().enabled = true; // Не отключаем!

        // Отключаем скрипт, чтобы скелет не двигался и не атаковал
        this.enabled = false;

        // Удаляем объект через несколько секунд (опционально)
        Destroy(gameObject, 5f);

        Debug.Log("Скелет погиб и переключен на слой EnemyDead");
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    protected float GetMovementDirection()
    {
        // Если герой жив и видим - идём к нему
        if (IsPlayerAlive() && IsPlayerDetected())
        {
            float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
            int moveDir = (int)directionToPlayer;

            bool hasObstacle = (moveDir > 0 && isTouchingWall) || (moveDir < 0 && isTouchingWall) ||
                               (moveDir > 0 && isNearEdge) || (moveDir < 0 && isNearEdge);

            if (!hasObstacle)
            {
                return directionToPlayer;
            }
            return GetPatrolDirection();
        }

        // Если герой мёртв или не видим - патрулируем
        return GetPatrolDirection();
    }

    protected float GetPatrolDirection()
    {
        float currentX = transform.position.x;

        if (currentX >= startX + patrolDistance && patrolDirection > 0)
        {
            patrolDirection = -1f;
        }
        else if (currentX <= startX - patrolDistance && patrolDirection < 0)
        {
            patrolDirection = 1f;
        }

        if (isTouchingWall || isNearEdge)
        {
            patrolDirection = -patrolDirection;
        }

        return patrolDirection;
    }

    protected virtual void CheckWall()
    {
        int checkDirection = (int)patrolDirection;

        if (IsPlayerAlive() && IsPlayerDetected() && player != null)
        {
            checkDirection = (int)Mathf.Sign(player.position.x - transform.position.x);
        }

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            new Vector2(checkDirection, 0),
            wallCheckDistance,
            groundLayer
        );

        isTouchingWall = hit.collider != null;
    }

    protected virtual void CheckEdge()
    {
        int checkDirection = (int)patrolDirection;

        if (IsPlayerAlive() && IsPlayerDetected() && player != null)
        {
            checkDirection = (int)Mathf.Sign(player.position.x - transform.position.x);
        }

        Vector3 checkPos = edgeCheckPoint.position;
        checkPos.x = transform.position.x + (checkDirection * 0.3f);

        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, edgeCheckDistance, groundLayer);
        isNearEdge = hit.collider == null;
    }

    protected void UpdateSpriteDirection()
    {
        if (sprite == null) return;

        bool shouldFaceRight;

        if (IsPlayerAlive() && IsPlayerDetected() && player != null)
        {
            shouldFaceRight = player.position.x > transform.position.x;
        }
        else
        {
            if (currentDirection > 0)
                shouldFaceRight = true;
            else if (currentDirection < 0)
                shouldFaceRight = false;
            else
                return;
        }

        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            sprite.flipX = !facingRight;
        }
    }

    protected void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f && isGrounded && !isAttacking && !isHitting;

        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isGrounded", isGrounded);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(startX - patrolDistance, transform.position.y - 0.5f),
                        new Vector3(startX + patrolDistance, transform.position.y - 0.5f));

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        Gizmos.color = Color.blue;
        int checkDir = (int)patrolDirection;
        if (IsPlayerAlive() && IsPlayerDetected() && player != null)
            checkDir = (int)Mathf.Sign(player.position.x - transform.position.x);
        Vector3 wallCheckDir = checkDir > 0 ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, wallCheckDir * wallCheckDistance);

        if (edgeCheckPoint != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 edgeCheckPos = edgeCheckPoint.position;
            edgeCheckPos.x = transform.position.x + (checkDir * 0.3f);
            Gizmos.DrawRay(edgeCheckPos, Vector3.down * edgeCheckDistance);
        }
    }
}