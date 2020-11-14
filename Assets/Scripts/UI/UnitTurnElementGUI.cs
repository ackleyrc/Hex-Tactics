using UnityEngine;
using UnityEngine.UI;

public class UnitTurnElementGUI : MonoBehaviour
{
    public HUDColorPalette colorPalette;

    public Text unitName;
    public Image unitNameBG;
    public GameObject currentTurnIndicator;
    public Image currentTurnBG;
    public Text currentTurnText;

    private bool isCurrentUnit;

    public void SetName(Allegiance allegiance, string name)
    {
        unitName.text = $"{allegiance} {name}".ToUpper();
        unitNameBG.color = allegiance == Allegiance.FRIENDLY ? colorPalette.FriendlyUnitColor : colorPalette.EnemyUnitColor;
    }

    public void SetCurrentTurn(bool isCurrentTurn)
    {
        currentTurnIndicator.SetActive(isCurrentTurn);
        isCurrentUnit = isCurrentTurn;
        SetCurrentTurnColors();
    }

    private void Update()
    {
        if (isCurrentUnit == true)
        {
            SetCurrentTurnColors();
        }
    }

    private void SetCurrentTurnColors()
    {
        if (isCurrentUnit == false)
        {
            currentTurnText.color = colorPalette.NamePlateText;
            currentTurnBG.color = colorPalette.NamePlateBackground;
        }
        else
        {
            float t = 0.5f * Mathf.Cos(2.0f * Time.time * Mathf.PI) + 0.5f;

            if (t > 0.5f)
            {
                currentTurnText.color = colorPalette.NamePlateText;
                currentTurnBG.color = colorPalette.NamePlateBackground;
            }
            else
            {
                currentTurnText.color = colorPalette.NamePlateTextInverted;
                currentTurnBG.color = colorPalette.NamePlateBackgroundInverted;
            }
        }
    }
}