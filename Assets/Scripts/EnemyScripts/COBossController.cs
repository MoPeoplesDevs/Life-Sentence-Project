using UnityEngine;

public class COBossController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] Collider2D shieldCollider;
    [SerializeField] Transform firePoint;
    [SerializeField] EnemyProjectile rubberBallPrefab;

    [Header("Shotgun")]
    [SerializeField, Min(1)] int pelletCount = 5;
    [SerializeField] float spreadAngle = 40f;
    [SerializeField] float projectileSpeed = 8f;
    [SerializeField] float projectileDamage = 8f;

    BossHealth health;

    public bool IsAlive { get; private set; } = true;

    void Awake()
    {
        health = GetComponent<BossHealth>();

        if (health != null)
            health.OnBossDeath += HandleDeath;
    }

    void HandleDeath()
    {
        IsAlive = false;
        SetShield(false);
    }

    public void SetShield(bool raised)
    {
        if (shieldCollider != null)
            shieldCollider.enabled = raised && IsAlive;
    }

    public void FaceDirection(Vector2 direction)
    {
        if (!IsAlive || direction.sqrMagnitude < 0.001f)
            return;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void FireShotgun()
    {
        if (!IsAlive || firePoint == null || rubberBallPrefab == null) return;

        int count = Mathf.Max(1, pelletCount);

        for (int i = 0; i < count; i++)
        {
            float angle = count == 1 ? 0f : Mathf.Lerp(
                -spreadAngle * 0.5f,
                spreadAngle * 0.5f,
                i / (float)(count - 1));

            Quaternion rotation =
                transform.rotation * Quaternion.Euler(0f, 0f, angle);

            Vector2 direction = rotation * Vector3.up;

            EnemyProjectile projectile = Instantiate(
                rubberBallPrefab, 
                firePoint.position,
                rotation);

            projectile.Initialize(
                direction,
                projectileSpeed,
                projectileDamage);
        }
    }

    void OnDisable()
    {
        SetShield(false);
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnBossDeath -= HandleDeath;
    }
}
