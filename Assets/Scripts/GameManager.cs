using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject plrCharacter; 

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject creditsMenu;

    [SerializeField] private TMP_Text waveCounter;
    [SerializeField] private TMP_Text enemyCounter;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int Score { get; private set; }
    public int WaveCount { get; private set; }

    public event Action OnGameOver;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameRestarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // UI Stuff
        pauseMenu.SetActive(false);
        creditsMenu.SetActive(false);

        pauseMenu.transform.Find("Restart").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ForceRestart);
        pauseMenu.transform.Find("Continue").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(TogglePause);
        pauseMenu.transform.Find("Credits").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleCredits);

        WaveManager.OnEnemyKilled += UpdateActiveEnemies;
        WaveManager.OnEnemySpawned += UpdateActiveEnemies;

        WaveManager.OnEnemyKilled += IncreaseScore;
        WaveManager.OnWaveFinished += ProgressGame;
    }

    private void Start()
    {
        WaveCount = 1;
        UpdateWaveCounter();
        WaveManager.Instance.SpawnWave(WaveCount);
    }

    private void Update()
    {
        if (creditsMenu.activeInHierarchy && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            creditsMenu.SetActive(false);
            pauseMenu.SetActive(true);
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    private void ProgressGame()
    {
        Debug.Log("Wave finished, spawning a new one!");

        Score++;
        WaveCount++;
        UpdateWaveCounter();
        WaveManager.Instance.SpawnWave(WaveCount);
    }

    private void IncreaseScore(int amount = 1)
    {
        Score += amount;
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        OnGameOver?.Invoke();
    }

    private void ForceRestart()
    {
        WaveManager.Instance.WipeLevel();
        IsGameOver = true;
        Restart();
    }

    public void Restart()
    {
        if (!IsGameOver) return;

        Score = 0;
        WaveCount = 1;
        IsGameOver = false;
        plrCharacter.transform.position = new Vector3(0, -0.5f, 0);

        UpdateWaveCounter();
        OnGameRestarted?.Invoke();
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void ToggleCredits()
    {
        bool isActive = creditsMenu.activeInHierarchy;
        if (!isActive)
            pauseMenu.SetActive(false);
        creditsMenu.SetActive(!isActive);
    }

    public void Pause()
    {
        if (IsPaused || IsGameOver) return;

        IsPaused = true;
        Time.timeScale = 0f;

        pauseMenu.SetActive(true);

        OnGamePaused?.Invoke();
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;

        pauseMenu.SetActive(false);

        OnGameResumed?.Invoke();
    }

    private void UpdateWaveCounter()
    {
        waveCounter.text = $"Wave: {WaveCount}";
    }

    private void UpdateActiveEnemies(int activeEnemies)
    {
        enemyCounter.text = $"{activeEnemies}";
    }
}