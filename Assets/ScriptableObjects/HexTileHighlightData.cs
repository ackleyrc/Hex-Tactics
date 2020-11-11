using UnityEngine;

[CreateAssetMenu(fileName = "HexTileHighlightData", menuName = "ScriptableObjects/HexTileHighlightData", order = 3)]
public class HexTileHighlightData : ScriptableObject
{
    [SerializeField]
    private Sprite backgroundSprite;
    public Sprite BackgroundSprite { get { return backgroundSprite; } }

    [SerializeField]
    private Color backgroundDarkColor;
    public Color BackgroundDarkColor { get { return backgroundDarkColor; } }

    [SerializeField]
    private Color backgroundLightColor;
    public Color BackgroundLightColor { get { return backgroundLightColor; } }

    [SerializeField]
    private Sprite selectSprite;
    public Sprite SelectSprite { get { return selectSprite; } }

    [SerializeField]
    private Color selectedColor;
    public Color SelectedColor { get { return selectedColor; } }

    [SerializeField]
    private Color unselectedColor;
    public Color UnselectedColor { get { return unselectedColor; } }

    [SerializeField]
    private Sprite moveSprite;
    public Sprite MoveSprite { get { return moveSprite; } }

    [SerializeField]
    private Color moveColor;
    public Color MoveColor { get { return moveColor; } }

    [SerializeField]
    private Sprite attackSprite;
    public Sprite AttackSprite { get { return attackSprite; } }

    [SerializeField]
    private Color attackColor;
    public Color AttackColor { get { return attackColor; } }
}