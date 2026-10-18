using UnityEngine;

public class HealthPickup : MonoBehaviour, IPickup
{
    [Header("Health Settings")]
    [Range(1, 100)][SerializeField] private int healthAmount;

    public void Pickup()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        IHeal heal = playerObject.GetComponent<IHeal>();

        if (heal != null)
        {
            heal.Heal(healthAmount);
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
