using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    // Returns a grid of values between 0 and 1
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float noiseScale) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        if (noiseScale <= 0) {
            noiseScale = 0.0001f;
        }

        // Generate perlin noise values in the map.
        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {
                float sampleX = x / noiseScale;
                float sampleY = y / noiseScale;

                // Perlin noise gets the same value each time if the arguments passed are integer values.
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;
    }
}
