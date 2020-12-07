using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    public event System.Action OnRestartGame = delegate { };

    public CanvasGroup canvasGroup;
    public Button resumeButton;
    public Button resetButton;
    public Button newStartButton;
    public Button newMapButton;
    public Button customizeButton;
    public Button exitButton;
    public float fadeInDuration;
    public float fadeOutDuration;

    public bool IsDisplayed { get; private set; } = false;

    private void Start()
    {
        resumeButton.onClick.AddListener(OnClickResumeButton);
        resetButton.onClick.AddListener(OnClickResetButton);
        newStartButton.onClick.AddListener(OnClickNewStartButton);
        newMapButton.onClick.AddListener(OnClickNewMapButton);
        customizeButton.onClick.AddListener(OnClickCustomizeButton);
        exitButton.onClick.AddListener(OnClickExitButton);
    }

    private void Update()
    {
        if (TitleScreenGUI.Instance.IsDisplaying == false &&
            DifficultySelectionGUI.Instance.IsDisplaying == false &&
            CustomScenarioGUI.Instance.IsDisplayed == false)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (IsDisplayed == true)
                {
                    Hide();
                }
                else
                {
                    Display();
                }
            }
        }
    }

    private void OnClickResumeButton()
    {
        Hide();
    }

    private void OnClickResetButton()
    {
        GameManager.Instance.ResetGame();
        DifficultySelectionGUI.Instance.Display();

        Hide(immediate: true);

        OnRestartGame?.Invoke();
    }

    private void OnClickNewStartButton()
    {
        GameManager.Instance.NewStart();
        DifficultySelectionGUI.Instance.Display();

        Hide(immediate: true);

        OnRestartGame?.Invoke();
    }

    private void OnClickNewMapButton()
    {
        GameManager.Instance.NewMap();
        DifficultySelectionGUI.Instance.Display();

        Hide(immediate: true);

        OnRestartGame?.Invoke();
    }

    private void OnClickCustomizeButton()
    {
        CustomScenarioGUI.Instance.Display();
    }

    private void OnClickExitButton()
    {
        Application.Quit();
    }

    public void Display()
    {
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void Hide(bool immediate = false)
    {
        if (immediate)
        {
            IsDisplayed = false;

            Time.timeScale = 1.0f;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup.alpha = 0.0f;
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
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