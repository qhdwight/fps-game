using UnityEngine;
using UnityEngine.UI;

public class Version : MonoBehaviour {

    [SerializeField] private Text versionText;

    public const string PREFIX = "Alpha";
    public const string VERSION = "1.9.6";
    public const string VERSION_NAME = "";

    private void Start()
    {
        versionText.text += PREFIX + " " + VERSION;
        if (VERSION_NAME != string.Empty)
             versionText.text += "    \"" + VERSION_NAME + "\"";
    }
}
