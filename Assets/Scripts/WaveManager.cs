using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] GameObject hordePrefab;
    [SerializeField] GameObject heavyPrefab;
    [SerializeField] GameObject seekingPrefab;
    [SerializeField] GameObject sniperPrefab;
    [SerializeField] GameObject bossPrefab;
    [SerializeField] Transform enemySpawns;

    private List<WaveData> waves;
    private bool finishedSpawning = false;
    public int activeEnemies {get; private set;} = 0;
    public bool isInitialized {get; private set;} = false;
    private List<Transform> spawnPoints = new List<Transform>();

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

        // Grab spawns
        foreach (Transform point in enemySpawns)
            spawnPoints.Add(point);
    }

    public void Initialize(LevelData currentLevel)
    {
        waves = currentLevel.waves;
        isInitialized = true;
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
        finishedSpawning = false;
        
        WaveData wave = waves[waveNumber - 1];
        StartCoroutine(SpawnWaveRoutine(wave));

        OnWaveStarted?.Invoke();
    }

    IEnumerator SpawnWaveRoutine(WaveData wave)
    {
        bool CanContinue()
        {
            if (GameManager.Instance.IsGameOver) return false;
            return true;
        }

        foreach (EnemyData data in wave.enemies)
        {
            for (int i = 0; i < data.count; ++i)
            {
                if (!CanContinue()) yield break;

                GameObject prefab = GetEnemyPrefab(data.type);
                if (prefab == null) break;

                Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
                GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
                
                WaveEnemyTracker tracker = enemy.AddComponent<WaveEnemyTracker>();
                tracker.SetWaveManager(this);

                activeEnemies++;
                OnEnemySpawned?.Invoke(activeEnemies);

                yield return new WaitForSeconds(UnityEngine.Random.Range(0, data.maxDelay));
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
