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

    [Header("Predator Avoidance")]
    [Min(0.5f)]
    public float threatDetectionRadius = 6f;

    [Min(1f)]
    public float fleeSpeedMultiplier = 1.6f;

    [Min(0.1f)]
    public float fleeDuration = 1.5f;

    [Min(0.05f)]
    public float threatSearchInterval = 0.25f;

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

        herbivore.threatDetectionRadius =
            threatDetectionRadius;

        herbivore.fleeSpeedMultiplier =
            fleeSpeedMultiplier;

        herbivore.fleeDuration =
            fleeDuration;

        herbivore.threatSearchInterval =
            threatSearchInterval;
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
        HerbivoreGenome parentA,
        HerbivoreGenome parentB)
    {
        HerbivoreGenome child =
            new HerbivoreGenome();

        child.speed =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.speed
                        : speed,
                    parentB != null
                        ? parentB.speed
                        : speed
                ),
                0.1f,
                8f
            );

        child.bodySize =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.bodySize
                        : herbivoreSize,
                    parentB != null
                        ? parentB.bodySize
                        : herbivoreSize
                ),
                0.1f,
                2f
            );

        child.maxEnergy =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.maxEnergy
                        : maxEnergy,
                    parentB != null
                        ? parentB.maxEnergy
                        : maxEnergy
                ),
                10f,
                500f
            );

        child.startingEnergy =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.startingEnergy
                        : offspringStartingEnergy,
                    parentB != null
                        ? parentB.startingEnergy
                        : offspringStartingEnergy
                ),
                1f,
                child.maxEnergy
            );

        child.energyDrainPerSecond =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.energyDrainPerSecond
                        : energyDrainPerSecond,
                    parentB != null
                        ? parentB.energyDrainPerSecond
                        : energyDrainPerSecond
                ),
                0.05f,
                10f
            );

        child.foodPerBite =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.foodPerBite
                        : foodPerBite,
                    parentB != null
                        ? parentB.foodPerBite
                        : foodPerBite
                ),
                0.1f,
                50f
            );

        child.eatingDistance =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.eatingDistance
                        : eatingDistance,
                    parentB != null
                        ? parentB.eatingDistance
                        : eatingDistance
                ),
                0.1f,
                4f
            );

        child.lifespan =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.lifespan
                        : lifespan,
                    parentB != null
                        ? parentB.lifespan
                        : lifespan
                ),
                10f,
                1500f
            );

        child.maturityAge =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.maturityAge
                        : maturityAge,
                    parentB != null
                        ? parentB.maturityAge
                        : maturityAge
                ),
                1f,
                child.lifespan
            );

        child.reproductionCooldown =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.reproductionCooldown
                        : reproductionCooldown,
                    parentB != null
                        ? parentB.reproductionCooldown
                        : reproductionCooldown
                ),
                1f,
                300f
            );

        child.reproductionEnergyThreshold =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.reproductionEnergyThreshold
                        : reproductionEnergyThreshold,
                    parentB != null
                        ? parentB.reproductionEnergyThreshold
                        : reproductionEnergyThreshold
                ),
                1f,
                child.maxEnergy
            );

        child.offspringStartingEnergy =
            MutateValue(
                Inherit(
                    parentA != null
                        ? parentA.offspringStartingEnergy
                        : offspringStartingEnergy,
                    parentB != null
                        ? parentB.offspringStartingEnergy
                        : offspringStartingEnergy
                ),
                1f,
                child.maxEnergy
            );

        child.habitat =
            CreateChildHabitat(
                parentA != null
                    ? parentA.habitat
                    : null,
                parentB != null
                    ? parentB.habitat
                    : null
            );

        return child;
    }

    private HabitatGenome CreateChildHabitat(
        HabitatGenome parentA,
        HabitatGenome parentB)
    {
        HabitatGenome first =
            CloneHabitat(parentA);

        HabitatGenome second =
            CloneHabitat(parentB);

        HabitatGenome child =
            new HabitatGenome();

        child.landAdaptation =
            MutateTrait(
                Inherit(
                    first.landAdaptation,
                    second.landAdaptation
                )
            );

        child.aquaticAdaptation =
            MutateTrait(
                Inherit(
                    first.aquaticAdaptation,
                    second.aquaticAdaptation
                )
            );

        child.flightAdaptation =
            MutateTrait(
                Inherit(
                    first.flightAdaptation,
                    second.flightAdaptation
                )
            );

        child.coldTolerance =
            MutateTrait(
                Inherit(
                    first.coldTolerance,
                    second.coldTolerance
                )
            );

        child.mountainAdaptation =
            MutateTrait(
                Inherit(
                    first.mountainAdaptation,
                    second.mountainAdaptation
                )
            );

        return child;
    }

    private HabitatGenome CloneHabitat(
        HabitatGenome source)
    {
        if (source == null)
        {
            HabitatGenome defaultHabitat =
                new HabitatGenome();

            defaultHabitat.landAdaptation =
                landAdaptation;

            defaultHabitat.aquaticAdaptation =
                aquaticAdaptation;

            defaultHabitat.flightAdaptation =
                flightAdaptation;

            defaultHabitat.coldTolerance =
                coldTolerance;

            defaultHabitat.mountainAdaptation =
                mountainAdaptation;

            return defaultHabitat;
        }

        HabitatGenome clone =
            new HabitatGenome();

        clone.landAdaptation =
            source.landAdaptation;

        clone.aquaticAdaptation =
            source.aquaticAdaptation;

        clone.flightAdaptation =
            source.flightAdaptation;

        clone.coldTolerance =
            source.coldTolerance;

        clone.mountainAdaptation =
            source.mountainAdaptation;

        return clone;
    }

    private bool TryGetSpawnPosition(
        HerbivoreGenome genome,
        out Vector3 spawnPosition)
    {
        if (
            worldGrid.TryGetCellCoordinates(
                transform.position,
                out int preferredX,
                out int preferredZ))
        {
            WorldTerrainType preferredTerrain =
                worldGrid.GetTerrainType(
                    preferredX,
                    preferredZ
                );

            if (
                genome.habitat.CanOccupy(
                    preferredTerrain))
            {
                spawnPosition =
                    worldGrid.GetCellWorldPosition(
                        preferredX,
                        preferredZ,
                        genome.bodySize
                    );

                return true;
            }
        }

        for (
            int attempt = 0;
            attempt < 200;
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
                !genome.habitat.CanOccupy(
                    terrainType))
            {
                continue;
            }

            spawnPosition =
                worldGrid.GetCellWorldPosition(
                    x,
                    z,
                    genome.bodySize
                );

            return true;
        }

        spawnPosition =
            transform.position;

        return false;
    }

    private Color GetColorForGenome(
        HerbivoreGenome genome)
    {
        if (genome.habitat == null)
        {
            return herbivoreColor;
        }

        CreatureLifestyle lifestyle =
            genome.habitat.GetLifestyle();

        switch (lifestyle)
        {
            case CreatureLifestyle.Aquatic:
                return new Color(
                    0.1f,
                    0.55f,
                    1f
                );

            case CreatureLifestyle.Amphibious:
                return new Color(
                    0.65f,
                    0.9f,
                    0.2f
                );

            case CreatureLifestyle.Flying:
                return new Color(
                    0.8f,
                    0.35f,
                    1f
                );

            default:
                return herbivoreColor;
        }
    }

    private float MutateTrait(
        float value)
    {
        return MutateValue(
            value,
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

    private int GetCurrentPopulation()
    {
        return FindObjectsByType<Herbivore>().Length;
    }
}