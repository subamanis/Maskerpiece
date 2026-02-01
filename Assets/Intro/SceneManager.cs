using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    [Header("InnterScenes")] public GameObject introInner;
    public GameObject gameInner;
    
    [Header("UI")]
    public GameObject startScreenPanel;

    [Header("Intro Girl")]
    public SpriteRenderer introGirlRenderer;
    public Sprite front1;
    public Sprite front2;
    public Sprite back1;
    public Sprite back2;

    [Header("Walk Settings")]
    public float walkDuration = 3f;
    public float stepInterval = 0.5f;
    public float startScale = 0.23f;
    public float endScale = 0.1f;
    public float fromOffsetY = 0f;
    public float toOffsetY = -2f;

    [Header("Curtains")]
    public Transform curtainLeft;
    public Transform curtainRight;
    public float curtainCloseDuration = 0.5f;
    public float curtainClosedWait = 1f;

    [Header("Effects")]
    public FlashManager flashManager;

    [Header("Audio")]
    public AudioClip curtainsSound;
    public AudioClip footstepSound;
    public AudioClip chatterSound;

    private AudioSource audioSource;
    private AudioSource chatterSource;
    private Coroutine walkCoroutine;

    private Vector3 curtainLeftOpenPos;
    private Vector3 curtainRightOpenPos;
    private Vector3 curtainLeftClosedPos;
    private Vector3 curtainRightClosedPos;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        chatterSource = gameObject.AddComponent<AudioSource>();
        chatterSource.loop = true;

        if (curtainLeft != null && curtainRight != null)
        {
            curtainLeftOpenPos = curtainLeft.position;
            curtainRightOpenPos = curtainRight.position;

            float leftWidth = curtainLeft.GetComponent<SpriteRenderer>().bounds.size.x;
            float rightWidth = curtainRight.GetComponent<SpriteRenderer>().bounds.size.x;

            curtainLeftClosedPos = curtainLeftOpenPos + Vector3.right * (leftWidth * 0.75f);
            curtainRightClosedPos = curtainRightOpenPos + Vector3.left * (rightWidth * 0.75f);
        }
    }

    public void OnStartButtonPressed()
    {
        startScreenPanel.SetActive(false);
        WalkGirlBack();
    }

    public void WalkGirlBack()
    {
        if (walkCoroutine != null)
            StopCoroutine(walkCoroutine);

        walkCoroutine = StartCoroutine(WalkGirlBackRoutine());
    }

    public void WalkGirlFront()
    {
        if (walkCoroutine != null)
            StopCoroutine(walkCoroutine);

        walkCoroutine = StartCoroutine(WalkGirlFrontRoutine());
    }

    IEnumerator WalkGirlBackRoutine()
    {
        if (!EnsureIntroGirlReady(back1, back2))
            yield break;

        if (chatterSound != null)
        {
            chatterSource.clip = chatterSound;
            chatterSource.Play();
        }

        if (flashManager != null)
            flashManager.StartFlashing();

        yield return StartCoroutine(AnimateWalk(back1, back2, startScale, endScale, fromOffsetY, toOffsetY));

        if (flashManager != null)
            flashManager.StopFlashing();

        chatterSource.Stop();

        yield return StartCoroutine(CloseCurtains());
        
        introInner.SetActive(true);
        gameInner.SetActive(true);

        yield return StartCoroutine(OpenCurtains());
    }

    IEnumerator WalkGirlFrontRoutine()
    {
        if (!EnsureIntroGirlReady(front1, front2))
            yield break;

        yield return StartCoroutine(OpenCurtains());

        yield return StartCoroutine(AnimateWalk(front1, front2, endScale, startScale, toOffsetY, fromOffsetY));
    }

    IEnumerator AnimateWalk(Sprite spriteA, Sprite spriteB, float fromScale, float toScale, float offsetXYtart, float offsetYEnd)
    {
        float walkTimer = 0f;
        float stepTimer = 0f;
        bool showingFirst = true;

        introGirlRenderer.sprite = spriteA;
        introGirlRenderer.gameObject.SetActive(true);
        introGirlRenderer.transform.localScale = Vector3.one * fromScale;

        while (walkTimer < walkDuration)
        {
            walkTimer += Time.deltaTime;
            stepTimer += Time.deltaTime;

            float t = Mathf.Clamp01(walkTimer / walkDuration);
            float currentScale = Mathf.Lerp(fromScale, toScale, t);
            float currentY = Mathf.Lerp(offsetXYtart, offsetYEnd, t);
            introGirlRenderer.transform.localScale = Vector3.one * currentScale;
            introGirlRenderer.transform.position = new Vector3(0f, currentY, 0f);

            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                showingFirst = !showingFirst;
                introGirlRenderer.sprite = showingFirst ? spriteA : spriteB;

                if (footstepSound != null)
                    audioSource.PlayOneShot(footstepSound);
            }

            yield return null;
        }

        introGirlRenderer.transform.localScale = Vector3.one * toScale;
    }

    IEnumerator CloseCurtains()
    {
        if (curtainsSound != null)
            audioSource.PlayOneShot(curtainsSound);

        yield return StartCoroutine(MoveCurtains(curtainLeftOpenPos, curtainLeftClosedPos, curtainRightOpenPos, curtainRightClosedPos));
    }

    IEnumerator OpenCurtains()
    {
        if (curtainsSound != null)
            audioSource.PlayOneShot(curtainsSound);

        yield return StartCoroutine(MoveCurtains(curtainLeftClosedPos, curtainLeftOpenPos, curtainRightClosedPos, curtainRightOpenPos));

        OnCurtainsOpened();
    }

    IEnumerator MoveCurtains(Vector3 leftFrom, Vector3 leftTo, Vector3 rightFrom, Vector3 rightTo)
    {
        float elapsed = 0f;
        while (elapsed < curtainCloseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / curtainCloseDuration;
            t = t * t * (3f - 2f * t);

            curtainLeft.position = Vector3.Lerp(leftFrom, leftTo, t);
            curtainRight.position = Vector3.Lerp(rightFrom, rightTo, t);
            yield return null;
        }

        curtainLeft.position = leftTo;
        curtainRight.position = rightTo;
    }

    void OnCurtainsOpened()
    {
        Debug.Log("Curtains opened - dressing room here");
    }

    bool EnsureIntroGirlReady(Sprite spriteA, Sprite spriteB)
    {
        if (introGirlRenderer == null)
        {
            Debug.LogWarning("Intro girl SpriteRenderer is not assigned.");
            return false;
        }

        if (spriteA == null || spriteB == null)
        {
            Debug.LogWarning("Intro girl sprites are not assigned.");
            return false;
        }

        return true;
    }
}
