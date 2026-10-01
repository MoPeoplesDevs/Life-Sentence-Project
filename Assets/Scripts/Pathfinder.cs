using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public static class Pathfinder
{
	private static LayerMask obstacleLayer;
	private static Tilemap groundTilemap;
	private static Tilemap[] obstacleTilemaps;

	public static void Initialize(Tilemap ground, LayerMask obstaclesLayer, params Tilemap[] obstacles)
	{
		groundTilemap = ground;
		obstacleLayer = obstaclesLayer;
		obstacleTilemaps = obstacles;
	}

	public static List<Vector3> Pathfind(Vector3 from, Vector3 to, float agentRadius = 2.5f)
	{
		Vector3Int start = groundTilemap.WorldToCell(from);
		Vector3Int target = groundTilemap.WorldToCell(to);

		List<Vector3Int> cellPath = FindCellPath(start, target, agentRadius);
		List<Vector3> path = new List<Vector3>();

		foreach (Vector3Int cell in cellPath)
			path.Add(groundTilemap.GetCellCenterWorld(cell));

		if (path.Count > 0)
			path.Add(to);

		return path;
	}

	private static List<Vector3Int> FindCellPath(Vector3Int start, Vector3Int target, float agentRadius)
	{
		List<Node> open = new List<Node>();
		HashSet<Vector3Int> closed = new HashSet<Vector3Int>();

		int startTargetDist = (int)(start - target).magnitude;
		open.Add(new Node(start, null, 0, startTargetDist));

		while (open.Count > 0)
		{
			Node current = open[0];

			for (int i = 1; i < open.Count; i++)
			{
				if (open[i].FCost < current.FCost || open[i].FCost == current.FCost && open[i].HCost < current.HCost)
					current = open[i];
			}

			open.Remove(current);
			closed.Add(current.Position);

			if (current.Position == target)
				return RetracePath(current);

			foreach (Vector3Int neighborPosition in GetNeighbors(current.Position))
			{
				if (closed.Contains(neighborPosition) || !IsWalkable(neighborPosition, agentRadius)) continue;

				int neighborDist = (int)(current.Position - neighborPosition).magnitude;
				int movementCost = current.GCost + neighborDist;

				Node neighbor = open.Find(node => node.Position == neighborPosition);

				if (neighbor == null)
				{
					int targetDist = (int)(neighborPosition - target).magnitude;
					open.Add(new Node(neighborPosition, current, movementCost, targetDist));
				}
				else if (movementCost < neighbor.GCost)
				{
					neighbor.GCost = movementCost;
					neighbor.Parent = current;
				}
			}
		}

		return new List<Vector3Int>();
	}

	private static List<Vector3Int> RetracePath(Node endNode)
	{
		List<Vector3Int> path = new List<Vector3Int>();
		Node current = endNode;

		while (current != null)
		{
			path.Add(current.Position);
			current = current.Parent;
		}

		path.Reverse();

		if (path.Count > 0)
			path.RemoveAt(0);

		return path;
	}

	private static Vector3Int[] GetNeighbors(Vector3Int cell)
	{
		return new Vector3Int[]
		{
			cell + Vector3Int.up,
			cell + Vector3Int.down,
			cell + Vector3Int.left,
			cell + Vector3Int.right
		};
	}

	private static bool IsWalkable(Vector3Int cell, float agentRadius)
	{
		if (!groundTilemap.HasTile(cell)) return false;

		foreach (Tilemap tilemap in obstacleTilemaps)
		{
			if (tilemap != null && tilemap.HasTile(cell))
				return false;
		}

		Vector3 center = groundTilemap.GetCellCenterWorld(cell);

		return Physics2D.OverlapCircle(center, agentRadius, obstacleLayer) == null;
	}

	private class Node
	{
		public Vector3Int Position;
		public Node Parent;

		public int GCost;
		public int HCost;

		public int FCost => GCost + HCost;

		public Node(Vector3Int position, Node parent, int gCost, int hCost)
		{
			Position = position;
			Parent = parent;
			GCost = gCost;
			HCost = hCost;
		}
	}
}