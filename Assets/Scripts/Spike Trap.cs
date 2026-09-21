using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpikeTrap : MonoBehaviour
{
	[Range(0, 10)][SerializeField] int damage;

	[SerializeField] float deployDelay = 0.35f;
	[SerializeField] float deployTime = 0.15f;
	[SerializeField] float activeTime = 1.5f;
	[SerializeField] float retractTime = 0.25f;

	[SerializeField] float undeployedScale = 0.2f;
	[SerializeField] float deployedScale = 1f;

	private bool active = false;
	private List<Transform> spikes = new List<Transform>();
	private List<Collider2D> spikeColliders = new List<Collider2D>();
	private HashSet<Collider2D> targetsInside = new HashSet<Collider2D>();

	private void Awake()
	{
		foreach (Transform child in transform)
		{
			if (child.name != "Spike") continue;

			spikes.Add(child);

			Collider2D spikeCollider = child.GetComponent<Collider2D>();
			if (spikeCollider != null)
			{
				spikeCollider.enabled = false;
				spikeColliders.Add(spikeCollider);
			}

			child.localScale = Vector3.one * undeployedScale;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		IDamage dmgComponent = collision.GetComponent<IDamage>();
		if (dmgComponent == null) return;

		targetsInside.Add(collision);

		if (!active)
			StartCoroutine(ActivateTrap());
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		targetsInside.Remove(collision);
	}

	private IEnumerator ActivateTrap()
	{
		active = true;

		yield return new WaitForSeconds(deployDelay);

		yield return ScaleSpikes(undeployedScale, deployedScale, deployTime);

		foreach (Collider2D target in targetsInside)
		{
			if (target == null) continue;

			IDamage dmgComponent = target.GetComponent<IDamage>();
			if (dmgComponent != null)
				dmgComponent.TakeDamage(damage);
		}

		foreach (Collider2D spikeCollider in spikeColliders)
			spikeCollider.enabled = true;

		yield return new WaitForSeconds(activeTime);

		foreach (Collider2D spikeCollider in spikeColliders)
			spikeCollider.enabled = false;

		yield return ScaleSpikes(deployedScale, undeployedScale, retractTime);

		active = false;
	}

	private IEnumerator ScaleSpikes(float startScale, float endScale, float duration)
	{
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;

			float alpha = Mathf.Clamp01(elapsed / duration);
			float size = Mathf.Lerp(startScale, endScale, alpha);

			foreach (Transform spike in spikes)
				spike.localScale = Vector3.one * size;

			yield return null;
		}

		foreach (Transform spike in spikes)
			spike.localScale = Vector3.one * endScale;
	}
}