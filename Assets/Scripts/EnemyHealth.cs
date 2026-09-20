using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamage
{

    [SerializeField] int enemyHP;

    public void TakeDamage(int damage)
    {
        enemyHP -= damage;

        if (enemyHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
