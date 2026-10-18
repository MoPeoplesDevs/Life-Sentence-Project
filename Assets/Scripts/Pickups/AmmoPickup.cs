using UnityEngine;

public class AmmoPickup : MonoBehaviour, IPickup
{
    public enum AmmoType
    {
        MachineGun,
        Shotgun,
        Sniper
    }

    [Header("Ammo Settings")]
    [SerializeField] private AmmoType ammoType = AmmoType.MachineGun;

    [Range(1, 200)][SerializeField] private int ammoAmount = 30;

    public void Pickup()
    {
        GameObject player_object = GameObject.FindGameObjectWithTag("Player");

        if (player_object == null)
        {
            return;
        }

        Playercontroller player = player_object.GetComponent<Playercontroller>();

        if (player == null)
        {
            Debug.LogError("Ammo pickup: Playercontroller not found!");
            return;
        }
        Debug.Log("Ammo pickup: Playercontroller found!");

        bool ammoAdded = false;

        switch (ammoType)
        {
            case AmmoType.MachineGun:
                ammoAdded = player.AddAmmoToWeapon(AmmoType.MachineGun, ammoAmount);
                break;

            case AmmoType.Shotgun:
                ammoAdded = player.AddAmmoToWeapon(AmmoType.Shotgun, ammoAmount);
                break;

            case AmmoType.Sniper:
                ammoAdded = player.AddAmmoToWeapon(AmmoType.Sniper, ammoAmount);
                break;
        }

        if (ammoAdded)
        {
            Destroy(gameObject);
        }
       
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }
}