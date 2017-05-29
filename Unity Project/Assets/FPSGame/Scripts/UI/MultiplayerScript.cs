using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

public class MultiplayerScript : MonoBehaviour {

    public static MultiplayerScript instance;
    private bool searchingForGame = false;
	private bool escapeKey;  
    public string playerUserName;
    [SerializeField] private Scene menuScene;
    [SerializeField] private GameObject loadingScreenPrefab;
    [SerializeField] private GameObject weaponSelectPrefab;
    [SerializeField] public GameObject multiplayerMenuCanvas, matchmakingMenuCanvas, createMatchCanvas, joinMatchCanvas, createLocalGameCanvas;
    private NetworkManager networkManager;
    private string[] localMapList = Directory.GetDirectories("maps/");
    public static string selectedMapName = "";

    public static bool isLoggedIn;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        searchingForGame = false;
    }

    private void Start()
    {
        networkManager = NetworkManager.singleton;
    }

    public bool HostTestGame() {
        if (!ClientScene.ready)
        {
            selectedMapName = "castle";
            playerUserName = "test-player";
            HostGame();
            MenuScript.instance.startMenuCanvas.SetActive(false);
            return true;
        }
        return false;
    }

    private void Update()
    {
        escapeKey = Input.GetButton("Cancel");
    }

    #region Local Networking

    [SerializeField] private Text createLocalGameMapText;

    public void OnHostClicked()
    {
        multiplayerMenuCanvas.SetActive(false);
        createLocalGameCanvas.SetActive(true);
        UpdateMatchMapText();
    }

    public void HostGame()
    {
        networkManager = NetworkManager.singleton;
        networkManager.networkPort = 7777;
        networkManager.networkAddress = "127.0.0.1";
        networkManager.StartHost();

        createLocalGameCanvas.SetActive(false);

        StartCoroutine(JoinGame(true));
    }

    public void OnHostGameBackClicked()
    {
        multiplayerMenuCanvas.SetActive(true);
        createLocalGameCanvas.SetActive(false);
    }

    public void OnStartServerClicked()
    {
        networkManager.StartServer();
        multiplayerMenuCanvas.SetActive(false);
    }

    public void OnJoinClicked()
    {
        //Search for local game started
        searchingForGame = true;
        //Start network client
        networkManager.networkAddress = "127.0.0.1";
        networkManager.networkPort = 7777;
        networkManager.StartClient();

        multiplayerMenuCanvas.SetActive(false);

        StartCoroutine(JoinGame(false));
    }

    #endregion

    #region Create Match

    private int currentMapIndex = 0;
    private string matchName = "Default Match";
    private uint matchSize = 2;
    private int maxMatchNameLength = 14;
    private int maxMatchSize = 16;
    [SerializeField] private Text matchSizeText, mapTextCreateMatch;

    public void OnMatchNameEntered(string newMatchName)
    {
        if (newMatchName != null && newMatchName != "" && newMatchName.Length <= maxMatchNameLength)
            matchName = newMatchName;
    }

    public void OnMatchSizeUp()
    {
        if (matchSize < maxMatchSize)
            matchSize++;

        UpdateMatchSizeText();
    }

    public void OnMatchSizeDown()
    {
        if (matchSize > 2)
            matchSize--;

        UpdateMatchSizeText();
    }

    public void UpdateMatchSizeText()
    {
        matchSizeText.text = "Match Size: " + matchSize.ToString();
    }

    public void OnCreateMatch()
    {
        networkManager = NetworkManager.singleton;

        networkManager.matchMaker.CreateMatch(matchName + " | " + selectedMapName, matchSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        matchmakingMenuCanvas.SetActive(false);

        StartCoroutine(JoinGame(true));
        createMatchCanvas.SetActive(false);
    }

    public void OnCreateMatchBackClicked()
    {
        createMatchCanvas.SetActive(false);
        matchmakingMenuCanvas.SetActive(true);
    }

    #endregion

    public void OnMatchMapUp()
    {
        if (currentMapIndex < localMapList.Length - 1)
            currentMapIndex++;

        UpdateMatchMapText();
    }

    public void OnMatchMapDown()
    {
        if (currentMapIndex > 0)
            currentMapIndex--;

        UpdateMatchMapText();
    }

    public void UpdateMatchMapText()
    {
        string shortenedMapName = localMapList[currentMapIndex].Replace("maps/", "");
        selectedMapName = shortenedMapName;

        if (createLocalGameCanvas.activeSelf)
            createLocalGameMapText.text = shortenedMapName;
        else if (createMatchCanvas.activeSelf)
            mapTextCreateMatch.text = shortenedMapName;
            
    }

    #region Join Match

    private List<GameObject> matchButtonList = new List<GameObject>();
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject matchButtonPrefab;
    [SerializeField] private Transform matchListParent;

    public void RefreshMatchList()
    {
        networkManager = NetworkManager.singleton;

        //Show loading text
        int randomNumber = UnityEngine.Random.Range(0, 10);
        if (randomNumber == 0)
            statusText.text = "Loading quad feed . . .";
        else
            statusText.text = "Loading . . .";
        //Get matches from json
        networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        statusText.text = "";

        //No matches recieved
        if (!success || matchList == null)
        {
            statusText.text = "Couldn't get room list.";
            return;
        }

        //Delete all buttons and clear gameObject list
        ClearMatchList();

        foreach (MatchInfoSnapshot match in matchList)
        {
            //Create buttons and set parent to scroll view
            GameObject matchButtonInstance = Instantiate(matchButtonPrefab);
            matchButtonInstance.transform.SetParent(matchListParent);

            MatchListItem matchListItem = matchButtonInstance.GetComponent<MatchListItem>();
            if (matchListItem != null)
                matchListItem.Setup(match, JoinMatch);
            //Add button to list
            matchButtonList.Add(matchButtonInstance);
        }

        if (matchButtonList.Count == 0)
        {
            statusText.text = "No matches found!";
        }
    }

    private void ClearMatchList()
    {
        //Destroy buttons
        for (int i = 0; i < matchButtonList.Count; i++)
        {
            Destroy(matchButtonList[i]);
        }

        //Remove gameObjects from list
        matchButtonList.Clear();
    }

    public void JoinMatch(MatchInfoSnapshot _match)
    {   
        networkManager = NetworkManager.singleton;

        //Get map name
        Debug.Log("Joining " + _match.name);
        //char[] parsingChars = { '|' };
        //string[] parsedStrings = _match.name.Split(parsingChars);
        //string mapName = parsedStrings[1].Replace(" ","");
        //selectedMapName = mapName;

        //Join match
        networkManager.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        //Debug.Log("Loading map " + mapName);

        joinMatchCanvas.SetActive(false);
        ClearMatchList();
        statusText.text = "Joining " + _match.name + " . . .";
        StartCoroutine(JoinGame(false));
    }

    public void OnJoinMatchBackClicked()
    {
        joinMatchCanvas.SetActive(false);
        matchmakingMenuCanvas.SetActive(true);
    }

    #endregion

    #region Match Making

    public void OnCreateMatchClicked()
    {
        matchmakingMenuCanvas.SetActive(false);
        createMatchCanvas.SetActive(true);

        // Update list with local maps
        localMapList = Directory.GetDirectories("maps/");
        
        UpdateMatchMapText();
    }

    public void OnJoinMatchClicked()
    {
        matchmakingMenuCanvas.SetActive(false);
        joinMatchCanvas.SetActive(true);
        localMapList = Directory.GetDirectories("maps/");
        RefreshMatchList();
    }

    #endregion

    public IEnumerator JoinGame(bool isCreator)
    {
        // Create loading screen
        GameObject loadingScreen = Instantiate(loadingScreenPrefab, Vector3.zero, Quaternion.identity);
        DontDestroyOnLoad(loadingScreen);

        // Wait until loaded into server
        while (!ClientScene.ready)
        {
            // Exit out of searching for match
            if (escapeKey)
            {
                networkManager.StopHost();
                Destroy(loadingScreen);
                multiplayerMenuCanvas.SetActive(true);
                matchmakingMenuCanvas.SetActive(false);
                searchingForGame = false;
                yield break;
            }

            yield return null;
        }
        while (GameManager.instance == null) { yield return null;  }
        yield return new WaitForSeconds(0.2f);

        // Check if correct version to server
        if (!Version.VERSION.Equals(GameManager.instance.version) && !isCreator)
        {
            networkManager.StopClient();
            Destroy(loadingScreen);
            multiplayerMenuCanvas.SetActive(true);
            matchmakingMenuCanvas.SetActive(false);
            searchingForGame = false;
            yield break;
        }

        // Joined server
        searchingForGame = false;

        // Disable save script
        SaveScript.instance.gameObject.SetActive(false);

        // Set game manager map name
        if (isCreator) GameManager.instance.mapName = selectedMapName;

        // Create the map
        uint checksum = MapManager.instance.CreateMap(GameManager.instance.mapName, isCreator, false);
        Debug.Log("Map checksum: " + checksum);

        // Set changed blocks and explosions from server
        if (!isCreator) {
            BlockManager.singleton.SetChangedBlocks();
            BlockManager.singleton.SetChangedExplosions();
        }

        yield return new WaitForSeconds(0.1F);

        // Destroy loading screen
        Destroy(loadingScreen);

        // Add player
        ClientScene.AddPlayer(ClientScene.readyConnection, 0, new PlayerInfoMessage(playerUserName));

        // Correct scoreboard and round info script based on gamemode
        ScoreBoardMenu.instance.CorrectScoreboardBasedOnGamemode();
        RoundInfoScript.singleton.CorrectRoundInfoBasedOnGamemode();

        // Create class selection UI
        ShowClassSelect();
    }

    public void ShowClassSelect()
    {
        // Create weapon select menu
        Instantiate(weaponSelectPrefab, Vector3.zero, Quaternion.identity);

        ClassManager classManager = ClassManager.singleton;
        classManager.Open();
    }

    public void OnMatchmakingClicked()
    {
        networkManager = NetworkManager.singleton;

        if (networkManager.matchMaker == null)
            networkManager.StartMatchMaker();
        networkManager.SetMatchHost("mm.unet.unity3d.com", 443, true);

        matchmakingMenuCanvas.SetActive(true);
        multiplayerMenuCanvas.SetActive(false);
    }

    public void OnMatchmakingBackClicked()
    {
        if (networkManager.matchMaker != null)
            networkManager.StopMatchMaker();

        multiplayerMenuCanvas.SetActive(true);
        matchmakingMenuCanvas.SetActive(false);
    }

    public void OnMultiplayerClicked()
    {
        MenuScript.instance.startMenuCanvas.SetActive(false);

        if (isLoggedIn)
            multiplayerMenuCanvas.SetActive(true);
        else
            AuthenticationScript.instance.loginCanvas.SetActive(true);

        StartCoroutine(MultiplayerClicked());
    }

    public void OnMultiplayerBackClicked()
    {
        MenuScript.instance.startMenuCanvas.SetActive(true);
        multiplayerMenuCanvas.SetActive(false);
    }

    private IEnumerator MultiplayerClicked()
    {
        //If creating username, escape key will exit out
        while (AuthenticationScript.instance.loginCanvas.activeSelf)
        {
            if (escapeKey)
            {
                AuthenticationScript.instance.loginCanvas.SetActive(false);
                MenuScript.instance.startMenuCanvas.SetActive(true);
            }
            yield return null;
        }
    }

    private string joinIp = "Enter IP:", hostIp = "Enter IP:";
    private void OnGUI()
    {
        if (!ClientScene.ready && multiplayerMenuCanvas.activeSelf)
        {
            joinIp = GUI.TextField(new Rect(8, 8, 200, 20), joinIp);

            if (GUI.Button(new Rect(216, 8, 100, 20), "Join Server"))
            {
                networkManager = NetworkManager.singleton;
                networkManager.networkAddress = joinIp;
                networkManager.networkPort = 7777;
                searchingForGame = false;
                networkManager.StartClient();
                multiplayerMenuCanvas.SetActive(false);
                StartCoroutine(JoinGame(false));
            }

            hostIp = GUI.TextField(new Rect(8, 32, 200, 20), hostIp);

            if (GUI.Button(new Rect(216, 32, 100, 20), "Host Server"))
            {
                selectedMapName = "castle";
                networkManager = NetworkManager.singleton;

                networkManager.networkAddress = hostIp;
                networkManager.serverBindToIP = true;
                networkManager.serverBindAddress = hostIp;
                networkManager.networkPort = 7777;

                searchingForGame = false;
                networkManager.StartHost();
                multiplayerMenuCanvas.SetActive(false);
                StartCoroutine(JoinGame(true));
            }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            CursorManagement.CorrectLockMode();
        }
    }

    private void OnApplicationQuit()
    {
        if (NetworkClient.active)
        {
            networkManager.StopHost();
        }
    }
}