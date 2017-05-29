using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {

    public static MenuScript instance;
    [SerializeField] public GameObject startMenuCanvas;
    [SerializeField] private GameObject confirmationWindow;
    [SerializeField] private GameObject[] menuButtons;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SetMenuButtonEnabledState(bool enabledState)
    {
        foreach (GameObject menuButton in menuButtons)
        {
            menuButton.GetComponent<Button>().interactable = enabledState;
        }
    }

    public void OnMultiplayerClicked()
    {
        MultiplayerScript.instance.gameObject.SetActive(true);
        MultiplayerScript.instance.OnMultiplayerClicked();
    }

    public void OnWorldEditClicked()
    {
        // Change to world edit canvas
        startMenuCanvas.SetActive(false);
        WorldEditScript.instance.chooseMapCanvas.SetActive(true);
        WorldEditScript.instance.UpdateMatchMapText();
    }

    public void OnSettingsClicked()
    {
        SettingsScript settings = SettingsScript.instance;

        settings.OnOpenSettings();
    }

    public void OnExitClicked()
    {
        confirmationWindow.SetActive(true);
        SetMenuButtonEnabledState(false);
    }

    public void OnExitYesClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OnExitNoClicked()
    {
        confirmationWindow.SetActive(false);
        SetMenuButtonEnabledState(true);
    }
}
