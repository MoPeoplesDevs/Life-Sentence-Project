using UnityEngine;

public class DefaultWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 5f;

    private int currentAmmo;
    private int reserveAmmo;

    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;

    private float nextFireTime;

    public void Shoot()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + (1f / fireRate);

        Instantiate(
            bullet,
            firePoint.position,
            firePoint.rotation
        );
    }
}
