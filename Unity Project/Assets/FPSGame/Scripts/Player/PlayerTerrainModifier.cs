using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerTerrainModifier : NetworkBehaviour {

    [SerializeField] private Camera cam;
    private PlayerWeaponManager weaponManager;
    [SerializeField] private LayerMask blockLayerMask;
    [SerializeField] private int maxReachDistance;
    private float minReachDistance = 1.8F;
    private bool canPlaceBlock = true;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        weaponManager = GetComponent<PlayerWeaponManager>();
    }

    private void Update()
    {
        if (weaponManager.currentPlayerItem != null && isLocalPlayer)
        {
            TerrainModification();
        }
    }

    private void TerrainModification()
    {
        if (weaponManager.currentPlayerItem is PlayerGun) {
            PlayerGun currentGun = (PlayerGun)weaponManager.currentPlayerItem;
            if (!currentGun.canEditBlocks || !canPlaceBlock)
                return;
        }

        if (Input.GetButtonDown("Place Block"))
        {
            SetBlock("DirtBlock");
        }
    }

    private IEnumerator SetCooldownForPlacingBlocks()
    {
        yield return new WaitForSeconds(0.2f);
        canPlaceBlock = true;
    }

    private bool SendOutBlockRaycast(out RaycastHit hit)
    {
        return Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxReachDistance, blockLayerMask);
    }

    [Command]
    private void CmdDestroyBlock()
    {
        RaycastHit hit;
        if (SendOutBlockRaycast(out hit))
        {
            ModifyTerrain.RemoveBlock(hit, false, true);
        }
    }

    [Command]
    private void CmdSetBlock(string blockName)
    {
        RaycastHit hit;
        if (SendOutBlockRaycast(out hit))
        {
            //If the camera is higher than the point of contanct
            if (Mathf.Sign(transform.position.y + 2f - hit.point.y) == 1)
                if (hit.distance < minReachDistance)
                    return;

            ModifyTerrain.SetBlock(hit, blockName, true);
        }
    }

    private void SetBlock(string blockName)
    {
        CmdSetBlock(blockName);

        canPlaceBlock = false;
        StartCoroutine(SetCooldownForPlacingBlocks());
    }

    public void DestroyBlock()
    {
        // Check if block is breakable on client side
        RaycastHit hit;
        if (SendOutBlockRaycast(out hit))
        {
            Block block = ModifyTerrain.GetBlock(hit, false);
            if (block != null)
                if (!block.breakable)
                    return;
        }
        CmdDestroyBlock();
    }
}
