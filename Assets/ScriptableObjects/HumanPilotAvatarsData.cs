using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Gender
{
    MALE,
    FEMALE,
}

public enum Ethnicity
{
    CAUCASIAN,
    ASIAN,
    AFRICAN,
}

[CreateAssetMenu(fileName = "HumanPilotAvatarsData", menuName = "ScriptableObjects/HumanPilotAvatarsData", order = 0)]
public class HumanPilotAvatarsData : ScriptableObject
{
    [SerializeField]
    private Avatar[] pilotAvatars;
    public Avatar[] PilotAvatars { get { return pilotAvatars; } }

    public List<Avatar> GetRandomAvatarSprites(int numSprites)
    {
        List<Avatar> selectedAvatars = new List<Avatar>();

        System.Random rnd = new System.Random();
        List<Avatar> randAvatars = PilotAvatars.OrderBy(q => rnd.Next()).ToList();

        // Ensure some degree of diversity with underrepresented characters
        bool includesCaucasianMale = false;
        foreach (Avatar avatar in randAvatars)
        {
            if (selectedAvatars.Count == 0)
            {
                selectedAvatars.Add(avatar);

                if (includesCaucasianMale == false &&
                    avatar.Gender == Gender.MALE &&
                    avatar.Ethnicity == Ethnicity.CAUCASIAN)
                {
                    includesCaucasianMale = true;
                }
            }
            else
            {
                if (includesCaucasianMale == true &&
                    avatar.Gender == Gender.MALE &&
                    avatar.Ethnicity == Ethnicity.CAUCASIAN)
                {
                    continue;
                }
                else
                {
                    selectedAvatars.Add(avatar);

                    if (includesCaucasianMale == false &&
                        avatar.Gender == Gender.MALE &&
                        avatar.Ethnicity == Ethnicity.CAUCASIAN)
                    {
                        includesCaucasianMale = true;
                    }
                }
            }

            if (selectedAvatars.Count >= numSprites)
            {
                break;
            }
        }

        return selectedAvatars;
    }
}

[System.Serializable]
public class Avatar
{
    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

    [SerializeField]
    private Gender gender;
    public Gender Gender { get { return gender; } }

    [SerializeField]
    private Ethnicity ethnicity;
    public Ethnicity Ethnicity { get { return ethnicity; } }
}