using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Flag : NetworkBehaviour {

    public FlagInfo info;

    [SyncVar] public ushort id;
    [SyncVar(hook="FlagTakenUpdated")] public bool flagIsTaken;
    private bool flagIsBeingTaken;

    [SerializeField] private GameObject flagCloth;
    [SerializeField] private GameObject flagHeldPrefab;
    [SerializeField] private Collider worldEditSelectionTrigger, playerTrigger;

    private Color flagColor, postColor;
    private SkinnedMeshRenderer flagClothComponent;
    private MeshRenderer meshRenderer;
    private MapInfo currentMapInfo;

    public const string PLAYER_COLLIDER_LAYER_NAME = "Player Collider", FLAG_HELD_TAG = "Flag Held";
    public const float TIME_TO_TAKE_FLAG = 2F;

    private Player playerToTakeFlag;

    public bool isWorldEditFlag;

    // Create a list of players that are in the area, players that are first in the list get the flag
    private List<Player> enemyPlayersInArea = new List<Player>();

    private void Awake()
    {
        flagClothComponent = GetComponentInChildren<SkinnedMeshRenderer>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        postColor = meshRenderer.material.color;
    }

    [Server]
    public void SetData(FlagInfo info, bool isWorldEditFlag, ushort id = 0)
    {
        this.isWorldEditFlag = isWorldEditFlag;
        this.info = info;
        this.id = id;

        Init();
    }

    public void Init()
    {
        // If world edit make it selectable
        if (isWorldEditFlag)
        {
            gameObject.AddComponent<WorldEditSelectable>().Setup(false, FlagPositionUpdated, SelectedByWorldEdit, Deleted);
            currentMapInfo = MapManager.instance.currentMapInfo;

            playerTrigger.enabled = false;
            worldEditSelectionTrigger.enabled = true;
        }

        // Set color of flag
        SetFlagColorBasedOnTeam();
    }

    private void OnEnable()
    {
        MapManager.instance.mapCreatedDelegate += MapCreated;
    }

    private void OnDisable()
    {
        MapManager.instance.mapCreatedDelegate -= MapCreated;
    }

    private void MapCreated()
    {
        MapManager mapCreator = MapManager.instance;

        if (!isServer && mapCreator) {
            info = mapCreator.GetFlagInfoFromID(id);

            SetFlagColorBasedOnTeam();
        }

        if (!isServer && flagIsTaken)
            SetFlagVisibility(false);
    }

    private void SetFlagColorBasedOnTeam()
    {
        flagColor = GameManager.GetTeamColor(info.team);
        SetFlagColor(flagColor);
    }

    private void SetFlagColor(Color col)
    {
        if (flagClothComponent)
        {
            flagClothComponent.material.color = col;
        }
    }

    private IEnumerator currentTakeFlag;

    private void OnTriggerEnter(Collider col)
    {
        // Only do this on the server
        if (!isWorldEditFlag)
        {
            if (isServer)
            {
                // Check if the flag is able to be taken
                if (!flagIsTaken && !flagIsBeingTaken)
                {
                    // Check if the collider that entered is a player
                    if (IsPlayerCollider(col))
                    {
                        Player player = col.GetComponentInParent<Player>();

                        // Make sure player that is capturing is not on our team
                        // Make sure player does not already have a flag
                        if (player.team != info.team && !player.hasFlag)
                        {
                            enemyPlayersInArea.Add(player);

                            // Start the taking the flag process
                            currentTakeFlag = TakeFlag();
                            StartCoroutine(currentTakeFlag);
                        }
                    }
                }

                // Check if we are returning the opponent's flag
                if (IsHeldFlagCollider(col))
                {
                    FlagHeld flagScript = col.GetComponent<FlagHeld>();
                    Player player = GameManager.GetPlayer(flagScript.playerId);

                    // Make sure this isn't the own flags held flag and that the player is on our team
                    if (flagScript.ownerFlag.id != id && player.team == info.team)
                    {
                        /* Enemy flag returned */
                        OpponentFlagCaptured(flagScript.ownerFlag, player);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (!isWorldEditFlag)
        {
            // Only do this on the server
            if (isServer)
            {
                // Check if the collider that exited is a player
                if (IsPlayerCollider(col))
                {
                    Player player = col.GetComponentInParent<Player>();

                    // Make sure player that is capturing is not on our team
                    if (player.team != info.team)
                    {
                        player.FlagCancelTaken();
                        enemyPlayersInArea.Remove(player);

                        if (player.Equals(playerToTakeFlag))
                            CancelTakeFlag();
                    }
                }
            }
        }
    }

    private void OpponentFlagCaptured(Flag opponentFlag, Player playerThatCaptured)
    {
        // Reset the flag
        opponentFlag.ResetFlag();

        // Remove held flag from player
        playerThatCaptured.hasFlag = false;
        NetworkServer.Destroy(playerThatCaptured.heldFlagInstance);

        // Give points to other team
        RoundInfoScript roundInfoScript = RoundInfoScript.singleton;
        switch (info.team) {
            case Team.Red: {
                    roundInfoScript.redScore++;
                    break;
                }
            case Team.Blue: {
                    roundInfoScript.blueScore++;
                    break;
                }
        }
    }

    private void CancelTakeFlag()
    {
        StopCoroutine(currentTakeFlag);
        flagIsBeingTaken = false;
    }

    public void SetFlagVisibility(bool enabled)
    {
        flagCloth.SetActive(enabled);
    }

    private bool IsPlayerCollider(Collider col)
    {
        // Check if collider has the player tag and is the capusle collider of the player, not the sphere
        return (col.gameObject.layer == LayerMask.NameToLayer(PLAYER_COLLIDER_LAYER_NAME) && col is CapsuleCollider);
    }

    private bool IsHeldFlagCollider(Collider col)
    {
        FlagHeld flagScript = col.GetComponent<FlagHeld>();

        if (flagScript != null)
        {
            if (col.gameObject.tag == FLAG_HELD_TAG)
            {
                return true;
            }
        }
        return false;
    }

    [Server]
    public void ResetFlag()
    {
        flagIsTaken = false;
    }

    [Server]
    private IEnumerator TakeFlag()
    {
        // Have the player first in the list take the flag
        if (enemyPlayersInArea.Count > 0)
        {
            /* Start to take the flag */

            flagIsBeingTaken = true;

            playerToTakeFlag = enemyPlayersInArea[0];
            playerToTakeFlag.FlagStartTaken();

            // Wait some amount of time before the flag can be taken
            yield return new WaitForSeconds(TIME_TO_TAKE_FLAG);

            /* Take the flag */

            // Make sure the player isnt dead
            if (!playerToTakeFlag.isDead)
            {
                // Get the first player who should take the flag
                playerToTakeFlag.FlagTaken();

                SetFlagVisibility(false);

                // Create the flag held object
                GameObject heldFlagInstance = Instantiate(flagHeldPrefab);

                FlagHeld heldFlagScript = heldFlagInstance.GetComponent<FlagHeld>();
                heldFlagScript.playerId = playerToTakeFlag.netId.Value;
                heldFlagScript.col = flagColor;
                heldFlagScript.ownerFlag = this;

                NetworkServer.Spawn(heldFlagInstance);   

                flagIsTaken = true;
                flagIsBeingTaken = false;
            }
        }
    }

    private void FlagTakenUpdated(bool isTaken)
    {
        flagIsTaken = isTaken;
        SetFlagVisibility(!flagIsTaken);
    }

    // Callback function
    private void FlagPositionUpdated(WorldPosition pos, Quaternion rot)
    {
        // Update position in map info
        info.pos = pos;
    }

    // Callback function
    private void SelectedByWorldEdit(bool selected)
    {
        meshRenderer.material.color = selected          ? Util.ShiftColor(postColor) : postColor;
        flagClothComponent.material.color = selected    ? Util.ShiftColor(flagColor) : flagColor;
    }

    // Callback function
    private void Deleted()
    {
        currentMapInfo.flags.Remove(info);
        Destroy(gameObject);
    }
}
