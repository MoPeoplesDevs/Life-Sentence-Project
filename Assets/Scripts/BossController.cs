using UnityEngine;

public class BossController : MonoBehaviour
{

    enum BossState
    {
        Idle,
        AttackOne,
        AttackTwo
    }

    [SerializeField] float attackCooldown = 2f;
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform player;

    BossState currentState = BossState.Idle;
    float attackTimer;

    bool phaseTwo = false;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            ChooseAttack();
            attackTimer = 0f;
        }
    }

    void ChooseAttack()
    {
        int attackChoice = Random.Range(0, 2);

        if (attackChoice == 0)
        {
            currentState = BossState.AttackOne;
        }
        else
        {
            currentState = BossState.AttackTwo;
        }

        Debug.Log("Boss State: " + currentState);
    }

    public void EnterPhaseTwo()
    {
        phaseTwo = true;
        attackCooldown = 1f;

        Debug.Log("Boss entered Phase Two");
    }
}
