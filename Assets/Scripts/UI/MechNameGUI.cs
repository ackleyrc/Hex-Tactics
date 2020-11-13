using UnityEngine;
using UnityEngine.UI;

public class MechNameGUI : MonoBehaviour
{
    public Text nameText;

    public void SetName(string name, Allegiance allegiance)
    {
        nameText.text = $"{allegiance} {name}".ToUpper();
    }
}