using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode {
        NoiseMap,
        ColorMap,
        Mesh
    }
    public DrawMode drawMode;

    public const int mapChunkSize = 241; // Unity hard caps number of vertices per mesh to be 65,025 vertices (255 per edge)
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

    Queue<MapThreadInformation<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInformation<MapData>>();
    Queue<MapThreadInformation<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInformation<MeshData>>();

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData();

        // Draw map.
        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapChunkSize, mapChunkSize, mapData.colorMap));
        }
        else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapChunkSize, mapChunkSize, mapData.colorMap));
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Map data threading.
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public void RequestMapData(System.Action<MapData> callbackFunction) {
        // Create lambda function for the data processing function.
        ThreadStart threadStart = delegate {
            MapDataThread(callbackFunction);
        };

        // Start the function on a different thread.
        new Thread(threadStart).Start();
    }

    private void MapDataThread(System.Action<MapData> callbackFunction) {
        MapData mapData = GenerateMapData();

        // Lock queue to preserve access order.
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInformation<MapData>(callbackFunction, mapData));
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Mesh data threading.
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public void RequestMeshData(MapData mapData, System.Action<MeshData> callbackFunction) {
        // Create lambda function for the data processing function.
        ThreadStart threadStart = delegate {
            MeshDataThread(callbackFunction, mapData);
        };

        // Start the function on a different thread.
        new Thread(threadStart).Start();
    }

    private void MeshDataThread(System.Action<MeshData> callbackFunction, MapData mapData) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);

        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInformation<MeshData>(callbackFunction, meshData));
        }
    }

    private void Update() {
        if (mapDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; ++i) {
                MapThreadInformation<MapData> threadInformation = mapDataThreadInfoQueue.Dequeue();
                threadInformation.callbackFunction(threadInformation.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; ++i) {
                MapThreadInformation<MeshData> threadInformation = meshDataThreadInfoQueue.Dequeue();
                threadInformation.callbackFunction(threadInformation.parameter);
            }
        }
    }

    private MapData GenerateMapData() {
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

        return new MapData(noiseMap, textureColorMap);

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

    // Generic function as threading will be done for both map and mesh generation.
    private struct MapThreadInformation<T> {
        public readonly System.Action<T> callbackFunction;
        public readonly T parameter;

        public MapThreadInformation(System.Action<T> callbackFunction, T parameter) {
            this.callbackFunction = callbackFunction;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainTypes {
    public string name;
    public float startingHeight;
    public Color color;
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
