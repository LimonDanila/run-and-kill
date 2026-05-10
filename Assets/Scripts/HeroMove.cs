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

    [Header("Hit Settings")]
    public float hitKnockbackForce = 5f;
    public float hitStunDuration = 0.25f;
    public float invincibilityDuration = 1f;

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

    // НОВАЯ ПЕРЕМЕННАЯ ДЛЯ БЛОКИРОВКИ ПЕРЕХОДОВ
    public bool isTakingDamage = false;  // Public чтобы можно было менять из другого скрипта

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        // ОБРАБОТКА ЗАЖАТОГО SHIFT
        isBlockingRotation = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Обновление таймеров
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                Debug.Log("Стан закончился");
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

        // Не обрабатываем ввод если оглушён ИЛИ получает урон
        if (!isStunned && !isTakingDamage)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

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

        // Параметры для аниматора
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("verticalSpeed", rb.linearVelocity.y);
        anim.SetBool("isFalling", isFalling);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("justLanded", justLanded);

        // НОВЫЙ ПАРАМЕТР: блокировка переходов при получении урона
        anim.SetBool("isTakingDamage", isTakingDamage);

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

        // Поворот спрайта (только если не в стане, не получаем урон и не блокируем поворот)
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

    public void TakeHit(int damage, float attackerX)
    {
        if (isInvincible)
        {
            Debug.Log("Урон проигнорирован (неуязвимость)");
            return;
        }

        if (isStunned) return;

        Debug.Log($"Герой получил урон: {damage}");

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Включаем флаг получения урона (блокирует переходы в Idle/Run)
        isTakingDamage = true;

        // Запускаем анимацию получения урона
        anim.SetTrigger("Hit");

        isStunned = true;
        stunTimer = hitStunDuration;

        float knockbackDirection = (transform.position.x - attackerX) > 0 ? 1f : -1f;
        rb.linearVelocity = new Vector2(knockbackDirection * hitKnockbackForce, 3f);

        isJumping = false;
        anim.SetBool("isJumping", false);
    }

    // НОВЫЙ МЕТОД: Вызывается из Animation Event в конце анимации hero_hit
    public void OnDamageAnimationEnd()
    {
        Debug.Log("Анимация получения урона закончилась");
        isTakingDamage = false;
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

        if (!isGrounded && (wallOnLeft || wallOnRight))
        {
            if (leftTop && horizontalInput < 0)
                Debug.Log("Застревает ГОЛОВОЙ слева!");
            if (rightTop && horizontalInput > 0)
                Debug.Log("Застревает ГОЛОВОЙ справа!");
        }
    }

    bool CheckWallAtPoint(Vector2 point, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(point, direction, wallCheckDistance, groundLayer);
        return hit.collider != null;
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
}