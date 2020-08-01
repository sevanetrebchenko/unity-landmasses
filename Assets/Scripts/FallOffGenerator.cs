using UnityEngine;

public static class FallOffGenerator
{
    public static float[,] GenerateFalloffMap(int mapSize) {
        float[,] falloffMap = new float[mapSize, mapSize];

        for (int y = 0; y < mapSize; ++y) {
            for (int x = 0; x < mapSize; ++x) {
                float coordinateX = y / (float)mapSize * 2 - 1;
                float coordinateY = x / (float)mapSize * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(coordinateX), Mathf.Abs(coordinateY));

                falloffMap[x, y] = Evaluate(value);
            }
        }

        return falloffMap;
    }

    // Function is expensive but is only used once at the start of the game, so there is little performance implications.
    private static float Evaluate(float value) {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
