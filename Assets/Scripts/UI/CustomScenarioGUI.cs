using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomScenarioGUI : MonoBehaviour
{
    #region SINGLETON_MGMT
    private static CustomScenarioGUI _Instance;
    public static CustomScenarioGUI Instance { get { return _Instance; } }

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

    public event System.Action OnStartNewGame = delegate { };

    public bool IsDisplayed { get; private set; } = false;

    [Header("Main Canvas Group")]
    public CanvasGroup canvasGroup;
    public float fadeInDuration;
    public float fadeOutDuration;

    [Header("Buttons")]
    public Button startButton;
    public Button randomizeButton;

    [Header("Base Seed")]
    public Slider baseSeedSlider;
    public Text baseSeedReadout;

    [Header("Wetness Scale")]
    public Slider wetnessScaleSlider;
    public Text wetnessScaleReadout;

    [Header("Wetness Bias")]
    public Slider wetnessBiasSlider;
    public Text wetnessBiasReadout;

    [Header("Vegetation Scale")]
    public Slider vegetationScaleSlider;
    public Text vegetationScaleReadout;

    [Header("Vegetation Bias")]
    public Slider vegetationBiasSlider;
    public Text vegetationBiasReadout;

    [Header("Map ID")]
    public Text mapIdText;

    private void Start()
    {
        randomizeButton.onClick.AddListener(OnClickRandomizeButton);
        startButton.onClick.AddListener(OnClickStartButton);

        baseSeedSlider.onValueChanged.AddListener(OnBaseSeedSliderChanged);
        wetnessScaleSlider.onValueChanged.AddListener(OnWetnessScaleSliderChanged);
        wetnessBiasSlider.onValueChanged.AddListener(OnWetnessBiasSliderChanged);
        vegetationScaleSlider.onValueChanged.AddListener(OnVegetationScaleSliderChanged);
        vegetationBiasSlider.onValueChanged.AddListener(OnVegetationBiasSliderChanged);

        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        IsDisplayed = false;

        UpdateMapId();
    }

    private void Update()
    {
        if (IsDisplayed == true)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Hide();
            }
        }
    }

    private void OnClickStartButton()
    {
        //float wetFreqNrml = 1.0f - (wetnessScaleSlider.value / 64.0f) * 1.0f;
        //float wetBiasNrml = (wetnessBiasSlider.value / 64.0f) * 0.5f;
        //float vegFreqNrml = 1.0f - (vegetationScaleSlider.value / 64.0f) * 1.0f;
        //float vegBiasNrml = (vegetationBiasSlider.value / 64.0f) * 0.5f;

        MapSeedParameters mapParams = new MapSeedParameters
        {
            biome = Biome.TROPICAL,
            baseSeed = (int)baseSeedSlider.value,
            biomeDimensions = new BiomeDimension[]
            {
                new BiomeDimension()
                {
                    scale = (int)wetnessScaleSlider.value,
                    bias = (int)wetnessBiasSlider.value,
                },
                new BiomeDimension()
                {
                    scale = (int)vegetationScaleSlider.value,
                    bias = (int)vegetationBiasSlider.value,
                }
            }
        };

        GameManager.Instance.NewCustomScenario(mapParams);
        DifficultySelectionGUI.Instance.Display();

        IsDisplayed = false;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        canvasGroup.alpha = 0.0f;

        PauseScreenGUI.Instance.Hide(immediate: true);

        OnStartNewGame?.Invoke();
    }

    private void OnClickRandomizeButton()
    {
        baseSeedSlider.SetValueWithoutNotify(Mathf.RoundToInt(Random.Range(baseSeedSlider.minValue, baseSeedSlider.maxValue)));
        wetnessScaleSlider.SetValueWithoutNotify(Mathf.RoundToInt((Random.Range(wetnessScaleSlider.minValue, wetnessScaleSlider.maxValue) + Random.Range(wetnessScaleSlider.minValue, wetnessScaleSlider.maxValue)) * 0.5f));
        wetnessBiasSlider.SetValueWithoutNotify(Mathf.RoundToInt((Random.Range(wetnessBiasSlider.minValue, wetnessBiasSlider.maxValue) + Random.Range(wetnessBiasSlider.minValue, wetnessBiasSlider.maxValue)) * 0.5f));
        vegetationScaleSlider.SetValueWithoutNotify(Mathf.RoundToInt((Random.Range(vegetationScaleSlider.minValue, vegetationScaleSlider.maxValue) + Random.Range(vegetationScaleSlider.minValue, vegetationScaleSlider.maxValue)) * 0.5f));
        vegetationBiasSlider.SetValueWithoutNotify(Mathf.RoundToInt((Random.Range(vegetationBiasSlider.minValue, vegetationBiasSlider.maxValue) + Random.Range(vegetationBiasSlider.minValue, vegetationBiasSlider.maxValue)) * 0.5f));

        UpdateBaseSeedReadout();
        UpdateWetnessScaleReadout();
        UpdateWetnessBiasReadout();
        UpdateVegetationScaleReadout();
        UpdateVegetationBiasReadout();

        UpdateMapId();
    }

    private void OnBaseSeedSliderChanged(float newValue)
    {
        UpdateBaseSeedReadout();
        UpdateMapId();
    }

    private void UpdateBaseSeedReadout()
    {
        baseSeedReadout.text = $"{(int)baseSeedSlider.value}";
    }

    private void OnWetnessScaleSliderChanged(float newValue)
    {
        UpdateWetnessScaleReadout();
        UpdateMapId();
    }

    private void UpdateWetnessScaleReadout()
    {
        wetnessScaleReadout.text = $"{Mathf.RoundToInt(100 * (wetnessScaleSlider.value + 1) / 64.0f)}%";
    }

    private void OnWetnessBiasSliderChanged(float newValue)
    {
        UpdateWetnessBiasReadout();
        UpdateMapId();
    }

    private void UpdateWetnessBiasReadout()
    {
        if (wetnessBiasSlider.value + 31 < 9)
        {
            wetnessBiasReadout.text = "Very Dry";
        }
        else if (wetnessBiasSlider.value + 31 < 18)
        {
            wetnessBiasReadout.text = "Dry";
        }
        else if (wetnessBiasSlider.value + 31 < 27)
        {
            wetnessBiasReadout.text = "Somewhat Dry";
        }
        else if (wetnessBiasSlider.value + 31 < 36)
        {
            wetnessBiasReadout.text = "Moderate";
        }
        else if (wetnessBiasSlider.value + 31 < 45)
        {
            wetnessBiasReadout.text = "Somewhat Wet";
        }
        else if (wetnessBiasSlider.value + 31 < 54)
        {
            wetnessBiasReadout.text = "Wet";
        }
        else // if (newValue + 31 < 63)
        {
            wetnessBiasReadout.text = "Very Wet";
        }
    }

    private void OnVegetationScaleSliderChanged(float newValue)
    {
        UpdateVegetationScaleReadout();
        UpdateMapId();
    }

    private void UpdateVegetationScaleReadout()
    {
        vegetationScaleReadout.text = $"{Mathf.RoundToInt(100 * (vegetationScaleSlider.value + 1) / 64.0f)}%";
    }

    private void OnVegetationBiasSliderChanged(float newValue)
    {
        UpdateVegetationBiasReadout();
        UpdateMapId();
    }

    private void UpdateVegetationBiasReadout()
    {
        if (vegetationBiasSlider.value + 31 < 9)
        {
            vegetationBiasReadout.text = "Barren";
        }
        else if (vegetationBiasSlider.value + 31 < 18)
        {
            vegetationBiasReadout.text = "Sparse";
        }
        else if (vegetationBiasSlider.value + 31 < 27)
        {
            vegetationBiasReadout.text = "Somewhat Sparse";
        }
        else if (vegetationBiasSlider.value + 31 < 36)
        {
            vegetationBiasReadout.text = "Moderate";
        }
        else if (vegetationBiasSlider.value + 31 < 45)
        {
            vegetationBiasReadout.text = "Somewhat Forested";
        }
        else if (vegetationBiasSlider.value + 31 < 54)
        {
            vegetationBiasReadout.text = "Forested";
        }
        else // if (newValue + 31 < 63)
        {
            vegetationBiasReadout.text = "Heavily Forested";
        }
    }

    private void UpdateMapId()
    {
        mapIdText.text = $"MAP ID: {MapIdHelper.GetMapId((int)baseSeedSlider.value, (int)wetnessScaleSlider.value, (int)wetnessBiasSlider.value, (int)vegetationScaleSlider.value, (int)vegetationBiasSlider.value)}";
    }

    #region MAIN_CANVAS_GROUP
    public void Display()
    {
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        IsDisplayed = true;

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
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        yield return null;

        IsDisplayed = false;

        while (canvasGroup.alpha > 0.0f)
        {
            yield return null;

            canvasGroup.alpha = canvasGroup.alpha - Time.unscaledDeltaTime / fadeOutDuration;
        }
    }
    #endregion MAIN_CANVAS_GROUP
}