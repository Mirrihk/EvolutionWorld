using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    [Header("World Size")]
    [Min(2)] public int width = 32;
    [Min(2)] public int depth = 32;
    [Min(0.1f)] public float cellSize = 1f;

    [Header("Terrain")]
    [Min(0.1f)] public float heightMultiplier = 3f;
    [Range(0.01f, 1f)] public float noiseScale = 0.12f;
    public int seed = 12345;

    [Header("Water and Moisture")]
    [Range(0f, 1f)] public float waterLevel = 0.35f;
    [Range(0.01f, 1f)] public float moistureScale = 0.08f;
    public int moistureSeed = 54321;
    [Min(0.01f)] public float waterHeight = 0.08f;

    private Transform generatedCells;
    private Material[] terrainMaterials;
    private Material waterMaterial;

    // These arrays store environmental information for each grid cell.
    private float[,] moistureMap;
    private bool[,] waterMap;

    private void Start()
    {
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        CreateMaterials();

        if (generatedCells != null)
        {
            Destroy(generatedCells.gameObject);
        }

        generatedCells = new GameObject("GeneratedCells").transform;
        generatedCells.SetParent(transform, false);

        moistureMap = new float[width, depth];
        waterMap = new bool[width, depth];

        float centerX = (width - 1) / 2f;
        float centerZ = (depth - 1) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // Elevation controls the shape of the land.
                float elevation = Mathf.PerlinNoise(
                    (x + seed) * noiseScale,
                    (z + seed) * noiseScale
                );

                // Moisture is a separate noise field.
                float moisture = Mathf.PerlinNoise(
                    (x + moistureSeed) * moistureScale,
                    (z + moistureSeed) * moistureScale
                );

                bool isWater = elevation < waterLevel;

                moistureMap[x, z] = moisture;
                waterMap[x, z] = isWater;

                float height = isWater
                    ? waterHeight
                    : Mathf.Lerp(0.25f, heightMultiplier, elevation);

                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.name = $"Cell_{x}_{z}";

                cell.transform.SetParent(generatedCells, false);

                cell.transform.localPosition = new Vector3(
                    (x - centerX) * cellSize,
                    height / 2f,
                    (z - centerZ) * cellSize
                );

                cell.transform.localScale = new Vector3(
                    cellSize * 0.92f,
                    height,
                    cellSize * 0.92f
                );

                Destroy(cell.GetComponent<Collider>());

                Renderer renderer = cell.GetComponent<Renderer>();

                if (isWater)
                {
                    renderer.sharedMaterial = waterMaterial;
                }
                else
                {
                    int terrainType = GetTerrainType(elevation, moisture);
                    renderer.sharedMaterial = terrainMaterials[terrainType];
                }
            }
        }
    }

    private int GetTerrainType(float elevation, float moisture)
    {
        if (elevation > 0.82f)
        {
            return 4; // Mountain
        }

        if (moisture < 0.25f)
        {
            return 2; // Dry land
        }

        if (moisture > 0.70f)
        {
            return 0; // Very fertile land
        }

        if (elevation < 0.55f)
        {
            return 1; // Grassland
        }

        return 3; // Hills
    }

    private void CreateMaterials()
    {
        Color[] colors =
        {
            new Color(0.10f, 0.25f, 0.08f), // Fertile
            new Color(0.20f, 0.50f, 0.12f), // Grass
            new Color(0.65f, 0.50f, 0.18f), // Dry
            new Color(0.35f, 0.25f, 0.15f), // Hills
            new Color(0.65f, 0.65f, 0.65f)  // Mountains
        };

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        terrainMaterials = new Material[colors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            terrainMaterials[i] = CreateMaterial(shader, colors[i]);
        }

        waterMaterial = CreateMaterial(
            shader,
            new Color(0.05f, 0.30f, 0.85f)
        );
    }

    private Material CreateMaterial(Shader shader, Color color)
    {
        Material material = new Material(shader);

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }

    // These methods will be used later by plants and animals.
    public bool IsWater(int x, int z)
    {
        if (!IsInsideGrid(x, z) || waterMap == null)
        {
            return false;
        }

        return waterMap[x, z];
    }

    public float GetMoisture(int x, int z)
    {
        if (!IsInsideGrid(x, z) || moistureMap == null)
        {
            return 0f;
        }

        return moistureMap[x, z];
    }

    private bool IsInsideGrid(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < depth;
    }
}