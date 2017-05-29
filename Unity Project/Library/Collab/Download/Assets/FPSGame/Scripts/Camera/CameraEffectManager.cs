using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(SunShafts))]
[RequireComponent(typeof(ScreenSpaceAmbientOcclusion))]
[RequireComponent(typeof(Bloom))]
[RequireComponent(typeof(CameraMotionBlur))]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CameraShake))]
public class CameraEffectManager : MonoBehaviour {

    public static List<CameraEffectManager> instances = new List<CameraEffectManager>();

    private SunShafts godRays;
    private ScreenSpaceAmbientOcclusion ambientOcclusion;
    private Bloom bloom;
    private CameraMotionBlur motionBlur;
    private CameraShake camShake;

    public bool shouldShake = false;

    public static void UpdateCameraEffectManagers(Settings settings)
    {
        foreach (CameraEffectManager manager in instances)
        {
            manager.UpdateSettings(settings);
        }
    }

    public static void ShakeCameras() {
        
        foreach (CameraEffectManager manager in instances)
        {
            if (manager.gameObject.activeSelf && manager.shouldShake)
                manager.camShake.ShakeCamera(8f, 0.3f);
        }
    }

    private void Awake()
    {
        // Add to the list of camera managers
        instances.Add(this);

        // Add all of the effects to the camera
        godRays = GetComponent<SunShafts>();
        ambientOcclusion = GetComponent<ScreenSpaceAmbientOcclusion>();
        bloom = GetComponent<Bloom>();
        motionBlur = GetComponent<CameraMotionBlur>();
        camShake = GetComponent<CameraShake>();
    }

    public void Start()
    {
        UpdateSettings(SettingsScript.instance.currentSettings);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }

    public void UpdateSettings(Settings settings)
    {
        // Disable or enable the image settings components
        godRays.enabled = settings.godRaysEnabled;
        bloom.enabled = settings.bloomEnabled;
        motionBlur.enabled = settings.motionBlurEnabled;
        ambientOcclusion.enabled = settings.ambientOcclusionEnabled;
        shouldShake = settings.camShakeEnabled;
    }
}
