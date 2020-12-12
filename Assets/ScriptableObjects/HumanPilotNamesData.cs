using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HumanPilotNamesData", menuName = "ScriptableObjects/HumanPilotNamesData", order = 0)]
public class HumanPilotNamesData : ScriptableObject
{
    [SerializeField]
    private string[] maleNames;
    public string[] MaleNames { get { return maleNames; } }

    [SerializeField]
    private string[] femaleNames;
    public string[] FemaleNames { get { return femaleNames; } }

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