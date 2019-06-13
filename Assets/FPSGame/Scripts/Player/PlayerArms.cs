using UnityEngine;

public class PlayerArms : MonoBehaviour {

    private PlayerWeaponManager weaponManager;
    private PlayerUse playerUse;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        weaponManager = GetComponentInParent<PlayerWeaponManager>();
        playerUse = GetComponentInParent<PlayerUse>();
    }

    public void HideProjectileOnWeapon()
    {
        weaponManager.currentItemInstance.GetComponent<StraightProjectileGraphics>().HideProjectileOnWeapon();
    }

    public void ShowProjectileOnWeapon()
    {
        weaponManager.currentItemInstance.GetComponent<StraightProjectileGraphics>().ShowProjectileOnWeapon();
    }

    public void HideGrenade()
    {
        SetGrenadeVisible(false);
    }

    public void ShowGrenade()
    {
        SetGrenadeVisible(true);
    }

    public void SetGrenadeVisible(bool visible)
    {
        if (weaponManager.currentPlayerItem is PlayerGrenade)
            (weaponManager.currentPlayerItem as PlayerGrenade).grenadeModel.SetActive(visible);
    }
}
