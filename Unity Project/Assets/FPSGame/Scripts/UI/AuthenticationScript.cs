using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DatabaseControl;

public class AuthenticationScript : MonoBehaviour {

    public static AuthenticationScript instance;

    public string playerUsername, playerPassword;
    public static string loggedInPlayerName, loggedInPlayerPassword, currentPlayerData;

    [SerializeField] private GameObject multiplayerCanvas;

    [Header("Login")]
    [SerializeField] public GameObject loginCanvas;
    [SerializeField] private Text loginErrorText;
    [SerializeField] private Image loginLoadingImage;

    [Header("Registration")]
    [SerializeField] private GameObject registerCanvas;
    [SerializeField] private Text registrationErrorText;
    [SerializeField] private Image registrationLoadingImage;

    private const string ERROR_HEADER = "[Error] ";
    private const float ROTATE_SPEED = 200F;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void OnLoginRegister()
    {
        registerCanvas.SetActive(true);
        loginCanvas.SetActive(false);
    }

    private void Update()
    {
        if (loginLoadingImage.gameObject.activeSelf)
        {
            loginLoadingImage.rectTransform.Rotate(0F, 0F, ROTATE_SPEED * Time.deltaTime);
        }
        else if (registrationLoadingImage.gameObject.activeSelf)
        {
            registrationLoadingImage.rectTransform.Rotate(0F, 0F, ROTATE_SPEED * Time.deltaTime);
        }
    }

    #region Login

    public void OnLogin()
    {
        loginLoadingImage.gameObject.SetActive(true);
        StartCoroutine(LoginUser(playerUsername, playerPassword));
    }

    public void OnLoginUsernameEntered(string playerUsername)
    {
        this.playerUsername = playerUsername;
    }

    public void OnLoginPasswordEntered(string playerPassword)
    {
        this.playerPassword = playerPassword;
    }

    public void OnBackLoginPressed()
    {
        loginCanvas.SetActive(false);
        MenuScript.instance.startMenuCanvas.SetActive(true);
    }

    private IEnumerator LoginUser(string playerUsername, string playerPassword)
    {
        // Send request to login, providing username and password
        IEnumerator e = DCF.Login(playerUsername, playerPassword);
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        // The returned string from the request
        string response = e.Current as string;

        // Get rid of loading button
        loginLoadingImage.gameObject.SetActive(false);

        if (response == "Success")
        {
            /* Successful login */

            // Tell multiplayer script we are logged in
            MultiplayerScript.isLoggedIn = true;
            // Set logged in data
            loggedInPlayerName = playerUsername; loggedInPlayerPassword = playerPassword;
            // Get rid of error text
            loginErrorText.gameObject.SetActive(false);
            // Get data
            StartCoroutine(GetData(playerUsername, playerPassword));
            // Go to multiplayer main menu
            loginCanvas.SetActive(false);
            MultiplayerScript.instance.playerUserName = playerUsername;
            MultiplayerScript.instance.multiplayerMenuCanvas.SetActive(true);
        }
        else
        {
            /* Something went wrong logging in */
            if (response == "UserError")
            {
                /* The Username was wrong so display relevent error message */

                loginErrorText.gameObject.SetActive(true);
                loginErrorText.text = ERROR_HEADER + "Username not Found";
            }
            else
            {
                if (response == "PassError")
                {
                    /* The Password was wrong so display relevent error message */

                    loginErrorText.gameObject.SetActive(true);
                    loginErrorText.text = ERROR_HEADER + "Password Incorrect";
                }
                else
                {
                    /* There was another error. This error message should never appear, but is here just in case. */

                    loginErrorText.gameObject.SetActive(true);
                    loginErrorText.text = ERROR_HEADER + "Unknown Error. Please try again later.";
                }
            }
        }
    }

    #endregion

    #region Registration

    public void OnRegister()
    {
        // Show loading button
        registrationLoadingImage.gameObject.SetActive(true);

        StartCoroutine(RegisterUser(playerUsername, playerPassword));
    }

    public void OnRegisterUsernameEntered(string playerUsername)
    {
        this.playerUsername = playerUsername;
    }

    public void OnRegisterPasswordEntered(string playerPassword)
    {
        this.playerPassword = playerPassword;
    }

    public void OnBackRegisterPressed()
    {
        registerCanvas.SetActive(false);
        loginCanvas.SetActive(true);
    }

    private IEnumerator RegisterUser(string playerUsername, string playerPassword)
    {
        IEnumerator e = DCF.RegisterUser(playerUsername, playerPassword, string.Empty); // << Send request to register a new user, providing submitted username and password. It also provides an initial value for the data string on the account, which is "Hello World".
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        // The returned string from the request
        string response = e.Current as string;

        // Get rid of loading button
        registrationLoadingImage.gameObject.SetActive(false);

        if (response == "Success")
        {
            /* Username and Password were valid. Account has been created */

            // Tell multiplayer script we are logged in
            MultiplayerScript.isLoggedIn = true;
            // Set logged in data
            loggedInPlayerName = playerUsername; loggedInPlayerPassword = playerPassword;
            // Get rid of error text
            registrationErrorText.gameObject.SetActive(false);
            // Get data
            StartCoroutine(GetData(playerUsername, playerPassword));
            // Go to multiplayer main menu
            registerCanvas.SetActive(false);
            MultiplayerScript.instance.playerUserName = playerUsername;
            MultiplayerScript.instance.multiplayerMenuCanvas.SetActive(true);
        }
        else
        {
            /* Something went wrong logging in */
            if (response == "UserError")
            {
                /* The username has already been taken. Player needs to choose another. */

                registrationErrorText.gameObject.SetActive(true);
                registrationErrorText.text = ERROR_HEADER + "Username Already Taken";
            }
            else
            {
                /* There was another error. This error message should never appear, but is here just in case. */

                registrationErrorText.gameObject.SetActive(true);
                registrationErrorText.text = ERROR_HEADER + "Unknown Error. Please try again later.";
            }
        }
    }

    #endregion

    #region Set/Get Data

    private IEnumerator GetData(string playerUsername, string playerPassword)
    {
        IEnumerator e = DCF.GetUserData(playerUsername, playerPassword); // << Send request to get the player's data string. Provides the username and password
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        // The returned string from the request
        string response = e.Current as string;

        if (response == "Error")
        {
            /* There was another error */
        }
        else
        {
            /* The player's data was retrieved */

            currentPlayerData = response;
        }
    }

    private IEnumerator SetData(string playerUsername, string playerPassword, string data)
    {
        IEnumerator e = DCF.SetUserData(playerUsername, playerPassword, data); // << Send request to set the player's data string. Provides the username, password and new data string
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        // The returned string from the request
        string response = e.Current as string;

        if (response == "Success")
        {
            /* The data string was set correctly */
        }
        else
        {
            /* There was another error */
        }
    }
    
    #endregion

    private void OnGUI()
    {
        if (loginCanvas.activeSelf)
        {
            if (GUI.Button(new Rect(8F, 8F, 200F, 20F), "Skip Login"))
            {
                loginCanvas.SetActive(false);
                MultiplayerScript.instance.multiplayerMenuCanvas.SetActive(true);
            }
        }
    }
}
