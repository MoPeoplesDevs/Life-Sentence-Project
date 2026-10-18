using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RiotGuard : Enemy
{
    [Header("Shield")]
    [SerializeField] int shieldHits = 5;
    [SerializeField] GameObject[] shieldVisuals;
    [SerializeField] float brokenSpeedMultiplier = 1.25f;

    [Header("Shield Bash")]
    [SerializeField] float bashDamage = 3f;
    [SerializeField] float bashRange = 1.3f;
    [SerializeField] float bashCooldown = 1.2f;

    [Header("Shotgun")]
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] float projectileSpeed = 9f;
    [SerializeField] float projectileDamage = 4f;
    [SerializeField] int pelletCount = 6;
    [SerializeField] float spreadAngle = 40f;

    [SerializeField] int magSize = 5;
    [SerializeField] int reloadTime = 5;

    [Header("Movement")]
    [SerializeField] float minDistance = 3f;
    [SerializeField] float maxDistance = 6f;
    [SerializeField] float repathInterval = 0.5f;
    [SerializeField] float retreatSpeedMultiplier = 1.6f;

    private int remainingShieldHits;
    private float lastAttack;
    private float nextRepath;
    private bool shieldBroken;
    private int ammoRemaining;
    private bool isReloading;
    private bool retreatingAfterBreak;

    private string variant;
    private EnemyAnimator animator;

    void Start()
    {
        remainingShieldHits = shieldHits;
        ammoRemaining = magSize;

        Difficulty difficulty = GameManager.Instance.gameDifficulty;
        switch (difficulty)
        {
            case Difficulty.Easy:
                projectileDamage *= 0.8f;
                break;
            case Difficulty.Hard:
                projectileDamage *= 1.2f;
                fireRate *= 0.9f;
                break;
            case Difficulty.Expert:
                projectileDamage *= 1.5f;
                fireRate *= 0.75f;
                break;
        }

        variant = $"Tone{Random.Range(1, 4)}";
        animator = gameObject.AddComponent<EnemyAnimator>();
        animator.Initialize(variant, this, "RiotGuard");

        if (remainingShieldHits <= 0)
            BreakShield();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver || buttonFunctions.IsPaused) return;

        Vector3 direction = player.position - transform.position;
        direction.z = 0f;

        float distance = direction.magnitude;
        direction.Normalize();

        bool canSeePlayer = CanSeePlayer(direction, distance);

        if (retreatingAfterBreak && distance >= maxDistance)
            retreatingAfterBreak = false;

        if (!shieldBroken)
        {
            if (distance <= bashRange && canSeePlayer)
                ShieldBash();
        }
        else if (!retreatingAfterBreak && distance <= maxDistance && canSeePlayer && !isReloading)
        {
            Shoot(direction);
        }

        if (Time.time >= nextRepath)
        {
            nextRepath = Time.time + repathInterval;

            float targetDistance = shieldBroken ? maxDistance : bashRange;

            if (!retreatingAfterBreak && (distance > targetDistance || !canSeePlayer))
                GetPathToPlayer();
            else
                pathState.path = null;
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver || buttonFunctions.IsPaused) return;
        Move();
    }

    public override void TakeDamage(float damage)
    {
        SFXManager.Instance.PlaySFXAt(hitmarker, transform, 0.35f);

        if (!shieldBroken)
        {
            remainingShieldHits--;

            if (remainingShieldHits <= 0)
                BreakShield();

            return;
        }

        base.TakeDamage(damage);
    }

    private void ShieldBash()
    {
        if (Time.time - lastAttack < bashCooldown) return;
        lastAttack = Time.time;

        IDamage damageable = player.GetComponent<IDamage>();
        if (damageable != null)
            damageable.TakeDamage(bashDamage);

        // Don't call base.Attack(): that triggers the shotgun attack sprite.
    }

    private void BreakShield()
    {
        if (shieldBroken) return;

        shieldBroken = true;
        remainingShieldHits = 0;
        retreatingAfterBreak = true;

        SPEED *= brokenSpeedMultiplier;
        pathState.path = null;
        nextRepath = 0f;

        if (animator != null)
        {
            animator.enabled = false;
            Destroy(animator);
        }

        animator = gameObject.AddComponent<EnemyAnimator>();
        animator.Initialize(variant, this, "RiotGuardWithoutShield");
    }

    private void Shoot(Vector3 direction)
    {
        if (!shieldBroken || retreatingAfterBreak || isReloading) return;
        if (Time.time - lastAttack < fireRate) return;
        lastAttack = Time.time;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = pelletCount > 1 ? Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, (float)i / (pelletCount - 1)) : 0f;

            angle += Random.Range(-3f, 3f);
            Vector3 pelletDirection = Quaternion.Euler(0f, 0f, angle) * direction;

            EnemyProjectile projectile = Instantiate(projectilePrefab, transform.position + direction * 0.7f, Quaternion.identity);
            projectile.Initialize(pelletDirection.normalized, projectileSpeed, projectileDamage);
        }

        base.Attack();

        ammoRemaining--;
        if (ammoRemaining <= 0)
            StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        ammoRemaining = magSize;
        isReloading = false;
    }

    protected override void Move()
    {
        Vector3 direction = player.position - transform.position;
        direction.z = 0f;

        float distance = direction.magnitude;
        if (distance <= 0.01f) return;

        direction.Normalize();

        Vector3 movement = Vector3.zero;
        float moveSpeed = SPEED;
        bool canSeePlayer = CanSeePlayer(direction, distance);

        if (!shieldBroken)
        {
            // Approach until close enough to shield bash.
            if (distance > bashRange || !canSeePlayer)
            {
                if (canSeePlayer)
                    movement = direction;
                else if (pathState.path != null && pathState.index < pathState.path.Count)
                    movement = GetMovementDirectionFromPath();
                else
                    movement = direction;
            }
        }
        else if (retreatingAfterBreak && distance < maxDistance)
        {
            // Retreat quickly before starting to shoot.
            movement = -direction;
            moveSpeed *= retreatSpeedMultiplier;
            pathState.path = null;
        }
        else if (distance < minDistance)
        {
            movement = -direction;
            pathState.path = null;
        }
        else if (distance > maxDistance || !canSeePlayer)
        {
            if (pathState.path != null && pathState.index < pathState.path.Count)
                movement = GetMovementDirectionFromPath();
            else
                movement = direction;
        }

        if (movement.sqrMagnitude > 0.0001f)
        {
            Vector2 position = rigidBody.position + (Vector2)(movement.normalized * moveSpeed * Time.fixedDeltaTime);
            rigidBody.MovePosition(position);
            base.Move();
        }
    }
}