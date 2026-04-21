using UnityEngine;

public class HeroMovementSimple : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;              // Базовая сила прыжка
    public float minJumpForce = 8f;            // Минимальная сила при быстром отпускании
    public float maxHoldTime = 0.25f;           // Максимальное время удержания

    [Header("Gravity Settings")]
    public float fallGravityMultiplier = 2f;    // Ускорение падения

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private float horizontalInput;
    private bool isGrounded;

    private bool isJumping = false;
    private float jumpHoldTimer = 0f;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // Параметры анимаций
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("verticalSpeed", rb.linearVelocity.y);

        // Начало прыжка - МГНОВЕННЫЙ рывок вверх
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            jumpHoldTimer = 0f;
            // Мгновенно даём полную силу прыжка
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetBool("isJumping", true);
        }

        // Удержание прыжка - НЕ УВЕЛИЧИВАЕМ скорость, а поддерживаем её
        // Если скорость падает ниже желаемой - немного добавляем, но без "разгона"
        if (Input.GetButton("Jump") && isJumping && jumpHoldTimer < maxHoldTime)
        {
            jumpHoldTimer += Time.deltaTime;

            // Если скорость упала ниже 80% от начальной - чуть поднимаем
            // Но не даём разгоняться выше начальной скорости
            if (rb.linearVelocity.y < jumpForce * 0.8f && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.85f);
            }
        }

        // Отпускание прыжка - резкое падение
        if (Input.GetButtonUp("Jump") && isJumping)
        {
            isJumping = false;
            // Если летим выше минимального - срезаем до минимума
            if (rb.linearVelocity.y > minJumpForce)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, minJumpForce);
            }
        }

        // Приземление
        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            isJumping = false;
            anim.SetBool("isJumping", false);
        }

        // Поворот спрайта
        if (sprite != null)
        {
            if (horizontalInput > 0)
                sprite.flipX = false;
            else if (horizontalInput < 0)
                sprite.flipX = true;
        }
    }

    void FixedUpdate()
    {
        // Перемещение
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Усиленная гравитация при падении для быстрого падения
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
    }
}