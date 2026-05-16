using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float accelerationTime = 2f;

    private float currentSpeed = 0f;
    private bool isMoving = false;

    void Update()
    {
        if (!isMoving) return;

        if (currentSpeed < moveSpeed)
        {
            currentSpeed += (moveSpeed / accelerationTime) * Time.deltaTime;
            if (currentSpeed > moveSpeed)
                currentSpeed = moveSpeed;
        }

        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    }

    public void StartMoving()
    {
        isMoving = true;
        Debug.Log("CameraMover: движение начато");
    }

    public void StopMoving()
    {
        isMoving = false;
        currentSpeed = 0f;
        Debug.Log("CameraMover: движение остановлено");
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}