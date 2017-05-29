using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

public class LiveGrenade : NetworkBehaviour
{

    [SerializeField] private GameObject explosionPrefab;

    private Rigidbody rb;

    [SyncVar] private uint shooterId;
    [SyncVar] private Grenade id;
    [SyncVar] private Quaternion initialRot;
    private PlayerGrenade grenade;
    private Vector3 initialVelocity;
    private bool isOnServer;

    private GameObject grenadeModelInstance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Server]
    public void SetDataServer(Grenade id, uint shooterId, Vector3 forward, Quaternion initialRot)
    {
        this.shooterId = shooterId;
        this.id = id;
        this.initialRot = initialRot;
        grenade = GetGrenade(id);

        float throwForce = grenade.throwForce;
        initialVelocity = forward * throwForce;
    }

    private void Start()
    {
        isOnServer = isServer;

        if (isOnServer)
            ServerInit();
        else
            StartCoroutine(GetData());

        transform.rotation = initialRot;
    }

    public PlayerGrenade GetGrenade(Grenade id)
    {
        return GameManager.localPlayer.GetComponent<PlayerWeaponManager>().GetItem(id).GetComponent<PlayerGrenade>();
    }

    [Server]
    public void ServerInit()
    {
        SetCollider();
        SetTrigger();
        CreateGrenadeModel();

        if (grenade.id == Grenade.MedicBag)
            CreateHealSphere();

        // Add throw force
        rb.AddForce(initialVelocity);   

        if (grenade.impactType == ImpactType.Time)
            StartCoroutine(ExplodeAfterTime(grenade.timeToExplode));
    }

    [Client]
    public void ClientInit()
    {
        grenade = GetGrenade(id);
        SetCollider();
        CreateGrenadeModel();
    }

    private IEnumerator GetData()
    {
        while (GameManager.localPlayer == null)
        {
            yield return null;
        }
        ClientInit();
    }

    private void SetTrigger()
    {
        if (grenade.grenadeTrigger)
        {
            GameObject grenadeTriggerInstance = Instantiate(grenade.grenadeTrigger, transform, false);
            grenadeTriggerInstance.SetActive(true);
        }
    }

    private void SetCollider()
    {
        // Create the collider
        GameObject grenadeColliderInstance = Instantiate(grenade.grenadeCollider, transform, false);
        grenadeColliderInstance.SetActive(true);

        // Adjust rigidbody
        rb.constraints = grenade.constraints;
    }

    private void CreateHealSphere()
    {
        GameObject healSphereInstance = Instantiate(grenade.healSphere, transform, false);
        HealSphere healSphereScript = healSphereInstance.GetComponent<HealSphere>();
        healSphereScript.SetData(grenade.healSphereRadius, grenade.healPerSecond, shooterId, grenade.itemName);
        healSphereInstance.SetActive(true);
    }

    private void CreateGrenadeModel()
    {
        // Create grenade model
        grenadeModelInstance = Instantiate(grenade.grenadeModel, transform, false);
        grenadeModelInstance.SetActive(true);
        Util.SetLayerRecursively(grenadeModelInstance, LayerMask.NameToLayer(PlayerWeaponManager.DEFAULT_LAYER_NAME));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isOnServer)
        {
            switch (grenade.impactType)
            {
                case ImpactType.ExplodeOnContact:
                    Explode();
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOnServer)
        {
            switch (grenade.impactType)
            {
                case ImpactType.Claymore:
                    if (IsPlayerCollider(other) && !IsColliderOnTeam(other))
                        Explode();
                    break;
            }
        }
    }

    [Server]
    private IEnumerator ExplodeAfterTime(float explodeTime)
    {
        yield return new WaitForSeconds(explodeTime);
        Explode();
    }

    [Server]
    private void Explode()
    {
        RpcExplode();
        Invoke("NetworkDestroy", 1F);
        //NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcExplode()
    {
        if (id != Grenade.MedicBag)
        {
            GameObject explosionInstance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosionInstance.GetComponent<Explosion>().SetData(
                isOnServer,
                grenade.hurtSphereRadius,
                grenade.blockDeleteRadius,
                grenade.damage,
                shooterId,
                grenade.itemName);
        }
        Destroy(gameObject);
    }

    [Server]
    private void NetworkDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }

    private bool IsPlayerCollider(Collider col)
    {
        return (col.gameObject.layer == LayerMask.NameToLayer("Player Collider") && col is CapsuleCollider);
    }

    private bool IsColliderOnTeam(Collider col)
    {
        return (col.GetComponentInParent<Player>().team == GameManager.GetPlayer(shooterId).team);
    }
}
