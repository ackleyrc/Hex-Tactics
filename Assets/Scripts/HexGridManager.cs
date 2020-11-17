using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    private static HexGridManager _Instance;
    public static HexGridManager Instance { get { return _Instance; } }

    public TerrainTypeData terrainData;
    public HexTileSpriteData spriteData;
    public HexTileMapData mapData;
    public HexTile hexPrefab;

    public HexGrid HexGrid { get; private set; }

    private Dictionary<Cube, HexTerrainType> cubeToTerrainType = new Dictionary<Cube, HexTerrainType>();

    private const float TILE_WIDTH = 2.56f;
    private const float TILE_HEIGHT = 2.56f; // sprite is 3.84f total
    private const float TILE_UNDER_HEIGHT = 1.28f;

    private void Awake()
    {
        _Instance = this;

        Generate();
    }

    private void OnDestroy()
    {
        if (_Instance == this)
        {
            _Instance = null;
        }
    }

    public List<Cube> GetPath(Cube start, Cube finish)
    {
        //return HexGrid.GetShortestPath(start, finish, (Cube c) => CanTravelOverHex(c));
        return HexGrid.GetQuickestPath(start, finish, (Cube c) => CanTravelOverHex(c), (Cube c) => GetTerrainMovementCost(c));
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

    private void Generate()
    {
        HexGrid = new HexGrid();
        HexGrid.GenerateRectangularGrid(HexGrid.Alignment.Horizontal, mapData.HexTileArrays[0].TerrainTypes.Length, mapData.HexTileArrays.Length);

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
                    HexTile hexTile = CreateHex(terrainType, -row, createUnderground);

                    Vector3 position = HexGrid.CubeToPixel(cube, TILE_WIDTH);
                    hexTile.transform.position = position;

                    cubeToTerrainType.Add(cube, terrainType);
                }
            }
        }
    }

    private HexTile CreateHex(HexTerrainType terrainType, int sortingOrder, bool createUnderground = false)
    {
        // tile sprite
        HexTile hexTile = GameObject.Instantiate(hexPrefab, this.transform) as HexTile;
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