using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightProjectile : MonoBehaviour {

    public const string FLOOR_LAYER_NAME = "Floor";
    //public const float CAN_EXPLODE_DELAY = 0.05f;
    public const float MAX_TIME_IN_AIR = 30F;

    public float speed;
    public ushort hurtSphereRadius, blockDeleteRadius;
    public int damage;
    public string projectileName;

    [SerializeField] private GameObject explosionPrefab;

    private uint shooterId;

    public bool isOnServer = false;

    public void SetData(bool isOnServer, uint shooterId)
    {
        this.isOnServer = isOnServer;
        this.shooterId = shooterId;
    }

    private void Start()
    {
        //StartCoroutine(CanExplodeDelay());
        Destroy(gameObject, MAX_TIME_IN_AIR);
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider col = collision.collider;

        int layer = col.gameObject.layer;

        if (ShouldExplode(layer))
        {
            ContactPoint contact = collision.contacts[0];
            OnImpact(contact.point + contact.normal);
        }
    }

    private void OnImpact(Vector3 impactPos)
    {
        // Create explosion
        CreateBlast(impactPos, isOnServer, hurtSphereRadius, blockDeleteRadius, damage, shooterId, projectileName);

        Destroy(gameObject);
    }

    private void CreateBlast(Vector3 impactPos, bool isOnServer, ushort hurtSphereRadius, ushort blockDeleteRadius, int damage, uint shooterId, string projectileName)
    {
        GameObject explosionInstance = Instantiate(explosionPrefab, impactPos, Quaternion.identity);
        explosionInstance.GetComponent<Explosion>().SetData(isOnServer, hurtSphereRadius, blockDeleteRadius, damage, shooterId, projectileName);
    }

    private bool ShouldExplode(int layer)
    {
        return (
            layer != LayerMask.NameToLayer("Player Collider")
         && layer != LayerMask.NameToLayer(PlayerWeaponManager.DONT_DRAW_LAYER_NAME));
    }
}
