using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    private const float GrindRideHeightOffset = 0.5f;
    private const float KickerRideHeightOffset = 0.04f;

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
    [SerializeField] private float railEntryTriggerGraceSeconds = 0.12f;
    [SerializeField] private float railBumpCooldownSeconds = 0.85f;
    [SerializeField] private float railTrickApproachDistance = 17f;
    [SerializeField] private float railTrickBehindDistance = 5f;
    [SerializeField] private float railHeadOnMaxOffset = 0.78f;
    [SerializeField] private float manualHeadOnSideMargin = 0.45f;
    [SerializeField] private float manualTrickEntryWindowSeconds = 1.0f;
    [SerializeField] private float manualPadSeconds = 1.45f;
    [SerializeField] private float manualPadMinEntryVelocity = 1.2f;
    [SerializeField] private int manualTrickEntryBonusPoints = 120;
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
    private float manualBalance;
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
    private float railBumpCooldownUntil;
    private GrindRail pendingRailContact;
    private float pendingRailContactUntil;
    private GrindRail railEntryTarget;
    private float railEntryUntil;
    private RailEntryKicker activeRailKicker;
    private float manualTrickEntryUntil;
    private string manualEntryTrickName;
    private float manualMissCooldownUntil;
    private float negativePickupCooldownUntil;

    public int FinalBonusTrickCount => finalBonusTricks;
    public float CurrentFinalBonusDistance => finalLaunchStarted ? Mathf.Max(0f, transform.position.z - finalLaunchStartZ) : 0f;
    public float CurrentFinalBonusPower => finalLaunchStarted
        ? Mathf.Clamp01((verticalVelocity + finalLaunchVelocity * 0.2f) / (finalLaunchVelocity * 1.2f))
        : 0f;
    public bool IsManualing => manualing;
    public float ManualBalance => manualBalance;

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
        UpdateRailKickerRide();
        UpdateGrindRide();

        backwardVelocity = Mathf.MoveTowards(backwardVelocity, 0f, shoveRecovery * Time.deltaTime);
        sideVelocity = Mathf.MoveTowards(sideVelocity, 0f, shoveRecovery * Time.deltaTime);
        UpdateFinalBonus();
        UpdateManual();
        UpdateFlipVisual();
        HandleKeyboardDebugInput();
        ResolvePendingRailContact();
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
        manualBalance = 0.5f;
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
        railBumpCooldownUntil = 0f;
        pendingRailContact = null;
        pendingRailContactUntil = 0f;
        railEntryTarget = null;
        railEntryUntil = 0f;
        activeRailKicker = null;
        manualTrickEntryUntil = 0f;
        manualEntryTrickName = null;
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
        manualBalance = 0.5f;
        flipping = false;
        currentRail = null;
        queuedGrindUntil = 0f;
        queuedGrindDirection = SwipeDirection.UpRight;
        queuedGrindTrickAlreadyPerformed = false;
        railEntryTarget = null;
        railEntryUntil = 0f;
        activeRailKicker = null;
        pendingRailContact = null;
        pendingRailContactUntil = 0f;
        manualTrickEntryUntil = 0f;
        manualEntryTrickName = null;
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
        nearbyRail = null;
        railEntryTarget = null;
        railEntryUntil = 0f;
        activeRailKicker = null;
        pendingRailContact = null;
        pendingRailContactUntil = 0f;
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
            ArmManualTrickEntry("Ollie");
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
            return;
        }

        if (IsDiagonal(direction) && nearbyRail != null)
        {
            if (CanEnterRailFromKicker(nearbyRail))
            {
                TrickOntoRail(nearbyRail, direction);
            }
            else
            {
                BufferGrindInput(direction, queuedGrindSeconds, true);
                PerformDirectionalTrick(direction);
            }
        }
        else if (IsDiagonal(direction))
        {
            GrindRail targetRail = FindTargetRail(direction);
            if (targetRail != null && CanEnterRailFromKicker(targetRail))
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

        queuedGrindUntil = 0f;
        queuedGrindDirection = SwipeDirection.UpRight;
        queuedGrindTrickAlreadyPerformed = false;
        railEntryTarget = null;
        railEntryUntil = 0f;
        pendingRailContact = null;
        pendingRailContactUntil = 0f;
        grinding = true;
        grindTimer = 0f;
        currentRail = rail;
        verticalVelocity = 0f;
        flipping = false;
        flipTimer = 0f;
        if (boardVisual != null)
        {
            boardVisual.localRotation = Quaternion.identity;
        }
        previousLane = lane;
        lane = Mathf.Clamp(Mathf.RoundToInt(rail.transform.position.x / laneWidth), -1, 1);
        steerTargetX = rail.transform.position.x;
        AlignBoardToRail(rail);
        gameManager.AddTrickScore("Grind", 150);
    }

    private void AlignBoardToRail(GrindRail rail)
    {
        Vector3 alignedPosition = transform.position;
        alignedPosition.x = rail.transform.position.x;

        if (boardVisual != null && boardVisual.TryGetComponent(out Renderer boardRenderer))
        {
            float boardBottomY = boardRenderer.bounds.min.y;
            alignedPosition.y += rail.SurfaceHeight - boardBottomY + GrindRideHeightOffset;
        }
        else
        {
            alignedPosition.y = rail.SurfaceHeight;
        }

        transform.position = alignedPosition;
    }

    private void UpdateGrindRide()
    {
        if (!grinding)
        {
            return;
        }

        if (currentRail == null || !currentRail.gameObject.activeInHierarchy)
        {
            DropFromGrind();
            return;
        }

        AlignBoardToRail(currentRail);

        // A rail's trigger is deliberately thin and the board rides above it, so
        // trigger exit cannot define the grind lifetime. Finish only at the rail end.
        if (transform.position.z >= currentRail.EndZ)
        {
            DropFromGrind();
        }
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
        if (rail == null || !CanEnterRailFromKicker(rail))
        {
            ClipRail();
            return;
        }

        string trickName = TrickNameForDirection(queuedGrindDirection);
        if (!queuedGrindTrickAlreadyPerformed)
        {
            PerformDirectionalTrick(queuedGrindDirection);
        }

        StartGrind(rail);
        gameManager.AddTrickScore("Kicker to grind", rail.EntryKickerBonusPoints);
        gameManager.ShowMessage($"{trickName} to grind");
    }

    private void TrickOntoRail(GrindRail rail, SwipeDirection direction)
    {
        if (rail == null || gameManager.State != RunState.Running)
        {
            return;
        }

        if (!CanEnterRailFromKicker(rail))
        {
            ClipRail();
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

    private void EnterRailKicker(RailEntryKicker kicker)
    {
        GrindRail rail = kicker.Rail != null ? kicker.Rail : kicker.GetComponentInParent<GrindRail>();
        if (rail == null || gameManager.State != RunState.Running || grinding)
        {
            return;
        }

        nearbyRail = rail;

        if (activeRailKicker == kicker)
        {
            return;
        }

        if (!IsHeadOnRail(rail))
        {
            ClipRail();
            return;
        }

        activeRailKicker = kicker;
        railEntryTarget = null;
        railEntryUntil = 0f;
        pendingRailContact = null;
        pendingRailContactUntil = 0f;
        verticalVelocity = 0f;
        lane = Mathf.Clamp(Mathf.RoundToInt(rail.transform.position.x / laneWidth), -1, 1);
        steerTargetX = rail.transform.position.x;
    }

    private void ExitRailKicker(RailEntryKicker kicker)
    {
        if (kicker == null || activeRailKicker != kicker)
        {
            return;
        }

        GrindRail rail = kicker.Rail != null ? kicker.Rail : kicker.GetComponentInParent<GrindRail>();
        activeRailKicker = null;
        if (rail == null)
        {
            return;
        }

        railEntryTarget = rail;
        railEntryUntil = Time.time + rail.EntryKickerWindowSeconds;
        verticalVelocity = Mathf.Max(verticalVelocity, rail.EntryKickerLaunchVelocity);
        gameManager.ShowMessage("Kicker to rail");
    }

    private void UpdateRailKickerRide()
    {
        if (activeRailKicker == null || grinding)
        {
            return;
        }

        verticalVelocity = 0f;
        Vector3 alignedPosition = transform.position;
        if (boardVisual != null && boardVisual.TryGetComponent(out Renderer boardRenderer))
        {
            alignedPosition.y += activeRailKicker.SurfaceHeightAt(transform.position) -
                boardRenderer.bounds.min.y + KickerRideHeightOffset;
        }
        else
        {
            alignedPosition.y = activeRailKicker.SurfaceHeightAt(transform.position);
        }

        transform.position = alignedPosition;
    }

    private bool CanEnterRailFromKicker(GrindRail rail)
    {
        return rail != null &&
            railEntryTarget == rail &&
            Time.time <= railEntryUntil &&
            IsHeadOnRail(rail);
    }

    private void StartRailEntryGrind(GrindRail rail)
    {
        if (!CanEnterRailFromKicker(rail))
        {
            ClipRail();
            return;
        }

        railEntryTarget = null;
        railEntryUntil = 0f;
        StartGrind(rail);
        gameManager.AddTrickScore("Kicker to grind", rail.EntryKickerBonusPoints);
        gameManager.ShowMessage("Kicker to grind");
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
            if (Mathf.Abs(xDelta) > railHeadOnMaxOffset)
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

    private void ClipRail()
    {
        if (Time.time < railBumpCooldownUntil || grinding || gameManager.State != RunState.Running)
        {
            return;
        }

        lane = previousLane;
        steerTargetX = lane * laneWidth;
        verticalVelocity = Mathf.Max(verticalVelocity, 2.5f);
        backwardVelocity = Mathf.Max(backwardVelocity, manualMissBackForce * 0.65f);
        sideVelocity = transform.position.x >= 0f ? -manualMissSideForce : manualMissSideForce;
        forwardSpeed = Mathf.Max(baseForwardSpeed * 0.85f, forwardSpeed - 1.25f);
        railBumpCooldownUntil = Time.time + railBumpCooldownSeconds;
        gameManager.AddSketchyPressure("RAIL CLIP - SECURITY +1");
    }

    private void QueueRailContact(GrindRail rail)
    {
        if (rail == null || grinding || pendingRailContact == rail)
        {
            return;
        }

        pendingRailContact = rail;
        pendingRailContactUntil = Time.time + railEntryTriggerGraceSeconds;
    }

    private void ResolvePendingRailContact()
    {
        if (pendingRailContact == null || grinding || Time.time < pendingRailContactUntil)
        {
            return;
        }

        GrindRail rail = pendingRailContact;
        pendingRailContact = null;
        pendingRailContactUntil = 0f;

        if (CanEnterRailFromKicker(rail))
        {
            StartRailEntryGrind(rail);
        }
        else
        {
            ClipRail();
        }
    }

    private bool IsHeadOnRail(GrindRail rail)
    {
        return rail != null && Mathf.Abs(transform.position.x - rail.transform.position.x) <= railHeadOnMaxOffset;
    }

    private bool IsHeadOnFeature(SkateFeature feature)
    {
        if (feature == null)
        {
            return false;
        }

        float halfWidth = Mathf.Abs(feature.transform.lossyScale.x) * 0.5f;
        float cleanEntryWidth = Mathf.Max(0.45f, halfWidth - manualHeadOnSideMargin);
        return Mathf.Abs(transform.position.x - feature.transform.position.x) <= cleanEntryWidth;
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
        ArmManualTrickEntry(trickName);

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

        if (other.TryGetComponent(out RailEntryKicker kicker))
        {
            EnterRailKicker(kicker);
            return;
        }

        GrindRail rail = other.GetComponentInParent<GrindRail>();
        if (rail != null)
        {
            nearbyRail = rail;
            if (Time.time <= queuedGrindUntil && CanEnterRailFromKicker(rail))
            {
                StartQueuedGrind(rail);
            }
            else if (CanEnterRailFromKicker(rail))
            {
                StartRailEntryGrind(rail);
            }
            else if (!grinding && gameManager.State == RunState.Running)
            {
                QueueRailContact(rail);
            }
        }

        if (gameManager.State == RunState.Running && other.TryGetComponent(out CoinPickup coin))
        {
            coin.Collect(gameManager);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (finalBonusActive || gameManager.State != RunState.Running)
        {
            return;
        }

        if (other.TryGetComponent(out SkateFeature feature) &&
            feature.FeatureType == SkateFeatureType.ManualPad &&
            !feature.Used)
        {
            ActivateFeature(feature);
            return;
        }

        if (other.TryGetComponent(out RailEntryKicker kicker))
        {
            EnterRailKicker(kicker);
            return;
        }

        GrindRail rail = other.GetComponentInParent<GrindRail>();
        if (rail != null)
        {
            nearbyRail = rail;
            if (Time.time <= queuedGrindUntil && CanEnterRailFromKicker(rail))
            {
                StartQueuedGrind(rail);
            }
            else if (CanEnterRailFromKicker(rail))
            {
                StartRailEntryGrind(rail);
            }
            else if (!grinding)
            {
                QueueRailContact(rail);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out RailEntryKicker kicker))
        {
            ExitRailKicker(kicker);
            return;
        }

        GrindRail rail = other.GetComponentInParent<GrindRail>();

        if (nearbyRail != null && rail == nearbyRail)
        {
            nearbyRail = null;
        }

        if (pendingRailContact != null && rail == pendingRailContact)
        {
            GrindRail missedRail = pendingRailContact;
            pendingRailContact = null;
            pendingRailContactUntil = 0f;
            if (!CanEnterRailFromKicker(missedRail))
            {
                ClipRail();
            }
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
            ArmManualTrickEntry("Ollie");
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
            if (CanEnterManualPad(feature))
            {
                string entryName = string.IsNullOrEmpty(manualEntryTrickName) ? "Trick" : manualEntryTrickName;
                manualing = true;
                float padTravelSeconds = Mathf.Abs(feature.transform.lossyScale.z) / Mathf.Max(1f, forwardSpeed);
                manualTimer = Mathf.Max(manualPadSeconds, padTravelSeconds + 0.15f);
                manualBalance = 0.5f;
                manualTrickEntryUntil = 0f;
                manualEntryTrickName = null;
                gameManager.AddTrickScore($"{entryName} to Manual", feature.BonusPoints + manualTrickEntryBonusPoints);
                gameManager.RecordFeature();
                gameManager.ShowMessage($"{entryName} to Manual");
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

    private bool CanEnterManualPad(SkateFeature feature)
    {
        return Time.time <= manualTrickEntryUntil && IsHeadOnFeature(feature) && HasManualPadEntryMotion();
    }

    private bool HasManualPadEntryMotion()
    {
        return !controller.isGrounded || verticalVelocity >= manualPadMinEntryVelocity;
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
        manualTrickEntryUntil = 0f;
        manualEntryTrickName = null;
        manualMissCooldownUntil = Time.time + manualMissCooldownSeconds;
        gameManager.AddSketchyPressure("MANNY PAD BUMP - SECURITY +1");
    }

    private void ArmManualTrickEntry(string trickName)
    {
        manualTrickEntryUntil = Time.time + manualTrickEntryWindowSeconds;
        manualEntryTrickName = trickName;
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

    private void UpdateManual()
    {
        if (!manualing)
        {
            return;
        }

        manualTimer -= Time.deltaTime;
        manualBalance = Mathf.PingPong(Time.time * 0.9f, 1f);
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
