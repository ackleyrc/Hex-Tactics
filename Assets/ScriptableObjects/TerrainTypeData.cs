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
}

[CreateAssetMenu(fileName = "TerrainTypeData", menuName = "ScriptableObjects/TerrainTypeData", order = 6)]
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