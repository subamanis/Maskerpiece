using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private Selectable currentSelection;

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

        if (Touchscreen.current != null)
        {
            int activeTouchCount = GetActiveTouchCount();
            if (activeTouchCount == 1 && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                HandleSelection(Touchscreen.current.primaryTouch.position.ReadValue());
                return;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleSelection(Mouse.current.position.ReadValue());
        }
    }

    private void HandleSelection(Vector2 screenPosition)
    {
        Vector3 worldPoint = targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        Selectable nextSelection = hit.collider != null ? hit.collider.GetComponentInParent<Selectable>() : null;

        if (nextSelection == currentSelection)
        {
            return;
        }

        if (currentSelection != null)
        {
            currentSelection.SetSelected(false);
        }

        currentSelection = nextSelection;

        if (currentSelection != null)
        {
            currentSelection.SetSelected(true);
        }
    }

    private static int GetActiveTouchCount()
    {
        if (Touchscreen.current == null)
        {
            return 0;
        }

        int count = 0;
        foreach (var touchControl in Touchscreen.current.touches)
        {
            if (touchControl.press.isPressed)
            {
                count++;
            }
        }

        return count;
    }
}
