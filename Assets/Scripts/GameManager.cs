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

    [SerializeField] private Tilemap wallTiles;
    [SerializeField] private Tilemap doorTiles;
    [SerializeField] private Tilemap groundTiles;
    [SerializeField] private LayerMask trapMask;
    [SerializeField] private TMP_Text waveCounter;
    [SerializeField] private TMP_Text enemyCounter;
    [SerializeField] private TextAsset levelConfigFile;

    private Coroutine progressGameRoutine;
    private Playercontroller Playercontroller;

    public int Score { get; set; }
    public int WaveCount { get; set; }
    public bool IsGameOver { get; set; }
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

        if (groundTiles != null)
            Pathfinder.Initialize(groundTiles, trapMask, wallTiles, doorTiles);
        
        // Grab level data
        LevelConfig config = LevelConfig.FromJson(levelConfigFile.text);
        Levels = config.levels;

        WaveManager.OnEnemyKilled += UpdateActiveEnemies;
        WaveManager.OnEnemySpawned += UpdateActiveEnemies;

        WaveManager.OnEnemyKilled += IncreaseScore;
        WaveManager.OnWaveFinished += ProgressGame;
    }

    private void Start()
    {
        Playercontroller = FindAnyObjectByType<Playercontroller>();
        if (Playercontroller != null)
            Playercontroller.OnPlayerDeath += GameOver;
        
        // Load level data
        string sceneName = SceneManager.GetActiveScene().name;
        Level = Levels.Find(level => level.name == sceneName);

        if (Level == null)
            Debug.LogWarning($"FAILED TO FIND LEVEL DATA FOR SCENE: {sceneName}");
        
        WaveManager.Instance.Initialize(Level);

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