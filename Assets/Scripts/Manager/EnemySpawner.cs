using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Boss Prefab")]
    [SerializeField] private GameObject finalBossPrefab;
    [SerializeField] private GameObject midBossPrefab;

    [SerializeField] private float safeRadius = 5f;   // khoảng cách an toàn quanh player
    [SerializeField] private Transform player;

    [Header("Danh sách điểm spawn cố định")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private int enemiesPerBatch = 2;   // số quái mỗi lần spawn
    [SerializeField] private float spawnInterval = 3f; // thời gian giữa các batch
    private int enemiesToSpawn;
    private int spawnedEnemies;
    private Coroutine spawnRoutine;
    private int currentWave; // 👈 để biết wave hiện tại

    // Gọi khi bắt đầu wave
    public void StartSpawning(int totalEnemies, int wave)
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
        enemiesToSpawn = totalEnemies;
        spawnedEnemies = 0;
        currentWave = wave;
        spawnRoutine = StartCoroutine(SpawnEnemiesOverTime());
    }

    public void RetryWave()
    {
        Debug.Log("🔄 RetryWave: Xóa toàn bộ enemy và spawn lại!");

        // Xóa toàn bộ enemy hiện có
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in allEnemies)
        {
            Destroy(e.gameObject);
        }

        // Dừng coroutine spawn cũ
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        spawnedEnemies = 0;

        // Spawn lại wave hiện tại
        spawnRoutine = StartCoroutine(SpawnEnemiesOverTime());
    }


    private IEnumerator SpawnEnemiesOverTime()
    {
        while (spawnedEnemies < enemiesToSpawn)
        {
            // Nếu game đang pause (upgrade), chờ đến khi resume
            while (Time.timeScale == 0f)
                yield return null;

            int count = Mathf.Min(enemiesPerBatch, enemiesToSpawn - spawnedEnemies);

            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = GetRandomSpawnPosition();
                GameObject prefab = GetEnemyPrefabByWave(currentWave);
                if (prefab == null) continue;

                GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

                spawnedEnemies++;
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        spawnRoutine = null;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Transform chosenPoint;
        int loopSafe = 0;

        do
        {
            chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            loopSafe++;
        }
        while (Vector2.Distance(chosenPoint.position, player.position) < safeRadius && loopSafe < 50);

        return chosenPoint.position;
    }

    // 👇 Chọn enemy phù hợp dựa trên wave
    private GameObject GetEnemyPrefabByWave(int wave)
    {
        int maxIndex = 0;

        if (wave >= 20)
            maxIndex = 4;
        else if (wave >= 15)
            maxIndex = 3;
        else if (wave >= 10)
            maxIndex = 2;
        else if (wave >= 5)
            maxIndex = 1;
        else
            maxIndex = 0;

        return enemyPrefabs[Random.Range(0, maxIndex + 1)];
    }

    public void SpawnBoss(bool isFinalBoss)
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject bossPrefab = isFinalBoss ? finalBossPrefab : midBossPrefab;
        GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        Enemy_Boss boss = bossObj.GetComponent<Enemy_Boss>();
        if (boss != null)
        {
            boss.SetIsFinalBoss(isFinalBoss);
        }
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

}
