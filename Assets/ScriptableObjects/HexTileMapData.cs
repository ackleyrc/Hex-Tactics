using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HexTileMapData", menuName = "ScriptableObjects/HexTileMapData", order = 2)]
public class HexTileMapData : ScriptableObject
{
    [SerializeField]
    private HexTileArray[] hexTileArrays;
    public HexTileArray[] HexTileArrays { get { return hexTileArrays; } }
}

[System.Serializable]
public class HexTileArray
{
    [SerializeField]
    private HexTerrainType[] terrainTypes;
    public HexTerrainType[] TerrainTypes { get { return terrainTypes; } }
}