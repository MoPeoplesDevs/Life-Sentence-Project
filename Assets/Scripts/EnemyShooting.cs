using UnityEngine;

public class EnemyShooting : MonoBehaviour
{

    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform player;
    [SerializeField] float fireRate = 1f;

    float fireTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.Instance.IsGameOver) return;

        fireTimer += Time.deltaTime;

        if(fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    void Shoot()
    {
        Vector2 direction = (player.position - firePoint.position).normalized;

        EnemyProjectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        projectile.GetComponent<EnemyProjectile>().SetDirection(direction);
    } 
}
