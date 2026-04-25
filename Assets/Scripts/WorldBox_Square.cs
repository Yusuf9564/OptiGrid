using UnityEngine;
using System.Collections.Generic;

public class WorldBox_Square : MonoBehaviour
{
    private GridGenerator_Square gridGen;

    [Header("Kutu Ayarları")]
    public Material boxMaterial;
    public float boxPadding = 10f;
    public float wallHeight = 50f;

    private GameObject worldContainer;

    void Start()
    {
        gridGen = GetComponent<GridGenerator_Square>();
        Invoke("BuildBox", 1.5f);
    }

    public void BuildBox()
    {
        if (gridGen == null || gridGen.allNodes == null || gridGen.allNodes.Count == 0) return;
        if (worldContainer != null) Destroy(worldContainer);

        worldContainer = new GameObject("SQUARE_WORLD_BOX");
        worldContainer.layer = 2; // Ignore Raycast

        // Şehrin sınırlarını hesapla
        Bounds bounds = new Bounds(gridGen.allNodes[0].transform.position, Vector3.zero);
        foreach (Node n in gridGen.allNodes) bounds.Encapsulate(n.transform.position);

        float sizeX = bounds.size.x + boxPadding;
        float sizeZ = bounds.size.z + boxPadding;
        Vector3 center = bounds.center;

        // 1. ZEMİN (Taban)
        CreateWall("Floor", new Vector3(center.x, -0.1f, center.z), new Vector3(sizeX * 2, 0.1f, sizeZ * 2), worldContainer.transform);

        // 2. YAN DUVARLAR
        // Arka
        CreateWall("Wall_Back", new Vector3(center.x, wallHeight / 2, center.z + sizeZ), new Vector3(sizeX * 2, wallHeight, 0.2f), worldContainer.transform);
        // Ön
        CreateWall("Wall_Front", new Vector3(center.x, wallHeight / 2, center.z - sizeZ), new Vector3(sizeX * 2, wallHeight, 0.2f), worldContainer.transform);
        // Sağ
        CreateWall("Wall_Right", new Vector3(center.x + sizeX, wallHeight / 2, center.z), new Vector3(0.2f, wallHeight, sizeZ * 2), worldContainer.transform);
        // Sol
        CreateWall("Wall_Left", new Vector3(center.x - sizeX, wallHeight / 2, center.z), new Vector3(0.2f, wallHeight, sizeZ * 2), worldContainer.transform);
    }

    void CreateWall(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.transform.SetParent(parent);
        wall.layer = 2; // Seçimi engellemesin

        if (boxMaterial != null) wall.GetComponent<Renderer>().material = boxMaterial;
        Destroy(wall.GetComponent<BoxCollider>()); // A-B seçimini engellememesi için şart
    }
}