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

    [Header("Biome Toggles")]
    public Toggle tropicalToggle;
    public Toggle desertToggle;
    public Toggle shrublandToggle;

    [Header("Base Seed")]
    public Slider baseSeedSlider;
    public Text baseSeedReadout;

    [Header("Dimensions")]
    public GameObject[] scaleHandles;
    public GameObject[] biasHandles;
    public Slider[] scaleSliders;
    public Slider[] biasSliders;
    public Text[] scaleLabels;
    public Text[] biasLabels;
    public Text[] scaleReadouts;
    public Text[] biasReadouts;

    [Header("Map ID")]
    public Text mapIdText;

    private void Start()
    {
        randomizeButton.onClick.AddListener(OnClickRandomizeButton);
        startButton.onClick.AddListener(OnClickStartButton);

        tropicalToggle.onValueChanged.AddListener(OnClickTropicalToggle);
        desertToggle.onValueChanged.AddListener(OnClickDesertToggle);
        shrublandToggle.onValueChanged.AddListener(OnClickShrublandToggle);

        baseSeedSlider.onValueChanged.AddListener(OnBaseSeedSliderChanged);

        scaleSliders[0].onValueChanged.AddListener((float newValue) => { OnScaleSliderChanged(0); });
        biasSliders[0].onValueChanged.AddListener((float newValue) => { OnBiasSliderChanged(0); });

        scaleSliders[1].onValueChanged.AddListener((float newValue) => { OnScaleSliderChanged(1); });
        biasSliders[1].onValueChanged.AddListener((float newValue) => { OnBiasSliderChanged(1); });

        scaleSliders[2].onValueChanged.AddListener((float newValue) => { OnScaleSliderChanged(2); });
        biasSliders[2].onValueChanged.AddListener((float newValue) => { OnBiasSliderChanged(2); });

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
            biome = GetCurrentSelectedBiome(),
            baseSeed = (int)baseSeedSlider.value
        };

        int numDimensions = BiomeData.GetNumDimensions(mapParams.biome);
        mapParams.biomeDimensions = new BiomeDimension[numDimensions];
        for (int i = 0; i < numDimensions; i++)
        {
            mapParams.biomeDimensions[i] = new BiomeDimension
            {
                scale = (int)scaleSliders[i].value,
                bias = (int)biasSliders[i].value,
            };
        }

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
        /*
        Biome currentBiome = GetCurrentSelectedBiome();
        Biome nextBiome = currentBiome;

        while (nextBiome == currentBiome)
        {
            nextBiome = (Biome)Random.Range(0, 3);
        }
        */

        switch ((Biome)Random.Range(0, 3))
        {
            case Biome.TROPICAL:

                tropicalToggle.SetIsOnWithoutNotify(true);
                desertToggle.SetIsOnWithoutNotify(false);
                shrublandToggle.SetIsOnWithoutNotify(false);

                tropicalToggle.interactable = false;
                desertToggle.interactable = true;
                shrublandToggle.interactable = true;

                break;

            case Biome.DESERT:

                tropicalToggle.SetIsOnWithoutNotify(false);
                desertToggle.SetIsOnWithoutNotify(true);
                shrublandToggle.SetIsOnWithoutNotify(false);

                tropicalToggle.interactable = true;
                desertToggle.interactable = false;
                shrublandToggle.interactable = true;

                break;

            case Biome.SHRUBLAND:

                tropicalToggle.SetIsOnWithoutNotify(false);
                desertToggle.SetIsOnWithoutNotify(false);
                shrublandToggle.SetIsOnWithoutNotify(true);

                tropicalToggle.interactable = true;
                desertToggle.interactable = true;
                shrublandToggle.interactable = false;

                break;
        }

        UpdateNumSliders();

        baseSeedSlider.SetValueWithoutNotify(Mathf.RoundToInt(Random.Range(baseSeedSlider.minValue, baseSeedSlider.maxValue)));
        UpdateBaseSeedReadout();

        for (int i = 0; i < BiomeData.GetNumDimensions(GetCurrentSelectedBiome()); i++)
        {
            scaleSliders[i].SetValueWithoutNotify(Mathf.RoundToInt((Random.Range(scaleSliders[i].minValue, scaleSliders[i].maxValue) + Random.Range(scaleSliders[i].minValue, scaleSliders[i].maxValue)) * 0.5f));
            biasSliders[i].SetValueWithoutNotify(Mathf.RoundToInt((Random.Range(biasSliders[i].minValue, biasSliders[i].maxValue) + Random.Range(biasSliders[i].minValue, biasSliders[i].maxValue)) * 0.5f));

            UpdateScaleLabel(i);
            UpdateScaleReadout(i);

            UpdateBiasLabel(i);
            UpdateBiasReadout(i);
        }

        UpdateMapId();
    }

    private void OnClickTropicalToggle(bool newValue)
    {
        //tropicalToggle.SetIsOnWithoutNotify(false);
        desertToggle.SetIsOnWithoutNotify(false);
        shrublandToggle.SetIsOnWithoutNotify(false);

        tropicalToggle.interactable = false;
        desertToggle.interactable = true;
        shrublandToggle.interactable = true;

        UpdateNumSliders();
        UpdateLabelsAndReadouts();
        UpdateMapId();
    }

    private void OnClickDesertToggle(bool newValue)
    {
        tropicalToggle.SetIsOnWithoutNotify(false);
        //desertToggle.SetIsOnWithoutNotify(false);
        shrublandToggle.SetIsOnWithoutNotify(false);

        tropicalToggle.interactable = true;
        desertToggle.interactable = false;
        shrublandToggle.interactable = true;

        UpdateNumSliders();
        UpdateLabelsAndReadouts();
        UpdateMapId();
    }

    private void OnClickShrublandToggle(bool newValue)
    {
        tropicalToggle.SetIsOnWithoutNotify(false);
        desertToggle.SetIsOnWithoutNotify(false);
        //shrublandToggle.SetIsOnWithoutNotify(false);

        tropicalToggle.interactable = true;
        desertToggle.interactable = true;
        shrublandToggle.interactable = false;

        UpdateNumSliders();
        UpdateLabelsAndReadouts();
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

    private void UpdateLabelsAndReadouts()
    {
        for (int i = 0; i < scaleHandles.Length; i++)
        {
            UpdateScaleLabel(i);
            UpdateScaleReadout(i);

            UpdateBiasLabel(i);
            UpdateBiasReadout(i);
        }
    }

    private void UpdateScaleLabel(int index)
    {
        switch (GetCurrentSelectedBiome())
        {
            case Biome.TROPICAL:
                scaleLabels[index].text = $"{(TropicalDimensions)index} SCALE";
                break;
            case Biome.DESERT:
                scaleLabels[index].text = $"{(DesertDimensions)index} SCALE";
                break;
            case Biome.SHRUBLAND:
                scaleLabels[index].text = $"{(ShrublandDimensions)index} SCALE";
                break;
            default:
                scaleLabels[index].text = $"DIMENSION [{index}] SCALE";
                break;
        }
    }

    private void OnScaleSliderChanged(int index)
    {
        UpdateScaleReadout(index);
        UpdateMapId();
    }

    private void UpdateScaleReadout(int index)
    {
        scaleReadouts[index].text = $"{Mathf.RoundToInt(100 * (scaleSliders[index].value + 1) / 64.0f)}%";
    }

    private void UpdateBiasLabel(int index)
    {
        switch (GetCurrentSelectedBiome())
        {
            case Biome.TROPICAL:
                biasLabels[index].text = $"{(TropicalDimensions)index} BIAS";
                break;
            case Biome.DESERT:
                biasLabels[index].text = $"{(DesertDimensions)index} BIAS";
                break;
            case Biome.SHRUBLAND:
                biasLabels[index].text = $"{(ShrublandDimensions)index} BIAS";
                break;
            default:
                biasLabels[index].text = $"DIMENSION [{index}] BIAS";
                break;
        }
    }

    private void OnBiasSliderChanged(int index)
    {
        UpdateBiasReadout(index);
        UpdateMapId();
    }

    private void UpdateBiasReadout(int index)
    {
        Biome biome = GetCurrentSelectedBiome();

        if (biome == Biome.TROPICAL &&
            (TropicalDimensions)index == TropicalDimensions.WETNESS)
        {
            if (biasSliders[index].value + 31 < 9)
            {
                biasReadouts[index].text = "Very Dry";
            }
            else if (biasSliders[index].value + 31 < 18)
            {
                biasReadouts[index].text = "Dry";
            }
            else if (biasSliders[index].value + 31 < 27)
            {
                biasReadouts[index].text = "Somewhat Dry";
            }
            else if (biasSliders[index].value + 31 < 36)
            {
                biasReadouts[index].text = "Moderate";
            }
            else if (biasSliders[index].value + 31 < 45)
            {
                biasReadouts[index].text = "Somewhat Wet";
            }
            else if (biasSliders[index].value + 31 < 54)
            {
                biasReadouts[index].text = "Wet";
            }
            else // if (newValue + 31 < 63)
            {
                biasReadouts[index].text = "Very Wet";
            }
        }
        else if (biome == Biome.TROPICAL &&
                 (TropicalDimensions)index == TropicalDimensions.VEGETATION)
        {
            if (biasSliders[index].value + 31 < 9)
            {
                biasReadouts[index].text = "Barren";
            }
            else if (biasSliders[index].value + 31 < 18)
            {
                biasReadouts[index].text = "Sparse";
            }
            else if (biasSliders[index].value + 31 < 27)
            {
                biasReadouts[index].text = "Somewhat Sparse";
            }
            else if (biasSliders[index].value + 31 < 36)
            {
                biasReadouts[index].text = "Moderate";
            }
            else if (biasSliders[index].value + 31 < 45)
            {
                biasReadouts[index].text = "Somewhat Forested";
            }
            else if (biasSliders[index].value + 31 < 54)
            {
                biasReadouts[index].text = "Forested";
            }
            else // if (newValue + 31 < 63)
            {
                biasReadouts[index].text = "Heavily Forested";
            }
        }
        else if ((biome == Biome.DESERT || biome == Biome.SHRUBLAND) &&
                 ((DesertDimensions)index == DesertDimensions.ELEVATION) || (ShrublandDimensions)index == ShrublandDimensions.ELEVATION)
        {
            if (biasSliders[index].value + 31 < 9)
            {
                biasReadouts[index].text = "Very Low";
            }
            else if (biasSliders[index].value + 31 < 18)
            {
                biasReadouts[index].text = "Low";
            }
            else if (biasSliders[index].value + 31 < 27)
            {
                biasReadouts[index].text = "Somewhat Low";
            }
            else if (biasSliders[index].value + 31 < 36)
            {
                biasReadouts[index].text = "Moderate";
            }
            else if (biasSliders[index].value + 31 < 45)
            {
                biasReadouts[index].text = "Somewhat High";
            }
            else if (biasSliders[index].value + 31 < 54)
            {
                biasReadouts[index].text = "High";
            }
            else // if (newValue + 31 < 63)
            {
                biasReadouts[index].text = "Very High";
            }
        }
        else if ((biome == Biome.DESERT || biome == Biome.SHRUBLAND) &&
                 ((DesertDimensions)index == DesertDimensions.VEGETATION) || (ShrublandDimensions)index == ShrublandDimensions.VEGETATION)
        {
            if (biasSliders[index].value + 31 < 9)
            {
                biasReadouts[index].text = "Barren";
            }
            else if (biasSliders[index].value + 31 < 18)
            {
                biasReadouts[index].text = "Sparse";
            }
            else if (biasSliders[index].value + 31 < 27)
            {
                biasReadouts[index].text = "Somewhat Sparse";
            }
            else if (biasSliders[index].value + 31 < 36)
            {
                biasReadouts[index].text = "Moderate";
            }
            else if (biasSliders[index].value + 31 < 45)
            {
                biasReadouts[index].text = "Somewhat Lush";
            }
            else if (biasSliders[index].value + 31 < 54)
            {
                biasReadouts[index].text = "Lush";
            }
            else // if (newValue + 31 < 63)
            {
                biasReadouts[index].text = "Very Lush";
            }
        }
        else
        {
            if (biasSliders[index].value + 31 < 9)
            {
                biasReadouts[index].text = "Very Low";
            }
            else if (biasSliders[index].value + 31 < 18)
            {
                biasReadouts[index].text = "Low";
            }
            else if (biasSliders[index].value + 31 < 27)
            {
                biasReadouts[index].text = "Somewhat Low";
            }
            else if (biasSliders[index].value + 31 < 36)
            {
                biasReadouts[index].text = "Moderate";
            }
            else if (biasSliders[index].value + 31 < 45)
            {
                biasReadouts[index].text = "Somewhat High";
            }
            else if (biasSliders[index].value + 31 < 54)
            {
                biasReadouts[index].text = "High";
            }
            else // if (newValue + 31 < 63)
            {
                biasReadouts[index].text = "Very High";
            }
        }
    }

    private void UpdateNumSliders()
    {
        int numDimensions = BiomeData.GetNumDimensions(Biome.TROPICAL);

        for (int i = 0; i < scaleHandles.Length; i++)
        {
            scaleHandles[i].SetActive(i < numDimensions);
            biasHandles[i].SetActive(i < numDimensions);
        }
    }

    private void UpdateMapId()
    {
        Biome currentBiome = GetCurrentSelectedBiome();

        mapIdText.text = $"MAP ID: {MapIdHelper.GetMapId(currentBiome, (int)baseSeedSlider.value, (int)scaleSliders[0].value, (int)biasSliders[0].value, (int)scaleSliders[1].value, (int)biasSliders[1].value, (int)scaleSliders[2].value, (int)biasSliders[2].value)}";
    }

    private Biome GetCurrentSelectedBiome()
    {
        return tropicalToggle.isOn ? Biome.TROPICAL : desertToggle.isOn ? Biome.DESERT : Biome.SHRUBLAND;
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