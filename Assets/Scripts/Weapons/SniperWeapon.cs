using UnityEngine;
using UnityEngine.InputSystem;

public class SniperWeapon : MonoBehaviour, IWeapon
{
    [Header("Sniper Stats")]
    [Range(1, 100)][SerializeField] private int damage = 75;
    [Range(1, 50)][SerializeField] private float bulletSpeed = 35f;
    [Range(0.1f, 5f)][SerializeField] private float fireRate = 1f;
    [Range(1f, 10f)][SerializeField] private float bulletLifetime = 5f;

    [Header("Bullet Settings")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform firePoint;

    private float nextFireTime;
    public bool sniperActive;

    public void SetStats(int damage, float bulletSpeed, float fireRate)
    {
        this.damage = damage;
        this.bulletSpeed = bulletSpeed;
        this.fireRate = fireRate;

        sniperActive = true;
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
        SetStats(damage, bulletSpeed, fireRate);

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
        if (!sniperActive)
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
        GameObject newBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);

        Destroy(newBullet, bulletLifetime);

        Bullet bulletScript = newBullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetStats(damage, bulletSpeed);
        }
    }

}