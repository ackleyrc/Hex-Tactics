using UnityEngine;

[CreateAssetMenu(fileName = "HexTileHighlightData", menuName = "ScriptableObjects/HexTileHighlightData", order = 0)]
public class HexTileHighlightData : ScriptableObject
{
    [SerializeField]
    private Sprite backgroundSprite;
    public Sprite BackgroundSprite { get { return backgroundSprite; } }

    [SerializeField]
    private Color backgroundDarkColor;
    public Color BackgroundDarkColor { get { return backgroundDarkColor; } }

    [SerializeField]
    private Color backgroundNeutralColor;
    public Color BackgroundNeutralColor { get { return backgroundNeutralColor; } }

    [SerializeField]
    private Color backgroundLightColor;
    public Color BackgroundLightColor { get { return backgroundLightColor; } }

    [SerializeField]
    private Color backgroundRedColor;
    public Color BackgroundRedColor { get { return backgroundRedColor; } }

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
    private Sprite moveValidSprite;
    public Sprite MoveValidSprite { get { return moveValidSprite; } }

    [SerializeField]
    private Color moveValidColor;
    public Color MoveValidColor { get { return moveValidColor; } }

    [SerializeField]
    private Sprite moveInvalidSprite;
    public Sprite MoveInvalidSprite { get { return moveInvalidSprite; } }

    [SerializeField]
    private Color moveInvalidColor;
    public Color MoveInvalidColor { get { return moveInvalidColor; } }

    [SerializeField]
    private Sprite moveNonTraversibleSprite;
    public Sprite MoveNonTraversibleSprite { get { return moveNonTraversibleSprite; } }

    [SerializeField]
    private Color moveNonTraversibleColor;
    public Color MoveNonTraversibleColor { get { return moveNonTraversibleColor; } }

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

    [SerializeField]
    private Sprite blockingLOSSprite;
    public Sprite BlockingLOSSprite { get { return blockingLOSSprite; } }

    [SerializeField]
    private Color blockingLOSColor;
    public Color BlockingLOSColor { get { return blockingLOSColor; } }

    [SerializeField]
    private Sprite deadSprite;
    public Sprite DeadSprite { get { return deadSprite; } }

    [SerializeField]
    private Color deadColor;
    public Color DeadColor { get { return deadColor; } }

    [SerializeField]
    private Sprite defenseBonusSprite;
    public Sprite DefenseBonusSprite { get { return defenseBonusSprite; } }

    [SerializeField]
    private Color defenseBonusColor;
    public Color DefenseBonusColor { get { return defenseBonusColor; } }
}