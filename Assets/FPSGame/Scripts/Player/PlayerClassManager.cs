using UnityEngine;
using UnityEngine.Networking;

public class PlayerClassManager : NetworkBehaviour {

    private PlayerWeaponManager weaponManager;

    [SyncVar] public PlayerClass currentClass;

    private void Awake()
    {
        weaponManager = GetComponent<PlayerWeaponManager>();
    }

    public void SetCurrentClassInAction()
    {
        SetClass(currentClass, true);
    }

    public void OnSetClass(PlayerClass playerClass)
    {
        CmdSetClass(playerClass);
    }

    [Command]
    private void CmdSetClass(PlayerClass playerClass)
    {
        currentClass = playerClass;
        RpcSetClass(playerClass);
    }

    [ClientRpc]
    private void RpcSetClass(PlayerClass playerClass)
    {
        SetClass(playerClass);
    }

    public void SetClass(PlayerClass playerClass, bool inAction = false)
    {
        switch(playerClass)
        {
            case PlayerClass.Sniper:
                weaponManager.GiveWeapon(Weapon.Barrett);
                weaponManager.GiveWeapon(Weapon.CZ75);
                weaponManager.GiveWeapon(Weapon.Shovel);
                weaponManager.GiveGrenade(Grenade.Claymore, 1);
                break;
            case PlayerClass.Assault:
                weaponManager.GiveWeapon(Weapon.M4A4);
                weaponManager.GiveWeapon(Weapon.Glock);
                weaponManager.GiveWeapon(Weapon.Shovel);
                weaponManager.GiveGrenade(Grenade.HE, 2);
                break;
            case PlayerClass.Demolitionist:
                weaponManager.GiveWeapon(Weapon.AK47);
                //weaponManager.GiveWeapon(Weapon.M249);
                weaponManager.GiveWeapon(Weapon.USPS);
                weaponManager.GiveWeapon(Weapon.Shovel);
                weaponManager.GiveWeapon(Weapon.RPG);
                break;
            case PlayerClass.Medic:
                weaponManager.GiveWeapon(Weapon.M1014);
                weaponManager.GiveWeapon(Weapon.USPS);
                weaponManager.GiveWeapon(Weapon.Shovel);
                weaponManager.GiveGrenade(Grenade.MedicBag, 3);
                break;
            case PlayerClass.Engineer:
                weaponManager.GiveWeapon(Weapon.UMP45);
                weaponManager.GiveWeapon(Weapon.Glock);
                weaponManager.GiveWeapon(Weapon.Shovel);
                break;
        }

        if (!inAction) {
            GetComponent<Player>().SetAliveDefaults();
            GetComponent<PlayerWeaponManager>().EquipWeapon(WeaponSlot.Primary);
        }
    }
}
