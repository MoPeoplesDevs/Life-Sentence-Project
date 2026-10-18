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

    [Header("Guard Phase Shooting")]
    [SerializeField] EnemyProjectile guardPhaseProjectile;
    [SerializeField] Transform firePoint;
    [SerializeField] LineRenderer aimWarning;
    [SerializeField] float shotInterval = 2f;
    [SerializeField] float shotWindup = 0.6f;
    [SerializeField] float projectileSpeed = 7f;
    [SerializeField] float projectileDamage = 10f;

    float shotTimer;
    bool preparingShot;
    Vector2 shotDirection;

    WardenState currentState = WardenState.CallGuards;

    int guardsAlive;
    float stateTimer;
    bool phaseTwo;
    List<GameObject> summonedGuards = new List<GameObject>();
    bool encounterEnded;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bossHealth = GetComponent<BossHealth>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        ResetGuardShooting();

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        if (bossHealth != null)
        {
            bossHealth.OnPhaseTwo += EnterPhaseTwo;
            bossHealth.OnBossDeath += EndEncounter;
        }


    }

    void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnPhaseTwo -= EnterPhaseTwo;
            bossHealth.OnBossDeath -= EndEncounter;
        }

        EndEncounter();
    }

    // Update is called once per frame
    void Update()
    {
        if (encounterEnded ||
        GameManager.Instance == null ||
        GameManager.Instance.IsGameOver ||
        player == null)

        {
            HideAimWarning();
            return;
        }

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
        summonedGuards.RemoveAll(guard => guard == null);
        guardsAlive = summonedGuards.Count;

        List<Transform> validSpawnPoints = new List<Transform>();

        if (guardSpawnPoints != null)
        {
            foreach (Transform point in guardSpawnPoints)
            {
                if (point != null)
                    validSpawnPoints.Add(point);
            }
        }

        if (guardPrefab == null || validSpawnPoints.Count == 0)
        {
            Debug.LogWarning(
                "Warden cannot summon guards: missing prefab or spawn points.",
                this);

            stateTimer = chaseDuration;
            pathRefreshTimer = 0f;
            currentState = WardenState.BatonChase;
            return;
        }

        int guardCount = phaseTwo ? guardsToSpawn + 2 : guardsToSpawn;

        for (int i = 0; i < guardCount; i++)
        {
            Transform spawnPoint =
                validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];

            GameObject guard = Instantiate(
                guardPrefab,
                spawnPoint.position,
                Quaternion.identity);

            WardenGuardTracker tracker =
                guard.GetComponent<WardenGuardTracker>();

            if (tracker == null)
                tracker = guard.AddComponent<WardenGuardTracker>();

            tracker.SetWarden(this);

            summonedGuards.Add(guard);
            guardsAlive++;
        }

        currentState = WardenState.WaitForGuards;
        ResetGuardShooting();
    }

    public void GuardDefeated()
    {
        if (encounterEnded) return;

        guardsAlive = Mathf.Max(0, guardsAlive - 1);
    }

    void WaitForGuards()
    {
        if (guardsAlive <= 0)
        {
            ResetGuardShooting();

            stateTimer = chaseDuration;
            pathRefreshTimer = 0f;
            currentState = WardenState.BatonChase;
            return;
        }

        GuardPhaseShooting();
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

         void EndEncounter()
         {
            if (encounterEnded) return;

            encounterEnded = true;

        ResetGuardShooting();

        foreach (GameObject guard in summonedGuards)
            {
                if (guard != null)
                    Destroy(guard);
            }

            summonedGuards.Clear();
            guardsAlive = 0;
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

    void GuardPhaseShooting()
    {
        if (player == null ||
            firePoint == null ||
            guardPhaseProjectile == null)
        {
            return;
        }

        shotTimer -= Time.deltaTime;

        if (shotTimer > 0f) return;

        if (!preparingShot)
        {
            // Lock the aim so the player can dodge during windup.
            shotDirection =
                ((Vector2)player.position -
                 (Vector2)firePoint.position).normalized;

            preparingShot = true;
            shotTimer = Mathf.Max(0.1f, shotWindup);

            if (aimWarning != null)
            {
                aimWarning.useWorldSpace = true;
                aimWarning.positionCount = 2;

                aimWarning.SetPosition(0, firePoint.position);

                Vector3 end = firePoint.position +
                    (Vector3)(shotDirection * 10f);

                aimWarning.SetPosition(1, end);
                aimWarning.enabled = true;
            }

            return;
        }

        HideAimWarning();

        float angle =
            Mathf.Atan2(shotDirection.y, shotDirection.x)
            * Mathf.Rad2Deg - 90f;

        EnemyProjectile projectile = Instantiate(
            guardPhaseProjectile,
            firePoint.position,
            Quaternion.Euler(0f, 0f, angle));

        projectile.Initialize(
            shotDirection,
            projectileSpeed,
            projectileDamage);

        preparingShot = false;
        shotTimer = Mathf.Max(0.1f, shotInterval - shotWindup);
    }

    void ResetGuardShooting()
    {
        preparingShot = false;
        shotTimer = Mathf.Max(0.1f, shotInterval);
        HideAimWarning();
    }

    void HideAimWarning()
    {
        if (aimWarning != null)
            aimWarning.enabled = false;
    }

    void OnDisable()
    {
        HideAimWarning();
    }
}
