using UnityEngine;
using System.Collections.Generic;

public static class DebugVisualLibrary
{
    public static void DrawPath(List<Vector3> points)
    {
        for (int i = 1; i < points.Count; i++)
        {
            Debug.DrawLine(points[i - 1], points[i], Color.azure);
        }
    }
}
