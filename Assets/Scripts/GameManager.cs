using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject creditsMenu;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int Score { get; private set; }

    public event Action OnGameOver;
    public event Action OnGameRestarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;

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

        pauseMenu.transform.Find("Continue").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(TogglePause);
        pauseMenu.transform.Find("Credits").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ToggleCredits);
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

    public void IncreaseScore(int amount = 1)
    {
        Score += amount;
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        OnGameOver?.Invoke();
    }

    public void Restart()
    {
        if (!IsGameOver) return;

        IsGameOver = false;
        Score = 0;

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
}