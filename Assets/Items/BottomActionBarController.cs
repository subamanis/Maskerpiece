using UnityEngine;

public class BottomActionBarController : MonoBehaviour
{
    [SerializeField] private SelectionManager selectionManager;

    public void OnDeletePressed()
    {
        if (selectionManager == null)
        {
            Debug.LogWarning("SelectionManager reference not set.");
            return;
        }

        selectionManager.DeleteSelected();
    }
}
