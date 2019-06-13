using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour
{
    [SyncVar] public string username;
    [SerializeField] private GameObject playerNameTag;
    [SerializeField] public GameObject playerUIPrefab, miniMapCirclePrefab;
    [HideInInspector] public GameObject playerUIInstance;
    private GameObject miniMapCircle;
    public PlayerGUI playerGUI;

    private Player player;

    public delegate void LocalPlayerCreated(Player player);
    public static LocalPlayerCreated localPlayerCreatedDelegate;
    private static Color lightGray = new Color(0.9F, 0.9F, 0.9F);

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        // Register player
        GameManager.RegisterPlayer(netId.Value, player);

        if (isLocalPlayer)
        {
            // Set local player on various scripts
            SettingsScript.instance.localPlayer = gameObject;
            MiniMap.instance.localPlayer = transform;

            // Create player UI
            if (PlayerGUI.singleton == null)
                CreatePlayerUI();
        }

        miniMapCircle = Instantiate(miniMapCirclePrefab, transform, false);
        // if (GameManager.localPlayer.team == player.team)
        //     SetMiniMapCircleColor(Color.green);
        // else
        //     SetMiniMapCircleColor(Color.red);

        player.SetFirstSetupDefaults();
        SetupScoreboardEntry();

        if (!isLocalPlayer)
            SetNameTag(username);
        else if (localPlayerCreatedDelegate != null)
            localPlayerCreatedDelegate.Invoke(player);
    }

    private void SetNameTag(string str)
    {
        if (playerNameTag != null)
        {
            playerNameTag.GetComponent<TextMesh>().text = str;
        }
    }

    public void CreatePlayerUI()
    {
        // Create UI instance
        playerUIInstance = Instantiate(playerUIPrefab);
        playerUIInstance.name = playerUIPrefab.name;
        playerGUI = playerUIInstance.GetComponent<PlayerGUI>();
        playerGUI.player = player;
        playerGUI.weaponManager = GetComponent<PlayerWeaponManager>();
    }

    private void SetupScoreboardEntry()
    {
        // Create scoreboard entry with correct color
        Color color = lightGray;
        if (isLocalPlayer)
            color = Color.white;
        ScoreBoardMenu.instance.AddScoreboardEntry(netId.Value, color);
    }

    public void SetMiniMapCircleColor(Color col)
    {
        if (miniMapCircle)
            miniMapCircle.GetComponent<MeshRenderer>().material.color = col;
    }
}
