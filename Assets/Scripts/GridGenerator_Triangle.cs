using UnityEngine;
using System.Collections.Generic;

public class GridGenerator_Triangle : MonoBehaviour 
{
    [Header("Prefab")]
    public GameObject nodePrefab;

    [Header("Ölçüler")]
    public float spacing = 4.0f;
    public int nodesPerCluster = 6;
    public int clusterCount = 6;
    public float boulevardWidth = 10.0f;

    public List<Node> allNodes = new List<Node>();
    public Dictionary<Vector2Int, Node> GridDict { get; private set; } = new Dictionary<Vector2Int, Node>();

    public float ClusterStepX { get; private set; }
    public float ClusterStepZ { get; private set; }
    public int TotalWidth { get; private set; }
    public int TotalHeight { get; private set; }

    void Awake() { GenerateGrid(); }

    public void GenerateGrid()
    {
        ClearGrid();

        TotalWidth = clusterCount * nodesPerCluster;
        TotalHeight = clusterCount * nodesPerCluster;

        float rowHeight = spacing * 0.866f;
        ClusterStepX = (nodesPerCluster - 1) * spacing + boulevardWidth;
        ClusterStepZ = (nodesPerCluster - 1) * rowHeight + boulevardWidth;

        GridDict = new Dictionary<Vector2Int, Node>(TotalWidth * TotalHeight);

        // ── ADIM 1: Tüm node'ları oluştur, pozisyon ve koordinat ata ──
        for (int cX = 0; cX < clusterCount; cX++)
        {
            for (int cZ = 0; cZ < clusterCount; cZ++)
            {
                float xBase = cX * ClusterStepX;
                float zBase = cZ * ClusterStepZ;

                for (int lX = 0; lX < nodesPerCluster; lX++)
                {
                    for (int lZ = 0; lZ < nodesPerCluster; lZ++)
                    {
                        // Üçgen kayması: cluster-local lZ bazlı (her cluster kendi içinde tutarlı)
                        float xOffset = (lZ % 2 == 1) ? spacing / 2f : 0f;

                        Vector3 pos = new Vector3(
                            xBase + lX * spacing + xOffset,
                            0f,
                            zBase + lZ * rowHeight
                        );

                        Vector2Int coord = new Vector2Int(
                            cX * nodesPerCluster + lX,
                            cZ * nodesPerCluster + lZ
                        );

                        GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
                        go.name = $"TriNode_{coord.x}_{coord.y}";

                        Node n = go.GetComponent<Node>();
                        n.GridCoord = coord;

                        GridDict[coord] = n;
                        allNodes.Add(n);
                    }
                }
            }
        }

        // ── ADIM 2: KOMŞULUKları KOORDINATLA kur (Square ile aynı mantık) ──
        BuildNeighbors();

        // ── ADIM 3: Ortala ──
        CenterGrid();
    }

    void BuildNeighbors()
    {
        // Square'deki gibi: koordinat bazlı, mesafe kontrolü YOK.
        // Her node için sağ (X+1), üst (Z+1) ve iki çapraz komşuyu koordinattan çek.
        // Bulvar geçişleri dahil tüm komşular otomatik bağlanır.

        for (int x = 0; x < TotalWidth; x++)
        {
            for (int z = 0; z < TotalHeight; z++)
            {
                Node current = GridDict[new Vector2Int(x, z)];

                // Sağ komşu (X+1)
                TryConnect(current, x + 1, z);

                // Üst komşu (Z+1)
                TryConnect(current, x, z + 1);

                // Üçgen çaprazları — tek satırlarda sağa, çift satırlarda sola kayar
                // lZ'ye göre (local): global z'nin cluster içindeki karşılığı
                int lZ = z % nodesPerCluster;
                if (lZ % 2 == 0)
                {
                    // Çift local satır: sol üst komşu
                    TryConnect(current, x - 1, z + 1);
                }
                else
                {
                    // Tek local satır: sağ üst komşu
                    TryConnect(current, x + 1, z + 1);
                }
            }
        }
    }

    void TryConnect(Node a, int nx, int nz)
    {
        if (!GridDict.TryGetValue(new Vector2Int(nx, nz), out Node b)) return;
        if (!a.Neighbors.Contains(b)) a.Neighbors.Add(b);
        if (!b.Neighbors.Contains(a)) b.Neighbors.Add(a);
    }

    void CenterGrid()
    {
        float rowHeight = spacing * 0.866f;
        float fullX = (clusterCount - 1) * ClusterStepX + (nodesPerCluster - 1) * spacing;
        float fullZ = (clusterCount - 1) * ClusterStepZ + (nodesPerCluster - 1) * rowHeight;
        Vector3 offset = new Vector3(fullX / 2f, 0f, fullZ / 2f);
        foreach (Node n in allNodes)
            n.transform.position -= offset;
    }

    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.GetComponent<Node>() != null)
                DestroyImmediate(child);
        }
        allNodes.Clear();
        if (GridDict != null) GridDict.Clear();
        else GridDict = new Dictionary<Vector2Int, Node>();
    }

    public Node GetNode(int x, int z)
    {
        GridDict.TryGetValue(new Vector2Int(x, z), out Node n);
        return n;
    }

    public Node GetNode(Vector2Int coord)
    {
        GridDict.TryGetValue(coord, out Node n);
        return n;
    }
}