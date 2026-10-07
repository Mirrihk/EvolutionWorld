using UnityEngine;

public class Herbivore : MonoBehaviour
{
    [Header("Genetics")]
    public HerbivoreGenome genome =
        new HerbivoreGenome();

    [Header("Mutation")]
    [Range(0f, 1f)]
    public float mutationChance = 0.1f;

    [Range(0f, 1f)]
    public float mutationStrength = 0.15f;

    [Header("Compatibility Settings")]
    [Min(1f)]
    public float lifespan = 120f;

    [Min(1f)]
    public float maturityAge = 10f;

    [Min(0.1f)]
    public float reproductionCooldown = 8f;

    [Min(0.1f)]
    public float reproductionEnergyThreshold = 14f;

    [Min(0.1f)]
    public float reproductionEnergyCost = 6f;

    [Min(0.1f)]
    public float offspringStartingEnergy = 10f;

    [Min(1)]
    public int maxPopulation = 50;

    [Header("Predator Avoidance")]
    [Min(0.5f)]
    public float threatDetectionRadius = 6f;

    [Min(1f)]
    public float fleeSpeedMultiplier = 1.6f;

    [Min(0.1f)]
    public float fleeDuration = 1.5f;

    [Min(0.05f)]
    public float threatSearchInterval = 0.25f;

    public float Energy { get; private set; }
    public float Age { get; private set; }

    public CreatureLifestyle Lifestyle
    {
        get
        {
            return genome.habitat.GetLifestyle();
        }
    }

    private WorldGrid worldGrid;
    private Plant targetPlant;

    private static readonly float[] fleeTurnAngles =
    {
        0f,
        45f,
        -45f,
        90f,
        -90f,
        135f,
        -135f,
        180f
    };

    private Predator currentThreat;
    private Vector3 lastThreatPosition;

    private float searchTimer;
    private float reproductionTimer;
    private float threatSearchTimer;
    private float fleeTimer;

    private Vector3 lastValidPosition;

    private void Start()
    {
        if (genome == null)
        {
            genome =
                new HerbivoreGenome();
        }

        if (genome.habitat == null)
        {
            genome.habitat =
                new HabitatGenome();
        }

        lifespan =
            genome.lifespan;

        maturityAge =
            genome.maturityAge;

        reproductionCooldown =
            genome.reproductionCooldown;

        reproductionEnergyThreshold =
            genome.reproductionEnergyThreshold;

        reproductionEnergyCost =
            genome.reproductionEnergyCost;

        offspringStartingEnergy =
            genome.offspringStartingEnergy;

        worldGrid =
            FindAnyObjectByType<WorldGrid>();

        Energy =
            Mathf.Clamp(
                genome.startingEnergy,
                0f,
                genome.maxEnergy
            );

        Age = 0f;
        searchTimer = 0f;
        reproductionTimer = 0f;
        threatSearchTimer = 0f;
        fleeTimer = 0f;

        threatDetectionRadius =
            Mathf.Max(
                0.5f,
                threatDetectionRadius
            );

        fleeSpeedMultiplier =
            Mathf.Max(
                1f,
                fleeSpeedMultiplier
            );

        fleeDuration =
            Mathf.Max(
                0.1f,
                fleeDuration
            );

        threatSearchInterval =
            Mathf.Max(
                0.05f,
                threatSearchInterval
            );

        lastValidPosition =
            transform.position;

        SnapToTerrain();
    }

    private void Update()
    {
        Age += Time.deltaTime;

        reproductionTimer =
            Mathf.Max(
                0f,
                reproductionTimer -
                Time.deltaTime
            );

        Energy -=
            genome.energyDrainPerSecond *
            Time.deltaTime;

        if (Age >= lifespan ||
            Energy <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (!SnapToTerrain())
        {
            transform.position =
                lastValidPosition;

            return;
        }

        UpdateThreatDetection(
            Time.deltaTime
        );

        if (fleeTimer > 0f)
        {
            MoveAwayFromThreat();
            TryReproduce();
            return;
        }

        searchTimer -=
            Time.deltaTime;

        if (targetPlant == null ||
            searchTimer <= 0f)
        {
            searchTimer =
                0.25f;

            FindClosestEdiblePlant();
        }

        if (targetPlant != null)
        {
            MoveTowardPlant();

            if (IsCloseEnoughToEat())
            {
                EatTargetPlant();
            }
        }

        TryReproduce();
    }

    private void UpdateThreatDetection(
        float deltaTime)
    {
        threatSearchTimer -= deltaTime;

        if (threatSearchTimer <= 0f)
        {
            threatSearchTimer =
                Mathf.Max(
                    0.05f,
                    threatSearchInterval
                );

            Predator nearestPredator =
                FindClosestPredator();

            if (nearestPredator != null)
            {
                currentThreat =
                    nearestPredator;

                lastThreatPosition =
                    nearestPredator.transform.position;

                fleeTimer =
                    fleeDuration;
            }
            else
            {
                currentThreat = null;
            }
        }

        if (currentThreat != null)
        {
            lastThreatPosition =
                currentThreat.transform.position;
        }
        else
        {
            fleeTimer =
                Mathf.Max(
                    0f,
                    fleeTimer - deltaTime
                );
        }

        if (fleeTimer <= 0f)
        {
            currentThreat = null;
        }
    }

    private Predator FindClosestPredator()
    {
        Predator[] predators =
            FindObjectsByType<Predator>();

        Predator closestPredator =
            null;

        float closestDistanceSquared =
            threatDetectionRadius *
            threatDetectionRadius;

        for (
            int i = 0;
            i < predators.Length;
            i++)
        {
            Predator predator =
                predators[i];

            if (predator == null)
            {
                continue;
            }

            Vector3 difference =
                predator.transform.position -
                transform.position;

            difference.y = 0f;

            float distanceSquared =
                difference.sqrMagnitude;

            if (
                distanceSquared <=
                closestDistanceSquared)
            {
                closestDistanceSquared =
                    distanceSquared;

                closestPredator =
                    predator;
            }
        }

        return closestPredator;
    }

    private void MoveAwayFromThreat()
    {
        Vector3 awayDirection =
            transform.position -
            lastThreatPosition;

        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude <= 0.001f)
        {
            Vector2 randomDirection =
                Random.insideUnitCircle.normalized;

            awayDirection =
                new Vector3(
                    randomDirection.x,
                    0f,
                    randomDirection.y
                );

            if (awayDirection.sqrMagnitude <= 0.001f)
            {
                awayDirection =
                    Vector3.forward;
            }
        }

        awayDirection.Normalize();

        float moveDistance =
            Mathf.Max(
                0.01f,
                genome.speed *
                fleeSpeedMultiplier *
                Time.deltaTime
            );

        for (
            int i = 0;
            i < fleeTurnAngles.Length;
            i++)
        {
            Vector3 direction =
                Quaternion.AngleAxis(
                    fleeTurnAngles[i],
                    Vector3.up
                ) * awayDirection;

            Vector3 nextPosition =
                transform.position +
                direction *
                moveDistance;

            if (!CanOccupyPosition(nextPosition))
            {
                continue;
            }

            transform.position =
                nextPosition;

            transform.forward =
                direction.normalized;

            SnapToTerrain();
            return;
        }
    }

    private void FindClosestEdiblePlant()
    {
        Plant[] plants =
            FindObjectsByType<Plant>();

        float closestDistance =
            Mathf.Infinity;

        Plant closestPlant =
            null;

        foreach (Plant plant in plants)
        {
            if (plant == null)
            {
                continue;
            }

            if (!plant.CanBeEatenBy(
                    genome.habitat))
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    plant.transform.position
                );

            if (distance < closestDistance)
            {
                closestDistance =
                    distance;

                closestPlant =
                    plant;
            }
        }

        targetPlant =
            closestPlant;
    }

    private void MoveTowardPlant()
    {
        if (targetPlant == null)
        {
            return;
        }

        Vector3 targetPosition =
            targetPlant.transform.position;

        targetPosition.y =
            transform.position.y;

        Vector3 nextPosition =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                genome.speed *
                Time.deltaTime
            );

        if (!CanOccupyPosition(nextPosition))
        {
            return;
        }

        Vector3 direction =
            targetPosition -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward =
                direction.normalized;
        }

        transform.position =
            nextPosition;

        SnapToTerrain();
    }

    private bool IsCloseEnoughToEat()
    {
        if (targetPlant == null)
        {
            return false;
        }

        Vector3 difference =
            targetPlant.transform.position -
            transform.position;

        difference.y = 0f;

        return difference.sqrMagnitude <=
               genome.eatingDistance *
               genome.eatingDistance;
    }

    private void EatTargetPlant()
    {
        if (targetPlant == null)
        {
            return;
        }

        if (!targetPlant.CanBeEatenBy(
                genome.habitat))
        {
            targetPlant = null;
            return;
        }

        float foodEaten =
            targetPlant.Eat(
                genome.foodPerBite
            );

        Energy =
            Mathf.Min(
                genome.maxEnergy,
                Energy + foodEaten
            );

        targetPlant = null;
    }

    private void TryReproduce()
    {
        if (Age < maturityAge ||
            Energy < reproductionEnergyThreshold ||
            reproductionTimer > 0f)
        {
            return;
        }

        Herbivore[] herbivores =
            FindObjectsByType<Herbivore>();

        foreach (Herbivore partner in herbivores)
        {
            if (partner == null ||
                partner == this)
            {
                continue;
            }

            if (partner.Age < partner.maturityAge ||
                partner.Energy <
                partner.reproductionEnergyThreshold)
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    partner.transform.position
                );

            if (distance > 1.5f)
            {
                continue;
            }

            if (!CanShareTerrain(
                    partner.transform.position))
            {
                continue;
            }

            HerbivoreSpawner spawner =
                FindAnyObjectByType<
                    HerbivoreSpawner
                >();

            if (spawner == null)
            {
                return;
            }

            Vector3 offspringPosition =
                Vector3.Lerp(
                    transform.position,
                    partner.transform.position,
                    0.5f
                );

            Herbivore offspring =
                spawner.SpawnOffspring(
                    offspringPosition,
                    genome,
                    partner.genome
                );

            if (offspring == null)
            {
                return;
            }

            TrySpendEnergy(
                reproductionEnergyCost
            );

            partner.TrySpendEnergy(
                partner.reproductionEnergyCost
            );

            reproductionTimer =
                reproductionCooldown;

            partner.SetReproductionCooldown(
                partner.reproductionCooldown
            );

            return;
        }
    }

    public bool CanShareTerrain(
        Vector3 position)
    {
        if (worldGrid == null)
        {
            return true;
        }

        if (!worldGrid.TryGetCellCoordinates(
                position,
                out int x,
                out int z))
        {
            return false;
        }

        return genome.habitat.CanOccupy(
            worldGrid.GetTerrainType(x, z)
        );
    }

    public bool TrySpendEnergy(
        float amount)
    {
        if (Energy < amount)
        {
            return false;
        }

        Energy -= amount;
        return true;
    }

    public void SetReproductionCooldown(
        float cooldown)
    {
        reproductionTimer =
            Mathf.Max(
                reproductionTimer,
                cooldown
            );
    }

    private bool CanOccupyPosition(
        Vector3 position)
    {
        if (worldGrid == null)
        {
            return true;
        }

        if (!worldGrid.TryGetCellCoordinates(
                position,
                out int x,
                out int z))
        {
            return false;
        }

        return genome.habitat.CanOccupy(
            worldGrid.GetTerrainType(x, z)
        );
    }

    private bool SnapToTerrain()
    {
        if (worldGrid == null)
        {
            return true;
        }

        if (!worldGrid.TryGetCellCoordinates(
                transform.position,
                out int x,
                out int z))
        {
            return false;
        }

        WorldTerrainType terrainType =
            worldGrid.GetTerrainType(x, z);

        if (!genome.habitat.CanOccupy(
                terrainType))
        {
            return false;
        }

        Vector3 surfacePosition =
            worldGrid.GetCellWorldPosition(
                x,
                z,
                genome.bodySize
            );

        Vector3 correctedPosition =
            transform.position;

        correctedPosition.y =
            surfacePosition.y;

        transform.position =
            correctedPosition;

        lastValidPosition =
            correctedPosition;

        return true;
    }
}