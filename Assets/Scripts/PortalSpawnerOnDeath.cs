using UnityEngine;

public class PortalSpawnerOnDeath : MonoBehaviour
{
    public GameObject portalPrefab;
    public float spawnDelay = 1f;              // Задержка после смерти перед спавном

    private bool hasSpawned = false;

    void Start()
    {
        // Подписываемся на событие смерти босса
        BossEnemy boss = GetComponent<BossEnemy>();
        if (boss != null)
        {
            StartCoroutine(WaitForDeath(boss));
        }
    }

    System.Collections.IEnumerator WaitForDeath(BossEnemy boss)
    {
        // Ждём, пока босс умрёт
        while (!boss.IsDead)
        {
            yield return null;
        }

        // Небольшая задержка перед спавном портала
        yield return new WaitForSeconds(spawnDelay);

        SpawnPortal();
    }

    void SpawnPortal()
    {
        if (portalPrefab == null) return;

        Vector3 portalPos = transform.position;
        portalPos.y += 1f;

        GameObject portal = Instantiate(portalPrefab, portalPos, Quaternion.identity);

        Portal portalScript = portal.GetComponent<Portal>();
        if (portalScript != null)
        {
            LevelGenerator levelGen = FindObjectOfType<LevelGenerator>();
            if (levelGen != null)
                portalScript.levelNumber = levelGen.GetCurrentLevelNumber();
        }

        Debug.Log("Портал появился после смерти босса!");
    }
}