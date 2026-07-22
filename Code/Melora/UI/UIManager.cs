using UnityEngine;

public class UIManager : MonoBehaviour
{
    private bool cursorEnabled = false;

    private void Start()
    {
        //SetCursorState(false);
    }

    public void ToggleCursor()
    {
        cursorEnabled = !cursorEnabled;
        SetCursorState(cursorEnabled);
    }

    public void SetCursorState(bool enabled)
    {
        Cursor.visible = enabled;
        Cursor.lockState = enabled ? CursorLockMode.None : CursorLockMode.Locked;
    }
}