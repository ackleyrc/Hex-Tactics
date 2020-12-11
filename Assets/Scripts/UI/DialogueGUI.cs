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

    public HUDColorPalette colorPalette;
    public MechEntityDetailsData mechDetails;
    public DialogueHumanData humanDialogueData;
    public AIDialogueData aiEasyDialogueData;
    public AIDialogueData aiModerateDialogueData;

    public CanvasGroup panelGroup;

    public float humanAutoTypeSpeed;
    public float humanDialogueLingerDuration;
    public float humanDialogueMaxDuration;

    public float aiAutoTypeSpeed;
    public float aiDialogueLingerDuration;
    public float aiDialogueMaxDuration;

    [Header("Portrait")]
    public Image portraitBorder;
    public Image portraitImage;
    public Text characterNameText;

    [Header("Speech")]
    public Image speechBG;
    public Text speechText;

    private bool isPlayingDialogue = false;

    private void Start()
    {
        panelGroup.alpha = 0.0f;
    }

    public bool HandleHumanMechMoveDialogue(MechController humanMech, Cube currHexPos, Cube newHexPos)
    {
        // Take Cover
        // Approach Enemy
        // Generic Move
        return false;
    }

    public bool HandleHumanMechAttackDialogue(MechController humanMech, MechController targetMech)
    {
        // Kill Shot
        // Vulnerable Target
        // Generic Attack
        return false;
    }

    public CustomYieldInstruction HandleHumanMechEndTurnDialogue(MechController humanMech)
    {
        DisplayHumanDialogueLine(humanMech, DialogueContext.STAY_PUT);

        return new WaitWhile(() => isPlayingDialogue);
    }

    public bool HandleHumanMechFallenTeammateDialogue(MechController fallenMech, Cube currHexPos, Cube newHexPos)
    {
        // Teammate Down
        return false;
    }

    private void DisplayHumanDialogueLine(MechController humanMech, DialogueContext context)
    {
        Debug.Log($"DialogueGUI::DisplayHumanDialogueLine( {humanMech} , {context} )");

        characterNameText.text = humanMech.MechName;
        portraitImage.sprite = mechDetails.GetPortrait(Allegiance.FRIENDLY, humanMech.UnitIndex);
        portraitBorder.color = colorPalette.FriendlyUnitColor;
        speechBG.color = colorPalette.FriendlyUnitColor;

        AudioManager.Instance.StopAIDialogue();

        StopAllCoroutines();
        StartCoroutine(RenderHumanDialogue(humanDialogueData.GetLine(context)));

        Debug.Log($"DialogueGUI :: Human Dialogue Line: {speechText.text}");
    }

    private IEnumerator RenderHumanDialogue(string line)
    {
        isPlayingDialogue = true;

        float startTime = Time.time;
        speechText.text = "";
        panelGroup.alpha = 1.0f;

        int maxChars = 0;

        while (Time.time < startTime + humanDialogueMaxDuration && maxChars <= line.Length + 2)
        {
            yield return new WaitForSeconds(0.06f);

            maxChars = Mathf.FloorToInt((Time.time - startTime) * humanAutoTypeSpeed);

            string lineSubstring = maxChars > 2 ? line.Substring(0, Mathf.Clamp(maxChars - 2, 0, line.Length)) : "";
            string postFixA = (maxChars > 4 && lineSubstring.Length < line.Length - 1 - 3) ? $"{(Random.Range(0, 5) < 1 ? "_" : "")}" : "";
            string postFixB = (maxChars > 3 && lineSubstring.Length < line.Length - 2 - 3) ? $"{(Random.Range(0, 4) < 1 ? "_" : "")}" : "";
            string postFixC = (maxChars > 2 && lineSubstring.Length < line.Length - 3 - 3) ? $"{(Random.Range(0, 3) < 1 ? "_" : "")}" : "";
            string postFixD = (maxChars > 1 && lineSubstring.Length < line.Length - 4 - 3) ? $"{(Random.Range(0, 2) < 1 ? "_" : "")}" : "";

            if (maxChars > 0)
            {
                speechText.text = $"{lineSubstring}{postFixA}{postFixB}{postFixC}{postFixD}";
            }

            if (maxChars > 1 && maxChars < line.Length)
            {
                AudioManager.Instance.PlayAIDialogue();
            }
            else if (maxChars >= line.Length)
            {
                AudioManager.Instance.StopAIDialogue();
            }
        }

        AudioManager.Instance.StopAIDialogue();

        yield return new WaitForSeconds(humanDialogueLingerDuration);

        panelGroup.alpha = 0.0f;

        isPlayingDialogue = false;
    }

    public void HandleModerateAIDialogue(MechController aiMech, MechController targetMech, Cube currHexPos, Cube newHexPos)
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
            DisplayAIDialogueLine(aiMech, AIContext.KILL_SHOT, Difficulty.MODERATE);
        }
        else if (targetMech != null &&
                 targetMech.health.CurrentHealth <= targetMech.health.initialHealth * 0.5f &&
                 (currNumOpponentsExposedTo >= 2 || newNumOpponentsExposedTo >= 2))
        {
            DisplayAIDialogueLine(aiMech, AIContext.VULNERABLE_TARGET, Difficulty.MODERATE);
        }
        else if (newHexPos == currHexPos)
        {
            DisplayAIDialogueLine(aiMech, AIContext.STAY_PUT, Difficulty.MODERATE);
        }
        else if (currDefenseMult == 1.0f && newDefenseMult != 1.0f)
        {
            DisplayAIDialogueLine(aiMech, AIContext.TAKE_COVER, Difficulty.MODERATE);
        }
        else if (newNumOpponentsExposedTo < currNumOpponentsExposedTo)
        {
            DisplayAIDialogueLine(aiMech, AIContext.REDUCE_EXPOSURE, Difficulty.MODERATE);
        }
        else if (targetMech != null && currNumOpponentsExposedTo == 0 && newNumOpponentsExposedTo >= 1)
        {
            DisplayAIDialogueLine(aiMech, AIContext.MOVE_TO_ATTACK, Difficulty.MODERATE);
        }
        else if (targetMech != null)
        {
            int currDistanceToTarget = HexGridManager.Instance.HexGrid.CubeDistance(currHexPos, targetMech.GetCurrentHexTile());
            int newDistanceToTaget = HexGridManager.Instance.HexGrid.CubeDistance(newHexPos, targetMech.GetCurrentHexTile());

            if (newDistanceToTaget < currDistanceToTarget)
            {
                DisplayAIDialogueLine(aiMech, AIContext.APPROACH_TARGET, Difficulty.MODERATE);
            }
        }
    }

    public void HandleEasyAIDialogue(MechController aiMech, MechController targetMech, Cube currHexPos, Cube newHexPos)
    {
        // Evaluate parameters for AI Dialogue logic
        float defenseMult = targetMech == null ? 0.0f : HexGridManager.Instance.GetDefenseMultiplier(targetMech.GetCurrentHexTile());
        //Debug.Log($"DialogueGUI :: Defense Multiplier: {defenseMult} for target {targetMech?.MechName} at {targetMech?.GetCurrentHexTile()}");
        int tentativeDmg = Mathf.RoundToInt(2.0f * defenseMult); // For now, this should be 2 or 1
        //Debug.Log($"DialogueGUI :: Tentative Damage: {tentativeDmg}");

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
        if (targetMech == null &&
            newHexPos == currHexPos)
        {
            DisplayAIDialogueLine(aiMech, AIContext.NO_TARGET, Difficulty.EASY);
        }
        else if (targetMech != null &&
                 targetMech.health.CurrentHealth - tentativeDmg <= 0.0f)
        {
            DisplayAIDialogueLine(aiMech, AIContext.KILL_SHOT, Difficulty.EASY);
        }
        else if (targetMech != null &&
                 currHexPos != newHexPos)
        {
            DisplayAIDialogueLine(aiMech, AIContext.MOVE_TO_ATTACK, Difficulty.EASY);
        }
        else if (targetMech != null &&
                 targetMech.health.CurrentHealth <= targetMech.health.initialHealth * 0.5f &&
                 (currNumOpponentsExposedTo >= 2 || newNumOpponentsExposedTo >= 2))
        {
            DisplayAIDialogueLine(aiMech, AIContext.VULNERABLE_TARGET, Difficulty.EASY);
        }
        else if (targetMech != null &&
                 (currNumOpponentsExposedTo >= 2 || newNumOpponentsExposedTo >= 2))
        {
            DisplayAIDialogueLine(aiMech, AIContext.ATTACK_NEARBY_TARGET, Difficulty.EASY);
        }
        else if (targetMech != null &&
                 Random.Range(0, 2) >= 1)
        {
            DisplayAIDialogueLine(aiMech, AIContext.GENERIC_ATTACK, Difficulty.EASY);
        }
        else if (targetMech != null &&
                 newHexPos == currHexPos)
        {
            DisplayAIDialogueLine(aiMech, AIContext.STAY_PUT, Difficulty.EASY);
        }
    }

    private void DisplayAIDialogueLine(MechController aiMech, AIContext context, Difficulty difficulty)
    {
        Debug.Log($"DialogueGUI::DisplayAIDialogueLine( {aiMech} , {context} )");

        characterNameText.text = aiMech.MechName;
        portraitImage.sprite = mechDetails.GetPortrait(Allegiance.ENEMY, aiMech.UnitIndex);
        portraitBorder.color = colorPalette.EnemyUnitColor;
        speechBG.color = colorPalette.EnemyUnitColor;

        AudioManager.Instance.StopAIDialogue();

        StopAllCoroutines();
        StartCoroutine(RenderAIDialogue(difficulty == Difficulty.MODERATE ? aiModerateDialogueData.GetLine(context) : aiEasyDialogueData.GetLine(context)));

        Debug.Log($"DialogueGUI :: AI Dialogue Line: {speechText.text}");
    }

    private IEnumerator RenderAIDialogue(string line)
    {
        float startTime = Time.time;
        speechText.text = "";
        panelGroup.alpha = 1.0f;

        int maxChars = 0;

        while (Time.time < startTime + aiDialogueMaxDuration && maxChars <= line.Length + 2)
        {
            yield return new WaitForSeconds(0.06f);

            maxChars = Mathf.FloorToInt((Time.time - startTime) * aiAutoTypeSpeed);

            string lineSubstring = maxChars > 2 ? line.Substring(0, Mathf.Clamp(maxChars - 2, 0, line.Length)) : "";
            string postFixA = (maxChars > 4 && lineSubstring.Length < line.Length - 1 - 3) ? $"{(Random.Range(0, 2) < 1 ? 0 : 1)}" : "";
            string postFixB = (maxChars > 3 && lineSubstring.Length < line.Length - 2 - 3) ? $"{(Random.Range(0, 2) < 1 ? 0 : 1)}" : "";
            string postFixC = (maxChars > 2 && lineSubstring.Length < line.Length - 3 - 3) ? $"{(Random.Range(0, 2) < 1 ? 0 : 1)}" : "";
            string postFixD = (maxChars > 1 && lineSubstring.Length < line.Length - 4 - 3) ? $"{(Random.Range(0, 2) < 1 ? 0 : 1)}" : "";

            if (maxChars > 0)
            {
                speechText.text = $"{lineSubstring}{postFixA}{postFixB}{postFixC}{postFixD}";
            }

            if (maxChars > 1 && maxChars < line.Length)
            {
                AudioManager.Instance.PlayAIDialogue();
            }
            else if (maxChars >= line.Length)
            {
                AudioManager.Instance.StopAIDialogue();
            }
        }

        AudioManager.Instance.StopAIDialogue();

        yield return new WaitForSeconds(aiDialogueLingerDuration);

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

    /// ATTACK
    /// assailing
    /// assaulting
    /// striking
    /// firing
    /// opening fire
    /// targeting

    /// NEARBY
    /// adjacent
    /// proximal
    /// at hand
    /// close
    
    /// NO TARGET
    /// Sensors return negative...
    /// No threats in sight...
    /// No targets in range...
    /// Enemies not detected...
}