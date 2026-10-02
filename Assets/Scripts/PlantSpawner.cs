using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    [Header("References")]
    public WorldGrid worldGrid;

    [Header("Population")]
    [Min(1)]
    public int plantsToSpawn = 150;

    public int seed = 24680;

    [Header("Rules")]
    [Range(0f, 1f)]
    public float minimumMoisture = 0.25f;

    [Range(0f, 1f)]
    public float aquaticPlantChance = 0.85f;

    [Header("Appearance")]
    [Min(0.05f)]
    public float plantSize = 0.2f;

    [Min(0.1f)]
    public float maxFood = 10f;

    private Transform plantContainer;

    private struct PlantDefinition
    {
        public PrimitiveType shape;
        public Color color;
        public float sizeMultiplier;
        public float foodMultiplier;
        public float spawnChance;
        public bool aquatic;
        public bool growsInDryTerrain;

        public PlantDefinition(
            PrimitiveType shape,
            Color color,
            float sizeMultiplier,
            float foodMultiplier,
            float spawnChance,
            bool aquatic,
            bool growsInDryTerrain)
        {
            this.shape = shape;
            this.color = color;
            this.sizeMultiplier = sizeMultiplier;
            this.foodMultiplier = foodMultiplier;
            this.spawnChance = spawnChance;
            this.aquatic = aquatic;
            this.growsInDryTerrain = growsInDryTerrain;
        }
    }

    private void Start()
    {
        if (worldGrid == null)
        {
            worldGrid =
                FindAnyObjectByType<WorldGrid>();
        }

        if (worldGrid == null)
        {
            Debug.LogError(
                "PlantSpawner could not find WorldGrid."
            );

            return;
        }

        plantContainer =
            new GameObject("Plants").transform;

        SpawnPlants();
    }

    private void SpawnPlants()
    {
        Random.InitState(seed);

        int spawned = 0;
        int attempts = 0;
        int maximumAttempts =
            plantsToSpawn * 50;

        while (spawned < plantsToSpawn &&
               attempts < maximumAttempts)
        {
            attempts++;

            int x =
                Random.Range(
                    0,
                    worldGrid.width
                );

            int z =
                Random.Range(
                    0,
                    worldGrid.depth
                );

            WorldTerrainType terrainType =
                worldGrid.GetTerrainType(x, z);

            PlantDefinition definition =
                GetPlantDefinition(
                    terrainType
                );

            if (definition.spawnChance <= 0f)
            {
                continue;
            }

            if (!definition.aquatic &&
                !definition.growsInDryTerrain &&
                worldGrid.GetMoisture(x, z) <
                minimumMoisture)
            {
                continue;
            }

            float chance =
                definition.spawnChance;

            if (definition.aquatic)
            {
                chance *=
                    aquaticPlantChance;
            }

            if (Random.value > chance)
            {
                continue;
            }

            SpawnPlant(
                spawned,
                x,
                z,
                terrainType,
                definition
            );

            spawned++;
        }

        Debug.Log(
            $"Spawned {spawned} biome plants."
        );
    }

    private void SpawnPlant(
        int index,
        int x,
        int z,
        WorldTerrainType terrainType,
        PlantDefinition definition)
    {
        float size =
            plantSize *
            definition.sizeMultiplier;

        GameObject plantObject =
            GameObject.CreatePrimitive(
                definition.shape
            );

        plantObject.name =
            $"{terrainType}_Plant_{index}";

        plantObject.transform.SetParent(
            plantContainer
        );

        plantObject.transform.localScale =
            Vector3.one * size;

        plantObject.transform.position =
            worldGrid.GetCellWorldPosition(
                x,
                z,
                size
            );

        Collider collider =
            plantObject.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer =
            plantObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material =
                CreateMaterial(
                    definition.color
                );
        }

        Plant plant =
            plantObject.AddComponent<Plant>();

        plant.terrainType =
            terrainType;

        plant.plantName =
            $"{terrainType} plant";

        plant.Initialize(
            maxFood *
            definition.foodMultiplier
        );
    }

    private Material CreateMaterial(
        Color color)
    {
        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );

        if (shader == null)
        {
            shader =
                Shader.Find("Standard");
        }

        Material material =
            new Material(shader);

        if (material.HasProperty(
                "_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                color
            );
        }

        if (material.HasProperty(
                "_Color"))
        {
            material.SetColor(
                "_Color",
                color
            );
        }

        return material;
    }

    private PlantDefinition GetPlantDefinition(
        WorldTerrainType terrainType)
    {
        switch (terrainType)
        {
            case WorldTerrainType.River:
                return Aquatic(
                    new Color(0.10f, 0.70f, 0.50f),
                    0.55f,
                    1.2f,
                    0.90f
                );

            case WorldTerrainType.Lake:
                return Aquatic(
                    new Color(0.08f, 0.60f, 0.45f),
                    0.70f,
                    1.4f,
                    0.90f
                );

            case WorldTerrainType.Ocean:
                return Aquatic(
                    new Color(0.04f, 0.35f, 0.55f),
                    0.45f,
                    1.1f,
                    0.35f
                );

            case WorldTerrainType.CoralReef:
                return Aquatic(
                    new Color(0.95f, 0.25f, 0.55f),
                    0.55f,
                    1.5f,
                    0.95f
                );

            case WorldTerrainType.Fjord:
                return Aquatic(
                    new Color(0.12f, 0.55f, 0.65f),
                    0.55f,
                    1.3f,
                    0.60f
                );

            case WorldTerrainType.DeepSea:
                return Aquatic(
                    new Color(0.20f, 0.50f, 0.90f),
                    0.45f,
                    1.6f,
                    0.45f
                );

            case WorldTerrainType.KelpForest:
                return Aquatic(
                    new Color(0.02f, 0.65f, 0.38f),
                    1.8f,
                    2.0f,
                    1.00f
                );

            case WorldTerrainType.Trench:
                return Aquatic(
                    new Color(0.55f, 0.15f, 0.75f),
                    0.35f,
                    1.8f,
                    0.25f
                );

            case WorldTerrainType.Forest:
                return Land(
                    new Color(0.03f, 0.28f, 0.04f),
                    1.3f,
                    1.5f,
                    0.95f,
                    false
                );

            case WorldTerrainType.Jungle:
                return Land(
                    new Color(0.01f, 0.45f, 0.03f),
                    1.8f,
                    2.0f,
                    0.95f,
                    false
                );

            case WorldTerrainType.Desert:
                return Land(
                    new Color(0.20f, 0.65f, 0.12f),
                    0.65f,
                    0.8f,
                    0.80f,
                    true
                );

            case WorldTerrainType.Oasis:
                return Land(
                    new Color(0.05f, 0.80f, 0.18f),
                    1.5f,
                    1.8f,
                    0.95f,
                    true
                );

            case WorldTerrainType.Swamp:
                return Land(
                    new Color(0.16f, 0.32f, 0.08f),
                    0.9f,
                    1.3f,
                    0.90f,
                    false
                );

            case WorldTerrainType.Marsh:
                return Land(
                    new Color(0.35f, 0.55f, 0.12f),
                    0.8f,
                    1.1f,
                    0.90f,
                    false
                );

            case WorldTerrainType.Wetland:
                return Land(
                    new Color(0.08f, 0.55f, 0.25f),
                    1.0f,
                    1.4f,
                    0.90f,
                    false
                );

            case WorldTerrainType.Mangrove:
                return Land(
                    new Color(0.03f, 0.38f, 0.25f),
                    1.7f,
                    1.8f,
                    0.95f,
                    false
                );

            case WorldTerrainType.Savanna:
                return Land(
                    new Color(0.62f, 0.72f, 0.10f),
                    1.1f,
                    1.0f,
                    0.90f,
                    true
                );

            case WorldTerrainType.Grassland:
                return Land(
                    new Color(0.18f, 0.70f, 0.08f),
                    0.9f,
                    1.0f,
                    0.95f,
                    false
                );

            case WorldTerrainType.Tundra:
                return Land(
                    new Color(0.42f, 0.60f, 0.48f),
                    0.6f,
                    0.8f,
                    0.65f,
                    false
                );

            case WorldTerrainType.Beach:
                return Land(
                    new Color(0.40f, 0.75f, 0.20f),
                    0.65f,
                    0.8f,
                    0.75f,
                    true
                );

            case WorldTerrainType.Cave:
                return Land(
                    new Color(0.70f, 0.20f, 0.90f),
                    0.4f,
                    1.2f,
                    0.40f,
                    true
                );

            case WorldTerrainType.Land:
            default:
                return Land(
                    new Color(0.12f, 0.65f, 0.08f),
                    1.0f,
                    1.0f,
                    0.85f,
                    false
                );
        }
    }

    private PlantDefinition Land(
        Color color,
        float sizeMultiplier,
        float foodMultiplier,
        float spawnChance,
        bool growsInDryTerrain)
    {
        return new PlantDefinition(
            PrimitiveType.Cylinder,
            color,
            sizeMultiplier,
            foodMultiplier,
            spawnChance,
            false,
            growsInDryTerrain
        );
    }

    private PlantDefinition Aquatic(
        Color color,
        float sizeMultiplier,
        float foodMultiplier,
        float spawnChance)
    {
        return new PlantDefinition(
            PrimitiveType.Sphere,
            color,
            sizeMultiplier,
            foodMultiplier,
            spawnChance,
            true,
            true
        );
    }
}