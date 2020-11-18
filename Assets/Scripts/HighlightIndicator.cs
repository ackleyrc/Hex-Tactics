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

    public void DisplayAsMoveIndicator(bool isValid)
    {
        spriteRendererBG.color = isValid ? spriteData.BackgroundDarkColor : spriteData.BackgroundRedColor;
        spriteRendererFG.sprite = isValid ? spriteData.MoveValidSprite : spriteData.MoveInvalidSprite;
        spriteRendererFG.color = isValid ? spriteData.MoveValidColor : spriteData.MoveInvalidColor;
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