using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Gun,
    Melee,
    Grenade,
    Misc,
};

public class PlayerItem : MonoBehaviour {

    public string itemName;
    public WeaponSlot weaponSlot;
    public WeaponType itemType;
    [HideInInspector] public bool isOnServer;

    public void SetData(bool isOnServer)
    {
        this.isOnServer = isOnServer;
    }

}
