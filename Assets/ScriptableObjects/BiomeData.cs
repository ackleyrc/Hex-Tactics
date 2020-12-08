using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Biome
{
    NONE,
    TROPICAL,
    YELLOW_DESERT,
    RED_DESERT,
}

public enum TropicalDimensions { WETNESS = 0, VEGETATION = 1 }

[System.Serializable]
public struct MapSeedParameters
{
    public Biome biome;
    /// <summary> Expected values from 0-4095 inclusive </summary>
    public int baseSeed;
    public BiomeDimension[] biomeDimensions;
}

[System.Serializable]
public struct BiomeDimension
{
    /// <summary> Expected values from 0-63 inclusive </summary>
    public int scale;
    /// <summary> Expected values from 0-63 inclusive </summary>
    public int persistence;
    /// <summary> Expected values from -31 to +31 inclusive </summary>
    public int bias;
}

[CreateAssetMenu(fileName = "BiomeData", menuName = "ScriptableObjects/BiomeData", order = 0)]
public class BiomeData : ScriptableObject
{
    [SerializeField]
    private Biome biomeType;
    public Biome BiomeType { get { return biomeType; } }

    [SerializeField]
    private MapSeedParameters defaultMapParams;
    public MapSeedParameters DefaultMapParams { get { return defaultMapParams; } }

    public MapSeedParameters GetDefaultsWithRandomSeed()
    {
        return new MapSeedParameters()
        {
            biome = biomeType,
            baseSeed = Random.Range(0, 4095),
            biomeDimensions = defaultMapParams.biomeDimensions,
        };
    }

    /// <summary> Obtain a terrain type provided the specified parameters </summary>
    public HexTerrainType GetTerrainType(int mapWidth, int mapLength, int col, int row, MapSeedParameters mapParams)
    {
        if (biomeType == Biome.TROPICAL)
        {
            return GetTropicalTerrain(mapWidth, mapLength, col, row, mapParams);
        }

        return HexTerrainType.NONE;
    }

    private HexTerrainType GetTropicalTerrain(int mapWidth, int mapLength, int col, int row, MapSeedParameters mapParams)
    {
        float lacunarity = (mapWidth + mapLength) * 0.5f * 0.75f;

        float wetFreqNrml = 1.0f - (mapParams.biomeDimensions[(int)TropicalDimensions.WETNESS].scale / 64.0f) * 1.0f;
        float wetBiasNrml = (mapParams.biomeDimensions[(int)TropicalDimensions.WETNESS].bias / 64.0f) * 0.5f;
        float wetPersNrml = ((DefaultMapParams.biomeDimensions[(int)TropicalDimensions.WETNESS].persistence + 1) / 64.0f) * 0.5f;

        float vegFreqNrml = 1.0f - (mapParams.biomeDimensions[(int)TropicalDimensions.VEGETATION].scale / 64.0f) * 1.0f;
        float vegBiasNrml = (mapParams.biomeDimensions[(int)TropicalDimensions.VEGETATION].bias / 64.0f) * 0.5f;
        float vegPersNrml = ((DefaultMapParams.biomeDimensions[(int)TropicalDimensions.VEGETATION].persistence + 1) / 64.0f) * 0.5f;

        float wetnessNoise = GetMultiOctaveNoise(col, row, wetFreqNrml, wetPersNrml, lacunarity, mapParams.baseSeed * 100) + wetBiasNrml;
        float vegetationNoise = GetMultiOctaveNoise(col, row, vegFreqNrml, vegPersNrml, lacunarity, mapParams.baseSeed * 100 + 1000) + vegBiasNrml;

        if (wetnessNoise < 0.2f)
        {
            return vegetationNoise < 0.5f ? HexTerrainType.SAND : HexTerrainType.SAND_PALMS;
        }
        else if (wetnessNoise < 0.4f)
        {
            return vegetationNoise < 0.5f ? HexTerrainType.GRASSY_SAND : HexTerrainType.GRASSY_SAND_PALMS;
        }
        else if (wetnessNoise < 0.6f)
        {
            return vegetationNoise < 0.5f ? HexTerrainType.TROPICAL_PLAINS : HexTerrainType.JUNGLE;
        }
        else if (wetnessNoise < 0.8f)
        {
            return vegetationNoise < 0.5f ? HexTerrainType.WETLANDS : HexTerrainType.SWAMP;
        }
        else
        {
            return HexTerrainType.BOG;
        }
    }

    private float GetMultiOctaveNoise(float x, float y, float frequency, float persistence, float lacunarity, int seed)
    {
        float max = 0.0f;
        float noise = 0.0f;
        float floatOffset = 0.5f;
        float freqExp = Mathf.Pow(frequency, 1.25f);
        float lacExp = Mathf.Pow(lacunarity, 0.5f);
        for (int i = 0; i < 3; i++)
        {
            //float oct = Mathf.PerlinNoise(Mathf.RoundToInt(seed + x * frequency * Mathf.Pow(lacunarity, (float)i)) + floatOffset, Mathf.RoundToInt(seed + y * frequency * Mathf.Pow(lacunarity, (float)i)) + floatOffset);
            //float oct = Mathf.PerlinNoise(seed + x * frequency * Mathf.Pow(lacunarity, (float)i) + floatOffset, seed + y * frequency * Mathf.Pow(lacunarity, (float)i) + floatOffset);
            //float oct = Mathf.PerlinNoise(seed + x * freqExp * Mathf.Pow(lacunarity, (float)i) + floatOffset, seed + y * freqExp * Mathf.Pow(lacunarity, (float)i) + floatOffset);
            float oct = Mathf.PerlinNoise(seed + x * freqExp * Mathf.Pow(lacExp, (float)i) + floatOffset, seed + y * freqExp * Mathf.Pow(lacExp, (float)i) + floatOffset);
            noise += oct * Mathf.Pow(persistence, (float)i);
            max += Mathf.Pow(persistence, (float)i);
        }
        return noise / max;
    }
}