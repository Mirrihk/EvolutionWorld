using UnityEngine;

public class Herbivore : MonoBehaviour
{
    [Header("Genome")]
    public HerbivoreGenome genome =
        new HerbivoreGenome();

    [Header("Mutation")]
    [Range(0f, 1f)]
    public float mutationChance = 0.35f;

    [Range(0f, 1f)]
    public float mutationStrength = 0.15f;

    [Header("Reproduction")]
    [Min(0.1f)]
    public float reproductionEnergyCost = 6f;

    [Min(0.1f)]
    public float offspringStartingEnergy = 8f;

    [Min(1)]
    public int maxPopulation = 20;

    public float Energy { get; private set; }
    public float Age { get; private set; }

    private Plant targetPlant;

    private float searchTimer;
    private float reproductionTimer;

    private bool initialized;

    private void Start()
    {
        if (!initialized)
        {
            ApplyGenome();

            Energy = Mathf.Min(
                genome.startingEnergy,
                genome.maxEnergy
            );

            Age = 0f;
            initialized = true;
        }

        searchTimer = 0f;
    }

    private void Update()
    {
        Age += Time.deltaTime;

        reproductionTimer -=
            Time.deltaTime;

        Energy -=
            genome.energyDrainPerSecond *
            Time.deltaTime;

        if (Energy <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (Age >= genome.lifespan)
        {
            Destroy(gameObject);
            return;
        }

        TryReproduce();

        if (targetPlant == null)
        {
            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0f)
            {
                FindClosestPlant();
                searchTimer = 0.25f;
            }

            return;
        }

        MoveTowardPlant();

        if (IsCloseEnoughToEat())
        {
            EatTargetPlant();
        }
    }

    private void ApplyGenome()
    {
        transform.localScale =
            Vector3.one * genome.bodySize;
    }

    private void FindClosestPlant()
    {
        Plant[] plants =
            FindObjectsByType<Plant>();

        float closestDistance =
            float.MaxValue;

        Plant closestPlant = null;

        foreach (Plant plant in plants)
        {
            if (plant == null)
            {
                continue;
            }

            float distance =
                (
                    plant.transform.position -
                    transform.position
                ).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlant = plant;
            }
        }

        targetPlant = closestPlant;
    }

    private void MoveTowardPlant()
    {
        Vector3 targetPosition =
            targetPlant.transform.position;

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                genome.speed *
                Time.deltaTime
            );

        Vector3 lookDirection =
            targetPosition -
            transform.position;

        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    lookDirection
                );
        }
    }

    private bool IsCloseEnoughToEat()
    {
        float distance =
            Vector3.Distance(
                transform.position,
                targetPlant.transform.position
            );

        return distance <=
            genome.eatingDistance;
    }

    private void EatTargetPlant()
    {
        float foodEaten =
            targetPlant.Eat(
                genome.foodPerBite
            );

        Energy = Mathf.Min(
            genome.maxEnergy,
            Energy + foodEaten
        );

        targetPlant = null;
    }

    private void TryReproduce()
    {
        if (Age < genome.maturityAge)
        {
            return;
        }

        if (reproductionTimer > 0f)
        {
            return;
        }

        if (Energy <
            genome.reproductionEnergyThreshold)
        {
            return;
        }

        Herbivore[] population =
            FindObjectsByType<Herbivore>();

        if (population.Length >= maxPopulation)
        {
            return;
        }

        Vector3 spawnOffset =
            Random.insideUnitSphere * 0.5f;

        spawnOffset.y = 0f;

        GameObject offspringObject =
            Instantiate(
                gameObject,
                transform.position + spawnOffset,
                transform.rotation,
                transform.parent
            );

        Herbivore offspring =
            offspringObject.GetComponent<Herbivore>();

        offspring.genome =
            genome.Clone();

        offspring.genome.Mutate(
            mutationChance,
            mutationStrength
        );

        offspring.mutationChance =
            mutationChance;

        offspring.mutationStrength =
            mutationStrength;

        offspring.reproductionEnergyCost =
            reproductionEnergyCost;

        offspring.offspringStartingEnergy =
            offspringStartingEnergy;

        offspring.maxPopulation =
            maxPopulation;

        Energy = Mathf.Max(
            0f,
            Energy - reproductionEnergyCost
        );

        reproductionTimer =
            genome.reproductionCooldown;

        offspring.InitializeOffspring();
    }

    public void InitializeOffspring()
    {
        initialized = true;

        Age = 0f;
        targetPlant = null;
        searchTimer = 0f;

        reproductionTimer =
            genome.reproductionCooldown;

        ApplyGenome();

        Energy = Mathf.Min(
            offspringStartingEnergy,
            genome.maxEnergy
        );
    }
}