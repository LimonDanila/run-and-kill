using UnityEngine;
using System.Collections.Generic;

public class ObjectMover : MonoBehaviour
{
    [Header("References")]
    public CameraMover cameraMover;         // Ссылка на скрипт движения камеры

    [Header("Objects to Move")]
    public List<GameObject> objectsToMove = new List<GameObject>();  // Объекты для движения
    public bool autoFindObjects = true;      // Автоматически найти объекты по тегу

    [Header("Movement Settings")]
    public bool moveWithCamera = true;       // Двигаться вместе с камерой
    public Vector3 offset = Vector3.zero;    // Дополнительное смещение

    private Vector3 initialPosition;
    private float lastCameraX;

    void Start()
    {
        // Находим камеру, если не указана
        if (cameraMover == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                cameraMover = cam.GetComponent<CameraMover>();
        }

        // Автоматически находим объекты по тегам
        if (autoFindObjects)
        {
            FindObjectsByTags();
        }

        lastCameraX = GetCameraX();
        initialPosition = transform.position;
    }

    void Update()
    {
        if (!moveWithCamera || cameraMover == null) return;

        float currentCameraX = GetCameraX();
        float cameraDelta = currentCameraX - lastCameraX;

        // Двигаем все объекты
        foreach (GameObject obj in objectsToMove)
        {
            if (obj != null)
            {
                obj.transform.Translate(Vector3.right * cameraDelta);
            }
        }

        // Двигаем сам объект (если это не камера)
        if (this.gameObject != cameraMover.gameObject)
        {
            transform.Translate(Vector3.right * cameraDelta);
        }

        lastCameraX = currentCameraX;
    }

    float GetCameraX()
    {
        if (cameraMover != null)
            return cameraMover.transform.position.x;

        Camera cam = Camera.main;
        if (cam != null)
            return cam.transform.position.x;

        return 0f;
    }

    void FindObjectsByTags()
    {
        // Ищем объекты по тегам
        GameObject[] background = GameObject.FindGameObjectsWithTag("Background");
        GameObject[] spikes = GameObject.FindGameObjectsWithTag("Spikes");

        objectsToMove.Clear();
        objectsToMove.AddRange(background);
        objectsToMove.AddRange(spikes);

        // Убираем дубликаты
        HashSet<GameObject> uniqueObjects = new HashSet<GameObject>(objectsToMove);
        objectsToMove.Clear();
        objectsToMove.AddRange(uniqueObjects);

        Debug.Log($"Найдено {objectsToMove.Count} объектов для движения");
    }

    public void AddObject(GameObject obj)
    {
        if (!objectsToMove.Contains(obj))
            objectsToMove.Add(obj);
    }

    public void RemoveObject(GameObject obj)
    {
        objectsToMove.Remove(obj);
    }
}