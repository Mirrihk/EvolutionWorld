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

    [Header("Terrain Placement")]
    [Min(0f)]
    public float surfaceOffset = 0.45f;

    public float Energy { get; private set; }
    public float Age { get; private set; }

    private WorldGrid worldGrid;
    private Herbivore targetHerbivore;
    private Vector3 lastValidPosition;

    private void Start()
    {
        worldGrid = FindAnyObjectByType<WorldGrid>();

        Energy = Mathf.Clamp(
            startingEnergy,
            0f,
            maxEnergy
        );

        Age = 0f;
        lastValidPosition = transform.position;

        SnapToTerrain();
    }

    private void Update()
    {
        Age += Time.deltaTime;

        Energy -=
            energyDrainPerSecond *
            Time.deltaTime;

        if (Age >= lifespan ||
            Energy <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (!SnapToTerrain())
        {
            transform.position = lastValidPosition;
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

        if (IsCloseEnoughToEat())
        {
            EatTarget();
        }
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

        Vector3 surfacePosition =
            worldGrid.GetCellWorldPosition(
                x,
                z,
                surfaceOffset
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

    private void FindClosestHerbivore()
    {
        Herbivore[] herbivores =
            FindObjectsByType<Herbivore>();

        float closestDistance =
            Mathf.Infinity;

        Herbivore closestHerbivore =
            null;

        foreach (Herbivore herbivore in herbivores)
        {
            if (herbivore == null)
            {
                continue;
            }

            Vector3 difference =
                herbivore.transform.position -
                transform.position;

            difference.y = 0f;

            float distance =
                difference.magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHerbivore = herbivore;
            }
        }

        targetHerbivore =
            closestHerbivore;
    }

    private void MoveTowardTarget()
    {
        if (targetHerbivore == null)
        {
            return;
        }

        Vector3 targetPosition =
            targetHerbivore.transform.position;

        targetPosition.y =
            transform.position.y;

        Vector3 direction =
            targetPosition -
            transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward =
                direction.normalized;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

        SnapToTerrain();
    }

    private bool IsCloseEnoughToEat()
    {
        if (targetHerbivore == null)
        {
            return false;
        }

        Vector3 difference =
            targetHerbivore.transform.position -
            transform.position;

        difference.y = 0f;

        return difference.sqrMagnitude <=
               eatingDistance * eatingDistance;
    }

    private void EatTarget()
    {
        if (targetHerbivore == null)
        {
            return;
        }

        Destroy(
            targetHerbivore.gameObject
        );

        Energy =
            Mathf.Min(
                maxEnergy,
                Energy + energyFromPrey
            );

        targetHerbivore = null;
    }
}