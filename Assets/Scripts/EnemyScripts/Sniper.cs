using UnityEngine;

public class Sniper : Enemy
{
    void Update()
    {
        if (GameManager.Instance.IsGameOver || buttonFunctions.Instance.IsPaused) return;

        Vector3 toPlayer = (player.position - transform.position).normalized;
        if (!CanSeePlayer(toPlayer))
            Move();
        else
        {
            Debug.Log("I can see the player!");
        }
    }

    protected override void Move()
    {
        Vector3 movementDir = new Vector3(0, 0, 0);

        if (pathState.path == null)
            Debug.Log("Get path to somewhere that can see the player");
            // GetPath();
        else
        {
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

        // Set velocity
        rigidBody.linearVelocity = movementDir * SPEED;
    }
}