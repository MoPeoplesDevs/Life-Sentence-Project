using UnityEngine;

public class HordeEnemyDamage : MonoBehaviour
{

    [SerializeField] int damage = 1;

    void OnCollisionEnter2D(Collision2D collision)
    {
        IDamage damageable = collision.gameObject.GetComponent<IDamage>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}
