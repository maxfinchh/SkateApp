using UnityEngine;

public sealed class RunnerObstacle : MonoBehaviour
{
    private bool hit;

    public bool HasBeenHit => hit;

    public void MarkHit()
    {
        hit = true;
    }

    public void ResetHit()
    {
        hit = false;
    }
}
