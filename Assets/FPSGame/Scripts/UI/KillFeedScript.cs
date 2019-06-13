using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class KillFeedScript : NetworkBehaviour {

	[SerializeField] private GameObject killFeedPrefab, killFeedParent;

	public static KillFeedScript instance;

	public static float killFeedDestroyTime = 5f;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
	}   

	[Server]
	public void CreateKillFeed(uint shooterID, uint dierID, string weapon, bool headshot) {
        //Get Player scripts based off of player network ID's
		string shooter = GameManager.GetPlayer(shooterID).GetComponent<PlayerSetup>().username;
		string dier = GameManager.GetPlayer(dierID).GetComponent<PlayerSetup>().username;
		string killFeed = shooter + " (" + weapon + ") ";
		//Add text if headshot
		if (headshot)
			killFeed += "(headshot) ";
		killFeed += dier;
		RpcCreateKillFeed(killFeed);
	}

	[ClientRpc]
	private void RpcCreateKillFeed(string killFeed) {
        // Create kill feed object
		GameObject killFeedInstance = Instantiate(killFeedPrefab);

        // Set it to red if it involves the player
		if (killFeed.Contains(MultiplayerScript.instance.playerUserName))
        	killFeedInstance.GetComponentInChildren<Image>().sprite = killFeedInstance.GetComponent<KillFeedEntry>().redImage;

        // Make it appear correctly
		killFeedInstance.transform.SetParent(killFeedParent.transform);
        killFeedInstance.transform.localScale = new Vector3(1f, 1.25f, 1f);

        // Get and update text
		Text killFeedText = killFeedInstance.GetComponent<Text>();
		killFeedText.text = killFeed;

        // Destroy the kill feed after the amount of time set
        Destroy(killFeedInstance, killFeedDestroyTime);
	}
}
