using UnityEngine;
using UnityEngine.InputSystem;

public class ShotgunWeapon : MonoBehaviour, IWeapon
{
    [Range(1, 15)] [SerializeField] private int bulletCount;
    [Range(1, 70)] [SerializeField] private int damage;
    [Range(1, 45)] [SerializeField] private float bulletSpread;
    [Range(1, 20)] [SerializeField] private float bulletSpeed;
    [Range(1, 20)] [SerializeField] private float fireRate;
    [Range(.1f, 10f)] [SerializeField] private float bulletLifetime;


    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform firePoint;

    private float nextFireTime;
    public bool shotgunActive;



    public void SetStats(int bulletCount, int damage, float bulletSpread, float bulletSpeed, float fireRate)
    {
        this.bulletCount = bulletCount;
        this.damage = damage;
        this.bulletSpread = bulletSpread;
        this.bulletSpeed = bulletSpeed;
        this.fireRate = fireRate;

        shotgunActive = true;
    }

    public void Pickup()
    {
        Playercontroller player = FindObjectOfType<Playercontroller>();

        if (player != null)
        {
            GiveToPlayer(player);
        }
    }

    private void GiveToPlayer(Playercontroller player)
    {
        SetStats(
            bulletCount,
            damage,
            bulletSpread,
            bulletSpeed,
            fireRate);

        player.AddWeapon(gameObject);

        transform.SetParent(player.transform);
        transform.localPosition = new Vector3(0, 0.736f, 0);
        transform.localRotation = Quaternion.Euler(0, 0, 90);

        Collider2D collider = GetComponent<Collider2D>();

        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Playercontroller player = other.GetComponent<Playercontroller>();

            if (player != null)
            {
                GiveToPlayer(player);
            }
        }
    }




    private void Update()
    {
        if (!shotgunActive)
        {
            return;
        }
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / fireRate);
            Shoot();
        }
    }

    public void Shoot()
    {
        Debug.Log("SHOTGUN SHOOTING: " + bulletCount + " bullets");

        for (int i = 0; i < bulletCount; i++)
        {
            float spread = Random.Range(-bulletSpread, bulletSpread);

            Quaternion bulletRotation =
                firePoint.rotation * Quaternion.Euler(0, 0, spread);

            GameObject newBullet =
                Instantiate(bullet, firePoint.position, bulletRotation);

            Destroy(newBullet, bulletLifetime);

            Bullet bulletScript = newBullet.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                bulletScript.SetStats(damage, bulletSpeed);
            }
        }
    }


}
