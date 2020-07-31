using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode {
        NoiseMap,
        ColorMap
    }
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;

    public float noiseScale;

    public int numNoiseOctaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;
    public Vector2 offset;

    public int mapSeed;

    public bool autoUpdate;

    public TerrainTypes[] terrainRegions;

    public void GenerateMap() {
        // Retrieve noise map.
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, mapSeed, noiseScale, numNoiseOctaves, persistence, lacunarity, offset);

        // Generate color array for map.
        Color[] textureColorMap = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {
                float terrainHeight = noiseMap[x, y];
                int colorIndex = x + mapWidth * y;

                for (int i = 0; i < terrainRegions.Length; ++i) {

                    // Found the region the current height the point at (x, y) belongs to.
                    if (terrainHeight <= terrainRegions[i].startingHeight) {
                        textureColorMap[colorIndex] = terrainRegions[i].color;
                        break;
                    }
                }
            }
        }

        // Draw map.
        MapDisplay display = FindObjectOfType<MapDisplay>();
        
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapWidth, mapHeight, textureColorMap));
        }
    }

    // Automatically called when variables are changed in the inspector.
    // Used to verify (and clamp) data.
    private void OnValidate() {
        if (mapWidth < 1) {
            mapWidth = 1;
        }

        if (mapHeight < 1) {
            mapHeight = 1;
        }

        if (numNoiseOctaves < 0) {
            numNoiseOctaves = 0;
        }

        if (lacunarity < 1.0f) {
            lacunarity = 1.0f;
        }
    }
}

[System.Serializable]
public struct TerrainTypes {
    public string name;
    public float startingHeight;
    public Color color;
}
