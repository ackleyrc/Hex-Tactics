using UnityEngine;
using UnityEngine.UI;

public class UnitTurnElementGUI : MonoBehaviour
{
    public HUDColorPalette colorPalette;

    public Text unitName;
    public Image unitNameBG;
    public GameObject currentTurnIndicator;

    public void SetName(Allegiance allegiance, string name)
    {
        unitName.text = $"{allegiance} {name}".ToUpper();
        unitNameBG.color = allegiance == Allegiance.FRIENDLY ? colorPalette.FriendlyUnitColor : colorPalette.EnemyUnitColor;
    }

    public void SetCurrentTurn(bool isCurrentTurn)
    {
        currentTurnIndicator.SetActive(isCurrentTurn);
    }
}