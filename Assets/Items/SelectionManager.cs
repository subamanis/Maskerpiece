using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float tapMaxMovement = 12f;

    public Selectable currentSelection;
    public event Action<Selectable> OnSelectionChanged;

    private bool pointerActive;
    private bool pointerMoved;
    private bool pointerMultiTouch;
    private Vector2 pointerStartPosition;
    [SerializeField] private float centerDepth = 0f; // for 2D: use object's Z, or 0
    [SerializeField] private Vector2 cloneOffset = new Vector2(0.5f, -0.5f);


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
        if (EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Touchscreen.current != null)
            {
                int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject(touchId))
                    return;
            }
        }

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

        // 🔔 Notify listeners (UI, etc.)
        OnSelectionChanged?.Invoke(currentSelection);
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

        // 🔔 Notify listeners that nothing is selected
        OnSelectionChanged?.Invoke(null);
    }

    public void ResetSelected()
    {
        if (currentSelection == null || targetCamera == null) return;

        // Choose the depth to compute the camera-plane intersection.
        // For ortho cameras, this doesn't matter much; for perspective, it does.
        float depth = centerDepth;

        // If you want "same plane as the object", you can do:
        // depth = Mathf.Abs(currentSelection.transform.position.z - targetCamera.transform.position.z);

        Vector3 centerWorld = targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));

        // Keep object on its original Z plane (common for 2D)
        centerWorld.z = currentSelection.transform.position.z;

        currentSelection.transform.position = centerWorld;
        currentSelection.transform.rotation = Quaternion.identity;
        currentSelection.transform.localScale = currentSelection.DefaultScale;
    }

    public void CloneSelected()
    {
        if (currentSelection == null) return;

        Selectable source = currentSelection;
        Transform sourceT = source.transform;

        GameObject cloneGo = Instantiate(sourceT.gameObject, sourceT.parent);

        // ✅ Ensure Reset uses the ORIGINAL default scale, not the cloned scale
        Selectable cloneSelectable = cloneGo.GetComponent<Selectable>();
        if (cloneSelectable != null)
        {
            cloneSelectable.SetDefaultScale(source.DefaultScale);
            cloneSelectable.SetSelected(false);
        }

        // Match current transform exactly
        Transform cloneT = cloneGo.transform;
        cloneT.position = sourceT.position + new Vector3(cloneOffset.x, cloneOffset.y, 0f);
        cloneT.rotation = sourceT.rotation;
        cloneT.localScale = sourceT.localScale;

        // Optional: select the clone
        source.SetSelected(false);
        currentSelection = cloneSelectable;
        if (currentSelection != null)
            currentSelection.SetSelected(true);
    }

}
