using UnityEngine;

public class EnemyWeaponDrop : MonoBehaviour
{
    [Header("Boss Weapon Drop")]
    [SerializeField] private GameObject weaponPickup;

    public void DropWeapon()
    {
        if (weaponPickup == null)
        {
            Debug.LogWarning("Boss weapon pickup has not been assigned!");
            return;
        }

        Instantiate(
            weaponPickup,
            transform.position,
            Quaternion.identity
        );
    }
}