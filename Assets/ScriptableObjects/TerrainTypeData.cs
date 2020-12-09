using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexTerrainType
{
    NONE = 0,
    BOG = 1,
    GRASSY_SAND = 2,
    GRASSY_SAND_PALMS = 3,
    JUNGLE = 4,
    SAND = 5,
    SAND_PALMS = 6,
    SWAMP = 7,
    TROPICAL_PLAINS = 8,
    WETLANDS = 9,
    DESERT_RED_BASE = 10,
    DESERT_RED_DIRT = 11,
    DESERT_RED_FOREST = 12,
    DESERT_RED_FOREST_OASIS = 13,
    DESERT_RED_GRASS = 14,
    DESERT_RED_GRASS_DUNES = 15,
    DESERT_RED_GRASS_OASIS = 16,
    DESERT_RED_HILLS = 17,
    DESERT_RED_HILLS_OASIS = 18,
    DESERT_RED_MESA_LARGE = 19,
    DESERT_RED_MESA_LARGE_CAVE = 20,
    DESERT_RED_MOUNTAINS = 21,
    DESERT_RED_MOUNTAINS_CAVE = 22,
    DESERT_YELLOW_BASE = 23,
    DESERT_YELLOW_CACTI = 24,
    DESERT_YELLOW_CRATER = 25,
    DESERT_YELLOW_DIRT = 26,
    DESERT_YELLOW_DIRT_DUNES = 27,
    DESERT_YELLOW_HILLS = 28,
    DESERT_YELLOW_HILLS_OASIS = 29,
    DESERT_YELLOW_MESA_LARGE = 30,
    DESERT_YELLOW_MESA_LARGE_CAVE = 31,
    DESERT_YELLOW_MESA_LARGE_OASIS = 32,
    DESERT_YELLOW_MESAS = 33,
    DESERT_YELLOW_MESAS_CAVE = 34,
    DESERT_YELLOW_SALT_FLAT = 35,
}

[CreateAssetMenu(fileName = "TerrainTypeData", menuName = "ScriptableObjects/TerrainTypeData", order = 0)]
public class TerrainTypeData : ScriptableObject
{
    [SerializeField]
    private TerrainMetaData[] terrainData;
    public TerrainMetaData[] TerrainData {  get { return terrainData; } }

    public string GetTerrainDisplayName(HexTerrainType terrainType)
    {
        foreach (TerrainMetaData terrainTypeData in terrainData)
        {
            if (terrainTypeData.TerrainType == terrainType)
            {
                return terrainTypeData.DisplayName;
            }
        }

        return "";
    }

    public float GetMovementCost(HexTerrainType terrainType)
    {
        foreach (TerrainMetaData terrainTypeData in terrainData)
        {
            if (terrainTypeData.TerrainType == terrainType)
            {
                return terrainTypeData.MovementCost;
            }
        }

        return float.MaxValue;
    }

    public bool IsTraversible(HexTerrainType terrainType)
    {
        foreach (TerrainMetaData terrainTypeData in terrainData)
        {
            if (terrainTypeData.TerrainType == terrainType)
            {
                return terrainTypeData.Traversible;
            }
        }

        return false;
    }

    public bool AllowsLineOfSight(HexTerrainType terrainType)
    {
        foreach (TerrainMetaData terrainTypeData in terrainData)
        {
            if (terrainTypeData.TerrainType == terrainType)
            {
                return terrainTypeData.AllowsLOS;
            }
        }

        return false;
    }

    public float GetDefenseMultiplier(HexTerrainType terrainType)
    {
        foreach (TerrainMetaData terrainTypeData in terrainData)
        {
            if (terrainTypeData.TerrainType == terrainType)
            {
                return terrainTypeData.DefenseMultiplier;
            }
        }

        return 1.0f;
    }
}

[System.Serializable]
public class TerrainMetaData
{
    [SerializeField]
    private HexTerrainType terrainType;
    public HexTerrainType TerrainType { get { return terrainType; } }

    [SerializeField]
    private string displayName;
    public string DisplayName { get { return displayName; } }

    [SerializeField]
    private bool traversible;
    public bool Traversible { get { return traversible; } }

    [SerializeField]
    private float movementCost;
    public float MovementCost { get { return movementCost; } }

    [SerializeField]
    private bool allowsLOS;
    public bool AllowsLOS { get { return allowsLOS; } }

    [SerializeField]
    private float defenseMultiplier;
    public float DefenseMultiplier { get { return defenseMultiplier; } }
}