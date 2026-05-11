using UnityEngine;
using System.Collections.Generic;

public class CityBuilder_Square : MonoBehaviour
{
    private GridGenerator_Square gridGen;
    private GameObject[] wasteAssets;

    [Header("Atık Dağılım Ayarları")]
    [Range(0f, 1f)] public float spawnChance = 0.7f;
    public float yOffset = 0.05f;
    public float minBuildingSpacing = 1.6f;
    public float streetMargin = 0.60f;

    private List<Vector3> occupiedPositions = new List<Vector3>();

    void Start()
    {
        gridGen = GetComponent<GridGenerator_Square>();
        LoadAssets();
        Invoke("BuildAdvancedCity", 0.6f);
    }

    void LoadAssets()
    {
        wasteAssets = Resources.LoadAll<GameObject>("Buildings/Residential");
    }

    public void BuildAdvancedCity()
    {
        if (gridGen == null || gridGen.allNodes == null) return;
        ClearCity();

        float s = gridGen.spacing;
        int blockCount = 0; // Blok sayacı

        foreach (Node n in gridGen.allNodes)
        {
            Vector2Int c = n.GridCoord;
            Node diag = gridGen.GetNode(c.x + 1, c.y + 1);
            if (diag == null) continue;

            Vector3 blockCenter = (n.transform.position + diag.transform.position) / 2f;

            // --- REZERVASYON KONTROLÜ ---
            if (blockCount == 0)
            {
                Debug.Log("Square Index 0: Bank alanı için boş bırakıldı.");
                blockCount++;
                continue;
            }

            GenerateWasteBlock(blockCenter);
            blockCount++;
        }
    }

    void GenerateWasteBlock(Vector3 center)
    {
        int rows = 2; // Kare blok içi 2x2 düzen
        float safeZone = gridGen.spacing * streetMargin;
        float step = safeZone / rows;
        float startOffset = (safeZone / 2f) - (step / 2f);

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                if (Random.value > spawnChance) continue;

                Vector3 offset = new Vector3(x * step - startOffset, 0, z * step - startOffset);
                Vector3 spawnPos = center + offset;

                if (IsOccupied(spawnPos)) continue;

                if (wasteAssets.Length > 0)
                {
                    GameObject prefab = wasteAssets[Random.Range(0, wasteAssets.Length)];
                    float scale = step * 0.70f;
                    SpawnBuilding(prefab, spawnPos, scale);
                }
            }
        }
    }

    void SpawnBuilding(GameObject prefab, Vector3 pos, float scale)
    {
        GameObject b = Instantiate(prefab, pos, Quaternion.identity, transform);
        b.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);

        // --- OTOMATİK SIĞDIRMA (Auto-Fit) ---
        // Kare bloklarda minBuildingSpacing bizim için bir sınır.
        // Nesnenin genişliği bu sınırdan büyükse küçültüyoruz.
        float finalScale = Mathf.Min(scale, minBuildingSpacing * 0.9f);

        b.transform.localScale = new Vector3(finalScale, finalScale, finalScale);

        // --- GÖMÜLME FIX ---
        Vector3 cPos = b.transform.position;
        cPos.y += (finalScale * 0.6f) + yOffset;
        b.transform.position = cPos;

        occupiedPositions.Add(pos);
    }

    bool IsOccupied(Vector3 pos)
    {
        foreach (Vector3 p in occupiedPositions)
            if (Vector3.Distance(p, pos) < minBuildingSpacing) return true;
        return false;
    }

    void ClearCity()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            if (transform.GetChild(i).GetComponent<Node>() == null)
                DestroyImmediate(transform.GetChild(i).gameObject);
        occupiedPositions.Clear();
    }
}