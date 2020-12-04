using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexInfoGUI : MonoBehaviour
{
#region SINGLETON_MGMT
    private static HexInfoGUI _Instance;
    public static HexInfoGUI Instance { get { return _Instance; } }

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
    public Text detailsText;
    public float lerpPosSpeed;
    public float lerpAlphaSpeed;
    public float repopulateOffset;

    private bool isDisplaying;

    private void Start()
    {
        infoPanelRect.localPosition = Vector3.left * infoPanelRect.sizeDelta.x;
        infoPanelGroup.alpha = 0.0f;
        isDisplaying = false;
    }

    public void UpdateInfoPanel(Cube hexCube)
    {
        if (HexGridManager.Instance.IsHexCubeOnMap(hexCube) == true)
        {
            HexTerrainType terrain = HexGridManager.Instance.GetHexTerrainType(hexCube);
            bool isTraversible = HexGridManager.Instance.terrainData.IsTraversible(terrain);
            float movementCost = HexGridManager.Instance.terrainData.GetMovementCost(terrain);
            bool allowsLOS = HexGridManager.Instance.terrainData.AllowsLineOfSight(terrain);
            float defenseMultiplier = HexGridManager.Instance.terrainData.GetDefenseMultiplier(terrain);

            detailsText.text = $"<b>Coordinates:</b>\n\t( {hexCube.q} , {hexCube.r} )\n" +
                               $"<b>Terrain:</b>:\n\t{HexGridManager.Instance.terrainData.GetTerrainDisplayName(terrain)}\n" +
                               $"<b>Movement:</b>\n\t{(isTraversible == false ? "Not Traversible" : (movementCost == 1 ? "100%" : "50%"))}\n" +
                               $"<b>{(allowsLOS ? "Allows Line of Sight" : "Blocks Line of Sight")}</b>\n" +
                               $"{(isTraversible == false ? "<b>Defense Bonus:</b>\n\tn/a" : defenseMultiplier < 1.0f ? "<b>Defense Bonus:</b>\n\t50% Damage Received" : "<b>No Defense Bonus:</b>\n\t100% Damage Received")}";

            /*
            if (isDisplaying == true)
            {
                infoPanelRect.localPosition = infoPanelRect.localPosition + Vector3.left * repopulateOffset;
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
            if (infoPanelRect.localPosition.x < -0.1f)
            {
                float posNormalized = 1.0f - Mathf.Abs(infoPanelRect.localPosition.x / infoPanelRect.sizeDelta.x);
                float t = Mathf.Clamp01(posNormalized + Screen.dpi * (Time.unscaledDeltaTime * lerpPosSpeed));
                infoPanelRect.localPosition = Vector3.Lerp(Vector3.left * infoPanelRect.sizeDelta.x, Vector3.zero, t);
            }

            if (infoPanelGroup.alpha < 0.99f)
            {
                infoPanelGroup.alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01(infoPanelGroup.alpha + Time.unscaledDeltaTime * lerpAlphaSpeed));
            }
        }
        else if (isDisplaying == false)
        {
            if (infoPanelRect.localPosition.x > -infoPanelRect.sizeDelta.x + 0.1f)
            {
                float posNormalized = Mathf.Abs(infoPanelRect.localPosition.x / infoPanelRect.sizeDelta.x);
                float t = Mathf.Clamp01(posNormalized + Screen.dpi * (Time.unscaledDeltaTime * lerpPosSpeed));
                infoPanelRect.localPosition = Vector3.Lerp(Vector3.zero, Vector3.left * infoPanelRect.sizeDelta.x, t);
            }

            if (infoPanelGroup.alpha > 0.01f)
            {
                infoPanelGroup.alpha = Mathf.Lerp(1.0f, 0.0f, Mathf.Clamp01((1.0f - infoPanelGroup.alpha) + Time.unscaledDeltaTime * lerpAlphaSpeed));
            }
        }
    }
}