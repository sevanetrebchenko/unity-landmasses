using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] noiseMap) {
        // Get map dimensions.
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);

        // Create 2D texture.
        Texture2D mapTexture = new Texture2D(mapWidth, mapHeight);

        // It's faster to generate an array of pixel colors and set them all at once in the texture than one by one.
        Color[] textureColorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; ++y) {
            for (int x = 0; x < mapWidth; ++x) {
                int colorIndex = x + mapWidth * y;
                textureColorMap[colorIndex] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        mapTexture.SetPixels(textureColorMap);
        mapTexture.Apply();

        // Apply texture texture and dimensions to plane.
        textureRenderer.sharedMaterial.mainTexture = mapTexture;
        textureRenderer.transform.localScale = new Vector3(mapWidth, 1, mapHeight);
    }
}
