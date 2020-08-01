using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const float movementThresholdForChunkUpdate = 25.0f;
    const float sqrMovementThresholdForChunkUpdate = movementThresholdForChunkUpdate * movementThresholdForChunkUpdate;

    public LevelOfDetailInformation[] detailLevels;
    public static float maxViewDistance;

    public Material mapMaterial;
    static MapGenerator mapGenerator;
    public Transform viewer;
    public static Vector2 viewerPosition;
    Vector2 previousViewerPosition;

    int chunkSize;
    int numVisibleChunks;

    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>(); // Position -> Chunk
    List<TerrainChunk> previousFrameChunks = new List<TerrainChunk>();

    private void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold; // Last LOD.
        numVisibleChunks = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((previousViewerPosition - viewerPosition).sqrMagnitude > sqrMovementThresholdForChunkUpdate) {
            previousViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    private void UpdateVisibleChunks() {
        // Set all the previous chunks invisible before updating again.
        for (int i = 0; i < previousFrameChunks.Count; ++i) {
            previousFrameChunks[i].SetVisible(false);
        }
        previousFrameChunks.Clear();

        // Get the coordinate of the chunk the viewer is standing on.
        int currentChunkCoordinateX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordinateY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -numVisibleChunks; yOffset <= numVisibleChunks; ++yOffset) {
            for (int xOffset = -numVisibleChunks; xOffset <= numVisibleChunks; ++xOffset) {
                Vector2 viewedChunkCoordinate = new Vector2(currentChunkCoordinateX + xOffset, currentChunkCoordinateY + yOffset);

                if (terrainChunks.ContainsKey(viewedChunkCoordinate)) {
                    terrainChunks[viewedChunkCoordinate].UpdateTerrainChunk();

                    // Add to list of visible terrain chunks if this chunk is visible.
                    if (terrainChunks[viewedChunkCoordinate].IsVisible()) {
                        previousFrameChunks.Add(terrainChunks[viewedChunkCoordinate]);
                    }
                }
                else {
                    terrainChunks.Add(viewedChunkCoordinate, new TerrainChunk(viewedChunkCoordinate, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject meshObject;
        Vector2 chunkPosition;
        Bounds chunkBounds;

        MapData mapData; // Terrain height / color information.
        bool mapDataReceived;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LevelOfDetailInformation[] detailLevels;
        LevelOfDetailMesh[] lodMeshes;
        int previousLODIndex = -1;


        // Terrain chunk takes in a normalized 'coordinate' based on the number of chunks it is away from the viewer position.
        // Size of the chunk in units is passed in to scale the world position correctly.
        public TerrainChunk(Vector2 coordinate, int size, LevelOfDetailInformation[] detailLevels, Transform parent, Material material) {
            // Initialize meshes for level of detail.
            this.detailLevels = detailLevels;
            lodMeshes = new LevelOfDetailMesh[this.detailLevels.Length];

            for (int i = 0; i < this.detailLevels.Length; ++i) {
                lodMeshes[i] = new LevelOfDetailMesh(this.detailLevels[i].levelOfDetail, UpdateTerrainChunk);
            }

            chunkPosition = coordinate * size;

            Vector3 worldPosition = new Vector3(chunkPosition.x, 0, chunkPosition.y);
            Vector3 worldSize = new Vector3(size, 1, size);
            chunkBounds = new Bounds(chunkPosition, Vector2.one * size);

            meshObject = new GameObject("Terrain chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();

            // Set chunk world position.
            meshObject.transform.position = worldPosition;
            meshObject.transform.parent = parent;
            meshRenderer.material = material;

            SetVisible(false);

            // Register callback.
            Debug.Log(mapGenerator);
            mapGenerator.RequestMapData(OnMapDataReceived, chunkPosition);
        }

        public void UpdateTerrainChunk() {
            if (mapDataReceived) {
                float distanceToNearestPoint = Mathf.Sqrt(chunkBounds.SqrDistance(viewerPosition));
                bool chunkIsVisible = distanceToNearestPoint <= maxViewDistance;

                if (chunkIsVisible) {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; ++i) {
                        // Chunk needs to have less definition than at this level (sufficiently further away).
                        if (distanceToNearestPoint > detailLevels[i].visibleDistanceThreshold) {
                            lodIndex = i + 1;
                        }
                        // Chunk is at the correct LOD.
                        else {
                            break;
                        }
                    }

                    // Only update if the LOD has changed since the previous iteration.
                    if (lodIndex != previousLODIndex) {
                        LevelOfDetailMesh lodMesh = lodMeshes[lodIndex];

                        // LOD mesh has received the mesh.
                        if (lodMesh.hasReceivedMesh) {
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        // If it hasn't requested the mesh, request it and wait for it.
                        else if (!lodMesh.hasRequestedMesh) {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }
                SetVisible(chunkIsVisible);
            }
        }

        public void SetVisible(bool chunkIsVisible) {
            meshObject.SetActive(chunkIsVisible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }

        private void OnMapDataReceived(MapData mapData) {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(MapGenerator.mapChunkSize, MapGenerator.mapChunkSize, mapData.colorMap);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }
    }

    class LevelOfDetailMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasReceivedMesh;
        System.Action updateCallback;

        int levelOfDetail;

        public LevelOfDetailMesh(int levelOfDetail, System.Action updateCallback) {
            this.levelOfDetail = levelOfDetail;
            this.updateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(OnMeshDataReceived, mapData, levelOfDetail);
        }

        private void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasReceivedMesh = true;

            updateCallback();
        }
    }

    [System.Serializable]
    public struct LevelOfDetailInformation {
        public int levelOfDetail;
        public float visibleDistanceThreshold; // Distance away from the viewer this LOD is active. 
                                               // Exceeding this distance will downgrade the LOD to the next level, if possible.
    }

}
