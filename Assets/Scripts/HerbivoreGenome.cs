using UnityEngine;

[System.Serializable]
public class HerbivoreGenome
{
    [Header("Physical Traits")]
    [Min(0.1f)]
    public float speed = 2f;

    [Min(0.1f)]
    public float bodySize = 0.35f;

    [Min(0.1f)]
    public float maxEnergy = 20f;

    [Min(0.1f)]
    public float startingEnergy = 20f;

    [Min(0.01f)]
    public float energyDrainPerSecond = 1f;

    [Min(0.1f)]
    public float foodPerBite = 2f;

    [Min(0.1f)]
    public float eatingDistance = 0.6f;

    [Header("Life Traits")]
    [Min(1f)]
    public float lifespan = 120f;

    [Min(1f)]
    public float maturityAge = 10f;

    [Min(0.1f)]
    public float reproductionCooldown = 8f;

    [Min(0.1f)]
    public float reproductionEnergyThreshold = 14f;

    [Min(0.1f)]
    public float reproductionEnergyCost = 6f;

    [Min(0.1f)]
    public float offspringStartingEnergy = 10f;

    [Header("Habitat Traits")]
    public HabitatGenome habitat =
        new HabitatGenome();

    public HerbivoreGenome Clone()
    {
        return new HerbivoreGenome
        {
            speed = speed,
            bodySize = bodySize,
            maxEnergy = maxEnergy,
            startingEnergy = startingEnergy,
            energyDrainPerSecond =
                energyDrainPerSecond,
            foodPerBite = foodPerBite,
            eatingDistance = eatingDistance,
            lifespan = lifespan,
            maturityAge = maturityAge,
            reproductionCooldown =
                reproductionCooldown,
            reproductionEnergyThreshold =
                reproductionEnergyThreshold,
            reproductionEnergyCost =
                reproductionEnergyCost,
            offspringStartingEnergy =
                offspringStartingEnergy,
            habitat = habitat != null
                ? habitat.Clone()
                : new HabitatGenome()
        };
    }

    public static HerbivoreGenome Combine(
        HerbivoreGenome first,
        HerbivoreGenome second,
        float mutationChance,
        float mutationStrength)
    {
        HerbivoreGenome child =
            new HerbivoreGenome();

        child.speed =
            Inherit(first.speed, second.speed);

        child.bodySize =
            Inherit(first.bodySize, second.bodySize);

        child.maxEnergy =
            Inherit(first.maxEnergy, second.maxEnergy);

        child.startingEnergy =
            Inherit(
                first.startingEnergy,
                second.startingEnergy
            );

        child.energyDrainPerSecond =
            Inherit(
                first.energyDrainPerSecond,
                second.energyDrainPerSecond
            );

        child.foodPerBite =
            Inherit(
                first.foodPerBite,
                second.foodPerBite
            );

        child.eatingDistance =
            Inherit(
                first.eatingDistance,
                second.eatingDistance
            );

        child.lifespan =
            Inherit(
                first.lifespan,
                second.lifespan
            );

        child.maturityAge =
            Inherit(
                first.maturityAge,
                second.maturityAge
            );

        child.reproductionCooldown =
            Inherit(
                first.reproductionCooldown,
                second.reproductionCooldown
            );

        child.reproductionEnergyThreshold =
            Inherit(
                first.reproductionEnergyThreshold,
                second.reproductionEnergyThreshold
            );

        child.reproductionEnergyCost =
            Inherit(
                first.reproductionEnergyCost,
                second.reproductionEnergyCost
            );

        child.offspringStartingEnergy =
            Inherit(
                first.offspringStartingEnergy,
                second.offspringStartingEnergy
            );

        child.habitat =
            HabitatGenome.Combine(
                first.habitat,
                second.habitat
            );

        child.Mutate(
            mutationChance,
            mutationStrength
        );

        return child;
    }

    public void Mutate(
        float mutationChance,
        float mutationStrength)
    {
        speed =
            MutateValue(
                speed,
                mutationChance,
                mutationStrength,
                0.2f,
                6f
            );

        bodySize =
            MutateValue(
                bodySize,
                mutationChance,
                mutationStrength,
                0.15f,
                1.5f
            );

        maxEnergy =
            MutateValue(
                maxEnergy,
                mutationChance,
                mutationStrength,
                5f,
                100f
            );

        startingEnergy =
            Mathf.Clamp(
                startingEnergy,
                1f,
                maxEnergy
            );

        energyDrainPerSecond =
            MutateValue(
                energyDrainPerSecond,
                mutationChance,
                mutationStrength,
                0.05f,
                5f
            );

        foodPerBite =
            MutateValue(
                foodPerBite,
                mutationChance,
                mutationStrength,
                0.25f,
                10f
            );

        eatingDistance =
            MutateValue(
                eatingDistance,
                mutationChance,
                mutationStrength,
                0.2f,
                2f
            );

        lifespan =
            MutateValue(
                lifespan,
                mutationChance,
                mutationStrength,
                20f,
                300f
            );

        maturityAge =
            MutateValue(
                maturityAge,
                mutationChance,
                mutationStrength,
                2f,
                80f
            );

        reproductionCooldown =
            MutateValue(
                reproductionCooldown,
                mutationChance,
                mutationStrength,
                1f,
                60f
            );

        reproductionEnergyThreshold =
            MutateValue(
                reproductionEnergyThreshold,
                mutationChance,
                mutationStrength,
                5f,
                maxEnergy
            );

        reproductionEnergyCost =
            MutateValue(
                reproductionEnergyCost,
                mutationChance,
                mutationStrength,
                1f,
                maxEnergy * 0.75f
            );

        offspringStartingEnergy =
            MutateValue(
                offspringStartingEnergy,
                mutationChance,
                mutationStrength,
                2f,
                maxEnergy
            );

        habitat ??= new HabitatGenome();

        habitat.Mutate(
            mutationChance,
            mutationStrength
        );
    }

    private static float Inherit(
        float first,
        float second)
    {
        return Mathf.Lerp(
            first,
            second,
            Random.value
        );
    }

    private static float MutateValue(
        float value,
        float chance,
        float strength,
        float minimum,
        float maximum)
    {
        if (Random.value <= chance)
        {
            value += Random.Range(
                -strength,
                strength
            );
        }

        return Mathf.Clamp(
            value,
            minimum,
            maximum
        );
    }
}