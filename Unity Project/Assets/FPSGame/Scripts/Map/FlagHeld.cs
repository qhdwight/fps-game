using UnityEngine;
using UnityEngine.Networking;

public class FlagHeld : NetworkBehaviour
{

    [SyncVar]
    public uint playerId;
    [SyncVar]
    public Color col;

    public Flag ownerFlag;
    private Player attachedPlayer;

    private void Start()
    {
        attachedPlayer = GameManager.GetPlayer(playerId);
        attachedPlayer.heldFlagInstance = gameObject;

        transform.SetParent(attachedPlayer.flagHolder.transform, false);

        if (attachedPlayer.isLocalPlayer)
            Util.SetLayerRecursively(gameObject, LayerMask.NameToLayer(PlayerWeaponManager.DONT_DRAW_LAYER_NAME));

        // Change color of the flag
        GetComponentInChildren<SkinnedMeshRenderer>().material.color = col;
    }
}
