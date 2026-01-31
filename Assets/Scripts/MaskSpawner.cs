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

        foreach (var selectable in maskDefinition.Selectables)
        {
            if (selectable == null) continue;

            GameObject buttonObj = Instantiate(buttonTemplate, buttonContainer);
            buttonObj.SetActive(true);

            SpriteRenderer prefabSR = selectable.GetComponentInChildren<SpriteRenderer>();
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
                Selectable capturedSelectable = selectable;
                btn.onClick.AddListener(() => SpawnMask(capturedSelectable.gameObject));
            }
        }
    }

    void SpawnMask(GameObject prefab)
    {
        Vector3 spawnPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        spawnPos.z = 0f;

        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (spawnParent != null)
            instance.transform.SetParent(spawnParent, true);
    }
}
