using UnityEngine;

public class HordeEnemyMovement : MonoBehaviour
{

    [SerializeField] float speed = 3f;
    [SerializeField] Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.Instance.IsGameOver) return;

        Vector2 direction = (player.position - transform.position).normalized;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}
