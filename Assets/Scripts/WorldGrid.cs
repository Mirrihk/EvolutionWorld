using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    [Header("World Size")]
    [Min(2)] public int width = 32;
    [Min(2)] public int depth = 32;
    [Min(0.1f)] public float cellSize = 1f;

    [Header("Terrain")]
    [Min(0.1f)] public float heightMultiplier = 3f;
    public float noiseScale = 0.12f;
    public int seed = 12345;

    private Transform generatedCells;
    private Material[] terrainMaterials;

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
        generatedCells.SetParent(transform);

        float centerX = (width - 1) / 2f;
        float centerZ = (depth - 1) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float noise = Mathf.PerlinNoise(
                    (x + seed) * noiseScale,
                    (z + seed) * noiseScale
                );

                float height = Mathf.Lerp(0.25f, heightMultiplier, noise);

                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.name = $"Cell_{x}_{z}";

                cell.transform.SetParent(generatedCells);
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
                renderer.sharedMaterial = terrainMaterials[GetTerrainType(noise)];
            }
        }
    }

    private int GetTerrainType(float height)
    {
        if (height < 0.2f) return 0;
        if (height < 0.4f) return 1;
        if (height < 0.6f) return 2;
        if (height < 0.8f) return 3;

        return 4;
    }

    private void CreateMaterials()
    {
        Color[] colors =
        {
            new Color(0.10f, 0.25f, 0.08f),
            new Color(0.20f, 0.50f, 0.12f),
            new Color(0.50f, 0.45f, 0.15f),
            new Color(0.35f, 0.25f, 0.15f),
            new Color(0.65f, 0.65f, 0.65f)
        };

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        terrainMaterials = new Material[colors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            terrainMaterials[i] = new Material(shader);
            terrainMaterials[i].color = colors[i];
        }
    }
}