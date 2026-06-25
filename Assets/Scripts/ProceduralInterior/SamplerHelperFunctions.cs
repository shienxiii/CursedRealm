using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SamplerHelperFunctions
{
    /// <summary>
    /// Returns true if the point is inside the polygon.
    /// Polygon points should be ordered around the perimeter
    /// (clockwise or counter-clockwise).
    /// </summary>
    public static bool IsPointInsidePolygon(List<Vector2> polygon, Vector2 point)
    {
        int count = 0;

        for(int i = 0; i < polygon.Count; i++)
        {
            Vector2 p0 = polygon[i];
            Vector2 p1 = polygon[(i + 1 )% polygon.Count];

            if ((point.y < p0.y) != (point.y < p1.y) &&
                (point.x < p0.x + ((point.y - p0.y) / (p1.y - p0.y)) * (p1.x - p0.x)))
            { count++; }
        }
        Vector2Int s = new(1,1);
        Debug.Log(count % 2 == 1);
        return count % 2 == 1;
    }


    // Use UnityEngine.Random to get a random element on the provided list and optionally remove from list
    public static T GetRandomElement<T>(List<T> list, bool bRemoveFromList = false)
    {
        T element = list[UnityEngine.Random.Range(0, list.Count)];

        if (bRemoveFromList)
            list.Remove(element);

        return element;
    }

    public static void GetCardinalSample(List<Vector2Int> validSamples, List<Vector2Int> cardinalSamples, Vector2Int sample)
    {
        void TestCardinalSample(List<Vector2Int> validSamples, List<Vector2Int> cardinalSamples, Vector2Int sample)
        {
            // test if cardinal sample still exist in validSamples
            if(validSamples.Contains(sample) && !cardinalSamples.Contains(sample))
                cardinalSamples.Add(sample);

        };

        TestCardinalSample(validSamples, cardinalSamples, sample + new Vector2Int(1, 0));
        TestCardinalSample(validSamples, cardinalSamples, sample + new Vector2Int(-1, 0));
        TestCardinalSample(validSamples, cardinalSamples, sample + new Vector2Int(0, 1));
        TestCardinalSample(validSamples, cardinalSamples, sample + new Vector2Int(0, -1));
    }
}
