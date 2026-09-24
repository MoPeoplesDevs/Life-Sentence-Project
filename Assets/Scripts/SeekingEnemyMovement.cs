using UnityEngine;

public class SeekingEnemyMovement : MonoBehaviour
{

    [SerializeField] Transform player;
    [SerializeField] float stoppingDistance = 4f;
    [SerializeField] float speed = 2.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.Instance.IsGameOver) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > stoppingDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }
}