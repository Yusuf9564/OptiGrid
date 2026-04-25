using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        if (startNode == null || targetNode == null) return null;
        if (startNode == targetNode) return new List<Node> { startNode };

        Node[] allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        foreach (Node n in allNodes)
        {
            n.gCost = float.MaxValue;
            n.hCost = 0f;
            n.parent = null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.gCost = 0f;
        startNode.hCost = Vector3.Distance(
            startNode.transform.position,
            targetNode.transform.position);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // En düşük fCost'lu node'u bul
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost ||
                   (openSet[i].fCost == current.fCost &&
                    openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            // Neighbors kopyasını al — iterasyon sırasında değişme riskine karşı
            List<Node> neighbors = new List<Node>(current.Neighbors);
            foreach (Node neighbor in neighbors)
            {
                if (neighbor == null) continue;
                if (closedSet.Contains(neighbor)) continue;

                float moveCost = current.gCost + Vector3.Distance(
                    current.transform.position,
                    neighbor.transform.position);

                if (moveCost < neighbor.gCost)
                {
                    neighbor.gCost = moveCost;
                    neighbor.hCost = Vector3.Distance(
                        neighbor.transform.position,
                        targetNode.transform.position);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // Rota bulunamadı
        return null;
    }

    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node curr = end;
        int guard = 100000;

        while (curr != null && curr != start && guard-- > 0)
        {
            path.Add(curr);
            curr = curr.parent;
        }

        if (curr == start)
            path.Add(start);
        else
            return null; // parent zinciri kopuksa null dön

        path.Reverse();
        return path;
    }
}