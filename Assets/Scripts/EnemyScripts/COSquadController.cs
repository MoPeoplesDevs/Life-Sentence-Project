using System.Collections;
using UnityEngine;

public class COSquadController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] COBossController[] officers;

    [Header("Timing")]
    [SerializeField] float shieldDuration = 3f;
    [SerializeField] float windupDuration = 0.8f;
    [SerializeField] float timeBetweenShots = 0.5f;
    [SerializeField] float recoveryDuration = 2f;

    [Header("Formation Clearance")]
    [SerializeField] LayerMask formationObstacleLayers;
    [SerializeField] float clearancePadding = 0.05f;

    [Header("Marching")]
    [SerializeField] float marchSpeed = 1.2f;
    [SerializeField] float stopDistance = 3f;
    [SerializeField] LayerMask marchBlockingLayers;

    IEnumerator Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (officers == null || officers.Length == 0)
            officers = GetComponentsInChildren<COBossController>();

        if (player == null || LivingCount() == 0)
        {
            Debug.LogError("CO squad needs a player and living officers.", this);
            yield break;
        }

        while (LivingCount() > 0)
        {
            FaceSquadTowardPlayer();
            RaiseShields();

            yield return MarchDuringShieldStance();

            if (LivingCount() == 0) break;

            FaceSquadTowardPlayer();

            yield return WaitForCombatTime(windupDuration);

            foreach (COBossController officer in officers)
            {
                if (!IsLiving(officer)) continue;

                yield return WaitForCombatTime(0f);

                if (IsLiving(officer))
                    officer.FireShotgun();

                yield return WaitForCombatTime(timeBetweenShots);
            }

            yield return WaitForCombatTime(recoveryDuration);
        }

        Destroy(gameObject);
    }

    bool IsLiving(COBossController officer)
    {
        return officer != null && officer.IsAlive;
    }

    int LivingCount()
    {
        int count = 0;

        foreach (COBossController officer in officers)
        {
            if (IsLiving(officer)) count++;
        }

        return count;
    }

    void RaiseShields()
    {
        foreach (COBossController officer in officers)
        {
            if (IsLiving(officer))
                officer.SetShield(true);
        }
    }

    void FaceSquadTowardPlayer()
    {
        if (player == null) return;

        Vector2 direction = player.position - transform.position;

        if (direction.sqrMagnitude < 0.001f) return;

        float targetAngle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        float currentAngle = transform.eulerAngles.z;
        float turnAmount = Mathf.DeltaAngle(currentAngle, targetAngle);

        if (!CanTurnFormation(turnAmount))
            return;

        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    bool CanTurnFormation(float turnAmount)
    {
        // Check intermediate positions, not just the final formation.
        int steps = Mathf.Max(
            1,
            Mathf.CeilToInt(Mathf.Abs(turnAmount) / 5f));

        Vector3 pivot = transform.position;

        foreach (COBossController officer in officers)
        {
            if (!IsLiving(officer)) continue;

            Collider2D[] colliders =
                officer.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D body in colliders)
            {
                if (!body.enabled || !body.gameObject.activeInHierarchy)
                    continue;

                Bounds bounds = body.bounds;

                // A circle enclosing the collider remains safe as it rotates.
                float radius =
                    new Vector2(bounds.extents.x, bounds.extents.y).magnitude
                    + clearancePadding;

                Vector3 offset = bounds.center - pivot;

                for (int i = 0; i <= steps; i++)
                {
                    float angle = turnAmount * i / steps;
                    Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

                    Vector2 checkPosition = pivot + rotation * offset;

                    if (Physics2D.OverlapCircle(
                        checkPosition,
                        radius,
                        formationObstacleLayers) != null)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    IEnumerator WaitForCombatTime(float duration)
    {
        float elapsed = 0f;

        while (LivingCount() > 0)
        {
            bool combatRunning =
                GameManager.Instance != null &&
                !GameManager.Instance.IsGameOver &&
                Time.timeScale > 0f;

            if (combatRunning)
            {
                if (elapsed >= duration)
                    yield break;

                elapsed += Time.deltaTime;
            }

            yield return null;
        }
    }

    IEnumerator MarchDuringShieldStance()
    {
        float elapsed = 0f;

        // Keep the direction locked throughout this stance.
        Vector2 direction = transform.up;

        while (elapsed < shieldDuration && LivingCount() > 0)
        {
            bool combatRunning =
                GameManager.Instance != null &&
                !GameManager.Instance.IsGameOver &&
                Time.timeScale > 0f;

            if (combatRunning)
            {
                elapsed += Time.deltaTime;

                float distance = Mathf.Max(0f, marchSpeed) * Time.deltaTime;
                int steps = Mathf.Max(1, Mathf.CeilToInt(distance / 0.05f));
                Vector2 step = direction * (distance / steps);

                for (int i = 0; i < steps; i++)
                {
                    if (TooCloseToPlayer(step) || !CanMarchStep(step))
                        break;

                    transform.position += (Vector3)step;
                }
            }

            yield return null;
        }
    }

    bool TooCloseToPlayer(Vector2 step)
    {
        if (player == null) return true;

        foreach (COBossController officer in officers)
        {
            if (!IsLiving(officer)) continue;

            Vector2 nextPosition =
                (Vector2)officer.transform.position + step;

            if (Vector2.Distance(nextPosition, player.position) <= stopDistance)
                return true;
        }

        return false;
    }

    bool CanMarchStep(Vector2 step)
    {
        foreach (COBossController officer in officers)
        {
            if (!IsLiving(officer)) continue;

            Collider2D[] colliders =
                officer.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D body in colliders)
            {
                if (!body.enabled || !body.gameObject.activeInHierarchy)
                    continue;

                Bounds bounds = body.bounds;

                // Check a box covering both the current and next positions.
                Vector2 center = (Vector2)bounds.center + step * 0.5f;

                Vector2 size = new Vector2(
                    bounds.size.x + Mathf.Abs(step.x),
                    bounds.size.y + Mathf.Abs(step.y));

                size += Vector2.one * clearancePadding * 2f;

                if (Physics2D.OverlapBox(
                    center,
                    size,
                    0f,
                    marchBlockingLayers) != null)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
