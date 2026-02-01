using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float tapMaxMovement = 12f;

    public Selectable currentSelection;
    public event Action<Selectable> OnSelectionChanged;

    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private readonly List<Selectable> collisionBuffer = new List<Selectable>(8);

    private bool pointerActive;
    private bool pointerMoved;
    private bool pointerMultiTouch;
    private Vector2 pointerStartPosition;
    [SerializeField] private float centerDepth = 0f; // for 2D: use object's Z, or 0


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

    public bool TrySwapWithAdjacentCollision(Selectable selection, bool isUp)
    {
        if (selection == null)
        {
            return false;
        }

        Selectable target = GetAdjacentCollisionSelectable(selection, isUp);
        if (target == null)
        {
            return false;
        }

        SpriteRenderer currentRenderer = selection.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if (currentRenderer == null || targetRenderer == null)
        {
            return false;
        }

        int currentOrder = currentRenderer.sortingOrder;
        int targetOrder = targetRenderer.sortingOrder;
        currentRenderer.sortingOrder = targetOrder;
        targetRenderer.sortingOrder = currentOrder;
        return true;
    }

    public void GetAdjacentCollisionAvailability(Selectable selection, out bool hasAbove, out bool hasBelow)
    {
        hasAbove = false;
        hasBelow = false;

        if (selection == null || !HasRestCollisions(selection))
        {
            return;
        }

        CollectCollisionSelectables(selection, collisionBuffer);
        if (collisionBuffer.Count == 0)
        {
            return;
        }

        SpriteRenderer currentRenderer = selection.GetComponentInChildren<SpriteRenderer>();
        if (currentRenderer == null)
        {
            return;
        }

        int currentOrder = currentRenderer.sortingOrder;
        foreach (Selectable selectable in collisionBuffer)
        {
            SpriteRenderer renderer = selectable.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                continue;
            }

            int order = renderer.sortingOrder;
            if (order > currentOrder)
            {
                hasAbove = true;
            }
            else if (order < currentOrder)
            {
                hasBelow = true;
            }
        }
    }

    private Selectable GetAdjacentCollisionSelectable(Selectable selection, bool isUp)
    {
        if (!HasRestCollisions(selection))
        {
            return null;
        }

        CollectCollisionSelectables(selection, collisionBuffer);
        if (collisionBuffer.Count == 0)
        {
            return null;
        }

        SpriteRenderer currentRenderer = selection.GetComponentInChildren<SpriteRenderer>();
        if (currentRenderer == null)
        {
            return null;
        }

        int currentOrder = currentRenderer.sortingOrder;
        Selectable best = null;
        int bestOrder = isUp ? int.MaxValue : int.MinValue;

        foreach (Selectable selectable in collisionBuffer)
        {
            SpriteRenderer renderer = selectable.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                continue;
            }

            int order = renderer.sortingOrder;
            if (isUp)
            {
                if (order > currentOrder && order < bestOrder)
                {
                    best = selectable;
                    bestOrder = order;
                }
            }
            else
            {
                if (order < currentOrder && order > bestOrder)
                {
                    best = selectable;
                    bestOrder = order;
                }
            }
        }

        return best;
    }

    private bool HasRestCollisions(Selectable selection)
    {
        return selection != null
               && selection.LastRestCollision.HasValue
               && selection.LastRestCollision.Value;
    }

    private void CollectCollisionSelectables(Selectable selection, List<Selectable> results)
    {
        results.Clear();
        if (selection == null)
        {
            return;
        }

        Collider2D collider = selection.GetComponent<Collider2D>();
        if (collider == null)
        {
            return;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(Physics2D.AllLayers);
        filter.SetDepth(float.NegativeInfinity, float.PositiveInfinity);
        int count = collider.Overlap(filter, overlapResults);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = overlapResults[i];
            if (hit == null || hit == collider)
            {
                continue;
            }

            Selectable otherSelectable = hit.GetComponentInParent<Selectable>();
            if (otherSelectable != null && otherSelectable != selection && !results.Contains(otherSelectable))
            {
                results.Add(otherSelectable);
            }
        }
    }

}
