using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] float lifetime = 4f;
    [SerializeField] LayerMask wallLayers;

    float speed;
    int damage;
    Vector2 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 newDirection, float newSpeed, int newDamage)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
        damage = newDamage;
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
            return;
        }

        if (((1 << collision.gameObject.layer) & wallLayers) != 0)
        {
            Destroy(gameObject);
        }
    }
}
