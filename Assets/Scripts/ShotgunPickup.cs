using UnityEngine;

public class Shotgun : MonoBehaviour, IPickup
{
    [Range (1, 15)] [SerializeField] private int bulletCount;
    [Range (1, 70)] [SerializeField] private int damage;
    [Range (1, 45)] [SerializeField] private float bulletSpread;
    [Range (1, 20)] [SerializeField] private float bulletSpeed;
    [Range (1, 20)] [SerializeField] private float fireRate;

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
        ShotgunWeapon shotgun = GetComponent<ShotgunWeapon>();

        if (shotgun != null)
        {
            shotgun.SetStats(bulletCount, damage, bulletSpread, bulletSpeed, fireRate);
            shotgun.enabled = true;
        }

        player.AddeWeapon(gameObject);

        transform.SetParent(player.transform);
        transform.localPosition = new Vector3(0, 0.736f, 0);
        transform.localRotation = Quaternion.Euler(0, 0, 90);

        GetComponent<Collider2D>().enabled = false;
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
}
