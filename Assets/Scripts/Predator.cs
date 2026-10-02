using UnityEngine;

public class Predator : MonoBehaviour
{
    [Header("References")]
    public WorldGrid worldGrid;

    [Header("Movement")]
    [Min(0.01f)]
    public float speed = 1.8f;

    [Min(0.01f)]
    public float surfaceOffset = 0.5f;

    [Min(0.05f)]
    public float searchInterval = 0.5f;

    [Min(0.5f)]
    public float roamRadius = 6f;

    [Min(0.1f)]
    public float roamDistance = 4f;

    [Header("Hunting")]
    [Min(0.1f)]
    public float eatingDistance = 0.8f;

    [Min(0.1f)]
    public float energyFromPrey = 35f;

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

    [Header("Habitat")]
    public HabitatGenome habitat;

    [Min(0f)]
    public float flyingHeight = 2.5f;

    private Herbivore currentTarget;
    private Vector3 roamTarget;

    private float currentEnergy;
    private float age;
    private float searchTimer;
    private float roamTimer;

    private bool hasRoamTarget;

    public float CurrentEnergy
    {
        get
        {
            return currentEnergy;
        }
    }

    private void Awake()
    {
        EnsureHabitat();
    }

    private void Start()
    {
        if (worldGrid == null)
        {
            worldGrid =
                FindAnyObjectByType<WorldGrid>();
        }

        if (worldGrid == null)
        {
            Debug.LogError(
                "Predator could not find a WorldGrid."
            );

            enabled = false;
            return;
        }

        EnsureHabitat();

        speed =
            Mathf.Max(
                0.1f,
                speed
            );

        maxEnergy =
            Mathf.Max(
                1f,
                maxEnergy
            );

        startingEnergy =
            Mathf.Clamp(
                startingEnergy,
                0f,
                maxEnergy
            );

        currentEnergy =
            startingEnergy;

        SnapToCompatibleSurface();

        ChooseNewRoamTarget();
    }

    private void Update()
    {
        if (worldGrid == null)
        {
            return;
        }

        float deltaTime =
            Time.deltaTime;

        age += deltaTime;

        currentEnergy -=
            energyDrainPerSecond *
            deltaTime;

        if (
            currentEnergy <= 0f ||
            age >= lifespan)
        {
            Die();
            return;
        }

        searchTimer -= deltaTime;

        if (
            searchTimer <= 0f ||
            !IsTargetValid(currentTarget))
        {
            currentTarget =
                FindClosestCompatiblePrey();

            searchTimer =
                Mathf.Max(
                    0.05f,
                    searchInterval
                );
        }

        if (currentTarget != null)
        {
            MoveTowardTarget(deltaTime);
        }
        else
        {
            MoveTowardRoamTarget(deltaTime);
        }
    }

    private Herbivore FindClosestCompatiblePrey()
    {
        Herbivore[] herbivores =
            FindObjectsByType<Herbivore>();

        Herbivore closest =
            null;

        float closestDistance =
            float.MaxValue;

        for (
            int i = 0;
            i < herbivores.Length;
            i++)
        {
            Herbivore herbivore =
                herbivores[i];

            if (
                !IsTargetValid(herbivore))
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    herbivore.transform.position
                );

            if (
                distance < closestDistance)
            {
                closestDistance =
                    distance;

                closest =
                    herbivore;
            }
        }

        return closest;
    }

    private bool IsTargetValid(
        Herbivore herbivore)
    {
        if (herbivore == null)
        {
            return false;
        }

        if (
            !herbivore.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (
            !TryGetTerrainAtPosition(
                herbivore.transform.position,
                out WorldTerrainType terrainType))
        {
            return false;
        }

        return CanUseTerrain(terrainType);
    }

    private void MoveTowardTarget(
        float deltaTime)
    {
        if (
            !IsTargetValid(currentTarget))
        {
            currentTarget =
                null;

            return;
        }

        Vector3 targetPosition =
            currentTarget.transform.position;

        MoveTowardPosition(
            targetPosition,
            deltaTime,
            true
        );

        float distance =
            Vector3.Distance(
                transform.position,
                currentTarget.transform.position
            );

        if (
            distance <= eatingDistance)
        {
            EatCurrentTarget();
        }
    }

    private void MoveTowardRoamTarget(
        float deltaTime)
    {
        roamTimer -= deltaTime;

        if (
            !hasRoamTarget ||
            roamTimer <= 0f)
        {
            ChooseNewRoamTarget();
        }

        if (!hasRoamTarget)
        {
            SnapToCompatibleSurface();
            return;
        }

        MoveTowardPosition(
            roamTarget,
            deltaTime,
            false
        );

        Vector3 flatDifference =
            new Vector3(
                roamTarget.x - transform.position.x,
                0f,
                roamTarget.z - transform.position.z
            );

        if (
            flatDifference.magnitude <=
            0.35f)
        {
            ChooseNewRoamTarget();
        }
    }

    private void MoveTowardPosition(
        Vector3 destination,
        float deltaTime,
        bool clearTargetWhenBlocked)
    {
        Vector3 flatDirection =
            new Vector3(
                destination.x - transform.position.x,
                0f,
                destination.z - transform.position.z
            );

        if (
            flatDirection.sqrMagnitude <=
            0.001f)
        {
            return;
        }

        Vector3 direction =
            flatDirection.normalized;

        Vector3 nextPosition =
            transform.position +
            direction *
            speed *
            deltaTime;

        if (
            TryGetSmoothMovementPosition(
                nextPosition,
                out Vector3 correctedPosition))
        {
            transform.position =
                correctedPosition;

            FaceDirection(direction);
        }
        else if (clearTargetWhenBlocked)
        {
            currentTarget =
                null;
        }
        else
        {
            ChooseNewRoamTarget();
        }
    }

    private void ChooseNewRoamTarget()
    {
        if (worldGrid == null)
        {
            return;
        }

        for (
            int attempt = 0;
            attempt < 100;
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
                !CanUseTerrain(terrainType))
            {
                continue;
            }

            Vector3 candidate =
                worldGrid.GetCellWorldPosition(
                    x,
                    z,
                    GetVerticalOffset()
                );

            float distance =
                Vector3.Distance(
                    transform.position,
                    candidate
                );

            if (
                distance <=
                roamRadius +
                roamDistance)
            {
                roamTarget =
                    candidate;

                hasRoamTarget =
                    true;

                roamTimer =
                    Random.Range(
                        2f,
                        6f
                    );

                return;
            }
        }

        hasRoamTarget =
            false;
    }

    private void EatCurrentTarget()
    {
        if (currentTarget == null)
        {
            return;
        }

        currentEnergy =
            Mathf.Min(
                maxEnergy,
                currentEnergy +
                energyFromPrey
            );

        Destroy(
            currentTarget.gameObject
        );

        currentTarget =
            null;

        ChooseNewRoamTarget();
    }

    private void SnapToCompatibleSurface()
    {
        if (
            TryGetCompatibleSurfacePosition(
                transform.position,
                out Vector3 surfacePosition))
        {
            transform.position =
                surfacePosition;
        }
    }

    private bool TryGetCompatibleSurfacePosition(
        Vector3 worldPosition,
        out Vector3 surfacePosition)
    {
        surfacePosition =
            transform.position;

        if (
            !worldGrid.TryGetCellCoordinates(
                worldPosition,
                out int x,
                out int z))
        {
            return false;
        }

        WorldTerrainType terrainType =
            worldGrid.GetTerrainType(
                x,
                z
            );

        if (
            !CanUseTerrain(terrainType))
        {
            return false;
        }

        surfacePosition =
            worldGrid.GetCellWorldPosition(
                x,
                z,
                GetVerticalOffset()
            );

        return true;
    }

    private bool TryGetSmoothMovementPosition(
        Vector3 worldPosition,
        out Vector3 correctedPosition)
    {
        correctedPosition =
            transform.position;

        if (
            !worldGrid.TryGetCellCoordinates(
                worldPosition,
                out int x,
                out int z))
        {
            return false;
        }

        WorldTerrainType terrainType =
            worldGrid.GetTerrainType(
                x,
                z
            );

        if (
            !CanUseTerrain(terrainType))
        {
            return false;
        }

        Vector3 localPosition =
            worldGrid.transform.InverseTransformPoint(
                worldPosition
            );

        float centerX =
            (worldGrid.width - 1) / 2f;

        float centerZ =
            (worldGrid.depth - 1) / 2f;

        float gridX =
            localPosition.x /
            worldGrid.cellSize +
            centerX;

        float gridZ =
            localPosition.z /
            worldGrid.cellSize +
            centerZ;

        gridX =
            Mathf.Clamp(
                gridX,
                0f,
                worldGrid.width - 1f
            );

        gridZ =
            Mathf.Clamp(
                gridZ,
                0f,
                worldGrid.depth - 1f
            );

        int x0 =
            Mathf.FloorToInt(gridX);

        int z0 =
            Mathf.FloorToInt(gridZ);

        int x1 =
            Mathf.Min(
                x0 + 1,
                worldGrid.width - 1
            );

        int z1 =
            Mathf.Min(
                z0 + 1,
                worldGrid.depth - 1
            );

        float xBlend =
            gridX - x0;

        float zBlend =
            gridZ - z0;

        float height00 =
            worldGrid.GetSurfaceHeight(
                x0,
                z0
            );

        float height10 =
            worldGrid.GetSurfaceHeight(
                x1,
                z0
            );

        float height01 =
            worldGrid.GetSurfaceHeight(
                x0,
                z1
            );

        float height11 =
            worldGrid.GetSurfaceHeight(
                x1,
                z1
            );

        float topHeight =
            Mathf.Lerp(
                height00,
                height10,
                xBlend
            );

        float bottomHeight =
            Mathf.Lerp(
                height01,
                height11,
                xBlend
            );

        float smoothHeight =
            Mathf.Lerp(
                topHeight,
                bottomHeight,
                zBlend
            );

        localPosition.y =
            smoothHeight +
            GetVerticalOffset();

        correctedPosition =
            worldGrid.transform.TransformPoint(
                localPosition
            );

        return true;
    }

    private bool TryGetTerrainAtPosition(
        Vector3 worldPosition,
        out WorldTerrainType terrainType)
    {
        terrainType =
            WorldTerrainType.Land;

        if (
            !worldGrid.TryGetCellCoordinates(
                worldPosition,
                out int x,
                out int z))
        {
            return false;
        }

        terrainType =
            worldGrid.GetTerrainType(
                x,
                z
            );

        return true;
    }

    private float GetVerticalOffset()
    {
        float offset =
            surfaceOffset;

        if (IsFlying())
        {
            offset +=
                flyingHeight;
        }

        return offset;
    }

    private bool CanUseTerrain(
        WorldTerrainType terrainType)
    {
        EnsureHabitat();

        bool flying =
            IsFlying();

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

    private bool IsFlying()
    {
        EnsureHabitat();

        return
            habitat.flightAdaptation >= 0.80f &&
            habitat.flightAdaptation >
            habitat.landAdaptation &&
            habitat.flightAdaptation >
            habitat.aquaticAdaptation;
    }

    private void FaceDirection(
        Vector3 direction)
    {
        if (
            direction.sqrMagnitude <=
            0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
    }

    private void EnsureHabitat()
    {
        if (habitat == null)
        {
            habitat =
                new HabitatGenome();
        }

        bool noAdaptations =
            habitat.landAdaptation <= 0f &&
            habitat.aquaticAdaptation <= 0f &&
            habitat.flightAdaptation <= 0f &&
            habitat.coldTolerance <= 0f &&
            habitat.mountainAdaptation <= 0f;

        if (noAdaptations)
        {
            habitat.landAdaptation =
                1f;

            habitat.aquaticAdaptation =
                0f;

            habitat.flightAdaptation =
                0f;

            habitat.coldTolerance =
                0.5f;

            habitat.mountainAdaptation =
                0.5f;
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}