using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float spawnRate = 3f;
    [SerializeField] int maxEnemies = 5;

    float spawnTimer;
    int currentEnemies;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;

        if(spawnTimer >= spawnRate && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        currentEnemies++;

        EnemySpawnTracker tracker = enemy.AddComponent<EnemySpawnTracker>();
        tracker.SetSpawner(this);
    }

    public void EnemyDestroyed()
    {
        currentEnemies--;
    }
}
