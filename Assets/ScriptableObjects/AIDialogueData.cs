using UnityEngine;

public enum AIContext
{
    NONE,
    GENERIC_MOVE,
    GENERIC_ATTACK,
    MOVE_TO_ATTACK,
    REDUCE_EXPOSURE,
    APPROACH_TARGET,
    TAKE_COVER,
    VULNERABLE_TARGET,
    KILL_SHOT,
    STAY_PUT,
}

[CreateAssetMenu(fileName = "AIDialogueData", menuName = "ScriptableObjects/AIDialogueData", order = 5)]
public class AIDialogueData : ScriptableObject
{
    [SerializeField]
    private ContextualDialogue[] contextualDialogue;
    public ContextualDialogue[] ContextualDialogue { get { return contextualDialogue; } }

    public string GetLine(AIContext context)
    {
        foreach (ContextualDialogue dialogue in ContextualDialogue)
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
public class ContextualDialogue
{
    [SerializeField]
    private AIContext context;
    public AIContext Context { get { return context; } }

    [SerializeField]
    private string[] possibleLines;
    public string[] PossibleLines { get { return possibleLines; } }
}