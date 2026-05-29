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

    [Header("Difficulty Multiplier")]
    [Range(0.5f, 5f)]
    public float difficultyMultiplier = 1f;     // Множитель урона и здоровья врагов (настраивается вручную)

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

    private int spawnedChunksCount = 0;          // Счётчик созданных чанков
    private GameObject lastSpawnedChunk;          // Последний созданный чанк

    [Header("Portal Settings")]
    public GameObject portalPrefab;              // Префаб портала
    public int spawnAfterChunks = 8;             // Через сколько чанков спавнить портал
    public float portalYOffset = 0f;
    public int currentLevelNumber = 1; // Смещение портала по Y

    [Header("Camera Stop Settings")]
    public float stopDistanceFromCamera = 4f;     // Когда портал на этом расстоянии от камеры - начинаем тормозить
    public float slowDownDuration = 2f;           // Длительность замедления до полной остановки

    private bool portalSpawned = false;
    private bool isSlowingDown = false;
    private GameObject spawnedPortal;
    private float originalCameraSpeed;
    private float slowDownTimer = 0f;
    private bool cameraStopped = false;
    private CameraMover cameraMover;

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

        // Находим CameraMover
        cameraMover = Camera.main.GetComponent<CameraMover>();
        if (cameraMover != null)
            originalCameraSpeed = cameraMover.GetCurrentSpeed();

        // Начинаем генерацию от текущей позиции камеры
        lastSpawnPositionX = cameraTransform.position.x;
        despawnThresholdX = lastSpawnPositionX - chunkWidth * 2;

        // Генерируем начальные участки
        GenerateInitialChunks();

        // Спавним игрока на начальном участке
        if (spawnPlayerOnStartChunk && playerTransform != null && startChunk != null)
        {
            PositionPlayerOnStartChunk();
        }
    }

    void Update()
    {
        if (cameraTransform == null) return;

        // Обработка замедления камеры у портала
        HandlePortalCameraStop();

        float cameraX = cameraTransform.position.x;

        // Проверяем, нужно ли создать новый участок впереди (только если камера не остановлена)
        if (!cameraStopped)
        {
            float furthestChunkX = GetFurthestChunkX();
            if (furthestChunkX - cameraX < chunkWidth * chunksToPreload)
            {
                SpawnNewChunk(furthestChunkX + chunkWidth, false);
            }
        }

        // Проверяем, нужно ли удалить участки позади
        RemoveOldChunks(cameraX);
    }

    void HandlePortalCameraStop()
    {
        if (cameraStopped) return;
        if (cameraMover == null) return;
        if (!portalSpawned || spawnedPortal == null) return;
        if (!cameraMover.IsMoving()) return;

        float distanceToPortal = spawnedPortal.transform.position.x - cameraMover.transform.position.x;

        // Если портал приближается к камере
        if (!isSlowingDown && distanceToPortal <= stopDistanceFromCamera && distanceToPortal > 0)
        {
            Debug.Log($"Портал на расстоянии {distanceToPortal:F2}! Начинаем замедление...");
            isSlowingDown = true;
            slowDownTimer = 0f;
            originalCameraSpeed = cameraMover.GetCurrentSpeed();
        }

        // Плавное замедление
        if (isSlowingDown)
        {
            slowDownTimer += Time.deltaTime;
            float t = Mathf.Clamp01(slowDownTimer / slowDownDuration);
            float easeT = 1f - Mathf.Pow(1f - t, 2f); // Ease Out
            float newSpeed = Mathf.Lerp(originalCameraSpeed, 0f, easeT);
            cameraMover.SetSpeed(newSpeed);

            if (t >= 1f)
            {
                isSlowingDown = false;
                cameraStopped = true;
                cameraMover.StopMoving();
                Debug.Log("Камера полностью остановлена! Игрок может войти в портал.");
            }
        }
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



    public int GetSpawnedChunksCount()
    {
        return spawnedChunksCount;
    }

    public GameObject GetLastChunk()
    {
        return lastSpawnedChunk;
    }

    public float GetChunkWidth()
    {
        return chunkWidth;
    }

    public int GetCurrentLevelNumber()
    {
        return currentLevelNumber;
    }

    public void SetCurrentLevelNumber(int levelNumber)
    {
        currentLevelNumber = levelNumber;
    }

    // Измените метод SpawnNewChunk, добавив в него счётчик:

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
            int randomIndex = Random.Range(0, chunkPrefabs.Length);
            chunkPrefab = chunkPrefabs[randomIndex];
        }

        Vector3 spawnPosition = new Vector3(xPosition + spawnOffsetX, 0, 0);
        GameObject newChunk = Instantiate(chunkPrefab, spawnPosition, Quaternion.identity);
        newChunk.transform.SetParent(transform);

        ApplyMultiplierToEnemies(newChunk);

        activeChunks.Add(newChunk);
        spawnedChunksCount++;
        lastSpawnedChunk = newChunk;

        if (isStartChunk)
        {
            startChunk = newChunk;
        }

        // ========== СПАВН ПОРТАЛА ==========
        if (portalPrefab != null && !portalSpawned && spawnedChunksCount == spawnAfterChunks)
        {
            Debug.Log($"Portal: достигнут чанк #{spawnedChunksCount}, спавним портал!");
            SpawnPortalInLastChunk();
        }
        // ==================================

        Debug.Log($"Создан участок: {chunkPrefab.name} на позиции X = {xPosition}, всего чанков: {spawnedChunksCount}");
    }

    void SpawnPortalInLastChunk()
    {
        if (portalPrefab == null)
        {
            Debug.LogWarning("LevelGenerator: portalPrefab не назначен!");
            return;
        }

        if (lastSpawnedChunk == null)
        {
            Debug.LogWarning("LevelGenerator: lastSpawnedChunk не найден!");
            return;
        }

        // Ищем точку спавна портала в чанке
        Transform portalSpawnPoint = lastSpawnedChunk.transform.Find("PortalSpawnPoint");

        Vector3 spawnPosition;
        if (portalSpawnPoint != null)
        {
            spawnPosition = portalSpawnPoint.position;
        }
        else
        {
            // Спавним в правой части чанка
            float spawnX = lastSpawnedChunk.transform.position.x + chunkWidth - 3f;
            spawnPosition = new Vector3(spawnX, portalYOffset, 0);
        }

        // Создаём портал
        spawnedPortal = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);
        spawnedPortal.transform.SetParent(lastSpawnedChunk.transform);

        // Настраиваем портал
        Portal portalScript = spawnedPortal.GetComponent<Portal>();
        if (portalScript != null)
        {
            portalScript.levelNumber = currentLevelNumber;
        }

        portalSpawned = true;
        Debug.Log($"✅ Портал спавнен в чанке #{spawnedChunksCount} на позиции X = {spawnPosition.x:F2}");
    }

    // НОВЫЙ МЕТОД: Применяет множитель ко всем врагам в чанке
    void ApplyMultiplierToEnemies(GameObject chunk)
    {
        if (difficultyMultiplier == 1f) return; // Если множитель 1, ничего не меняем

        // Находим всех врагов (MeleeEnemy и ArcherEnemy)
        MeleeEnemy[] meleeEnemies = chunk.GetComponentsInChildren<MeleeEnemy>();
        ArcherEnemy[] archerEnemies = chunk.GetComponentsInChildren<ArcherEnemy>();

        // Применяем к ближним врагам
        foreach (MeleeEnemy enemy in meleeEnemies)
        {
            int newDamage = Mathf.RoundToInt(enemy.damage * difficultyMultiplier);
            int newHealth = Mathf.RoundToInt(enemy.maxHealth * difficultyMultiplier);

            enemy.damage = newDamage;
            enemy.maxHealth = newHealth;

            // Если враг уже жив, обновляем текущее здоровье пропорционально
            if (!enemy.IsDead && enemy.CurrentHealth > 0)
            {
                float healthPercent = (float)enemy.CurrentHealth / enemy.damage;
                enemy.CurrentHealth = Mathf.RoundToInt(newHealth * healthPercent);
            }

            Debug.Log($"MeleeEnemy: урон {enemy.damage} -> {newDamage}, здоровье {enemy.maxHealth} -> {newHealth}");
        }

        // Применяем к лучникам
        foreach (ArcherEnemy enemy in archerEnemies)
        {
            int newDamage = Mathf.RoundToInt(enemy.damage * difficultyMultiplier);
            int newHealth = Mathf.RoundToInt(enemy.maxHealth * difficultyMultiplier);

            enemy.damage = newDamage;
            enemy.maxHealth = newHealth;

            if (!enemy.IsDead && enemy.CurrentHealth > 0)
            {
                float healthPercent = (float)enemy.CurrentHealth / enemy.damage;
                enemy.CurrentHealth = Mathf.RoundToInt(newHealth * healthPercent);
            }

            Debug.Log($"ArcherEnemy: урон {enemy.damage} -> {newDamage}, здоровье {enemy.maxHealth} -> {newHealth}");
        }

        // Также обновляем урон в анимациях (если есть)
        // Note: damage уже обновлён, анимации используют тот же damage
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

    // Метод для изменения множителя в реальном времени
    public void SetDifficultyMultiplier(float newMultiplier)
    {
        difficultyMultiplier = Mathf.Clamp(newMultiplier, 0.5f, 5f);
        Debug.Log($"Уровень сложности изменён: множитель = {difficultyMultiplier}x");
    }

    // Метод для получения текущего множителя
    public float GetDifficultyMultiplier()
    {
        return difficultyMultiplier;
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