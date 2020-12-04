using UnityEngine;

public class HighlightIndicator : MonoBehaviour
{
    public HexTileHighlightData spriteData;
    public SpriteRenderer spriteRendererBG;
    public SpriteRenderer spriteRendererMG;
    public SpriteRenderer spriteRendererFG;

    private void Awake()
    {
        spriteRendererBG.sprite = spriteData.BackgroundSprite;
        spriteRendererMG.sprite = null;
        spriteRendererMG.color = spriteData.DefenseBonusColor;
        spriteRendererBG.color = spriteData.BackgroundLightColor;
    }

    public void DisplayAsGenericHighlight()
    {
        spriteRendererBG.color = spriteData.BackgroundLightColor;
        spriteRendererMG.sprite = null;
        spriteRendererFG.color = Color.clear;
    }

    public void DisplayAsSelectIndicator(bool isSelected, bool isDefended)
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererMG.sprite = isDefended ? spriteData.DefenseBonusSprite : null;
        spriteRendererFG.sprite = spriteData.SelectSprite;
        spriteRendererFG.color = isSelected ? spriteData.SelectedColor : spriteData.UnselectedColor;
    }

    public void DisplayAsMoveRangeIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererMG.sprite = null;
        spriteRendererFG.sprite = null;
        spriteRendererFG.color = Color.clear;
    }

    public void DisplayAsDestinationIndicator(bool isInRange, bool isDefended)
    {
        spriteRendererBG.color = isInRange ? spriteData.BackgroundDarkColor : spriteData.BackgroundNeutralColor;
        spriteRendererMG.sprite = isDefended ? spriteData.DefenseBonusSprite : null;
        spriteRendererFG.sprite = isInRange ? spriteData.MoveValidSprite : spriteData.MoveInvalidSprite;
        spriteRendererFG.color = isInRange ? spriteData.MoveValidColor : spriteData.MoveInvalidColor;
    }

    public void DisplayAsNonTraversibleIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundRedColor;
        spriteRendererMG.sprite = null;
        spriteRendererFG.sprite = spriteData.MoveNonTraversibleSprite;
        spriteRendererFG.color = spriteData.MoveNonTraversibleColor;
    }

    public void DisplayAsAttackIndicator(bool isValid, bool isDefended)
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererMG.sprite = isDefended ? spriteData.DefenseBonusSprite : null;
        spriteRendererFG.sprite = spriteData.AttackSprite;
        spriteRendererFG.color = isValid ? spriteData.AttackValidColor : spriteData.AttackInvalidColor;
    }

    public void DisplayAsCollateralIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererMG.sprite = null;
        spriteRendererFG.sprite = spriteData.CollateralSprite;
        spriteRendererFG.color = spriteData.CollateralColor;
    }

    public void DisplayAsBlockedLOSIndicator(bool isDefended)
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererMG.sprite = isDefended ? spriteData.DefenseBonusSprite : null;
        spriteRendererFG.sprite = spriteData.BlockingLOSSprite;
        spriteRendererFG.color = spriteData.BlockingLOSColor;
    }

    public void DisplayAsDeadIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererMG.sprite = null;
        spriteRendererFG.sprite = spriteData.DeadSprite;
        spriteRendererFG.color = spriteData.DeadColor;
    }
}