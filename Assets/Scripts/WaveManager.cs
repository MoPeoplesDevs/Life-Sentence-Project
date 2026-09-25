using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    [SerializeField] TextAsset waveJson;
    [SerializeField] GameObject hordePrefab;
    [SerializeField] GameObject heavyPrefab;
    [SerializeField] GameObject seekingPrefab;
    [SerializeField] GameObject sniperPrefab;
    [SerializeField] GameObject bossPrefab;
    [SerializeField] Transform[] spawnPoints;

    WaveConfig waveConfig;

    int currentWave = 1;
    int activeEnemies = 0;
    bool finishedSpawning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waveConfig = JsonUtility.FromJson<WaveConfig>(waveJson.text);

        SpawnWave(currentWave);
    }

    public void SpawnWave(int waveNumber)
    {
        foreach (WaveData wave in waveConfig.waves)
        {
            if (wave.waveNumber == waveNumber)
            {
                finishedSpawning = false;

                StartCoroutine(SpawnWaveRoutine(wave));
                return;
            }
        }
    }

    IEnumerator SpawnWaveRoutine(WaveData wave)
    {
        yield return new WaitForSeconds(wave.startDelay);

        foreach (SpawnGroup group in wave.spawnGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                GameObject prefabToSpawn = GetEnemyPrefab(group.enemyType);

                if (prefabToSpawn != null)
                {
                    Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

                    GameObject enemy = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

                    activeEnemies++;

                    WaveEnemyTracker tracker = enemy.AddComponent<WaveEnemyTracker>();
                    tracker.SetWaveManager(this);
                }

                yield return new WaitForSeconds(group.spawnInterval);

            }
        }

        finishedSpawning = true;

        CheckWaveComplete();

    }

    public void EnemyDestroyed()
    {
        activeEnemies--;

        CheckWaveComplete();
    }

    void CheckWaveComplete()
    {
        if (finishedSpawning && activeEnemies <= 0)
        {
            currentWave++;

            SpawnWave(currentWave);
        }
    }

    GameObject GetEnemyPrefab(string enemyType)
    {
        switch (enemyType)
        {
            case "Horde":
                return hordePrefab;

            case "Heavy":
                return heavyPrefab;

            case "Seeking":
                return seekingPrefab;

            case "Sniper":
                return sniperPrefab;

            case "Boss":
                return bossPrefab;

            default:
                Debug.LogWarning("Unknown enemy type: " + enemyType);
                return null;
        }
    }
}
