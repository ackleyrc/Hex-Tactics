using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    private static HexGridManager _Instance;
    public static HexGridManager Instance { get { return _Instance; } }

    public BiomeData tropicalBiomeData;
    public BiomeData desertBiomeData;
    public BiomeData shrublandBiomeData;

    public TerrainTypeData terrainData;
    public HexTileSpriteData spriteData;
    public HexTileMapData mapData; // Should only be used when initially generating the map (static data)
    public HexTile hexPrefab;

    public HexGrid HexGrid { get; private set; }

    private Dictionary<Cube, HexTerrainType> cubeToTerrainType = new Dictionary<Cube, HexTerrainType>(); // This should be used for evaluation at run-time (potentially dynamic data)

    private const float TILE_WIDTH = 2.56f;
    private const float TILE_HEIGHT = 2.56f; // sprite is 3.84f total
    private const float TILE_UNDER_HEIGHT = 1.28f;

    public int MapWidth { get; private set; }
    public int MapLength { get; private set; }

    private Transform hexTilesParent;

    private void Awake()
    {
        _Instance = this;

        hexTilesParent = GameObject.Instantiate(new GameObject(), Vector3.zero, Quaternion.identity, this.transform).transform;
        hexTilesParent.gameObject.name = "HexTilesParent";
    }

    private void OnDestroy()
    {
        if (_Instance == this)
        {
            _Instance = null;
        }
    }

    public MapSeedParameters GenerateMap()
    {
        for (int i = 0; i < hexTilesParent.childCount; i++)
        {
            GameObject.Destroy(hexTilesParent.GetChild(i).gameObject);
        }

        MapSeedParameters mapParams = new MapSeedParameters();

        switch ((Biome)Random.Range(0, 3))
        {
            case Biome.TROPICAL:
                Debug.Log("HexGridManager :: Generate TROPICAL Map from defaults");
                mapParams = tropicalBiomeData.GetDefaultsWithRandomSeed();
                break;
            case Biome.DESERT:
                Debug.Log("HexGridManager :: Generate DESERT Map from defaults");
                mapParams = desertBiomeData.GetDefaultsWithRandomSeed();
                break;
            case Biome.SHRUBLAND:
                Debug.Log("HexGridManager :: Generate SHRUBLAND Map from defaults");
                mapParams = shrublandBiomeData.GetDefaultsWithRandomSeed();
                break;
        }

        //GenerateFromMapData(mapData);
        GenerateProcedurally(12, 12, mapParams);

        return mapParams;
    }

    public void GenerateCustomMap(MapSeedParameters mapParams)
    {
        for (int i = 0; i < hexTilesParent.childCount; i++)
        {
            GameObject.Destroy(hexTilesParent.GetChild(i).gameObject);
        }

        //GenerateFromMapData(mapData);
        GenerateProcedurally(12, 12, mapParams);
    }

    public IEnumerable<Cube> GetDefensiveHexTiles()
    {
        foreach (Cube cube in cubeToTerrainType.Keys)
        {
            HexTerrainType terrain = cubeToTerrainType[cube];
            float defensiveMultiplier = terrainData.GetDefenseMultiplier(terrain);

            if (terrainData.IsTraversible(terrain) &&
                defensiveMultiplier < 1.0f)
            {
                yield return cube;
            }
        }
    }

    public Dictionary<Cube, float> GetDistanceMap(Cube origin, List<Cube> destinations, float minWeightedRange)
    {
        return HexGrid.GetDistanceMap(origin, destinations, minWeightedRange, (Cube c) => CanTravelOverHex(c), (Cube c) => GetTerrainMovementCost(c));
    }

    public HashSet<Cube> GetReachableHexes(Cube start, float weightedRange)
    {
        return HexGrid.GetReachable(start, weightedRange, (Cube c) => CanTravelOverHex(c), (Cube c) => GetTerrainMovementCost(c));
    }

    public List<Cube> GetPath(Cube start, Cube finish)
    {
        //return HexGrid.GetShortestPath(start, finish, (Cube c) => CanTravelOverHex(c));
        return HexGrid.GetQuickestPath(start, finish, (Cube c) => CanTravelOverHex(c), (Cube c) => GetTerrainMovementCost(c));
    }

    public float GetPathCost(List<Cube> path)
    {
        float cost = 0;
        if (path != null && path.Count > 1)
        {
            for (int i = 1; i < path.Count; i++)
            {
                float prevCost = HexGridManager.Instance.GetTerrainMovementCost(path[i - 1]);
                float nextCost = HexGridManager.Instance.GetTerrainMovementCost(path[i]);
                cost += (0.5f * prevCost) + (0.5f * nextCost);
            }
        }
        return cost;
    }

    public bool CanTravelOverHex(Cube cube)
    {
        //Debug.Log($"HexGridManager::CanTravelOverHex( {cube} )");

        if (IsHexCubeOnMap(cube) == false)
        {
            //Debug.Log($"HexGridManager :: Not on Map");
            return false;
        }

        if (GameManager.Instance.GetFriendlyMechAt(cube) != null)
        {
            //Debug.Log($"HexGridManager :: Occupied by Friendly");
            return false;
        }

        if (GameManager.Instance.GetEnemyMechAt(cube) != null)
        {
            //Debug.Log($"HexGridManager :: Occupied by Enemy");
            return false;
        }

        if (cubeToTerrainType.ContainsKey(cube) == false)
        {
            //Debug.Log($"HexGridManager :: Not in terrain dictionary");
            return false; 
        }

        if (terrainData.IsTraversible(cubeToTerrainType[cube]) == false)
        {
            //Debug.Log($"HexGridManager :: Terrain Type {cubeToTerrainType[cube]} NOT traversible");
            return false;
        }

        //Debug.Log($"HexGridManager :: Terrain Type {cubeToTerrainType[cube]} IS traversible");
        return true;
    }

    public HexTerrainType GetHexTerrainType(Cube cube)
    {
        if (cubeToTerrainType.ContainsKey(cube) == true)
        {
            return cubeToTerrainType[cube];
        }

        return HexTerrainType.NONE;
    }

    public float GetTerrainMovementCost(Cube cube)
    {
        if (cubeToTerrainType.ContainsKey(cube) == true)
        {
            return terrainData.GetMovementCost(cubeToTerrainType[cube]);
        }

        return float.MaxValue;
    }

    public float GetDefenseMultiplier(Cube cube)
    {
        if (cubeToTerrainType.ContainsKey(cube) == true)
        {
            return terrainData.GetDefenseMultiplier(cubeToTerrainType[cube]);
        }

        return 1.0f;
    }

    public Cube GetHexCubeForWorldPosition(Vector3 worldPos)
    {
        return HexGrid.PixelToCube(worldPos.x, worldPos.y, TILE_WIDTH);
    }

    public Cube GetHexCubeUnderMouse()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return HexGrid.PixelToCube(worldPos.x, worldPos.y, TILE_WIDTH);
    }

    public bool IsHexCubeOnMap(Cube hexCube)
    {
        //OffsetCoord hexOffsetCoord = hexCube.ToOffsetCoord();
        //return hexOffsetCoord.row >= 0 && hexOffsetCoord.row < mapData.HexTileArrays.Length &&
        //       hexOffsetCoord.col >= 0 && hexOffsetCoord.col < mapData.HexTileArrays[hexOffsetCoord.row].TerrainTypes.Length;
        return cubeToTerrainType.ContainsKey(hexCube);
    }

    public Vector3 GetHexCubeWorldPostion(Cube hexCube)
    {
        return HexGrid.CubeToPixel(hexCube, TILE_WIDTH);
    }

    public Vector2 GetMapCenter()
    {
        float x = (MapWidth - 0.5f) * TILE_WIDTH * 0.5f;
        float y = (MapLength - 1) * 0.75f * TILE_HEIGHT * 0.5f;
        return new Vector2(x, y);
    }

    private void GenerateFromMapData(HexTileMapData mapData)
    {
        HexGrid = new HexGrid();
        HexGrid.GenerateRectangularGrid(HexGrid.Alignment.Horizontal, mapData.HexTileArrays[0].TerrainTypes.Length, mapData.HexTileArrays.Length);

        cubeToTerrainType.Clear();

        MapWidth = mapData.HexTileArrays[0].TerrainTypes.Length;
        MapLength = mapData.HexTileArrays.Length;

        int seed = Random.Range(0, 10000);

        foreach (Cube cube in HexGrid.GetHexes())
        {
            //Debug.Log($"Cube Coord ({cube.q} {cube.r}) [{cube.ToOffsetCoord().ToString()}]"); // Pos: ({hexGrid.CubeToPixel(cube, TILE_WIDTH)})");

            OffsetCoord offsetCoord = cube.ToOffsetCoord();

            int row = offsetCoord.row;
            int col = offsetCoord.col;

            if (row >= 0 && row < mapData.HexTileArrays.Length)
            {
                if (col >= 0 && col < mapData.HexTileArrays[row].TerrainTypes.Length)
                {
                    HexTerrainType terrainType = mapData.HexTileArrays[row].TerrainTypes[col];
                    bool createUnderground = (row == 0) || (row == mapData.HexTileArrays.Length - 1) || (col == 0) || (col == mapData.HexTileArrays[row].TerrainTypes.Length - 1);
                    HexTile hexTile = CreateHex(terrainType, -row, createUnderground, hexTilesParent);

                    Vector3 position = HexGrid.CubeToPixel(cube, TILE_WIDTH);
                    hexTile.transform.position = position;

                    cubeToTerrainType.Add(cube, terrainType);
                }
            }
        }
    }

    private void GenerateProcedurally(int width, int length, MapSeedParameters mapParams)
    {
        Debug.Log($"HexGridManager::GenerateProcedurally( {width} , {length} , {mapParams.biome} )");

        HexGrid = new HexGrid();
        HexGrid.GenerateRectangularGrid(HexGrid.Alignment.Horizontal, width, length);

        cubeToTerrainType.Clear();

        MapWidth = width;
        MapLength = length;

        float lacunarity = (MapWidth + MapLength) * 0.5f * 0.75f;

        foreach (Cube cube in HexGrid.GetHexes())
        {
            //Debug.Log($"Cube Coord ({cube.q} {cube.r}) [{cube.ToOffsetCoord().ToString()}]"); // Pos: ({hexGrid.CubeToPixel(cube, TILE_WIDTH)})");

            OffsetCoord offsetCoord = cube.ToOffsetCoord();

            int row = offsetCoord.row;
            int col = offsetCoord.col;

            if (row >= 0 && row < MapLength)
            {
                if (col >= 0 && col < MapWidth)
                {
                    HexTerrainType terrainType = HexTerrainType.NONE;

                    switch (mapParams.biome)
                    {
                        case Biome.TROPICAL:
                            terrainType = tropicalBiomeData.GetTerrainType(MapWidth, MapLength, col, row, mapParams);
                            break;
                        case Biome.DESERT:
                            terrainType = desertBiomeData.GetTerrainType(MapWidth, MapLength, col, row, mapParams);
                            break;
                        case Biome.SHRUBLAND:
                            terrainType = shrublandBiomeData.GetTerrainType(MapWidth, MapLength, col, row, mapParams);
                            break;
                    }

                    bool createUnderground = (row == 0) || (row == length - 1) || (col == 0) || (col == width - 1);
                    HexTile hexTile = CreateHex(terrainType, -row, createUnderground, hexTilesParent);

                    Vector3 position = HexGrid.CubeToPixel(cube, TILE_WIDTH);
                    hexTile.transform.position = position;

                    cubeToTerrainType.Add(cube, terrainType);
                }
            }
        }
    }

    private HexTile CreateHex(HexTerrainType terrainType, int sortingOrder, bool createUnderground, Transform parent)
    {
        // tile sprite
        HexTile hexTile = GameObject.Instantiate(hexPrefab, parent) as HexTile;
        hexTile.gameObject.name = $"{terrainType}";
        hexTile.spriteRenderer.sprite = spriteData.GetSpriteForTerrainType(terrainType);
        hexTile.spriteRenderer.sortingOrder = sortingOrder;
        hexTile.terrainType = terrainType;

        if (createUnderground == true)
        {
            // underground sprite
            GameObject dirt = new GameObject($"{terrainType}_underground");
            dirt.AddComponent(typeof(SpriteRenderer));
            dirt.GetComponent<SpriteRenderer>().sprite = spriteData.GetUndergroundSpite();
            dirt.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;

            dirt.transform.SetParent(hexTile.spriteRenderer.transform);
            dirt.transform.localPosition = new Vector3(0.0f, TILE_UNDER_HEIGHT * 0.5f, 1.0f);
        }

        return hexTile;
    }
}