using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestNoiseGUI : MonoBehaviour
{
    public RawImage image;

    public int width = 100;
    public int length = 100;

    [Range(0.0f, 0.5f)]
    public float frequency;
    [Range(0.0f, 1.0f)]
    public float persistence;
    [Range(1.0f, 50.0f)]
    public float lacunarity;

    public float floatOffset;

    private void Start()
    {
        Texture2D texture = new Texture2D(width, length);

        //int seed = Random.Range(0, 10000);
        int seed = 0;

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                //float noise = Mathf.PerlinNoise(Mathf.RoundToInt(x * frequency) + floatOffset, Mathf.RoundToInt(y * frequency) + floatOffset);
                float noise = GetMultiOctaveNoise(x, y, frequency, persistence, lacunarity, seed);
                texture.SetPixel(x, y, new Color(noise, noise, noise));
            }
        }

        texture.Apply();
        image.texture = texture;
    }

    private float GetMultiOctaveNoise(float x, float y, float frequency, float persistence, float lacunarity, int seed)
    {
        float max = 0.0f;
        float noise = 0.0f;
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

            //Debug.Log($"TestNoiseGUI :: [{x},{y}] Oct: {oct:0.##} Noise: {noise:0.##} Max: {max:0.##}");
        }
        return noise / max;
    }
}