using UnityEngine;

public class PlayerGameSettings : MonoBehaviour {

    [SerializeField]
    private CameraEffectManager camManager;

    private PlayerMultiplayerMotor playerMotor;
    private PlayerUse playerUse;

    private void Start()
    {
        camManager = GetComponentInChildren<CameraEffectManager>();
        playerMotor = GetComponent<PlayerMultiplayerMotor>();
        playerUse = GetComponent<PlayerUse>();
        UpdateSettings(GetSettings());
    }

    public Settings GetSettings()
    {
        return SettingsScript.instance.currentSettings;
    }

    public void UpdateSettings(Settings settings)
    {
        // Apply all of the settings
        playerMotor.sensitivityX = settings.mouseSensitivity;
        playerMotor.sensitivityY = settings.mouseSensitivity;
        playerUse.autoScope = settings.autoScopeEnabled;
    }

}
