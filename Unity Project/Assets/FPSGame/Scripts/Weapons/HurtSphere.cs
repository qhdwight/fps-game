using UnityEngine;

public class HurtSphere : MonoBehaviour {

    protected SphereCollider col;

    protected int radius, amount;
    protected uint shooterId;
    protected string weaponName;

    protected Player playerThatCreated;

    public const float FRIENDLY_FIRE_MULTIPLIER = 0.5F;
    public const bool FRIENDLY_FIRE = false, SELF_DAMAGE = true;

    public void SetData(int radius, int amount, uint shooterId, string weaponName)
    {
        playerThatCreated = GameManager.GetPlayer(shooterId);

        this.radius = radius;
        this.amount = amount;
        this.shooterId = playerThatCreated.netId.Value;
        this.weaponName = weaponName;
    }

    protected void Start()
    {
        col = GetComponent<SphereCollider>();
        col.radius = radius;
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (IsPlayerCollider(otherCollider))
        {
            Player player = otherCollider.GetComponentInParent<Player>();

            // No friendly fire
            if ((player.team != playerThatCreated.team || shooterId == player.netId.Value) || FRIENDLY_FIRE)
            {
                float multiplier = player.netId.Value == shooterId ? FRIENDLY_FIRE_MULTIPLIER : 1F;
                int finalDamage = Mathf.RoundToInt(multiplier * amount);

                player.RpcTakeDamage(finalDamage, shooterId, false, weaponName);
            }
        }
    }

    protected bool IsPlayerCollider(Collider col)
    {
        // Check if collider has the player tag and is the capusle collider of the player, not the sphere
        return (col.gameObject.layer == LayerMask.NameToLayer("Player Collider") && col is CapsuleCollider);
    }

}
