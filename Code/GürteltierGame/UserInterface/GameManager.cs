using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private PlayerInputController inputController;

    [SerializeField] private GameObject overlayPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject controlsTooltip;
    private bool showingControls = false;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject confirmDialog;

    private bool isPaused;
    public bool useCursorLock;

    private GameObject activePopup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        inputController.pauseAction.performed += ctx =>
        {
            OnPause(ctx);
        };
        inputController.interactAction.performed += ctx =>
        {
            OnInteract(ctx);
        };
        inputController.toggleControlsAction.performed += ctx =>
        {
            OnToggleControls(ctx); 
        };

        if (useCursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnDestroy()
    {
        inputController.pauseAction.performed -= OnPause;
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        if (useCursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void openSettingsMenu()
    {
        //pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void closeSettingsMenu()
    {
        //pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void closeConfirmDialog()
    {
        confirmDialog.SetActive(false);
    }
    public void QuitToMainMenu()
    {
        confirmDialog.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (activePopup) HidePopup();
    }

    private void OnToggleControls(InputAction.CallbackContext ctx)
    {
        ToggleControls();
    }

    public void ToggleControls()
    {
        if (showingControls)
        {
            controlsPanel.SetActive(false);
            //controlsTooltip.SetActive(true);
            showingControls = false;
        }
        else
        {
            controlsPanel.SetActive(true);
            //controlsTooltip.SetActive(false);
            showingControls = true;
        }
    }

    public void ShowPopup(GameObject popup)
    {
        activePopup = popup;
        overlayPanel.SetActive(true);
        activePopup.SetActive(true);
    }

    private void HidePopup()
    {
        overlayPanel.SetActive(false);
        activePopup.SetActive(false);
    }
}