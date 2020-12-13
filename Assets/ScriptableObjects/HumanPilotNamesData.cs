using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "HumanPilotNamesData", menuName = "ScriptableObjects/HumanPilotNamesData", order = 0)]
public class HumanPilotNamesData : ScriptableObject
{
    [SerializeField]
    private string[] maleNames;
    public string[] MaleNames { get { return maleNames; } }

    [SerializeField]
    private string[] femaleNames;
    public string[] FemaleNames { get { return femaleNames; } }

    public List<string> GetNamesForAvatars(IEnumerable<Avatar> avatars)
    {
        List<string> selectedNames = new List<string>();

        foreach (Avatar avatar in avatars)
        {
            if (selectedNames.Count == 0)
            {
                selectedNames.Add(avatar.Gender == Gender.MALE ? GetRandomMaleName() : GetRandomFemaleName());
            }
            else
            {
                string nextName = avatar.Gender == Gender.MALE ? GetRandomMaleName() : GetRandomFemaleName();

                while (selectedNames.Contains(nextName))
                {
                    nextName = avatar.Gender == Gender.MALE ? GetRandomMaleName() : GetRandomFemaleName();
                }

                selectedNames.Add(nextName);
            }
        }

        return selectedNames;
    }

    public string GetRandomMaleName()
    {
        int randIndex = Random.Range(0, MaleNames.Length);
        return MaleNames[randIndex];
    }

    public string GetRandomFemaleName()
    {
        int randIndex = Random.Range(0, FemaleNames.Length);
        return FemaleNames[randIndex];
    }
}