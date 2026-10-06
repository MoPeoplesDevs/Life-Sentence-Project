using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamage
{

    [SerializeField] int enemyHP;
    [SerializeField] private Image healthBar;

    private int enemyMaxHP;

    public void Start()
    {
        enemyMaxHP = enemyHP;
    }


    public void TakeDamage(int damage)
    {
        enemyHP -= damage;
        healthBar.fillAmount = (float)enemyHP / enemyMaxHP;

        if (enemyHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
