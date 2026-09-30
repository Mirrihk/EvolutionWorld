using UnityEngine;

public class Predator : MonoBehaviour
{
    [Header("Movement")]
    [Min(0.1f)]
    public float speed = 2.5f;

    [Min(0.1f)]
    public float eatingDistance = 0.7f;

    [Header("Energy")]
    [Min(0.1f)]
    public float maxEnergy = 30f;

    [Min(0.1f)]
    public float startingEnergy = 30f;

    [Min(0.01f)]
    public float energyDrainPerSecond = 1f;

    [Min(0.1f)]
    public float energyFromPrey = 12f;

    [Header("Life Cycle")]
    [Min(1f)]
    public float lifespan = 90f;

    public float Energy { get; private set; }
    public float Age { get; private set; }

    private Herbivore targetHerbivore;

    private void Start()
    {
        Energy = Mathf.Clamp(startingEnergy, 0f, maxEnergy);
        Age = 0f;
    }

    private void Update()
    {
        Age += Time.deltaTime;
        Energy -= energyDrainPerSecond * Time.deltaTime;

        if (Age >= lifespan || Energy <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (targetHerbivore == null)
        {
            FindClosestHerbivore();
        }

        if (targetHerbivore == null)
        {
            return;
        }

        MoveTowardTarget();

        if (Vector3.Distance(transform.position, targetHerbivore.transform.position) <= eatingDistance)
        {
            EatTarget();
        }
    }

    private void FindClosestHerbivore()
    {
        Herbivore[] herbivores = FindObjectsByType<Herbivore>();

        float closestDistance = Mathf.Infinity;
        Herbivore closestHerbivore = null;

        foreach (Herbivore herbivore in herbivores)
        {
            if (herbivore == null)
            {
                continue;
            }

            float distance = Vector3.Distance(
                transform.position,
                herbivore.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHerbivore = herbivore;
            }
        }

        targetHerbivore = closestHerbivore;
    }

    private void MoveTowardTarget()
    {
        Vector3 targetPosition = targetHerbivore.transform.position;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction.normalized;
        }
    }

    private void EatTarget()
    {
        if (targetHerbivore == null)
        {
            return;
        }

        Destroy(targetHerbivore.gameObject);

        Energy = Mathf.Min(
            maxEnergy,
            Energy + energyFromPrey
        );

        targetHerbivore = null;
    }
}