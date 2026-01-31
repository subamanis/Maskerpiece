using UnityEngine;

public class GirlScaler : MonoBehaviour
{
    [SerializeField] float proportionToScreenHeight = 0.8f;
    int lastScreenWidth;
    int lastScreenHeight;

    void Start()
    {
        ApplyLayout();
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ApplyLayout();
        }
    }

    void ApplyLayout()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        Vector3 camPos = cam.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        float worldScreenHeight = cam.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        float targetWorldSize = worldScreenHeight * proportionToScreenHeight;
        float spriteMaxSize = Mathf.Max(spriteWidth, spriteHeight);

        // Keep a uniform 1:1 scale while fitting inside the screen.
        float totalScale = targetWorldSize / spriteMaxSize;

        transform.localScale = new Vector3(totalScale, totalScale, 1);
    }
}
