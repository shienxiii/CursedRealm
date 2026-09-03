using UnityEngine;

public static class CustomVectorMath
{
    public static bool EqualWithTolerance(Vector3 v1, Vector3 v2, float tolerance = 0.001f)
    {
        return (v1 - v2).sqrMagnitude <= tolerance * tolerance;
    }
}
