using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCustomGUI : MonoBehaviour
{
    public Button button;
    public Text textDefault;
    public Text textInverted;

    public void HandlePointerEnter()
    {
        if (button.interactable == true)
        {
            textDefault.gameObject.SetActive(false);
            textInverted.gameObject.SetActive(true);
        }
    }

    public void HandlePointerExit()
    {
        textDefault.gameObject.SetActive(true);
        textInverted.gameObject.SetActive(false);
    }
}