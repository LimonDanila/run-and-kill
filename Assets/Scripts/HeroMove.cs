using UnityEngine;

public class HeroMovementSimple : MonoBehaviour
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
    public float wallCheckHeightOffset = 0.5f;  // Высота проверки стены (от ног)

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
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

    // Переменные для стен
    private bool wallOnLeft;
    private bool wallOnRight;

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

        // Проверка стен с нескольких точек
        CheckWalls();

        // Определяем падение
        isFalling = !isGrounded && rb.linearVelocity.y < -0.1f;

        // Обработка justLanded с таймером
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

        // Параметры анимаций
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("verticalSpeed", rb.linearVelocity.y);
        anim.SetBool("isFalling", isFalling);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("justLanded", justLanded);

        // Начало прыжка
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            jumpHoldTimer = 0f;
            justLanded = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetBool("isJumping", true);
        }

        // Удержание прыжка
        if (Input.GetButton("Jump") && isJumping && jumpHoldTimer < maxHoldTime)
        {
            jumpHoldTimer += Time.deltaTime;

            if (rb.linearVelocity.y < jumpForce * 0.8f && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.85f);
            }
        }

        // Отпускание прыжка
        if (Input.GetButtonUp("Jump") && isJumping)
        {
            isJumping = false;
            anim.SetBool("isJumping", false);

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

    // НОВЫЙ МЕТОД: Проверка стен из нескольких точек
    void CheckWalls()
    {
        // Получаем размеры коллайдера (или спрайта)
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        float colliderHeight = boxCollider != null ? boxCollider.size.y : 1f;

        // Верхняя точка проверки (голова)
        Vector2 topPoint = new Vector2(transform.position.x, transform.position.y + colliderHeight / 2f);
        // Нижняя точка проверки (ноги)
        Vector2 bottomPoint = new Vector2(transform.position.x, transform.position.y - colliderHeight / 2f);
        // Средняя точка (пояс)
        Vector2 middlePoint = transform.position;

        // Проверяем стены из ТРЁХ точек
        bool leftTop = CheckWallAtPoint(topPoint, Vector2.left);
        bool leftMiddle = CheckWallAtPoint(middlePoint, Vector2.left);
        bool leftBottom = CheckWallAtPoint(bottomPoint, Vector2.left);

        bool rightTop = CheckWallAtPoint(topPoint, Vector2.right);
        bool rightMiddle = CheckWallAtPoint(middlePoint, Vector2.right);
        bool rightBottom = CheckWallAtPoint(bottomPoint, Vector2.right);

        // Если ХОТЯ БЫ ОДНА точка касается стены - считаем, что стена есть
        wallOnLeft = leftTop || leftMiddle || leftBottom;
        wallOnRight = rightTop || rightMiddle || rightBottom;

        // Для отладки: показываем, где именно застревает
        if (!isGrounded && (wallOnLeft || wallOnRight))
        {
            if (leftTop && horizontalInput < 0)
                Debug.Log("Застревает ГОЛОВОЙ слева!");
            if (rightTop && horizontalInput > 0)
                Debug.Log("Застревает ГОЛОВОЙ справа!");
        }
    }

    // Вспомогательный метод для проверки в одной точке
    bool CheckWallAtPoint(Vector2 point, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(point, direction, wallCheckDistance, groundLayer);
        return hit.collider != null;
    }

    void FixedUpdate()
    {
        // Логика движения с учётом стен
        float horizontalMovement = horizontalInput * moveSpeed;

        // Если персонаж НЕ на земле (в воздухе)
        if (!isGrounded)
        {
            // Блокируем движение в сторону стены
            if (horizontalInput < 0 && wallOnLeft)
            {
                horizontalMovement = 0;
            }
            else if (horizontalInput > 0 && wallOnRight)
            {
                horizontalMovement = 0;
            }
        }

        // Применяем движение
        rb.linearVelocity = new Vector2(horizontalMovement, rb.linearVelocity.y);

        // Усиленная гравитация при падении
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = originalGravity * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = originalGravity;
        }
    }

    // Визуализация для отладки
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        // Получаем размеры коллайдера
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            float colliderHeight = boxCollider.size.y;

            // Точки проверки
            Vector3 topPoint = new Vector3(transform.position.x, transform.position.y + colliderHeight / 2f, 0);
            Vector3 middlePoint = transform.position;
            Vector3 bottomPoint = new Vector3(transform.position.x, transform.position.y - colliderHeight / 2f, 0);

            // Рисуем лучи для отладки
            Gizmos.color = Color.blue;

            // Верхние лучи
            Gizmos.DrawRay(topPoint, Vector3.left * wallCheckDistance);
            Gizmos.DrawRay(topPoint, Vector3.right * wallCheckDistance);

            // Средние лучи
            Gizmos.DrawRay(middlePoint, Vector3.left * wallCheckDistance);
            Gizmos.DrawRay(middlePoint, Vector3.right * wallCheckDistance);

            // Нижние лучи
            Gizmos.DrawRay(bottomPoint, Vector3.left * wallCheckDistance);
            Gizmos.DrawRay(bottomPoint, Vector3.right * wallCheckDistance);

            // Рисуем точки
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(topPoint, 0.05f);
            Gizmos.DrawWireSphere(middlePoint, 0.05f);
            Gizmos.DrawWireSphere(bottomPoint, 0.05f);
        }
    }
}