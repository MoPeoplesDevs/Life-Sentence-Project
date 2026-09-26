using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Range(1, 3200)][SerializeField] float bulletSpeed;
    [Range(5, 50)][SerializeField] int bulletDamage;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] LayerMask wallLayer;

    CircleCollider2D bulletCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bulletCollider = GetComponent<CircleCollider2D>();

        bulletCollider.excludeLayers = ~(enemyLayer | wallLayer);
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.up * bulletSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamage damage = other.GetComponent<IDamage>();
        if (damage != null)
        {
            damage.TakeDamage(bulletDamage);
        }

        Destroy(gameObject);
    }
}
