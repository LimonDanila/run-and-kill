using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("References")]
    public CameraMover cameraMover;

    [Header("Parallax Settings")]
    public float parallaxFactor = 0.5f;     // Множитель параллакса (0.5 = двигается в 2 раза медленнее камеры)

    private Vector3 lastCameraPosition;
    private Vector3 initialPosition;

    void Start()
    {
        if (cameraMover == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                cameraMover = cam.GetComponent<CameraMover>();
        }

        initialPosition = transform.position;
        lastCameraPosition = GetCameraPosition();
    }

    void Update()
    {
        if (cameraMover == null) return;

        Vector3 currentCameraPos = GetCameraPosition();
        Vector3 cameraDelta = currentCameraPos - lastCameraPosition;

        // Движение с параллаксом
        Vector3 movement = new Vector3(cameraDelta.x * parallaxFactor, 0, 0);
        transform.Translate(movement);

        lastCameraPosition = currentCameraPos;
    }

    Vector3 GetCameraPosition()
    {
        if (cameraMover != null)
            return cameraMover.transform.position;

        Camera cam = Camera.main;
        if (cam != null)
            return cam.transform.position;

        return Vector3.zero;
    }
}