using UnityEngine;
using UnityEngine.UI;

public class MechNameGUI : MonoBehaviour
{
    public HUDColorPalette colorPalette;

    public Text nameText;
    public Image nameBackground;

    private bool isCurrentUnit;

    public void SetName(string name, Allegiance allegiance)
    {
        nameText.text = $"{allegiance} {name}".ToUpper();
    }

    public void DisplayAsCurrentUnit(bool isCurrentUnit)
    {
        this.isCurrentUnit = isCurrentUnit;
        SetNamePlateColors();
    }

    private void Update()
    {
        if (isCurrentUnit == true)
        {
            SetNamePlateColors();
        }
    }

    private void SetNamePlateColors()
    {
        if (isCurrentUnit == false)
        {
            nameText.color = colorPalette.NamePlateText;
            nameBackground.color = colorPalette.NamePlateBackground;
        }
        else
        {
            float t = 0.5f * Mathf.Cos(2.0f * Time.time * Mathf.PI) + 0.5f;

            if (t > 0.5f)
            {
                nameText.color = colorPalette.NamePlateText;
                nameBackground.color = colorPalette.NamePlateBackground;
            }
            else
            {
                nameText.color = colorPalette.NamePlateTextInverted;
                nameBackground.color = colorPalette.NamePlateBackgroundInverted;
            }
        }
    }
}