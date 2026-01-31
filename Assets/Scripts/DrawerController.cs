using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DrawerController : MonoBehaviour
{
    [Header("References")]
    public RectTransform drawerPanel;
    public RectTransform drawerButtonRect;
    public Button drawerButton;

    [Header("Animation")]
    public float animationDuration = 0.3f;

    [Header("Audio")]
    public AudioClip drawerSound;

    private AudioSource audioSource;
    private bool isOpen;
    private bool isAnimating;
    private float closedX;
    private float openX;
    private float buttonClosedX;
    private float buttonOpenX;

    void Start()
    {
        if (drawerPanel == null) return;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        float panelWidth = drawerPanel.sizeDelta.x;
        closedX = drawerPanel.anchoredPosition.x;
        openX = closedX - panelWidth;

        if (drawerButtonRect != null)
        {
            buttonClosedX = drawerButtonRect.anchoredPosition.x;
            buttonOpenX = buttonClosedX - panelWidth;
        }

        isOpen = false;

        if (drawerButton != null)
            drawerButton.onClick.AddListener(ToggleDrawer);
    }

    public void ToggleDrawer()
    {
        if (isAnimating) return;

        if (drawerSound != null)
            audioSource.PlayOneShot(drawerSound);

        StartCoroutine(AnimateDrawer(!isOpen));
    }

    IEnumerator AnimateDrawer(bool opening)
    {
        isAnimating = true;

        float startX = drawerPanel.anchoredPosition.x;
        float endX = opening ? openX : closedX;

        float buttonStartX = drawerButtonRect != null ? drawerButtonRect.anchoredPosition.x : 0f;
        float buttonEndX = opening ? buttonOpenX : buttonClosedX;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = t * t * (3f - 2f * t);

            Vector2 pos = drawerPanel.anchoredPosition;
            pos.x = Mathf.Lerp(startX, endX, t);
            drawerPanel.anchoredPosition = pos;

            if (drawerButtonRect != null)
            {
                Vector2 btnPos = drawerButtonRect.anchoredPosition;
                btnPos.x = Mathf.Lerp(buttonStartX, buttonEndX, t);
                drawerButtonRect.anchoredPosition = btnPos;
            }

            yield return null;
        }

        Vector2 finalPos = drawerPanel.anchoredPosition;
        finalPos.x = endX;
        drawerPanel.anchoredPosition = finalPos;

        if (drawerButtonRect != null)
        {
            Vector2 btnFinalPos = drawerButtonRect.anchoredPosition;
            btnFinalPos.x = buttonEndX;
            drawerButtonRect.anchoredPosition = btnFinalPos;
        }

        isOpen = opening;
        isAnimating = false;
    }
}
