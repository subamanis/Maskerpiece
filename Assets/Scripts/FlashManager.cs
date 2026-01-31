using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FlashManager : MonoBehaviour
{
    [Header("Flash Container")]
    public Transform paparazzisParent;

    [Header("Timing")]
    public float minInterval = 0.2f;
    public float maxInterval = 0.8f;
    public float flashDuration = 0.15f;

    [Header("Screen Flash")]
    public Image screenFlashImage;
    public float screenFlashDelay = 0.2f;
    public float screenFlashCooldown = 0.3f;
    public float screenFlashDuration = 0.08f;
    public float screenFlashOpacity = 0.7f;

    private List<GameObject> flashObjects = new List<GameObject>();
    private bool isFlashing;
    private int lastFlashIndex = -1;
    private float lastScreenFlashTime = -999f;

    void Start()
    {
        if (paparazzisParent != null)
        {
            foreach (Transform child in paparazzisParent)
            {
                flashObjects.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }
        }
    }

    public void StartFlashing()
    {
        isFlashing = true;
        StartCoroutine(FlashLoop());
    }

    public void StopFlashing()
    {
        isFlashing = false;
        StopAllCoroutines();

        foreach (var flash in flashObjects)
        {
            if (flash != null)
                flash.SetActive(false);
        }

        if (screenFlashImage != null)
        {
            Color c = screenFlashImage.color;
            c.a = 0f;
            screenFlashImage.color = c;
        }

        lastScreenFlashTime = -999f;
    }

    IEnumerator FlashLoop()
    {
        while (isFlashing)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            if (!isFlashing) break;

            StartCoroutine(DoFlash());
            StartCoroutine(TriggerScreenFlashDelayed());
        }
    }

    IEnumerator TriggerScreenFlashDelayed()
    {
        yield return new WaitForSeconds(screenFlashDelay);

        if (!isFlashing) yield break;

        if (Time.time - lastScreenFlashTime >= screenFlashCooldown)
        {
            lastScreenFlashTime = Time.time;
            StartCoroutine(ScreenFlash());
        }
    }

    IEnumerator DoFlash()
    {
        if (flashObjects.Count == 0) yield break;

        int index;
        if (flashObjects.Count == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, flashObjects.Count);
            } while (index == lastFlashIndex);
        }
        lastFlashIndex = index;

        GameObject flash = flashObjects[index];
        if (flash == null) yield break;

        SpriteRenderer sr = flash.GetComponent<SpriteRenderer>();
        Vector3 originalScale = flash.transform.localScale;

        flash.SetActive(true);

        float elapsed = 0f;
        while (elapsed < flashDuration && flash != null && isFlashing)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;

            float scaleMult = t < 0.3f ? Mathf.Lerp(0.5f, 1.2f, t / 0.3f) : Mathf.Lerp(1.2f, 0f, (t - 0.3f) / 0.7f);
            flash.transform.localScale = originalScale * scaleMult;

            if (sr != null)
            {
                float alpha = t < 0.2f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.2f) / 0.8f);
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            yield return null;
        }

        if (flash == null) yield break;

        flash.transform.localScale = originalScale;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
        flash.SetActive(false);
    }

    IEnumerator ScreenFlash()
    {
        if (screenFlashImage == null) yield break;

        Color c = screenFlashImage.color;
        c.a = screenFlashOpacity;
        screenFlashImage.color = c;

        float elapsed = 0f;
        while (elapsed < screenFlashDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(screenFlashOpacity, 0f, elapsed / screenFlashDuration);
            screenFlashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        screenFlashImage.color = c;
    }
}
