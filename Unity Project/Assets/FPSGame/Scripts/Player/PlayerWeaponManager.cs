using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel = 0)]
public class PlayerWeaponManager : NetworkBehaviour
{
    public static string VIEWMODEL_LAYER_NAME = "Viewmodel", DONT_DRAW_LAYER_NAME = "Dont Draw", DEFAULT_LAYER_NAME = "Default";
    private const float ITEM_SWITCH_DELAY = 0.3F;

    [SerializeField] private GameObject[] weapons, grenades;
    [SerializeField] public GameObject playerArms, playerBody, weaponHolder;
    [SerializeField] private GameObject weaponCam;

    private Player player;
    private PlayerUse playerUse;
    private PlayerReload playerReload;
    private PlayerMultiplayerMotor playerMotor;

	public WeaponGraphics currentWeaponGraphicsScript;
	public PlayerItem currentPlayerItem;
    public Animator playerArmsAnimator;
	public GameObject currentItemInstance;
    public GameObject[] itemInstances;

    public bool canChangeWeapons = true;
    private float scrollWheelInput = 0;
    private int maxSlot = 3, minSlot = 0;
    private IEnumerator currentHealSequence;

    [SyncVar(hook = "OnCurrentWeaponUpdated")] private WeaponSlot currentWeaponSlot;

    private void Awake()
    {
        itemInstances = new GameObject[maxSlot+1];

        player = GetComponent<Player>();
        playerUse = GetComponent<PlayerUse>();
        playerReload = GetComponent<PlayerReload>();
        playerMotor = GetComponent<PlayerMultiplayerMotor>();
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            // parent viewmodel to player if local
            playerArms.transform.SetParent(weaponCam.transform);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer || CursorManagement.IsMenuOpen())
            return;

        scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollWheelInput < 0)
            WeaponDown();
        else if (scrollWheelInput > 0)
            WeaponUp();
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            OnEquipWeapon(WeaponSlot.Primary);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            OnEquipWeapon(WeaponSlot.Secondary);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            OnEquipWeapon(WeaponSlot.Melee);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            OnEquipWeapon(WeaponSlot.Misc1);
    }

    public GameObject GetItem(Weapon weaponId)
    {
        return weapons[(int)weaponId];
    }

    public GameObject GetItem(Grenade grenadeId)
    {
        return grenades[(int)grenadeId];
    }

    public void GiveWeapon(Weapon weaponId)
    {
        GameObject weaponPrefab = GetItem(weaponId);
        GiveItem(weaponPrefab);
    }

    public void GiveGrenade(Grenade grenadeId, int amount)
    {
        GameObject grenadePrefab = GetItem(grenadeId);
        GameObject grenadeInstance = GiveItem(grenadePrefab);
        PlayerGrenade grenadeScript = grenadeInstance.GetComponent<PlayerGrenade>();
        grenadeScript.currentAmmo = amount;
    }

    public GameObject GiveItem(GameObject itemPrefab)
    {
        // Create the new weapon
        GameObject itemInstance = Instantiate(itemPrefab, weaponHolder.transform, false);
        PlayerItem item = itemInstance.GetComponent<PlayerItem>();
        if (item is PlayerGun)
        {
            PlayerGun gun = item as PlayerGun;
            gun.SetData(isServer);
            // Create name for weapon GameObject
            string weaponName = string.Empty;
            if (gun.isGun || gun.isLauncherRaycast) weaponName += "Gun";
            weaponName += gun.itemName;

            itemInstance.name = weaponName;
        }
        else if (item is PlayerGrenade)
        {
            PlayerGrenade grenade = item as PlayerGrenade;
            grenade.SetData(isServer, grenade.currentAmmo, netId.Value);
            // Create name for grenade GameObject
            itemInstance.name = grenade.itemName;
            if (grenade.id == Grenade.MedicBag)
            {
                if (isServer)
                    grenade.healSphereScript.SetData(
                        grenade.healSphereRadius,
                        grenade.healPerSecond,
                        netId.Value,
                        grenade.itemName);
            }
            else
            {
                itemInstance.name += "Grenade";
            }
        }

        // Set layer to viewmodel if the weapon is for the local player
        if (isLocalPlayer)
            Util.SetLayerRecursively(itemInstance, LayerMask.NameToLayer(VIEWMODEL_LAYER_NAME));
        
        // Set the instance of the gameObject in the slot
        itemInstances[(int)item.weaponSlot] = itemInstance;

        // Make sure the Animator recognizes all of
        // the parts of the newly instantiated GameObject
        playerArmsAnimator.Rebind();

        return itemInstance;
    }

    public void EquipCurrentWeaponInAction()
    {
        EquipWeapon(currentWeaponSlot);
    }

    private void WeaponUp()
    {
        if (canChangeWeapons)
        {
            int nextVal = (int)currentWeaponSlot + 1;
            if (nextVal >= 1 + maxSlot) { nextVal = minSlot; }
            OnEquipWeapon((WeaponSlot)(nextVal));
        }
    }
    
    private void WeaponDown()
    {
        if (canChangeWeapons)
        {
            int prevVal = (int)currentWeaponSlot - 1;
            if (prevVal <= minSlot - 1) { prevVal = maxSlot; }
            OnEquipWeapon((WeaponSlot)(prevVal));
        }
    }

    [Client]
    public void OnEquipWeapon(WeaponSlot weaponSlot)
    {   
        if (canChangeWeapons)
        {
            PlayerItem itemInSlot = itemInstances[(int)weaponSlot].GetComponent<PlayerItem>();
            if (itemInSlot is PlayerGrenade)
            {
                // Check that the grenade in the slot has ammo left
                if ((itemInSlot as PlayerGrenade).currentAmmo <= 0)
                    return;
            }

            if (playerReload.Reloading)
                playerReload.CancelReload();

            CmdEquipWeapon(weaponSlot);

            canChangeWeapons = false;
            StartCoroutine(ItemSwitchDelay());
        }
    }

    [Command]
    private void CmdEquipWeapon(WeaponSlot weaponSlot)
    {
        currentWeaponSlot = weaponSlot;
    }

    private void OnCurrentWeaponUpdated(WeaponSlot slot)
    {
        currentWeaponSlot = slot;
        EquipWeapon(slot);
    }

    public void EquipWeapon(WeaponSlot weaponSlot)
    {
        // Get weapon
        GameObject itemInstance = itemInstances[(int)weaponSlot];
        PlayerItem item = itemInstance.GetComponent<PlayerItem>();

        // Test if weapon switch is valid
        if (itemInstance == null) {
            Debug.LogWarning("Weapon in slot: " + weaponSlot + " does not exist");
            return;
        }
        if (itemInstance == currentItemInstance)
            return;

        SetWeaponDefaults(item);
        StartCoroutine(ItemSwitchDelay());
    }

    private void SetWeaponDefaults(PlayerItem item) 
    {
        canChangeWeapons = false;
        // Stop current healing sequence
        StopHealOverTime();
        if (isServer) {
            
            if (currentPlayerItem is PlayerGrenade)
            {
                PlayerGrenade grenade = currentPlayerItem as PlayerGrenade;
                if (grenade.id == Grenade.MedicBag)
                {
                    grenade.healSphere.SetActive(false);
                }
            }
        }

        // Set current values
        currentItemInstance = item.gameObject;

        // Get rid of scope if there is one
        GetComponent<PlayerUse>().SetScope(false);

        // Get weapon graphics script
        currentWeaponGraphicsScript = currentItemInstance.GetComponent<WeaponGraphics>();

        // Get weapon data
        currentPlayerItem = currentItemInstance.GetComponent<PlayerItem>();

        string layerName = isLocalPlayer ? VIEWMODEL_LAYER_NAME : DEFAULT_LAYER_NAME;
        SetOnlyOneItemLayerAndOthersNotVisible(item.gameObject, layerName);

        // Play retrieving animation
        playerArmsAnimator.Play(item.itemName + "Retrieve");

        if (item is PlayerGun) SetGunDefaults(item as PlayerGun);
        else if (item is PlayerGrenade) SetGrenadeDefaults(item as PlayerGrenade);
    }

    public void SetGrenadeDefaults(PlayerGrenade grenade)
    {
        // Play retrieving animation
        //playerArmsAnimator.Play(grenade.itemName + "Retrieve");

        if (isLocalPlayer)
            SetCrosshairVisibility(true);

        // Heal player sequence
        if (isServer && grenade.id == Grenade.MedicBag) {
            StartHealOverTime(grenade.healPerSecond);
            grenade.healSphere.SetActive(true);
        }
    }

    public void SetGunDefaults(PlayerGun gun)
    {
        // Play appropriate animation
        //if (playerMotor.running && isLocalPlayer)
        //    playerArmsAnimator.Play(gun.itemName + "Run");
        //else
        //    playerArmsAnimator.Play(gun.itemName + "Idle");

        if (isLocalPlayer)
            SetCrosshairVisibility(!gun.dontShowCrosshair);
    }

    public void SetCrosshairVisibility(bool visible)
    {
        GameObject crosshair = PlayerGUI.singleton.crosshair;
        if (crosshair) crosshair.SetActive(visible);
    }

    public void SetOnlyOneItemLayerAndOthersNotVisible(GameObject weaponToSet, string layerName) {
        foreach (GameObject weapon in itemInstances)
        {
            if (weapon == null)
                break;
            if (!weapon.Equals(weaponToSet))
                Util.SetLayerRecursively(weapon, LayerMask.NameToLayer(DONT_DRAW_LAYER_NAME));
            else
                Util.SetLayerRecursively(weaponToSet, LayerMask.NameToLayer(layerName));
        }
    }

    private IEnumerator ItemSwitchDelay()
    {
        yield return new WaitForSeconds(ITEM_SWITCH_DELAY);
        canChangeWeapons = true;
    }

    private void StopHealOverTime()
    {
        if (currentHealSequence != null)
            StopCoroutine(currentHealSequence);
    }

    private void StartHealOverTime(int healthPerSecond)
    {
        currentHealSequence = player.HealOverTime(healthPerSecond);
        StartCoroutine(currentHealSequence);
    }
}

public enum GunSound
{
    Shoot       = 0,
    OutOfAmmo   = 1,
    Reload      = 2,
};

public enum MeleeSound
{
    Swing       = 0,
    Smack       = 1,
};

public enum Weapon
{
    Shovel      = 0,
    M4A4        = 1,
    Glock       = 2,
    AK47        = 3,
    USPS        = 4,
    Barrett     = 5,
    CZ75        = 6,
    RPG         = 7,
    M1014       = 8,
    UMP45       = 9,
    M249        = 10,
};

public enum Grenade
{
    HE          = 0,
    Claymore    = 1,
    MedicBag    = 2,
}

public enum WeaponSlot
{
    Primary     = 0,
    Secondary   = 1,
    Melee       = 2,
    Misc1       = 3,
    Misc2       = 4,
};