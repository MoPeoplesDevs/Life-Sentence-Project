using UnityEngine;

public class SniperMovement : MonoBehaviour
{

    [SerializeField] Transform player;
    [SerializeField] float speed = 2.5f;
    [SerializeField] float preferredDistance = 8f;
    [SerializeField] float minDistance = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        Vector2 direction = (player.position - transform.position).normalized;

        if (distanceToPlayer > preferredDistance)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
        else if (distanceToPlayer < minDistance)
        {
            transform.position -= (Vector3)(direction * speed * Time.deltaTime);
        }
    }
}
