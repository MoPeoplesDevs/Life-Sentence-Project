using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, IDamage
{
    [SerializeField] protected int MAX_HEALTH;
    [Range(1, 10)][SerializeField] public float SPEED;
	[SerializeField] public LayerMask ignoreRaycastLayer;

    protected int health;
    protected Image healthBar;

    protected Transform player;
    protected Rigidbody2D rigidBody;
    protected CircleCollider2D collider;

    protected PathState pathState = new PathState();
    // protected LineRenderer pathRenderer;

    public Vector3 lastKnownLocation;
    private Coroutine healthBarTween;

    protected buttonFunctions buttonFunctions;
    
    void Awake()
    {
        buttonFunctions = FindFirstObjectByType<buttonFunctions>();

        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        health = MAX_HEALTH;
        healthBar = this.gameObject.transform.Find("Health").transform.Find("Bar").GetComponent<Image>();

        // pathRenderer = gameObject.AddComponent<LineRenderer>();
        // pathRenderer.startWidth = 0.08f;
        // pathRenderer.endWidth = 0.08f;
        // pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // pathRenderer.startColor = Color.green;
        // pathRenderer.endColor = Color.green;
        // pathRenderer.sortingOrder = 100;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver || buttonFunctions.IsPaused) return;
        Move();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        float targetAlpha = Mathf.Clamp01((float) health / MAX_HEALTH);
        
        if (healthBarTween != null)
            StopCoroutine(healthBarTween);

        healthBarTween = StartCoroutine(TweenHealthBar(targetAlpha, 0.25f));

        if (health <= 0)
            Destroy(gameObject);
    }

    private IEnumerator TweenHealthBar(float target, float tweenTime)
    {
        float start = healthBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < tweenTime)
        {
            elapsed += Time.deltaTime;
            healthBar.fillAmount = Mathf.Lerp(start, target, elapsed / tweenTime);
            yield return null;
        }

        healthBar.fillAmount = target;
        healthBarTween = null;
    }

    //
    protected virtual void Move()
    {
        Debug.Log("Base move");
    }

    protected void GetPathToPlayer()
    {
        lastKnownLocation = player.position;

        pathState.index = 1;
        pathState.path = Pathfinder.Pathfind(transform.position, player.position, collider.radius + 0.25f);
    }

    protected bool CanSeePlayer(Vector3 dir)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(~ignoreRaycastLayer);
        filter.useTriggers = false;

        Vector2 origin = transform.position;
        Vector2 direction = new Vector2(dir.x, dir.y);

        RaycastHit2D[] results = new RaycastHit2D[10];
        int hitCount = Physics2D.Raycast(origin, direction, filter, results, 15f);

        if (hitCount > 0)
        {
            RaycastHit2D hit = results[0];
            if (hit.transform.gameObject == player.gameObject)
                return true;
        } 
        return false;
    }

    /*
    protected void VisualizePath()
    {
        if (pathState.path == null || pathState.path.Count == 0)
        {
            pathRenderer.positionCount = 0;
            return;
        }

        int remainingPoints = pathState.path.Count - pathState.index;

        pathRenderer.positionCount = remainingPoints + 1;
        pathRenderer.SetPosition(0, transform.position);

        for (int i = 0; i < remainingPoints; i++)
        {
            Vector3 position = pathState.path[pathState.index + i];
            position.z = 0;

            pathRenderer.SetPosition(i + 1, position);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"Path Point {pathState.index + i}";
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * 0.15f;

            Collider collider = sphere.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = i == 0 ? Color.red : Color.yellow;

            Object.Destroy(sphere, 0.05f);
        }
    }
    /**/

    //
    protected class PathState
    {
        public int index;
        public List<Vector3> path;

        public PathState()
        {
            index = 1;
        }
    }
}
