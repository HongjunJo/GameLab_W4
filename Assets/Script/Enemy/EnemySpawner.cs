using UnityEngine;

using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{

        [Header("Spawn Settings")]
        [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
        [SerializeField] private List<Vector3> spawnPositions = new List<Vector3>();
        [SerializeField] private int maxEnemyCount = 5;
        [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnDuration = 20f;
    [SerializeField] private bool useSpawnDuration = true; // true: duration 사용, false: 무한 스폰

        [Header("Spawn Options")]
        [SerializeField] private bool spawnPointRandom = false; // true: 랜덤, false: 순서대로
        [SerializeField] private bool enemyTypeRandom = false; // true: 랜덤, false: 순서대로
        private int spawnPointIndex = 0;
        private int enemyTypeIndex = 0;

    [Header("Debug")] 
    [SerializeField] private bool showGizmos = true;

    [Header("Spawn Control")]
    [SerializeField] private bool spawning = false; // 인스펙터에서 on/off

    private float spawnTimer = 0f;
    private float spawnTimeElapsed = 0f;
    private int currentEnemyCount = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool allOreMined = false;
    // 외부에서 호출: 플레이어가 광석을 때릴 때
    public void StartSpawning()
    {
        if (!spawning && !allOreMined)
        {
            spawning = true;
            spawnTimeElapsed = 0f;
        }
    }

    // 외부에서 호출: 광석이 모두 채굴되었을 때
    public void StopSpawning()
    {
        spawning = false;
        allOreMined = true;
    }

    private void Update()
    {
        if (!spawning || allOreMined) return;

        if (useSpawnDuration)
        {
            spawnTimeElapsed += Time.deltaTime;
            if (spawnTimeElapsed >= spawnDuration)
            {
                spawning = false;
                return;
            }
        }

        spawnTimer += Time.deltaTime;

        // 최대 적 수 유지
        spawnedEnemies.RemoveAll(e => e == null);
        currentEnemyCount = spawnedEnemies.Count;
        if (currentEnemyCount >= maxEnemyCount) return;

        // 스폰
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPositions.Count == 0 || enemyPrefabs.Count == 0) return;

        // 스폰 위치 선택
        int posIdx = spawnPointRandom ? Random.Range(0, spawnPositions.Count) : spawnPointIndex;
        Vector3 spawnPos = spawnPositions[posIdx];
        if (!spawnPointRandom)
            spawnPointIndex = (spawnPointIndex + 1) % spawnPositions.Count;

        // 적 종류 선택
        int enemyIdx = enemyTypeRandom ? Random.Range(0, enemyPrefabs.Count) : enemyTypeIndex;
        GameObject prefab = enemyPrefabs[enemyIdx];
        if (!enemyTypeRandom)
            enemyTypeIndex = (enemyTypeIndex + 1) % enemyPrefabs.Count;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        spawnedEnemies.Add(enemy);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || spawnPositions == null) return;
        Gizmos.color = Color.red;
        foreach (var pos in spawnPositions)
        {
            Gizmos.DrawSphere(pos, 0.3f);
        }
    }
}
