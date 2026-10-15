
using UnityEngine;
using System.Collections.Generic;

public class Sniper : Enemy
{
    [Header("Combat")]
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] float projectileSpeed = 6f;
    [SerializeField] float projectileDamage = 10f;

    [Header("Positioning")]
    [SerializeField] float minSnipingDistance = 7f;
    [SerializeField] float maxSnipingDistance = 13f;
    [SerializeField] float relocationInterval = 12f;
    [SerializeField] float arrivalDistance = 0.5f;
    [SerializeField] int positionAttempts = 20;

    [Header("Avoidance")]
    [SerializeField] float playerAvoidanceRadius = 6f;
    [SerializeField] float preferredSnipingDistance = 10f;
    [SerializeField] float pathSafetyWeight = 3f;
    [SerializeField] float pathLengthWeight = 0.5f;

    private float lastAttack;
    private float nextRelocation;
    private float movementTimeout;
    private bool isRelocating;

    //
    private string variant;
    private EnemyAnimator animator;

    void Start()
    {
        Difficulty difficulty = GameManager.Instance.gameDifficulty;
        switch (difficulty)
        {
            case Difficulty.Easy:
                fireRate *= 0.8f;
                projectileDamage *= 0.8f;
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

        nextRelocation = Time.time;

        //
        animator = gameObject.AddComponent<EnemyAnimator>();
        animator.Initialize(variant, this, "Sniper");
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver || buttonFunctions.IsPaused) return;

        Vector3 dir = (player.position - transform.position).normalized;

        if (isRelocating)
        {
            if (Time.time >= movementTimeout)
                TryRelocate();

            return;
        }

        if (Time.time >= nextRelocation || !CanSeePlayer(dir))
        {
            TryRelocate();
            return;
        }

        Shoot(dir);
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver || buttonFunctions.IsPaused) return;
        if (!isRelocating) return;

        MoveToPosition();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (health <= 0) return;

        TryRelocate();
    }
        
    private void TryRelocate()
    {
        List<Vector3> bestPath = null;
        float bestScore = float.MinValue;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(~ignoreRaycastLayer);
        filter.useTriggers = false;

        for (int i = 0; i < positionAttempts; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minSnipingDistance, maxSnipingDistance);

            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
            Vector3 position = player.position + offset;

            float distanceFromCurrent = (position - transform.position).magnitude;
            if (distanceFromCurrent < 4f) continue;

            List<Vector3> path = Pathfinder.Pathfind(transform.position, position, collider.radius + 0.25f);
            if (path == null || path.Count < 2) continue;

            // Make sure the destination is actually reachable
            if ((path[path.Count - 1] - position).magnitude > 1f) continue;

            // Check whether the position can see the player
            Vector2 direction = (player.position - position).normalized;
            RaycastHit2D[] results = new RaycastHit2D[10];
            int hitCount = Physics2D.Raycast(position, direction, filter, results, maxSnipingDistance + 5f);
            bool canSeePlayer = false;

            for (int j = 0; j < hitCount; j++)
            {
                if (results[j].transform == player)
                {
                    canSeePlayer = true;
                    break;
                }

                if (results[j].transform != transform) break;
            }

            if (!canSeePlayer) continue;

            // Evaluate the entire path for safety
            float pathLength = 0f;
            float closestDistance = float.MaxValue;
            float danger = 0f;
            bool isSafe = true;

            Vector3 previousPoint = transform.position;

            foreach (Vector3 point in path)
            {
                Vector3 segment = point - previousPoint;
                float segmentLength = segment.magnitude;
                pathLength += segmentLength;

                // Find the closest point to the player along this segment
                Vector3 segmentDirection = segmentLength > 0f ? segment / segmentLength : Vector3.zero;
                float projection = Mathf.Clamp(Vector3.Dot(player.position - previousPoint, segmentDirection), 0f, segmentLength);
                Vector3 closestPoint = previousPoint + segmentDirection * projection;

                float distanceToPlayer = (closestPoint - player.position).magnitude;
                closestDistance = Mathf.Min(closestDistance, distanceToPlayer);

                // Reject paths that bring us too close to the player
                if (distanceToPlayer < playerAvoidanceRadius)
                {
                    isSafe = false;
                    break;
                }

                // Penalize paths near the player, even outside the avoidance radius
                danger += segmentLength / Mathf.Max(distanceToPlayer, 0.1f);

                previousPoint = point;
            }

            if (!isSafe) continue;

            // Prefer destinations near our optimal sniping distance
            float snipingDistance = (position - player.position).magnitude;
            float distancePenalty = Mathf.Abs(snipingDistance - preferredSnipingDistance);

            // Reward paths that stay far away from the player
            float score = closestDistance * pathSafetyWeight - pathLength * pathLengthWeight - danger * pathSafetyWeight - distancePenalty;

            if (score > bestScore)
            {
                bestScore = score;
                bestPath = path;
            }
        }

        if (bestPath == null)
        {
            nextRelocation = Time.time + 2f;
            isRelocating = false;
            return;
        }

        pathState.path = bestPath;
        pathState.index = 1;

        isRelocating = true;
        movementTimeout = Time.time + 8f;
    }


    private void MoveToPosition()
    {
        if (pathState.path == null || pathState.index >= pathState.path.Count)
        {
            FinishRelocating();
            return;
        }

        Vector3 target = pathState.path[pathState.index];
        Vector3 difference = target - transform.position;
        difference.z = 0f;

        if (difference.magnitude <= arrivalDistance)
        {
            pathState.index++;

            if (pathState.index >= pathState.path.Count)
                FinishRelocating();

            return;
        }

        Vector2 position = Vector2.MoveTowards(rigidBody.position, target, SPEED * Time.fixedDeltaTime);
        rigidBody.MovePosition(position);
        base.Move();
    }

    private void FinishRelocating()
    {
        isRelocating = false;
        pathState.path = null;
        nextRelocation = Time.time + relocationInterval;
    }

    private void Shoot(Vector3 plrDirection)
    {
        if (Time.time - lastAttack < fireRate) return;
        lastAttack = Time.time;

        EnemyProjectile projectile = Instantiate(
            projectilePrefab, transform.position + (plrDirection * 0.7f), Quaternion.identity
        );

        projectile.Initialize(plrDirection, projectileSpeed, projectileDamage);
        base.Attack();
    }
}
