using UnityEngine;

public static class CustomVectorMath
{
    public static bool EqualWithTolerance(Vector3 v1, Vector3 v2, float tolerance = 0.001f)
    {
        return (v1 - v2).sqrMagnitude <= tolerance * tolerance;
    }

    /// <summary>
    /// Calculate the difference in scale between 2 Vector3
    /// </summary>
    /// <param name="v1">Base Vector</param>
    /// <param name="v2">Target Vector</param>
    /// <returns>Vector3 representing the scale of v2 relative to v1</returns>
    public static Vector3 GetScaleDifference(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v2.x / v1.x,
                            v2.y / v1.y,
                            v2.z / v1.z);
    }
}
