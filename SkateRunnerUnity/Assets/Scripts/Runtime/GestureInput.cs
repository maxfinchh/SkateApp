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

public sealed class GestureInput : MonoBehaviour
{
    [SerializeField] private float minSwipePixels = 60f;
    [SerializeField] private float holdSeconds = 0.18f;
    [SerializeField] private float diagonalBias = 0.45f;

    public event Action<SwipeDirection> Swipe;
    public event Action<SwipeDirection> SwipeHoldStarted;
    public event Action SwipeHoldReleased;

    private bool tracking;
    private bool holdStarted;
    private float startTime;
    private Vector2 startPosition;
    private Vector2 latestPosition;
    private SwipeDirection heldDirection;

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
    }

    private void Move(Vector2 screenPosition)
    {
        if (!tracking)
        {
            return;
        }

        latestPosition = screenPosition;
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

    private static bool IsDiagonal(SwipeDirection direction)
    {
        return direction == SwipeDirection.UpLeft ||
               direction == SwipeDirection.UpRight ||
               direction == SwipeDirection.DownLeft ||
               direction == SwipeDirection.DownRight;
    }
}
