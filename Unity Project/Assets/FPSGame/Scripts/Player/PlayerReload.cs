using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class PlayerReload : NetworkBehaviour
{

    public bool canReload = true;
    private PlayerUse playerUse;
    private PlayerWeaponManager weaponManager;
    private PlayerMultiplayerMotor playerMotor;
    [SerializeField]
    public GameObject playerGraphicArmature;
    private bool reloadKeyDown, reloadKey;

    private const float RELOAD_BLEND_TIME = 0.3F;

    public bool Reloading { get { return !canReload; } }

    private IEnumerator currentReload;

    private void Awake()
    {
        playerUse = GetComponent<PlayerUse>();
        weaponManager = GetComponent<PlayerWeaponManager>();
        playerMotor = GetComponent<PlayerMultiplayerMotor>();
    }

    private void Update()
    {
        reloadKey = Input.GetButton("Reload");
        reloadKeyDown = Input.GetButtonDown("Reload");

        if (reloadKeyDown && isLocalPlayer && weaponManager.currentPlayerItem is PlayerGun)
        {
            Reload(weaponManager.currentPlayerItem as PlayerGun);
        }
    }

    [Command]
    private void CmdReloadEffect()
    {
        RpcReloadEffect();
    }

    [ClientRpc]
    private void RpcReloadEffect()
    {
        if (!isLocalPlayer)
            ReloadEffect();
    }

    private void ReloadEffect()
    {
        if (weaponManager.currentPlayerItem is PlayerGun)
        {
            // Get current weapon
            PlayerGun currentGun = weaponManager.currentPlayerItem as PlayerGun;

            //Play reload animation for weapon graphic
            string reloadName = currentGun.itemName + "Reload";
            if (currentGun.isShotgun) reloadName += "1";
            weaponManager.playerArmsAnimator.CrossFade(reloadName, RELOAD_BLEND_TIME);

            // Play reloading sound if it exists
            PlayReloadingSound(currentGun);
        }
    }

    public void Reload(PlayerGun startingReloadWeapon)
    {
        // Choose correct loading for the gun
        if (startingReloadWeapon.isShotgun)
        {
            currentReload = ShotgunReload(startingReloadWeapon);
            StartCoroutine(currentReload);
        }
        else if (startingReloadWeapon.isGun || startingReloadWeapon.isLauncherRaycast)
        {
            currentReload = OnReload(startingReloadWeapon);
            StartCoroutine(currentReload);
        }
    }

    public void CancelReload()
    {
        // Cancel the reloading enumerator
        StopCoroutine(currentReload);

        // Player can reload again
        canReload = true;
    }

    private IEnumerator ShotgunReload(PlayerGun startingReloadWeapon)
    {
        if (!canReload || startingReloadWeapon.currentAmmo == startingReloadWeapon.clipSize)
            yield break;

        // Do the reloading sequences while are ammo is not full
        do
        {
            // Disable scope if active
            playerUse.SetScope(false);

            // Make sure you cannot reload until current reload is done
            canReload = false;

            // Play reload animation for weapon viewmodel
            weaponManager.playerArmsAnimator.CrossFade(startingReloadWeapon.itemName + "Reload1", 0.15F);

            // Play reloading sound if it exists
            PlayReloadingSound(startingReloadWeapon);

            // Do reload effect on other instances
            CmdReloadEffect();

            // Wait until current weapon's reload time is over
            yield return new WaitForSeconds(startingReloadWeapon.reloadTime);

            /* Done reloading: */

            // Player can now reload again
            canReload = true;

            // Add one ammo since it is a shotgun
            if (weaponManager.currentPlayerItem == startingReloadWeapon)
                startingReloadWeapon.currentAmmo += 1;
        }
        while (canReload && !startingReloadWeapon.IsAmmoFull());

        /* Done with reloading sequence */

        if (startingReloadWeapon.IsAmmoFull())
            weaponManager.playerArmsAnimator.CrossFade(startingReloadWeapon.itemName + "Reload2", 0.15F);

        yield return new WaitForSeconds(1F);

        // Set running animation if running
        if (playerMotor.running)
            playerUse.SetRunningAnimation();
    }

    private void PlayReloadingSound(PlayerGun playerWeapon)
    {
        try
        {
            weaponManager.currentItemInstance.GetComponents<AudioSource>()[(int)GunSound.Reload].Play();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Nonexistent reload sound tried to play");
        }
    }

    public IEnumerator OnReload(PlayerGun startingReloadWeapon)
    {
        if (canReload && !startingReloadWeapon.IsAmmoFull() && startingReloadWeapon.AreClipsLeft())
        {
            // Disable scope if active
            playerUse.SetScope(false);

            // Make sure you cannot reload until current reload is done
            canReload = false;

            // Play reload animation for weapon viewmodel
            weaponManager.playerArmsAnimator.CrossFade(weaponManager.currentPlayerItem.itemName + "Reload", RELOAD_BLEND_TIME);

            // Play reloading sound
            weaponManager.currentItemInstance.GetComponents<AudioSource>()[2].Play();

            // Do reload effect on other instances
            CmdReloadEffect();

            // Wait until current weapon's reload time is over
            yield return new WaitForSeconds(startingReloadWeapon.reloadTime);

            /* Done reloading: */

            // Player can now reload again
            canReload = true;

            // Refill ammo if the same gun that was used to start reloading
            if (weaponManager.currentPlayerItem == startingReloadWeapon) {
                startingReloadWeapon.FillAmmo();
                startingReloadWeapon.RemoveClip();
            }

            // Set running animation if running
            if (playerMotor.running)
                playerUse.SetRunningAnimation();
        }
    }
}
