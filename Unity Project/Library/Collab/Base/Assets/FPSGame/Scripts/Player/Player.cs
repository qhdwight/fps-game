using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour, IFlagHandler
{
    [SyncVar] public bool isDead;
    [SyncVar] public bool hasFlag;
    [SyncVar(hook = "SetKillsOnScoreBoard")] public int kills;
    [SyncVar(hook = "SetDeathsOnScoreBoard")] public int deaths;
    [SyncVar(hook = "OnTeamUpdated")] public Team team = Team.None;
    public int currentHealth;
    [SerializeField] public int maxHealth;
    [SerializeField] private AudioSource hitSoundEffect;
    [SerializeField] public string defaultLayerName, enemyLayerName, friendlyLayerName, dontDrawLayerName, playerColliderLayerName, viewmodelLayerName;
    [SerializeField] public GameObject playerBody, playerNameTag, playerArms, flagHolder;
    [SerializeField] private GameObject ragdollPrefab;
    [SerializeField] public Collider[] colliders;
    [SerializeField] public Collider[] playerColliders;
    [SerializeField] public Behaviour[] componentsToDisableOnDeath, componentsToDisableOnRemotePlayer;
    public GameObject heldFlagInstance;
    private PlayerSetup playerSetup;
    private PlayerWeaponManager weaponManager;
    private PlayerClassManager playerClassManager;

    public const float RAGDOLL_LIFESPAN = 10F;

    private void Awake()
    {
        playerSetup = GetComponent<PlayerSetup>();
        weaponManager = GetComponent<PlayerWeaponManager>();
        playerClassManager = GetComponent<PlayerClassManager>();

        currentHealth = maxHealth;
    }

    private void Update()
    {
        //if (Input.GetButtonDown("Suicide") && !isDead && isLocalPlayer)
        //{
        //    isDead = true;
        //    CmdSuicide();
        //}

        if (transform.position.y < -64 && !isDead)
        {
            isDead = true;
            GetComponent<Rigidbody>().position = Vector3.zero;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            CmdSuicide();
        }
    }

    public void SendTeamSwitchRequest(Team newTeam)
    {
        if (newTeam != team)
        {
            CmdRequestTeamSwitch(newTeam);
        }
    }

    [Command]
    private void CmdRequestTeamSwitch(Team newTeam)
    {
        team = newTeam;

        if (!isDead)
        {
            isDead = true;
            CmdSuicide();
        }
    }

    [ClientRpc]
    private void RpcSuicide()
    {
        if (isLocalPlayer)
        {
            // Start the respawning process
            StartCoroutine(Respawn());
        }

        // Set all the properties of a dead player
        SetDeadDefaults();
    }

    [Command]
    private void CmdSuicide()
    {
        // Add death to player
        deaths++;
        // Create kill feed
        KillFeedScript.instance.CreateKillFeed(netId.Value, netId.Value, "void", false);

        // Create ragdoll
        RpcCreateRagdoll();

        RpcSuicide();
    }

    private IEnumerator Respawn()
    {
        // Wait for the amount of type specified in the GameManager
        yield return new WaitForSeconds(GameManager.instance.respawnTime);

        // Show class selection menu
        ClassManager classManager = ClassManager.singleton;
        classManager.Open();
        
    }

	[Command]
    public void CmdDestroyPlayer()
    {
		NetworkServer.Destroy(gameObject);
	}

    [ClientRpc]
    public void RpcTakeDamage(int amount, uint shooterID, bool headshot, string weapon)
    {
        if (!isDead)
        {
            // Play hit sound effect
            if (isLocalPlayer)
            {
                hitSoundEffect.pitch = UnityEngine.Random.Range(0.9F, 1.1F);
                hitSoundEffect.Play();

                Debug.Log("Hit for: " + amount.ToString() + " damage from player: " + GameManager.GetPlayerUsername(shooterID));
            }

            // Subtract damage from player's health
            currentHealth -= amount;

            if (currentHealth <= 0)
            {
                // Set defaults for death
                SetDeadDefaults();

                if (isServer)
                {
                    // Create ragdoll
                    RpcCreateRagdoll();

                    Player shooter = GameManager.GetPlayer(shooterID);
                    // Add a kill to the player if you are on the server
                    shooter.kills++;
                    ScoreBoardMenu.instance.SetKillValue(shooter.kills, shooterID);
                    // Add a death to your player if you are on the server
                    deaths++;
                    ScoreBoardMenu.instance.SetDeathsValue(deaths, netId.Value);

                    // Create kill feed
                    KillFeedScript.instance.CreateKillFeed(shooterID, netId.Value, weapon, headshot);
                }

                if (isLocalPlayer)
                {
                    // Start the respawning process
                    StartCoroutine(Respawn());
                }
            }
        }
    }

    public void OnSetAliveDefaults()
    {
        CmdSetAliveDefaults();
    }

    [Command]
    public void CmdSetAliveDefaults()
    {
        RpcSetAliveDefaults();
    }

    [ClientRpc]
    public void RpcSetAliveDefaults()
    {
        SetAliveDefaults();
    }

    public void SetDeadDefaults()
    {
        isDead = true;

        if (isLocalPlayer)
        {
            // Enable scene camera
            //SceneCameraScript.instance.SetSceneCameraActive(true);
            SceneCameraScript.instance.PlayerDiedCinematic(transform.position, transform.forward);

            // Disable HUD
            if (PlayerGUI.singleton != null)
                playerSetup.playerUIInstance.SetActive(false);

            CursorManagement.CorrectLockMode();

            // Enable behaviours
            foreach (Behaviour behaviour in componentsToDisableOnDeath)
            {
                behaviour.enabled = false;
            }  
        }
        else
        {
            // Disable view of the player name tag and body
            Util.SetLayerRecursively(playerNameTag, LayerMask.NameToLayer(dontDrawLayerName));
            Util.SetLayerRecursively(playerBody, LayerMask.NameToLayer(dontDrawLayerName));
        }
        if (isServer)
        {
            if (hasFlag) {
                heldFlagInstance.GetComponent<FlagHeld>().ownerFlag.ResetFlag();
                hasFlag = false;
                NetworkServer.Destroy(heldFlagInstance);
            }
        }
        // Don't draw any of the player
        Util.SetLayerRecursively(playerArms, LayerMask.NameToLayer(dontDrawLayerName));

        // Get rid of all weapons
        foreach (Transform child in weaponManager.weaponHolder.transform)
        {
            Destroy(child.gameObject);
        }

        // Don't use gravity and reset velocity
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;

        // Disable colliders
        foreach (Collider col in colliders)
        {
            col.enabled = false;        
        }
        foreach(Collider col in playerColliders)
        {
            col.enabled = false;
        }
    }

    public void SetAliveDefaults()
    {
        isDead = false;
        currentHealth = maxHealth;

        if (isLocalPlayer)
        {
            // Disable scene camera
            SceneCameraScript.instance.SetSceneCameraActive(false);

            // Enable HUD
            if (PlayerGUI.singleton != null)
                playerSetup.playerUIInstance.SetActive(true);

            // Lock mouse
            CursorManagement.CorrectLockMode();

            // Enable behaviours
            foreach (Behaviour behaviour in componentsToDisableOnDeath)
            {
                behaviour.enabled = true;
            }

            Util.SetLayerRecursively(playerArms, LayerMask.NameToLayer(viewmodelLayerName));
        }
        else
        {
            Util.SetLayerRecursively(playerArms, LayerMask.NameToLayer(defaultLayerName));
            Util.SetLayerRecursively(playerBody, LayerMask.NameToLayer(defaultLayerName));
            Util.SetLayerRecursively(playerNameTag, LayerMask.NameToLayer(defaultLayerName));
        }

        // Reset gravity
        GetComponent<Rigidbody>().useGravity = true;

        // Disable colliders
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
        foreach (Collider col in playerColliders)
        {
            col.enabled = true;
        }
    }
    
    [ClientRpc]
    private void RpcCreateRagdoll()
    {
        GameObject ragdollInstance = Instantiate(ragdollPrefab, Util.Round(transform.position, 0.8F), Util.Round(transform.rotation));
        Destroy(ragdollInstance, RAGDOLL_LIFESPAN);
    }
    
    public void SetFirstSetupDefaults()
    {
        if (isLocalPlayer)
        {
            isDead = true;

            // Set body hitboxes to hitable
            //SetHitboxLayer(friendlyLayerName);

            Util.SetLayerRecursively(playerArms, LayerMask.NameToLayer(viewmodelLayerName));
            Util.SetLayerRecursively(playerBody, LayerMask.NameToLayer(dontDrawLayerName));
            Util.SetLayerRecursively(playerNameTag, LayerMask.NameToLayer(dontDrawLayerName));
        }
        else
        {
            Util.SetLayerRecursively(playerArms, LayerMask.NameToLayer(defaultLayerName));
            Util.SetLayerRecursively(playerBody, LayerMask.NameToLayer(defaultLayerName));
            Util.SetLayerRecursively(playerNameTag, LayerMask.NameToLayer(defaultLayerName));

            // Disable components on remote player
            foreach (Behaviour behaviour in componentsToDisableOnRemotePlayer)
            {
                behaviour.enabled = false;
            }

            //// Disable player terrain collider for raycasting
            //foreach (Collider col in playerColliders)
            //{
            //    col.gameObject.layer = LayerMask.NameToLayer(playerColliderLayerName);
            //}

            // Set body hitboxes to hitable
            //SetHitboxLayer(enemyLayerName);

        }

        SetHitboxLayer(enemyLayerName);

        if (isDead)
            SetDeadDefaults();
        else
            SetAliveDefaults();

        // Equip the remote player's class and current weapon
        if (!isDead && !isLocalPlayer) {
            playerClassManager.SetCurrentClassInAction();
            weaponManager.EquipCurrentWeaponInAction();
        }
    }

    private void SetHitboxLayer(string layerName)
    {
        foreach (Collider col in colliders)
        {
            col.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    private void SetKillsOnScoreBoard(int kills)
    {
        ScoreBoardMenu.instance.SetKillValue(kills, netId.Value);
    }

    private void SetDeathsOnScoreBoard(int deaths)
    {
        ScoreBoardMenu.instance.SetDeathsValue(deaths, netId.Value);
    }

    private void OnTeamUpdated(Team team)
    {
        // Update the team variable syncvar
        this.team = team;

        // Correct the scoreboard
        ScoreBoardMenu.instance.ChangeScoreboardEntryTeam(netId.Value, team);
        // Correct the minimap
        if (!GameManager.localPlayer)
            return;
        if (GameManager.localPlayer.team == team)
            playerSetup.SetMiniMapCircleColor(Color.green);
        else
            playerSetup.SetMiniMapCircleColor(Color.red);            
    }

    [Server]
    public void FlagStartTaken()
    {
        TargetStartProgressBar(connectionToClient);
    }

    [Server]
    public void FlagCancelTaken()
    {
        TargetCancelProgressBar(connectionToClient);
    }

    [Server]
    public void FlagTaken()
    {
        hasFlag = true;
    }

    [TargetRpc]
    public void TargetCancelProgressBar(NetworkConnection conn)
    {
        playerSetup.playerGUI.CancelProgressBar(); 
    }

    [TargetRpc]
    public void TargetStartProgressBar(NetworkConnection conn)
    {
        playerSetup.playerGUI.StartProgressBar(Flag.TIME_TO_TAKE_FLAG);
    }

    private void OnDestroy()
    {
        // Destroy scoreboard entry
        ScoreBoardMenu.instance.DestroyScoreboardEntry(netId.Value);
        // Unregister player from GameManager list
        GameManager.UnRegisterPlayer(netId.Value);
    }
}
