using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class RoundInfoScript : NetworkBehaviour {

    public static RoundInfoScript singleton;
	[SerializeField] public Text roundTimeText;
    [SerializeField] public Text blueScoreText, redScoreText;
    [SerializeField] private GameObject blueScoreGameObject, redScoreGameObject;
	[SyncVar(hook = "UpdateText")] public ushort roundTime;
    [SyncVar(hook = "BlueScoreUpdated")] public ushort blueScore;
    [SyncVar(hook = "RedScoreUpdated")] public ushort redScore;
    [SyncVar] public bool inOvertime;

    public const ushort OVERTIME_TIME_ADD = 60*1;

    public delegate void RoundTimeUp();
    public static RoundTimeUp roundTimeUpDelegate;

    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else if (singleton != this)
            Destroy(gameObject);
    }

    private void Start() {
		roundTime = (ushort)(GameManager.instance.roundTime * 60);
		if (isServer)
        {
            StartCoroutine(CountDown());
        }
        else
        {
            BlueScoreUpdated(blueScore);
            RedScoreUpdated(redScore);
        }
	}

	[Server]
	private IEnumerator CountDown() {
		yield return new WaitForSeconds(1F);
        if (roundTime > 0)
        {
            roundTime--;
            StartCoroutine(CountDown());
        }
        else
        {
            if (redScore != blueScore) {
                roundTimeUpDelegate.Invoke();
            } else {
                roundTime += OVERTIME_TIME_ADD;
                inOvertime = true;
                StartCoroutine(CountDown());
            }
        }
	}

    public void CorrectRoundInfoBasedOnGamemode()
    {
        switch (GameManager.instance.gameMode)
        {
            case GameMode.CaptureTheFlag: case GameMode.Conquest: { break; }
            default: {
                    blueScoreGameObject.SetActive(false);
                    redScoreGameObject.SetActive(false);
                    break;
                }
        }
    }

    private void UpdateText(ushort time)
    {
        roundTimeText.text = string.Empty;
        if (inOvertime) roundTimeText.text += "+";
        roundTimeText.text += SecondsToString(time);
    }

    private void BlueScoreUpdated(ushort val)
    {
        blueScoreText.text = val.ToString();
    }

    private void RedScoreUpdated(ushort val)
    {
        redScoreText.text = val.ToString();
    }

	//[ClientRpc]
	//private void RpcUpdateTime(int _roundTime) {
	//	roundTime = _roundTime;
	//	//Update text
	//	roundTimeText.text = SecondsToString(roundTime);
	//}

	private string SecondsToString(ushort _seconds) {
		int seconds = _seconds % 60;
		int minutes = _seconds / 60;
		string result = minutes + " : ";
		if (seconds < 10)
			result += "0";
		return result += seconds;
	}
}
