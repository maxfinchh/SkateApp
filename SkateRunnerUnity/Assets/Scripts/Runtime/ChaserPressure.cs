using UnityEngine;

public sealed class ChaserPressure : MonoBehaviour
{
    [SerializeField] private float mistakeWindowSeconds = 15f;
    [SerializeField] private int mistakesToFail = 2;

    public float Pressure { get; private set; }

    private int recentMistakes;
    private float lastMistakeTime;

    private void Update()
    {
        if (recentMistakes <= 0)
        {
            return;
        }

        float age = Time.time - lastMistakeTime;
        if (age >= mistakeWindowSeconds)
        {
            recentMistakes = 0;
            Pressure = 0f;
            return;
        }

        float basePressure = Mathf.Clamp01((float)recentMistakes / mistakesToFail);
        Pressure = basePressure * (1f - age / mistakeWindowSeconds);
    }

    public bool AddSketchyAction()
    {
        if (Time.time - lastMistakeTime > mistakeWindowSeconds)
        {
            recentMistakes = 0;
        }

        recentMistakes++;
        lastMistakeTime = Time.time;
        Pressure = Mathf.Clamp01((float)recentMistakes / mistakesToFail);
        return recentMistakes >= mistakesToFail;
    }

    public void ResetRun()
    {
        recentMistakes = 0;
        lastMistakeTime = 0f;
        Pressure = 0f;
    }
}
