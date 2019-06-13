using UnityEngine;
using UnityEngine.SceneManagement;

static class CursorManagement {

	public static bool CursorShouldBeLocked()
    {
        if (IsOnMenuScene() || IsPlayerDead() || IsWorldEditCanvasOpen())
        {
            return false;
        }
        else
        {
            return !IsMenuOpen();
        }
    }

    public static bool IsMenuOpen()
    {
        return ConsoleScript.open || EscapeMenuScript.escapeMenuIsOpen || ClassManager.open || SettingsScript.settingsAreOpen;
    }

    public static bool IsOnMenuScene()
    {
        return SceneManager.GetActiveScene().buildIndex == 0;
    }

    public static bool IsWorldEditCanvasOpen()
    {
        if (WorldEditScript.instance != null) return WorldEditScript.instance.worldEditMenuCanvas.activeSelf || WorldEditScript.instance.selectionCanvas.activeSelf;
        else return false;
    }

    public static bool IsPlayerDead()
    {
        if (GameManager.localPlayer)
        {
            Player player = GameManager.localPlayer;
            // Check whether or not the player is dead
            return player.isDead;
        }
        else if (WorldEditScript.instance.playerWorldEditScript)
        {
            return false;
        }
        else return true;
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public static void CorrectLockMode()
    {
        if (CursorShouldBeLocked())
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
