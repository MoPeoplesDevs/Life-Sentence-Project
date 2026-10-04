using UnityEngine;

public class Sniper : Enemy
{
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] float fireRate = 1f;
    [SerializeField] float projectileSpeed = 6f;
    [SerializeField] int projectileDamage = 10;

    private float lastAttack;

    void Update()
    {
        if (GameManager.Instance.IsGameOver || buttonFunctions.IsPaused) return;

        Vector3 dir = (player.position - transform.position).normalized;
        
        if (CanSeePlayer(dir))
            Shoot(dir);
    }

    private void Shoot(Vector3 plrDirection)
    {
        if (Time.time - lastAttack < fireRate) return;
        lastAttack = Time.time;

        EnemyProjectile projectile = Instantiate(
            projectilePrefab, transform.position + (transform.forward * 2), Quaternion.identity
        );
        projectile.Initialize(plrDirection, projectileSpeed, projectileDamage);
    }
}