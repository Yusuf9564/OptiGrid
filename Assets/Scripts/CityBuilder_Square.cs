using UnityEngine;
using System.Collections.Generic;

public class CityBuilder_Square : MonoBehaviour
{
    private GridGenerator_Square gridGen;

    private GameObject[] skyscrapers, midRangeBuildings, residentialHouses;

    [Header("Yerleşim Ayarları")]
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
        skyscrapers = Resources.LoadAll<GameObject>("Buildings/Skyscrapers");
        midRangeBuildings = Resources.LoadAll<GameObject>("Buildings/MidRange");
        residentialHouses = Resources.LoadAll<GameObject>("Buildings/Residential");
    }

    public void BuildAdvancedCity()
    {
        if (gridGen == null || gridGen.allNodes == null) return;
        ClearCity();

        float s = gridGen.spacing;

        foreach (Node n in gridGen.allNodes)
        {
            Vector2Int c = n.GridCoord;

            Node right = gridGen.GetNode(c.x + 1, c.y);
            Node up = gridGen.GetNode(c.x, c.y + 1);
            Node diag = gridGen.GetNode(c.x + 1, c.y + 1);

            if (right == null || up == null || diag == null) continue;

            float distRight = Vector3.Distance(n.transform.position, right.transform.position);
            float distUp = Vector3.Distance(n.transform.position, up.transform.position);
            if (distRight > s * 1.5f || distUp > s * 1.5f) continue;

            Vector3 blockCenter = (n.transform.position + diag.transform.position) / 2f;
            GenerateBlockDetails(blockCenter);
        }
    }

    void GenerateBlockDetails(Vector3 center)
    {
        float dist = center.magnitude;

        // Zone sınırları (genişletilmiş)
        // dist < 15  → CBD çekirdeği      (gökdelen, 2x2 yoğun)
        // dist < 35  → Gökdelen saçağı    (gökdelen, 1x1 seyrek)
        // dist < 60  → Orta boy bölgesi   (midRange, 2x2)
        // dist >= 60 → Dış halka          (residential, 2x2)
        bool isCBD = dist < 15f;
        bool isSkyscraperZone = dist < 35f;
        bool isMidZone = dist < 60f;

        int rows = isCBD ? 2 : (isSkyscraperZone ? 1 : 2);

        float safeZone = gridGen.spacing * streetMargin;
        float step = safeZone / rows;
        float startOffset = (safeZone / 2f) - (step / 2f);

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 offset = new Vector3(x * step - startOffset, 0, z * step - startOffset);
                Vector3 spawnPos = center + offset;

                if (IsOccupied(spawnPos)) continue;

                GameObject prefab = null;
                float h = 1f;
                float w = step * 0.70f;

                if (isSkyscraperZone && skyscrapers.Length > 0)
                {
                    prefab = skyscrapers[Random.Range(0, skyscrapers.Length)];
                    h = Random.Range(2.5f, 5.0f) * Mathf.Clamp(35f / (dist + 1f), 1f, 2.5f);
                }
                else if (isMidZone && midRangeBuildings.Length > 0)
                {
                    prefab = midRangeBuildings[Random.Range(0, midRangeBuildings.Length)];
                    h = Random.Range(1.3f, 2.3f);
                }
                else if (residentialHouses.Length > 0)
                {
                    prefab = residentialHouses[Random.Range(0, residentialHouses.Length)];
                    h = Random.Range(0.8f, 1.3f);
                    w *= 0.8f;
                }

                if (prefab != null)
                    SpawnBuilding(prefab, spawnPos, h, w);
            }
        }
    }

    void SpawnBuilding(GameObject prefab, Vector3 pos, float h, float w)
    {
        GameObject b = Instantiate(prefab, pos, Quaternion.identity, transform);
        b.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);
        b.transform.localScale = new Vector3(w, h, w);
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