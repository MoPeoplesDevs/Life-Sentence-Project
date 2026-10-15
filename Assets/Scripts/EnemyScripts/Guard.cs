
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Guard : Enemy
{
    [SerializeField] float minDistance = 3f;
    [SerializeField] float maxDistance = 6f;
    [SerializeField] float repathInterval = 0.5f;

    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] float projectileSpeed = 9f;
    [SerializeField] float projectileDamage = 4f;

    [SerializeField] int magSize = 12;
    [SerializeField] int reloadTime = 3;

    private float lastAttack;
    private float nextRepath;
    private int ammoRemaining;
    private bool isReloading = false;
    
    //
    private string variant;
    private EnemyAnimator animator;

    void Start()
    {
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

        ammoRemaining = magSize;

        //
        variant = $"Tone{UnityEngine.Random.Range(1, 4)}";
        animator = gameObject.AddComponent<EnemyAnimator>();
        animator.Initialize(variant, this, "Guard");
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
  
    private void Shoot(Vector3 plrDirection)
    {
        if (Time.time - lastAttack < fireRate) return;
        lastAttack = Time.time;

        EnemyProjectile projectile = Instantiate(projectilePrefab, transform.position + (plrDirection * 0.7f), Quaternion.identity);
        projectile.Initialize(plrDirection, projectileSpeed, projectileDamage);

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
