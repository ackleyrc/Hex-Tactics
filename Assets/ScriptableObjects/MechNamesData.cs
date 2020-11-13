using UnityEngine;

[CreateAssetMenu(fileName = "MechNamesData", menuName = "ScriptableObjects/MechNamesData", order = 4)]
public class MechNamesData : ScriptableObject
{
    [SerializeField]
    private string[] friendlyMechNames;
    public string[] FriendlyMechNames { get { return friendlyMechNames; } }

    [SerializeField]
    private string[] enemyMechNames;
    public string[] EnemyMechNames { get { return enemyMechNames; } }
}