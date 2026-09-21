using UnityEngine;

public class HordeEnemyDamage : MonoBehaviour
{

    [SerializeField] int damage = 1;

    void OnCollisionEnter2D(Collision2D collision)
    {
        Playercontroller player = collision.gameObject.GetComponent<Playercontroller>();

        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
}
