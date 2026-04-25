using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* tabanlı pathfinding. GridGenerator_Square ile tam uyumludur.
/// Bulvar geçişlerini (cluster sınırları) doğal olarak destekler.
/// </summary>
public class Pathfinding_Square : MonoBehaviour
{
    private GridGenerator_Square gridGen;

    void Awake()
    {
        gridGen = GetComponent<GridGenerator_Square>();
    }

    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        if (startNode == null || targetNode == null) return null;

        // Referans kontrolü
        if (gridGen == null) gridGen = GetComponent<GridGenerator_Square>();

        // Çok kritik: A'dan B'ye gittikten sonra B'den C'ye giderken 
        // önceki parent ve gCost verilerini temizlemezsek yol sapıtır.
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
                if (closedSet.Contains(neighbor)) continue;

                float dist = Vector3.Distance(current.transform.position, neighbor.transform.position);
                float newGCost = current.gCost + dist;

                if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = Heuristic(neighbor, targetNode);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("[Pathfinding] Yol bulunamadı!");
        return null;
    }

    float Heuristic(Node a, Node b)
    {
        Vector3 d = new Vector3(
            Mathf.Abs(a.transform.position.x - b.transform.position.x),
            0f,
            Mathf.Abs(a.transform.position.z - b.transform.position.z)
        );
        return (d.x + d.z) + (Mathf.Sqrt(2f) - 2f) * Mathf.Min(d.x, d.z);
    }

    Node GetLowestFCost(List<Node> list)
    {
        Node best = list[0];
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].fCost < best.fCost ||
               (list[i].fCost == best.fCost && list[i].hCost < best.hCost))
                best = list[i];
        }
        return best;
    }

    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node curr = end;
        while (curr != null) { path.Add(curr); curr = curr.parent; }
        path.Reverse();
        return path;
    }
}