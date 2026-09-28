using System;
using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] TextAsset waveJson;
    [SerializeField] GameObject hordePrefab;
    [SerializeField] GameObject heavyPrefab;
    [SerializeField] GameObject seekingPrefab;
    [SerializeField] GameObject sniperPrefab;
    [SerializeField] GameObject bossPrefab;
    [SerializeField] Transform[] spawnPoints;

    WaveConfig waveConfig;

    public int activeEnemies = 0;
    private bool finishedSpawning = false;

    public static event Action OnWaveFinished;
    public static event Action OnWaveStarted;
    public static event Action<int> OnEnemyKilled;
    public static event Action<int> OnEnemySpawned;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        waveConfig = JsonUtility.FromJson<WaveConfig>(waveJson.text);
    }

    // Used to kill off all enemies in the level!
    public void WipeLevel()
    {
        GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in objects)
        {
            if (obj.name.Contains("(Clone)"))
                Destroy(obj);
        }
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
        OnWaveStarted?.Invoke();
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
                    Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                    GameObject enemy = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
                    activeEnemies++;
                    WaveEnemyTracker tracker = enemy.AddComponent<WaveEnemyTracker>();
                    tracker.SetWaveManager(this);

                    OnEnemySpawned?.Invoke(activeEnemies);
                }

                yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        finishedSpawning = true;
    }

    public void EnemyDestroyed()
    {
        activeEnemies--;
        OnEnemyKilled?.Invoke(activeEnemies);
        CheckWaveComplete();
    }

    void CheckWaveComplete()
    {
        if (finishedSpawning && activeEnemies <= 0)
            OnWaveFinished?.Invoke();
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
