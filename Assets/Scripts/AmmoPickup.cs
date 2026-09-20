using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private int ammoAmount;

    public void Pickup()
    {
        IAmmo ammo = GameObject.FindGameObjectWithTag("Player").GetComponent<IAmmo>();
        if (ammo != null)
        {
            ammo.AddAmmo(ammoAmount);
            Destroy(gameObject);
        }
    }

}
