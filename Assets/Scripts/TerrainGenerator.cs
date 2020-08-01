using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public const float maxViewDistance = 450;
    public Transform viewer;
    public static Vector2 viewerPosition;

    int chunkSize;
    int numVisibleChunks;

    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>(); // Position -> Chunk
    List<TerrainChunk> previousFrameChunks = new List<TerrainChunk>();

    private void Start() {
        chunkSize = MapGenerator.mapChunkSize - 1;
        numVisibleChunks = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    terrainChunks.Add(viewedChunkCoordinate, new TerrainChunk(viewedChunkCoordinate, chunkSize, transform));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject meshObject;
        Vector2 chunkPosition;
        Bounds chunkBounds;

        // Terrain chunk takes in a normalized 'coordinate' based on the number of chunks it is away from the viewer position.
        // Size of the chunk in units is passed in to scale the world position correctly.
        public TerrainChunk(Vector2 coordinate, int size, Transform parent) {
            chunkPosition = coordinate * size;
            chunkBounds = new Bounds(chunkPosition, Vector2.one * size);
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);

            // Set chunk world position.
            Vector3 worldPosition = new Vector3(chunkPosition.x, 0, chunkPosition.y);
            meshObject.transform.position = worldPosition;
            meshObject.transform.localScale = Vector3.one * size / 10; // Primitive plane is 10 units by default.
            meshObject.transform.parent = parent;

            SetVisible(false);
        }

        public void UpdateTerrainChunk() {
            float distanceToNearestPoint = Mathf.Sqrt(chunkBounds.SqrDistance(viewerPosition));
            bool chunkIsVisible = distanceToNearestPoint <= maxViewDistance;
            SetVisible(chunkIsVisible);
        }

        public void SetVisible(bool chunkIsVisible) {
            meshObject.SetActive(chunkIsVisible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }
}
