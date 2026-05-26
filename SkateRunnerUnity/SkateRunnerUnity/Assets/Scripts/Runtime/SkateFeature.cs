using UnityEngine;

public enum SkateFeatureType
{
    KickerRamp,
    StairSet,
    ManualPad,
    NegativePickup
}

public sealed class SkateFeature : MonoBehaviour
{
    [SerializeField] private SkateFeatureType featureType;
    [SerializeField] private int bonusPoints = 150;
    [SerializeField] private float launchVelocity = 9f;

    private bool used;

    public SkateFeatureType FeatureType => featureType;
    public int BonusPoints => bonusPoints;
    public float LaunchVelocity => launchVelocity;
    public bool Used => used;

    public void Configure(SkateFeatureType type, int points, float launch)
    {
        featureType = type;
        bonusPoints = points;
        launchVelocity = launch;
    }

    public void MarkUsed()
    {
        used = true;
    }

    public void ResetUsed()
    {
        used = false;
    }
}
