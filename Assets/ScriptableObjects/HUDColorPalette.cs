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

    [SerializeField]
    private Color actionEnabledBorder;
    public Color ActionEnabledBorder { get { return actionEnabledBorder; } }

    [SerializeField]
    private Color actionEnabledBackground;
    public Color ActionEnabledBackground { get { return actionEnabledBackground; } }

    [SerializeField]
    private Color actionEnabledIcon;
    public Color ActionEnabledIcon { get { return actionEnabledIcon; } }

    [SerializeField]
    private Color actionDisabledBorder;
    public Color ActionDisabledBorder { get { return actionDisabledBorder; } }

    [SerializeField]
    private Color actionDisabledBackground;
    public Color ActionDisabledBackground { get { return actionDisabledBackground; } }

    [SerializeField]
    private Color actionDisabledIcon;
    public Color ActionDisabledIcon { get { return actionDisabledIcon; } }
}