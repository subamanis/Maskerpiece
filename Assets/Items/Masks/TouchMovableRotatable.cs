using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Selectable))]
public class TouchMovableRotatable : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float scaleSpeed = 1f;

    private Vector2 moveOffset;
    private bool isMoving;
    private bool isRotating;
    private float lastRotationAngle;
    private float lastPinchDistance;
    private float cachedZ;
    private int lastTouchCount;
    private Selectable selectable;
    private Collider2D cachedCollider;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;
    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private const float RestEpsilon = 0.0001f;

    private enum TouchState
    {
        Began,
        Moved,
        Stationary,
        Ended,
        Canceled
    }

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        cachedCollider = GetComponent<Collider2D>();
        CacheTransformState();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            return;
        }

        if (selectable == null || !selectable.IsSelected)
        {
            isMoving = false;
            isRotating = false;
            lastTouchCount = 0;
            CacheTransformState();
            return;
        }

        int touchCount = GetActiveTouches(out TouchInfo touchA, out TouchInfo touchB);

        if (lastTouchCount >= 2 && touchCount == 1)
        {
            BeginMove(touchA);
        }

        if (touchCount == 1)
        {
            HandleMove(touchA);
        }
        else if (touchCount >= 2)
        {
            HandleRotation(touchA, touchB);
        }
        else
        {
            isMoving = false;
            isRotating = false;
        }

        bool isTransformStable = IsTransformStable();
        if (touchCount == 0 && !isMoving && !isRotating && isTransformStable)
        {
            LogCollisionCheck();
        }

        lastTouchCount = touchCount;
        CacheTransformState();
    }

    private void HandleMove(TouchInfo touch)
    {
        if (touch.phase == TouchState.Began)
        {
            BeginMove(touch);
        }

        if (!isMoving)
        {
            return;
        }

        if (touch.phase == TouchState.Moved || touch.phase == TouchState.Stationary)
        {
            Vector2 hitPoint = ScreenToWorld2D(touch.position);
            Vector2 newPosition = hitPoint + moveOffset;
            transform.position = new Vector3(newPosition.x, newPosition.y, cachedZ);
        }
        else if (touch.phase == TouchState.Ended || touch.phase == TouchState.Canceled)
        {
            isMoving = false;
        }
    }

    private void HandleRotation(TouchInfo touchA, TouchInfo touchB)
    {
        if (!isRotating || touchA.phase == TouchState.Began || touchB.phase == TouchState.Began)
        {
            lastRotationAngle = GetAngleBetweenTouches(touchA.position, touchB.position);
            lastPinchDistance = Vector2.Distance(touchA.position, touchB.position);
            isRotating = true;
            isMoving = false;
            return;
        }

        if (touchA.phase == TouchState.Moved || touchB.phase == TouchState.Moved)
        {
            float currentAngle = GetAngleBetweenTouches(touchA.position, touchB.position);
            float deltaAngle = Mathf.DeltaAngle(lastRotationAngle, currentAngle);
            transform.Rotate(0f, 0f, deltaAngle * rotationSpeed, Space.World);
            lastRotationAngle = currentAngle;

            float currentDistance = Vector2.Distance(touchA.position, touchB.position);
            if (!Mathf.Approximately(lastPinchDistance, 0f))
            {
                float scaleFactor = (currentDistance / lastPinchDistance) * scaleSpeed;
                Vector3 newScale = transform.localScale * scaleFactor;
                transform.localScale = new Vector3(newScale.x, newScale.y, transform.localScale.z);
            }
            lastPinchDistance = currentDistance;
        }

        if (touchA.phase == TouchState.Ended || touchA.phase == TouchState.Canceled ||
            touchB.phase == TouchState.Ended || touchB.phase == TouchState.Canceled)
        {
            isRotating = false;
        }
    }

    private Vector2 ScreenToWorld2D(Vector2 screenPosition)
    {
        Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, cachedZ - targetCamera.transform.position.z));
        return new Vector2(world.x, world.y);
    }

    private void BeginMove(TouchInfo touch)
    {
        isRotating = false;
        cachedZ = transform.position.z;
        Vector2 hitPoint = ScreenToWorld2D(touch.position);
        moveOffset = (Vector2)transform.position - hitPoint;
        isMoving = true;
    }

    private static float GetAngleBetweenTouches(Vector2 a, Vector2 b)
    {
        Vector2 direction = b - a;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private struct TouchInfo
    {
        public Vector2 position;
        public TouchState phase;

        public TouchInfo(Vector2 position, TouchState phase)
        {
            this.position = position;
            this.phase = phase;
        }
    }

    private int GetActiveTouches(out TouchInfo touchA, out TouchInfo touchB)
    {
        touchA = default;
        touchB = default;

        if (Touchscreen.current == null)
        {
            return 0;
        }

        int count = 0;
        foreach (var touchControl in Touchscreen.current.touches)
        {
            if (!touchControl.press.isPressed)
            {
                continue;
            }

            TouchState phase = ConvertInputSystemPhase(touchControl.phase.ReadValue());
            Vector2 position = touchControl.position.ReadValue();
            if (count == 0)
            {
                touchA = new TouchInfo(position, phase);
            }
            else if (count == 1)
            {
                touchB = new TouchInfo(position, phase);
            }
            count++;
            if (count >= 2)
            {
                break;
            }
        }

        return count;
    }

    private static TouchState ConvertInputSystemPhase(UnityEngine.InputSystem.TouchPhase phase)
    {
        switch (phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Began:
                return TouchState.Began;
            case UnityEngine.InputSystem.TouchPhase.Moved:
                return TouchState.Moved;
            case UnityEngine.InputSystem.TouchPhase.Stationary:
                return TouchState.Stationary;
            case UnityEngine.InputSystem.TouchPhase.Ended:
                return TouchState.Ended;
            case UnityEngine.InputSystem.TouchPhase.Canceled:
                return TouchState.Canceled;
            default:
                return TouchState.Stationary;
        }
    }

    private void CacheTransformState()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastScale = transform.localScale;
    }

    private bool IsTransformStable()
    {
        float epsilonSqr = RestEpsilon * RestEpsilon;
        if (Vector3.SqrMagnitude(transform.position - lastPosition) > epsilonSqr)
        {
            return false;
        }

        if (Quaternion.Angle(transform.rotation, lastRotation) > RestEpsilon)
        {
            return false;
        }

        if (Vector3.SqrMagnitude(transform.localScale - lastScale) > epsilonSqr)
        {
            return false;
        }

        return true;
    }

    private void LogCollisionCheck()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        if (cachedCollider == null)
        {
            Debug.LogWarning($"[{timestamp}] No Collider2D found for {name}.");
            return;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(Physics2D.AllLayers);
        filter.SetDepth(float.NegativeInfinity, float.PositiveInfinity);
        int count = cachedCollider.Overlap(filter, overlapResults);
        var collisions = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = overlapResults[i];
            if (hit == null || hit == cachedCollider)
            {
                continue;
            }

            Selectable otherSelectable = hit.GetComponentInParent<Selectable>();
            if (otherSelectable != null && otherSelectable != selectable)
            {
                collisions.Add(otherSelectable.name);
            }
        }

        bool hasCollision = collisions.Count > 0;
        bool? previousCollision = selectable.LastRestCollision;
        selectable.NotifyRestCollisionChanged(hasCollision);

        if (!previousCollision.HasValue || previousCollision.Value != hasCollision)
        {
            if (hasCollision)
            {
                Debug.Log($"[{timestamp}] Collisions: {string.Join(", ", collisions)}");
            }
            else
            {
                Debug.Log($"[{timestamp}] No collisions.");
            }
        }
    }
}
