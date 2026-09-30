using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    [Header("World Reference")]
    public WorldGrid worldGrid;

    [Header("Plant Rules")]
    [Range(0f, 1f)]
    public float minimumMoisture = 0.35f;

    [Range(0f, 1f)]
    public float spawnChance = 0.35f;

    public int seed = 2468;

    [Header("Plant Appearance")]
    [Min(0.05f)]
    public float plantWidth = 0.12f;

    [Min(0.05f)]
    public float plantHeight = 0.25f;

    [Header("Plant Food")]
    [Min(0.1f)]
    public float plantFood = 10f;

    private Transform plantContainer;
    private Material plantMaterial;

    private void Start()
    {
        if (worldGrid == null)
        {
            worldGrid = GetComponent<WorldGrid>();
        }

        if (worldGrid == null)
        {
            Debug.LogError(
                "PlantSpawner needs a WorldGrid reference."
            );

            return;
        }

        CreatePlantMaterial();
        SpawnPlants();
    }

    private void SpawnPlants()
    {
        plantContainer =
            new GameObject("Plants").transform;

        plantContainer.SetParent(transform, false);

        Random.InitState(seed);

        for (int x = 0; x < worldGrid.width; x++)
        {
            for (int z = 0; z < worldGrid.depth; z++)
            {
                if (worldGrid.IsWater(x, z))
                {
                    continue;
                }

                float moisture =
                    worldGrid.GetMoisture(x, z);

                if (moisture < minimumMoisture)
                {
                    continue;
                }

                if (Random.value > spawnChance)
                {
                    continue;
                }

                GameObject plant =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Cylinder
                    );

                plant.name = $"Plant_{x}_{z}";

                plant.transform.SetParent(
                    plantContainer,
                    true
                );

                plant.transform.localScale =
                    new Vector3(
                        plantWidth,
                        plantHeight,
                        plantWidth
                    );

                plant.transform.position =
                    worldGrid.GetCellWorldPosition(
                        x,
                        z,
                        plantHeight
                    );

                Destroy(plant.GetComponent<Collider>());

                Renderer plantRenderer =
                    plant.GetComponent<Renderer>();

                plantRenderer.sharedMaterial =
                    plantMaterial;

                Plant plantData =
                    plant.AddComponent<Plant>();

                plantData.Initialize(plantFood);
            }
        }
    }

    private void CreatePlantMaterial()
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        plantMaterial =
            new Material(shader);

        Color plantColor =
            new Color(0.05f, 0.65f, 0.08f);

        if (plantMaterial.HasProperty("_BaseColor"))
        {
            plantMaterial.SetColor(
                "_BaseColor",
                plantColor
            );
        }

        if (plantMaterial.HasProperty("_Color"))
        {
            plantMaterial.SetColor(
                "_Color",
                plantColor
            );
        }
    }
}