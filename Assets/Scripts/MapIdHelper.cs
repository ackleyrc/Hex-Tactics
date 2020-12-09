using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapIdHelper : MonoBehaviour
{
    // TODO: Implement with string builder

    public static string GetMapId(MapSeedParameters currentMapParams)
    {
        //float wetFreqNrml = 1.0f - (wetScale / 64.0f) * 1.0f;
        //float wetBiasNrml = (wetBias / 64.0f) * 0.5f;
        //float vegFreqNrml = 1.0f - (vegScale / 64.0f) * 1.0f;
        //float vegBiasNrml = (vegBias / 64.0f) * 0.5f;

        int seedIdx0 = currentMapParams.baseSeed / 64;
        int seedIdx1 = currentMapParams.baseSeed % 64;

        int wetScaleIdx = 64 - currentMapParams.biomeDimensions[(int)TropicalDimensions.WETNESS].scale;
        int wetBiasIdx = currentMapParams.biomeDimensions[(int)TropicalDimensions.WETNESS].bias + 31;

        int vegScaleIdx = 64 - currentMapParams.biomeDimensions[(int)TropicalDimensions.VEGETATION].scale;
        int vegBiasIdx = currentMapParams.biomeDimensions[(int)TropicalDimensions.VEGETATION].bias + 31;

        return $"{(int)currentMapParams.biome}66{GetChar(seedIdx0)}{GetChar(seedIdx1)}{GetChar(wetScaleIdx)}{GetChar(wetBiasIdx)}{GetChar(vegScaleIdx)}{GetChar(vegBiasIdx)}";
    }

    public static string GetMapId(Biome biome, int baseSeed, int dim0Scale = 0, int dim0Bias = 0, int dim1Scale = 0, int dim1Bias = 0, int dim2Scale = 0, int dim2Bias = 0)
    {
        int seedIdx0 = baseSeed / 64;
        int seedIdx1 = baseSeed % 64;

        int dim0ScaleIdx = 64 - dim0Scale;
        int dim0BiasIdx = dim0Bias + 31;

        int dim1ScaleIdx = 64 - dim1Scale;
        int dim1BiasIdx = dim1Bias + 31;

        int dim2ScaleIdx = 64 - dim2Scale;
        int dim2BiasIdx = dim2Bias + 31;

        switch (BiomeData.GetNumDimensions(biome))
        {
            case 1:
                return $"{(int)biome}66{GetChar(seedIdx0)}{GetChar(seedIdx1)}{GetChar(dim0ScaleIdx)}{GetChar(dim0BiasIdx)}";
            case 2:
                return $"{(int)biome}66{GetChar(seedIdx0)}{GetChar(seedIdx1)}{GetChar(dim0ScaleIdx)}{GetChar(dim0BiasIdx)}{GetChar(dim1ScaleIdx)}{GetChar(dim1BiasIdx)}";
            case 3:
                return $"{(int)biome}66{GetChar(seedIdx0)}{GetChar(seedIdx1)}{GetChar(dim0ScaleIdx)}{GetChar(dim0BiasIdx)}{GetChar(dim1ScaleIdx)}{GetChar(dim1BiasIdx)}{GetChar(dim2ScaleIdx)}{GetChar(dim2BiasIdx)}";
            default:
                return "";
        }
    }

    public static string GetChar(int index)
    {
        if (index < 0)
        {
            return "0";
        }
        else if (index < 10)
        {
            return index.ToString();
        }
        else if (index < 36)
        {
            return $"{(char)(index + 87)}";
        }
        else if (index < 62)
        {
            return $"{(char)(index + 29)}";
        }
        else if (index == 62)
        {
            return "-";
        }
        else
        {
            return "_";
        }
    }
}