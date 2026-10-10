using UnityEngine;

public class Horde : Enemy
{
    [SerializeField] float damage = 1;
    [SerializeField] float damageCooldown = 1f;

    private float lastDamageTime;

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
                damageCooldown *= 0.9f;
                break;
            case Difficulty.Expert:
                damage *= 1.5f;
                damageCooldown *= 0.75f;
                break;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.tag != "Player") return;
        if (Time.time < lastDamageTime + damageCooldown) return;

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
    }
}