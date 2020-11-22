using UnityEngine;

public class HighlightIndicator : MonoBehaviour
{
    public HexTileHighlightData spriteData;
    public SpriteRenderer spriteRendererBG;
    public SpriteRenderer spriteRendererFG;

    private void Awake()
    {
        spriteRendererBG.sprite = spriteData.BackgroundSprite;
        spriteRendererBG.color = spriteData.BackgroundLightColor;
    }

    public void DisplayAsGenericHighlight()
    {
        spriteRendererBG.color = spriteData.BackgroundLightColor;
        spriteRendererFG.color = Color.clear;
    }

    public void DisplayAsSelectIndicator(bool isSelected)
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererFG.sprite = spriteData.SelectSprite;
        spriteRendererFG.color = isSelected ? spriteData.SelectedColor : spriteData.UnselectedColor;
    }

    public void DisplayAsMoveRangeIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererFG.sprite = null;
        spriteRendererFG.color = Color.clear;
    }

    public void DisplayAsDestinationIndicator(bool isInRange)
    {
        spriteRendererBG.color = isInRange ? spriteData.BackgroundDarkColor : spriteData.BackgroundNeutralColor;
        spriteRendererFG.sprite = isInRange ? spriteData.MoveValidSprite : spriteData.MoveInvalidSprite;
        spriteRendererFG.color = isInRange ? spriteData.MoveValidColor : spriteData.MoveInvalidColor;
    }

    public void DisplayAsNonTraversibleIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundRedColor;
        spriteRendererFG.sprite = spriteData.MoveNonTraversibleSprite;
        spriteRendererFG.color = spriteData.MoveNonTraversibleColor;
    }

    public void DisplayAsAttackIndicator(bool isValid)
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererFG.sprite = spriteData.AttackSprite;
        spriteRendererFG.color = isValid ? spriteData.AttackValidColor : spriteData.AttackInvalidColor;
    }

    public void DisplayAsCollateralIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererFG.sprite = spriteData.CollateralSprite;
        spriteRendererFG.color = spriteData.CollateralColor;
    }

    public void DisplayAsBlockedLOSIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererFG.sprite = spriteData.BlockingLOSSprite;
        spriteRendererFG.color = spriteData.BlockingLOSColor;
    }

    public void DisplayAsDeadIndicator()
    {
        spriteRendererBG.color = spriteData.BackgroundDarkColor;
        spriteRendererFG.sprite = spriteData.DeadSprite;
        spriteRendererFG.color = spriteData.DeadColor;
    }
}