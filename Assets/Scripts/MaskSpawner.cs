using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaskSpawner : MonoBehaviour
{
    [Header("Mask Definition")]
    public MaskDefinition maskDefinition;

    [Header("UI References")]
    public Transform buttonContainer;
    public GameObject buttonTemplate;

    [Header("Spawn Settings")]
    public Transform spawnParent;

    [Header("Audio")]
    public AudioClip purchaseSound;
    public AudioClip cantAffordSound;

    private Camera mainCamera;
    private AudioSource audioSource;

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        PopulateDrawer();
    }

    void PopulateDrawer()
    {
        if (buttonContainer == null || buttonTemplate == null || maskDefinition == null) return;

        buttonTemplate.SetActive(false);

        foreach (var maskItem in maskDefinition.Masks)
        {
            if (maskItem == null || maskItem.prefab == null) continue;

            GameObject buttonObj = Instantiate(buttonTemplate, buttonContainer);
            buttonObj.SetActive(true);

            SpriteRenderer prefabSR = maskItem.prefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabSR != null && prefabSR.sprite != null)
            {
                Image img = buttonObj.GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = prefabSR.sprite;
                    img.preserveAspect = true;
                }
            }

            Transform priceTransform = buttonObj.transform.Find("Price");
            if (priceTransform != null)
            {
                TextMeshProUGUI priceText = priceTransform.GetComponent<TextMeshProUGUI>();
                if (priceText != null)
                    priceText.text = $"${maskItem.price}";
            }

            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                MaskItem captured = maskItem;
                btn.onClick.AddListener(() => TrySpawnMask(captured));
            }
        }
    }

    void TrySpawnMask(MaskItem maskItem)
    {
        if (BudgetManager.Instance == null || !BudgetManager.Instance.CanAfford(maskItem.price))
        {
            if (cantAffordSound != null)
                audioSource.PlayOneShot(cantAffordSound);
            return;
        }

        BudgetManager.Instance.TrySpend(maskItem.price);

        if (purchaseSound != null)
            audioSource.PlayOneShot(purchaseSound);

        Vector3 spawnPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        spawnPos.z = 0f;

        GameObject instance = Instantiate(maskItem.prefab.gameObject, spawnPos, Quaternion.identity);

        if (spawnParent != null)
            instance.transform.SetParent(spawnParent, true);
    }
}
