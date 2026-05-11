using UnityEngine;
using System.Collections.Generic;

public class WorldBox_Triangle : MonoBehaviour
{
    private GridGenerator_Triangle gridGen; // Claude'un jeneratörü

    [Header("Kutu Ayarları")]
    public Material boxMaterial;
    public float boxPadding = 8f;   // Üçgenlerin köşeleri için pay
    public float wallHeight = 45f;  // Duvar yüksekliği

    private GameObject worldContainer;

    void Start()
    {
        gridGen = GetComponent<GridGenerator_Triangle>();
        // Üçgenler ve yollar yerleşince kutuyu inşa et
        Invoke("BuildBox", 1.5f);
    }

    [ContextMenu("Rebuild Triangle Box")]
    public void BuildBox()
    {
        if (gridGen == null || gridGen.allNodes == null || gridGen.allNodes.Count == 0) return;
        if (worldContainer != null) Destroy(worldContainer);

        worldContainer = new GameObject("TRIANGLE_WORLD_BOX");
        worldContainer.layer = 2; // Ignore Raycast (Tıklamayı engellemez)
        // 1. ADIM: Üçgenlerin sınırlarını (Bounds) hesapla
        Bounds bounds = new Bounds(gridGen.allNodes[0].transform.position, Vector3.zero);
        foreach (Node n in gridGen.allNodes)
        {
            if (n != null) bounds.Encapsulate(n.transform.position);
        }

        float sizeX = bounds.size.x + boxPadding;
        float sizeZ = bounds.size.z + boxPadding;
        Vector3 center = bounds.center;

        // 2. ADIM: ZEMİN (Taban)
        // Yollarla çakışmaması için zemini -0.1f aşağıya alıyoruz
        CreateWall("Floor", new Vector3(center.x, -0.1f, center.z), new Vector3(sizeX * 2, 0.1f, sizeZ * 2), worldContainer.transform);

        // 3. ADIM: YAN DUVARLAR (4 Adet)
        // Arka Duvar
        CreateWall("Wall_Back", new Vector3(center.x, wallHeight / 2, center.z + sizeZ), new Vector3(sizeX * 2, wallHeight, 0.2f), worldContainer.transform);
        // Ön Duvar
        CreateWall("Wall_Front", new Vector3(center.x, wallHeight / 2, center.z - sizeZ), new Vector3(sizeX * 2, wallHeight, 0.2f), worldContainer.transform);
        // Sağ Duvar
        CreateWall("Wall_Right", new Vector3(center.x + sizeX, wallHeight / 2, center.z), new Vector3(0.2f, wallHeight, sizeZ * 2), worldContainer.transform);
        // Sol Duvar
        CreateWall("Wall_Left", new Vector3(center.x - sizeX, wallHeight / 2, center.z), new Vector3(0.2f, wallHeight, sizeZ * 2), worldContainer.transform);

        Debug.Log("WorldBox_Triangle: Duvarlar üçgen şehri kuşattı!");
    }

    void CreateWall(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.transform.SetParent(parent);
        wall.layer = 2; // Önemli: Yol seçimini (A-B) bozmaz

        if (boxMaterial != null) wall.GetComponent<Renderer>().material = boxMaterial;

        // Raycast'in geçmesi için Collider'ı siliyoruz
        if (wall.GetComponent<BoxCollider>() != null)
            Destroy(wall.GetComponent<BoxCollider>());
    }
}