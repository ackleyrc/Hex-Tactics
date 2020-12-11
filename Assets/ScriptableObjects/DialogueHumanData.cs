using UnityEngine;

public enum DialogueContext
{
    NONE,
    GENERIC_MOVE,
    GENERIC_ATTACK,
    MOVE_TO_ATTACK,
    REDUCE_EXPOSURE,
    APPROACH_ENEMY,
    TAKE_COVER,
    VULNERABLE_TARGET,
    KILL_SHOT,
    STAY_PUT,
    ATTACK_NEARBY_TARGET,
    NO_TARGET,
    TEAMMATE_DOWN,
}

[CreateAssetMenu(fileName = "DialogueHumanData", menuName = "ScriptableObjects/DialogueHumanData", order = 0)]
public class DialogueHumanData : ScriptableObject
{
    [SerializeField]
    private HumanDialogue[] humanDialogue;
    public HumanDialogue[] HumanDialogue { get { return humanDialogue; } }

    public string GetLine(DialogueContext context)
    {
        foreach (HumanDialogue dialogue in HumanDialogue)
        {
            if (dialogue.Context == context)
            {
                int randIndex = Random.Range(0, dialogue.PossibleLines.Length);
                return dialogue.PossibleLines[randIndex];
            }
        }

        return "";
    }
}

[System.Serializable]
public class HumanDialogue
{
    [SerializeField]
    private DialogueContext context;
    public DialogueContext Context { get { return context; } }

    [SerializeField]
    private string[] possibleLines;
    public string[] PossibleLines { get { return possibleLines; } }
}