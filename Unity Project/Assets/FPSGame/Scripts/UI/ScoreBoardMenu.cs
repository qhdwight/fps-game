using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ScoreBoardMenu : NetworkBehaviour {

    public static ScoreBoardMenu instance;

    [SerializeField] public GameObject scoreBoardCanvas, scoreBoardEntryPrefab, joinTeamContainer;
    [SerializeField] private GameObject redScoreBoard, blueScoreBoard, singleScoreBoard;
    [SerializeField] public Transform redScoreBoardContent, blueScoreBoardContent, singleScoreBoardContent;

    private List<ScoreBoardEntry> scoreBoardEntries = new List<ScoreBoardEntry>();

    public static bool scoreBoardIsOpen;

    public bool scoreboardKeyDown = false, scoreboardKeyUp = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Update()
    {
        if (GameManager.players.Count > 0)
        {
            scoreboardKeyDown = Input.GetButtonDown("Scoreboard");
            scoreboardKeyUp = Input.GetButtonUp("Scoreboard");

            if (scoreboardKeyDown)
            {
                scoreBoardIsOpen = true;
                scoreBoardCanvas.SetActive(true);
            }
            else if (scoreboardKeyUp)
            {
                scoreBoardIsOpen = false;
                scoreBoardCanvas.SetActive(false);

                CursorManagement.CorrectLockMode();
            }

            if (scoreBoardIsOpen)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    CursorManagement.UnlockCursor();
                }
            }
        }
    }

    public void OnJoinRed()
    {
        GameManager.localPlayer.SendTeamSwitchRequest(Team.Red);
    }

    public void OnJoinBlue()
    {
        GameManager.localPlayer.SendTeamSwitchRequest(Team.Blue);
    }

    public void CorrectScoreboardBasedOnGamemode()
    {
        switch (GameManager.instance.gameMode)
        {
            case GameMode.CaptureTheFlag:
            case GameMode.Conquest:
                {
                    redScoreBoard.SetActive(true);
                    blueScoreBoard.SetActive(true);
                    joinTeamContainer.SetActive(true);
                    break;
                }
            default:
                {
                    singleScoreBoard.SetActive(true);
                    break;
                }
        }
    }

	public void AddScoreboardEntry(uint playerNetId, Color color)
    {
        // Get player with given ID
        Player player = GameManager.GetPlayer(playerNetId);
        PlayerSetup playerSetup = player.GetComponent<PlayerSetup>();

        if (player != null && GetScoreBoardEntryByID(playerNetId) == null)
        {
            // Create prefab
            GameObject scoreBoardEntryInstance = Instantiate(scoreBoardEntryPrefab);

            Transform parentTransform = GetTransformFromTeam(player.team);
            scoreBoardEntryInstance.transform.SetParent(parentTransform);

            ScoreBoardEntry scoreBoardEntry = scoreBoardEntryInstance.GetComponent<ScoreBoardEntry>();

            // Add to list
            scoreBoardEntries.Add(scoreBoardEntry);

            // Set colors
            scoreBoardEntry.usernameText.color = color;
            scoreBoardEntry.killsText.color = color;
            scoreBoardEntry.deathsText.color = color;
            scoreBoardEntry.pingText.color = color;

            // Get and set data on scoreboard entry
            scoreBoardEntry.info.playerNetId = playerNetId;
            scoreBoardEntry.info.kills = player.kills;
            scoreBoardEntry.info.deaths = player.deaths;
            scoreBoardEntry.info.username = playerSetup.username;
            scoreBoardEntry.info.col = color;

            // Update scoreboard UI text component
            scoreBoardEntry.UpdateText();
        }
    }

    public Transform GetTransformFromTeam(Team team)
    {
            Transform parentTransform = null;
            switch(team) {
                case Team.Red: {
                        parentTransform = redScoreBoardContent;
                        break;
                    }
                case Team.Blue: {
                        parentTransform = blueScoreBoardContent;
                        break;
                    }
                default: {
                        if (singleScoreBoard.gameObject.activeSelf)
                            parentTransform = singleScoreBoardContent;
                        break;
                    }
            }
            return parentTransform;
    }

    public void ChangeScoreboardEntryTeam(uint playerNetId, Team newTeam)
    {
        // Get the entry
        ScoreBoardEntry entry = GetScoreBoardEntryByID(playerNetId);

        if (entry == null)
            return;

        // Get the proper tranform for the scoreboard
        Transform parentTransform = GetTransformFromTeam(newTeam);

        // Set the parent
        entry.transform.SetParent(parentTransform);
    }

    public void SetKillValue(int kills, uint playerNetId)
    {
        ScoreBoardEntry scoreBoardEntry = GetScoreBoardEntryByID(playerNetId);
        if (scoreBoardEntry != null)
        {
            scoreBoardEntry.info.kills = kills;
            scoreBoardEntry.killsText.text = scoreBoardEntry.info.kills.ToString();
        }
    }

    public void SetDeathsValue(int deaths, uint playerNetId)
    {
        ScoreBoardEntry scoreBoardEntry = GetScoreBoardEntryByID(playerNetId);
        if (scoreBoardEntry != null)
        {
            scoreBoardEntry.info.deaths = deaths;
            scoreBoardEntry.deathsText.text = scoreBoardEntry.info.deaths.ToString();
        }
    }
		
    public void DestroyScoreboardEntry(uint playerNetId)
    {
		ScoreBoardEntry entry = GetScoreBoardEntryByID(playerNetId);
        scoreBoardEntries.Remove(entry);
		Destroy(entry);
    }

	public string GetUsernameByID(uint playerNetId)
    {
		return GameManager.GetPlayer(playerNetId).GetComponent<PlayerSetup>().username;
	}

    public ScoreBoardEntry GetScoreBoardEntryByID(uint playerNetId)
    {
        foreach (ScoreBoardEntry scoreBoardEntry in scoreBoardEntries)
        {
			if (scoreBoardEntry.info.playerNetId == playerNetId)
            {
                return scoreBoardEntry;
            }
        }
        return null;
    }
}
