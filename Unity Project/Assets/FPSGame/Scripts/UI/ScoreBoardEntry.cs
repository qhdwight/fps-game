using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardEntry : MonoBehaviour {

	public Text usernameText, killsText, deathsText, pingText;

	public ScoreBoardEntryInfo info;

	public struct ScoreBoardEntryInfo {
        public uint playerNetId;
		public int kills, deaths, ping;
		public string username;
		public Color col;

		public ScoreBoardEntryInfo(int kills, int deaths, int ping, uint playerNetId, string username, Color col) {
			this.kills = kills;
			this.deaths = deaths;
			this.ping = ping;
			this.username = username;
            this.playerNetId = playerNetId;
			this.col = col;
		}
	}

	public void UpdateText() {
        usernameText.text = info.username;
		killsText.text= info.kills.ToString();
		deathsText.text = info.deaths.ToString();
		pingText.text = info.ping.ToString();
	}
}
