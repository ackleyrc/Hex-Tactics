using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueGUI : MonoBehaviour
{
#region SINGLETON_MGMT
    private static DialogueGUI _Instance;
    public static DialogueGUI Instance { get { return _Instance; } }

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

    public MechEntityDetailsData mechDetails;
    public AIDialogueData aiDialogueData;

    public CanvasGroup panelGroup;

    [Header("Portrait")]
    public Image portraitBorder;
    public Image portraitImage;
    public Text characterNameText;

    [Header("Speech")]
    public Text speechText;
    public Image speechBG;

    public void DisplayAIDialogueLine(MechController aiMech, AIContext context)
    {
        Debug.Log($"DialogueGUI::DisplayAIDialogueLine( {aiMech} , {context} )");

        portraitImage.sprite = mechDetails.GetPortrait(Allegiance.ENEMY, aiMech.UnitIndex);
        characterNameText.text = aiMech.MechName;

        speechText.text = aiDialogueData.GetLine(context);

        Debug.Log($"DialogueGUI :: Dialogue Line: {speechText.text}");
    }
}