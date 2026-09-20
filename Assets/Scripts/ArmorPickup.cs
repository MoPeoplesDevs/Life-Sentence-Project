using UnityEngine;

public class ArmorPickup : MonoBehaviour
{
    [SerializeField] private int armorAmount;

    public void Pickup()
    {
        IArmor armor = GameObject.FindGameObjectWithTag("Player").GetComponent<IArmor>();
        if (armor != null)
        {
            armor.AddArmor(armorAmount);
            Destroy(gameObject);
        }
    }


}
