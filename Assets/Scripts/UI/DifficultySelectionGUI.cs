using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelectionGUI : MonoBehaviour
{
#region SINGLETON_MGMT
    private static DifficultySelectionGUI _Instance;
    public static DifficultySelectionGUI Instance { get { return _Instance; } }

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

    [Header("Main Canvas Group")]
    public CanvasGroup mainCanvasGroup;
    public float lerpAlphaSpeed;

    [Header("Header")]
    public CanvasGroup headerGroup;
    public float headerLerpAlphaSpeed;

    [Header("Panel")]
    public RectTransform panel;
    public CanvasGroup panelGroup;
    public Button easyButton;
    public Button moderateButton;
    public float panelLerpPosSpeed;
    public float panelLerpAlphaSpeed;

    public bool IsDisplaying { get; private set; }
    private bool isHiding = false;
    private bool isRevealingButtons;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        mainCanvasGroup.alpha = 1.0f;
        mainCanvasGroup.interactable = true;
        mainCanvasGroup.blocksRaycasts = true;
        headerGroup.alpha = 0.0f;
        panel.localPosition = Vector3.up * panel.sizeDelta.y;
        panelGroup.alpha = 0.0f;
        easyButton.onClick.AddListener(OnClickEasyButton);
        moderateButton.onClick.AddListener(OnClickModerateButton);
        IsDisplaying = false;
        isHiding = false;
        isRevealingButtons = false;
    }

    public void Display()
    {
        Initialize();

        mainCanvasGroup.interactable = true;
        mainCanvasGroup.blocksRaycasts = true;

        IsDisplaying = true;
        isHiding = false;
    }

    private void Hide()
    {
        mainCanvasGroup.interactable = false;
        mainCanvasGroup.blocksRaycasts = false;

        IsDisplaying = false;
        isHiding = true;
    }

    private void Update()
    {
        if (IsDisplaying == true)
        {
            if (headerGroup.alpha < 0.99f)
            {
                headerGroup.alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01(headerGroup.alpha + Time.unscaledDeltaTime * headerLerpAlphaSpeed));
            }

            if (headerGroup.alpha > 0.99f)
            {
                if (isRevealingButtons == false)
                {
                    StartCoroutine(RevealButtonsPanel());
                    isRevealingButtons = true;
                }
            }
        }
        else if (isHiding == true)
        {
            if (mainCanvasGroup.alpha > 0.01f)
            {
                mainCanvasGroup.alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01(mainCanvasGroup.alpha - Time.unscaledDeltaTime * lerpAlphaSpeed));
            }
        }
    }

    private IEnumerator RevealButtonsPanel()
    {
        yield return new WaitForSeconds(0.5f);

        while (panel.localPosition.y > 0.1f ||
               panelGroup.alpha < 0.99f)
        {
            if (panel.localPosition.y > 0.1f)
            {
                float posNormalized = 1.0f - Mathf.Abs(panel.localPosition.y / panel.sizeDelta.y) + 0.01f;
                float t = Mathf.Clamp01(posNormalized + (Time.unscaledDeltaTime * panelLerpPosSpeed));
                panel.localPosition = Vector3.Lerp(Vector3.up * panel.sizeDelta.y, Vector3.zero, t);
            }

            if (panelGroup.alpha < 0.99f)
            {
                panelGroup.alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01(panelGroup.alpha + Time.unscaledDeltaTime * panelLerpAlphaSpeed));
            }

            yield return null;
        }
    }

    private void OnClickEasyButton()
    {
        MechAIManager.Instance.SetDifficulty(Difficulty.EASY);
        Hide();
    }

    private void OnClickModerateButton()
    {
        MechAIManager.Instance.SetDifficulty(Difficulty.MODERATE);
        Hide();
    }
}