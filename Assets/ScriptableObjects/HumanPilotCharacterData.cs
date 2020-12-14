using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum CharacterTrait
{
    COOL_HEADED     = (1 << 0),
    HOT_TEMPERED    = (1 << 1),
    HUMOROUS        = (1 << 2),
    SERIOUS         = (1 << 3),
    LITERAL         = (1 << 4),
    METAPHORICAL    = (1 << 5),
    OPTIMIST        = (1 << 6),
    PESSIMIST       = (1 << 7),
    NAIVE           = (1 << 8),
    VENGEFUL        = (1 << 9),
}

[CreateAssetMenu(fileName = "HumanPilotCharacterData", menuName = "ScriptableObjects/HumanPilotCharacterData", order = 0)]
public class HumanPilotCharacterData : ScriptableObject
{
    [SerializeField]
    private CharacterProfile[] potentialProfiles;
    public CharacterProfile[] PotentialProfiles { get { return potentialProfiles; } }

    public List<CharacterProfile> GetRandomCharacterProfiles(int numCharacters)
    {
        List<int> selectedIndices = new List<int>();

        selectedIndices.Add(Random.Range(0, potentialProfiles.Length));

        while (selectedIndices.Count < numCharacters)
        {
            int nextIndex = Random.Range(0, potentialProfiles.Length);

            if (selectedIndices.Contains(nextIndex) == false)
            {
                selectedIndices.Add(nextIndex);
            }
        }

        List<CharacterProfile> selectedCharacters = new List<CharacterProfile>();

        foreach (int index in selectedIndices)
        {
            selectedCharacters.Add(PotentialProfiles[index]);
        }

        return selectedCharacters;
    }
}

[System.Serializable]
public class CharacterProfile
{
    [SerializeField]
    private CharacterTrait traits;
    public CharacterTrait Traits { get { return traits; } }

    [SerializeField] [TextArea]
    private string description;
    public string Description { get { return description; } }

    public string GetFullyRenderedDescription(string name, Gender gender)
    {
        string fullDescription = Description;

        fullDescription = fullDescription.Replace("#Name#", name);
        fullDescription = fullDescription.Replace("#Pronoun-subject#", gender == Gender.MALE ? "He" : "She");
        fullDescription = fullDescription.Replace("#pronoun-subject#", gender == Gender.MALE ? "he" : "she");
        //fullDescription = fullDescription.Replace("#Pronoun-object#", gender == Gender.MALE ? "Him" : "Her"); // I don't currently expect this to occur anywhere
        fullDescription = fullDescription.Replace("#pronoun-object#", gender == Gender.MALE ? "him" : "her"); // I don't currently expect this to occur anywhere
        fullDescription = fullDescription.Replace("#Pronoun-possessive#", gender == Gender.MALE ? "His" : "Her");
        fullDescription = fullDescription.Replace("#pronoun-possessive#", gender == Gender.MALE ? "his" : "her");

        return fullDescription;
    }
}