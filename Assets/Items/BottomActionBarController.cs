using UnityEngine;
using UnityEngine.UI;

public class BottomActionBarController : MonoBehaviour
{
    [SerializeField] private SelectionManager selectionManager;

    [Header("Action Buttons")]
    [SerializeField] private Button cloneButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private void Awake()
    {
        SetButtonsInteractable(false);
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
    }

    private void HandleSelectionChanged(Selectable selection)
    {
        bool hasSelection = selection != null;
        SetButtonsInteractable(hasSelection);
    }

    private void SetButtonsInteractable(bool enabled)
    {
        if (cloneButton != null) cloneButton.interactable = enabled;
        if (deleteButton != null) deleteButton.interactable = enabled;
        if (resetButton != null) resetButton.interactable = enabled;
        if (upButton != null) upButton.interactable = enabled;
        if (downButton != null) downButton.interactable = enabled;
    }

    public void OnClonePressed()
    {
        if (selectionManager == null) return;
        selectionManager.CloneSelected();
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
}
