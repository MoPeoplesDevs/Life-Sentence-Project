using Unity.Tutorials.Editor;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Range(1, 3200)][SerializeField] float bulletSpeed;
    [Range(5, 50)][SerializeField] int bulletDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
