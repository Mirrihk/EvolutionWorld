using UnityEngine;

public class Plant : MonoBehaviour
{
    [Header("Food")]
    [Min(0.1f)]
    public float maxFood = 10f;

    public float CurrentFood { get; private set; }

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        CurrentFood = maxFood;
    }

    public void Initialize(float foodAmount)
    {
        maxFood = Mathf.Max(0.1f, foodAmount);
        CurrentFood = maxFood;
        originalScale = transform.localScale;
    }

    public float Eat(float amount)
    {
        if (CurrentFood <= 0f)
        {
            return 0f;
        }

        float foodEaten = Mathf.Min(
            amount,
            CurrentFood
        );

        CurrentFood -= foodEaten;

        float foodPercent =
            CurrentFood / maxFood;

        transform.localScale = new Vector3(
            originalScale.x,
            originalScale.y *
                Mathf.Max(0.05f, foodPercent),
            originalScale.z
        );

        if (CurrentFood <= 0.01f)
        {
            Destroy(gameObject);
        }

        return foodEaten;
    }
}