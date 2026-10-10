using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Unity.Tutorials.Editor;

public class buttonFunctions : MonoBehaviour
{
    [SerializeField] private GameObject loseMenu;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private GameObject restartButton;


    public bool IsPaused { get; private set; }

    public bool isControllerActive;
    public event System.Action OnGameRestarted;

    [SerializeField] private Playercontroller Playercontroller;
    
    private void Awake()
    {
        Playercontroller = FindAnyObjectByType<Playercontroller>();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameWon += ShowWinMenu;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
            TogglePause();

        if(creditsMenu.activeInHierarchy && (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame))
        {
            creditsMenu.SetActive(false);
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
            }
            
        }

        if (Gamepad.current != null)
        {
            if(Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
            {
                isControllerActive = false;
                Cursor.visible = IsPaused;
            }

            if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.25f && !isControllerActive)
            {
                Cursor.visible = false;

                if (!isControllerActive)
                {
                    isControllerActive = true;

                    if (playButton != null && playButton.activeInHierarchy)
                    {
                        EventSystem.current.SetSelectedGameObject(playButton);
                    }
                    else if (pauseMenu != null && pauseMenu.activeInHierarchy && resumeButton != null)
                    {
                        EventSystem.current.SetSelectedGameObject(resumeButton);
                    }
                    else if (winMenu != null && winMenu.activeInHierarchy && nextLevelButton != null)
                    {
                        EventSystem.current.SetSelectedGameObject(nextLevelButton);
                    }
                    else if (loseMenu != null && loseMenu != null && loseMenu.activeInHierarchy && restartButton != null)
                    {
                        EventSystem.current.SetSelectedGameObject(restartButton);
                    }
                }

            }
        }

        if(Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.delta.ReadValue().magnitude > 0.2f)
        {
            isControllerActive = false;
            Cursor.visible = IsPaused || winMenu.activeInHierarchy || loseMenu.activeInHierarchy;
            
            EventSystem.current.SetSelectedGameObject(null);
        }

    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    public void NextLevel()
    {
        GameManager.Instance.IsFromContinued = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

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

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

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
        if (IsPaused)
            Resume();
        else
            Pause();

        UnityEngine.Cursor.visible = !IsPaused;
        UnityEngine.Cursor.lockState = !IsPaused ? CursorLockMode.None : CursorLockMode.Confined;

        if ( IsPaused && isControllerActive && resumeButton != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton);
        }
    }

    public void ToggleCredits()
    {
        bool isActive = creditsMenu.activeInHierarchy;
        if (!isActive)
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
        }
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
        if (winMenu == null) return;

        winMenu.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        if (isControllerActive && nextLevelButton != null)
        {
            EventSystem.current.SetSelectedGameObject(nextLevelButton);
        }
    }
}
