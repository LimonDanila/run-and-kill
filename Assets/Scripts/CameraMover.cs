using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;           // Скорость движения камеры вправо
    public float accelerationTime = 3f;    // Время разгона до максимальной скорости
    public bool autoStart = true;          // Начинать ли движение сразу

    private float currentSpeed = 0f;
    private bool isMoving = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (autoStart)
        {
            StartMoving();
        }
    }

    void Update()
    {
        if (!isMoving) return;

        // Плавный разгон
        if (currentSpeed < moveSpeed)
        {
            currentSpeed += (moveSpeed / accelerationTime) * Time.deltaTime;
            if (currentSpeed > moveSpeed)
                currentSpeed = moveSpeed;
        }

        // Движение камеры вправо
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    }

    public void StartMoving()
    {
        isMoving = true;
        currentSpeed = 0f;
        Debug.Log("Камера начала движение");
    }

    public void StopMoving()
    {
        isMoving = false;
        Debug.Log("Камера остановилась");
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}