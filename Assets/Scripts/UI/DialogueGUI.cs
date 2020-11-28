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
    public AIDialogueData aiEasyDialogueData;
    public AIDialogueData aiModerateDialogueData;

    public CanvasGroup panelGroup;

    public float autoTypeSpeed;
    public float dialogueDuration;

    [Header("Portrait")]
    public Image portraitBorder;
    public Image portraitImage;
    public Text characterNameText;

    [Header("Speech")]
    public Image speechBG;
    public Text speechText;

    public void HandleAIDialogue(MechController aiMech, MechController targetMech, Cube currHexPos, Cube newHexPos)
    {
        // Evaluate parameters for AI Dialogue logic
        float defenseMult = targetMech == null ? 0.0f : HexGridManager.Instance.GetDefenseMultiplier(targetMech.GetCurrentHexTile());
        int tentativeDmg = Mathf.RoundToInt(2.0f * defenseMult); // For now, this should be 2 or 1

        float currDefenseMult = HexGridManager.Instance.GetDefenseMultiplier(currHexPos);
        float newDefenseMult = HexGridManager.Instance.GetDefenseMultiplier(newHexPos);

        int currNumOpponentsExposedTo = 0;
        int newNumOpponentsExposedTo = 0;
        foreach (MechController opposingMech in GameManager.Instance.MechFriendlies)
        {
            if (opposingMech.health.CurrentHealth <= 0)
            {
                continue;
            }

            if (GameManager.Instance.CheckForLineOfSight(fromHexTile: currHexPos, opposingMech.GetCurrentHexTile()) == null)
            {
                currNumOpponentsExposedTo++;
            }

            if (GameManager.Instance.CheckForLineOfSight(fromHexTile: newHexPos, opposingMech.GetCurrentHexTile()) == null)
            {
                newNumOpponentsExposedTo++;
            }
        }

        // Determine AI Dialogue Line
        if (targetMech != null &&
            targetMech.health.CurrentHealth - tentativeDmg <= 0.0f)
        {
            DisplayAIDialogueLine(aiMech, AIContext.KILL_SHOT);
        }
        else if (targetMech != null &&
                 targetMech.health.CurrentHealth <= targetMech.health.initialHealth * 0.5f &&
                 (currNumOpponentsExposedTo >= 2 || newNumOpponentsExposedTo >= 2))
        {
            DisplayAIDialogueLine(aiMech, AIContext.VULNERABLE_TARGET);
        }
        else if (newHexPos == currHexPos)
        {
            DisplayAIDialogueLine(aiMech, AIContext.STAY_PUT);
        }
        else if (currDefenseMult == 1.0f && newDefenseMult != 1.0f)
        {
            DisplayAIDialogueLine(aiMech, AIContext.TAKE_COVER);
        }
        else if (newNumOpponentsExposedTo < currNumOpponentsExposedTo)
        {
            DisplayAIDialogueLine(aiMech, AIContext.REDUCE_EXPOSURE);
        }
        else if (targetMech != null && currNumOpponentsExposedTo == 0 && newNumOpponentsExposedTo >= 1)
        {
            DisplayAIDialogueLine(aiMech, AIContext.MOVE_TO_ATTACK);
        }
        else if (targetMech != null)
        {
            int currDistanceToTarget = HexGridManager.Instance.HexGrid.CubeDistance(currHexPos, targetMech.GetCurrentHexTile());
            int newDistanceToTaget = HexGridManager.Instance.HexGrid.CubeDistance(newHexPos, targetMech.GetCurrentHexTile());

            if (newDistanceToTaget < currDistanceToTarget)
            {
                DisplayAIDialogueLine(aiMech, AIContext.APPROACH_TARGET);
            }
        }
    }

    public void DisplayAIDialogueLine(MechController aiMech, AIContext context)
    {
        Debug.Log($"DialogueGUI::DisplayAIDialogueLine( {aiMech} , {context} )");

        portraitImage.sprite = mechDetails.GetPortrait(Allegiance.ENEMY, aiMech.UnitIndex);
        characterNameText.text = aiMech.MechName;

        StopAllCoroutines();
        StartCoroutine(RenderDialogue(aiModerateDialogueData.GetLine(context)));

        Debug.Log($"DialogueGUI :: Dialogue Line: {speechText.text}");
    }

    private IEnumerator RenderDialogue(string line)
    {
        float startTime = Time.time;

        panelGroup.alpha = 1.0f;

        while (Time.time < startTime + dialogueDuration)
        {
            yield return new WaitForSeconds(0.06f);

            int maxChars = Mathf.FloorToInt((Time.time - startTime) * autoTypeSpeed);

            string lineSubstring = maxChars > 2 ? line.Substring(0, Mathf.Clamp(maxChars - 2, 0, line.Length)) : "";
            string postFixA = (maxChars > 4 && lineSubstring.Length < line.Length - 1 - 3) ? $"{(Random.Range(0, 2) < 1 ? 0 : 1)}" : "";
            string postFixB = (maxChars > 3 && lineSubstring.Length < line.Length - 2 - 3) ? $"{(Random.Range(0, 2) < 1 ? 0 : 1)}" : "";
            string postFixC = (maxChars > 2 && lineSubstring.Length < line.Length - 3 - 3) ? $"{(Random.Range(0, 2) < 1 ? 0 : 1)}" : "";
            string postFixD = (maxChars > 1 && lineSubstring.Length < line.Length - 4 - 3) ? $"{(Random.Range(0, 2) < 1 ? 0 : 1)}" : "";

            if (maxChars > 0)
            {
                speechText.text = $"{lineSubstring}{postFixA}{postFixB}{postFixC}{postFixD}";
            }
        }

        panelGroup.alpha = 0.0f;
    }

    /// POSITION
    /// point
    /// area
    /// environment
    /// location
    /// post
    /// bearings
    /// ground
    /// setting
    /// site
    /// station

    /// STANCE
    /// attitude
    /// condition
    /// situation
    /// status
    /// state
    /// disposition
    /// manner
    /// pose

    /// ENTER
    /// get in
    /// move into 
    /// passing into
    /// crossing into
    /// migrating to
    /// proceeding to
    /// relocating to
    /// taking on

    /// COVER
    /// defensive position
    /// defensive environment
    /// sheltered site
    /// guarded area
    /// shielded ground
    /// protected region

    /// APPROACH
    /// advancing on
    /// proceeding to
    /// marching on
    /// gaining ground on
    /// pushing in on
    /// verging upon
    /// drawing near
    /// gaining on

    /// ENEMY
    /// adversary
    /// opponent
    /// opposition
    /// threat
    /// target
    /// mark

    /// REDUCE
    /// minimize
    /// limit
    /// mitigate
    /// moderate
    /// alleviate

    /// EXPOSURE
    /// hazards
    /// liabilities
    /// risks
    /// contingencies
    /// threats
}