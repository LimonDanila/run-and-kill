using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float initialMoveSpeed = 3f;      // Начальная скорость
    public float maxMoveSpeed = 10f;         // Максимальная скорость
    public float accelerationTime = 2f;      // Время разгона до максимальной скорости

    [Header("Progressive Acceleration")]
    public bool progressiveAcceleration = true;  // Постепенное ускорение со временем
    public float timeToMaxSpeed = 30f;           // Время до достижения максимальной скорости (сек)
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Кривая ускорения

    private float currentSpeed = 0f;
    private float baseSpeed = 0f;
    private bool isMoving = false;
    private float gameTimer = 0f;            // Таймер с начала движения

    void Update()
    {
        if (!isMoving) return;

        // Обновляем таймер
        gameTimer += Time.deltaTime;

        // Расчёт текущей максимальной скорости (увеличивается со временем)
        float currentMaxSpeed;

        if (progressiveAcceleration)
        {
            // Рассчитываем прогресс времени (от 0 до 1)
            float timeProgress = Mathf.Clamp01(gameTimer / timeToMaxSpeed);

            // Получаем множитель из кривой
            float speedMultiplier = speedCurve.Evaluate(timeProgress);

            // Текущая максимальная скорость
            currentMaxSpeed = Mathf.Lerp(initialMoveSpeed, maxMoveSpeed, speedMultiplier);
        }
        else
        {
            currentMaxSpeed = maxMoveSpeed;
        }

        // Плавное ускорение к текущей максимальной скорости
        if (currentSpeed < currentMaxSpeed)
        {
            currentSpeed += (currentMaxSpeed / accelerationTime) * Time.deltaTime;
            if (currentSpeed > currentMaxSpeed)
                currentSpeed = currentMaxSpeed;
        }

        // Движение камеры
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);

        // Отладка (опционально)
        if (Time.frameCount % 120 == 0)
            Debug.Log($"CameraMover: скорость = {currentSpeed:F2}, таймер = {gameTimer:F1}");
    }

    public void StartMoving()
    {
        isMoving = true;
        gameTimer = 0f;
        currentSpeed = initialMoveSpeed;
        Debug.Log($"CameraMover: движение начато со скоростью {initialMoveSpeed}");
    }

    public void StopMoving()
    {
        isMoving = false;
        currentSpeed = 0f;
        gameTimer = 0f;
        Debug.Log("CameraMover: движение остановлено");
    }

    public void ResetSpeed()
    {
        currentSpeed = initialMoveSpeed;
        gameTimer = 0f;
        Debug.Log($"CameraMover: скорость сброшена до {initialMoveSpeed}");
    }

    public void SetSpeed(float speed)
    {
        initialMoveSpeed = speed;
        if (isMoving && speed > currentSpeed)
        {
            currentSpeed = speed;
        }
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetGameTimer()
    {
        return gameTimer;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}