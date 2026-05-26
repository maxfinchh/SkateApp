using System.Collections.Generic;
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
    [SerializeField] private float roadHalfWidth = 3.05f;
    [SerializeField] private float smoothSteerLerp = 24f;
    [SerializeField] private float forwardSpeed = 12f;
    [SerializeField] private float maxForwardSpeed = 19f;
    [SerializeField] private float speedGainPerSecond = 0.12f;
    [SerializeField] private float pushAcceleration = 11f;
    [SerializeField] private float pushReleaseDeceleration = 1.15f;
    [SerializeField] private float pushSpeedCarrySeconds = 1.35f;
    [SerializeField] private float coastSpeed = 8.5f;
    [SerializeField] private float manualMissBackForce = 8f;
    [SerializeField] private float manualMissSideForce = 3.2f;
    [SerializeField] private float shoveRecovery = 18f;
    [SerializeField] private float jumpVelocity = 8.5f;
    [SerializeField] private float gravity = -24f;
    [SerializeField] private float flipSeconds = 0.42f;
    [SerializeField] private float finalBonusSpeedMultiplier = 1.35f;
    [SerializeField] private float finalLaunchVelocity = 36f;
    [SerializeField] private float finalTrickBoost = 2.8f;
    [SerializeField] private float finalTrickForwardBoost = 0.75f;
    [SerializeField] private float finalTrickCooldownSeconds = 0.12f;
    [SerializeField] private float minimumFinalAirSeconds = 3f;
    [SerializeField] private float queuedGrindSeconds = 1.15f;
    [SerializeField] private float grindFlickBufferSeconds = 0.55f;
    [SerializeField] private float railBumpGraceSeconds = 0.22f;
    [SerializeField] private float railBumpCooldownSeconds = 0.85f;
    [SerializeField] private float railTrickApproachDistance = 17f;
    [SerializeField] private float railTrickBehindDistance = 5f;
    [SerializeField] private float railSideReach = 3.8f;
    [SerializeField] private float manualEntryGraceSeconds = 0.9f;
    [SerializeField] private float manualPadSeconds = 1.45f;
    [SerializeField] private float manualMissCooldownSeconds = 15f;
    [SerializeField] private float negativePickupCooldownSeconds = 0.75f;

    private CharacterController controller;
    private int lane;
    private int previousLane;
    private float steerTargetX;
    private float verticalVelocity;
    private float backwardVelocity;
    private float sideVelocity;
    private bool running;
    private bool pushing;
    private bool flipping;
    private bool grinding;
    private bool manualing;
    private float flipTimer;
    private float grindTimer;
    private float manualTimer;
    private GrindRail nearbyRail;
    private GrindRail currentRail;
    private float baseForwardSpeed;
    private float pushCarryUntil;
    private bool hasFailed;
    private bool finalBonusActive;
    private bool finalLaunchStarted;
    private float finalRampZ;
    private float finalLaunchStartZ;
    private float finalAirTimer;
    private int finalBonusTricks;
    private float finalTrickCooldown;
    private Vector3 flipAxis = Vector3.forward;
    private float flipDegrees = 360f;
    private SkateFeature pendingFeatureTrick;
    private float featureTrickTimer;
    private float queuedGrindUntil;
    private SwipeDirection queuedGrindDirection = SwipeDirection.UpRight;
    private bool queuedGrindTrickAlreadyPerformed;
    private GrindRail pendingBumpRail;
    private float pendingRailBumpTime;
    private float railBumpCooldownUntil;
    private float lastManualEntryInputTime;
    private float manualMissCooldownUntil;
    private float negativePickupCooldownUntil;

    public int FinalBonusTrickCount => finalBonusTricks;
    public float CurrentFinalBonusDistance => finalLaunchStarted ? Mathf.Max(0f, transform.position.z - finalLaunchStartZ) : 0f;
    public float CurrentFinalBonusPower => finalLaunchStarted
        ? Mathf.Clamp01((verticalVelocity + finalLaunchVelocity * 0.2f) / (finalLaunchVelocity * 1.2f))
        : 0f;

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
        gestureInput.LaneTargetChanged += OnLaneTargetChanged;
        gestureInput.SteerTargetChanged += OnSteerTargetChanged;
        gestureInput.PushChanged += OnPushChanged;
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
        gestureInput.LaneTargetChanged -= OnLaneTargetChanged;
        gestureInput.SteerTargetChanged -= OnSteerTargetChanged;
        gestureInput.PushChanged -= OnPushChanged;
    }

    private void Update()
    {
        if (!running)
        {
            return;
        }

        if (finalBonusActive)
        {
            lane = 0;
            steerTargetX = 0f;
        }
        else if (gameManager.State == RunState.Running)
        {
            float speedMultiplier = gameManager.Upgrades != null ? gameManager.Upgrades.SpeedMultiplier : 1f;
            if (gestureInput != null && gestureInput.Scheme == ControlScheme.PushAndFlick)
            {
                float coastTarget = coastSpeed * speedMultiplier;
                if (!pushing && Time.time < pushCarryUntil && forwardSpeed > coastTarget)
                {
                    // Keep speed briefly after a push so the player has room to chain tricks.
                }
                else
                {
                    float targetSpeed = pushing ? maxForwardSpeed * speedMultiplier : coastTarget;
                    float rate = pushing ? pushAcceleration * speedMultiplier : pushReleaseDeceleration;
                    forwardSpeed = Mathf.MoveTowards(forwardSpeed, targetSpeed, rate * Time.deltaTime);
                }
            }
            else
            {
                forwardSpeed = Mathf.Min(maxForwardSpeed * speedMultiplier, forwardSpeed + speedGainPerSecond * speedMultiplier * Time.deltaTime);
            }
        }

        Vector3 targetPosition = transform.position;
        bool smoothSteering = gestureInput != null && gestureInput.Scheme == ControlScheme.PushAndFlick;
        targetPosition.x = smoothSteering ? steerTargetX : lane * laneWidth;

        float nextX = Mathf.Lerp(transform.position.x, targetPosition.x, (smoothSteering ? smoothSteerLerp : laneLerp) * Time.deltaTime);
        float xDelta = nextX - transform.position.x + sideVelocity * Time.deltaTime;

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

        controller.Move(new Vector3(xDelta, verticalVelocity * Time.deltaTime, (forwardSpeed - backwardVelocity) * Time.deltaTime));
        backwardVelocity = Mathf.MoveTowards(backwardVelocity, 0f, shoveRecovery * Time.deltaTime);
        sideVelocity = Mathf.MoveTowards(sideVelocity, 0f, shoveRecovery * Time.deltaTime);
        UpdateFinalBonus();
        UpdateManual();
        UpdateFlipVisual();
        HandleKeyboardDebugInput();
        ProcessPendingRailBump();
    }

    public void ResetRun()
    {
        running = true;
        lane = 0;
        previousLane = 0;
        steerTargetX = 0f;
        verticalVelocity = 0f;
        backwardVelocity = 0f;
        sideVelocity = 0f;
        pushing = false;
        flipping = false;
        grinding = false;
        manualing = false;
        hasFailed = false;
        finalBonusActive = false;
        finalLaunchStarted = false;
        finalAirTimer = 0f;
        finalBonusTricks = 0;
        finalTrickCooldown = 0f;
        pushCarryUntil = 0f;
        forwardSpeed = gestureInput != null && gestureInput.Scheme == ControlScheme.PushAndFlick ? 0f : baseForwardSpeed;
        flipTimer = 0f;
        grindTimer = 0f;
        manualTimer = 0f;
        featureTrickTimer = 0f;
        queuedGrindUntil = 0f;
        queuedGrindDirection = SwipeDirection.UpRight;
        queuedGrindTrickAlreadyPerformed = false;
        pendingBumpRail = null;
        pendingRailBumpTime = 0f;
        railBumpCooldownUntil = 0f;
        lastManualEntryInputTime = -999f;
        manualMissCooldownUntil = 0f;
        negativePickupCooldownUntil = 0f;
        pendingFeatureTrick = null;
        nearbyRail = null;
        currentRail = null;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        if (trickRoot != null)
        {
            trickRoot.localRotation = Quaternion.identity;
        }

        if (boardVisual != null)
        {
            boardVisual.localRotation = Quaternion.identity;
        }
    }

    public void StopRun()
    {
        running = false;
    }

    public void CenterForResults()
    {
        lane = 0;
        previousLane = 0;
        steerTargetX = 0f;
        verticalVelocity = 0f;
        backwardVelocity = 0f;
        sideVelocity = 0f;
        pushing = false;
        grinding = false;
        manualing = false;
        flipping = false;
        currentRail = null;
        queuedGrindUntil = 0f;
        queuedGrindDirection = SwipeDirection.UpRight;
        queuedGrindTrickAlreadyPerformed = false;
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;

        if (trickRoot != null)
        {
            trickRoot.localRotation = Quaternion.identity;
        }

        if (boardVisual != null)
        {
            boardVisual.localRotation = Quaternion.identity;
        }
    }

    public void DebugFinalTrick(SwipeDirection direction)
    {
        if (!running || gameManager.State != RunState.FinalBonus || !IsDiagonal(direction))
        {
            return;
        }

        PerformDirectionalTrick(direction);
    }

    public void StartFinalBonus(float rampZ)
    {
        finalBonusActive = true;
        finalLaunchStarted = false;
        finalRampZ = rampZ;
        finalAirTimer = 0f;
        finalBonusTricks = 0;
        finalTrickCooldown = 0f;
        lane = 0;
        grinding = false;
        currentRail = null;
        flipping = true;
        flipTimer = 0f;
        forwardSpeed = baseForwardSpeed * finalBonusSpeedMultiplier;
    }

    private void OnSwipe(SwipeDirection direction)
    {
        if (!running)
        {
            return;
        }

        if (gameManager.State == RunState.FinalBonus)
        {
            PerformFinalBonusInput(direction);
            return;
        }

        if (gameManager.State != RunState.Running)
        {
            return;
        }

        if (grinding)
        {
            if (direction == SwipeDirection.Up || IsDiagonal(direction))
            {
                TrickOutOfGrind(direction);
            }

            return;
        }

        if (gestureInput != null &&
            (gestureInput.Scheme == ControlScheme.HoldDragSteer || gestureInput.Scheme == ControlScheme.PushAndFlick) &&
            (direction == SwipeDirection.Left || direction == SwipeDirection.Right || direction == SwipeDirection.Down))
        {
            return;
        }

        if (direction == SwipeDirection.Left)
        {
            MoveLane(-1);
        }
        else if (direction == SwipeDirection.Right)
        {
            MoveLane(1);
        }
        else if (direction == SwipeDirection.Up)
        {
            Jump();
            MarkManualEntryInput();
            gameManager.AddTrickScore("Ollie", 75);
        }
        else if (IsDiagonal(direction))
        {
            BufferGrindInput(direction, grindFlickBufferSeconds, true);
            PerformDirectionalTrick(direction);
        }
    }

    private void OnSwipeHoldStarted(SwipeDirection direction)
    {
        if (!running)
        {
            return;
        }

        if (gameManager.State == RunState.FinalBonus && IsDiagonal(direction))
        {
            PerformFinalBonusInput(direction);
            return;
        }

        if (gameManager.State != RunState.Running)
        {
            return;
        }

        if (grinding)
        {
            if (IsDiagonal(direction))
            {
                TrickOutOfGrind(direction);
            }

            return;
        }

        if (gestureInput != null && gestureInput.Scheme == ControlScheme.PushAndFlick)
        {
            if (!IsDiagonal(direction))
            {
                return;
            }

            GrindRail targetRail = nearbyRail != null ? nearbyRail : FindTargetRail(direction);
            if (targetRail != null)
            {
                TrickOntoRail(targetRail, direction);
            }
            else
            {
                BufferGrindInput(direction, queuedGrindSeconds, false);
            }

            return;
        }

        if (IsDiagonal(direction) && nearbyRail != null)
        {
            TrickOntoRail(nearbyRail, direction);
        }
        else if (IsDiagonal(direction))
        {
            GrindRail targetRail = FindTargetRail(direction);
            if (targetRail != null)
            {
                TrickOntoRail(targetRail, direction);
            }
            else
            {
                BufferGrindInput(direction, queuedGrindSeconds, true);
                PerformDirectionalTrick(direction);
            }
        }
    }

    private void OnSwipeHoldReleased()
    {
        if (!grinding)
        {
            return;
        }

        DropFromGrind();
    }

    private void OnLaneTargetChanged(int targetLane)
    {
        if (!running || grinding || finalBonusActive || gameManager.State != RunState.Running)
        {
            return;
        }

        int clampedLane = Mathf.Clamp(targetLane, -1, 1);
        if (clampedLane == lane)
        {
            return;
        }

        previousLane = lane;
        lane = clampedLane;
    }

    private void OnSteerTargetChanged(float normalizedX)
    {
        if (!running || grinding || finalBonusActive || gameManager.State != RunState.Running)
        {
            return;
        }

        steerTargetX = Mathf.Clamp(normalizedX, -1f, 1f) * roadHalfWidth;
        lane = Mathf.Clamp(Mathf.RoundToInt(steerTargetX / laneWidth), -1, 1);
    }

    private void OnPushChanged(bool isPushing)
    {
        if (!running || finalBonusActive || gameManager.State != RunState.Running)
        {
            return;
        }

        pushing = isPushing;
        if (!isPushing)
        {
            pushCarryUntil = Time.time + pushSpeedCarrySeconds;
        }
    }

    private void Jump()
    {
        float jumpBonus = gameManager.Upgrades != null ? gameManager.Upgrades.JumpBonus : 0f;
        if (controller.isGrounded || grinding)
        {
            verticalVelocity = jumpVelocity + jumpBonus;
            MarkManualEntryInput();
        }
    }

    private void MoveLane(int direction)
    {
        int nextLane = Mathf.Clamp(lane + direction, -1, 1);
        if (nextLane == lane)
        {
            return;
        }

        previousLane = lane;
        lane = nextLane;
    }

    private void StartGrind(GrindRail rail)
    {
        if (rail == null)
        {
            return;
        }

        pendingBumpRail = null;
        queuedGrindUntil = 0f;
        queuedGrindDirection = SwipeDirection.UpRight;
        queuedGrindTrickAlreadyPerformed = false;
        grinding = true;
        grindTimer = 0f;
        currentRail = rail;
        verticalVelocity = 0f;
        previousLane = lane;
        lane = Mathf.Clamp(Mathf.RoundToInt(rail.transform.position.x / laneWidth), -1, 1);
        steerTargetX = rail.transform.position.x;
        transform.position = new Vector3(rail.transform.position.x, rail.GrindHeight, transform.position.z);
        gameManager.AddTrickScore("Grind", 150);
    }

    private void DropFromGrind()
    {
        if (!grinding)
        {
            return;
        }

        grinding = false;
        currentRail = null;
        nearbyRail = null;
        grindTimer = 0f;
        queuedGrindUntil = 0f;
        queuedGrindTrickAlreadyPerformed = false;
        verticalVelocity = -1f;
        gameManager.ShowMessage("Dropped from rail");
    }

    private void TrickOutOfGrind(SwipeDirection direction)
    {
        if (!grinding)
        {
            return;
        }

        float finishedGrindTimer = grindTimer;
        grinding = false;
        currentRail = null;
        nearbyRail = null;
        grindTimer = 0f;
        float jumpBonus = gameManager.Upgrades != null ? gameManager.Upgrades.JumpBonus : 0f;
        verticalVelocity = jumpVelocity + jumpBonus;
        MarkManualEntryInput();

        if (direction == SwipeDirection.Up)
        {
            gameManager.AddTrickScore("Ollie out", Mathf.RoundToInt(120f + finishedGrindTimer * 100f));
            return;
        }

        if (IsDiagonal(direction))
        {
            PerformDirectionalTrick(direction);
            gameManager.AddTrickScore($"{TrickNameForDirection(direction)} out", Mathf.RoundToInt(160f + finishedGrindTimer * 120f));
        }
    }

    private void StartQueuedGrind(GrindRail rail)
    {
        if (rail == null)
        {
            return;
        }

        string trickName = TrickNameForDirection(queuedGrindDirection);
        if (!queuedGrindTrickAlreadyPerformed)
        {
            PerformDirectionalTrick(queuedGrindDirection);
        }

        StartGrind(rail);
        gameManager.ShowMessage($"{trickName} to grind");
    }

    private void TrickOntoRail(GrindRail rail, SwipeDirection direction)
    {
        if (rail == null || gameManager.State != RunState.Running)
        {
            return;
        }

        PerformDirectionalTrick(direction);
        StartGrind(rail);
        gameManager.ShowMessage($"{TrickNameForDirection(direction)} to grind");
    }

    private void BufferGrindInput(SwipeDirection direction, float seconds, bool trickAlreadyPerformed)
    {
        queuedGrindUntil = Time.time + seconds;
        queuedGrindDirection = direction;
        queuedGrindTrickAlreadyPerformed = trickAlreadyPerformed;
    }

    private GrindRail FindTargetRail(SwipeDirection direction)
    {
        GrindRail bestRail = null;
        float bestScore = float.MaxValue;
        IReadOnlyList<GrindRail> rails = GrindRail.ActiveRails;

        for (int i = 0; i < rails.Count; i++)
        {
            GrindRail rail = rails[i];
            if (rail == null || !rail.gameObject.activeInHierarchy)
            {
                continue;
            }

            float zDelta = rail.transform.position.z - transform.position.z;
            if (zDelta < -railTrickBehindDistance || zDelta > railTrickApproachDistance)
            {
                continue;
            }

            float xDelta = rail.transform.position.x - transform.position.x;
            if (Mathf.Abs(xDelta) > railSideReach)
            {
                continue;
            }

            if (!SwipePointsTowardRail(direction, xDelta))
            {
                continue;
            }

            float score = Mathf.Abs(xDelta) + Mathf.Max(0f, zDelta) * 0.22f;
            if (score < bestScore)
            {
                bestScore = score;
                bestRail = rail;
            }
        }

        return bestRail;
    }

    private void ProcessPendingRailBump()
    {
        if (pendingBumpRail == null || grinding || gameManager.State != RunState.Running)
        {
            return;
        }

        if (Time.time < pendingRailBumpTime)
        {
            return;
        }

        if (Time.time < railBumpCooldownUntil)
        {
            pendingBumpRail = null;
            return;
        }

        if (!controller.isGrounded && verticalVelocity > 0.5f)
        {
            return;
        }

        lane = previousLane;
        steerTargetX = lane * laneWidth;
        verticalVelocity = Mathf.Max(verticalVelocity, 2.5f);
        forwardSpeed = Mathf.Max(baseForwardSpeed * 0.85f, forwardSpeed - 1.25f);
        railBumpCooldownUntil = Time.time + railBumpCooldownSeconds;
        pendingBumpRail = null;
        gameManager.AddSketchyPressure("RAIL CLIP - SECURITY +1");
    }

    private void Kickflip()
    {
        StartBoardTrick("Kickflip", Vector3.forward, 360f, 200, true);
    }

    private void Heelflip()
    {
        StartBoardTrick("Heelflip", Vector3.forward, -360f, 220, true);
    }

    private void ShoveIt()
    {
        StartBoardTrick("Shove-it", Vector3.up, 360f, 240, true);
    }

    private void ThreeSixtyFlip()
    {
        StartBoardTrick("360 Flip", new Vector3(0.25f, 1f, 0.75f).normalized, 540f, 360, true);
    }

    private void StartBoardTrick(string trickName, Vector3 axis, float degrees, int points, bool jump)
    {
        if (finalBonusActive)
        {
            TryFinalBonusTrick(trickName, axis, degrees);
            return;
        }

        if (flipping)
        {
            return;
        }

        flipping = true;
        flipTimer = 0f;
        flipAxis = axis;
        flipDegrees = degrees;
        MarkManualEntryInput();

        if (jump)
        {
            Jump();
        }

        gameManager.AddTrickScore(trickName, points);
        AwardPendingFeatureTrick(trickName);
    }

    private void PerformDirectionalTrick(SwipeDirection direction)
    {
        if (direction == SwipeDirection.UpLeft)
        {
            Kickflip();
        }
        else if (direction == SwipeDirection.UpRight)
        {
            Heelflip();
        }
        else if (direction == SwipeDirection.DownLeft)
        {
            ShoveIt();
        }
        else if (direction == SwipeDirection.DownRight)
        {
            ThreeSixtyFlip();
        }
    }

    private static bool SwipePointsTowardRail(SwipeDirection direction, float xDelta)
    {
        if (Mathf.Abs(xDelta) < 0.45f)
        {
            return true;
        }

        bool railIsLeft = xDelta < 0f;
        return railIsLeft
            ? direction == SwipeDirection.UpLeft || direction == SwipeDirection.DownLeft
            : direction == SwipeDirection.UpRight || direction == SwipeDirection.DownRight;
    }

    private static string TrickNameForDirection(SwipeDirection direction)
    {
        return direction switch
        {
            SwipeDirection.UpLeft => "Kickflip",
            SwipeDirection.UpRight => "Heelflip",
            SwipeDirection.DownLeft => "Shove-it",
            SwipeDirection.DownRight => "360 Flip",
            _ => "Trick"
        };
    }

    private void PerformFinalBonusInput(SwipeDirection direction)
    {
        if (IsDiagonal(direction))
        {
            PerformDirectionalTrick(direction);
            return;
        }

        if (direction == SwipeDirection.Left)
        {
            TryFinalBonusTrick("Indy Air", Vector3.right, -180f);
        }
        else if (direction == SwipeDirection.Right)
        {
            TryFinalBonusTrick("Tail Grab", Vector3.right, 180f);
        }
        else if (direction == SwipeDirection.Up)
        {
            TryFinalBonusTrick("Rocket Air", Vector3.forward, 180f);
        }
        else if (direction == SwipeDirection.Down)
        {
            TryFinalBonusTrick("Method Air", Vector3.forward, -180f);
        }
    }

    private void UpdateFlipVisual()
    {
        if (!flipping || boardVisual == null)
        {
            return;
        }

        flipTimer += Time.deltaTime;
        float t = Mathf.Clamp01(flipTimer / flipSeconds);
        boardVisual.localRotation = Quaternion.AngleAxis(flipDegrees * t, flipAxis);

        if (t >= 1f)
        {
            flipping = false;
            boardVisual.localRotation = Quaternion.identity;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.TryGetComponent(out RunnerObstacle obstacle))
        {
            HitObstacle(obstacle);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finalBonusActive)
        {
            if (other.TryGetComponent(out CoinPickup finalCoin))
            {
                finalCoin.Collect(gameManager);
            }

            return;
        }

        if (other.TryGetComponent(out SkateFeature feature))
        {
            ActivateFeature(feature);
            return;
        }

        if (other.TryGetComponent(out RunnerObstacle obstacle))
        {
            HitObstacle(obstacle);
            return;
        }

        if (other.TryGetComponent(out GrindRail rail))
        {
            nearbyRail = rail;
            if (Time.time <= queuedGrindUntil)
            {
                StartQueuedGrind(rail);
            }
            else if (!grinding && gameManager.State == RunState.Running)
            {
                pendingBumpRail = rail;
                pendingRailBumpTime = Time.time + railBumpGraceSeconds;
            }
        }

        if (gameManager.State == RunState.Running && other.TryGetComponent(out CoinPickup coin))
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

        if (currentRail != null && other.gameObject == currentRail.gameObject)
        {
            DropFromGrind();
        }

        if (pendingBumpRail != null && other.gameObject == pendingBumpRail.gameObject)
        {
            pendingBumpRail = null;
        }
    }

    private static bool IsDiagonal(SwipeDirection direction)
    {
        return direction == SwipeDirection.UpLeft ||
               direction == SwipeDirection.UpRight ||
               direction == SwipeDirection.DownLeft ||
               direction == SwipeDirection.DownRight;
    }

    private void HitObstacle(RunnerObstacle obstacle)
    {
        if (hasFailed || obstacle.HasBeenHit || gameManager.State != RunState.Running)
        {
            return;
        }

        hasFailed = true;
        grinding = false;
        currentRail = null;
        obstacle.MarkHit();
        gameManager.FailRun();
    }

    private void HandleKeyboardDebugInput()
    {
        if (gameManager.State != RunState.Running)
        {
            return;
        }

        if (grinding)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                TrickOutOfGrind(SwipeDirection.Up);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                TrickOutOfGrind(SwipeDirection.UpRight);
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                TrickOutOfGrind(SwipeDirection.UpLeft);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                TrickOutOfGrind(SwipeDirection.DownLeft);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                TrickOutOfGrind(SwipeDirection.DownRight);
            }

            if (Input.GetKeyUp(KeyCode.G))
            {
                DropFromGrind();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveLane(1);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            Jump();
            MarkManualEntryInput();
            gameManager.AddTrickScore("Ollie", 75);
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q))
        {
            Kickflip();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ShoveIt();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            ThreeSixtyFlip();
        }

        if (Input.GetKeyDown(KeyCode.G) && nearbyRail != null)
        {
            StartGrind(nearbyRail);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            GrindRail targetRail = FindTargetRail(SwipeDirection.UpRight) ?? FindTargetRail(SwipeDirection.UpLeft);
            if (targetRail != null)
            {
                TrickOntoRail(targetRail, SwipeDirection.UpRight);
            }
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            OnSwipeHoldReleased();
        }
    }

    private void UpdateFinalBonus()
    {
        if (!finalBonusActive)
        {
            return;
        }

        if (finalTrickCooldown > 0f)
        {
            finalTrickCooldown -= Time.deltaTime;
        }

        if (!finalLaunchStarted && transform.position.z >= finalRampZ - 1.4f)
        {
            finalLaunchStarted = true;
            finalLaunchStartZ = transform.position.z;
            verticalVelocity = finalLaunchVelocity;
            flipping = true;
            flipTimer = 0f;
        }

        if (!finalLaunchStarted)
        {
            return;
        }

        finalAirTimer += Time.deltaTime;

        if (trickRoot != null)
        {
            trickRoot.localRotation = Quaternion.Euler(540f * finalAirTimer, 0f, 0f);
        }

        if (finalAirTimer > minimumFinalAirSeconds && controller.isGrounded)
        {
            float distance = transform.position.z - finalLaunchStartZ;
            finalBonusActive = false;
            finalLaunchStarted = false;
            if (trickRoot != null)
            {
                trickRoot.localRotation = Quaternion.identity;
            }

            gameManager.CompleteFinalBonus(distance);
        }
    }

    private void TryFinalBonusTrick(string trickName, Vector3 axis, float degrees)
    {
        if (!finalLaunchStarted || finalTrickCooldown > 0f)
        {
            return;
        }

        finalBonusTricks++;
        finalTrickCooldown = finalTrickCooldownSeconds;
        verticalVelocity += finalTrickBoost;
        forwardSpeed += finalTrickForwardBoost;
        flipping = true;
        flipTimer = 0f;
        flipAxis = axis;
        flipDegrees = degrees;
        gameManager.AddScore(125 * finalBonusTricks);
        gameManager.RecordFinalBonusTrickFeedback(trickName);
    }

    private void ActivateFeature(SkateFeature feature)
    {
        if (feature.Used || gameManager.State != RunState.Running)
        {
            return;
        }

        feature.MarkUsed();

        if (feature.FeatureType == SkateFeatureType.KickerRamp)
        {
            verticalVelocity = Mathf.Max(verticalVelocity, feature.LaunchVelocity);
            ArmFeatureTrick(feature, "Kicker launch");
        }
        else if (feature.FeatureType == SkateFeatureType.StairSet)
        {
            verticalVelocity = Mathf.Max(verticalVelocity, feature.LaunchVelocity);
            ArmFeatureTrick(feature, "Bounce stairs");
        }
        else if (feature.FeatureType == SkateFeatureType.ManualPad)
        {
            if (CanEnterManualPad())
            {
                manualing = true;
                manualTimer = manualPadSeconds;
                gameManager.AddTrickScore("Manual Pad", feature.BonusPoints);
                gameManager.RecordFeature();
            }
            else
            {
                MissManualPad();
            }
        }
        else if (feature.FeatureType == SkateFeatureType.NegativePickup)
        {
            HitNegativePickup(feature.gameObject.name);
        }
    }

    private bool CanEnterManualPad()
    {
        return flipping ||
               !controller.isGrounded ||
               verticalVelocity > 0.5f ||
               Time.time - lastManualEntryInputTime <= manualEntryGraceSeconds;
    }

    private void MissManualPad()
    {
        if (Time.time < manualMissCooldownUntil)
        {
            return;
        }

        manualing = false;
        int shoveDirection = transform.position.x >= 0f ? -1 : 1;
        lane = Mathf.Clamp(lane + shoveDirection, -1, 1);
        steerTargetX = lane * laneWidth;
        verticalVelocity = Mathf.Max(verticalVelocity, 2.8f);
        backwardVelocity = Mathf.Max(backwardVelocity, manualMissBackForce);
        sideVelocity = shoveDirection * manualMissSideForce;
        forwardSpeed = Mathf.Max(0f, forwardSpeed - 2.5f);
        manualMissCooldownUntil = Time.time + manualMissCooldownSeconds;
        gameManager.AddControlPenalty("MANUAL MISS - RECOVER", 75, 1);
    }

    private void HitNegativePickup(string pickupName)
    {
        if (Time.time < negativePickupCooldownUntil)
        {
            return;
        }

        manualing = false;
        verticalVelocity = Mathf.Max(verticalVelocity, 1.8f);
        forwardSpeed = Mathf.Max(baseForwardSpeed * 0.8f, forwardSpeed - 1.6f);
        negativePickupCooldownUntil = Time.time + negativePickupCooldownSeconds;
        gameManager.AddControlPenalty($"{pickupName.ToUpperInvariant()} - COMBO DOWN", 125, 2);
    }

    private void MarkManualEntryInput()
    {
        lastManualEntryInputTime = Time.time;
    }

    private void UpdateManual()
    {
        if (!manualing)
        {
            return;
        }

        manualTimer -= Time.deltaTime;
        gameManager.AddScore(Mathf.RoundToInt(35f * Time.deltaTime * gameManager.Combo));

        if (!flipping && boardVisual != null)
        {
            boardVisual.localRotation = Quaternion.Euler(-12f, 0f, 0f);
        }

        if (manualTimer <= 0f)
        {
            manualing = false;
            if (!flipping && boardVisual != null)
            {
                boardVisual.localRotation = Quaternion.identity;
            }
        }
    }

    private void ArmFeatureTrick(SkateFeature feature, string message)
    {
        pendingFeatureTrick = feature;
        featureTrickTimer = 1.1f;
        gameManager.ShowMessage($"{message}: swipe trick");
    }

    private void AwardPendingFeatureTrick(string trickName)
    {
        if (pendingFeatureTrick == null || featureTrickTimer <= 0f)
        {
            return;
        }

        string featureName = pendingFeatureTrick.FeatureType == SkateFeatureType.KickerRamp
            ? "Kicker"
            : "Bounce Stairs";
        gameManager.RecordFeature();
        gameManager.AddTrickScore($"{featureName} {trickName}", pendingFeatureTrick.BonusPoints);
        pendingFeatureTrick = null;
        featureTrickTimer = 0f;
    }

    private void LateUpdate()
    {
        if (featureTrickTimer <= 0f)
        {
            return;
        }

        featureTrickTimer -= Time.deltaTime;
        if (featureTrickTimer <= 0f)
        {
            pendingFeatureTrick = null;
        }
    }
}
