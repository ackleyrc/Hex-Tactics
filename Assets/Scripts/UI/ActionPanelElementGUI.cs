using UnityEngine;
using UnityEngine.UI;

public class ActionPanelElementGUI : MonoBehaviour
{
    public HUDColorPalette colorPalette;

    public Image border;
    public Image background;
    public Image icon;

    private bool isPending = false;

    public void DisplayAsEnabled()
    {
        border.color = colorPalette.ActionEnabledBorder;
        background.color = colorPalette.ActionEnabledBackground;
        icon.color = colorPalette.ActionEnabledIcon;

        isPending = false;
    }

    public void DisplayAsDisabled()
    {
        border.color = colorPalette.ActionDisabledBorder;
        background.color = colorPalette.ActionDisabledBackground;
        icon.color = colorPalette.ActionDisabledIcon;

        isPending = false;
    }

    public void DisplayAsPending()
    {
        isPending = true;
    }

    private void Update()
    {
        if (isPending == true)
        {
            float t = 0.5f * Mathf.Cos(2.0f * Time.time * Mathf.PI) + 0.5f;

            border.color = Color.Lerp(colorPalette.ActionEnabledBorder, colorPalette.ActionDisabledBorder, t);
            background.color = Color.Lerp(colorPalette.ActionEnabledBackground, colorPalette.ActionDisabledBackground, t);
            icon.color = Color.Lerp(colorPalette.ActionEnabledIcon, colorPalette.ActionDisabledIcon, t);
        }
    }
}