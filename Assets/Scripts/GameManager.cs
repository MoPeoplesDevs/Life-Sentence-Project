using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject loseMenu;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject creditsMenu;

    [SerializeField] private TMP_Text waveCounter;
    [SerializeField] private TMP_Text enemyCounter;

    private Playercontroller Playercontroller;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int Score { get; private set; }
    public int WaveCount { get; private set; }

    public event Action OnGameOver;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameRestarted;
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

        // UI Stuff
        pauseMenu.SetActive(false);
        creditsMenu.SetActive(false);
        winMenu.SetActive(false);

        loseMenu.transform.Find("Restart").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Restart);
        loseMenu.transform.Find("Exit").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Exit);
        winMenu.transform.Find("Restart").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Restart);
        winMenu.transform.Find("Exit").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Exit);
        pauseMenu.transform.Find("Restart").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ForceRestart);
        pauseMenu.transform.Find("Continue").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(TogglePause);
        pauseMenu.transform.Find("Credits").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleCredits);
        pauseMenu.transform.Find("Exit").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Exit);

        OnGameExit += QuitGame;
        
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
        StartCoroutine(ProgressGameRoutine());
    }

    private IEnumerator ProgressGameRoutine()
    {
        if (IsGameOver) yield break;
        if (WaveManager.Instance == null) yield break;

        if(WaveCount >= 5)
        {
            WinGame();
            yield break;
        }

        Score++;
        WaveCount++;

        UpdateWaveCounter();

        yield return new WaitForSeconds(1f);

        if (IsGameOver) yield break;

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
    public void WinGame()
    {
        Time.timeScale = 0f;

        winMenu.SetActive(true);

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    private void ForceRestart()
    {
        IsGameOver = true;
        Restart();
    }

    public void Restart()
    {
        if (!IsGameOver) return;

        WaveManager.Instance.WipeLevel();

        Score = 0;
        WaveCount = 1;
        IsGameOver = false;
        Playercontroller.Respawn();

        UpdateWaveCounter();
        OnGameRestarted?.Invoke();
        WaveManager.Instance.SpawnWave(WaveCount);
    }

    public void TogglePause()
    {
        UnityEngine.Cursor.visible = !IsPaused;
        UnityEngine.Cursor.lockState = !IsPaused ? CursorLockMode.None : CursorLockMode.Confined;
        
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

    public void Exit()
    {
        OnGameExit?.Invoke();
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}