using System.Collections;
using Unity.VisualScripting;
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

        Vector2 center = Vector2.zero;
        int count = 0;

        foreach (COBossController officer in officers)
        {
            if (!IsLiving(officer)) continue;

            center += (Vector2)officer.transform.position;
            count++;
        }

        if (count == 0) return;

        center /= count;
        Vector2 direction = (Vector2)player.position - center;

        foreach (COBossController officer in officers)
        {
            if (IsLiving(officer))
                officer.FaceDirection(direction);
        }
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
}
