using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Selectable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.6f, 1f);

    private Color defaultColor = Color.white;

    public bool IsSelected { get; private set; }

    // ✅ ORIGINAL TRANSFORM SNAPSHOT
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

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

    private void Start()
    {
        // ✅ Capture after all Awake() calls in the scene
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;
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

    private void OnDestroy()
    {
        if (IsSelected)
        {
            SetSelected(false);
        }
    }

    // ✅ RESET API (call this from your button)
    public void ResetToOriginalTransform()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialScale;
    }

}
