using UnityEngine;

public class PlayerGrenade : PlayerItem {

    public void SetData(bool isOnServer, int amount, uint shooterId) {
        currentAmmo = amount;
        this.isOnServer = isOnServer;
        this.shooterId = shooterId;

        Init();
    }

    public void Init()
    {
        if (id == Grenade.MedicBag && isOnServer)
        {
            healSphereScript = healSphere.GetComponent<HealSphere>();
            healSphereScript.SetData(healSphereRadius, healPerSecond, shooterId, itemName);
            healSphereCollider = GetComponent<SphereCollider>();
        }
    }

    [HideInInspector] public HealSphere healSphereScript;
    [HideInInspector] public SphereCollider healSphereCollider;

    private uint shooterId;

    public GameObject grenadeModel;
    public GameObject grenadeCollider;
    public GameObject grenadeTrigger;

    public Grenade id;

    public int currentAmmo;
    [HideInInspector] public bool isReadyToUse = true;
    [Tooltip("How long it takes to throw another grenade")]
    public float throwDelay;
    [Tooltip("Time it takes to throw grenade from start of animation")]
    public float timeToThrow;
    public float throwForce;
    public float timeToExplode;
    public RigidbodyConstraints constraints;

    [Header("Damaging")]
    [Tooltip("How the grenade should explode")]
    public ImpactType impactType;
    public int damage;
    public ushort blockDeleteRadius, hurtSphereRadius;

    [Header("Healing")]
    public GameObject healSphere;
    public int healPerSecond;
    public ushort healSphereRadius;
}

public enum ImpactType
{
    ExplodeOnContact,
    Time,
    Claymore,
    Constant,
}
