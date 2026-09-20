using UnityEngine;

public class Landmine : MonoBehaviour
{
	private float nextFlip = 0f;
	private bool hasExploded = false;
	private SpriteRenderer mineButton;

	[Range(0, 10)][SerializeField] int damage;

	private void Awake()
	{
		mineButton = transform.Find("Button").GetComponent<SpriteRenderer>();
		nextFlip = Time.time + 1f;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (hasExploded) return;
		hasExploded = true;
		mineButton.color = Color.black;

		IDamage dmgComponent = collision.GetComponent<IDamage>();

		if (dmgComponent != null)
			dmgComponent.TakeDamage(this.damage);
	}

	private void Update()
	{
		if (hasExploded) return;
		if (Time.time >= nextFlip)
		{
			mineButton.color = mineButton.color == Color.red ? Color.white : Color.red;
			nextFlip = Time.time + 1f;
		}
	}
}
