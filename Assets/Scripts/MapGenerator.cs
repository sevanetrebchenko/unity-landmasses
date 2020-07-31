using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

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

    public void GenerateMap() {
        // Retrieve noise map.
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, mapSeed, noiseScale, numNoiseOctaves, persistence, lacunarity, offset);

        // Display noise map.
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
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
