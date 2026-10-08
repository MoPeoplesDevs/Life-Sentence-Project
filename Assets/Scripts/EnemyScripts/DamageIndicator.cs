using TMPro;
using UnityEngine;
using System.Collections;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.6f;
    [SerializeField] private float moveDistance = 0.75f;

    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.sortingOrder = 100;
    }

    public void Show(float damage)
    {
        text.text = damage.ToString();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.up * moveDistance;

        Color startColor = text.color;
        Color endColor = startColor;
        endColor.a = 0f;

        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / lifetime;

            transform.position = Vector3.Lerp(startPosition, endPosition, alpha);
            text.color = Color.Lerp(startColor, endColor, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }
}