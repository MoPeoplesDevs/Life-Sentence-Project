using UnityEngine;

public class DamagePickup : MonoBehaviour
{
    [SerializeField] private int damageAmount;

    public void Pickup()
    {
        IDamage damage = GameObject.FindGameObjectWithTag("Player").GetComponent<IDamage>();
        if (damage != null)
        {
            damage.TakeDamage(damageAmount);
            Destroy(gameObject);
        }
    }
}