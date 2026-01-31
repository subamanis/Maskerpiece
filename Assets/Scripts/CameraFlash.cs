using UnityEngine;

public class CameraFlash : MonoBehaviour
{
    public float flashDuration = 0.3f;
    public float maxScale = 1.5f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve opacityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private SpriteRenderer spriteRenderer;
    private float timer;
    private Vector3 baseScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / flashDuration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        float scaleT = scaleCurve.Evaluate(t);
        float peakT = t < 0.3f ? t / 0.3f : 1f - ((t - 0.3f) / 0.7f);
        transform.localScale = baseScale * maxScale * Mathf.Clamp01(peakT * 2f);

        float alpha = opacityCurve.Evaluate(t);
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
