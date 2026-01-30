using UnityEngine;

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

    private void Awake()
    {
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

        if (lastTouchCount >= 2 && Input.touchCount == 1)
        {
            BeginMove(Input.GetTouch(0));
        }

        if (Input.touchCount == 1)
        {
            HandleMove(Input.GetTouch(0));
        }
        else if (Input.touchCount >= 2)
        {
            HandleRotation(Input.GetTouch(0), Input.GetTouch(1));
        }
        else
        {
            isMoving = false;
            isRotating = false;
        }

        lastTouchCount = Input.touchCount;
    }

    private void HandleMove(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            BeginMove(touch);
        }

        if (!isMoving)
        {
            return;
        }

        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            Vector2 hitPoint = ScreenToWorld2D(touch.position);
            Vector2 newPosition = hitPoint + moveOffset;
            transform.position = new Vector3(newPosition.x, newPosition.y, cachedZ);
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isMoving = false;
        }
    }

    private void HandleRotation(Touch touchA, Touch touchB)
    {
        if (!isRotating || touchA.phase == TouchPhase.Began || touchB.phase == TouchPhase.Began)
        {
            lastRotationAngle = GetAngleBetweenTouches(touchA.position, touchB.position);
            lastPinchDistance = Vector2.Distance(touchA.position, touchB.position);
            isRotating = true;
            isMoving = false;
            return;
        }

        if (touchA.phase == TouchPhase.Moved || touchB.phase == TouchPhase.Moved)
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

        if (touchA.phase == TouchPhase.Ended || touchA.phase == TouchPhase.Canceled ||
            touchB.phase == TouchPhase.Ended || touchB.phase == TouchPhase.Canceled)
        {
            isRotating = false;
        }
    }

    private Vector2 ScreenToWorld2D(Vector2 screenPosition)
    {
        Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, cachedZ - targetCamera.transform.position.z));
        return new Vector2(world.x, world.y);
    }

    private void BeginMove(Touch touch)
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
}
