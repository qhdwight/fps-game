using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MyNetworkManager : NetworkManager
{
    public MyNetworkManager()
        : base()
    {
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader reader)
    {
        GameObject player = Instantiate(playerPrefab);
        Player playerScript = player.GetComponent<Player>();
        PlayerSetup playerSetup = player.GetComponent<PlayerSetup>();

        // Set team and username
        playerSetup.username = reader.ReadString();
        playerScript.team = GameManager.RequestTeam();

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        Debug.Log("Player added");
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        MultiplayerScript.instance.multiplayerMenuCanvas.SetActive(true);
    }

    public Transform GetSpawnPosition(Team team)
    {
        List<Transform> teamSpawns = new List<Transform>();
        foreach (Transform spawn in startPositions)
        {
            if (team == spawn.GetComponent<MyNetworkStartPosition>().Team)
                teamSpawns.Add(spawn);
        }
        if (teamSpawns.Count == 0)
        {
            return startPositions[0];
        }
        else
        {
            int randIndex = Random.Range(0, teamSpawns.Count);
            return teamSpawns[randIndex];
        }
    }
}