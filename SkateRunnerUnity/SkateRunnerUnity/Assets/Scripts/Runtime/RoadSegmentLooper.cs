using UnityEngine;

public sealed class RoadSegmentLooper : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float segmentLength = 48f;
    [SerializeField] private int segmentCount = 6;

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;

        if (player == null)
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float trailingEdge = transform.position.z + segmentLength * 0.5f;
        if (trailingEdge < player.position.z - segmentLength)
        {
            transform.position += Vector3.forward * segmentLength * segmentCount;
        }
    }

    public void ResetSegment()
    {
        transform.position = startPosition;
    }
}
