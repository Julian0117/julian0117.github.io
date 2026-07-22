using UnityEngine;
using UnityEngine.InputSystem;

public class PauseModeNoUI : MonoBehaviour
{
    [SerializeField]
    private PlayerInputController inputController;

    private bool isPaused;

    private void Start()
    {
        isPaused = false;
        inputController.pauseAction.performed += ctx =>
        {
            OnPause(ctx);
        };
        Cursor.lockState = CursorLockMode.Locked;
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

    private void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;  
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}