using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    [Header("World Size")]
    [Min(2)]
    public int width = 32;

    [Min(2)]
    public int depth = 32;

    [Min(0.1f)]
    public float cellSize = 1f;

    [Header("Terrain")]
    [Min(0.1f)]
    public float heightMultiplier = 3f;

    [Range(0.01f, 1f)]
    public float noiseScale = 0.12f;

    public int seed = 12345;

    [Header("Water")]
    [Range(0f, 1f)]
    public float waterLevel = 0.35f;

    [Min(0.01f)]
    public float waterHeight = 0.08f;

    [Header("Moisture")]
    [Range(0.01f, 1f)]
    public float moistureScale = 0.08f;

    public int moistureSeed = 54321;

    [Header("Climate")]
    [Range(0.01f, 1f)]
    public float temperatureScale = 0.07f;

    public int temperatureSeed = 67890;

    [Range(0f, 1f)]
    public float iceTemperatureThreshold = 0.28f;

    [Range(0f, 1f)]
    public float mountainElevationThreshold = 0.82f;

    [Header("Biome Generation")]
    [Range(0.01f, 1f)]
    public float biomeScale = 0.08f;

    public int biomeSeed = 24680;

    [Range(0f, 1f)]
    public float urbanChance = 0f;

    [Range(0f, 1f)]
    public float caveChance = 0.08f;

    [Range(0.001f, 0.2f)]
    public float riverWidth = 0.025f;

    private Transform generatedCells;

    private Material[] terrainMaterials;

    private float[,] elevationMap;
    private float[,] moistureMap;
    private float[,] temperatureMap;

    private bool[,] waterMap;
    private WorldTerrainType[,] terrainMap;

    private void Awake()
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

        generatedCells =
            new GameObject("GeneratedCells").transform;

        generatedCells.SetParent(transform, false);

        elevationMap = new float[width, depth];
        moistureMap = new float[width, depth];
        temperatureMap = new float[width, depth];

        waterMap = new bool[width, depth];
        terrainMap = new WorldTerrainType[width, depth];

        GenerateEnvironmentalMaps();
        ClassifyTerrain();
        BuildTerrainCells();
    }

    private void GenerateEnvironmentalMaps()
    {
        float centerZ =
            (depth - 1) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float elevation =
                    Mathf.PerlinNoise(
                        (x + seed) * noiseScale,
                        (z + seed) * noiseScale
                    );

                float moisture =
                    Mathf.PerlinNoise(
                        (x + moistureSeed) * moistureScale,
                        (z + moistureSeed) * moistureScale
                    );

                float temperatureNoise =
                    Mathf.PerlinNoise(
                        (x + temperatureSeed) * temperatureScale,
                        (z + temperatureSeed) * temperatureScale
                    );

                float latitude =
                    Mathf.Abs(
                        (z - centerZ) /
                        Mathf.Max(1f, centerZ)
                    );

                float temperature =
                    Mathf.Clamp01(
                        temperatureNoise * 0.65f +
                        (1f - latitude) * 0.35f -
                        elevation * 0.15f
                    );

                elevationMap[x, z] = elevation;
                moistureMap[x, z] = moisture;
                temperatureMap[x, z] = temperature;

                waterMap[x, z] =
                    elevation < waterLevel;
            }
        }
    }

    private void ClassifyTerrain()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                terrainMap[x, z] =
                    DetermineTerrainType(x, z);
            }
        }
    }

    private void BuildTerrainCells()
    {
        float centerX =
            (width - 1) / 2f;

        float centerZ =
            (depth - 1) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                WorldTerrainType terrainType =
                    terrainMap[x, z];

                float cellHeight =
                    GetCellHeight(
                        elevationMap[x, z],
                        terrainType
                    );

                GameObject cell =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Cube
                    );

                cell.name = $"Cell_{x}_{z}";

                cell.transform.SetParent(
                    generatedCells,
                    false
                );

                cell.transform.localPosition =
                    new Vector3(
                        (x - centerX) * cellSize,
                        cellHeight / 2f,
                        (z - centerZ) * cellSize
                    );

                cell.transform.localScale =
                    new Vector3(
                        cellSize * 0.92f,
                        cellHeight,
                        cellSize * 0.92f
                    );

                Collider cellCollider =
                    cell.GetComponent<Collider>();

                if (cellCollider != null)
                {
                    Destroy(cellCollider);
                }

                Renderer cellRenderer =
                    cell.GetComponent<Renderer>();

                cellRenderer.sharedMaterial =
                    terrainMaterials[(int)terrainType];
            }
        }
    }

    private WorldTerrainType DetermineTerrainType(
        int x,
        int z)
    {
        float elevation = elevationMap[x, z];
        float moisture = moistureMap[x, z];
        float temperature = temperatureMap[x, z];

        if (waterMap[x, z])
        {
            return DetermineWaterTerrain(
                x,
                z,
                elevation,
                temperature
            );
        }

        bool nearWater =
            IsAdjacentToWater(x, z);

        float localSlope =
            GetLocalSlope(x, z);

        float biomeNoise =
            GetBiomeNoise(x, z, 10);

        float specialNoise =
            GetBiomeNoise(x, z, 50);

        if (urbanChance > 0f &&
            biomeNoise > 1f - urbanChance)
        {
            return WorldTerrainType.Urban;
        }

        if (nearWater &&
            elevation <= waterLevel + 0.05f)
        {
            if (moisture > 0.72f &&
                temperature > 0.58f)
            {
                return WorldTerrainType.Mangrove;
            }

            return WorldTerrainType.Beach;
        }

        if (elevation >= mountainElevationThreshold)
        {
            if (temperature < 0.20f)
            {
                return WorldTerrainType.Glacier;
            }

            if (specialNoise > 0.90f)
            {
                return WorldTerrainType.Volcano;
            }

            if (specialNoise < caveChance)
            {
                return WorldTerrainType.Cave;
            }

            if (localSlope > 0.32f)
            {
                return WorldTerrainType.Cliff;
            }

            return WorldTerrainType.Mountain;
        }

        if (localSlope > 0.32f)
        {
            return WorldTerrainType.Cliff;
        }

        if (localSlope > 0.20f &&
            moisture < 0.40f)
        {
            return WorldTerrainType.Canyon;
        }

        if (elevation > 0.70f &&
            localSlope < 0.12f)
        {
            return WorldTerrainType.Plateau;
        }

        if (temperature < 0.12f)
        {
            return WorldTerrainType.Snow;
        }

        if (temperature <= iceTemperatureThreshold)
        {
            if (elevation > 0.60f)
            {
                return WorldTerrainType.Glacier;
            }

            return WorldTerrainType.Ice;
        }

        if (temperature < 0.36f)
        {
            return WorldTerrainType.Tundra;
        }

        if (moisture > 0.84f &&
            elevation < 0.48f)
        {
            return WorldTerrainType.Swamp;
        }

        if (moisture > 0.76f &&
            elevation < 0.42f)
        {
            return WorldTerrainType.Marsh;
        }

        if (moisture > 0.78f &&
            temperature > 0.70f)
        {
            return WorldTerrainType.Jungle;
        }

        if (moisture > 0.68f &&
            temperature > 0.48f)
        {
            return WorldTerrainType.Forest;
        }

        if (moisture > 0.72f)
        {
            return WorldTerrainType.Wetland;
        }

        if (moisture < 0.22f &&
            temperature > 0.68f)
        {
            if (specialNoise > 0.90f)
            {
                return WorldTerrainType.Oasis;
            }

            if (elevation > 0.55f)
            {
                return WorldTerrainType.Badlands;
            }

            return WorldTerrainType.Desert;
        }

        if (moisture < 0.36f &&
            temperature > 0.48f)
        {
            return WorldTerrainType.Steppe;
        }

        if (temperature > 0.68f &&
            moisture < 0.58f)
        {
            return WorldTerrainType.Savanna;
        }

        if (moisture > 0.42f &&
            moisture < 0.68f)
        {
            return WorldTerrainType.Grassland;
        }

        return WorldTerrainType.Land;
    }

    private WorldTerrainType DetermineWaterTerrain(
        int x,
        int z,
        float elevation,
        float temperature)
    {
        bool nearLand =
            IsAdjacentToLand(x, z);

        bool nearMountain =
            IsAdjacentToHighGround(x, z);

        bool edgeCell =
            IsEdgeCell(x, z);

        float waterDepth =
            Mathf.InverseLerp(
                waterLevel,
                0f,
                elevation
            );

        bool shallowWater =
            waterDepth < 0.35f;

        if (nearMountain && edgeCell)
        {
            return WorldTerrainType.Fjord;
        }

        if (shallowWater &&
            nearLand &&
            temperature > 0.55f)
        {
            return WorldTerrainType.CoralReef;
        }

        if (!edgeCell &&
            nearLand &&
            temperature > 0.40f &&
            GetBiomeNoise(x, z, 720) > 0.55f &&
            waterDepth < 0.55f)
        {
            return WorldTerrainType.KelpForest;
        }

        if (!edgeCell &&
            nearLand &&
            IsRiverChannel(x, z))
        {
            return WorldTerrainType.River;
        }

        if (waterDepth > 0.84f)
        {
            return WorldTerrainType.Trench;
        }

        if (waterDepth > 0.55f)
        {
            return WorldTerrainType.DeepSea;
        }

        if (edgeCell ||
            elevation < waterLevel - 0.12f)
        {
            return WorldTerrainType.Ocean;
        }

        if (nearLand)
        {
            return WorldTerrainType.Lake;
        }

        return WorldTerrainType.Water;
    }

    private float GetCellHeight(
        float elevation,
        WorldTerrainType terrainType)
    {
        if (IsAquaticTerrain(terrainType))
        {
            return waterHeight;
        }

        if (terrainType == WorldTerrainType.Beach)
        {
            return Mathf.Lerp(
                0.20f,
                0.45f,
                elevation
            );
        }

        if (terrainType == WorldTerrainType.Canyon ||
            terrainType == WorldTerrainType.Cliff ||
            terrainType == WorldTerrainType.Mountain ||
            terrainType == WorldTerrainType.Volcano ||
            terrainType == WorldTerrainType.Plateau ||
            terrainType == WorldTerrainType.Cave)
        {
            return Mathf.Lerp(
                0.40f,
                heightMultiplier,
                elevation
            );
        }

        return Mathf.Lerp(
            0.25f,
            heightMultiplier,
            elevation
        );
    }

    private void CreateMaterials()
    {
        int terrainCount =
            (int)WorldTerrainType.Trench + 1;

        terrainMaterials =
            new Material[terrainCount];

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        for (int i = 0; i < terrainCount; i++)
        {
            WorldTerrainType terrainType =
                (WorldTerrainType)i;

            terrainMaterials[i] =
                CreateMaterial(
                    shader,
                    GetTerrainColor(terrainType)
                );
        }
    }

    private Color GetTerrainColor(
        WorldTerrainType terrainType)
    {
        switch (terrainType)
        {
            case WorldTerrainType.Land:
                return new Color(0.42f, 0.38f, 0.22f);

            case WorldTerrainType.Water:
                return new Color(0.05f, 0.30f, 0.85f);

            case WorldTerrainType.Mountain:
                return new Color(0.45f, 0.45f, 0.45f);

            case WorldTerrainType.Forest:
                return new Color(0.08f, 0.30f, 0.08f);

            case WorldTerrainType.Desert:
                return new Color(0.82f, 0.65f, 0.30f);

            case WorldTerrainType.Swamp:
                return new Color(0.18f, 0.28f, 0.12f);

            case WorldTerrainType.Grassland:
                return new Color(0.28f, 0.58f, 0.16f);

            case WorldTerrainType.Tundra:
                return new Color(0.48f, 0.58f, 0.52f);

            case WorldTerrainType.Urban:
                return new Color(0.30f, 0.30f, 0.34f);

            case WorldTerrainType.Snow:
                return new Color(0.92f, 0.96f, 1.00f);

            case WorldTerrainType.Beach:
                return new Color(0.90f, 0.78f, 0.45f);

            case WorldTerrainType.River:
                return new Color(0.10f, 0.55f, 0.95f);

            case WorldTerrainType.Lake:
                return new Color(0.04f, 0.25f, 0.70f);

            case WorldTerrainType.Ocean:
                return new Color(0.02f, 0.08f, 0.40f);

            case WorldTerrainType.Canyon:
                return new Color(0.55f, 0.20f, 0.08f);

            case WorldTerrainType.Volcano:
                return new Color(0.20f, 0.04f, 0.02f);

            case WorldTerrainType.Glacier:
                return new Color(0.45f, 0.82f, 1.00f);

            case WorldTerrainType.Marsh:
                return new Color(0.25f, 0.42f, 0.20f);

            case WorldTerrainType.Savanna:
                return new Color(0.68f, 0.62f, 0.20f);

            case WorldTerrainType.Jungle:
                return new Color(0.02f, 0.22f, 0.04f);

            case WorldTerrainType.Ice:
                return new Color(0.65f, 0.90f, 1.00f);

            case WorldTerrainType.Cliff:
                return new Color(0.25f, 0.25f, 0.28f);

            case WorldTerrainType.Plateau:
                return new Color(0.48f, 0.34f, 0.18f);

            case WorldTerrainType.Wetland:
                return new Color(0.20f, 0.48f, 0.34f);

            case WorldTerrainType.CoralReef:
                return new Color(0.95f, 0.30f, 0.55f);

            case WorldTerrainType.Cave:
                return new Color(0.08f, 0.07f, 0.08f);

            case WorldTerrainType.Steppe:
                return new Color(0.48f, 0.50f, 0.20f);

            case WorldTerrainType.Badlands:
                return new Color(0.68f, 0.25f, 0.08f);

            case WorldTerrainType.Oasis:
                return new Color(0.10f, 0.70f, 0.25f);

            case WorldTerrainType.Mangrove:
                return new Color(0.04f, 0.32f, 0.25f);

            case WorldTerrainType.Fjord:
                return new Color(0.12f, 0.38f, 0.60f);

            case WorldTerrainType.DeepSea:
                return new Color(0.01f, 0.04f, 0.20f);

            case WorldTerrainType.KelpForest:
                return new Color(0.02f, 0.28f, 0.18f);

            case WorldTerrainType.Trench:
                return new Color(0.00f, 0.01f, 0.06f);

            default:
                return Color.white;
        }
    }

    private Material CreateMaterial(
        Shader shader,
        Color color)
    {
        Material material =
            new Material(shader);

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                color
            );
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor(
                "_Color",
                color
            );
        }

        return material;
    }

    private bool IsAdjacentToWater(
        int x,
        int z)
    {
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
            {
                if (offsetX == 0 &&
                    offsetZ == 0)
                {
                    continue;
                }

                int neighborX = x + offsetX;
                int neighborZ = z + offsetZ;

                if (!IsInsideGrid(neighborX, neighborZ))
                {
                    continue;
                }

                if (waterMap[neighborX, neighborZ])
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsAdjacentToLand(
        int x,
        int z)
    {
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
            {
                if (offsetX == 0 &&
                    offsetZ == 0)
                {
                    continue;
                }

                int neighborX = x + offsetX;
                int neighborZ = z + offsetZ;

                if (!IsInsideGrid(neighborX, neighborZ))
                {
                    continue;
                }

                if (!waterMap[neighborX, neighborZ])
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsAdjacentToHighGround(
        int x,
        int z)
    {
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
            {
                if (offsetX == 0 &&
                    offsetZ == 0)
                {
                    continue;
                }

                int neighborX = x + offsetX;
                int neighborZ = z + offsetZ;

                if (!IsInsideGrid(neighborX, neighborZ))
                {
                    continue;
                }

                if (elevationMap[neighborX, neighborZ] >=
                    mountainElevationThreshold)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsRiverChannel(
        int x,
        int z)
    {
        float riverNoise =
            Mathf.PerlinNoise(
                (x + biomeSeed + 9001) * 0.045f,
                (z + biomeSeed + 9001) * 0.045f
            );

        return Mathf.Abs(riverNoise - 0.5f)
               <= riverWidth;
    }

    private bool IsEdgeCell(
        int x,
        int z)
    {
        return x == 0 ||
               z == 0 ||
               x == width - 1 ||
               z == depth - 1;
    }

    private float GetLocalSlope(
        int x,
        int z)
    {
        float currentElevation =
            elevationMap[x, z];

        float largestDifference = 0f;

        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
            {
                if (offsetX == 0 &&
                    offsetZ == 0)
                {
                    continue;
                }

                int neighborX = x + offsetX;
                int neighborZ = z + offsetZ;

                if (!IsInsideGrid(neighborX, neighborZ))
                {
                    continue;
                }

                float difference =
                    Mathf.Abs(
                        currentElevation -
                        elevationMap[neighborX, neighborZ]
                    );

                if (difference > largestDifference)
                {
                    largestDifference = difference;
                }
            }
        }

        return largestDifference;
    }

    private float GetBiomeNoise(
        int x,
        int z,
        int salt)
    {
        return Mathf.PerlinNoise(
            (x + biomeSeed + salt) * biomeScale,
            (z + biomeSeed + salt) * biomeScale
        );
    }

    public bool IsWater(
        int x,
        int z)
    {
        if (!IsInsideGrid(x, z) ||
            terrainMap == null)
        {
            return false;
        }

        return IsAquaticTerrain(
            terrainMap[x, z]
        );
    }

    public bool IsIce(
        int x,
        int z)
    {
        if (!IsInsideGrid(x, z) ||
            terrainMap == null)
        {
            return false;
        }

        return IsColdTerrain(
            terrainMap[x, z]
        );
    }

    public bool IsMountain(
        int x,
        int z)
    {
        if (!IsInsideGrid(x, z) ||
            terrainMap == null)
        {
            return false;
        }

        return IsMountainTerrain(
            terrainMap[x, z]
        );
    }

    public WorldTerrainType GetTerrainType(
        int x,
        int z)
    {
        if (!IsInsideGrid(x, z) ||
            terrainMap == null)
        {
            return WorldTerrainType.Land;
        }

        return terrainMap[x, z];
    }

    public float GetMoisture(
        int x,
        int z)
    {
        if (!IsInsideGrid(x, z) ||
            moistureMap == null)
        {
            return 0f;
        }

        return moistureMap[x, z];
    }

    public float GetTemperature(
        int x,
        int z)
    {
        if (!IsInsideGrid(x, z) ||
            temperatureMap == null)
        {
            return 0.5f;
        }

        return temperatureMap[x, z];
    }

    public float GetElevation(
        int x,
        int z)
    {
        if (!IsInsideGrid(x, z) ||
            elevationMap == null)
        {
            return 0f;
        }

        return elevationMap[x, z];
    }

    public float GetSurfaceHeight(
        int x,
        int z)
    {
        if (!IsInsideGrid(x, z) ||
            elevationMap == null ||
            terrainMap == null)
        {
            return 0.25f;
        }

        return GetCellHeight(
            elevationMap[x, z],
            terrainMap[x, z]
        );
    }

    public Vector3 GetCellWorldPosition(
        int x,
        int z,
        float verticalOffset = 0f)
    {
        float centerX =
            (width - 1) / 2f;

        float centerZ =
            (depth - 1) / 2f;

        Vector3 localPosition =
            new Vector3(
                (x - centerX) * cellSize,
                GetSurfaceHeight(x, z) +
                verticalOffset,
                (z - centerZ) * cellSize
            );

        return transform.TransformPoint(
            localPosition
        );
    }

    public bool TryGetCellCoordinates(
        Vector3 worldPosition,
        out int x,
        out int z)
    {
        Vector3 localPosition =
            transform.InverseTransformPoint(
                worldPosition
            );

        float centerX =
            (width - 1) / 2f;

        float centerZ =
            (depth - 1) / 2f;

        x = Mathf.RoundToInt(
            localPosition.x / cellSize +
            centerX
        );

        z = Mathf.RoundToInt(
            localPosition.z / cellSize +
            centerZ
        );

        return IsInsideGrid(x, z);
    }

    public bool IsInsideGrid(
        int x,
        int z)
    {
        return x >= 0 &&
               x < width &&
               z >= 0 &&
               z < depth;
    }

    public static bool IsAquaticTerrain(
        WorldTerrainType terrainType)
    {
        return terrainType ==
                   WorldTerrainType.Water ||
               terrainType ==
                   WorldTerrainType.River ||
               terrainType ==
                   WorldTerrainType.Lake ||
               terrainType ==
                   WorldTerrainType.Ocean ||
               terrainType ==
                   WorldTerrainType.CoralReef ||
               terrainType ==
                   WorldTerrainType.Fjord ||
               terrainType ==
                   WorldTerrainType.DeepSea ||
               terrainType ==
                   WorldTerrainType.KelpForest ||
               terrainType ==
                   WorldTerrainType.Trench;
    }

    public static bool IsColdTerrain(
        WorldTerrainType terrainType)
    {
        return terrainType ==
                   WorldTerrainType.Ice ||
               terrainType ==
                   WorldTerrainType.Snow ||
               terrainType ==
                   WorldTerrainType.Tundra ||
               terrainType ==
                   WorldTerrainType.Glacier;
    }

    public static bool IsMountainTerrain(
        WorldTerrainType terrainType)
    {
        return terrainType ==
                   WorldTerrainType.Mountain ||
               terrainType ==
                   WorldTerrainType.Cliff ||
               terrainType ==
                   WorldTerrainType.Plateau ||
               terrainType ==
                   WorldTerrainType.Canyon ||
               terrainType ==
                   WorldTerrainType.Volcano ||
               terrainType ==
                   WorldTerrainType.Cave ||
               terrainType ==
                   WorldTerrainType.Glacier;
    }
}