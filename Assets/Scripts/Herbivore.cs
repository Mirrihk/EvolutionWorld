using UnityEngine;

public class Herbivore : MonoBehaviour
{
    [Header("Movement")]
    [Min(0.1f)]
    public float speed = 2f;

    [Min(0.1f)]
    public float eatingDistance = 0.6f;

    [Min(0.05f)]
    public float searchInterval = 0.25f;

    [Header("Energy")]
    [Min(0.1f)]
    public float maxEnergy = 20f;

    [Min(0.1f)]
    public float startingEnergy = 20f;

    [Min(0.01f)]
    public float energyDrainPerSecond = 1f;

    [Min(0.1f)]
    public float foodPerBite = 2f;

    [Header("Life Cycle")]
    [Min(1f)]
    public float lifespan = 60f;

    [Min(0f)]
    public float maturityAge = 5f;

    [Min(0.1f)]
    public float reproductionCooldown = 12f;

    [Min(0.1f)]
    public float reproductionEnergyThreshold = 14f;

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
            Energy = Mathf.Min(
                startingEnergy,
                maxEnergy
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
            energyDrainPerSecond *
            Time.deltaTime;

        if (Energy <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (Age >= lifespan)
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
                searchTimer = searchInterval;
            }

            return;
        }

        MoveTowardPlant();

        if (IsCloseEnoughToEat())
        {
            EatTargetPlant();
        }
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
                speed * Time.deltaTime
            );

        Vector3 lookDirection =
            targetPosition - transform.position;

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

        return distance <= eatingDistance;
    }

    private void EatTargetPlant()
    {
        float foodEaten =
            targetPlant.Eat(foodPerBite);

        Energy = Mathf.Min(
            maxEnergy,
            Energy + foodEaten
        );

        targetPlant = null;
    }

    private void TryReproduce()
    {
        if (Age < maturityAge)
        {
            return;
        }

        if (reproductionTimer > 0f)
        {
            return;
        }

        if (Energy < reproductionEnergyThreshold)
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

        Energy = Mathf.Max(
            0f,
            Energy - reproductionEnergyCost
        );

        reproductionTimer =
            reproductionCooldown;

        offspring.InitializeOffspring(
            offspringStartingEnergy
        );
    }

    public void InitializeOffspring(
        float offspringEnergy)
    {
        initialized = true;

        Age = 0f;

        Energy = Mathf.Min(
            offspringEnergy,
            maxEnergy
        );

        targetPlant = null;
        searchTimer = 0f;

        reproductionTimer =
            reproductionCooldown;
    }
}