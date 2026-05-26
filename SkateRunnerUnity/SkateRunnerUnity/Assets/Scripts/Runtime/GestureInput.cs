using System;
using UnityEngine;

public enum SwipeDirection
{
    Left,
    Right,
    Up,
    Down,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public enum ControlScheme
{
    PushAndFlick,
    HoldDragSteer,
    SwipeLane
}

public sealed class GestureInput : MonoBehaviour
{
    [SerializeField] private ControlScheme controlScheme = ControlScheme.PushAndFlick;
    [SerializeField] private float minSwipePixels = 60f;
    [SerializeField] private float holdSeconds = 0.18f;
    [SerializeField] private float diagonalBias = 0.45f;
    [SerializeField] private float holdDragFlickCooldownSeconds = 0.18f;
    [SerializeField] private float pushStartHoldSeconds = 0.14f;
    [SerializeField] private float pushSwipeGraceSeconds = 0.36f;
    [SerializeField] private float pushFlickMaxSeconds = 0.46f;
    [SerializeField] private float pushSteerScreenSensitivity = 1.65f;
    [SerializeField] private float pushOllieWidthRatio = 0.32f;

    public event Action<SwipeDirection> Swipe;
    public event Action<SwipeDirection> SwipeHoldStarted;
    public event Action SwipeHoldReleased;
    public event Action<int> LaneTargetChanged;
    public event Action<float> SteerTargetChanged;
    public event Action<bool> PushChanged;

    private bool tracking;
    private bool holdStarted;
    private float startTime;
    private Vector2 startPosition;
    private Vector2 latestPosition;
    private Vector2 flickOrigin;
    private float lastFlickTime = -999f;
    private int lastLaneTarget = int.MinValue;
    private bool flickEmittedDuringHold;
    private bool pushActive;
    private SwipeDirection heldDirection;

    public ControlScheme Scheme => controlScheme;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Begin(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                Move(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                End(touch.position);
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Begin(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            Move(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            End(Input.mousePosition);
        }
    }

    private void Begin(Vector2 screenPosition)
    {
        tracking = true;
        holdStarted = false;
        startTime = Time.unscaledTime;
        startPosition = screenPosition;
        latestPosition = screenPosition;
        flickOrigin = screenPosition;
        lastLaneTarget = int.MinValue;
        flickEmittedDuringHold = false;
        pushActive = false;

        if (controlScheme == ControlScheme.PushAndFlick)
        {
            return;
        }
        else if (controlScheme == ControlScheme.HoldDragSteer)
        {
            EmitLaneTarget(screenPosition);
        }
    }

    private void Move(Vector2 screenPosition)
    {
        if (!tracking)
        {
            return;
        }

        latestPosition = screenPosition;
        if (controlScheme == ControlScheme.PushAndFlick)
        {
            MovePushAndFlick(screenPosition);
            return;
        }

        if (controlScheme == ControlScheme.HoldDragSteer)
        {
            MoveHoldDrag(screenPosition);
            return;
        }

        Vector2 delta = latestPosition - startPosition;
        if (holdStarted || delta.magnitude < minSwipePixels || Time.unscaledTime - startTime < holdSeconds)
        {
            return;
        }

        heldDirection = ResolveDirection(delta);
        if (IsDiagonal(heldDirection))
        {
            holdStarted = true;
            SwipeHoldStarted?.Invoke(heldDirection);
        }
    }

    private void End(Vector2 screenPosition)
    {
        if (!tracking)
        {
            return;
        }

        latestPosition = screenPosition;
        Vector2 delta = latestPosition - startPosition;

        if (controlScheme == ControlScheme.PushAndFlick)
        {
            if (pushActive)
            {
                PushChanged?.Invoke(false);
            }

            EndPushAndFlick(delta);
            tracking = false;
            holdStarted = false;
            pushActive = false;
            return;
        }

        if (controlScheme == ControlScheme.HoldDragSteer)
        {
            EndFlickOrHold(delta);
            tracking = false;
            holdStarted = false;
            return;
        }

        if (holdStarted)
        {
            SwipeHoldReleased?.Invoke();
        }
        else if (delta.magnitude >= minSwipePixels)
        {
            Swipe?.Invoke(ResolveDirection(delta));
        }

        tracking = false;
        holdStarted = false;
    }

    private void MovePushAndFlick(Vector2 screenPosition)
    {
        if (!pushActive)
        {
            Vector2 delta = screenPosition - startPosition;
            float touchSeconds = Time.unscaledTime - startTime;
            bool likelyVerticalTrickSwipe = delta.magnitude >= minSwipePixels &&
                                           Mathf.Abs(delta.y) >= Mathf.Abs(delta.x) * 0.55f;

            if (touchSeconds < pushStartHoldSeconds ||
                (likelyVerticalTrickSwipe && touchSeconds < pushSwipeGraceSeconds))
            {
                return;
            }

            pushActive = true;
            PushChanged?.Invoke(true);
        }

        EmitPushRailIntentIfReady(screenPosition);
        EmitSteerTarget(screenPosition);
    }

    private void EmitPushRailIntentIfReady(Vector2 screenPosition)
    {
        Vector2 flickDelta = screenPosition - flickOrigin;
        if (flickDelta.magnitude < minSwipePixels || Time.unscaledTime - lastFlickTime < holdDragFlickCooldownSeconds)
        {
            return;
        }

        SwipeDirection direction = ResolvePushFlickDirection(flickDelta);
        if (!IsDiagonal(direction))
        {
            return;
        }

        SwipeHoldStarted?.Invoke(direction);
        flickOrigin = screenPosition;
        lastFlickTime = Time.unscaledTime;
    }

    private void MoveHoldDrag(Vector2 screenPosition)
    {
        EmitLaneTarget(screenPosition);
        EmitFlickIfReady(screenPosition);
    }

    private void EmitFlickIfReady(Vector2 screenPosition)
    {
        Vector2 flickDelta = screenPosition - flickOrigin;
        if (flickDelta.magnitude < minSwipePixels || Time.unscaledTime - lastFlickTime < holdDragFlickCooldownSeconds)
        {
            return;
        }

        SwipeDirection direction = ResolveDirection(flickDelta);
        if (IsDiagonal(direction))
        {
            holdStarted = true;
            flickEmittedDuringHold = true;
            SwipeHoldStarted?.Invoke(direction);
            flickOrigin = screenPosition;
            lastFlickTime = Time.unscaledTime;
            return;
        }

        flickEmittedDuringHold = true;
        Swipe?.Invoke(direction);
        flickOrigin = screenPosition;
        lastFlickTime = Time.unscaledTime;
    }

    private void EndFlickOrHold(Vector2 delta)
    {
        if (holdStarted)
        {
            SwipeHoldReleased?.Invoke();
        }
        else if (!flickEmittedDuringHold && delta.magnitude >= minSwipePixels)
        {
            Swipe?.Invoke(ResolveDirection(delta));
        }
    }

    private void EndPushAndFlick(Vector2 delta)
    {
        float touchSeconds = Time.unscaledTime - startTime;
        if (!pushActive && touchSeconds <= pushFlickMaxSeconds && delta.magnitude >= minSwipePixels)
        {
            Swipe?.Invoke(ResolvePushFlickDirection(delta));
        }
    }

    private void EmitSteerTarget(Vector2 screenPosition)
    {
        float width = Mathf.Max(1f, Screen.width);
        float normalizedX = Mathf.Clamp01(screenPosition.x / width);
        float centered = (normalizedX - 0.5f) * 2f * pushSteerScreenSensitivity;
        SteerTargetChanged?.Invoke(Mathf.Clamp(centered, -1f, 1f));
    }

    private void EmitLaneTarget(Vector2 screenPosition)
    {
        float width = Mathf.Max(1f, Screen.width);
        float normalizedX = Mathf.Clamp01(screenPosition.x / width);
        int targetLane = normalizedX < 0.34f ? -1 : normalizedX > 0.66f ? 1 : 0;

        if (targetLane == lastLaneTarget)
        {
            return;
        }

        lastLaneTarget = targetLane;
        LaneTargetChanged?.Invoke(targetLane);
    }

    private SwipeDirection ResolveDirection(Vector2 delta)
    {
        Vector2 normal = delta.normalized;
        bool horizontal = Mathf.Abs(normal.x) > diagonalBias;
        bool vertical = Mathf.Abs(normal.y) > diagonalBias;

        if (horizontal && vertical)
        {
            if (normal.y > 0f)
            {
                return normal.x < 0f ? SwipeDirection.UpLeft : SwipeDirection.UpRight;
            }

            return normal.x < 0f ? SwipeDirection.DownLeft : SwipeDirection.DownRight;
        }

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return delta.x < 0f ? SwipeDirection.Left : SwipeDirection.Right;
        }

        return delta.y < 0f ? SwipeDirection.Down : SwipeDirection.Up;
    }

    private SwipeDirection ResolvePushFlickDirection(Vector2 delta)
    {
        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);
        bool mostlyHorizontal = absX > absY * 1.65f && absY < minSwipePixels * 0.75f;

        if (mostlyHorizontal)
        {
            return delta.x < 0f ? SwipeDirection.Left : SwipeDirection.Right;
        }

        if (delta.y > 0f && absY >= minSwipePixels * 0.45f)
        {
            if (absX <= absY * pushOllieWidthRatio)
            {
                return SwipeDirection.Up;
            }

            return delta.x < 0f ? SwipeDirection.UpLeft : SwipeDirection.UpRight;
        }

        if (delta.y < 0f && absY >= minSwipePixels * 0.45f)
        {
            if (absX <= absY * pushOllieWidthRatio)
            {
                return SwipeDirection.Down;
            }

            return delta.x < 0f ? SwipeDirection.DownLeft : SwipeDirection.DownRight;
        }

        return delta.x < 0f ? SwipeDirection.Left : SwipeDirection.Right;
    }

    private static bool IsDiagonal(SwipeDirection direction)
    {
        return direction == SwipeDirection.UpLeft ||
               direction == SwipeDirection.UpRight ||
               direction == SwipeDirection.DownLeft ||
               direction == SwipeDirection.DownRight;
    }
}
