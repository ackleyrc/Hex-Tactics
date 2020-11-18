using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenGUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Button startButton;
    public Button exitButton;
    public float fadeOutDuration;

    private void Start()
    {
        startButton.onClick.AddListener(OnClickStartButton);
        exitButton.onClick.AddListener(OnClickExitButton);
    }

    private void OnClickStartButton()
    {
        StartCoroutine(FadeOut());
    }

    private void OnClickExitButton()
    {
        Application.Quit();
    }

    private IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (canvasGroup.alpha > 0.0f)
        {
            yield return null;

            canvasGroup.alpha = canvasGroup.alpha - Time.unscaledDeltaTime / fadeOutDuration;
        }
    }
}