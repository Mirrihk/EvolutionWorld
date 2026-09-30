using UnityEngine;
using UnityEngine.InputSystem;

public class SimulationControls : MonoBehaviour
{
    [Header("Simulation Speeds")]
    [Min(0.1f)]
    public float normalSpeed = 1f;

    [Min(0.1f)]
    public float fastSpeed = 2f;

    [Min(0.1f)]
    public float veryFastSpeed = 4f;

    private bool paused;
    private float currentSpeed = 1f;
    private float speedBeforePause = 1f;

    private int plantCount;
    private int herbivoreCount;
    private int predatorCount;

    private float statisticsTimer;

    private void Start()
    {
        SetSimulationSpeed(normalSpeed);
        RefreshPopulationCounts();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                TogglePause();
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SetSimulationSpeed(normalSpeed);
            }

            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SetSimulationSpeed(fastSpeed);
            }

            if (keyboard.digit4Key.wasPressedThisFrame)
            {
                SetSimulationSpeed(veryFastSpeed);
            }
        }

        statisticsTimer -=
            Time.unscaledDeltaTime;

        if (statisticsTimer <= 0f)
        {
            RefreshPopulationCounts();
            statisticsTimer = 0.5f;
        }
    }

    public void TogglePause()
    {
        if (paused)
        {
            SetSimulationSpeed(
                speedBeforePause
            );
        }
        else
        {
            speedBeforePause = currentSpeed;
            paused = true;
            Time.timeScale = 0f;
        }
    }

    public void SetSimulationSpeed(
        float speed)
    {
        currentSpeed = speed;
        speedBeforePause = speed;
        paused = false;
        Time.timeScale = speed;
    }

    private void RefreshPopulationCounts()
    {
        plantCount =
            FindObjectsByType<Plant>().Length;

        herbivoreCount =
            FindObjectsByType<Herbivore>().Length;

        predatorCount =
            FindObjectsByType<Predator>().Length;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(
            new Rect(
                15f,
                15f,
                250f,
                240f
            ),
            GUI.skin.window
        );

        GUILayout.Label(
            "EvolutionWorld"
        );

        string speedText = paused
            ? "Paused"
            : currentSpeed.ToString("0.0") + "x";

        GUILayout.Label(
            "Simulation Speed: " +
            speedText
        );

        GUILayout.Label(
            "Plants: " +
            plantCount
        );

        GUILayout.Label(
            "Herbivores: " +
            herbivoreCount
        );

        GUILayout.Label(
            "Predators: " +
            predatorCount
        );

        if (GUILayout.Button(
            paused
                ? "Resume - Space"
                : "Pause - Space"
        ))
        {
            TogglePause();
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("1x"))
        {
            SetSimulationSpeed(
                normalSpeed
            );
        }

        if (GUILayout.Button("2x"))
        {
            SetSimulationSpeed(
                fastSpeed
            );
        }

        if (GUILayout.Button("4x"))
        {
            SetSimulationSpeed(
                veryFastSpeed
            );
        }

        GUILayout.EndHorizontal();

        GUILayout.Label(
            "Keyboard: Space, 1, 2, 4"
        );

        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}