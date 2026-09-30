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

    public float Energy { get; private set; }

    private Plant targetPlant;
    private float searchTimer;

    private void Start()
    {
        Energy = Mathf.Min(
            startingEnergy,
            maxEnergy
        );

        searchTimer = 0f;
    }

    private void Update()
    {
        Energy -=
            energyDrainPerSecond *
            Time.deltaTime;

        if (Energy <= 0f)
        {
            Destroy(gameObject);
            return;
        }

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

        float distance =
            Vector3.Distance(
                transform.position,
                targetPosition
            );

        if (distance <= eatingDistance)
        {
            float foodEaten =
                targetPlant.Eat(foodPerBite);

            Energy = Mathf.Min(
                maxEnergy,
                Energy + foodEaten
            );

            targetPlant = null;
        }
    }

    private void FindClosestPlant()
    {
        Plant[] plants =
            FindObjectsByType<Plant>(
                FindObjectsSortMode.None
            );

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
}
