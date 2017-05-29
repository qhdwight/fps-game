using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

	public const float EXPLOSION_TIME = 5F, HURT_SPHERE_TIME = 0.2F;

	[SerializeField] private GameObject hurtSpherePrefab, blastPrefab;

	private bool isOnServer;
    private ushort hurtSphereRadius, blockDeleteRadius;
	private int damage;
	private uint shooterId;
	private string projectileName;

	public void SetData(bool isOnServer, ushort hurtSphereRadius, ushort blockDeleteRadius, int damage, uint shooterId, string projectileName) {
		this.isOnServer = isOnServer;
        this.hurtSphereRadius = hurtSphereRadius;
        this.blockDeleteRadius = blockDeleteRadius;
		this.damage = damage;
		this.shooterId = shooterId;
		this.projectileName = projectileName;

		Init();
	}

	public void Init()
	{
        // Create blast
        GameObject blastInstance = Instantiate(blastPrefab, transform, false);
        Destroy(blastInstance, EXPLOSION_TIME);

        // Shake camera
        CameraEffectManager.ShakeCameras();

        if (isOnServer)
        {
            // Destroy blocks
            BlockManager.singleton.DestroyBlocksInRadius(blockDeleteRadius, Util.Vector3ToWorldPos(transform.position), true);

            GameObject hurtSphereInstance = Instantiate(hurtSpherePrefab, transform, false);

            HurtSphere hurtSphere = hurtSphereInstance.GetComponent<HurtSphere>();
            hurtSphere.SetData(hurtSphereRadius, damage, shooterId, projectileName);

            Destroy(hurtSphereInstance, HURT_SPHERE_TIME);
        }

		Destroy(gameObject, EXPLOSION_TIME);
	}
}
