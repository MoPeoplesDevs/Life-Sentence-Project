using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] int healAmount;

    public void Pickup()
    {
        IHeal heal = GameObject.FindGameObjectWithTag("Player").GetComponent<IHeal>();
        if (heal != null)
        {
            heal.Heal(healAmount);
            Destroy(gameObject);
        }
    }


}
