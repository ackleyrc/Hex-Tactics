using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsScreenGUI : MonoBehaviour
{
    #region SINGLETON_MGMT
    private static CreditsScreenGUI _Instance;
    public static CreditsScreenGUI Instance { get { return _Instance; } }

    private void Awake()
    {
        _Instance = this;
    }

    private void OnDestroy()
    {
        if (_Instance == this)
        {
            _Instance = null;
        }
    }
    #endregion SINGLETON_MGMT

    public event System.Action OnCloseCredits = delegate { };

    public CanvasGroup canvasGroup;
    public float fadeInDuration;
    public float fadeOutDuration;

    public RectTransform panelObject;

    public float scrollDelay = 2.0f;
    public float scrollSpeed = 1.0f;
    public Vector3 positionStart = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 positionEnd = new Vector3(0.0f, 4200.0f, 0.0f);
    public float creditsLinger = 5.0f;

    private float displayTimeElapsed = 0.0f;
    private float lingerTimeElapsed = 0.0f;
    public bool IsDisplayed { get; private set; }

    private void Start()
    {
        //panelObject.transform.localPosition = positionStart;

        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void DisplayCredits()
    {
        //Debug.Log($"CreditsScreenGUI::DisplayCredits()");

        StopAllCoroutines();
        StartCoroutine(FadeIn());
        IsDisplayed = true;
    }

    private void Update()
    {
        if (IsDisplayed == true)
        {
            displayTimeElapsed += Time.unscaledDeltaTime;

            if (panelObject.localPosition.y < positionEnd.y)
            {
                if (displayTimeElapsed > scrollDelay)
                {
                    Vector3 pos = panelObject.localPosition;
                    panelObject.localPosition = new Vector3(pos.x, Mathf.Clamp(pos.y + scrollSpeed * Time.unscaledDeltaTime, positionStart.y, positionEnd.y), pos.z);
                }
            }
            else // if (panelObject.transform.localPosition.y >= positionEnd.y)
            {
                lingerTimeElapsed += Time.unscaledDeltaTime;
            }

            if (lingerTimeElapsed >= creditsLinger || Input.GetKeyUp(KeyCode.Escape))
            {
                StopAllCoroutines();
                StartCoroutine(FadeOut());
                IsDisplayed = false;

                OnCloseCredits?.Invoke();
            }
        }
    }

    private IEnumerator FadeIn()
    {
        //Debug.Log($"CreditsScreenGUI::FadeIn()");

        panelObject.localPosition = positionStart;

        lingerTimeElapsed = 0.0f;
        displayTimeElapsed = 0.0f;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (canvasGroup.alpha < 1.0f)
        {
            yield return null;

            canvasGroup.alpha = canvasGroup.alpha + Time.unscaledDeltaTime / fadeInDuration;
        }

        //Debug.Log($"CreditsScreenGUI :: Fade In Complete");
    }

    private IEnumerator FadeOut()
    {
        //Debug.Log($"CreditsScreenGUI::FadeOut()");

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (canvasGroup.alpha > 0.0f)
        {
            yield return null;

            canvasGroup.alpha = canvasGroup.alpha - Time.unscaledDeltaTime / fadeOutDuration;
        }

        //Debug.Log($"CreditsScreenGUI :: Fade Out Complete");
    }
}