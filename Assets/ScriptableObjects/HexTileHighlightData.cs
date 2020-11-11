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
    private Color attackValidColor;
    public Color AttackValidColor { get { return attackValidColor; } }

    [SerializeField]
    private Color attackInvalidColor;
    public Color AttackInvalidColor { get { return attackInvalidColor; } }

    [SerializeField]
    private Sprite collateralSprite;
    public Sprite CollateralSprite { get { return collateralSprite; } }

    [SerializeField]
    private Color collateralColor;
    public Color CollateralColor { get { return collateralColor; } }
}