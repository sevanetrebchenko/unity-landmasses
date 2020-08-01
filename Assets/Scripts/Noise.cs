using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    public enum NormalizeMode { Local, Global }

    // Returns a grid of values between 0 and 1
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int mapSeed, float noiseScale, int numNoiseOctaves, float persistence, float lacunarity, Vector2 manualOffset, NormalizeMode normalizeMode) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Seeded generation.
        System.Random randomNumberGenerator = new System.Random(mapSeed);
        Vector2[] octaveOffets = new Vector2[numNoiseOctaves];

        float maximumPossibleHeight = 0;
        float amplitude = 1.0f;
        float frequency = 1.0f;

        for (int i = 0; i < numNoiseOctaves; ++i) {
            float offsetX = randomNumberGenerator.Next(-100000, 100000) + manualOffset.x;
            float offsetY = randomNumberGenerator.Next(-100000, 100000) - manualOffset.y;

            octaveOffets[i] = new Vector2(offsetX, offsetY);

            // Get the maximum possible height value (so far) for this chunk.
            maximumPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        if (noiseScale <= 0) {
            noiseScale = 0.0001f;
        }

        float localMaximumNoiseHeight = float.MinValue;
        float localMinimumNoiseHeight = float.MaxValue;

        // Changing noise scale should zoom noise in and out from the center of the screen rather than top left.
        float halfWidth = mapWidth / 2.0f;
        float halfHeight = mapHeight / 2.0f;

        // Generate perlin noise values in the map.
        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {

                amplitude = 1.0f;
                frequency = 1.0f;
                float noiseHeight = 0.0f;

                for (int i = 0; i < numNoiseOctaves; ++i) {
                    float sampleX = (x - halfWidth + octaveOffets[i].x) / noiseScale * frequency;
                    float sampleY = (y - halfHeight + octaveOffets[i].y) / noiseScale * frequency;

                    // Perlin noise gets the same value each time if the arguments passed are integer values.
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1.0f; // Get in the range -1 to 1.
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence; // Persistence should be between 0 and 1 - amplitude decreases with each octave.
                    frequency *= lacunarity;  // Lacunarity should be greater than 1 - frequency increases with each octave.
                }

                // Get the highest and lowest noise map values.
                if (noiseHeight > localMaximumNoiseHeight) {
                    localMaximumNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < localMinimumNoiseHeight) {
                    localMinimumNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize noise map values back to range [0, 1]
        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {
                if (normalizeMode == NormalizeMode.Local) {
                    noiseMap[x, y] = Mathf.InverseLerp(localMinimumNoiseHeight, localMaximumNoiseHeight, noiseMap[x, y]);
                }
                else if (normalizeMode == NormalizeMode.Global) {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2.0f * maximumPossibleHeight / 1.66f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }
}
