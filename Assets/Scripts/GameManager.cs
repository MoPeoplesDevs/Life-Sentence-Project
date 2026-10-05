using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Tilemap wallTiles;
    private Tilemap doorTiles;
    private Tilemap groundTiles;
    private LayerMask trapMask;
    private TMP_Text waveCounter;
    private TMP_Text enemyCounter;

    private Coroutine progressGameRoutine;
    private Playercontroller Playercontroller;

    public int Score { get; set; }
    public int WaveCount { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsFromContinued {get; set;}
    public LevelData Level {get; private set;}
    public List<LevelData> Levels { get; private set;}

    public event Action OnGameOver;
    public event Action OnGameExit;
    public event Action OnGameWon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Grab level data
        TextAsset levelConfig = Resources.Load<TextAsset>("Data/Level");
        Levels = LevelConfig.FromJson(levelConfig.text).levels;

        WaveManager.OnEnemyKilled += UpdateActiveEnemies;
        WaveManager.OnEnemySpawned += UpdateActiveEnemies;

        WaveManager.OnEnemyKilled += IncreaseScore;
        WaveManager.OnWaveFinished += ProgressGame;

        //
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        Playercontroller = FindAnyObjectByType<Playercontroller>();
        if (Playercontroller != null)
            Playercontroller.OnPlayerDeath += GameOver;
        
        SetSceneReferences();

        //
        WaveCount = 1;
        UpdateWaveCounter();
        WaveManager.Instance.SpawnWave(WaveCount);
    }

    private void ProgressGame()
    {
        if (IsGameOver) return;
        if (progressGameRoutine != null) return;
        progressGameRoutine = StartCoroutine(ProgressGameRoutine());
    }

    private IEnumerator ProgressGameRoutine()
    {
        if (IsGameOver) yield break;
        if (WaveManager.Instance == null) yield break;

        Score++;
        WaveCount++;

        if (WaveCount - 1 >= Level.waves.Count)
        {
            WinGame();
            yield break;
        }

        yield return new WaitForSeconds(1f);

        UpdateWaveCounter();
        if (IsGameOver || WaveManager.Instance == null)
        {
            progressGameRoutine = null;
            yield break;
        }

        WaveManager.Instance.SpawnWave(WaveCount);
        progressGameRoutine = null;
    }

    public void StopProgressGameRoutine()
    {
        if (progressGameRoutine == null) return;

        StopCoroutine(progressGameRoutine);
        progressGameRoutine = null;
    }

    private void IncreaseScore(int amount = 1)
    {
        Score += amount;
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        StopProgressGameRoutine();

        OnGameOver?.Invoke();
    }
    public void WinGame()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        GameOver();
        OnGameWon?.Invoke();
    }

    public void UpdateWaveCounter()
    {
        waveCounter.text = $"Wave: {WaveCount}";
    }

    private void UpdateActiveEnemies(int activeEnemies)
    {
        enemyCounter.text = $"{activeEnemies}";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsFromContinued)
        {
            SetSceneReferences();
            WaveManager.Instance.SpawnWave(WaveCount);
            IsFromContinued = false;
        }
    }

    private void SetSceneReferences()
    {
        //
        GameObject ui = GameObject.Find("UI");
        GameObject grid = GameObject.Find("Grid");

        if (grid != null)
        {
            wallTiles = grid.transform.Find("Walls").GetComponent<Tilemap>();
            doorTiles = grid.transform.Find("Doors").GetComponent<Tilemap>();
            groundTiles = grid.transform.Find("Ground").GetComponent<Tilemap>();
        }

        if (ui != null)
        {
            Transform hud = ui.transform.Find("MainRoot/HUD");

            waveCounter = hud.Find("Enemy/Wave").GetComponent<TMP_Text>();
            enemyCounter = hud.Find("Enemy/Amount").GetComponent<TMP_Text>();
        }
        
        // Load level data
        string sceneName = SceneManager.GetActiveScene().name;
        Level = Levels.Find(level => level.name == sceneName);

        if (Level == null)
            Debug.LogWarning($"FAILED TO FIND LEVEL DATA FOR SCENE: {sceneName}");

        WaveManager.Instance.Initialize(Level);
        Pathfinder.Initialize(groundTiles, trapMask, doorTiles, wallTiles);
    }

    private void OnDestroy()
    {
        WaveManager.OnEnemyKilled -= UpdateActiveEnemies;
        WaveManager.OnEnemySpawned -= UpdateActiveEnemies;
        WaveManager.OnEnemyKilled -= IncreaseScore;
        WaveManager.OnWaveFinished -= ProgressGame;

        if (Playercontroller != null)
            Playercontroller.OnPlayerDeath -= GameOver;

        if (Instance == this)
            Instance = null;
    }
}



// {
//             "name": "Level 1",
//             "waves": [
//                 {
//                     "enemies": [
//                         {
//                             "type": "Horde",
//                             "count": 4,
//                             "maxDelay": 0.25
//                         }
//                     ]
//                 },
//                 {
//                     "enemies": [
//                         {
//                             "type": "Heavy",
//                             "count": 2,
//                             "maxDelay": 1.5
//                         }
//                     ]
//                 },
//                 {
//                     "enemies": [
//                         {
//                             "type": "Horde",
//                             "count": 12,
//                             "maxDelay": 0.25
//                         },
//                         {
//                             "type": "Heavy",
//                             "count": 2,
//                             "maxDelay": 0.25
//                         }
//                     ]
//                 }
//             ]
//         },