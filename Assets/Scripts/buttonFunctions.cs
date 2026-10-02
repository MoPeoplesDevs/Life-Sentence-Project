using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public static buttonFunctions Instance { get; private set; }

    [SerializeField] private GameObject loseMenu;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject creditsMenu;

    public bool IsPaused { get; private set; }

    public event System.Action OnGameRestarted;

    [SerializeField] private Playercontroller Playercontroller;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Playercontroller = FindAnyObjectByType<Playercontroller>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWon += ShowWinMenu;
        }
    
    }
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if(creditsMenu.activeInHierarchy && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            creditsMenu.SetActive(false);
            pauseMenu.SetActive(true);
        }
    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;

        winMenu.SetActive(false);
        loseMenu.SetActive(false);
        pauseMenu.SetActive(false);

    }

    public void Pause()
    {
        if (IsPaused || GameManager.Instance.IsGameOver) return;

        IsPaused = true;
        Time.timeScale = 0f;

        pauseMenu.SetActive(true);

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

    public void Restart()
    { 
        GameManager.Instance.StopProgressGameRoutine();

        WaveManager.Instance.WipeLevel();

        GameManager.Instance.Score = 0;
        GameManager.Instance.WaveCount = 1;
        GameManager.Instance.IsGameOver = false;

        IsPaused = false;
        Time.timeScale = 1f;

        Playercontroller.Respawn();
        GameManager.Instance.UpdateWaveCounter();

        winMenu.SetActive(false);
        loseMenu.SetActive(false);
        pauseMenu.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        WaveManager.Instance.SpawnWave(GameManager.Instance.WaveCount);
    }

    private void ForceRestart()
    {
        GameManager.Instance.IsGameOver = true;
        Restart();
    }

    public void ShowWinMenu()
    {
        winMenu.SetActive(true);
    }

}
