using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

public class EscapeMenuScript : MonoBehaviour {

    public static EscapeMenuScript instance = null;

    [SerializeField]
    private GameObject[] buttons;

    [SerializeField]
    private GameObject quitConfirmationWindow;

    [SerializeField]
    private GameObject escapeMenuCanvas;

    private GameObject[] canvases;

    private NetworkManager networkManager;

    public static bool escapeMenuIsOpen = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();
    }

    void Update()
    {
        bool escapeKey = Input.GetButtonDown("Cancel");

        if (escapeKey && !escapeMenuIsOpen)
            OnOpenEscapeMenu();
        else if (escapeKey && escapeMenuIsOpen)
            OnCloseEscapeMenu();

    }

    void SetOtherCanvasesState(GameObject[] canvases, bool isEnabled)
    {
        if (canvases == null)
            return;

        foreach (GameObject canvas in canvases)
        {
            canvas.SetActive(isEnabled);
        }
    }

    public void OnDisconnectClicked()
    {
        networkManager = NetworkManager.singleton;

        if (networkManager.matchMaker != null)
        {
            MatchInfo matchInfo = networkManager.matchInfo;
            networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, networkManager.OnDropConnection);
        }
        networkManager.StopHost();

        OnCloseEscapeMenu();

        CursorManagement.CorrectLockMode();

        if (ClassManager.singleton != null) ClassManager.singleton.weaponManagerCanvas.SetActive(false);
        //MenuScript.instance.startMenuCanvas.SetActive(true);
        if (MultiplayerScript.isLoggedIn) MultiplayerScript.instance.multiplayerMenuCanvas.SetActive(true);
        else MenuScript.instance.startMenuCanvas.SetActive(true);
    }

    public void SetButtonInteractableState(bool isInteractable)
    {
        foreach(GameObject button in buttons)
        {
            button.GetComponent<Button>().interactable = isInteractable;
        }
    }

    public void OnExitClicked()
    {
        quitConfirmationWindow.SetActive(true);
        SetButtonInteractableState(false);
    }

    public void OnExitYesClicked()
    {
        quitConfirmationWindow.SetActive(false);
        escapeMenuCanvas.SetActive(false);
    
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OnExitNoClicked()
    {
        quitConfirmationWindow.SetActive(false);
        SetButtonInteractableState(true);
    }

    public void OnOpenEscapeMenu()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            canvases = GameObject.FindGameObjectsWithTag("Canvas");
            SetOtherCanvasesState(canvases, false);
            escapeMenuCanvas.SetActive(true);

            escapeMenuIsOpen = true;

            CursorManagement.CorrectLockMode();
        }
    }

    public void OnCloseEscapeMenu()
    {
        escapeMenuCanvas.SetActive(false);
        SetOtherCanvasesState(canvases, true);

        escapeMenuIsOpen = false;

        CursorManagement.CorrectLockMode();
    }

    public void OnSettingsClicked()
    {
        if (SettingsScript.instance != null)
            SettingsScript.instance.OnOpenSettings();
    }

    //void OnApplicationQuit()
    //{
    //    networkManager = NetworkManager.singleton;

    //    if (networkManager.matchMaker != null)
    //    {
    //        MatchInfo matchInfo = networkManager.matchInfo;
    //        networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, networkManager.OnDropConnection);
    //        networkManager.StopMatchMaker();
    //    }

    //    Debug.Log("Quitting...");
    //    networkManager = NetworkManager.singleton;
    //    networkManager.StopHost();
    //}
}
