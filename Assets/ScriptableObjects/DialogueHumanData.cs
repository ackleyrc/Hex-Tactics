using System.Collections.Generic;
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
    private ContextBasedDialogue[] dialogueContexts;
    public ContextBasedDialogue[] DialogueContexts { get { return dialogueContexts; } }

    public string GetLine(DialogueContext context, CharacterTrait traitFlags)
    {
        foreach (ContextBasedDialogue contextualDialogue in DialogueContexts)
        {
            if (contextualDialogue.DialogueContext == context)
            {
                return contextualDialogue.GetLine(traitFlags);
            }
        }

        return "";
    }
}

[System.Serializable]
public class ContextBasedDialogue
{
    [SerializeField]
    private DialogueContext dialogueContext;
    public DialogueContext DialogueContext { get { return dialogueContext; } }

    [SerializeField]
    private TraitBasedLines[] traitBasedLines;
    public TraitBasedLines[] TraitBasedLines { get { return traitBasedLines; } }

    public string GetLine(CharacterTrait traitFlags)
    {
        List<int> validIndices = new List<int>();

        for (int i = 0; i < traitBasedLines.Length; i++)
        {
            if ((traitBasedLines[i].MatchingTraits & traitFlags) != 0)
            {
                validIndices.Add(i);
            }
        }

        int randIndex = validIndices[Random.Range(0, validIndices.Count)];

        return TraitBasedLines[randIndex].Line;
    }
}

[System.Serializable]
public class TraitBasedLines
{
    [SerializeField]
    private CharacterTrait matchingTraits;
    public CharacterTrait MatchingTraits { get { return matchingTraits; } }

    [SerializeField]
    private string line;
    public string Line { get { return line; } }
}