using UnityEngine;

public class HerbivoreSpawner : MonoBehaviour
{
    [Header("References")]
    public WorldGrid worldGrid;

    [Header("Population")]
    [Min(1)]
    public int startingHerbivores = 1;

    public int seed = 13579;

    [Header("Appearance")]
    [Min(0.1f)]
    public float herbivoreSize = 0.35f;

    public Color herbivoreColor =
        new Color(0.65f, 0.25f, 0.08f);

    [Header("Movement Settings")]
    [Min(0.1f)]
    public float speed = 2f;

    [Min(0.1f)]
    public float eatingDistance = 0.6f;

    [Header("Energy Settings")]
    [Min(0.1f)]
    public float maxEnergy = 20f;

    [Min(0.1f)]
    public float startingEnergy = 20f;

    [Min(0.01f)]
    public float energyDrainPerSecond = 1f;

    [Min(0.1f)]
    public float foodPerBite = 2f;

    private Transform herbivoreContainer;
    private Material herbivoreMaterial;

    private void Start()
    {
        if (worldGrid == null)
        {
            worldGrid = GetComponent<WorldGrid>();
        }

        if (worldGrid == null)
        {
            Debug.LogError(
                "HerbivoreSpawner needs a WorldGrid reference."
            );

            return;
        }

        CreateHerbivoreMaterial();
        SpawnHerbivores();
    }

    private void SpawnHerbivores()
    {
        herbivoreContainer =
            new GameObject("Herbivores").transform;

        herbivoreContainer.SetParent(
            transform,
            false
        );

        Random.InitState(seed);

        for (int i = 0; i < startingHerbivores; i++)
        {
            SpawnOneHerbivore(i);
        }
    }

    private void SpawnOneHerbivore(int index)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x =
                Random.Range(0, worldGrid.width);

            int z =
                Random.Range(0, worldGrid.depth);

            if (worldGrid.IsWater(x, z))
            {
                continue;
            }

            GameObject herbivore =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );

            herbivore.name =
                $"Herbivore_{index}";

            herbivore.transform.SetParent(
                herbivoreContainer,
                true
            );

            herbivore.transform.localScale =
                Vector3.one * herbivoreSize;

            herbivore.transform.position =
                worldGrid.GetCellWorldPosition(
                    x,
                    z,
                    herbivoreSize / 2f
                );

            Destroy(
                herbivore.GetComponent<Collider>()
            );

            Renderer renderer =
                herbivore.GetComponent<Renderer>();

            renderer.sharedMaterial =
                herbivoreMaterial;

            Herbivore herbivoreData =
                herbivore.AddComponent<Herbivore>();

            herbivoreData.speed = speed;
            herbivoreData.eatingDistance =
                eatingDistance;
            herbivoreData.maxEnergy = maxEnergy;
            herbivoreData.startingEnergy =
                startingEnergy;
            herbivoreData.energyDrainPerSecond =
                energyDrainPerSecond;
            herbivoreData.foodPerBite =
                foodPerBite;

            return;
        }

        Debug.LogWarning(
            "Could not find land for herbivore."
        );
    }

    private void CreateHerbivoreMaterial()
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        herbivoreMaterial =
            new Material(shader);

        if (herbivoreMaterial.HasProperty("_BaseColor"))
        {
            herbivoreMaterial.SetColor(
                "_BaseColor",
                herbivoreColor
            );
        }

        if (herbivoreMaterial.HasProperty("_Color"))
        {
            herbivoreMaterial.SetColor(
                "_Color",
                herbivoreColor
            );
        }
    }
}