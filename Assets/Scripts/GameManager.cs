using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject startScreenPanel;

    [Header("Girl Sprites")]
    public GameObject girlFront;
    public GameObject girlBack1;
    public GameObject girlBack2;

    [Header("Walk Settings")]
    public float walkDuration = 3f;
    public float stepInterval = 0.5f;
    public float startScale = 0.23f;
    public float endScale = 0.1f;

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
    private bool isWalking;
    private float walkTimer;
    private bool showingGirl1 = true;
    private float stepTimer;
    private Transform activeGirl;

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
        girlFront.SetActive(false);

        girlBack1.transform.localScale = Vector3.one * startScale;
        girlBack2.transform.localScale = Vector3.one * startScale;

        girlBack1.SetActive(true);
        girlBack2.SetActive(false);
        showingGirl1 = true;

        activeGirl = girlBack1.transform;
        isWalking = true;
        walkTimer = 0f;
        stepTimer = 0f;

        if (chatterSound != null)
        {
            chatterSource.clip = chatterSound;
            chatterSource.Play();
        }

        if (flashManager != null)
            flashManager.StartFlashing();
    }

    void Update()
    {
        if (!isWalking) return;

        walkTimer += Time.deltaTime;
        stepTimer += Time.deltaTime;

        float t = walkTimer / walkDuration;
        float currentScale = Mathf.Lerp(startScale, endScale, t);

        girlBack1.transform.localScale = Vector3.one * currentScale;
        girlBack2.transform.localScale = Vector3.one * currentScale;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f;
            showingGirl1 = !showingGirl1;
            girlBack1.SetActive(showingGirl1);
            girlBack2.SetActive(!showingGirl1);

            if (footstepSound != null)
                audioSource.PlayOneShot(footstepSound);
        }

        if (walkTimer >= walkDuration)
        {
            isWalking = false;
            OnWalkComplete();
        }
    }

    void OnWalkComplete()
    {
        if (flashManager != null)
            flashManager.StopFlashing();

        girlBack1.SetActive(false);
        girlBack2.SetActive(false);
        StartCoroutine(CurtainTransition());
    }

    IEnumerator CurtainTransition()
    {
        if (curtainsSound != null)
            audioSource.PlayOneShot(curtainsSound);

        yield return StartCoroutine(MoveCurtains(curtainLeftOpenPos, curtainLeftClosedPos, curtainRightOpenPos, curtainRightClosedPos));

        chatterSource.Stop();

        yield return new WaitForSeconds(curtainClosedWait);

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
}
