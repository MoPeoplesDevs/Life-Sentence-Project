using UnityEngine;

public class Horde : Enemy
{
    [SerializeField] float damage = 1;
    [SerializeField] bool isHeavy = false;

    private float lastDamageTime;

    //
    private string variant;
    private EnemyAnimator animator;

    void Start()
    {
        // Difficulty
        Difficulty difficulty = GameManager.Instance.gameDifficulty;
        switch (difficulty)
        {
            case Difficulty.Easy:
                damage *= 0.8f;
                break;
            case Difficulty.Medium:
                damage *= 1f;
                break;
            case Difficulty.Hard:
                damage *= 1.2f;
                fireRate *= 0.9f;
                break;
            case Difficulty.Expert:
                damage *= 1.5f;
                fireRate *= 0.75f;
                break;
        }

        //
        variant = $"Tone{UnityEngine.Random.Range(1, 4)}";
        animator = gameObject.AddComponent<EnemyAnimator>();
        animator.Initialize(variant, this, isHeavy ? "Heavy" : "Horde");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.tag != "Player") return;
        if (Time.time < lastDamageTime + fireRate) return;
        
        base.Attack();

        Playercontroller player = collision.gameObject.GetComponent<Playercontroller>();

        if (player != null)
        {
            player.TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }
    
    protected override void Move()
    {
        Vector3 movementDir = new Vector3(0, 0, 0);

        // Can we see the player?
        Vector3 dir = (player.position - transform.position).normalized;
        if (CanSeePlayer(dir))
        {
            movementDir = dir;
            pathState.path = null;
        }
        else
        {
            if (pathState.path == null)
                GetPathToPlayer();
            else
            {
                // Did the player make a substantial movement? Should we recompute our path?
                if (lastKnownLocation != null && (lastKnownLocation - player.position).magnitude > 5)
                    GetPathToPlayer();

                movementDir = GetMovementDirectionFromPath();

                if (movementDir == new Vector3(0, 0, 0)) // Reset the path if we get nothing...
                    pathState.path = null;
            }
        }

        // Set velocity
        rigidBody.linearVelocity = movementDir * SPEED;
        base.Move();
    }
}