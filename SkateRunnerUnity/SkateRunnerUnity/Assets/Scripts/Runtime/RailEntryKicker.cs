using UnityEngine;

public sealed class RailEntryKicker : MonoBehaviour
{
    [SerializeField] private GrindRail rail;

    public GrindRail Rail => rail;

    public void Configure(GrindRail owner)
    {
        rail = owner;
    }

    public float SurfaceHeightAt(Vector3 worldPosition)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPosition);
        localPoint.x = 0f;
        localPoint.y = 0.5f;
        return transform.TransformPoint(localPoint).y;
    }
}
