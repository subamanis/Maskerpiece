using UnityEngine;
using UnityEngine.UI;

public class MaskSpawner : MonoBehaviour
{
    [Header("Mask Definition")]
    public MaskDefinition maskDefinition;

    [Header("UI References")]
    public Transform buttonContainer;
    public GameObject buttonTemplate;

    [Header("Spawn Settings")]
    public Transform spawnParent;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
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
        if (BudgetManager.Instance == null || !BudgetManager.Instance.TrySpend(maskItem.price))
            return;

        Vector3 spawnPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        spawnPos.z = 0f;

        GameObject instance = Instantiate(maskItem.prefab.gameObject, spawnPos, Quaternion.identity);

        if (spawnParent != null)
            instance.transform.SetParent(spawnParent, true);
    }
}
