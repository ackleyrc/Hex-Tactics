using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseScreenGUI : MonoBehaviour
{
#region SINGLETON_MGMT
    private static PauseScreenGUI _Instance;
    public static PauseScreenGUI Instance { get { return _Instance; } }

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

    public CanvasGroup canvasGroup;
    public Button resumeButton;
    public Button exitButton;
    public float fadeInDuration;
    public float fadeOutDuration;

    public bool IsDisplayed { get; private set; } = false;

    private void Start()
    {
        resumeButton.onClick.AddListener(OnClickResumeButton);
        exitButton.onClick.AddListener(OnClickExitButton);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (IsDisplayed == true)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                StartCoroutine(FadeIn());
            }
        }
    }

    private void OnClickResumeButton()
    {
        StartCoroutine(FadeOut());
    }

    private void OnClickExitButton()
    {
        Application.Quit();
    }

    private IEnumerator FadeIn()
    {
        IsDisplayed = true;

        Time.timeScale = 0.0f;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (canvasGroup.alpha < 1.0f)
        {
            yield return null;

            canvasGroup.alpha = canvasGroup.alpha + Time.unscaledDeltaTime / fadeInDuration;
        }
    }

    private IEnumerator FadeOut()
    {
        IsDisplayed = false;

        Time.timeScale = 1.0f;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (canvasGroup.alpha > 0.0f)
        {
            yield return null;

            canvasGroup.alpha = canvasGroup.alpha - Time.unscaledDeltaTime / fadeOutDuration;
        }
    }
}