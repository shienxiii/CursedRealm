using UnityEngine;

/// <summary>
/// Represents a collection of sample point in a Room.
/// Used to reduce memory consumption of sample information and reduce number of floor mesh to spawn.
/// </summary>
public struct Space
{
    public Vector2Int StartPoint;
    public Vector2Int Size;
}

public struct WallSegment
{
    // Holds the index of the samples separated by this wall
    public Vector2Int IndexA, IndexB;

    // Holds the sample values separated by this wall
    public int ValueA, ValueB;

    public bool IsWallForSamples(Vector2Int inIndexA, Vector2Int inIndexB)
    {
        return (IndexA == inIndexA && IndexB == inIndexB) || (IndexA == inIndexB && IndexB == inIndexA);
    }
}