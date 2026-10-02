using UnityEngine;

public class Plant : MonoBehaviour
{
    [Header("Food")]
    [Min(0.1f)]
    public float maxFood = 10f;

    public float CurrentFood { get; private set; }

    public WorldTerrainType terrainType;

    public string plantName;

    public bool IsAquaticPlant
    {
        get
        {
            return IsAquaticTerrain(terrainType);
        }
    }

    public void Initialize(
        float startingFood)
    {
        maxFood =
            Mathf.Max(
                0.1f,
                startingFood
            );

        CurrentFood =
            maxFood;
    }

    public bool CanBeEatenBy(
        HabitatGenome habitat)
    {
        if (habitat == null)
        {
            return false;
        }

        return habitat.CanOccupy(
            terrainType
        );
    }

    public float Eat(
        float requestedAmount)
    {
        if (requestedAmount <= 0f ||
            CurrentFood <= 0f)
        {
            return 0f;
        }

        float foodEaten =
            Mathf.Min(
                requestedAmount,
                CurrentFood
            );

        CurrentFood -=
            foodEaten;

        if (CurrentFood <= 0.01f)
        {
            Destroy(gameObject);
        }

        return foodEaten;
    }

    private bool IsAquaticTerrain(
        WorldTerrainType type)
    {
        return type ==
                   WorldTerrainType.Water ||
               type ==
                   WorldTerrainType.River ||
               type ==
                   WorldTerrainType.Lake ||
               type ==
                   WorldTerrainType.Ocean ||
               type ==
                   WorldTerrainType.CoralReef ||
               type ==
                   WorldTerrainType.Fjord ||
               type ==
                   WorldTerrainType.DeepSea ||
               type ==
                   WorldTerrainType.KelpForest ||
               type ==
                   WorldTerrainType.Trench;
    }
}