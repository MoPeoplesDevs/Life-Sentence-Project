using UnityEngine;

public class Sniper : Enemy
{
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] float fireRate = 1f;
    [SerializeField] float projectileSpeed = 6f;
    [SerializeField] float projectileDamage = 10;

    private float lastAttack;

    void Start()
    {
        // Difficulty
        Difficulty difficulty = GameManager.Instance.gameDifficulty;
        switch (difficulty)
        {
            case Difficulty.Easy:
                fireRate *= 0.8f;
                projectileDamage *= 0.8f;
                break;
            case Difficulty.Medium:
                fireRate *= 1f;
                projectileDamage *= 1f;
                break;
            case Difficulty.Hard:
                fireRate *= 1.2f;
                projectileSpeed *= 1.1f;
                projectileDamage *= 1.2f;
                break;
            case Difficulty.Expert:
                fireRate *= 1.5f;
                projectileDamage *= 1.5f;
                projectileSpeed *= 1.25f;
                break;
        }
    }

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