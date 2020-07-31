using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {
    // Get the map texture based on a color array.
    public static Texture2D TextureFromColorMap(int mapWidth, int mapHeight, Color[] textureColorMap) {
        Texture2D mapTexture = new Texture2D(mapWidth, mapHeight);

        // Prevent blurry pixels.
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;

        mapTexture.SetPixels(textureColorMap);
        mapTexture.Apply();

        return mapTexture;
    }

    // Get the map texture based on a height array.
    public static Texture2D TextureFromHeightMap(float[,] heightMap) {
        // Get map dimensions.
        int mapWidth = heightMap.GetLength(0);
        int mapHeight = heightMap.GetLength(1);

        // It's faster to generate an array of pixel colors and set them all at once in the texture than one by one.
        Color[] textureColorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {
                int colorIndex = x + mapWidth * y;
                textureColorMap[colorIndex] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(mapWidth, mapHeight, textureColorMap);
    }
}
