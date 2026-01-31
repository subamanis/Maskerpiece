using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float tapMaxMovement = 12f;

    public Selectable currentSelection;
    private bool pointerActive;
    private bool pointerMoved;
    private bool pointerMultiTouch;
    private Vector2 pointerStartPosition;

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
                pointerActive = true;
                pointerMoved = false;
                pointerMultiTouch = false;
                pointerStartPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return;
            }

            if (pointerActive)
            {
                Vector2 currentPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                if (Vector2.Distance(pointerStartPosition, currentPosition) > tapMaxMovement)
                {
                    pointerMoved = true;
                }

                if (activeTouchCount > 1)
                {
                    pointerMultiTouch = true;
                }

                if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
                {
                    if (!pointerMoved && !pointerMultiTouch)
                    {
                        HandleSelection(currentPosition);
                    }

                    pointerActive = false;
                }

                return;
            }
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                pointerActive = true;
                pointerMoved = false;
                pointerStartPosition = Mouse.current.position.ReadValue();
            }

            if (pointerActive && Mouse.current.leftButton.isPressed)
            {
                Vector2 currentPosition = Mouse.current.position.ReadValue();
                if (Vector2.Distance(pointerStartPosition, currentPosition) > tapMaxMovement)
                {
                    pointerMoved = true;
                }
            }

            if (pointerActive && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Vector2 currentPosition = Mouse.current.position.ReadValue();
                if (!pointerMoved)
                {
                    HandleSelection(currentPosition);
                }
                pointerActive = false;
            }
        }
    }

    private void HandleSelection(Vector2 screenPosition)
    {
        // Don't select objects when the pointer is interacting with UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

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

    public void DeleteSelected()
    {
        if (currentSelection == null) return;

        var toDelete = currentSelection.gameObject;
        currentSelection = null;
        Destroy(toDelete);
    }

    public void ResetSelected()
    {
        if (currentSelection == null) return;
        currentSelection.ResetToOriginalTransform();
    }
    
}
