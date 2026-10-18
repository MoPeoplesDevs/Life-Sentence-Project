using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamage
{

    [SerializeField] float enemyHP;
    [SerializeField] private Image healthBar;

    private float enemyMaxHP;

    public void Start()
    {
        enemyMaxHP = enemyHP;
    }


    public void TakeDamage(float damage)
    {
        enemyHP -= damage;

        if (healthBar != null)
            healthBar.fillAmount = enemyHP / enemyMaxHP;

        if (enemyHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
