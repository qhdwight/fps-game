using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using DG.Tweening;

public class ConsoleScript : MonoBehaviour
{	
	[SerializeField] private GameObject logPrefab, content, canvas;
	[SerializeField] private Scrollbar scrollbar;
	[SerializeField] private InputField input;

	public static ConsoleScript singleton;
	public static bool open;

	private const float smoothScrollTime= 0.5F;

	private CursorLockMode prevLockMode;

	private void Awake()
	{
        if (singleton == null)  
            singleton = this;
        else if (singleton != this)
            Destroy(gameObject);

		DontDestroyOnLoad(this);
	}

	private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

	private void Update()
	{
		if (Input.GetButtonDown("Console"))
		{
			if (open)
				CloseConsole();
			else
				OpenConsole();
		}
	}

	public void ScrollDownButtonPressed()
	{
		DOTween.To
		(
			()=> scrollbar.value,
			x=> scrollbar.value = x,
			0F,
			smoothScrollTime
		);
	}

	public void OpenConsole()
	{
		open = true;

		canvas.SetActive(true);

        CursorManagement.CorrectLockMode();

        // Set focus to the input field
        EventSystem.current.SetSelectedGameObject(input.gameObject);
	}

	public void CloseConsole()
	{
		open = false;
		
		input.text = string.Empty;

		canvas.SetActive(false);

        CursorManagement.CorrectLockMode();

        // Remove focus from the input field
        EventSystem.current.SetSelectedGameObject(null);
	}

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        CreateLog(logString, type);
    }

	private void CreateLog(string logString, LogType type)
	{
		GameObject logInstance = Instantiate(logPrefab, content.transform, false);
		Text logText = logInstance.GetComponent<Text>();

		if (type == LogType.Error || type == LogType.Exception) {
			logText.color = Color.red;
			logText.text += "[Error] ";
		} else if (type == LogType.Warning) {
			logText.color = Color.yellow;
			logText.text += "[Warning] ";
		} else {
			logText.color = Color.white;
			logText.text += "[Info] ";
		}
		logText.text += logString;
	}

	public void OnInput(string command)
	{
		// Check if command is empty
		if (command == string.Empty)
			return;

		string[] args = command.Split(' ');

		switch(args[0]) {
            case "allow_bhop":
                {
                    if (args.Length == 2) {
                        if (args[1] == "help") {
                            Debug.Log("Allows bhop for the server. Usage: allow_bhop [1/0]");
                        }
                        if (args[1  ] == "1" || args[1] == "true") {
                            GameManager.instance.bhopEnabled = true;
                        } else if (args[1] == "0" || args[1] == "false") {
                            GameManager.instance.bhopEnabled = false;
                        }
                    }
                    else {
                        Debug.LogWarning("Usage: allow_bhop [1/0]");
                    }
                    break;
                }
            case "shoot_on_run_allowed":
                {
                    if (args.Length == 2) {
                        if (args[1] == "help") {
                            Debug.Log("Allows running and gunning. Usage: shoot_on_run_allowed [1/0]");
                        }
                        if (args[1] == "1" || args[1] == "true") {
                            GameManager.instance.canShootWhileSprinting = true;
                        }
                        else if (args[1] == "0" || args[1] == "false") {
                            GameManager.instance.canShootWhileSprinting = false;
                        }
                    } else {
                        Debug.LogWarning("Usage: shoot_on_run_allowed [1/0]");
                    }
                    break;
                }
            case "t":
			case "test_match":
			    {
				    if (args.Length == 2) {
					    if (args[1] == "help") {
						    Debug.Log("Starts a test game");
					    }
				    } else {
					    if (!MultiplayerScript.instance.HostTestGame())
						    Debug.LogWarning("Cannot start game, maybe already in one?");
				    }
				    break;
			    }
			case "help":
                {
				    Debug.Log("Available commands: t, test_match, allow_bhop, shoot_on_run_allowed");
				    break;
			    }
			default:
                {
				    Debug.LogWarning("Command \"" + command + "\" not recognized");
				    break;
			    }
		}
	}
}