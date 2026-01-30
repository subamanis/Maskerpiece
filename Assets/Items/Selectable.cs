using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Selectable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.6f, 1f);

    private Color defaultColor = Color.white;

    public bool IsSelected { get; private set; }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"Selectable on {name} has no SpriteRenderer assigned.");
        }
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected)
        {
            return;
        }

        IsSelected = selected;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = selected ? selectedColor : defaultColor;
        }
    }
}
