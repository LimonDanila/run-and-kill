using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    public GameObject[] chunkPrefabs;           // Массив префабов участков (Grid с Tilemap и врагами)
    public GameObject startChunkPrefab;         // Начальный участок, на котором спавнится герой
    public float chunkWidth = 20f;              // Ширина одного участка
    public int chunksToPreload = 3;             // Сколько участков вперёд создавать
    public int chunksToKeep = 4;                // Сколько участков хранить в памяти (включая текущий)

    [Header("References")]
    public Transform cameraTransform;           // Ссылка на камеру
    public Transform playerTransform;           // Ссылка на игрока (опционально)

    [Header("Spawn Settings")]
    public float spawnOffsetX = 0f;             // Смещение спавна по X
    public bool spawnPlayerOnStartChunk = true; // Спавнить игрока на начальном участке

    private List<GameObject> activeChunks = new List<GameObject>();
    private float lastSpawnPositionX;
    private float despawnThresholdX;
    private GameObject startChunk;               // Ссылка на созданный начальный участок
    private bool startChunkSpawned = false;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("LevelGenerator: Не назначены префабы участков!");
            return;
        }

        if (startChunkPrefab == null)
        {
            Debug.LogWarning("LevelGenerator: Не назначен начальный участок! Будет использован случайный.");
        }

        // Начинаем генерацию от текущей позиции камеры
        lastSpawnPositionX = cameraTransform.position.x;
        despawnThresholdX = lastSpawnPositionX - chunkWidth * 2;

        // Генерируем начальные участки
        GenerateInitialChunks();

        // Спавним игрока на начальном участке
        //if (spawnPlayerOnStartChunk && playerTransform != null && startChunk != null)
        //{
        //    PositionPlayerOnStartChunk();
        //}
    }

    void Update()
    {
        if (cameraTransform == null) return;

        float cameraX = cameraTransform.position.x;

        // Проверяем, нужно ли создать новый участок впереди
        float furthestChunkX = GetFurthestChunkX();
        if (furthestChunkX - cameraX < chunkWidth * chunksToPreload)
        {
            SpawnNewChunk(furthestChunkX + chunkWidth, false);
        }

        // Проверяем, нужно ли удалить участки позади
        RemoveOldChunks(cameraX);
    }

    void GenerateInitialChunks()
    {
        float startX = lastSpawnPositionX - chunkWidth;

        // Определяем позицию начального участка (там, где камера)
        float startChunkX = Mathf.Floor(lastSpawnPositionX / chunkWidth) * chunkWidth;

        for (int i = -chunksToPreload; i <= chunksToPreload; i++)
        {
            float chunkX = startX + i * chunkWidth;
            bool isStartChunk = (Mathf.Abs(chunkX - startChunkX) < 0.1f);

            SpawnNewChunk(chunkX, isStartChunk);
        }
    }

    void SpawnNewChunk(float xPosition, bool isStartChunk)
    {
        GameObject chunkPrefab;

        if (isStartChunk && startChunkPrefab != null && !startChunkSpawned)
        {
            chunkPrefab = startChunkPrefab;
            startChunkSpawned = true;
            Debug.Log($"Создан НАЧАЛЬНЫЙ участок на позиции X = {xPosition}");
        }
        else
        {
            // Выбираем случайный префаб
            int randomIndex = Random.Range(0, chunkPrefabs.Length);
            chunkPrefab = chunkPrefabs[randomIndex];
        }

        // Создаём участок
        Vector3 spawnPosition = new Vector3(xPosition + spawnOffsetX, 0, 0);
        GameObject newChunk = Instantiate(chunkPrefab, spawnPosition, Quaternion.identity);
        newChunk.transform.SetParent(transform);

        // Добавляем компонент для управления чанком (если нет)

        activeChunks.Add(newChunk);

        if (isStartChunk)
        {
            startChunk = newChunk;
        }

        Debug.Log($"Создан участок: {chunkPrefab.name} на позиции X = {xPosition} ({(isStartChunk ? "НАЧАЛЬНЫЙ" : "случайный")})");
    }

    void RemoveOldChunks(float cameraX)
    {
        float despawnX = cameraX - chunkWidth * chunksToKeep;

        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = activeChunks[i];
            if (chunk == null)
            {
                activeChunks.RemoveAt(i);
                continue;
            }

            float chunkX = chunk.transform.position.x;

            // Никогда не удаляем начальный участок
            if (chunk == startChunk)
                continue;

            // Если участок слишком далеко позади камеры
            if (chunkX + chunkWidth < despawnX)
            {
                activeChunks.RemoveAt(i);
                Destroy(chunk);
                Debug.Log($"Удалён участок на позиции X = {chunkX}");
            }
        }
    }

    void PositionPlayerOnStartChunk()
    {
        if (startChunk == null)
        {
            Debug.LogWarning("LevelGenerator: Начальный участок не найден для спавна игрока!");
            return;
        }

        // Ищем точку спавна в начальном чанке
        Transform spawnPoint = startChunk.transform.Find("SpawnPoint");

        if (spawnPoint == null)
        {
            // Если нет специальной точки, спавним в центре чанка
            Vector3 spawnPosition = startChunk.transform.position;
            spawnPosition.y += 2f; // Небольшое смещение вверх
            playerTransform.position = spawnPosition;
            Debug.Log($"Игрок спавнен в центре начального участка: {spawnPosition}");
        }
        else
        {
            playerTransform.position = spawnPoint.position;
            Debug.Log($"Игрок спавнен на точке SpawnPoint начального участка: {spawnPoint.position}");
        }

        // Обновляем позицию камеры, если нужно
        if (cameraTransform != null && cameraTransform != playerTransform)
        {
            Vector3 cameraPos = cameraTransform.position;
            cameraPos.x = playerTransform.position.x;
            cameraTransform.position = cameraPos;
        }
    }

    float GetFurthestChunkX()
    {
        float furthest = -Mathf.Infinity;

        foreach (GameObject chunk in activeChunks)
        {
            if (chunk != null && chunk.transform.position.x > furthest)
                furthest = chunk.transform.position.x;
        }

        if (furthest == -Mathf.Infinity)
            furthest = lastSpawnPositionX;

        return furthest;
    }

    public void ClearAllChunks()
    {
        foreach (GameObject chunk in activeChunks)
        {
            if (chunk != null && chunk != startChunk)
                Destroy(chunk);
        }
        activeChunks.Clear();

        if (startChunk != null)
        {
            activeChunks.Add(startChunk);
        }
    }

    public GameObject GetStartChunk()
    {
        return startChunk;
    }

    // Визуализация в редакторе
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 cameraPos = cameraTransform.position;
            Gizmos.DrawWireCube(new Vector3(cameraPos.x, 0, 0), new Vector3(chunkWidth, 5, 0));

            Gizmos.color = Color.red;
            float despawnX = cameraPos.x - chunkWidth * chunksToKeep;
            Gizmos.DrawLine(new Vector3(despawnX, -3, 0), new Vector3(despawnX, 3, 0));
        }

        if (startChunkPrefab != null)
        {
            Gizmos.color = Color.green;
            // Визуализация позиции стартового чанка (только в редакторе, не в игре)
        }
    }
}