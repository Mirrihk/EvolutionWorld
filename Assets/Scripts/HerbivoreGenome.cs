using UnityEngine;

[System.Serializable]
public class HerbivoreGenome
{
    public float speed = 2f;
    public float bodySize = 0.35f;
    public float maxEnergy = 20f;
    public float startingEnergy = 20f;
    public float energyDrainPerSecond = 1f;
    public float foodPerBite = 2f;
    public float eatingDistance = 0.6f;

    public float lifespan = 60f;
    public float maturityAge = 5f;
    public float reproductionCooldown = 12f;
    public float reproductionEnergyThreshold = 14f;

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
                reproductionEnergyThreshold
        };
    }

    public void Mutate(
        float mutationChance,
        float mutationStrength)
    {
        speed = MutateTrait(
            speed,
            0.2f,
            5f,
            mutationChance,
            mutationStrength
        );

        bodySize = MutateTrait(
            bodySize,
            0.15f,
            0.7f,
            mutationChance,
            mutationStrength
        );

        maxEnergy = MutateTrait(
            maxEnergy,
            5f,
            50f,
            mutationChance,
            mutationStrength
        );

        startingEnergy = MutateTrait(
            startingEnergy,
            3f,
            40f,
            mutationChance,
            mutationStrength
        );

        energyDrainPerSecond = MutateTrait(
            energyDrainPerSecond,
            0.1f,
            3f,
            mutationChance,
            mutationStrength
        );

        foodPerBite = MutateTrait(
            foodPerBite,
            0.5f,
            8f,
            mutationChance,
            mutationStrength
        );

        lifespan = MutateTrait(
            lifespan,
            20f,
            180f,
            mutationChance,
            mutationStrength
        );

        maturityAge = MutateTrait(
            maturityAge,
            2f,
            30f,
            mutationChance,
            mutationStrength
        );

        reproductionCooldown = MutateTrait(
            reproductionCooldown,
            3f,
            60f,
            mutationChance,
            mutationStrength
        );

        reproductionEnergyThreshold =
            MutateTrait(
                reproductionEnergyThreshold,
                5f,
                40f,
                mutationChance,
                mutationStrength
            );

        startingEnergy =
            Mathf.Min(
                startingEnergy,
                maxEnergy
            );

        reproductionEnergyThreshold =
            Mathf.Min(
                reproductionEnergyThreshold,
                maxEnergy
            );

        maturityAge =
            Mathf.Min(
                maturityAge,
                lifespan * 0.75f
            );
    }

    private float MutateTrait(
        float value,
        float minimum,
        float maximum,
        float mutationChance,
        float mutationStrength)
    {
        if (Random.value > mutationChance)
        {
            return value;
        }

        float multiplier =
            1f + Random.Range(
                -mutationStrength,
                mutationStrength
            );

        float mutatedValue =
            value * multiplier;

        return Mathf.Clamp(
            mutatedValue,
            minimum,
            maximum
        );
    }
}