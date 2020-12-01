using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinConditionGUI : MonoBehaviour
{
#region SINGLETON_MGMT
    private static WinConditionGUI _Instance;
    public static WinConditionGUI Instance { get { return _Instance; } }

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

    public HUDColorPalette colorPalette;

    [Header("Main Canvas Group")]
    public CanvasGroup canvasGroup;
    public float lerpAlphaSpeed;

    [Header("Header")]
    public RectTransform header;
    public Image headerBG;
    public Text headerText;
    public float headerLerpPosSpeed;

    [Header("Sub-Header")]
    public RectTransform subHeader;
    public Image subHeaderBG;
    public Text subHeaderText;
    public float subHeaderLerpPosSpeed;

    [Header("Panel")]
    public RectTransform panel;
    public CanvasGroup panelGroup;
    public Button restartButton;
    public Button exitButton;
    public float panelLerpPosSpeed;
    public float panelLerpAlphaSpeed;

    private bool isDisplaying;
    private bool isRevealingButtons;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        header.localPosition = Vector3.left * header.sizeDelta.x * 2.0f;
        subHeader.localPosition = Vector3.right * subHeader.sizeDelta.x * 2.0f;
        panel.localPosition = Vector3.up * panel.sizeDelta.y;
        panelGroup.alpha = 0.0f;
        restartButton.onClick.AddListener(OnClickRestartButton);
        exitButton.onClick.AddListener(OnClickExitButton);
        isDisplaying = false;
        isRevealingButtons = false;
    }

    public void Display(bool humanPlayerWon)
    {
        if (humanPlayerWon == true)
        {
            headerText.text = "MISSION SUCCESS";
            subHeaderText.text = "All Enemies Eliminated";

            headerBG.color = colorPalette.FriendlyUnitColor;
            subHeaderText.color = colorPalette.FriendlyUnitColor;
        }
        else
        {
            headerText.text = "MISSION FAILED";
            subHeaderText.text = "All Allies Incapacitated";

            headerBG.color = colorPalette.EnemyUnitColor;
            subHeaderText.color = colorPalette.EnemyUnitColor;
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        isDisplaying = true;
    }

    private void Update()
    {
        if (isDisplaying == true)
        {
            if (canvasGroup.alpha > 0.5f)
            {
                if (header.localPosition.x < -0.1f)
                {
                    float posNormalized = 1.0f - Mathf.Abs(header.localPosition.x / (header.sizeDelta.x * 2.0f)) + 0.01f;
                    float t = Mathf.Clamp01(posNormalized + posNormalized * Time.unscaledDeltaTime * headerLerpPosSpeed);
                    header.localPosition = Vector3.Lerp(Vector3.left * header.sizeDelta.x * 2.0f, Vector3.zero, t);
                }

                if (subHeader.localPosition.x > 0.1f)
                {
                    float posNormalized = 1.0f - Mathf.Abs(subHeader.localPosition.x / (subHeader.sizeDelta.x * 2.0f)) + 0.01f;
                    float t = Mathf.Clamp01(posNormalized + posNormalized * Time.unscaledDeltaTime * subHeaderLerpPosSpeed);
                    subHeader.localPosition = Vector3.Lerp(Vector3.right * subHeader.sizeDelta.x * 2.0f, Vector3.zero, t);
                }
            }

            if (canvasGroup.alpha < 0.99f)
            {
                canvasGroup.alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01(canvasGroup.alpha + Time.unscaledDeltaTime * lerpAlphaSpeed));
            }

            if (header.localPosition.x > -0.1f &&
                subHeader.localPosition.x < 0.1f &&
                canvasGroup.alpha > 0.99f)
            {
                if (isRevealingButtons == false)
                {
                    StartCoroutine(RevealButtonsPanel());
                    isRevealingButtons = true;
                }
            }
        }
    }

    private IEnumerator RevealButtonsPanel()
    {
        yield return new WaitForSeconds(1.25f);

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

    private void OnClickRestartButton()
    {
        GameManager.Instance.ResetGame();
        DifficultySelectionGUI.Instance.Display();
        Initialize();

        OnRestartGame?.Invoke();
    }

    private void OnClickExitButton()
    {
        Application.Quit();
    }
}