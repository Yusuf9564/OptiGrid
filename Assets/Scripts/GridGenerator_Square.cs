using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Kare grid sistemi. clusterCount x clusterCount adet cluster oluşturur.
/// Her cluster nodesPerCluster x nodesPerCluster node içerir.
/// Clusterlar arası boşluk boulevardWidth kadardır (bulvar).
/// Pathfinding_Square ile tam uyumludur.
/// </summary>
public class GridGenerator_Square : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject nodePrefab;

    [Header("Ölçüler")]
    [Tooltip("Node'lar arası mesafe (aynı cluster içinde)")]
    public float spacing = 4.0f;

    [Tooltip("Bir cluster içindeki node sayısı (tek kenar)")]
    public int nodesPerCluster = 8;

    [Tooltip("Kaç x kaç cluster olsun")]
    public int clusterCount = 4;

    [Tooltip("Clusterlar arası bulvar genişliği")]
    public float boulevardWidth = 10.0f;

    // Dışarıdan erişilebilir veri
    public List<Node> allNodes = new List<Node>();
    public Dictionary<Vector2Int, Node> GridDict { get; private set; } = new Dictionary<Vector2Int, Node>();

    // Hesaplanan sabitler (diğer scriptler okuyabilir)
    public float ClusterStep { get; private set; }   // Bir cluster'ın başından bir sonrakinin başına mesafe
    public int TotalSide { get; private set; }        // Toplam node sayısı (tek kenar)

    void Awake()
    {
        GenerateGrid();
    }

    // ─────────────────────────────────────────────
    // ANA METOT
    // ─────────────────────────────────────────────
    public void GenerateGrid()
    {
        ClearGrid();

        TotalSide = clusterCount * nodesPerCluster;
        ClusterStep = (nodesPerCluster - 1) * spacing + boulevardWidth;
        // ClusterStep: son node'dan bulvar boşluğu atlayıp bir sonraki cluster'ın ilk node'una mesafe
        // Örnek: spacing=4, nodesPerCluster=8, boulevardWidth=10
        //   → (8-1)*4 + 10 = 28 + 10 = 38 birim

        GridDict = new Dictionary<Vector2Int, Node>(TotalSide * TotalSide);

        // ── ADIM 1: Node'ları oluştur ve sözlüğe kaydet ──
        for (int cX = 0; cX < clusterCount; cX++)
        {
            for (int cZ = 0; cZ < clusterCount; cZ++)
            {
                float xBase = cX * ClusterStep;
                float zBase = cZ * ClusterStep;

                for (int lX = 0; lX < nodesPerCluster; lX++)
                {
                    for (int lZ = 0; lZ < nodesPerCluster; lZ++)
                    {
                        // Dünya pozisyonu
                        Vector3 pos = new Vector3(
                            xBase + lX * spacing,
                            0f,
                            zBase + lZ * spacing
                        );

                        // Global grid koordinatı (bulvar atlamalarından bağımsız, sıralı)
                        Vector2Int coord = new Vector2Int(
                            cX * nodesPerCluster + lX,
                            cZ * nodesPerCluster + lZ
                        );

                        GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
                        go.name = $"Node_{coord.x}_{coord.y}";

                        Node n = go.GetComponent<Node>();
                        n.GridCoord = coord; // Node'a kendi koordinatını bildir (Node scripti bunu destekliyorsa)

                        GridDict[coord] = n;
                        allNodes.Add(n);
                    }
                }
            }
        }

        // ── ADIM 2: Komşulukları bağla ──
        BuildNeighbors();

        // ── ADIM 3: Şehri ortala ──
        CenterGrid();
    }

    // ─────────────────────────────────────────────
    // KOMŞULUK BAĞLAMA
    // ─────────────────────────────────────────────
    void BuildNeighbors()
    {
        for (int x = 0; x < TotalSide; x++)
        {
            for (int z = 0; z < TotalSide; z++)
            {
                Node current = GridDict[new Vector2Int(x, z)];

                // Sağ (X+1)
                TryAddNeighbor(current, x + 1, z, isDiagonal: false);

                // Üst (Z+1)
                TryAddNeighbor(current, x, z + 1, isDiagonal: false);

                // Çapraz (X+1, Z+1) — sadece aynı cluster içinde
                bool sameClusterX = (x + 1) / nodesPerCluster == x / nodesPerCluster;
                bool sameClusterZ = (z + 1) / nodesPerCluster == z / nodesPerCluster;
                if (sameClusterX && sameClusterZ)
                    TryAddNeighbor(current, x + 1, z + 1, isDiagonal: true);
            }
        }
    }

    void TryAddNeighbor(Node current, int nx, int nz, bool isDiagonal)
    {
        Vector2Int coord = new Vector2Int(nx, nz);
        if (!GridDict.TryGetValue(coord, out Node neighbor)) return;

        if (isDiagonal)
        {
            if (!current.DiagonalNeighbors.Contains(neighbor))
                current.DiagonalNeighbors.Add(neighbor);
        }
        else
        {
            if (!current.Neighbors.Contains(neighbor))
                current.Neighbors.Add(neighbor);
            if (!neighbor.Neighbors.Contains(current))
                neighbor.Neighbors.Add(current);
        }
    }

    // ─────────────────────────────────────────────
    // ORTALAMA
    // ─────────────────────────────────────────────
    void CenterGrid()
    {
        // Son node'un pozisyonu: (TotalSide - 1) node * spacing + (clusterCount - 1) bulvar
        float fullX = (nodesPerCluster - 1) * spacing * clusterCount + boulevardWidth * (clusterCount - 1);
        float fullZ = fullX; // Kare grid, simetrik
        Vector3 offset = new Vector3(fullX / 2f, 0f, fullZ / 2f);

        foreach (Node n in allNodes)
            n.transform.position -= offset;
    }

    // ─────────────────────────────────────────────
    // TEMİZLİK
    // ─────────────────────────────────────────────
    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.GetComponent<Node>() != null)
                DestroyImmediate(child);
        }
        allNodes.Clear();
        GridDict.Clear();
    }

    // ─────────────────────────────────────────────
    // YARDIMCI: Koordinattan Node al
    // ─────────────────────────────────────────────
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