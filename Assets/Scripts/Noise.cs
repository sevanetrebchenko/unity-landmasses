using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    // Returns a grid of values between 0 and 1
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int mapSeed, float noiseScale, int numNoiseOctaves, float persistence, float lacunarity, Vector2 manualOffset) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Seeded generation.
        System.Random randomNumberGenerator = new System.Random(mapSeed);
        Vector2[] octaveOffets = new Vector2[numNoiseOctaves];

        for (int i = 0; i < numNoiseOctaves; ++i) {
            float offsetX = randomNumberGenerator.Next(-100000, 100000) + manualOffset.x;
            float offsetY = randomNumberGenerator.Next(-100000, 100000) + manualOffset.y;

            octaveOffets[i] = new Vector2(offsetX, offsetY);
        }

        if (noiseScale <= 0) {
            noiseScale = 0.0001f;
        }

        float maximumNoiseHeight = float.MinValue;
        float minimumNoiseHeight = float.MaxValue;

        // Changing noise scale should zoom noise in and out from the center of the screen rather than top left.
        float halfWidth = mapWidth / 2.0f;
        float halfHeight = mapHeight / 2.0f;

        // Generate perlin noise values in the map.
        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {

                float amplitude = 1.0f;
                float frequency = 1.0f;
                float noiseHeight = 0.0f;

                for (int i = 0; i < numNoiseOctaves; ++i) {
                    float sampleX = (x - halfWidth) / noiseScale * frequency + octaveOffets[i].x;
                    float sampleY = (y - halfHeight) / noiseScale * frequency + octaveOffets[i].y;

                    // Perlin noise gets the same value each time if the arguments passed are integer values.
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1.0f; // Get in the range -1 to 1.
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence; // Persistence should be between 0 and 1 - amplitude decreases with each octave.
                    frequency *= lacunarity;  // Lacunarity should be greater than 1 - frequency increases with each octave.
                }

                // Get the highest and lowest noise map values.
                if (noiseHeight > maximumNoiseHeight) {
                    maximumNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minimumNoiseHeight) {
                    minimumNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize noise map values back to range [0, 1]
        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {
                noiseMap[x, y] = Mathf.InverseLerp(minimumNoiseHeight, maximumNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
