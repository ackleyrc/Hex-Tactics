using UnityEngine;

[CreateAssetMenu(fileName = "MechEntityDetailsData", menuName = "ScriptableObjects/MechEntityDetailsData", order = 0)]
public class MechEntityDetailsData : ScriptableObject
{
    [SerializeField]
    private Entity[] friendlyMechEntities;
    public Entity[] FriendlyMechEntities { get { return friendlyMechEntities; } }

    [SerializeField]
    private Entity[] enemyMechEntities;
    public Entity[] EnemyMechEntities { get { return enemyMechEntities; } }

    public string GetName(Allegiance allegiance, int index)
    {
        if (index >= 0)
        {
            if (allegiance == Allegiance.FRIENDLY)
            {
                if (index < FriendlyMechEntities.Length)
                {
                    return FriendlyMechEntities[index].EntityName;
                }
            }
            else if (allegiance == Allegiance.ENEMY)
            {
                if (index < EnemyMechEntities.Length)
                {
                    return EnemyMechEntities[index].EntityName;
                }
            }
        }

        return "";
    }

    public Sprite GetPortrait(Allegiance allegiance, int index)
    {
        if (index >= 0)
        {
            if (allegiance == Allegiance.FRIENDLY)
            {
                if (index < FriendlyMechEntities.Length)
                {
                    return FriendlyMechEntities[index].EntityPortrait;
                }
            }
            else if (allegiance == Allegiance.ENEMY)
            {
                if (index < EnemyMechEntities.Length)
                {
                    return EnemyMechEntities[index].EntityPortrait;
                }
            }
        }

        return null;
    }
}

[System.Serializable]
public class Entity
{
    [SerializeField]
    private string entityName;
    public string EntityName { get { return entityName; } }

    [SerializeField]
    private Sprite entityPortrait;
    public Sprite EntityPortrait { get { return entityPortrait; } }
}