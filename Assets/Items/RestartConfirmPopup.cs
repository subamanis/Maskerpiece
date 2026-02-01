using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestartConfirmPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    private Action onConfirm;

    private void Awake()
    {
        // Start hidden
        gameObject.SetActive(false);

        if (yesButton != null) yesButton.onClick.AddListener(HandleYes);
        if (noButton != null) noButton.onClick.AddListener(Hide);
    }

    public void Show(string title, string message, Action confirmAction)
    {
        onConfirm = confirmAction;

        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        onConfirm = null;
    }

    private void HandleYes()
    {
        var cb = onConfirm;
        Hide();
        cb?.Invoke();
    }
}
