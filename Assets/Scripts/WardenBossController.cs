using System.Collections.Generic;
using UnityEngine;

public class WardenBossController : MonoBehaviour
{

    enum WardenState
    {
        CallGuards,
        WaitForGuards,
        BatonChase,
        Recovery

    }

    [Header("References")]
    [SerializeField] BossHealth bossHealth;
    [SerializeField] Transform player;
    [SerializeField] GameObject guardPrefab;
    [SerializeField] Transform[] guardSpawnPoints;

    [Header("Guard Call")]
    [SerializeField] int guardsToSpawn = 3;

    [Header("Baton Chase")]
    [SerializeField] float chaseSpeed = 4f;
    [SerializeField] float chaseDuration = 6f;
    [SerializeField] float batonRange = 1.2f;
    [SerializeField] int batonDamage = 20;

    [Header("Pathfinding")]
    [SerializeField] float pathRefreshRate = 0.4f;
    [SerializeField] float agentRadius = 1f;

    List<Vector3> currentPath = new List<Vector3>();
    int pathIndex;
    float pathRefreshTimer;

    [Header("Timing")]
    [SerializeField] float recoveryTime = 2f;

    WardenState currentState = WardenState.CallGuards;

    int guardsAlive;
    float stateTimer;
    bool phaseTwo;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        bossHealth.OnPhaseTwo += EnterPhaseTwo;
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
            case WardenState.CallGuards:
                CallGuards();
                break;

            case WardenState.WaitForGuards:
                WaitForGuards();
                break;

            case WardenState.BatonChase:
                BatonChase();
                break;

            case WardenState.Recovery:
                Recovery();
                break;
        }
    }

    void CallGuards()
    {
        guardsAlive = 0;

        int guardCount = phaseTwo ? guardsToSpawn + 2 : guardsToSpawn;

        for (int i = 0; i < guardCount; i++)
        {
            Transform spawnPoint = guardSpawnPoints[Random.Range(0, guardSpawnPoints.Length)];

            GameObject guard = Instantiate(
                guardPrefab,
                spawnPoint.position,
                Quaternion.identity);

            WardenGuardTracker tracker = guard.AddComponent<WardenGuardTracker>();

            tracker.SetWarden(this);

            guardsAlive++;

        }

        currentState = WardenState.WaitForGuards;

    }

    public void GuardDefeated()
    {
        guardsAlive--;
    }

    void WaitForGuards()
    {
        if (guardsAlive <= 0)
        {
            stateTimer = chaseDuration;
            pathRefreshTimer = 0f;
            currentState = WardenState.BatonChase;
        }
    }

    void BatonChase()
    {
        stateTimer -= Time.deltaTime;
        pathRefreshTimer -= Time.deltaTime;

        if (pathRefreshTimer < 0f)
        {
            currentPath = Pathfinder.Pathfind(
                transform.position,
                player.position,
                agentRadius);

            pathIndex = 0;
            pathRefreshTimer = pathRefreshRate;
        }

        FollowPath();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= batonRange)
        {
            Playercontroller playerController = player.GetComponent<Playercontroller>();

            if (playerController != null)
            {
                playerController.TakeDamage(batonDamage);
            }

            stateTimer = recoveryTime;
            currentState = WardenState.Recovery;
            return;
        }

        if (stateTimer <= 0f)
        {
            stateTimer = recoveryTime;
            currentState = WardenState.Recovery;
        }

    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        if (pathIndex >= currentPath.Count) return;

        float currentSpeed = chaseSpeed;

        if (phaseTwo)
        {
            currentSpeed *= 1.4f;
        }

        Vector3 targetPoint = currentPath[pathIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint,
            currentSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            pathIndex++;
        }
    }

    void Recovery()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            currentState = WardenState.CallGuards;
        }
    }

    void EnterPhaseTwo()
    {
        phaseTwo = true;
    }
}
