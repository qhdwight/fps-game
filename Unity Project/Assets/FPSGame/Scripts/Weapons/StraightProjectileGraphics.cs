using UnityEngine;

public class StraightProjectileGraphics : MonoBehaviour
{

    [SerializeField] public GameObject projectilePrefab, projectileOnWeapon;

    public void HideProjectileOnWeapon()
    {
        if (projectileOnWeapon != null) projectileOnWeapon.SetActive(false);
    }

    public void ShowProjectileOnWeapon()
    {
        if (projectileOnWeapon != null) projectileOnWeapon.SetActive(true);
    }
}
