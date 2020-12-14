using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoGUI : MonoBehaviour
{
    #region SINGLETON_MGMT
    private static CharacterInfoGUI _Instance;
    public static CharacterInfoGUI Instance { get { return _Instance; } }

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

    public RectTransform infoPanelRect;
    public CanvasGroup infoPanelGroup;
    public Image avatarImage;
    public Text toplineDetailsText;
    public Text profileDetailsText;
    public float lerpPosSpeed;
    public float lerpAlphaSpeed;

    private bool isDisplaying;

    private void Start()
    {
        Debug.Log($"CharacterInfoGUI :: Start()");
        Debug.Log($"CharacterInfoGUI :: Target: {Vector3.right * infoPanelRect.sizeDelta.x}");
        infoPanelRect.localPosition = Vector3.right * infoPanelRect.sizeDelta.x;
        Debug.Log($"CharacterInfoGUI :: Position: {infoPanelRect.localPosition}");
        infoPanelGroup.alpha = 0.0f;
        isDisplaying = false;
    }

    public void PopulateHumanPilotInfo(MechController humanPilot, Cube currentHex)
    {
        if (HexGridManager.Instance.IsHexCubeOnMap(currentHex) == true)
        {
            avatarImage.sprite = GameManager.Instance.GetHumanPilotSprite(humanPilot.UnitIndex);

            HexTerrainType terrain = HexGridManager.Instance.GetHexTerrainType(currentHex);
            float defenseMultiplier = HexGridManager.Instance.terrainData.GetDefenseMultiplier(terrain);

            toplineDetailsText.text = $"<size=32><b>{humanPilot.MechName.ToUpper()}</b></size>\n\n" +
                                      $"<b>STATUS EFFECTS:</b>\n\n" +
                                      $"<b>{(defenseMultiplier < 1.0f ? "Terrain Defense Bonus" : "Open Terrain")}</b>\n" +
                                      $"{(defenseMultiplier < 1.0f ? "50% Damage Received" : "100% Damage Received")}";

            profileDetailsText.text = $"<b>PROFILE:</b>\n\n" +
                                      $"{GameManager.Instance.GetHumanPilotProfileDescription(humanPilot.UnitIndex)}";

            /*
            if (isDisplaying == true)
            {
                infoPanelRect.localPosition = infoPanelRect.localPosition + Vector3.right * repopulateOffset;
            }
            */

            isDisplaying = true;
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        isDisplaying = false;
    }

    private void Update()
    {
        if (isDisplaying == true)
        {
            if (infoPanelRect.localPosition.x > 0.1f)
            {
                float posNormalized = Mathf.Abs(infoPanelRect.localPosition.x / infoPanelRect.sizeDelta.x);
                float t = Mathf.Clamp01(posNormalized - Screen.dpi * (Time.unscaledDeltaTime * lerpPosSpeed));
                infoPanelRect.localPosition = Vector3.Lerp(Vector3.zero, Vector3.right * infoPanelRect.sizeDelta.x, t);
            }

            if (infoPanelGroup.alpha < 0.99f)
            {
                infoPanelGroup.alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01(infoPanelGroup.alpha + Time.unscaledDeltaTime * lerpAlphaSpeed));
            }
        }
        else if (isDisplaying == false)
        {
            if (infoPanelRect.localPosition.x < infoPanelRect.sizeDelta.x - 0.1f)
            {
                float posNormalized = Mathf.Abs(infoPanelRect.localPosition.x / infoPanelRect.sizeDelta.x);
                float t = Mathf.Clamp01(posNormalized + Screen.dpi * (Time.unscaledDeltaTime * lerpPosSpeed));
                infoPanelRect.localPosition = Vector3.Lerp(Vector3.zero, Vector3.right * infoPanelRect.sizeDelta.x, t);
            }

            if (infoPanelGroup.alpha > 0.01f)
            {
                infoPanelGroup.alpha = Mathf.Lerp(1.0f, 0.0f, Mathf.Clamp01((1.0f - infoPanelGroup.alpha) + Time.unscaledDeltaTime * lerpAlphaSpeed));
            }
        }
    }
}