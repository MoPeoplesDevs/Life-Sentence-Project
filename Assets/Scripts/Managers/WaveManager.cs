using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    public Dictionary<string, EnemyDefinition> enemyConfig {get; private set;}
    private GameObject[] enemies;

    [SerializeField] GameObject InmateBossPrefab;
    [SerializeField] GameObject correctionalOfficerBossPrefab;
    [SerializeField] GameObject gangFightBossPrefab;
    [SerializeField] GameObject wardenBossPrefab;

    private List<WaveData> waves;
    private Transform enemySpawns;
    private bool finishedSpawning = false;
    public int activeEnemies {get; private set;} = 0;
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

        //
        enemies = Resources.LoadAll<GameObject>("Enemies");
        enemyConfig = EnemyConfigLoader.Load();
    }

    public void Initialize(LevelData currentLevel)
    {
        waves = currentLevel.waves;
        enemySpawns = GameObject.Find("Enemy Spawns").transform;

        spawnPoints.Clear();
        foreach (Transform point in enemySpawns)
            spawnPoints.Add(point);
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
                if (prefab == null)
                {
                    Debug.LogWarning($"Failed to find prefab for enemy type: {data.type}");
                    break;
                }

                Vector2 offset = UnityEngine.Random.insideUnitCircle * 1.5f;
                Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
                Vector3 spawnPosition = spawnPoint.position + new Vector3(offset.x, offset.y, 0f);
                GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
                WaveEnemyTracker tracker = enemy.AddComponent<WaveEnemyTracker>();
                
                tracker.SetWaveManager(this);
                activeEnemies++;
                OnEnemySpawned?.Invoke(activeEnemies);

                yield return new WaitForSeconds(UnityEngine.Random.Range(0, data.maxDelay));
            }
        }

        finishedSpawning = true;
        CheckWaveComplete();
    }

    public void EnemyDestroyed()
    {
        activeEnemies--;
        OnEnemyKilled?.Invoke(activeEnemies);
        CheckWaveComplete();
    }

    public EnemySprites GetEnemySprites(string type, string variant)
    {
        return enemyConfig[type].Variants[variant];
    }

    void CheckWaveComplete()
    {
        if (finishedSpawning && activeEnemies <= 0)
            OnWaveFinished?.Invoke();
    }

    GameObject GetEnemyPrefab(string enemyType)
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy.name == enemyType)
                return enemy;
        }
        
        switch (enemyType)
        {
            case "InmateBoss":
                return InmateBossPrefab;

            case "CorrectionalOfficerBoss":
                return correctionalOfficerBossPrefab;

            case "GangFightBoss":
                return gangFightBossPrefab;

            case "WardenBoss":
                return wardenBossPrefab;

            default:
                Debug.LogWarning("Unknown enemy type: " + enemyType);
                return null;
        }
    }
}
