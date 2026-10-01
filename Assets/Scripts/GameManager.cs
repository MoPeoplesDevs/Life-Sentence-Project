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

    [SerializeField] private GameObject loseMenu;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject creditsMenu;

    [SerializeField] private TMP_Text waveCounter;
    [SerializeField] private TMP_Text enemyCounter;

    private Coroutine progressGameRoutine;
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

        //
        if (groundTiles != null)
            Pathfinder.Initialize(groundTiles, trapMask, wallTiles, doorTiles);

        //
        creditsMenu.SetActive(false);
        winMenu.SetActive(false);

        winMenu.transform.Find("Exit").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Exit);
        loseMenu.transform.Find("Exit").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Exit);
        winMenu.transform.Find("Restart").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Restart);
        loseMenu.transform.Find("Restart").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Restart);

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            pauseMenu.transform.Find("Exit").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Exit);
            pauseMenu.transform.Find("Restart").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ForceRestart);
            pauseMenu.transform.Find("Continue").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(TogglePause);
            pauseMenu.transform.Find("Credits").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleCredits);
        }

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

    private void StopProgressGameRoutine()
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
        winMenu.SetActive(true);

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        OnGameWon?.Invoke();
    }

    private void ForceRestart()
    {
        IsGameOver = true;
        Restart();
    }

    public void Restart()
    {
        if (!IsGameOver) return;

        StopProgressGameRoutine();

        WaveManager.Instance.WipeLevel();

        Score = 0;
        WaveCount = 1;
        IsGameOver = false;

        Playercontroller.Respawn();
        UpdateWaveCounter();
        OnGameRestarted?.Invoke();

        winMenu.SetActive(false);
        loseMenu.SetActive(false);
        pauseMenu.SetActive(false);

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

        winMenu.SetActive(false);
        loseMenu.SetActive(false);
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

    public void Play()
    {
        SceneManager.LoadScene(1);
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