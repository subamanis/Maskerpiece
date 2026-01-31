using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Selectable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.6f, 1f);

    private Color defaultColor = Color.white;
    public bool IsSelected { get; private set; }

    private Vector3 defaultScale;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            defaultColor = spriteRenderer.color;
        else
            Debug.LogWarning($"Selectable on {name} has no SpriteRenderer assigned.");
    }

    private void Start()
    {
        // Capture after all Awake() calls in the scene
        defaultScale = transform.localScale;
    }

    public Vector3 DefaultScale => defaultScale;

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected) return;

        IsSelected = selected;
        if (spriteRenderer != null)
            spriteRenderer.color = selected ? selectedColor : defaultColor;
    }

    private void OnDestroy()
    {
        // (Optional) Not really needed, but harmless
        if (IsSelected)
            SetSelected(false);
    }
}
