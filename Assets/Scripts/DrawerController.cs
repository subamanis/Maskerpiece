using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DrawerController : MonoBehaviour
{
    [Header("References")]
    public RectTransform drawerPanel;
    public Button drawerButton;

    [Header("Animation")]
    public float animationDuration = 0.3f;

    private bool isOpen;
    private bool isAnimating;
    private float closedX;
    private float openX;

    void Start()
    {
        if (drawerPanel == null) return;

        float panelWidth = drawerPanel.sizeDelta.x;
        closedX = drawerPanel.anchoredPosition.x;
        openX = closedX - panelWidth;

        isOpen = false;

        if (drawerButton != null)
            drawerButton.onClick.AddListener(ToggleDrawer);
    }

    public void ToggleDrawer()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateDrawer(!isOpen));
    }

    IEnumerator AnimateDrawer(bool opening)
    {
        isAnimating = true;

        float startX = drawerPanel.anchoredPosition.x;
        float endX = opening ? openX : closedX;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = t * t * (3f - 2f * t);

            Vector2 pos = drawerPanel.anchoredPosition;
            pos.x = Mathf.Lerp(startX, endX, t);
            drawerPanel.anchoredPosition = pos;

            yield return null;
        }

        Vector2 finalPos = drawerPanel.anchoredPosition;
        finalPos.x = endX;
        drawerPanel.anchoredPosition = finalPos;

        isOpen = opening;
        isAnimating = false;
    }
}
