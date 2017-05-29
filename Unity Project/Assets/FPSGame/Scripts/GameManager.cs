using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public enum GameMode
{
    Deathmatch,
    CaptureTheFlag,
    Conquest,
};

public enum Team
{
    None = 0,
    Red = 1,
    Blue = 2,
};

public class GameManager : NetworkBehaviour {

    public static GameManager instance;
    public static Player localPlayer;

    [SyncVar] public int respawnTime = 3;
    [SyncVar] public ushort roundTime = 10;
    [SyncVar] public string mapName;
    [SyncVar] public string version = Version.VERSION;
    [SyncVar] public bool canShootWhileSprinting, bhopEnabled;
    [SyncVar] public GameMode gameMode = GameMode.CaptureTheFlag;
    [SyncVar(hook="GameIsOver")] public bool gameIsOver;

    [SerializeField] private GameObject gameOverContainer;
    [SerializeField] private Text winLossText, scoresText;
    [SerializeField] private Image gameOverPanel;

    public static Color GetTeamColor(Team team)
    {
        Color color;
        switch (team)
        {
            case Team.Red:
                color = Color.red;
                break;
            case Team.Blue:
                color = Color.blue;
                break;
            default:
                color = Color.black;
                break;
        }
        return color;
    }

    private void OnEnable()
    {
        PlayerSetup.localPlayerCreatedDelegate += LocalPlayerCreated;
        RoundInfoScript.roundTimeUpDelegate += RoundTimeUp;
    }

    private void OnDisable()
    {
        localPlayer = null;
        PlayerSetup.localPlayerCreatedDelegate -= LocalPlayerCreated;
        RoundInfoScript.roundTimeUpDelegate -= RoundTimeUp;
    }

    private void LocalPlayerCreated(Player player)
    {
        localPlayer = player;
    }

    private void Awake()
    {
        version = Version.VERSION;

        if (instance != null)
            Debug.LogError("More than one gamemanager in the scene.");
        else
            instance = this;
    }

    private void Start()
    {
        GameIsOver(gameIsOver);
    }

    [Server]
    private void RoundTimeUp()
    {
        gameIsOver = true;
    }

    private void GameIsOver(bool gameIsOver)
    {
        this.gameIsOver = gameIsOver;

        if (gameIsOver) {
            gameOverContainer.SetActive(true);
            RoundInfoScript roundInfoScript = RoundInfoScript.singleton;
            SetGameOverText(roundInfoScript.redScore, roundInfoScript.blueScore);
        }
    }

    private void SetGameOverText(ushort redScore, ushort blueScore)
    {
        // Set win/loss text
        bool playerWon = false, redScoreGreater = (redScore > blueScore);
        Team localPlayerTeam = localPlayer.team;
            switch (localPlayerTeam) {
                case Team.Red: {
                    playerWon = redScoreGreater;
                    break;
                }
                case Team.Blue: {
                    playerWon = !redScoreGreater;
                    break;
                }
            }

        if (playerWon)
            winLossText.text = "Your Team Won!";
        else
            winLossText.text = "Your Team Lost!";

        // Set ScoresText
        scoresText.text = "Blue: " + blueScore.ToString() + " – " + "Red: " + redScore.ToString();

        // Fade in
        StartCoroutine(Util.FadeInImage(gameOverPanel, 0.8F));
    }

    public static Team RequestTeam()
    {
        switch (instance.gameMode)
        {
            case GameMode.CaptureTheFlag:
            case GameMode.Conquest:
                {
                    ushort numOfBlue, numOfRed;
                    NumberOfPlayersOnEachTeam(out numOfRed, out numOfBlue);

                    if (numOfRed > numOfBlue) {
                        return Team.Blue;
                    } else {
                        return Team.Red;
                    }
                }
            default:
                {
                    return Team.None;
                }
        }
    }

    private static void NumberOfPlayersOnEachTeam(out ushort numOfRed, out ushort numOfBlue)
    {
        numOfBlue = 0; numOfRed = 0;
        foreach (KeyValuePair<uint, Player> keyValuePair in players)
        {
            Team team = keyValuePair.Value.team;
            switch (team)
            {
                case Team.Red:
                    {
                        numOfRed+=1;
                        break;
                    }
                case Team.Blue:
                    {
                        numOfBlue+=1;
                        break;
                    }
            }
        }
    }

    #region Player Tracking

    public static Dictionary<uint, Player> players = new Dictionary<uint, Player>();

	public static void RegisterPlayer(uint netId, Player player)
    {
		players.Add(netId, player);
		player.transform.name = "Player: " + netId;
    }

	public static void UnRegisterPlayer(uint netId)
    {
        players.Remove(netId);
    }

	public static Player GetPlayer(uint netId)
    {
        Player player;
        players.TryGetValue(netId, out player);
        return player;
    }

    public static string GetPlayerUsername(uint netId)
    {
        Player player = GetPlayer(netId);
        return player.GetComponent<PlayerSetup>().username;
    }

    #endregion Player Tracking

    void OnGUI()
    {
        if (Input.GetButton("Debug"))
        {
            // Display controller IDs
            GUILayout.BeginArea(new Rect(200, 200, 200, 500));
            GUILayout.BeginVertical();

            List<uint> keys = new List<uint>(players.Keys);
            foreach (uint id in keys)
            {
                GUILayout.Label(id.ToString());
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // FPS display
        int fps = Mathf.RoundToInt(1F / Time.smoothDeltaTime);
        if (Input.GetButton("Debug"))
            GUI.Label(new Rect(5, 5, 500, 20), "FPS: " + fps.ToString());

    }
}
