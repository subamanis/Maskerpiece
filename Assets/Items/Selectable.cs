using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Selectable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool useGlow = true;
    [SerializeField] private bool autoGlowColor = true;
    [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private float glowAlpha = 0.7f;
    [SerializeField] private float glowSize = 2f;
    [SerializeField] private float glowIntensity = 1f;
    [SerializeField] private float glowPulseAmplitude = 0.35f;
    [SerializeField] private float glowPulseSpeed = 2f;
    [SerializeField] private float dominantAlphaThreshold = 0.1f;
    [SerializeField] private Shader glowShader;

    private Color defaultColor = Color.white;
    private MaterialPropertyBlock propertyBlock;
    private Material originalMaterial;
    private Material glowMaterial;
    private Sprite cachedSprite;
    private Color cachedGlowColor;

    private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
    private static readonly int GlowIntensityId = Shader.PropertyToID("_GlowIntensity");
    private static readonly int GlowSizeId = Shader.PropertyToID("_GlowSize");

    public bool IsSelected { get; private set; }
    public event Action<bool> OnRestCollisionChanged;
    public bool? LastRestCollision { get; private set; }

    private Vector3 defaultScale;
    private bool defaultScaleInitialized;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null){
            defaultColor = spriteRenderer.color;
            originalMaterial = spriteRenderer.sharedMaterial;
            propertyBlock = new MaterialPropertyBlock();
        }
        else
            Debug.LogWarning($"Selectable on {name} has no SpriteRenderer assigned.");
    }

    private void Start()
    {
        // Capture after all Awake() calls in the scene
        // ✅ Only capture if not already set (e.g., for clones)
        if (!defaultScaleInitialized)
        {
            defaultScale = transform.localScale;
            defaultScaleInitialized = true;
        }
    }

    private void Update()
    {
        if (!IsSelected || !useGlow || spriteRenderer == null || glowMaterial == null)
        {
            return;
        }

        ApplyGlowProperties();
    }
    public Vector3 DefaultScale => defaultScale;

    // ✅ Use this for clones to preserve original's default scale
    public void SetDefaultScale(Vector3 scale)
    {
        defaultScale = scale;
        defaultScaleInitialized = true;
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected) return;

        IsSelected = selected;
        if (!selected)
        {
            LastRestCollision = null;
        }
        if (spriteRenderer != null && !selected)
        {
            spriteRenderer.color = defaultColor;
        }

        if (!useGlow || spriteRenderer == null)
        {
            return;
        }

        if (selected)
        {
            EnsureGlowMaterial();
            UpdateGlowColor();
            ApplyGlowProperties();
        }
        else
        {
            RestoreOriginalMaterial();
        }
    }

    public void NotifyRestCollisionChanged(bool hasCollision)
    {
        if (LastRestCollision.HasValue && LastRestCollision.Value == hasCollision)
        {
            return;
        }

        LastRestCollision = hasCollision;
        OnRestCollisionChanged?.Invoke(hasCollision);
    }

    private void EnsureGlowMaterial()
    {
        if (glowMaterial == null)
        {
            Shader shader = glowShader != null ? glowShader : Shader.Find("Custom/SpriteGlowPulse");
            if (shader == null)
            {
                Debug.LogWarning("Selectable glow shader not found. Assign a shader or add Custom/SpriteGlowPulse.");
                return;
            }

            glowMaterial = new Material(shader)
            {
                hideFlags = HideFlags.DontSave
            };
        }

        spriteRenderer.material = glowMaterial;
    }

    private void RestoreOriginalMaterial()
    {
        spriteRenderer.material = originalMaterial;
        spriteRenderer.SetPropertyBlock(null);
    }

    private void ApplyGlowProperties()
    {
        float pulse = 1f + Mathf.Sin(Time.time * glowPulseSpeed * Mathf.PI * 2f) * glowPulseAmplitude;
        float intensity = Mathf.Max(0f, glowIntensity * pulse);
        Color finalGlow = cachedGlowColor;
        finalGlow.a = glowAlpha;

        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(GlowColorId, finalGlow);
        propertyBlock.SetFloat(GlowIntensityId, intensity);
        propertyBlock.SetFloat(GlowSizeId, glowSize);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private void UpdateGlowColor()
    {
        if (spriteRenderer == null)
        {
            cachedGlowColor = glowColor;
            return;
        }

        if (spriteRenderer.sprite != cachedSprite)
        {
            cachedSprite = spriteRenderer.sprite;
            cachedGlowColor = autoGlowColor ? GetOppositeColor(GetDominantColor(cachedSprite)) : glowColor;
        }
        else if (!autoGlowColor)
        {
            cachedGlowColor = glowColor;
        }
    }

    private Color GetDominantColor(Sprite sprite)
    {
        if (sprite == null || sprite.texture == null)
        {
            return glowColor;
        }

        try
        {
            Rect rect = sprite.textureRect;
            int width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(rect.height));
            Color[] pixels = sprite.texture.GetPixels(
                Mathf.RoundToInt(rect.x),
                Mathf.RoundToInt(rect.y),
                width,
                height);

            float total = 0f;
            Vector3 sum = Vector3.zero;
            foreach (Color pixel in pixels)
            {
                if (pixel.a <= dominantAlphaThreshold)
                {
                    continue;
                }

                float weight = pixel.a;
                sum += new Vector3(pixel.r, pixel.g, pixel.b) * weight;
                total += weight;
            }

            if (total > 0.0001f)
            {
                Vector3 avg = sum / total;
                return new Color(avg.x, avg.y, avg.z, 1f);
            }
        }
        catch (System.Exception)
        {
            // Texture not readable or missing; fall back to configured glow color.
        }

        return glowColor;
    }

    private static Color GetOppositeColor(Color color)
    {
        return new Color(1f - color.r, 1f - color.g, 1f - color.b, color.a);
    }

}
