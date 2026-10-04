using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Collections;
using TMPro;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Tilemap wallTiles;
    [SerializeField] private Tilemap doorTiles;
    [SerializeField] private Tilemap groundTiles;
    [SerializeField] private LayerMask trapMask;
    [SerializeField] private TMP_Text waveCounter;
    [SerializeField] private TMP_Text enemyCounter;

    private Coroutine progressGameRoutine;
    private Playercontroller Playercontroller;

    public bool IsGameOver { get; set; }
    public int Score { get; set; }
    public int WaveCount { get; set; }

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

        if(WaveCount > 5)
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
        GameOver();

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

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