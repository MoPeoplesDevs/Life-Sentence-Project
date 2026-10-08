using UnityEngine;

public class HordeEnemyDamage : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float damageCooldown = 1f;

    private float lastDamageTime;

    void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < lastDamageTime + damageCooldown) return;

        Playercontroller player = collision.gameObject.GetComponent<Playercontroller>();

        if (player != null)
        {
            player.TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }
}