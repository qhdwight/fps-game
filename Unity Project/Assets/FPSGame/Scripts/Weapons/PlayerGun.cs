using UnityEngine;

public class PlayerGun : PlayerItem {

    public int damage;
    public float range;
    public float recoil;
    public int clipSize;
    public int currentAmmo;
    public int ammountOfClipsLeft = -1;
    public bool isReadyToUse = true;
    public float reloadTime;
    public bool canUseOnRun;
    public bool isGun;
    public bool isSniper;
    public bool isShotgun;
    public bool isLauncherRaycast;
    public bool canEditBlocks;
    public bool dontShowCrosshair;
    public float shotDelay;

    public void FillAmmo() {
        currentAmmo = clipSize;
    }

    public bool IsAmmoFull() {
        return currentAmmo == clipSize;
    }

    public bool AreClipsLeft() {
        return (ammountOfClipsLeft == -1 || ammountOfClipsLeft > 0);
    }

    public void RemoveClip() {
        if (ammountOfClipsLeft != -1)
            ammountOfClipsLeft -= 1;
    }

    public int GetAmmoLeft() {
        return currentAmmo;
    }

    public int GetClipSize() {
        return clipSize;
    }
}
