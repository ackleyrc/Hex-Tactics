using UnityEngine;

[CreateAssetMenu(fileName = "HUDColorPalette", menuName = "ScriptableObjects/HUDColorPalette", order = 5)]
public class HUDColorPalette : ScriptableObject
{
    [SerializeField]
    private Color friendlyUnitColor;
    public Color FriendlyUnitColor { get { return friendlyUnitColor; } }

    [SerializeField]
    private Color enemyUnitColor;
    public Color EnemyUnitColor { get { return enemyUnitColor; } }
}