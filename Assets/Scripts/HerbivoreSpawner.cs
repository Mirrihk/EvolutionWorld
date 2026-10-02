using UnityEngine;

public class HerbivoreSpawner : MonoBehaviour
{
    [Header("References")]
    public WorldGrid worldGrid;

    [Header("Population")]
    [Min(0)]
    public int startingHerbivores = 12;

    [Min(1)]
    public int maxPopulation = 60;

    public int seed = 24680;

    [Header("Appearance")]
    [Min(0.05f)]
    public float herbivoreSize = 0.35f;

    public Color herbivoreColor =
        new Color(0.35f, 0.8f, 0.25f);

    [Header("Genome Defaults")]
    [Min(0.01f)]
    public float speed = 1.4f;

    [Min(1f)]
    public float maxEnergy = 100f;

    [Min(0f)]
    public float startingEnergy = 75f;

    [Min(0f)]
    public float energyDrainPerSecond = 0.8f;

    [Min(0.01f)]
    public float foodPerBite = 8f;

    [Min(0.01f)]
    public float eatingDistance = 0.75f;

    [Header("Life Cycle")]
    [Min(1f)]
    public float lifespan = 180f;

    [Min(0f)]
    public float maturityAge = 25f;

    [Min(0f)]
    public float reproductionCooldown = 20f;

    [Min(0f)]
    public float reproductionEnergyThreshold = 65f;

    [Min(0f)]
    public float reproductionEnergyCost = 30f;

    [Min(0f)]
    public float offspringStartingEnergy = 35f;

    [Header("Mutation")]
    [Range(0f, 1f)]
    public float mutationChance = 0.08f;

    [Range(0f, 1f)]
    public float mutationStrength = 0.15f;

    [Header("Starting Habitat")]
    [Range(0f, 1f)]
    public float startingAquaticChance = 0.20f;

    [Range(0f, 1f)]
    public float startingAmphibiousChance = 0.20f;

    [Range(0f, 1f)]
    public float landAdaptation = 0.90f;

    [Range(0f, 1f)]
    public float aquaticAdaptation = 0.25f;

    [Range(0f, 1f)]
    public float flightAdaptation = 0.05f;

    [Range(0f, 1f)]
    public float coldTolerance = 0.50f;

    [Range(0f, 1f)]
    public float mountainAdaptation = 0.40f;

    private void Awake()
    {
        if (worldGrid == null)
        {
            worldGrid =
                FindAnyObjectByType<WorldGrid>();
        }
    }

    private void Start()
    {
        if (worldGrid == null)
        {
            Debug.LogError(
                "HerbivoreSpawner could not find a WorldGrid."
            );

            return;
        }

        Random.InitState(seed);
        SpawnStartingHerbivores();
    }

    public void SpawnStartingHerbivores()
    {
        for (
            int i = 0;
            i < startingHerbivores;
            i++)
        {
            if (
                GetCurrentPopulation() >=
                maxPopulation)
            {
                return;
            }

            HerbivoreGenome genome =
                CreateStartingGenome();

            if (
                !TryGetSpawnPosition(
                    genome,
                    out Vector3 spawnPosition))
            {
                Debug.LogWarning(
                    "Could not find a suitable habitat for a herbivore."
                );

                continue;
            }

            CreateHerbivore(
                spawnPosition,
                genome,
                $"Herbivore_{i}"
            );
        }
    }

    public Herbivore SpawnOffspring(
        Vector3 spawnPosition,
        HerbivoreGenome parentGenomeA,
        HerbivoreGenome parentGenomeB)
    {
        if (
            GetCurrentPopulation() >=
            maxPopulation)
        {
            return null;
        }

        HerbivoreGenome childGenome =
            CreateChildGenome(
                parentGenomeA,
                parentGenomeB
            );

        return CreateHerbivore(
            spawnPosition,
            childGenome,
            "Herbivore_Offspring"
        );
    }

    public Herbivore SpawnOffspring(
        Herbivore parent)
    {
        if (parent == null)
        {
            return null;
        }

        return SpawnOffspring(
            parent,
            null
        );
    }

    public Herbivore SpawnOffspring(
        Herbivore parentA,
        Herbivore parentB)
    {
        if (
            GetCurrentPopulation() >=
            maxPopulation)
        {
            return null;
        }

        HerbivoreGenome childGenome =
            CreateChildGenome(
                parentA != null
                    ? parentA.genome
                    : null,
                parentB != null
                    ? parentB.genome
                    : null
            );

        if (
            !TryGetSpawnPosition(
                childGenome,
                out Vector3 spawnPosition))
        {
            return null;
        }

        return CreateHerbivore(
            spawnPosition,
            childGenome,
            "Herbivore_Offspring"
        );
    }

    private Herbivore CreateHerbivore(
        Vector3 spawnPosition,
        HerbivoreGenome genome,
        string objectName)
    {
        GameObject herbivoreObject =
            GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

        herbivoreObject.name =
            objectName;

        herbivoreObject.transform.position =
            spawnPosition;

        float size =
            Mathf.Max(
                0.1f,
                genome.bodySize
            );

        herbivoreObject.transform.localScale =
            Vector3.one * size;

        Renderer renderer =
            herbivoreObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color =
                GetColorForGenome(genome);
        }

        Herbivore herbivore =
            herbivoreObject.AddComponent<Herbivore>();

        ApplyGenomeToHerbivore(
            herbivore,
            genome
        );

        return herbivore;
    }

    private void ApplyGenomeToHerbivore(
        Herbivore herbivore,
        HerbivoreGenome genome)
    {
        herbivore.genome =
            genome;

        herbivore.lifespan =
            genome.lifespan;

        herbivore.maturityAge =
            genome.maturityAge;

        herbivore.reproductionCooldown =
            genome.reproductionCooldown;

        herbivore.reproductionEnergyThreshold =
            genome.reproductionEnergyThreshold;

        herbivore.reproductionEnergyCost =
            reproductionEnergyCost;

        herbivore.offspringStartingEnergy =
            offspringStartingEnergy;

        herbivore.maxPopulation =
            maxPopulation;

        herbivore.mutationChance =
            mutationChance;

        herbivore.mutationStrength =
            mutationStrength;
    }

    private HerbivoreGenome CreateStartingGenome()
    {
        HerbivoreGenome genome =
            new HerbivoreGenome();

        genome.speed =
            speed;

        genome.bodySize =
            herbivoreSize;

        genome.maxEnergy =
            maxEnergy;

        genome.startingEnergy =
            startingEnergy;

        genome.energyDrainPerSecond =
            energyDrainPerSecond;

        genome.foodPerBite =
            foodPerBite;

        genome.eatingDistance =
            eatingDistance;

        genome.lifespan =
            lifespan;

        genome.maturityAge =
            maturityAge;

        genome.reproductionCooldown =
            reproductionCooldown;

        genome.reproductionEnergyThreshold =
            reproductionEnergyThreshold;

        genome.offspringStartingEnergy =
            offspringStartingEnergy;

        genome.habitat =
            new HabitatGenome();

        float habitatRoll =
            Random.value;

        if (
            habitatRoll <
            startingAquaticChance)
        {
            genome.habitat.landAdaptation =
                0.20f;

            genome.habitat.aquaticAdaptation =
                0.95f;

            genome.habitat.flightAdaptation =
                0.05f;

            genome.habitat.coldTolerance =
                0.55f;

            genome.habitat.mountainAdaptation =
                0.20f;
        }
        else if (
            habitatRoll <
            startingAquaticChance +
            startingAmphibiousChance)
        {
            genome.habitat.landAdaptation =
                0.75f;

            genome.habitat.aquaticAdaptation =
                0.75f;

            genome.habitat.flightAdaptation =
                0.05f;

            genome.habitat.coldTolerance =
                0.50f;

            genome.habitat.mountainAdaptation =
                0.30f;
        }
        else
        {
            genome.habitat.landAdaptation =
                landAdaptation;

            genome.habitat.aquaticAdaptation =
                aquaticAdaptation;

            genome.habitat.flightAdaptation =
                flightAdaptation;

            genome.habitat.coldTolerance =
                coldTolerance;

            genome.habitat.mountainAdaptation =
                mountainAdaptation;
        }

        return genome;
    }

    private HerbivoreGenome CreateChildGenome(
        HerbivoreGenome parentGenomeA,
        HerbivoreGenome parentGenomeB)
    {
        if (
            parentGenomeA == null &&
            parentGenomeB == null)
        {
            return CreateStartingGenome();
        }

        if (parentGenomeA == null)
        {
            parentGenomeA =
                parentGenomeB;
        }

        HerbivoreGenome child =
            new HerbivoreGenome();

        if (parentGenomeB == null)
        {
            CopyGenome(
                parentGenomeA,
                child
            );
        }
        else
        {
            child.speed =
                Inherit(
                    parentGenomeA.speed,
                    parentGenomeB.speed
                );

            child.bodySize =
                Inherit(
                    parentGenomeA.bodySize,
                    parentGenomeB.bodySize
                );

            child.maxEnergy =
                Inherit(
                    parentGenomeA.maxEnergy,
                    parentGenomeB.maxEnergy
                );

            child.startingEnergy =
                Inherit(
                    parentGenomeA.startingEnergy,
                    parentGenomeB.startingEnergy
                );

            child.energyDrainPerSecond =
                Inherit(
                    parentGenomeA.energyDrainPerSecond,
                    parentGenomeB.energyDrainPerSecond
                );

            child.foodPerBite =
                Inherit(
                    parentGenomeA.foodPerBite,
                    parentGenomeB.foodPerBite
                );

            child.eatingDistance =
                Inherit(
                    parentGenomeA.eatingDistance,
                    parentGenomeB.eatingDistance
                );

            child.lifespan =
                Inherit(
                    parentGenomeA.lifespan,
                    parentGenomeB.lifespan
                );

            child.maturityAge =
                Inherit(
                    parentGenomeA.maturityAge,
                    parentGenomeB.maturityAge
                );

            child.reproductionCooldown =
                Inherit(
                    parentGenomeA.reproductionCooldown,
                    parentGenomeB.reproductionCooldown
                );

            child.reproductionEnergyThreshold =
                Inherit(
                    parentGenomeA.reproductionEnergyThreshold,
                    parentGenomeB.reproductionEnergyThreshold
                );

            child.offspringStartingEnergy =
                Inherit(
                    parentGenomeA.offspringStartingEnergy,
                    parentGenomeB.offspringStartingEnergy
                );

            child.habitat =
                new HabitatGenome();

            child.habitat.landAdaptation =
                Inherit(
                    parentGenomeA.habitat.landAdaptation,
                    parentGenomeB.habitat.landAdaptation
                );

            child.habitat.aquaticAdaptation =
                Inherit(
                    parentGenomeA.habitat.aquaticAdaptation,
                    parentGenomeB.habitat.aquaticAdaptation
                );

            child.habitat.flightAdaptation =
                Inherit(
                    parentGenomeA.habitat.flightAdaptation,
                    parentGenomeB.habitat.flightAdaptation
                );

            child.habitat.coldTolerance =
                Inherit(
                    parentGenomeA.habitat.coldTolerance,
                    parentGenomeB.habitat.coldTolerance
                );

            child.habitat.mountainAdaptation =
                Inherit(
                    parentGenomeA.habitat.mountainAdaptation,
                    parentGenomeB.habitat.mountainAdaptation
                );
        }

        MutateGenome(child);

        child.startingEnergy =
            Mathf.Clamp(
                child.startingEnergy,
                1f,
                child.maxEnergy
            );

        child.offspringStartingEnergy =
            Mathf.Clamp(
                child.offspringStartingEnergy,
                1f,
                child.maxEnergy
            );

        return child;
    }

    private void CopyGenome(
        HerbivoreGenome source,
        HerbivoreGenome target)
    {
        target.speed =
            source.speed;

        target.bodySize =
            source.bodySize;

        target.maxEnergy =
            source.maxEnergy;

        target.startingEnergy =
            source.startingEnergy;

        target.energyDrainPerSecond =
            source.energyDrainPerSecond;

        target.foodPerBite =
            source.foodPerBite;

        target.eatingDistance =
            source.eatingDistance;

        target.lifespan =
            source.lifespan;

        target.maturityAge =
            source.maturityAge;

        target.reproductionCooldown =
            source.reproductionCooldown;

        target.reproductionEnergyThreshold =
            source.reproductionEnergyThreshold;

        target.offspringStartingEnergy =
            source.offspringStartingEnergy;

        target.habitat =
            new HabitatGenome();

        target.habitat.landAdaptation =
            source.habitat.landAdaptation;

        target.habitat.aquaticAdaptation =
            source.habitat.aquaticAdaptation;

        target.habitat.flightAdaptation =
            source.habitat.flightAdaptation;

        target.habitat.coldTolerance =
            source.habitat.coldTolerance;

        target.habitat.mountainAdaptation =
            source.habitat.mountainAdaptation;
    }

    private void MutateGenome(
        HerbivoreGenome genome)
    {
        genome.speed =
            MutateValue(
                genome.speed,
                0.1f,
                6f
            );

        genome.bodySize =
            MutateValue(
                genome.bodySize,
                0.15f,
                1.5f
            );

        genome.maxEnergy =
            MutateValue(
                genome.maxEnergy,
                20f,
                500f
            );

        genome.startingEnergy =
            MutateValue(
                genome.startingEnergy,
                5f,
                genome.maxEnergy
            );

        genome.energyDrainPerSecond =
            MutateValue(
                genome.energyDrainPerSecond,
                0.05f,
                10f
            );

        genome.foodPerBite =
            MutateValue(
                genome.foodPerBite,
                0.5f,
                50f
            );

        genome.eatingDistance =
            MutateValue(
                genome.eatingDistance,
                0.2f,
                3f
            );

        genome.lifespan =
            MutateValue(
                genome.lifespan,
                10f,
                1000f
            );

        genome.maturityAge =
            MutateValue(
                genome.maturityAge,
                1f,
                genome.lifespan
            );

        genome.reproductionCooldown =
            MutateValue(
                genome.reproductionCooldown,
                1f,
                200f
            );

        genome.reproductionEnergyThreshold =
            MutateValue(
                genome.reproductionEnergyThreshold,
                10f,
                genome.maxEnergy
            );

        genome.offspringStartingEnergy =
            MutateValue(
                genome.offspringStartingEnergy,
                1f,
                genome.maxEnergy
            );

        if (genome.habitat == null)
        {
            genome.habitat =
                new HabitatGenome();
        }

        genome.habitat.landAdaptation =
            MutateValue(
                genome.habitat.landAdaptation,
                0f,
                1f
            );

        genome.habitat.aquaticAdaptation =
            MutateValue(
                genome.habitat.aquaticAdaptation,
                0f,
                1f
            );

        genome.habitat.flightAdaptation =
            MutateValue(
                genome.habitat.flightAdaptation,
                0f,
                1f
            );

        genome.habitat.coldTolerance =
            MutateValue(
                genome.habitat.coldTolerance,
                0f,
                1f
            );

        genome.habitat.mountainAdaptation =
            MutateValue(
                genome.habitat.mountainAdaptation,
                0f,
                1f
            );
    }

    private float MutateValue(
        float value,
        float minimum,
        float maximum)
    {
        if (
            Random.value <=
            mutationChance)
        {
            float scale =
                Mathf.Max(
                    1f,
                    Mathf.Abs(value)
                );

            value +=
                Random.Range(
                    -mutationStrength,
                    mutationStrength
                ) * scale;
        }

        return Mathf.Clamp(
            value,
            minimum,
            maximum
        );
    }

    private float Inherit(
        float valueA,
        float valueB)
    {
        return Random.value < 0.5f
            ? valueA
            : valueB;
    }

    private bool TryGetSpawnPosition(
        HerbivoreGenome genome,
        out Vector3 spawnPosition)
    {
        for (
            int attempt = 0;
            attempt < 500;
            attempt++)
        {
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
                worldGrid.GetTerrainType(
                    x,
                    z
                );

            if (
                CanUseTerrain(
                    genome,
                    terrainType))
            {
                spawnPosition =
                    worldGrid.GetCellWorldPosition(
                        x,
                        z,
                        genome.bodySize * 0.5f
                    );

                return true;
            }
        }

        spawnPosition =
            transform.position;

        return false;
    }

    private bool CanUseTerrain(
        HerbivoreGenome genome,
        WorldTerrainType terrainType)
    {
        if (
            genome == null ||
            genome.habitat == null)
        {
            return true;
        }

        bool aquaticTerrain =
            terrainType ==
                WorldTerrainType.Water ||
            terrainType ==
                WorldTerrainType.River ||
            terrainType ==
                WorldTerrainType.Lake ||
            terrainType ==
                WorldTerrainType.Ocean ||
            terrainType ==
                WorldTerrainType.DeepSea ||
            terrainType ==
                WorldTerrainType.KelpForest ||
            terrainType ==
                WorldTerrainType.Trench ||
            terrainType ==
                WorldTerrainType.CoralReef ||
            terrainType ==
                WorldTerrainType.Mangrove ||
            terrainType ==
                WorldTerrainType.Fjord;

        bool coldTerrain =
            terrainType ==
                WorldTerrainType.Ice ||
            terrainType ==
                WorldTerrainType.Snow ||
            terrainType ==
                WorldTerrainType.Glacier ||
            terrainType ==
                WorldTerrainType.Tundra;

        bool mountainTerrain =
            terrainType ==
                WorldTerrainType.Mountain ||
            terrainType ==
                WorldTerrainType.Cliff ||
            terrainType ==
                WorldTerrainType.Plateau ||
            terrainType ==
                WorldTerrainType.Canyon ||
            terrainType ==
                WorldTerrainType.Volcano;

        if (aquaticTerrain)
        {
            return
                genome.habitat.aquaticAdaptation
                >= 0.35f;
        }

        float adaptation =
            genome.habitat.landAdaptation;

        if (coldTerrain)
        {
            adaptation =
                Mathf.Min(
                    adaptation,
                    genome.habitat.coldTolerance
                );
        }

        if (mountainTerrain)
        {
            adaptation =
                Mathf.Min(
                    adaptation,
                    genome.habitat.mountainAdaptation
                );
        }

        return adaptation >= 0.35f;
    }

    private Color GetColorForGenome(
        HerbivoreGenome genome)
    {
        if (
            genome != null &&
            genome.habitat != null)
        {
            if (
                genome.habitat.aquaticAdaptation >
                genome.habitat.landAdaptation +
                0.20f)
            {
                return new Color(
                    0.15f,
                    0.65f,
                    1f
                );
            }

            if (
                genome.habitat.aquaticAdaptation >=
                0.60f &&
                genome.habitat.landAdaptation >=
                0.60f)
            {
                return new Color(
                    0.65f,
                    0.85f,
                    0.25f
                );
            }
        }

        return herbivoreColor;
    }

    private int GetCurrentPopulation()
    {
        return FindObjectsByType<Herbivore>().Length;
    }
}