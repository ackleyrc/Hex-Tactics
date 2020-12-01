using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenGUI : MonoBehaviour
{
#region SINGLETON_MGMT
    private static TitleScreenGUI _Instance;
    public static TitleScreenGUI Instance { get { return _Instance; } }

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

    public event System.Action OnStartGame = delegate { };

    public CanvasGroup canvasGroup;
    public Button startButton;
    public Button exitButton;
    public float fadeOutDuration;

    public bool IsDisplaying { get; private set; }

    private void Start()
    {
        startButton.onClick.AddListener(OnClickStartButton);
        exitButton.onClick.AddListener(OnClickExitButton);

        IsDisplaying = true;
    }

    private void OnClickStartButton()
    {
        StartCoroutine(FadeOut());

        OnStartGame?.Invoke();
    }

    private void OnClickExitButton()
    {
        Application.Quit();
    }

    private IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0.0f)
        {
            yield return null;

            canvasGroup.alpha = canvasGroup.alpha - Time.unscaledDeltaTime / fadeOutDuration;

            if (canvasGroup.alpha < 0.15f &&
                DifficultySelectionGUI.Instance.IsDisplaying == false)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                DifficultySelectionGUI.Instance.Display();

                IsDisplaying = false;
            }
        }
    }
}