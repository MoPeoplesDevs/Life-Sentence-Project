using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{

    [SerializeField] float speed = 6f;
    [SerializeField] int damage = 2;
    [SerializeField] float lifetime = 4f;

    Vector2 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }
    // Update is called once per frame
    void Update()
    {

        if (GameManager.Instance.IsGameOver) return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Playercontroller player = collision.GetComponent<Playercontroller>();

        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
