using UnityEngine;

public class WaveEnemyTracker : MonoBehaviour
{

    WaveManager waveManager;

    public void SetWaveManager(WaveManager manager)
    {
        waveManager = manager;
    }

    void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.EnemyDestroyed();
        }
    }

}
