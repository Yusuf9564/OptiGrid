using System.Collections.Generic;
using UnityEngine;

public class Pathfinding_Triangle : MonoBehaviour
{
    private GridGenerator_Triangle gridGen;

    void Awake()
    {
        gridGen = GetComponent<GridGenerator_Triangle>();
    }

    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        if (startNode == null || targetNode == null) return null;

        // Dinamik referans kontrolü (Güvenlik için)
        if (gridGen == null) gridGen = GetComponent<GridGenerator_Triangle>();
        if (gridGen == null || gridGen.allNodes == null) return null;

        // Her yeni aramada Node verilerini tamamen sıfırla
        foreach (Node n in gridGen.allNodes)
        {
            if (n == null) continue;
            n.gCost = float.MaxValue;
            n.hCost = 0f;
            n.parent = null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.gCost = 0f;
        startNode.hCost = Heuristic(startNode, targetNode);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = GetLowestFCost(openSet);
            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Node neighbor in current.Neighbors)
            {
                if (neighbor == null || closedSet.Contains(neighbor)) continue;

                float dist = Vector3.Distance(current.transform.position, neighbor.transform.position);
                float newGCost = current.gCost + dist;

                if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = Heuristic(neighbor, targetNode);
                    neighbor.parent = current;
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    float Heuristic(Node a, Node b)
    {
        // Üçgen ızgara için Manhattan/Euclidean hibrit yaklaşımı
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    Node GetLowestFCost(List<Node> list)
    {
        Node best = list[0];
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].fCost < best.fCost || (list[i].fCost == best.fCost && list[i].hCost < best.hCost))
                best = list[i];
        }
        return best;
    }

    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node curr = end;
        while (curr != null)
        {
            path.Add(curr);
            if (curr == start) break;
            curr = curr.parent;
        }
        path.Reverse();
        return path;
    }
}