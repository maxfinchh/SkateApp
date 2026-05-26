using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField] private GestureInput gestureInput;
    [SerializeField] private SkateRunnerGameManager gameManager;
    [SerializeField] private Transform trickRoot;
    [SerializeField] private Transform boardVisual;
    [SerializeField] private float laneWidth = 2.2f;
    [SerializeField] private float laneLerp = 14f;
    [SerializeField] private float forwardSpeed = 12f;
    [SerializeField] private float jumpVelocity = 8.5f;
    [SerializeField] private float gravity = -24f;
    [SerializeField] private float flipSeconds = 0.42f;

    private CharacterController controller;
    private int lane;
    private float verticalVelocity;
    private bool running;
    private bool flipping;
    private bool grinding;
    private float flipTimer;
    private float grindTimer;
    private GrindRail nearbyRail;
    private float baseForwardSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        baseForwardSpeed = forwardSpeed;

        if (gestureInput == null)
        {
            gestureInput = FindFirstObjectByType<GestureInput>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<SkateRunnerGameManager>();
        }
    }

    private void OnEnable()
    {
        if (gestureInput == null)
        {
            return;
        }

        gestureInput.Swipe += OnSwipe;
        gestureInput.SwipeHoldStarted += OnSwipeHoldStarted;
        gestureInput.SwipeHoldReleased += OnSwipeHoldReleased;
    }

    private void OnDisable()
    {
        if (gestureInput == null)
        {
            return;
        }

        gestureInput.Swipe -= OnSwipe;
        gestureInput.SwipeHoldStarted -= OnSwipeHoldStarted;
        gestureInput.SwipeHoldReleased -= OnSwipeHoldReleased;
    }

    private void Update()
    {
        if (!running)
        {
            return;
        }

        Vector3 targetPosition = transform.position;
        targetPosition.x = lane * laneWidth;

        float nextX = Mathf.Lerp(transform.position.x, targetPosition.x, laneLerp * Time.deltaTime);
        float xDelta = nextX - transform.position.x;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        if (!grinding)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = 0f;
            grindTimer += Time.deltaTime;
            gameManager.AddGrindScore(Time.deltaTime);
        }

        controller.Move(new Vector3(xDelta, verticalVelocity * Time.deltaTime, forwardSpeed * Time.deltaTime));
        UpdateFlipVisual();
    }

    public void ResetRun()
    {
        running = true;
        lane = 0;
        verticalVelocity = 0f;
        flipping = false;
        grinding = false;
        forwardSpeed = baseForwardSpeed;
        flipTimer = 0f;
        grindTimer = 0f;
        nearbyRail = null;
        transform.position = new Vector3(0f, 0.95f, 0f);
        transform.rotation = Quaternion.identity;

        if (trickRoot != null)
        {
            trickRoot.localRotation = Quaternion.identity;
        }
    }

    public void StopRun()
    {
        running = false;
    }

    public void StartFinalBonus()
    {
        forwardSpeed *= 1.35f;
        Jump();
    }

    private void OnSwipe(SwipeDirection direction)
    {
        if (!running || gameManager.State != RunState.Running)
        {
            return;
        }

        if (direction == SwipeDirection.Left)
        {
            lane = Mathf.Max(lane - 1, -1);
        }
        else if (direction == SwipeDirection.Right)
        {
            lane = Mathf.Min(lane + 1, 1);
        }
        else if (direction == SwipeDirection.Up)
        {
            Jump();
            gameManager.AddTrickScore(75);
        }
        else if (IsDiagonal(direction))
        {
            Kickflip();
        }
    }

    private void OnSwipeHoldStarted(SwipeDirection direction)
    {
        if (!running || gameManager.State != RunState.Running)
        {
            return;
        }

        if (IsDiagonal(direction) && nearbyRail != null)
        {
            grinding = true;
            grindTimer = 0f;
            verticalVelocity = 0f;
            transform.position = new Vector3(nearbyRail.transform.position.x, nearbyRail.GrindHeight, transform.position.z);
            gameManager.AddTrickScore(150);
        }
        else if (IsDiagonal(direction))
        {
            Kickflip();
        }
    }

    private void OnSwipeHoldReleased()
    {
        if (!grinding)
        {
            return;
        }

        grinding = false;
        Jump();
        gameManager.AddTrickScore(Mathf.RoundToInt(120f + grindTimer * 100f));
    }

    private void Jump()
    {
        if (controller.isGrounded || grinding)
        {
            verticalVelocity = jumpVelocity;
        }
    }

    private void Kickflip()
    {
        if (flipping)
        {
            return;
        }

        flipping = true;
        flipTimer = 0f;
        Jump();
        gameManager.AddTrickScore(200);
    }

    private void UpdateFlipVisual()
    {
        if (!flipping || boardVisual == null)
        {
            return;
        }

        flipTimer += Time.deltaTime;
        float t = Mathf.Clamp01(flipTimer / flipSeconds);
        boardVisual.localRotation = Quaternion.Euler(0f, 0f, 360f * t);

        if (t >= 1f)
        {
            flipping = false;
            boardVisual.localRotation = Quaternion.identity;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.TryGetComponent(out RunnerObstacle _))
        {
            gameManager.FailRun();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GrindRail rail))
        {
            nearbyRail = rail;
        }

        if (other.TryGetComponent(out CoinPickup coin))
        {
            coin.Collect(gameManager);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (nearbyRail != null && other.gameObject == nearbyRail.gameObject)
        {
            nearbyRail = null;
        }
    }

    private static bool IsDiagonal(SwipeDirection direction)
    {
        return direction == SwipeDirection.UpLeft ||
               direction == SwipeDirection.UpRight ||
               direction == SwipeDirection.DownLeft ||
               direction == SwipeDirection.DownRight;
    }
}
