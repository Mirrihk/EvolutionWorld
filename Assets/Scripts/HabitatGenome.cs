using UnityEngine;

public enum CreatureLifestyle
{
    Terrestrial,
    Aquatic,
    Amphibious,
    Flying,
    Alpine
}

[System.Serializable]
public class HabitatGenome
{
    [Range(0f, 1f)]
    public float landAdaptation = 0.9f;

    [Range(0f, 1f)]
    public float aquaticAdaptation = 0.05f;

    [Range(0f, 1f)]
    public float flightAdaptation = 0f;

    [Range(0f, 1f)]
    public float coldTolerance = 0.15f;

    [Range(0f, 1f)]
    public float mountainAdaptation = 0.2f;

    public HabitatGenome Clone()
    {
        return new HabitatGenome
        {
            landAdaptation = landAdaptation,
            aquaticAdaptation = aquaticAdaptation,
            flightAdaptation = flightAdaptation,
            coldTolerance = coldTolerance,
            mountainAdaptation = mountainAdaptation
        };
    }

    public static HabitatGenome Combine(
        HabitatGenome first,
        HabitatGenome second)
    {
        if (first == null)
        {
            return second != null
                ? second.Clone()
                : new HabitatGenome();
        }

        if (second == null)
        {
            return first.Clone();
        }

        return new HabitatGenome
        {
            landAdaptation = Inherit(
                first.landAdaptation,
                second.landAdaptation
            ),

            aquaticAdaptation = Inherit(
                first.aquaticAdaptation,
                second.aquaticAdaptation
            ),

            flightAdaptation = Inherit(
                first.flightAdaptation,
                second.flightAdaptation
            ),

            coldTolerance = Inherit(
                first.coldTolerance,
                second.coldTolerance
            ),

            mountainAdaptation = Inherit(
                first.mountainAdaptation,
                second.mountainAdaptation
            )
        };
    }

    public void Mutate(
        float mutationChance,
        float mutationStrength)
    {
        landAdaptation =
            MutateValue(
                landAdaptation,
                mutationChance,
                mutationStrength
            );

        aquaticAdaptation =
            MutateValue(
                aquaticAdaptation,
                mutationChance,
                mutationStrength
            );

        flightAdaptation =
            MutateValue(
                flightAdaptation,
                mutationChance,
                mutationStrength
            );

        coldTolerance =
            MutateValue(
                coldTolerance,
                mutationChance,
                mutationStrength
            );

        mountainAdaptation =
            MutateValue(
                mountainAdaptation,
                mutationChance,
                mutationStrength
            );
    }

    public bool CanOccupy(
        WorldTerrainType terrainType)
    {
        bool canFly =
            flightAdaptation >= 0.72f;

        if (canFly)
        {
            return true;
        }

        if (IsAquaticTerrain(terrainType))
        {
            return aquaticAdaptation >= 0.55f ||
                   (aquaticAdaptation >= 0.30f &&
                    landAdaptation >= 0.40f);
        }

        if (IsColdTerrain(terrainType) &&
            IsMountainTerrain(terrainType))
        {
            return coldTolerance >= 0.45f &&
                   mountainAdaptation >= 0.40f;
        }

        if (IsColdTerrain(terrainType))
        {
            return coldTolerance >= 0.45f &&
                   landAdaptation >= 0.35f;
        }

        if (IsMountainTerrain(terrainType))
        {
            return mountainAdaptation >= 0.45f &&
                   landAdaptation >= 0.35f;
        }

        if (IsWetTerrain(terrainType))
        {
            return landAdaptation >= 0.35f ||
                   (aquaticAdaptation >= 0.30f &&
                    landAdaptation >= 0.30f);
        }

        return landAdaptation >= 0.35f;
    }

    public CreatureLifestyle GetLifestyle()
    {
        if (flightAdaptation >= 0.72f)
        {
            return CreatureLifestyle.Flying;
        }

        if (aquaticAdaptation >= 0.65f &&
            landAdaptation < 0.40f)
        {
            return CreatureLifestyle.Aquatic;
        }

        if (aquaticAdaptation >= 0.30f &&
            landAdaptation >= 0.40f)
        {
            return CreatureLifestyle.Amphibious;
        }

        if (mountainAdaptation >= 0.65f &&
            coldTolerance >= 0.50f)
        {
            return CreatureLifestyle.Alpine;
        }

        return CreatureLifestyle.Terrestrial;
    }

    private static float Inherit(
        float first,
        float second)
    {
        return Mathf.Clamp01(
            Mathf.Lerp(
                first,
                second,
                Random.value
            )
        );
    }

    private static float MutateValue(
        float value,
        float mutationChance,
        float mutationStrength)
    {
        if (Random.value <= mutationChance)
        {
            value += Random.Range(
                -mutationStrength,
                mutationStrength
            );
        }

        return Mathf.Clamp01(value);
    }

    private static bool IsAquaticTerrain(
        WorldTerrainType terrainType)
    {
        return terrainType ==
                   WorldTerrainType.Water ||
               terrainType ==
                   WorldTerrainType.River ||
               terrainType ==
                   WorldTerrainType.Lake ||
               terrainType ==
                   WorldTerrainType.Ocean ||
               terrainType ==
                   WorldTerrainType.CoralReef ||
               terrainType ==
                   WorldTerrainType.Fjord ||
               terrainType ==
                   WorldTerrainType.DeepSea ||
               terrainType ==
                   WorldTerrainType.KelpForest ||
               terrainType ==
                   WorldTerrainType.Trench;
    }

    private static bool IsColdTerrain(
        WorldTerrainType terrainType)
    {
        return terrainType ==
                   WorldTerrainType.Ice ||
               terrainType ==
                   WorldTerrainType.Snow ||
               terrainType ==
                   WorldTerrainType.Tundra ||
               terrainType ==
                   WorldTerrainType.Glacier;
    }

    private static bool IsMountainTerrain(
        WorldTerrainType terrainType)
    {
        return terrainType ==
                   WorldTerrainType.Mountain ||
               terrainType ==
                   WorldTerrainType.Cliff ||
               terrainType ==
                   WorldTerrainType.Plateau ||
               terrainType ==
                   WorldTerrainType.Canyon ||
               terrainType ==
                   WorldTerrainType.Volcano ||
               terrainType ==
                   WorldTerrainType.Cave;
    }

    private static bool IsWetTerrain(
        WorldTerrainType terrainType)
    {
        return terrainType ==
                   WorldTerrainType.Swamp ||
               terrainType ==
                   WorldTerrainType.Marsh ||
               terrainType ==
                   WorldTerrainType.Wetland ||
               terrainType ==
                   WorldTerrainType.Mangrove;
    }
}