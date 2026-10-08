using UnityEngine;

public class WardenGuardTracker : MonoBehaviour
{

    WardenBossController warden;

    public void SetWarden(WardenBossController newWarden)
    {
        warden = newWarden;
    }

    void OnDestroy()
    {
        if (warden != null)
        {
            warden.GuardDefeated();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
