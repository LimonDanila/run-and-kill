using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallingSpikes : MonoBehaviour
{
    [Header("Spike Settings")]
    public GameObject spikePrefab;              // Префаб шипа
    public float spawnHeight = 8f;              // Высота спавна (над экраном)
    public float spikeFallSpeed = 8f;           // Скорость падения шипа
    public float spikeWarningDuration = 0.8f;   // Длительность предупреждения

    [Header("Spawn Settings")]
    public int minSpikesPerWave = 1;
    public int maxSpikesPerWave = 3;
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 5f;
    public float spawnRangeX = 10f;

    [Header("Warning Effect")]
    public GameObject warningPrefab;

    [Header("References")]
    public Transform player;
    public Camera mainCamera;

    private bool isSpawning = true;
    private float nextSpawnTime;
    private List<GameObject> activeSpikes = new List<GameObject>();
    private HeroMove heroMove;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                heroMove = playerObj.GetComponent<HeroMove>();
            }
        }

        SetRandomSpawnInterval();
    }

    void Update()
    {
        if (!isSpawning) return;

        if (Time.time >= nextSpawnTime)
        {
            StartCoroutine(SpawnSpikeWave());
            SetRandomSpawnInterval();
        }
    }

    void SetRandomSpawnInterval()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    IEnumerator SpawnSpikeWave()
    {
        int spikeCount = Random.Range(minSpikesPerWave, maxSpikesPerWave + 1);
        Debug.Log($"Падающие шипы: волна из {spikeCount} шипов");

        List<Vector3> spawnPositions = new List<Vector3>();

        for (int i = 0; i < spikeCount; i++)
        {
            float randomX = GetRandomXPosition();
            Vector3 spawnPos = new Vector3(randomX, spawnHeight, 0);
            spawnPositions.Add(spawnPos);

            ShowWarningAtPosition(spawnPos);
        }

        yield return new WaitForSeconds(spikeWarningDuration);

        foreach (Vector3 pos in spawnPositions)
        {
            SpawnSpikeAtPosition(pos);
        }
    }

    float GetRandomXPosition()
    {
        if (mainCamera != null)
        {
            float cameraLeft = mainCamera.transform.position.x - spawnRangeX;
            float cameraRight = mainCamera.transform.position.x + spawnRangeX;
            return Random.Range(cameraLeft, cameraRight);
        }
        return Random.Range(-spawnRangeX, spawnRangeX);
    }

    void ShowWarningAtPosition(Vector3 position)
    {
        if (warningPrefab != null)
        {
            GameObject warning = Instantiate(warningPrefab, position, Quaternion.identity);
            Destroy(warning, spikeWarningDuration);
        }
        else
        {
            CreateWarningSprite(position);
        }
    }

    void CreateWarningSprite(Vector3 position)
    {
        GameObject warning = new GameObject("SpikeWarning");
        warning.transform.position = position;

        SpriteRenderer sr = warning.AddComponent<SpriteRenderer>();

        Texture2D texture = new Texture2D(64, 64);
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float dx = x - 32;
                float dy = y - 32;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 30)
                {
                    float alpha = 1f - (dist / 30f);
                    texture.SetPixel(x, y, new Color(1f, 0.2f, 0.2f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        sr.sprite = sprite;
        sr.sortingOrder = 200;

        StartCoroutine(PulseWarning(warning));
        Destroy(warning, spikeWarningDuration);
    }

    IEnumerator PulseWarning(GameObject warning)
    {
        SpriteRenderer sr = warning.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        float duration = spikeWarningDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.3f;
            warning.transform.localScale = new Vector3(scale, scale, 1f);

            if (sr != null)
            {
                float alpha = 0.7f - t * 0.5f;
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            yield return null;
        }
    }

    void SpawnSpikeAtPosition(Vector3 position)
    {
        if (spikePrefab == null)
        {
            Debug.LogError("FallingSpikes: spikePrefab не назначен!");
            return;
        }

        GameObject spike = Instantiate(spikePrefab, position, Quaternion.identity);
        activeSpikes.Add(spike);

        FallingSpikeProjectile spikeScript = spike.GetComponent<FallingSpikeProjectile>();
        if (spikeScript == null)
            spikeScript = spike.AddComponent<FallingSpikeProjectile>();

        spikeScript.Initialize(spikeFallSpeed, heroMove);
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void StartSpawning()
    {
        isSpawning = true;
        SetRandomSpawnInterval();
    }

    public void ClearAllSpikes()
    {
        foreach (GameObject spike in activeSpikes)
        {
            if (spike != null)
                Destroy(spike);
        }
        activeSpikes.Clear();
    }
}