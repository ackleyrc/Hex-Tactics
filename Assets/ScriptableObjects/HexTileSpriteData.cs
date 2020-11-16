using UnityEngine;

[CreateAssetMenu(fileName = "HexTileSpriteData", menuName = "ScriptableObjects/HexTileSpriteData", order = 1)]
public class HexTileSpriteData : ScriptableObject
{
    [SerializeField]
    private TerrainSprites[] spritesPerTerrainType;
    public TerrainSprites[] SpritesPerTerrainType { get { return spritesPerTerrainType; } }

    [SerializeField]
    private Sprite undergroundSprite;
    public Sprite UndergroundSprite { get { return undergroundSprite; } }

    public Sprite GetUndergroundSpite()
    {
        return UndergroundSprite;
    }

    public Sprite GetSpriteForTerrainType(HexTerrainType terrainType)
    {
        Sprite sprite = null;

        foreach (TerrainSprites terrainSprites in spritesPerTerrainType)
        {
            if (terrainSprites.TerrainType == terrainType)
            {
                int index = Random.Range(0, terrainSprites.Sprites.Length);
                sprite = terrainSprites.Sprites[index];
                break;
            }
        }

        return sprite;
    }
}

[System.Serializable]
public class TerrainSprites
{
    [SerializeField]
    private HexTerrainType terrainType;
    public HexTerrainType TerrainType { get { return terrainType; } }

    [SerializeField]
    private Sprite[] sprites;
    public Sprite[] Sprites { get { return sprites; } }
}