using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {

    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve, int levelOfDetail) {
        // Get map dimensions.
        int mapWidth = heightMap.GetLength(0);
        int mapHeight = heightMap.GetLength(1);

        // Have (0, 0) be at the center of the mesh instead of the top left corner.
        float topLeftX = (mapWidth - 1) / -2.0f;
        float topLeftZ = (mapHeight - 1) / 2.0f;

        // Range of the simplification element is 2 * the level of detail to one of the following pre-determined levels of detail: 2, 4, 6, 8, 10 ,12
        // If level of detail is 0, we want full detail (1).
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerEdge = (mapWidth - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerEdge, verticesPerEdge);
        int vertexIndex = 0;

        for (int y = 0; y < mapHeight; y += meshSimplificationIncrement) {
            for (int x = 0; x < mapWidth; x += meshSimplificationIncrement) {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, meshHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)mapWidth, y / (float)mapHeight);

                // Ignore right and bottom edge vertices of the map.
                if (x < (mapWidth - 1) && y < (mapHeight - 1)) {
                    // 2 triangles:
                    //          vertexIndex       vertexIndex + 1
                    //               |--------------|
                    //               | \            |
                    //               |    \         |
                    //               |       \      |
                    //               |          \   |
                    //               |             \|
                    //               |--------------|
                    // vertexIndex + mapWidth      vertexIndex + mapWidth + 1
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerEdge + 1, vertexIndex + verticesPerEdge);
                    meshData.AddTriangle(vertexIndex + verticesPerEdge + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangleIndices;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight) {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangleIndices = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        triangleIndex = 0;
    }

    public void AddTriangle(int index1, int index2, int index3) {
        triangleIndices[triangleIndex++] = index1;
        triangleIndices[triangleIndex++] = index2;
        triangleIndices[triangleIndex++] = index3;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        return mesh;
    }
}
