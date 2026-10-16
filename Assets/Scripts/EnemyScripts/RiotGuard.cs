
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RiotGuard : Enemy
{
    [Header("Shield")]
    [SerializeField] int shieldHits = 5;
    [SerializeField] GameObject[] shieldVisuals;
    [SerializeField] float brokenSpeedMultiplier = 1.25f;

    [Header("Shotgun")]
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] float projectileSpeed = 9f;
    [SerializeField] float projectileDamage = 4f;
    [SerializeField] int pelletCount = 6;
    [SerializeField] float spreadAngle = 40f;

    [Header("Movement")]
    [SerializeField] float minDistance = 3f;
    [SerializeField] float maxDistance = 6f;
    [SerializeField] float repathInterval = 0.5f;

    [SerializeField] int magSize = 5;
    [SerializeField] int reloadTime = 5;

    private int remainingShieldHits;
    private float lastAttack;
    private float nextRepath;
    private bool shieldBroken;
    private int ammoRemaining;
    private bool isReloading = false;

    //
    private string variant;
    private EnemyAnimator animator;

    void Start()
    {
        remainingShieldHits = shieldHits;

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

        if (remainingShieldHits <= 0)
            BreakShield();

        //
        variant = $"Tone{UnityEngine.Random.Range(1, 4)}";
        animator = gameObject.AddComponent<EnemyAnimator>();
        animator.Initialize(variant, this, "RiotGuard");
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver || buttonFunctions.IsPaused) return;
               
        Vector3 difference = (player.position - transform.position);
        Vector3 direction = difference.normalized;
        float distance = difference.magnitude;
        bool canSeePlayer = CanSeePlayer(direction, distance);

        // Attack independently of movement
        if (distance <= maxDistance && canSeePlayer && !isReloading)
            Shoot(direction);

        // Only recalculate paths periodically
        if (Time.time >= nextRepath)
        {
            nextRepath = Time.time + repathInterval;

            if (distance > maxDistance || !canSeePlayer)
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

    private void BreakShield()
    {
        if (shieldBroken) return;

        Destroy(animator);
        animator = gameObject.AddComponent<EnemyAnimator>();
        animator.Initialize(variant, this, "RiotGuardWithoutShield");

        shieldBroken = true;
        remainingShieldHits = 0;

        SPEED *= brokenSpeedMultiplier;
    }

    private void Shoot(Vector3 direction)
    {
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

        if (transform.gameObject != null)
        {
            isReloading = false;
            ammoRemaining = magSize;
        }
    }

    protected override void Move()
    {
        Vector3 direction = player.position - transform.position;
        direction.z = 0f;

        float distance = direction.magnitude;
        if (distance <= 0.01f) return;

        direction.Normalize();

        Vector3 movement = Vector3.zero;

        if (distance < minDistance)
        {
            // Retreat directly away from the player
            movement = -direction;
            pathState.path = null;
        }
        else if (distance > maxDistance)
        {
            // Follow our path if one exists
            if (pathState.path != null && pathState.index < pathState.path.Count)
                movement = GetMovementDirectionFromPath();
            else
                movement = direction;
        }
        else
            movement = Vector3.zero;

        if (movement.magnitude > 0.01f)
        {
            Vector2 position = rigidBody.position + (Vector2)(movement.normalized * SPEED * Time.fixedDeltaTime);
            rigidBody.MovePosition(position);
            base.Move();
        }
    }
}
