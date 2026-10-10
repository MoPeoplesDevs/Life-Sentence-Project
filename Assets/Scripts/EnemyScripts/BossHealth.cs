using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamage
{

    [SerializeField] int maxHP = 100;
    [SerializeField] int phaseTwoThreshold = 50;

    float currentHP;
    bool phaseTwoTriggered = false;

    public event Action OnPhaseTwo;
    public event Action OnBossDeath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
            return;
        }

        if (!phaseTwoTriggered && currentHP <= phaseTwoThreshold)
        {
            phaseTwoTriggered = true;
            OnPhaseTwo?.Invoke();
        }

    }

    void Die()
    {
        OnBossDeath?.Invoke();
        Destroy(gameObject);
    }

}
