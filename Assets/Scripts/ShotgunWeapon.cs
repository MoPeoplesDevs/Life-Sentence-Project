using UnityEngine;

public class ShotgunWeapon : MonoBehaviour
{
    [Range(1, 15)] [SerializeField] private int bulletCount;
    [Range(1, 70)] [SerializeField] private int damage;
    [Range(1, 45)] [SerializeField] private float bulletSpread;
    [Range(1, 20)] [SerializeField] private float bulletSpeed;
    [Range(1, 20)] [SerializeField] private float fireRate;


    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform firePoint;

    private float nextFireTime;
    private bool shotgunActive;



    public void SetStats(int bulletCount, int damage, float bulletSpread, float bulletSpeed, float fireRate)
    {
        this.bulletCount = bulletCount;
        this.damage = damage;
        this.bulletSpread = bulletSpread;
        this.bulletSpeed = bulletSpeed;
        this.fireRate = fireRate;

        shotgunActive = true;
    }

    private void Update()
    {
        if (!shotgunActive)
        {
            return;
        }
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / fireRate);
            Shoot();
        }
    }

    private void Shoot()
    {
        Debug.Log("SHOTGUN SHOOTING: " + bulletCount + " bullets");

        for (int i = 0; i < bulletCount; i++)
        {
            float spread = Random.Range(-bulletSpread, bulletSpread);

            Quaternion bulletRotation =
                firePoint.rotation * Quaternion.Euler(0, 0, spread);

            GameObject newBullet =
                Instantiate(bullet, firePoint.position, bulletRotation);

            Bullet bulletScript = newBullet.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                bulletScript.SetStats(damage, bulletSpeed);
            }
        }
    }


}
