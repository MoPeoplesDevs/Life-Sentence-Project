using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    [SerializeField] float chargeSpeed = 7f;
    [SerializeField] float chargeWindupTime = 1f;
    [SerializeField] float chargeDuration = 1f;
    [SerializeField] int chargeDamage = 15;

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

    }

    void ChargeWindup()
    {

    }

    void Charge()
    {

    }

    void Recovery()
    {

    }
}
