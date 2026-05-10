using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;           // Скорость движения
    public float patrolDistance = 3f;      // Расстояние патрулирования от начальной точки

    [Header("Detection Settings")]
    public float detectionRange = 5f;      // Дальность обнаружения игрока

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.3f;  // Дистанция проверки стены

    [Header("Edge Check")]
    public float edgeCheckDistance = 0.5f;  // Дистанция проверки ямы (от ног)
    public Transform edgeCheckPoint;        // Точка проверки ямы

    [Header("Hero Check")]
    public Transform player;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    private float startX;                   // Начальная X позиция
    private float patrolDirection = 1f;     // Направление патрулирования (1 - вправо, -1 - влево)
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isNearEdge;                // Яма впереди
    private bool facingRight = true;        // Куда смотрит скелет (true - вправо)
    private float currentDirection = 1f;    // Текущее направление движения

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();

        startX = transform.position.x;

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
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (!isGrounded)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        CheckWall();
        CheckEdge();

        float direction = GetMovementDirection();
        currentDirection = direction;  // Запоминаем текущее направление

        // Движение
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // Поворот спрайта в зависимости от ситуации
        UpdateSpriteDirection();
    }

    float GetMovementDirection()
    {
        // Если видим игрока - двигаемся к нему
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

        if (!isNearEdge)
        {
            Vector3 furtherPos = transform.position + new Vector3(checkDirection * 0.5f, -0.5f, 0);
            RaycastHit2D furtherHit = Physics2D.Raycast(furtherPos, Vector2.down, 0.2f, groundLayer);
            if (furtherHit.collider == null)
            {
                isNearEdge = true;
            }
        }
    }

    // НОВЫЙ МЕТОД: Обновление направления спрайта
    void UpdateSpriteDirection()
    {
        if (sprite == null) return;

        bool shouldFaceRight;

        // Если видим игрока - смотрим на игрока
        if (IsPlayerDetected() && player != null)
        {
            shouldFaceRight = player.position.x > transform.position.x;
        }
        else
        {
            // Если не видим игрока - смотрим по направлению движения
            if (currentDirection > 0)
                shouldFaceRight = true;
            else if (currentDirection < 0)
                shouldFaceRight = false;
            else
                return; // Не двигаемся - не меняем направление
        }

        // Меняем направление только если нужно
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            sprite.flipX = !facingRight;
        }
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f && isGrounded;

        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isGrounded", isGrounded);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

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