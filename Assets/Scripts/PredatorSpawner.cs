using UnityEngine;

public class PredatorSpawner : MonoBehaviour
{
    [Header("References")]
    public WorldGrid worldGrid;

    [Header("Population")]
    [Min(0)]
    public int startingPredators = 2;

    [Min(1)]
    public int maxPopulation = 30;

    public int seed = 98765;

    [Header("Appearance")]
    [Min(0.1f)]
    public float predatorSize = 0.45f;

    public Color predatorColor =
        new Color(0.8f, 0.08f, 0.05f);

    [Header("Movement")]
    [Min(0.01f)]
    public float speed = 2.5f;

    [Min(0.05f)]
    public float searchInterval = 0.5f;

    [Min(0.5f)]
    public float roamRadius = 6f;

    [Min(0.1f)]
    public float roamDistance = 4f;

    [Min(0f)]
    public float flyingHeight = 2.5f;

    [Header("Hunting")]
    [Min(0.1f)]
    public float eatingDistance = 0.8f;

    [Min(0.1f)]
    public float energyFromPrey = 35f;

    [Min(0.5f)]
    public float sightRadius = 12f;

    [Min(0.25f)]
    public float blockedTargetTimeout = 1.5f;

    [Min(0.05f)]
    public float stuckRecoveryInterval = 0.25f;

    [Min(0f)]
    public float blockedTargetRetryDelay = 4f;

    [Header("Energy")]
    [Min(1f)]
    public float maxEnergy = 100f;

    [Min(0f)]
    public float startingEnergy = 75f;

    [Min(0f)]
    public float energyDrainPerSecond = 1f;

    [Header("Life Span")]
    [Min(1f)]
    public float lifespan = 240f;

    [Header("Reproduction")]
    [Min(0f)]
    public float maturityAge = 30f;

    [Min(0f)]
    public float reproductionCooldown = 25f;

    [Min(0f)]
    public float reproductionEnergyThreshold = 70f;

    [Min(0f)]
    public float reproductionEnergyCost = 35f;

    [Min(0.1f)]
    public float offspringStartingEnergy = 45f;

    [Min(0.5f)]
    public float mateSearchRadius = 3f;

    [Header("Mutation")]
    [Range(0f, 1f)]
    public float mutationChance = 0.08f;

    [Range(0f, 1f)]
    public float mutationStrength = 0.15f;

    [Header("Starting Habitat Variety")]
    [Range(0f, 1f)]
    public float startingAquaticChance = 0.15f;

    [Range(0f, 1f)]
    public float startingAmphibiousChance = 0.20f;

    [Range(0f, 1f)]
    public float startingFlyingChance = 0.05f;

    [Header("Default Habitat")]
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
                "PredatorSpawner could not find a WorldGrid."
            );

            return;
        }

        Random.InitState(seed);

        SpawnStartingPredators();
    }

    public void SpawnStartingPredators()
    {
        for (
            int i = 0;
            i < startingPredators;
            i++)
        {
            if (
                GetCurrentPopulation() >=
                maxPopulation)
            {
                return;
            }

            HabitatGenome habitat =
                CreateStartingHabitat();

            if (
                !TryGetCompatibleSpawnPosition(
                    habitat,
                    predatorSize,
                    transform.position,
                    out Vector3 spawnPosition))
            {
                Debug.LogWarning(
                    "Could not find a suitable habitat for a predator."
                );

                continue;
            }

            CreatePredator(
                spawnPosition,
                habitat,
                predatorSize,
                startingEnergy,
                $"Predator_{i}"
            );
        }
    }

    public Predator SpawnOffspring(
        Vector3 preferredPosition,
        Predator parentA,
        Predator parentB)
    {
        if (
            parentA == null ||
            parentB == null)
        {
            return null;
        }

        if (
            GetCurrentPopulation() >=
            maxPopulation)
        {
            return null;
        }

        HabitatGenome childHabitat =
            CreateChildHabitat(
                parentA.habitat,
                parentB.habitat
            );

        float childSize =
            MutateValue(
                Inherit(
                    parentA.bodySize,
                    parentB.bodySize
                ),
                0.15f,
                1.5f
            );

        float childSightRadius =
            MutateValue(
                Inherit(
                    parentA.sightRadius,
                    parentB.sightRadius
                ),
                2f,
                40f
            );

        float childSpeed =
            MutateValue(
                Inherit(
                    parentA.speed,
                    parentB.speed
                ),
                0.1f,
                8f
            );

        float childMaxEnergy =
            MutateValue(
                Inherit(
                    parentA.maxEnergy,
                    parentB.maxEnergy
                ),
                20f,
                500f
            );

        float childEnergyDrain =
            MutateValue(
                Inherit(
                    parentA.energyDrainPerSecond,
                    parentB.energyDrainPerSecond
                ),
                0.05f,
                10f
            );

        float childEnergyFromPrey =
            MutateValue(
                Inherit(
                    parentA.energyFromPrey,
                    parentB.energyFromPrey
                ),
                1f,
                200f
            );

        float childEatingDistance =
            MutateValue(
                Inherit(
                    parentA.eatingDistance,
                    parentB.eatingDistance
                ),
                0.2f,
                4f
            );

        float childLifespan =
            MutateValue(
                Inherit(
                    parentA.lifespan,
                    parentB.lifespan
                ),
                10f,
                1500f
            );

        float childMaturityAge =
            MutateValue(
                Inherit(
                    parentA.maturityAge,
                    parentB.maturityAge
                ),
                1f,
                childLifespan
            );

        float childCooldown =
            MutateValue(
                Inherit(
                    parentA.reproductionCooldown,
                    parentB.reproductionCooldown
                ),
                1f,
                300f
            );

        float childEnergyThreshold =
            MutateValue(
                Inherit(
                    parentA.reproductionEnergyThreshold,
                    parentB.reproductionEnergyThreshold
                ),
                5f,
                childMaxEnergy
            );

        float childEnergyCost =
            MutateValue(
                Inherit(
                    parentA.reproductionEnergyCost,
                    parentB.reproductionEnergyCost
                ),
                1f,
                childMaxEnergy
            );

        float childOffspringEnergy =
            MutateValue(
                Inherit(
                    parentA.offspringStartingEnergy,
                    parentB.offspringStartingEnergy
                ),
                1f,
                childMaxEnergy
            );

        float childMateRadius =
            MutateValue(
                Inherit(
                    parentA.mateSearchRadius,
                    parentB.mateSearchRadius
                ),
                0.5f,
                15f
            );

        float childMutationChance =
            MutateValue(
                Inherit(
                    parentA.mutationChance,
                    parentB.mutationChance
                ),
                0f,
                1f
            );

        float childMutationStrength =
            MutateValue(
                Inherit(
                    parentA.mutationStrength,
                    parentB.mutationStrength
                ),
                0f,
                1f
            );

        float childStartingEnergy =
            Mathf.Clamp(
                childOffspringEnergy,
                1f,
                childMaxEnergy
            );

        if (
            !TryGetCompatibleSpawnPosition(
                childHabitat,
                childSize,
                preferredPosition,
                out Vector3 spawnPosition))
        {
            return null;
        }

        Predator child =
            CreatePredator(
                spawnPosition,
                childHabitat,
                childSize,
                childStartingEnergy,
                "Predator_Offspring"
            );

        child.sightRadius =
            childSightRadius;

        child.speed =
            childSpeed;

        child.maxEnergy =
            childMaxEnergy;

        child.startingEnergy =
            childStartingEnergy;

        child.energyDrainPerSecond =
            childEnergyDrain;

        child.energyFromPrey =
            childEnergyFromPrey;

        child.eatingDistance =
            childEatingDistance;

        child.lifespan =
            childLifespan;

        child.maturityAge =
            childMaturityAge;

        child.reproductionCooldown =
            childCooldown;

        child.reproductionEnergyThreshold =
            childEnergyThreshold;

        child.reproductionEnergyCost =
            childEnergyCost;

        child.offspringStartingEnergy =
            childOffspringEnergy;

        child.mateSearchRadius =
            childMateRadius;

        child.mutationChance =
            childMutationChance;

        child.mutationStrength =
            childMutationStrength;

        child.transform.localScale =
            Vector3.one * childSize;

        child.surfaceOffset =
            childSize;

        return child;
    }

    private Predator CreatePredator(
        Vector3 spawnPosition,
        HabitatGenome habitat,
        float size,
        float initialEnergy,
        string objectName)
    {
        GameObject predatorObject =
            GameObject.CreatePrimitive(
                PrimitiveType.Capsule
            );

        predatorObject.name =
            objectName;

        predatorObject.transform.position =
            spawnPosition;

        predatorObject.transform.localScale =
            Vector3.one * size;

        Renderer renderer =
            predatorObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color =
                GetColorForHabitat(habitat);
        }

        Predator predator =
            predatorObject.AddComponent<Predator>();

        predator.worldGrid =
            worldGrid;

        predator.spawner =
            this;

        predator.bodySize =
            size;

        predator.surfaceOffset =
            size;

        predator.speed =
            speed;

        predator.searchInterval =
            searchInterval;

        predator.roamRadius =
            roamRadius;

        predator.roamDistance =
            roamDistance;

        predator.flyingHeight =
            flyingHeight;

        predator.eatingDistance =
            eatingDistance;

        predator.energyFromPrey =
            energyFromPrey;

        predator.sightRadius =
            sightRadius;

        predator.blockedTargetTimeout =
            blockedTargetTimeout;

        predator.stuckRecoveryInterval =
            stuckRecoveryInterval;

        predator.blockedTargetRetryDelay =
            blockedTargetRetryDelay;

        predator.maxEnergy =
            maxEnergy;

        predator.startingEnergy =
            initialEnergy;

        predator.energyDrainPerSecond =
            energyDrainPerSecond;

        predator.lifespan =
            lifespan;

        predator.maturityAge =
            maturityAge;

        predator.reproductionCooldown =
            reproductionCooldown;

        predator.reproductionEnergyThreshold =
            reproductionEnergyThreshold;

        predator.reproductionEnergyCost =
            reproductionEnergyCost;

        predator.offspringStartingEnergy =
            offspringStartingEnergy;

        predator.mateSearchRadius =
            mateSearchRadius;

        predator.maxPopulation =
            maxPopulation;

        predator.mutationChance =
            mutationChance;

        predator.mutationStrength =
            mutationStrength;

        predator.habitat =
            CloneHabitat(habitat);

        return predator;
    }

    private HabitatGenome CreateStartingHabitat()
    {
        HabitatGenome habitat =
            new HabitatGenome();

        float roll =
            Random.value;

        if (
            roll <
            startingFlyingChance)
        {
            habitat.landAdaptation =
                0.75f;

            habitat.aquaticAdaptation =
                0.75f;

            habitat.flightAdaptation =
                0.95f;

            habitat.coldTolerance =
                0.60f;

            habitat.mountainAdaptation =
                0.60f;
        }
        else if (
            roll <
            startingFlyingChance +
            startingAquaticChance)
        {
            habitat.landAdaptation =
                0.20f;

            habitat.aquaticAdaptation =
                0.95f;

            habitat.flightAdaptation =
                0.05f;

            habitat.coldTolerance =
                0.55f;

            habitat.mountainAdaptation =
                0.20f;
        }
        else if (
            roll <
            startingFlyingChance +
            startingAquaticChance +
            startingAmphibiousChance)
        {
            habitat.landAdaptation =
                0.75f;

            habitat.aquaticAdaptation =
                0.75f;

            habitat.flightAdaptation =
                0.05f;

            habitat.coldTolerance =
                0.50f;

            habitat.mountainAdaptation =
                0.30f;
        }
        else
        {
            habitat.landAdaptation =
                landAdaptation;

            habitat.aquaticAdaptation =
                aquaticAdaptation;

            habitat.flightAdaptation =
                flightAdaptation;

            habitat.coldTolerance =
                coldTolerance;

            habitat.mountainAdaptation =
                mountainAdaptation;
        }

        return habitat;
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
                1f;

            defaultHabitat.aquaticAdaptation =
                0f;

            defaultHabitat.flightAdaptation =
                0f;

            defaultHabitat.coldTolerance =
                0.5f;

            defaultHabitat.mountainAdaptation =
                0.5f;

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

    private bool TryGetCompatibleSpawnPosition(
        HabitatGenome habitat,
        float size,
        Vector3 preferredPosition,
        out Vector3 spawnPosition)
    {
        if (
            worldGrid.TryGetCellCoordinates(
                preferredPosition,
                out int preferredX,
                out int preferredZ))
        {
            WorldTerrainType preferredTerrain =
                worldGrid.GetTerrainType(
                    preferredX,
                    preferredZ
                );

            if (
                CanHabitatUseTerrain(
                    habitat,
                    preferredTerrain))
            {
                spawnPosition =
                    worldGrid.GetCellWorldPosition(
                        preferredX,
                        preferredZ,
                        GetVerticalOffset(
                            habitat,
                            size
                        )
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
                !CanHabitatUseTerrain(
                    habitat,
                    terrainType))
            {
                continue;
            }

            spawnPosition =
                worldGrid.GetCellWorldPosition(
                    x,
                    z,
                    GetVerticalOffset(
                        habitat,
                        size
                    )
                );

            return true;
        }

        spawnPosition =
            transform.position;

        return false;
    }

    private bool CanHabitatUseTerrain(
        HabitatGenome habitat,
        WorldTerrainType terrainType)
    {
        if (habitat == null)
        {
            return true;
        }

        bool flying =
            IsFlying(habitat);

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
                WorldTerrainType.Fjord ||
            terrainType ==
                WorldTerrainType.Mangrove ||
            terrainType ==
                WorldTerrainType.Marsh ||
            terrainType ==
                WorldTerrainType.Wetland ||
            terrainType ==
                WorldTerrainType.Swamp;

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
                habitat.aquaticAdaptation >=
                0.35f ||
                flying;
        }

        float adaptation =
            habitat.landAdaptation;

        if (coldTerrain)
        {
            adaptation =
                Mathf.Min(
                    adaptation,
                    habitat.coldTolerance
                );
        }

        if (mountainTerrain)
        {
            adaptation =
                Mathf.Min(
                    adaptation,
                    habitat.mountainAdaptation
                );
        }

        return
            adaptation >= 0.35f ||
            flying;
    }

    private bool IsFlying(
        HabitatGenome habitat)
    {
        if (habitat == null)
        {
            return false;
        }

        return
            habitat.flightAdaptation >= 0.80f &&
            habitat.flightAdaptation >
            habitat.landAdaptation &&
            habitat.flightAdaptation >
            habitat.aquaticAdaptation;
    }

    private float GetVerticalOffset(
        HabitatGenome habitat,
        float size)
    {
        float offset =
            size;

        if (IsFlying(habitat))
        {
            offset +=
                flyingHeight;
        }

        return offset;
    }

    private Color GetColorForHabitat(
        HabitatGenome habitat)
    {
        if (habitat == null)
        {
            return predatorColor;
        }

        if (
            habitat.flightAdaptation >=
            0.80f)
        {
            return new Color(
                0.75f,
                0.30f,
                1f
            );
        }

        if (
            habitat.aquaticAdaptation >
            habitat.landAdaptation +
            0.20f)
        {
            return new Color(
                0.10f,
                0.45f,
                1f
            );
        }

        if (
            habitat.aquaticAdaptation >=
            0.60f &&
            habitat.landAdaptation >=
            0.60f)
        {
            return new Color(
                0.65f,
                0.85f,
                0.20f
            );
        }

        return predatorColor;
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
        return FindObjectsByType<Predator>().Length;
    }
}