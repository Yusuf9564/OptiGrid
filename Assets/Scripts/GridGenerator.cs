using UnityEngine;
using System.Collections.Generic;

public class GridGenerator : MonoBehaviour
{
    public GameObject nodePrefab;

    [Header("Grid Ayarları")]
    public int rings = 4;
    public float hexSize = 10.0f;

    [HideInInspector] public float spacing;
    [HideInInspector] public List<Node> allNodes = new List<Node>();
    [HideInInspector] public List<Vector3> hexCenters = new List<Vector3>();

    void Start()
    {
        GenerateHexGrid();
    }

    public void GenerateHexGrid()
    {
        ClearGrid();
        spacing = hexSize;

        // ─────────────────────────────────────────────────────────────
        // FLAT-TOP HONEYCOMB — köşeler node, kenarlar yol
        //
        // Flat-top hex merkezi axial (q,r) → world:
        //   cx = hexSize * 1.5 * q
        //   cz = hexSize * sqrt(3) * (r + q*0.5)
        //
        // Flat-top köşe açıları: 0°, 60°, 120°, 180°, 240°, 300°
        // (Pointy-top'tan farklı olarak 30° offset YOK)
        // ─────────────────────────────────────────────────────────────

        // Pozisyon → Node eşlemesi (çakışan köşeleri birleştirmek için)
        Dictionary<long, Node> cornerMap = new Dictionary<long, Node>();

        for (int q = -rings; q <= rings; q++)
        {
            int rMin = Mathf.Max(-rings, -q - rings);
            int rMax = Mathf.Min(rings, -q + rings);

            for (int r = rMin; r <= rMax; r++)
            {
                // Flat-top hex merkezi
                float cx = hexSize * 1.5f * q;
                float cz = hexSize * Mathf.Sqrt(3f) * (r + q * 0.5f);
                Vector3 hexCenter = new Vector3(cx, 0f, cz);
                hexCenters.Add(hexCenter);

                // 6 köşe — flat-top açıları: 0,60,120,180,240,300
                Node[] corners = new Node[6];
                for (int i = 0; i < 6; i++)
                {
                    float angleRad = (60f * i) * Mathf.Deg2Rad;
                    float nx = cx + hexSize * Mathf.Cos(angleRad);
                    float nz = cz + hexSize * Mathf.Sin(angleRad);

                    long key = PosToKey(nx, nz);
                    if (!cornerMap.ContainsKey(key))
                    {
                        Node node = CreateNode(new Vector3(nx, 0.5f, nz));
                        allNodes.Add(node);
                        cornerMap[key] = node;
                    }
                    corners[i] = cornerMap[key];
                }

                // Kenar komşularını bağla
                for (int i = 0; i < 6; i++)
                {
                    Node a = corners[i];
                    Node b = corners[(i + 1) % 6];
                    if (!a.Neighbors.Contains(b)) a.Neighbors.Add(b);
                    if (!b.Neighbors.Contains(a)) b.Neighbors.Add(a);
                }
            }
        }
    }

    // Float pozisyonu → unique long key
    // 100x hassasiyet yeterli (hexSize=10 → komşular ~10 birim uzakta)
    long PosToKey(float x, float z)
    {
        int ix = Mathf.RoundToInt(x * 100f);
        int iz = Mathf.RoundToInt(z * 100f);
        // int aralığını long'a taşı, çakışmasın
        return ((long)(ix + 1000000)) * 10000000L + (iz + 1000000);
    }

    Node CreateNode(Vector3 pos)
    {
        GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
        go.name = "Node_" + allNodes.Count;
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = true;
        return go.GetComponent<Node>();
    }

    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<Node>() != null)
                DestroyImmediate(child.gameObject);
        }
        allNodes.Clear();
        hexCenters.Clear();
    }
}