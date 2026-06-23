using System.Collections.Generic;
using System.Drawing;
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

        Debug.Log(count % 2 == 1);
        return count % 2 == 1;
    }
}
