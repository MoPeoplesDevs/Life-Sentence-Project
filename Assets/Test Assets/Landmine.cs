using UnityEngine;

public class Landmine : MonoBehaviour
{

	private bool hasExploded = false;
	private SpriteRenderer mineButton;

	[Range(0, 10)][SerializeField] int damage;

	private void Awake()
	{
		mineButton = transform.Find("Button").GetComponent<SpriteRenderer>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (hasExploded) return;
		hasExploded = true;
		mineButton.color = Color.black;

		IDamage dmgComponent = collision.GetComponent<IDamage>();

		if (dmgComponent != null)
		{
			Debug.Log("Landmine triggered by " + collision.name);
			dmgComponent.TakeDamage(this.damage);
		}
	}
}
