using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(PlayerWeaponManager))]
public class PlayerUse : NetworkBehaviour
{
    private const string CHESTTAG = "Chest", LEGTAG = "Leg", HEADTAG = "Head";
    private const float SNIPER_RIFLE_INNACURACY = 0.05F;
    private const float HEADSHOT_MULTIPLIER = 2F, LEG_MULTIPLIER = 0.8F;
    private const float SCOPE_VIEW_MULTIPLIER = 2F;
    private const float SHOTGUN_SPREAD = 0.05F;
    private const ushort NUM_OF_SHOTGUN_PELLETS = 5;

    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask mask;
    [SerializeField] private GameObject scopeCanvasPrefab;
    [SerializeField] public GameObject playerArms;
    [SerializeField] private GameObject blastPrefab, hurtSpherePrefab, liveGrenadePrefab;

    private GameObject scopeCanvasInstance = null;
    private float recoil = 0F, recoilSpeed = 10F, maxRecoilX = -20F;
    private bool scopeIsOpen = false, applyRecoil = false;
    public bool autoScope = false;
    private PlayerWeaponManager weaponManager;
    private PlayerReload playerReload;
    private PlayerTerrainModifier playerTerrainModifier;
    private PlayerMultiplayerMotor playerMotor;
    private Player player;
    private PlayerSetup playerSetup;

    private void Awake()
    {
        weaponManager = GetComponent<PlayerWeaponManager>();
        playerReload = GetComponent<PlayerReload>();
        playerTerrainModifier = GetComponent<PlayerTerrainModifier>();
        playerMotor = GetComponent<PlayerMultiplayerMotor>();
        player = GetComponent<Player>();
        playerSetup = GetComponent<PlayerSetup>();
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (applyRecoil) ApplyRecoil();

            if (weaponManager.currentPlayerItem != null && !CursorManagement.IsMenuOpen() && !ScoreBoardMenu.scoreBoardIsOpen)
            {
                if (weaponManager.currentPlayerItem is PlayerGun)
                {
                    if (Input.GetButton("Fire1"))
                    {
                        PlayerGun currentGun = weaponManager.currentPlayerItem as PlayerGun;
                        // Check if weapon is ready to use
                        if (currentGun.isReadyToUse && !playerReload.Reloading)
                        {
                            // Check if you can shoot on the run
                            if ((!(playerMotor.running && !currentGun.canUseOnRun)) || GameManager.instance.canShootWhileSprinting)
                            {
                                // Not automatic
                                if (currentGun.shotDelay <= 0f)
                                {
                                    if (Input.GetButtonDown("Fire1"))
                                        StartCoroutine(Shoot(currentGun));
                                }
                                // Automatic
                                else
                                {
                                    if (currentGun.isGun || currentGun.isLauncherRaycast)
                                        StartCoroutine(Shoot(currentGun));
                                    else if (currentGun.itemType == WeaponType.Melee)
                                        StartCoroutine(Swing(currentGun));
                                }
                            }
                        }
                    }
                    else if (Input.GetButtonDown("Fire2"))
                    {
                        PlayerGun currentGun = weaponManager.currentPlayerItem as PlayerGun;
                        // Scoping
                        if (currentGun.isSniper && currentGun.isReadyToUse && !ScoreBoardMenu.scoreBoardIsOpen)
                            SetScope(!scopeIsOpen);
                    }
                }
                else if (weaponManager.currentPlayerItem is PlayerGrenade)
                {
                    if (Input.GetButtonDown("Fire1"))
                    {
                        PlayerGrenade playerGrenade = weaponManager.currentPlayerItem as PlayerGrenade;

                        if (playerGrenade.isReadyToUse)
                            StartCoroutine(ThrowGrenade(playerGrenade));
                    }
                }
            }
        }
    }

    private void ApplyRecoil()
    {
        if (recoil > 0)
        {
            Quaternion maxRecoil = Quaternion.Euler(maxRecoilX, 0, 0);
            // Dampen towards the target rotation
            cam.transform.localRotation = Quaternion.Slerp(
                cam.transform.localRotation,
                maxRecoil,
                Time.deltaTime * recoilSpeed);
            recoil -= Time.deltaTime;
        }
        else
        {
            recoil = 0;
            Quaternion minRecoil = Quaternion.Euler(0, 0, 0);
            // Dampen towards the target rotation
            cam.transform.localRotation = Quaternion.Slerp(
                cam.transform.localRotation,
                minRecoil,
                Time.deltaTime * recoilSpeed / 2);
        }
    }

    public void SetRunningAnimation()
    {
        if (!playerReload.Reloading && weaponManager.currentPlayerItem != null)
            weaponManager.playerArmsAnimator.CrossFade(weaponManager.currentPlayerItem.itemName + "Run", 0.2f);
    }

    public void ResetAnimation()
    {
        if (!playerReload.Reloading && weaponManager.currentPlayerItem != null)
            weaponManager.playerArmsAnimator.CrossFade(weaponManager.currentPlayerItem.itemName + "Idle", 0.2f);
    }

    public void SetScope(bool toggleState)
    {
        if (playerReload.Reloading)
            return;

        if (toggleState)
        {
            // Scope on
            scopeCanvasInstance = Instantiate(scopeCanvasPrefab, Vector3.zero, Quaternion.identity);

            // FOV scope effect, zooms in
            cam.fieldOfView *= 1F / SCOPE_VIEW_MULTIPLIER;
            playerMotor.sensitivityX *= 1F / SCOPE_VIEW_MULTIPLIER;
            playerMotor.sensitivityY *= 1F / SCOPE_VIEW_MULTIPLIER;

            // Dont draw arms or guns
            Util.SetLayerRecursively(playerArms, LayerMask.NameToLayer("Dont Draw"));
            scopeIsOpen = true;
        }
        else
        {
            if (scopeCanvasInstance == null)
                return;

            // Scope off
            Destroy(scopeCanvasInstance);

            // FOV scope effect, zooms out
            cam.fieldOfView *= SCOPE_VIEW_MULTIPLIER;
            playerMotor.sensitivityX *= SCOPE_VIEW_MULTIPLIER;
            playerMotor.sensitivityY *= SCOPE_VIEW_MULTIPLIER;

            // Set layers (make current weapon visible)
            Util.SetLayerRecursively(playerArms, LayerMask.NameToLayer(PlayerWeaponManager.VIEWMODEL_LAYER_NAME));
            weaponManager.SetOnlyOneItemLayerAndOthersNotVisible(weaponManager.currentItemInstance, PlayerWeaponManager.VIEWMODEL_LAYER_NAME);
            scopeIsOpen = false;
        }
    }

    [Server]
    private void OnHit(Vector3 position, Vector3 normal, bool hitPlayer, bool playSmackSound)
    {
        RpcDoHitEffect(position, normal, hitPlayer, playSmackSound);
    }

    [ClientRpc]
    private void RpcDoHitEffect(Vector3 position, Vector3 normal, bool hitPlayer, bool playSmackSound)
    {
        GameObject effectInstance;

        if (hitPlayer)
        {
            effectInstance = Instantiate(weaponManager.currentWeaponGraphicsScript.bloodSplatterPrefab, position, Quaternion.LookRotation(normal));
            if (playSmackSound)
            {
                // Play hitting sound
                AudioSource audioSource = weaponManager.currentPlayerItem.GetComponents<AudioSource>()[(int)MeleeSound.Smack];
                audioSource.pitch = Random.Range(0.95F, 1.05F);
                audioSource.Play();
            }

            // Hitmarker
            if (isLocalPlayer) playerSetup.playerGUI.SetHitmarker();
        }
        else
            effectInstance = Instantiate(weaponManager.currentWeaponGraphicsScript.impactEffectPrefab, position, Quaternion.LookRotation(normal));

        Destroy(effectInstance, 2F);
    }

    [Command]
    private void CmdOnThrow(Vector3 pos, Vector3 dir)
    {
        RpcOnThrow(pos, dir);

        // Create live grenade
        if (weaponManager.currentPlayerItem is PlayerGrenade)
        {
            PlayerGrenade grenade = weaponManager.currentPlayerItem as PlayerGrenade;
            CreateGrenade(pos, dir, netId.Value, true);
        }
    }

    [ClientRpc]
    private void RpcOnThrow(Vector3 pos, Vector3 dir)
    {
        if (weaponManager.currentPlayerItem is PlayerGrenade)
        {
            PlayerGrenade grenade = weaponManager.currentPlayerItem as PlayerGrenade;
            if (!isLocalPlayer)
                ThrowEffects(grenade);
        }
    }

    private void ThrowEffects(PlayerGrenade grenade)
    {
        // Animation
        weaponManager.playerArmsAnimator.CrossFade(grenade.itemName + "Throw", 0.05F, 0, 0F);
    }

    [Server]
    private void CreateGrenade(Vector3 pos, Vector3 forward, uint shooterId, bool isOnServer)
    {
        if (weaponManager.currentPlayerItem is PlayerGrenade)
        {
            PlayerGrenade grenade = weaponManager.currentPlayerItem as PlayerGrenade;
            GameObject grenadeInstance = Instantiate(liveGrenadePrefab, pos + forward, transform.rotation);
            LiveGrenade grenadeScript = grenadeInstance.GetComponent<LiveGrenade>();
            grenadeScript.SetDataServer(grenade.id, netId.Value, forward, transform.rotation);
            NetworkServer.Spawn(grenadeInstance);
        }
    }

    private IEnumerator ThrowGrenade(PlayerGrenade grenade)
    {
        {
            grenade.isReadyToUse = false;

            ThrowEffects(grenade);

            yield return new WaitForSeconds(grenade.timeToThrow);

            if (weaponManager.currentPlayerItem.Equals(grenade))
            {
                #if UNITY_EDITOR
                #else
                grenade.currentAmmo -= 1;
                #endif

                CmdOnThrow(cam.transform.position, cam.transform.forward);
            }

            yield return new WaitForSeconds(grenade.throwDelay-grenade.timeToThrow);

            grenade.isReadyToUse = true;

            if (weaponManager.currentPlayerItem.Equals(grenade))
            {
                // Either play animation to get new grenade or switch back to Primary.
                // because we are out of grenades to throw
                if (grenade.currentAmmo > 0)
                    weaponManager.playerArmsAnimator.CrossFade(grenade.itemName + "Retrieve", 0.05F);
                else
                    weaponManager.OnEquipWeapon(WeaponSlot.Primary);
            }
        }
    }

    [Command]
    private void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    [ClientRpc]
    private void RpcDoShootEffect()
    {
        ShootEffects();
    }

    private void ShootEffects()
    {
        if (!isLocalPlayer)
        {
            if (weaponManager.currentPlayerItem && weaponManager.currentWeaponGraphicsScript)
            {
                //Muzzle flash on weapon
                weaponManager.currentWeaponGraphicsScript.muzzleFlash.Play();

                // Weapon graphic kickback
                weaponManager.playerArmsAnimator.CrossFade(weaponManager.currentPlayerItem.itemName + "Shoot", 0.01f, 0, 0F);

                // Play sounds (shooting)
                AudioSource audioSource = weaponManager.currentItemInstance.GetComponents<AudioSource>()[(int)GunSound.Shoot];
                audioSource.pitch = Random.Range(0.95F, 1.05F);
                audioSource.Play();
            }
        }
    }

    private IEnumerator Shoot(PlayerGun weaponUsedToShoot)
    {
        if (weaponUsedToShoot.currentAmmo > 0)
        {
            weaponUsedToShoot.isReadyToUse = false;
            weaponUsedToShoot.currentAmmo -= 1;

            // Muzzle flash on weapon
            weaponManager.currentItemInstance.GetComponent<WeaponGraphics>().muzzleFlash.Play();

            // Weapon shooting animation
            weaponManager.playerArmsAnimator.CrossFade(weaponUsedToShoot.itemName + "Shoot", 0.01F, 0, 0F);

            // Recoil
            recoil += weaponUsedToShoot.recoil;

            // Play sounds (shooting)
            AudioSource audioSource = weaponManager.currentItemInstance.GetComponents<AudioSource>()[(int)GunSound.Shoot];
            audioSource.pitch = Random.Range(0.95F, 1.05F);
            audioSource.Play();

            // Shooting, call the OnShoot method on the server which handles graphics
            CmdOnShoot();

            if (weaponUsedToShoot.isLauncherRaycast)
            {
                SendProjectileStraight(weaponUsedToShoot, netId.Value);
            }
            else if (weaponUsedToShoot.isShotgun)
            {
                SendSpreadRaycast(netId.Value);
            }
            else
            {
                SendBulletRaycast(netId.Value, scopeIsOpen);
            }

            // Toggle scope if sniper
            if (weaponUsedToShoot.isSniper)
                SetScope(false);

            yield return new WaitForSeconds(Mathf.Abs(weaponUsedToShoot.shotDelay));

            weaponUsedToShoot.isReadyToUse = true;

            if (weaponManager.currentPlayerItem == weaponUsedToShoot && weaponUsedToShoot.isSniper && autoScope && !player.isDead)
                SetScope(true);
        }
        else
        {
            // Play sounds (out of ammo)
            AudioSource audioSource = weaponManager.currentItemInstance.GetComponents<AudioSource>()[(int)GunSound.OutOfAmmo];
            audioSource.pitch = Random.Range(0.95F, 1.05F);
            audioSource.Play();

            // Reload automatically
            playerReload.Reload(weaponUsedToShoot);
        }
    }

    [Command]
    private void CmdOnSwing()
    {
        RpcOnSwing();
    }

    [ClientRpc]
    private void RpcOnSwing()
    {
        SwingEffects(weaponManager.currentPlayerItem as PlayerGun);
    }

    private void SwingEffects(PlayerGun gun)
    {
        if (!isLocalPlayer)
        {
            // Play sounds (swishing sound)
            AudioSource audioSource = gun.gameObject.GetComponents<AudioSource>()[0];
            audioSource.pitch = Random.Range(0.95F, 1.05F);
            audioSource.Play();

            //Play swing animation for viewmodel
            weaponManager.playerArmsAnimator.Play(gun.itemName + "Swing");
        }
    }

    private IEnumerator Swing(PlayerGun weaponUsedToSwing)
    {

        weaponUsedToSwing.isReadyToUse = false;

        // Play sounds (swish)
        AudioSource audioSource = weaponUsedToSwing.gameObject.GetComponents<AudioSource>()[(int)MeleeSound.Swing];
        audioSource.pitch = Random.Range(0.95F, 1.05F);
        audioSource.Play();

        // Play swing animation for viewmodel
        weaponManager.playerArmsAnimator.Play(weaponUsedToSwing.itemName + "Swing");

        // Call OnSwing method on server
        CmdOnSwing();

        // If the weapon can break blocks(shovel) then break block
        yield return new WaitForSeconds(0.16F);
        if (weaponUsedToSwing.canEditBlocks)
            playerTerrainModifier.DestroyBlock();

        SendBulletRaycast(netId.Value);

        yield return new WaitForSeconds(weaponUsedToSwing.shotDelay);

        weaponUsedToSwing.isReadyToUse = true;
    }

    private void SendProjectileStraight(PlayerGun gun, uint shooterId)
    {
        CmdProjectileStraightLaunched(shooterId);
    }

    #region Spread Raycast

    private void SendSpreadRaycast(uint shooterId)
    {
        CmdSpreadRaycast(shooterId);
    }

    [Command]
    private void CmdSpreadRaycast(uint shooterId)
    {
        SpreadRaycast(shooterId);
    }

    [Server]
    private void SpreadRaycast(uint shooterId)
    {
        for (ushort i = 0; i < NUM_OF_SHOTGUN_PELLETS; i++)
        {
            // Add random spread to ray
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            ray = Util.AddRandomToRay(ray, -SHOTGUN_SPREAD, SHOTGUN_SPREAD);

            BulletRaycast(weaponManager.currentPlayerItem as PlayerGun, shooterId, false, ray);
        }
    }

    #endregion

    #region Bullet Raycast

    private void SendBulletRaycast(uint shooterId, bool isScopedIn = false)
    {
        CmdBulletRaycast(netId.Value, isScopedIn);
    }

    [Command]
    private void CmdBulletRaycast(uint shooterId, bool isScopedIn)
    {
        BulletRaycast(weaponManager.currentPlayerItem as PlayerGun, shooterId, isScopedIn);
    }

    [Server]
    private void BulletRaycast(PlayerGun gun, uint shooterId, bool isScopedIn, Ray? ray = null)
    {
        if (!ray.HasValue)
        {
            ray = new Ray(cam.transform.position, cam.transform.forward);

            // Add random variation if it is a sniper rifle and it is unscoped
            if (gun.isSniper && !isScopedIn) ray = Util.AddRandomToRay(ray.Value, -SNIPER_RIFLE_INNACURACY, SNIPER_RIFLE_INNACURACY);
        }

        // Temporarily add ourselves to friendly layer during the raycast
        player.SetHitboxLayer(player.friendlyLayerName);

        // Raycast for swinging and hitting players
        RaycastHit hit;
        if (Physics.Raycast(ray.Value, out hit, gun.range, mask))
        {
            string tag = hit.collider.tag;
            if (tag == HEADTAG || tag == CHESTTAG || tag == LEGTAG)
            {
                // Get hit player
                Player hitPlayer = hit.collider.GetComponentInParent<Player>();

                // Check that we didn't hit ourselfs or teammate
                if (hitPlayer.netId.Value == shooterId || hitPlayer.team == player.team)
                    return;

                bool headshot = false;
                float multiplier = 1F;
                float dropOffMultiplier = gun.isShotgun ? CalculateDropoffDamage(gun, hit.distance) : 1F;
                if (tag == HEADTAG)
                {
                    headshot = true;
                    multiplier = HEADSHOT_MULTIPLIER;
                }
                else if (tag == LEGTAG)
                {
                    multiplier = LEG_MULTIPLIER;
                }

                PlayerShot(
                    hitPlayer.netId.Value,
                    shooterId,
                    Mathf.RoundToInt(gun.damage * multiplier * dropOffMultiplier),
                    headshot,
                    gun.itemName
                );
                OnHit(hit.point, hit.normal, true, !gun.isGun);
            }
            else
            {
                OnHit(hit.point, hit.normal, false, false);
            }
        }

        // Move ourselves back to enemy layer
        player.SetHitboxLayer(player.enemyLayerName);
    }

    private int CalculateDropoffDamage(PlayerGun gun, float distance)
    {
        return Mathf.Abs(Mathf.RoundToInt(gun.damage * (1F - Mathf.Sqrt(1/gun.range) * Mathf.Sqrt(distance))));
    }

    #endregion

    [Command]
    private void CmdProjectileStraightLaunched(uint shooterId)
    {
        Player player = GameManager.GetPlayer(shooterId);
        PlayerMotor playerMotor = player.GetComponent<PlayerMotor>();
        PlayerGun gun = weaponManager.currentPlayerItem as PlayerGun;
        float height = gun.GetComponent<StraightProjectileGraphics>().projectilePrefab.GetComponent<CapsuleCollider>().height;
        RpcProjectileStraightLaunched(
            cam.transform.position + cam.transform.forward * height,
            cam.transform.forward,
            shooterId);
    }

    [ClientRpc]
    public void RpcProjectileStraightLaunched(Vector3 pos, Vector3 dir, uint shooterId)
    {
        ProjectleStraightLaunched(
            weaponManager.currentPlayerItem as PlayerGun,
            pos,
            dir,
            shooterId);
    }

    public void ProjectleStraightLaunched(PlayerGun weapon, Vector3 pos, Vector3 dir, uint shooterId)
    {
        StraightProjectileGraphics weaponGraphics = weapon.GetComponent<StraightProjectileGraphics>();

        if (weaponGraphics != null)
        {
            // Create projectile
            GameObject projectile = Instantiate(weaponGraphics.projectilePrefab, pos, Quaternion.identity);
            projectile.transform.forward = dir;

            // Give projectile data
            StraightProjectile projectileScript = projectile.GetComponent<StraightProjectile>();
            if (projectileScript != null) projectileScript.SetData(isServer, shooterId);
        }
    }

    [Server]
    private void PlayerShot(uint playerShotID, uint shooterID, int damage, bool headshot, string weaponName)
    {
        Player playerShot = GameManager.GetPlayer(playerShotID);
        playerShot.RpcTakeDamage(damage, shooterID, headshot, weaponName);
    }

    private void OnDisable()
    {
        // Disable the scope when the player is killed
        if (isLocalPlayer)
            SetScope(false);
    }
}
