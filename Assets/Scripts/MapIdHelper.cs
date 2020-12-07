using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapIdHelper : MonoBehaviour
{
    // TODO: Implement with string builder

    public static string GetMapId(int baseSeed, float wetFreqNrml, float wetBiasNrml, float vegFreqNrml, float vegBiasNrml)
    {
        int seedIdx0 = baseSeed / 64;
        int seedIdx1 = baseSeed % 64;

        //float wetFreqNrml = 1.0f - (wetScale / 64.0f) * 1.0f;
        //float wetBiasNrml = (wetBias / 64.0f) * 0.5f;
        //float vegFreqNrml = 1.0f - (vegScale / 64.0f) * 1.0f;
        //float vegBiasNrml = (vegBias / 64.0f) * 0.5f;

        int wetScale = Mathf.RoundToInt((1.0f - wetFreqNrml) * 64.0f);
        int wetBias = Mathf.RoundToInt(wetBiasNrml * 2.0f * 64.0f);
        int vegScale = Mathf.RoundToInt((1.0f - vegFreqNrml) * 64.0f);
        int vegBias = Mathf.RoundToInt(vegBiasNrml * 2.0f * 64.0f);

        int wetScaleIdx = 64 - wetScale;
        int wetBiasIdx = wetBias + 31;

        int vegScaleIdx = 64 - vegScale;
        int vegBiasIdx = vegBias + 31;

        return $"066{GetChar(seedIdx0)}{GetChar(seedIdx1)}{GetChar(wetScaleIdx)}{GetChar(wetBiasIdx)}{GetChar(vegScaleIdx)}{GetChar(vegBiasIdx)}";
    }

    public static string GetMapId(int baseSeed, int wetScale, int wetBias, int vegScale, int vegBias)
    {
        int seedIdx0 = baseSeed / 64;
        int seedIdx1 = baseSeed % 64;

        int wetScaleIdx = 64 - wetScale;
        int wetBiasIdx = wetBias + 31;

        int vegScaleIdx = 64 - vegScale;
        int vegBiasIdx = vegBias + 31;

        return $"066{GetChar(seedIdx0)}{GetChar(seedIdx1)}{GetChar(wetScaleIdx)}{GetChar(wetBiasIdx)}{GetChar(vegScaleIdx)}{GetChar(vegBiasIdx)}";
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