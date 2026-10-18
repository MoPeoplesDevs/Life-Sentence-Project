using UnityEngine;
using UnityEngine.InputSystem;

public class MachineGun : MonoBehaviour, IWeapon
{
    [Header("Machine Gun Stats")]
    [Range(1, 70)][SerializeField] private int damage = 10;
    [Range(1, 50)][SerializeField] private float bulletSpeed = 20f;
    [Range(0.01f, 15f)][SerializeField] private float fireRate = 0.1f;
    [Range(1f, 20f)][SerializeField] private float bulletLifetime = 5f;

    [Header("Ammo Settings")]
    [Range(1, 100)][SerializeField] private int magazineSize;
    [Range(0, 500)][SerializeField] private int maxReserveAmmo;
    [Range(0.1f, 10f)][SerializeField] private float reloadTime;

    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading;

    [Header("Bullet Settings")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform firePoint;

    private float nextFireTime;
    public bool machineGunActive;

    private void Start()
    {
        currentAmmo = magazineSize;
        reserveAmmo = maxReserveAmmo;
    }

    public void SetStats(int damage, float bulletSpeed, float fireRate)
    {
        this.damage = damage;
        this.bulletSpeed = bulletSpeed;
        this.fireRate = fireRate;

        machineGunActive = true;
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
        if (!machineGunActive)
        {
            return;
        }
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / fireRate);
            Shoot();
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Reload();
        }
    }

    public void Shoot()
    {
        if (isReloading || currentAmmo <= 0)
        {
            return;
        }

        currentAmmo--;

        GameObject newBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);

        Destroy(newBullet, bulletLifetime);

        Bullet bulletScript = newBullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetStats(damage, bulletSpeed);
        }
    }

    public bool AddAmmo(int amount)
    {
        if (reserveAmmo >= maxReserveAmmo)
        {
            return false;
        }

        reserveAmmo += amount;

        if (reserveAmmo > maxReserveAmmo)
        {
            reserveAmmo = maxReserveAmmo;
        }

        return true;
    }

    public void Reload()
    {
        if (isReloading || currentAmmo >= magazineSize || reserveAmmo <= 0)
        {
            return;
        }

        isReloading = true;

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;

        isReloading = false;

        Debug.Log("Reloaded. Magazine: " + currentAmmo +
                  " | Reserve: " + reserveAmmo);
    }


}