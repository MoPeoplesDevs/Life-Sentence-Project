using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InmateBossController : MonoBehaviour
{

    enum InmateState
    {
        Chase,
        ChargeWindup,
        Charge,
        Recovery
    }

    [Header("References")]
    [SerializeField] BossHealth bossHealth;
    [SerializeField] Transform player;

    [Header("Chase")]
    [SerializeField] float chaseSpeed = 3f;
    [SerializeField] float chargeTriggerDistance = 4f;

    [Header("Charge")]
    [SerializeField] float chargeSpeed = 7f;
    [SerializeField] float chargeWindupTime = 1f;
    [SerializeField] float chargeDuration = 1f;
    [SerializeField] int chargeDamage = 15;
    [SerializeField] float chargeHitRange = 1.2f;
    [SerializeField] LayerMask chargeObstacleLayers;
    [SerializeField] Tilemap[] chargeObstacleTilemaps; 

    [Header("Pathfinding")]
    [SerializeField] float pathRefreshRate = 0.4f;
    [SerializeField] float agentRadius = 1f;

    [Header("Recovery")]
    [SerializeField] float recoveryTime = 1.5f;

    InmateState currentState = InmateState.Chase;

    List<Vector3> currentPath = new List<Vector3>();
    int pathIndex;

    float pathRefreshTimer;
    float stateTimer;

    Vector2 chargeDirection;

    bool phaseTwo;

    bool chargeHitPlayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bossHealth = GetComponent<BossHealth>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        if (bossHealth != null)
        {
            bossHealth.OnPhaseTwo += EnterPhaseTwo;
        }
    }

    void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnPhaseTwo -= EnterPhaseTwo;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        if (player == null) return;

        switch (currentState)
        {
            case InmateState.Chase:
                Chase();
                break;

            case InmateState.ChargeWindup:
                ChargeWindup();
                break;

            case InmateState.Charge:
                Charge();
                break;

            case InmateState.Recovery:
                Recovery();
                break;
        }
    }

    void EnterPhaseTwo()
    {
        phaseTwo = true;
    }

    void Chase()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chargeTriggerDistance)
        {
            stateTimer = chargeWindupTime;
            currentState = InmateState.ChargeWindup;
            return;
        }

        pathRefreshTimer -= Time.deltaTime;

        if(pathRefreshTimer <= 0f)
        {
            currentPath = Pathfinder.Pathfind(
                transform.position,
                player.position,
                agentRadius);

            pathIndex = 0;
            pathRefreshTimer = pathRefreshRate;
        }

        FollowPath();

    }

    void FollowPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count) return;

        Vector3 targetPoint = currentPath[pathIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint,
            chaseSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            pathIndex++;
        }
    }

    void ChargeWindup()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            chargeDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;

            chargeHitPlayer = false;
            stateTimer = chargeDuration;
            currentState = InmateState.Charge;
        }
    }

    void Charge()
    {
        stateTimer -= Time.deltaTime;

        float speed = phaseTwo ? chargeSpeed * 1.4f : chargeSpeed;
        float distance = speed * Time.deltaTime;

        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / 0.1f));
        Vector2 step = chargeDirection * (distance / steps);

        for (int i = 0; i < steps; i++)
        {
            Vector2 nextPosition = (Vector2)transform.position + step;

            if (ChargeBlocked(nextPosition))
            {
                BeginRecovery();
                return;
            }

            transform.position = new Vector3(
                nextPosition.x,
                nextPosition.y,
                transform.position.z);

            if (!chargeHitPlayer &&
                Vector2.Distance(nextPosition, player.position) <= chargeHitRange)
            {
                Playercontroller playerController =
                    player.GetComponent<Playercontroller>();

                if (playerController != null)
                {
                    playerController.TakeDamage(chargeDamage);
                    chargeHitPlayer = true;
                }

                BeginRecovery();
                return;
            }
        }

        if (stateTimer <= 0f)
        {
            BeginRecovery();
        }
    }

    bool ChargeBlocked(Vector2 position)
    {
        if (Physics2D.OverlapCircle(
            position, agentRadius, chargeObstacleLayers) != null)
        {
            return true;
        }

        if (chargeObstacleTilemaps != null)
        {
            foreach (Tilemap tilemap in chargeObstacleTilemaps)
            {
                if (tilemap != null &&
                    tilemap.HasTile(tilemap.WorldToCell(
                        new Vector3(position.x, position.y, transform.position.z))))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void BeginRecovery()
    {
        stateTimer = recoveryTime;
        currentState = InmateState.Recovery;
    }

    void Recovery()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            pathRefreshTimer = 0f;
            currentState = InmateState.Chase;
        }
    }
}
