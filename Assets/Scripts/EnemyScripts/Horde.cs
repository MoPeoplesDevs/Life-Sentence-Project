using UnityEngine;

public class Horde : Enemy
{
    [SerializeField] int damage = 1;
    [SerializeField] float damageCooldown = 1f;

    private float lastDamageTime;

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

                // VisualizePath();

                if (pathState.index < pathState.path.Count)
                {
                    Vector3 newTarget = pathState.path[pathState.index];
                    Vector3 difference = (newTarget - transform.position);

                    movementDir = difference.normalized;

                    // Should we increase to the next index?
                    if (difference.magnitude < 1f)
                        pathState.index++;
                }
                else
                    pathState.path = null;
            }
        }

        // Set velocity
        rigidBody.linearVelocity = movementDir * SPEED;
    }
}