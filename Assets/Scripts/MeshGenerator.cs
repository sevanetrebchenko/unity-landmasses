using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {

    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve) {
        // Get map dimensions.
        int mapWidth = heightMap.GetLength(0);
        int mapHeight = heightMap.GetLength(1);

        // Have (0, 0) be at the center of the mesh instead of the top left corner.
        float topLeftX = (mapWidth - 1) / -2.0f;
        float topLeftZ = (mapHeight - 1) / 2.0f;

        MeshData meshData = new MeshData(mapWidth, mapHeight);
        int vertexIndex = 0;

        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {
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
                    meshData.AddTriangle(vertexIndex, vertexIndex + mapWidth + 1, vertexIndex + mapWidth);
                    meshData.AddTriangle(vertexIndex + mapWidth + 1, vertexIndex, vertexIndex + 1);
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
