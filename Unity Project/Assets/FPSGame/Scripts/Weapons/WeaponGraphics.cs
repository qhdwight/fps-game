using UnityEngine;

public class WeaponGraphics : MonoBehaviour {

    private PlayerGun playerWeapon;
    public ParticleSystem muzzleFlash;
    public GameObject bloodSplatterPrefab, impactEffectPrefab, tracerEffectPrefab;

    void Start()
    {
        playerWeapon = GetComponent<PlayerGun>();
    }
}
