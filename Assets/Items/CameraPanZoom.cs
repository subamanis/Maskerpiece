using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraPanZoom : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SelectionManager selectionManager;
    [SerializeField] private bool clampToBounds = true;
    [SerializeField] private Vector2 panMin = new Vector2(-20f, -20f);
    [SerializeField] private Vector2 panMax = new Vector2(20f, 20f);
    [SerializeField] private float minOrthographicSize = 2f;
    [SerializeField] private float maxOrthographicSize = 12f;
    private bool isPanning;
    private Vector2 lastPanPosition;
    private bool isPinching;
    private float lastPinchDistance;
    private Vector2 lastPinchMidpoint;
    private Vector3 initialCameraPosition;
    private float initialOrthographicSize;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (selectionManager == null)
        {
            selectionManager = FindFirstObjectByType<SelectionManager>();
        }

        if (targetCamera != null)
        {
            initialCameraPosition = targetCamera.transform.position;
            if (targetCamera.orthographic)
            {
                initialOrthographicSize = targetCamera.orthographicSize;
            }
        }
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            return;
        }

        if (selectionManager != null && selectionManager.currentSelection != null)
        {
            return;
        }

        if (Touchscreen.current != null && HandleTouchInput())
        {
            return;
        }

        HandleMouseInput();
    }

    private bool HandleTouchInput()
    {
        int touchCount = GetActiveTouches(out TouchControl touchA, out TouchControl touchB);
        if (touchCount == 0)
        {
            isPanning = false;
            isPinching = false;
            lastPinchDistance = 0f;
            return false;
        }

        if (touchCount == 1)
        {
            isPinching = false;
            lastPinchDistance = 0f;
            HandleSingleTouchPan(touchA);
            return true;
        }

        HandlePinchZoom(touchA, touchB);
        return true;
    }

    private void HandleSingleTouchPan(TouchControl touch)
    {
        var phase = touch.phase.ReadValue();
        Vector2 position = touch.position.ReadValue();

        if (phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            isPanning = true;
            lastPanPosition = position;
            return;
        }

        if (!isPanning)
        {
            return;
        }

        if (phase == UnityEngine.InputSystem.TouchPhase.Moved ||
            phase == UnityEngine.InputSystem.TouchPhase.Stationary)
        {
            PanFromScreenDelta(lastPanPosition, position);
            lastPanPosition = position;
        }
        else if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                 phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            isPanning = false;
        }
    }

    private void HandlePinchZoom(TouchControl touchA, TouchControl touchB)
    {
        Vector2 posA = touchA.position.ReadValue();
        Vector2 posB = touchB.position.ReadValue();
        Vector2 midpoint = (posA + posB) * 0.5f;
        float distance = Vector2.Distance(posA, posB);

        if (!isPinching)
        {
            isPinching = true;
            lastPinchDistance = distance;
            lastPinchMidpoint = midpoint;
            return;
        }

        if (lastPinchDistance > 0.01f && distance > 0.01f)
        {
            float scale = lastPinchDistance / distance;
            SetOrthographicSize(targetCamera.orthographicSize * scale);
        }

        PanFromScreenDelta(lastPinchMidpoint, midpoint);
        lastPinchDistance = distance;
        lastPinchMidpoint = midpoint;

        if (touchA.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
            touchA.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled ||
            touchB.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
            touchB.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            isPinching = false;
            lastPinchDistance = 0f;
        }
    }

    private void HandleMouseInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isPanning = true;
            lastPanPosition = Mouse.current.position.ReadValue();
        }

        if (isPanning && Mouse.current.leftButton.isPressed)
        {
            Vector2 current = Mouse.current.position.ReadValue();
            PanFromScreenDelta(lastPanPosition, current);
            lastPanPosition = current;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isPanning = false;
        }
    }

    private void PanFromScreenDelta(Vector2 lastScreenPosition, Vector2 currentScreenPosition)
    {
        Vector3 worldLast = targetCamera.ScreenToWorldPoint(new Vector3(lastScreenPosition.x, lastScreenPosition.y, -targetCamera.transform.position.z));
        Vector3 worldCurrent = targetCamera.ScreenToWorldPoint(new Vector3(currentScreenPosition.x, currentScreenPosition.y, -targetCamera.transform.position.z));
        Vector3 delta = worldCurrent - worldLast;
        Vector3 nextPosition = targetCamera.transform.position - new Vector3(delta.x, delta.y, 0f);

        if (clampToBounds)
        {
            ApplyPanClamp(ref nextPosition);
        }

        targetCamera.transform.position = nextPosition;
    }

    private void SetOrthographicSize(float size)
    {
        if (!targetCamera.orthographic)
        {
            return;
        }

        targetCamera.orthographicSize = Mathf.Clamp(size, minOrthographicSize, maxOrthographicSize);

        if (clampToBounds)
        {
            Vector3 position = targetCamera.transform.position;
            ApplyPanClamp(ref position);
            targetCamera.transform.position = position;
        }
    }

    private void ApplyPanClamp(ref Vector3 position)
    {
        float factor = GetPanClampFactor();
        Vector2 min = Vector2.Lerp(new Vector2(initialCameraPosition.x, initialCameraPosition.y), panMin, factor);
        Vector2 max = Vector2.Lerp(new Vector2(initialCameraPosition.x, initialCameraPosition.y), panMax, factor);
        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.y = Mathf.Clamp(position.y, min.y, max.y);
    }

    private float GetPanClampFactor()
    {
        if (!targetCamera.orthographic)
        {
            return 1f;
        }

        float size = targetCamera.orthographicSize;
        float factor = Mathf.InverseLerp(initialOrthographicSize, minOrthographicSize, size);
        return Mathf.Clamp01(factor);
    }

    private static int GetActiveTouches(out TouchControl touchA, out TouchControl touchB)
    {
        touchA = null;
        touchB = null;

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

            if (count == 0)
            {
                touchA = touchControl;
            }
            else if (count == 1)
            {
                touchB = touchControl;
            }

            count++;
            if (count >= 2)
            {
                break;
            }
        }

        return count;
    }
}
