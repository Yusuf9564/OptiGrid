using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> DiagonalNeighbors = new List<Node>();
    public List<Node> Neighbors = new List<Node>();
    public float gCost;
    public float hCost;
    public Node parent;
    public Vector2Int GridCoord { get; set; }

    public float fCost => gCost + hCost;

    // Bağlantıları Scene ekranında mavi çizgilerle görmek için
    void OnDrawGizmos()
    {
        if (Neighbors == null) return;
        Gizmos.color = Color.blue;
        foreach (Node neighbor in Neighbors)
        {
            if (neighbor != null)
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}