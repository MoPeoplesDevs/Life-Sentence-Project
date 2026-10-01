using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Range(1, 10)][SerializeField] float SPEED;

    private Transform player;
    private Rigidbody2D rigidBody;
    private CircleCollider2D collider;

    private Memory enemyMemory = new Memory();
    private PathState pathState = new PathState();

    private LineRenderer pathRenderer;
    
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

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
        if (pathState.path == null)
            GetPath();
        else
        {
            // Did the player make a substantial movement? Should we recompute our path?
            if (enemyMemory.lastKnownLocation != null && (enemyMemory.lastKnownLocation - player.position).magnitude > 5)
                GetPath();

            // VisualizePath();

            if (pathState.index < pathState.path.Count)
            {
                Vector3 newTarget = pathState.path[pathState.index];
                Vector3 difference = (newTarget - transform.position);

                rigidBody.linearVelocity = difference.normalized * SPEED;
                
                // Should we increase to the next index?
                if (difference.magnitude < 0.1f)
                    pathState.index++;
            }
            else
            {
                Debug.Log("Out of path!");
                pathState.path = null;
            }
        }
    }

    //
    private void GetPath()
    {
        enemyMemory.lastKnownLocation = player.position;
        pathState.index = 1;
        pathState.path = Pathfinder.Pathfind(transform.position, player.position, collider.radius + 0.25f);
    }

    /*
    private void VisualizePath()
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
    */

    //
    private class PathState
    {
        public int index;
        public List<Vector3> path;

        public PathState()
        {
            index = 1;
        }
    }

    private class Memory
    {
        public Vector3 lastKnownLocation;
    }
}
