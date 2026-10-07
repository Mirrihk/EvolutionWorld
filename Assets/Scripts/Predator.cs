using UnityEngine;

public class Predator : MonoBehaviour
{
    [Header("References")]
    public WorldGrid worldGrid;
    public PredatorSpawner spawner;

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

    [Header("Phenotype")]
    [Min(0.1f)]
    public float bodySize = 0.45f;

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

    [Min(0f)]
    public float preyPredictionTime = 0.65f;

    [Min(0f)]
    public float maxPredictionDistance = 4f;

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

    [Min(1)]
    public int maxPopulation = 30;

    [Header("Mutation")]
    [Range(0f, 1f)]
    public float mutationChance = 0.08f;

    [Range(0f, 1f)]
    public float mutationStrength = 0.15f;

    [Header("Habitat")]
    public HabitatGenome habitat;

    [Min(0f)]
    public float flyingHeight = 2.5f;

    private static readonly float[] blockedRecoveryAngles =
    {
        45f,
        -45f,
        90f,
        -90f,
        135f,
        -135f,
        180f
    };

    private Herbivore currentTarget;
    private Vector3 roamTarget;

    private float currentEnergy;
    private float age;
    private float searchTimer;
    private float roamTimer;
    private float blockedTargetTimer;
    private float stuckRecoveryTimer;
    private float blockedTargetRetryTimer;
    private Herbivore blockedTarget;

    private Herbivore velocityTrackedTarget;
    private Vector3 previousTargetPosition;
    private Vector3 targetVelocity;

    private float timeSinceReproduction =
        Mathf.Infinity;

    private bool hasRoamTarget;

    public float CurrentEnergy
    {
        get
        {
            return currentEnergy;
        }
    }

    public float Age
    {
        get
        {
            return age;
        }
    }

    public bool IsReadyToReproduce
    {
        get
        {
            return
                age >= maturityAge &&
                currentEnergy >=
                reproductionEnergyThreshold &&
                timeSinceReproduction >=
                reproductionCooldown;
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

        if (spawner == null)
        {
            spawner =
                FindAnyObjectByType<PredatorSpawner>();
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

        bodySize =
            Mathf.Max(
                0.1f,
                bodySize
            );

        sightRadius =
            Mathf.Max(
                0.5f,
                sightRadius
            );

        eatingDistance =
            Mathf.Max(
                0.1f,
                eatingDistance
            );

        preyPredictionTime =
            Mathf.Max(
                0f,
                preyPredictionTime
            );

        maxPredictionDistance =
            Mathf.Max(
                0f,
                maxPredictionDistance
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

        timeSinceReproduction +=
            deltaTime;

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

        if (IsReadyToReproduce)
        {
            TryReproduce();
        }

        blockedTargetRetryTimer =
            Mathf.Max(
                0f,
                blockedTargetRetryTimer - deltaTime
            );

        if (blockedTargetRetryTimer <= 0f)
        {
            blockedTarget = null;
        }

        searchTimer -= deltaTime;

        if (
            searchTimer <= 0f ||
            !IsTargetValid(currentTarget))
        {
            Herbivore newTarget =
                FindClosestCompatiblePrey();

            if (newTarget != currentTarget)
            {
                blockedTargetTimer = 0f;
                stuckRecoveryTimer = 0f;
                ResetTargetVelocityTracking();
            }

            currentTarget =
                newTarget;

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

    private void TryReproduce()
    {
        if (spawner == null)
        {
            spawner =
                FindAnyObjectByType<PredatorSpawner>();
        }

        if (spawner == null)
        {
            return;
        }

        Predator mate =
            FindNearbyMate();

        if (mate == null)
        {
            return;
        }

        Vector3 offspringPosition =
            Vector3.Lerp(
                transform.position,
                mate.transform.position,
                0.5f
            );

        Predator offspring =
            spawner.SpawnOffspring(
                offspringPosition,
                this,
                mate
            );

        if (offspring != null)
        {
            MarkReproduced();
            mate.MarkReproduced();
        }
    }

    private Predator FindNearbyMate()
    {
        if (!IsReadyToReproduce)
        {
            return null;
        }

        if (
            !TryGetTerrainAtPosition(
                transform.position,
                out WorldTerrainType terrainType))
        {
            return null;
        }

        Predator[] predators =
            FindObjectsByType<Predator>();

        Predator closestMate =
            null;

        float closestDistance =
            mateSearchRadius;

        for (
            int i = 0;
            i < predators.Length;
            i++)
        {
            Predator candidate =
                predators[i];

            if (candidate == null)
            {
                continue;
            }

            if (candidate == this)
            {
                continue;
            }

            if (!candidate.IsReadyToReproduce)
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    candidate.transform.position
                );

            if (distance > closestDistance)
            {
                continue;
            }

            if (!CanUseTerrain(terrainType))
            {
                continue;
            }

            if (
                !candidate.CanOccupyTerrain(
                    terrainType))
            {
                continue;
            }

            closestDistance =
                distance;

            closestMate =
                candidate;
        }

        return closestMate;
    }

    public bool CanOccupyTerrain(
        WorldTerrainType terrainType)
    {
        return CanUseTerrain(terrainType);
    }

    public void MarkReproduced()
    {
        currentEnergy =
            Mathf.Max(
                0f,
                currentEnergy -
                reproductionEnergyCost
            );

        timeSinceReproduction =
            0f;
    }

    private Herbivore FindClosestCompatiblePrey()
    {
        Herbivore[] herbivores =
            FindObjectsByType<Herbivore>();

        Herbivore closest =
            null;

        float closestDistanceSquared =
            sightRadius * sightRadius;

        for (
            int i = 0;
            i < herbivores.Length;
            i++)
        {
            Herbivore herbivore =
                herbivores[i];

            if (
                !IsTargetValid(herbivore) ||
                (herbivore == blockedTarget &&
                 blockedTargetRetryTimer > 0f))
            {
                continue;
            }

            Vector3 difference =
                herbivore.transform.position -
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
        Herbivore target =
            currentTarget;

        if (!IsTargetValid(target))
        {
            currentTarget = null;
            return;
        }

        UpdateTargetVelocity(
            target,
            deltaTime
        );

        Vector3 targetPosition =
            GetPredictedTargetPosition(target);

        MoveTowardPosition(
            targetPosition,
            deltaTime,
            true
        );

        // The movement step may drop a blocked target, or another
        // predator may have eaten it while this frame was running.
        if (
            target == null ||
            currentTarget != target)
        {
            return;
        }

        Vector3 flatDifference =
            target.transform.position -
            transform.position;

        flatDifference.y = 0f;

        if (
            flatDifference.sqrMagnitude <=
            eatingDistance * eatingDistance)
        {
            EatCurrentTarget();
        }
    }

    private void UpdateTargetVelocity(
        Herbivore target,
        float deltaTime)
    {
        Vector3 targetPosition =
            target.transform.position;

        targetPosition.y = 0f;

        if (velocityTrackedTarget != target)
        {
            velocityTrackedTarget =
                target;

            previousTargetPosition =
                targetPosition;

            targetVelocity =
                Vector3.zero;

            return;
        }

        if (deltaTime <= 0.0001f)
        {
            return;
        }

        Vector3 observedVelocity =
            (targetPosition -
             previousTargetPosition) /
            deltaTime;

        observedVelocity =
            Vector3.ClampMagnitude(
                observedVelocity,
                15f
            );

        targetVelocity =
            Vector3.Lerp(
                targetVelocity,
                observedVelocity,
                0.35f
            );

        targetVelocity.y = 0f;

        previousTargetPosition =
            targetPosition;
    }

    private Vector3 GetPredictedTargetPosition(
        Herbivore target)
    {
        Vector3 predictionOffset =
            targetVelocity *
            preyPredictionTime;

        predictionOffset.y = 0f;

        predictionOffset =
            Vector3.ClampMagnitude(
                predictionOffset,
                maxPredictionDistance
            );

        return
            target.transform.position +
            predictionOffset;
    }

    private void ResetTargetVelocityTracking()
    {
        velocityTrackedTarget = null;
        previousTargetPosition = Vector3.zero;
        targetVelocity = Vector3.zero;
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
        bool recoverWhenBlocked)
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

            if (recoverWhenBlocked)
            {
                blockedTargetTimer = 0f;
                stuckRecoveryTimer = 0f;
            }
        }
        else if (recoverWhenBlocked)
        {
            blockedTargetTimer += deltaTime;
            stuckRecoveryTimer -= deltaTime;

            if (stuckRecoveryTimer <= 0f)
            {
                TryRecoverAroundBlockedPath(
                    direction,
                    deltaTime
                );

                stuckRecoveryTimer =
                    Mathf.Max(
                        0.05f,
                        stuckRecoveryInterval
                    );
            }

            if (
                blockedTargetTimer >=
                blockedTargetTimeout)
            {
                blockedTarget =
                    currentTarget;

                blockedTargetRetryTimer =
                    Mathf.Max(
                        0f,
                        blockedTargetRetryDelay
                    );

                currentTarget = null;
                searchTimer = 0f;
                blockedTargetTimer = 0f;
                stuckRecoveryTimer = 0f;
            }
        }
        else
        {
            ChooseNewRoamTarget();
        }
    }

    private bool TryRecoverAroundBlockedPath(
        Vector3 blockedDirection,
        float deltaTime)
    {
        for (
            int i = 0;
            i < blockedRecoveryAngles.Length;
            i++)
        {
            Vector3 recoveryDirection =
                Quaternion.AngleAxis(
                    blockedRecoveryAngles[i],
                    Vector3.up
                ) * blockedDirection;

            Vector3 recoveryPosition =
                transform.position +
                recoveryDirection *
                speed *
                deltaTime;

            if (
                !TryGetSmoothMovementPosition(
                    recoveryPosition,
                    out Vector3 correctedPosition))
            {
                continue;
            }

            transform.position =
                correctedPosition;

            FaceDirection(recoveryDirection);
            return true;
        }

        return false;
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

        blockedTargetTimer = 0f;
        stuckRecoveryTimer = 0f;
        ResetTargetVelocityTracking();

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