using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ClassManager : MonoBehaviour {

    public GameObject localPlayer;
    public static ClassManager singleton;
    private NetworkManager networkManager;
    
    [SerializeField] public GameObject weaponManagerCanvas;
    [SerializeField] private Button[] buttons;

    public PlayerClass currentClass { get; private set; }
    public Animator animator;

    public static bool open;

    private void Awake()
    {
        if (singleton == null)  
            singleton = this;
        else if (singleton != this)
            Destroy(gameObject);

        animator = GetComponent<Animator>();
        //DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        PlayerSetup.localPlayerCreatedDelegate += LocalPlayerCreated;
    }

    private void OnDisable()
    {
        PlayerSetup.localPlayerCreatedDelegate += LocalPlayerCreated;
    }

    // Callback function
    private void LocalPlayerCreated(Player localPlayerScript)
    {
        localPlayer = localPlayerScript.gameObject;
    }

    public void OnAssaultClassClicked()
    {
        AddPlayer(PlayerClass.Assault);
    }

    public void OnSniperClassClicked()
    {
        AddPlayer(PlayerClass.Sniper);
    }
        
	public void OnDemoClassClicked()
    {
        AddPlayer(PlayerClass.Demolitionist);
    }

    public void OnMedicClicked()
    {
        AddPlayer(PlayerClass.Medic);
    }

    public void OnEngineerClicked()
    {
        AddPlayer(PlayerClass.Engineer);
    }

    public void AddPlayer(PlayerClass playerClass)
    {
        currentClass = playerClass;

        Transform spawn = (NetworkManager.singleton as MyNetworkManager).GetSpawnPosition(GameManager.localPlayer.team);
        localPlayer.transform.position = spawn.position;
        localPlayer.transform.rotation = spawn.rotation;

        localPlayer.GetComponent<PlayerClassManager>().OnSetClass(playerClass);

        // Close the settings
        Close();
    }

    public void Open()
    {
        open = true;

        foreach (Button button in buttons)
        {
            button.interactable = true;
        }
        animator.CrossFade("ClassManagerOpen", 0.5f);
    }

    public void Close()
    {
        open = false;

        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
        animator.CrossFade("ClassManagerClose", 0.5f);
    }

    public void OnDestroy()
    {
        open = false;
    }
}

public enum PlayerClass
{
    Sniper = 1,
    Assault = 2,
    Demolitionist = 3,
    Medic = 4,
    Engineer = 5
}
