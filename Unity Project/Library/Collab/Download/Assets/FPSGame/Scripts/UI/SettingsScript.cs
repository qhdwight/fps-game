using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public struct Settings
{
    // Graphics settings
    public bool godRaysEnabled, bloomEnabled, motionBlurEnabled, ambientOcclusionEnabled, camShakeEnabled;

    public bool autoScopeEnabled;
    public float mouseSensitivity;
    public float volume;
    public int fpsCap;

    public static Settings Default()
    {
        Settings s = new Settings();
        s.mouseSensitivity = 1f;
        s.volume = 0.5f;
        s.fpsCap = 200;
        return s;
    }
}

public class SettingsScript : MonoBehaviour
{
    public static SettingsScript instance;

    public static Scene menuScene;

    public Behaviour godRayComponent;

    [SerializeField] private GameObject settingsMenuCanvas;

    private GameObject[] canvases;

    public GameObject localPlayer;

    public static bool settingsAreOpen = false;

    public static string saveSettingsFolderName = "settings/";
    public static string saveSettingsFileName = saveSettingsFolderName + "settings.json";

    //---------------Settings UI objects---------------

    [SerializeField] private Slider sensitivitySlider, volumeSlider, fpsCapSlider;
    [SerializeField] private Toggle godRayToggle, bloomToggle, motionBlurToggle, ambientOcclusionToggle, autoScopeToggle, camShakeToggle;

    //---------------Settings variables----------------

    public Settings currentSettings;
    public Settings newSettings;

    //-------------------------------------------------

    private CursorLockMode prevLockMode;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

     private void OnEnable()
    {
        PlayerSetup.localPlayerCreatedDelegate += LocalPlayerCreated;
    }

    private void OnDisable()
    {
        PlayerSetup.localPlayerCreatedDelegate -= LocalPlayerCreated;
    }

    private void LocalPlayerCreated(Player player)
    {
        localPlayer = player.gameObject;
    }
       
    private void Start()
    {
        menuScene = SceneManager.GetActiveScene();

        //Load settings store in game install path
        LoadSettings();

        // Adjust volume
        AudioListener.volume = currentSettings.volume;
        // Adjust target frame rate
        Application.targetFrameRate = currentSettings.fpsCap;
    }

    private void SetOtherCanvasesState(GameObject[] canvases, bool isEnabled)
    {
        if (canvases == null)
            return;

        foreach (GameObject canvas in canvases)
        {
            canvas.SetActive(isEnabled);
        }

        if (EscapeMenuScript.instance != null)
            EscapeMenuScript.instance.gameObject.SetActive(isEnabled);
    }

    public void OnVolumeChanged(float volume)
    {
        SetSliderGraphics(volumeSlider, "Volume", volume, 1, false);
        newSettings.volume = volume;
    }

    public void OnFPSCapChanged(float val)
    {
        int cap = Mathf.RoundToInt(val);
        SetSliderGraphics(fpsCapSlider, "FPS Cap", cap, 0, false);
        newSettings.fpsCap = cap;
    }

    public void OnSensitivityChanged(float sensitivity)
    {
        SetSliderGraphics(sensitivitySlider, "Sensitivity", sensitivity, 1);
        newSettings.mouseSensitivity = sensitivity;
    }

    public void OnGodRayToggled(bool enabled)
    {
        newSettings.godRaysEnabled = enabled;
    }

    public void OnBloomToggled(bool enabled)
    {
        newSettings.bloomEnabled = enabled;
    }

    public void OnMotionBlurToggled(bool enabled)
    {
        newSettings.motionBlurEnabled = enabled;
    }

    public void OnAmbientOcclusionToggled(bool _enabled)
    {
        newSettings.ambientOcclusionEnabled = _enabled;
    }

    public void OnAutoScopeToggled(bool _enabled)
    {
        newSettings.autoScopeEnabled = _enabled;
    }

    public void OnCamShakeToggled(bool _enabled)
    {
        newSettings.camShakeEnabled = _enabled;
    }

    private void SettingsUpdated()
    {
        currentSettings = newSettings;

        // Update settings on player and all cameras
        CameraEffectManager.UpdateCameraEffectManagers(newSettings);
        if (localPlayer != null)
            localPlayer.GetComponent<PlayerGameSettings>().UpdateSettings(newSettings);

        // Adjust volume
        AudioListener.volume = currentSettings.volume;
        // Adjust target frame rate
        Application.targetFrameRate = currentSettings.fpsCap;
        Debug.Log("Settings updated!");
    }

    public void OnOpenSettings()
    {
        newSettings = currentSettings;

        UpdateSettingGraphics();

        canvases = GameObject.FindGameObjectsWithTag("Canvas");
        SetOtherCanvasesState(canvases, false);
        settingsMenuCanvas.SetActive(true);
        settingsAreOpen = true;

        CursorManagement.CorrectLockMode();
    }

    public void UpdateSettingGraphics()
    {
        /* Update all the graphics of the UI components */

        SetToggleGraphics(godRayToggle, currentSettings.godRaysEnabled);
        SetToggleGraphics(bloomToggle, currentSettings.bloomEnabled);
        SetToggleGraphics(motionBlurToggle, currentSettings.motionBlurEnabled);
        SetToggleGraphics(ambientOcclusionToggle, currentSettings.ambientOcclusionEnabled);
        SetToggleGraphics(camShakeToggle, currentSettings.camShakeEnabled);

        SetToggleGraphics(autoScopeToggle, currentSettings.autoScopeEnabled);

        SetSliderGraphics(volumeSlider, "Volume", currentSettings.volume, 1);
        SetSliderGraphics(sensitivitySlider, "Sensitivity", currentSettings.mouseSensitivity, 1);
        SetSliderGraphics(fpsCapSlider, "FPS Cap", currentSettings.fpsCap, 0);
    }

    public void SetToggleGraphics(Toggle toggle, bool isOn)
    {
        toggle.isOn = isOn;
    }

    public void SetSliderGraphics(Slider slider, string text, float val, int roundAmount, bool correctVal = true)
    {
        slider.GetComponentInChildren<Text>().text = text + ": " + Math.Round(val, roundAmount).ToString();
        if (correctVal)
            slider.value = val;
    }

    public void OnCloseSettings()
    {
        CloseSettings();
    }

    public void OnApplySettingsClicked()
    {
        SettingsUpdated();
        SaveSettings();
        CloseSettings();
    }

    private void SaveSettings()
    {
        // Check if directory exists and if not make one
        if (!Directory.Exists(saveSettingsFolderName))
            Directory.CreateDirectory(saveSettingsFolderName);

        // Turn the object into json
        string json = JsonUtility.ToJson(currentSettings);
        json = Util.FormatJSON(json);

        try
        {
            // Try to write the json string to the json file
            File.WriteAllText(saveSettingsFileName, json);
        }
        catch (Exception e)
        {
            // Log the warning
            Debug.LogWarning(e);
        }
    }

    private void LoadSettings()
    {
        // Check if file exists. If not, make a new one
        if (!File.Exists(saveSettingsFileName))
        {
            // Create new settings save
            SaveDefaultSettings();
            return;
        }

        // Try to get one big string from the json file
        string jsonFile = File.ReadAllText(saveSettingsFileName);

        try
        {
            // Try to read the string json to a object
            currentSettings = JsonUtility.FromJson<Settings>(jsonFile);
        }
        catch (Exception e)
        {
            // Log the warning
            Debug.LogWarning(e);

            // Overwrite bad file
            SaveDefaultSettings();
        }
    }

    public void SaveDefaultSettings()
    {
        currentSettings = Settings.Default();
        SaveSettings();
        Debug.Log("Default settings created");
    }

    public void CloseSettings()
    {
        settingsMenuCanvas.SetActive(false);
        SetOtherCanvasesState(canvases, true);
        settingsAreOpen = false;

        CursorManagement.CorrectLockMode();
    }
}
