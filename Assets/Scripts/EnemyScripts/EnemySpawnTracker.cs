using UnityEngine;

public class EnemySpawnTracker : MonoBehaviour
{

    EnemySpawner spawner;

    public void SetSpawner(EnemySpawner newSpawner)
    {
        spawner = newSpawner;
    }

    void OnDestroy()
    {
        if(spawner != null)
        {
            spawner.EnemyDestroyed();
        }
    }

}
