using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode {
        NoiseMap,
        ColorMap,
        Mesh
    }
    public DrawMode drawMode;

    const int mapChunkSize = 241; // Unity hard caps number of vertices per mesh to be 65,025 vertices (255 per edge)
                                  // Formula ((mapWidth - 1) / LOD + 1) allows an optimal 241 vertices per edge, which works perfectly for LOD levels: 1, 2, 4, 6, 8, 10, and 12.
                                  // Allowing for 7 different levels of detail.
    [Range(0, 6)]
    public int levelOfDetail;
    public float noiseScale;

    public int numNoiseOctaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public int mapSeed;

    public bool autoUpdate;

    public TerrainTypes[] terrainRegions;

    public void GenerateMap() {
        // Retrieve noise map.
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, mapSeed, noiseScale, numNoiseOctaves, persistence, lacunarity, offset);

        // Generate color array for map.
        Color[] textureColorMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; ++y) {
            for (int x = 0; x < mapChunkSize; ++x) {
                float terrainHeight = noiseMap[x, y];
                int colorIndex = x + mapChunkSize * y;

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
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapChunkSize, mapChunkSize, textureColorMap));
        }
        else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapChunkSize, mapChunkSize, textureColorMap));
        }
    }

    // Automatically called when variables are changed in the inspector.
    // Used to verify (and clamp) data.
    private void OnValidate() {
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
