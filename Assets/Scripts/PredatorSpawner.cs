using UnityEngine;

public class PredatorSpawner : MonoBehaviour
{
    [Header("References")]
    public WorldGrid worldGrid;

    [Header("Population")]
    [Min(1)]
    public int startingPredators = 2;

    public int seed = 98765;

    [Header("Appearance")]
    [Min(0.1f)]
    public float predatorSize = 0.45f;

    public Color predatorColor =
        new Color(0.8f, 0.08f, 0.05f);

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

    private Transform predatorContainer;
    private Material predatorMaterial;

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
                "PredatorSpawner could not find WorldGrid."
            );

            return;
        }

        CreateContainer();
        CreateMaterial();
        SpawnPredators();
    }

    private void CreateContainer()
    {
        GameObject container =
            new GameObject("Predators");

        predatorContainer =
            container.transform;
    }

    private void CreateMaterial()
    {
        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );

        if (shader == null)
        {
            shader =
                Shader.Find("Standard");
        }

        predatorMaterial =
            new Material(shader);

        if (predatorMaterial.HasProperty(
                "_BaseColor"))
        {
            predatorMaterial.SetColor(
                "_BaseColor",
                predatorColor
            );
        }

        if (predatorMaterial.HasProperty(
                "_Color"))
        {
            predatorMaterial.SetColor(
                "_Color",
                predatorColor
            );
        }
    }

    private void SpawnPredators()
    {
        Random.InitState(seed);

        for (int i = 0;
             i < startingPredators;
             i++)
        {
            SpawnOnePredator(i);
        }
    }

    private void SpawnOnePredator(int index)
    {
        const int maximumAttempts = 100;

        for (int attempt = 0;
             attempt < maximumAttempts;
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

            if (worldGrid.IsWater(x, z))
            {
                continue;
            }

            GameObject predatorObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule
                );

            predatorObject.name =
                $"Predator_{index}";

            predatorObject.transform.SetParent(
                predatorContainer
            );

            predatorObject.transform.localScale =
                Vector3.one * predatorSize;

            predatorObject.transform.position =
                worldGrid.GetCellWorldPosition(
                    x,
                    z,
                    predatorSize
                );

            Collider collider =
                predatorObject.GetComponent<Collider>();

            if (collider != null)
            {
                Destroy(collider);
            }

            MeshRenderer renderer =
                predatorObject.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                renderer.material =
                    predatorMaterial;
            }

            Predator predator =
                predatorObject.AddComponent<Predator>();

            predator.speed =
                speed;

            predator.eatingDistance =
                eatingDistance;

            predator.maxEnergy =
                maxEnergy;

            predator.startingEnergy =
                startingEnergy;

            predator.energyDrainPerSecond =
                energyDrainPerSecond;

            predator.energyFromPrey =
                energyFromPrey;

            predator.lifespan =
                lifespan;

            predator.surfaceOffset =
                predatorSize;

            return;
        }

        Debug.LogWarning(
            "PredatorSpawner could not find land."
        );
    }
}