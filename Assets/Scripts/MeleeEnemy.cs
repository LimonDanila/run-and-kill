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
    public float contactDamageCooldown = 1f;  // Задержка между уроном при касании
    public float contactKnockbackForce = 3f;   // Сила отбрасывания при касании

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

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    private float startX;
    private float patrolDirection = 1f;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isNearEdge;
    private bool facingRight = true;
    private float currentDirection = 1f;

    private int currentHealth;
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool isDead = false;
    private bool isHitting = false;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    // НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ КОНТАКТНОГО УРОНА
    private float lastContactDamageTime = 0f;

    void Start()
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
    }

    void Update()
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

        if (canAttack && !isAttacking && !isHitting && IsPlayerDetected())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(PerformAttack());
            }
        }
    }

    void FixedUpdate()
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

    // НОВЫЙ МЕТОД: Обработка столкновения с героем
    void OnCollisionStay2D(Collision2D collision)
    {
        // Проверяем, не мёртв ли скелет и не в процессе атаки
        if (isDead) return;

        // Проверяем, что столкнулись с героем
        if (collision.gameObject.CompareTag("Player"))
        {
            // Проверяем задержку между уроном
            if (Time.time - lastContactDamageTime >= contactDamageCooldown)
            {
                lastContactDamageTime = Time.time;

                // Наносим урон герою
                HeroMove heroMovement = collision.gameObject.GetComponent<HeroMove>();
                if (heroMovement != null)
                {
                    heroMovement.TakeHit(damage, transform.position.x);
                    Debug.Log($"Скелет нанёс урон при касании: {damage}");
                }
            }
        }
    }

    // Альтернативный вариант: использование Trigger
    //void OnTriggerStay2D(Collider2D other)
    //{
    //    if (isDead) return;

    //    if (other.CompareTag("Hero") || other.CompareTag("Player"))
    //    {
    //        if (Time.time - lastContactDamageTime >= contactDamageCooldown)
    //        {
    //            lastContactDamageTime = Time.time;

    //            HeroMove heroMovement = other.GetComponent<HeroMove>();
    //            if (heroMovement != null)
    //            {
    //                heroMovement.TakeHit(damage, transform.position.x);
    //                Debug.Log($"Скелет нанёс урон (триггер): {damage}");
    //            }
    //        }
    //    }
    //}

    IEnumerator PerformAttack()
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

    public void DealFirstHit()
    {
        if (player == null) return;

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
        if (player == null) return;

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

    public void TakeDamage(int damage, float attackerX)
    {
        if (isDead) return;

        if (isInvincible)
        {
            Debug.Log("Скелет неуязвим, урон проигнорирован");
            return;
        }

        currentHealth -= damage;

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        StartCoroutine(HandleHit(attackerX));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HandleHit(float attackerX)
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

    void Die()
    {
        isDead = true;
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Death");
        anim.SetBool("isDead", true);

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 2f);
    }

    // Свойство для доступа к неуязвимости (опционально)
    public bool IsInvincible
    {
        get { return isInvincible; }
    }

    float GetMovementDirection()
    {
        if (IsPlayerDetected())
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
        return GetPatrolDirection();
    }

    float GetPatrolDirection()
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

    bool IsPlayerDetected()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    void CheckWall()
    {
        int checkDirection = (int)patrolDirection;

        if (IsPlayerDetected() && player != null)
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

    void CheckEdge()
    {
        int checkDirection = (int)patrolDirection;

        if (IsPlayerDetected() && player != null)
        {
            checkDirection = (int)Mathf.Sign(player.position.x - transform.position.x);
        }

        Vector3 checkPos = edgeCheckPoint.position;
        checkPos.x = transform.position.x + (checkDirection * 0.3f);

        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, edgeCheckDistance, groundLayer);
        isNearEdge = hit.collider == null;
    }

    void UpdateSpriteDirection()
    {
        if (sprite == null) return;

        bool shouldFaceRight;

        if (IsPlayerDetected() && player != null)
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

    void UpdateAnimations()
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
        if (IsPlayerDetected() && player != null)
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