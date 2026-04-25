using UnityEngine;

public class WorldBoxGenerator : MonoBehaviour
{
    private GridGenerator gridGen;

    [Header("Kutu Ayarları")]
    public Material boxMaterial; // Koyu bir renk veya hafif yansımalı bir materyal
    public float boxPadding = 10f; // Şehirle duvar arasındaki boşluk
    public float wallHeight = 50f; // Duvarların yüksekliği

    void Start()
    {
        gridGen = GetComponent<GridGenerator>();
        // Şehir oluştuktan sonra kutuyu çiz
        Invoke("BuildBox", 0.2f);
    }

    public void BuildBox()
    {
        // Şehrin toplam yarıçapını hesapla
        float cityRadius = gridGen.rings * gridGen.spacing * 2.5f; // Clusterları da kapsayacak şekilde
        float size = cityRadius + boxPadding;

        GameObject worldContainer = new GameObject("WORLD_BOX");

        // 1. ZEMİN (Taban)
        CreateWall("Floor", new Vector3(0, -0.1f, 0), new Vector3(size * 2, 0.1f, size * 2), worldContainer.transform);

        // 2. YAN DUVARLAR (4 Adet)
        // Arka
        CreateWall("Wall_Back", new Vector3(0, wallHeight / 2, size), new Vector3(size * 2, wallHeight, 0.1f), worldContainer.transform);
        // Ön
        CreateWall("Wall_Front", new Vector3(0, wallHeight / 2, -size), new Vector3(size * 2, wallHeight, 0.1f), worldContainer.transform);
        // Sağ
        CreateWall("Wall_Right", new Vector3(size, wallHeight / 2, 0), new Vector3(0.1f, wallHeight, size * 2), worldContainer.transform);
        // Sol
        CreateWall("Wall_Left", new Vector3(-size, wallHeight / 2, 0), new Vector3(0.1f, wallHeight, size * 2), worldContainer.transform);
    }

    void CreateWall(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.transform.SetParent(parent);

        if (boxMaterial != null) wall.GetComponent<Renderer>().material = boxMaterial;

        // Kamera içinden geçebilsin istersen collider'ı silebilirsin
        // Destroy(wall.GetComponent<BoxCollider>());
    }
}