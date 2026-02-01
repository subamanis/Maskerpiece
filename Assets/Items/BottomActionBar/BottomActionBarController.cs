using UnityEngine;
using UnityEngine.UI;

public class BottomActionBarController : MonoBehaviour
{
    [SerializeField] private SelectionManager selectionManager;

    [Header("Action Buttons")]
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    [Header("Restart")]
    [SerializeField] private RestartConfirmPopup restartPopup;
    private Selectable currentSelection;

    private void Awake()
    {
        SetActionButtonsInteractable(false);
        SetMoveButtonsInteractable(false);
    }

    private void OnEnable()
    {
        if (selectionManager != null)
            selectionManager.OnSelectionChanged += HandleSelectionChanged;
    }

    private void OnDisable()
    {
        if (selectionManager != null)
            selectionManager.OnSelectionChanged -= HandleSelectionChanged;

        if (currentSelection != null)
        {
            currentSelection.OnRestCollisionChanged -= HandleRestCollisionChanged;
            currentSelection = null;
        }
    }

    private void HandleSelectionChanged(Selectable selection)
    {
        if (currentSelection != null)
        {
            currentSelection.OnRestCollisionChanged -= HandleRestCollisionChanged;
        }

        currentSelection = selection;

        bool hasSelection = currentSelection != null;
        SetActionButtonsInteractable(hasSelection);

        if (!hasSelection)
        {
            SetMoveButtonsInteractable(false);
            return;
        }

        currentSelection.OnRestCollisionChanged += HandleRestCollisionChanged;
        UpdateMoveButtonsForSelection();
    }

    private void HandleRestCollisionChanged(bool hasCollision)
    {
        UpdateMoveButtonsForSelection();
    }

    private void SetActionButtonsInteractable(bool enabled)
    {
        if (deleteButton != null) deleteButton.interactable = enabled;
        if (resetButton != null) resetButton.interactable = enabled;
    }

    private void SetMoveButtonsInteractable(bool enabled)
    {
        if (upButton != null) upButton.interactable = enabled;
        if (downButton != null) downButton.interactable = enabled;
    }

    public void OnDeletePressed()
    {
        if (selectionManager == null) return;
        selectionManager.DeleteSelected();
    }

    public void OnResetPressed()
    {
        if (selectionManager == null) return;
        selectionManager.ResetSelected();
    }

    public void OnLayerUpPressed()
    {
        if (selectionManager == null || currentSelection == null)
        {
            UpdateMoveButtonsForSelection();
            return;
        }

        selectionManager.TrySwapWithAdjacentCollision(currentSelection, isUp: true);
        UpdateMoveButtonsForSelection();
    }

    public void OnLayerDownPressed()
    {
        if (selectionManager == null || currentSelection == null)
        {
            UpdateMoveButtonsForSelection();
            return;
        }

        selectionManager.TrySwapWithAdjacentCollision(currentSelection, isUp: false);
        UpdateMoveButtonsForSelection();
    }

    private void UpdateMoveButtonsForSelection()
    {
        if (selectionManager == null || currentSelection == null)
        {
            SetMoveButtonsInteractable(false);
            return;
        }

        selectionManager.GetAdjacentCollisionAvailability(currentSelection, out bool hasAbove, out bool hasBelow);
        if (upButton != null) upButton.interactable = hasAbove;
        if (downButton != null) downButton.interactable = hasBelow;
    }

    public void OnRestartPressed()
    {
        if (restartPopup == null)
        {
            Debug.LogWarning("RestartConfirmPopup reference not set.");
            return;
        }

        restartPopup.Show(
            "Restart?",
            "Are you sure you want to restart?",
            () =>
            {
                // For now only confirm works; we'll add initialization next
                Debug.Log("Restart confirmed (TODO: initialize).");
            }
        );
    }
}
