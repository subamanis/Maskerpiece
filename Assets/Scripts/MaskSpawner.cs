using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaskSpawner : MonoBehaviour
{
    [Header("Mask Definition")] public MaskDefinition maskDefinition;

    [Header("UI References")] public Transform buttonContainer;
    public GameObject buttonTemplate;

    [Header("Spawn Settings")] public Transform spawnParent;
    public float spawnScale = 1f;
    public Vector2 spawnOffset = Vector2.zero;

    [Header("Audio")] public AudioClip purchaseSound;
    public AudioClip cantAffordSound;

    private int spawnMod = 7;
    private AudioSource audioSource;
    private int nextSortingOrder = 0;

    void Start()
    {
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
                btn.onClick.AddListener(() => TrySpawnMask(maskItem));
            }
        }

        Destroy(buttonTemplate.gameObject);
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

        nextSortingOrder++;

        Vector3 spawnPos = Vector3.zero;
        spawnPos.z = 0f;
        int modIndex = nextSortingOrder % spawnMod;
        Vector3 offset = new Vector3(spawnOffset.x, spawnOffset.y, 0f) * modIndex;
        spawnPos += offset;

        GameObject instance = Instantiate(maskItem.prefab.gameObject, spawnPos, Quaternion.identity);
        SpriteRenderer instanceSR = instance.GetComponentInChildren<SpriteRenderer>();
        instanceSR.sortingOrder = nextSortingOrder;

        instance.transform.SetParent(spawnParent);
        instance.transform.localScale = Vector3.one * spawnScale;
        instance.transform.localPosition = spawnPos;
    }
}