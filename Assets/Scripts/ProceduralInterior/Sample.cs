using UnityEngine;

/// <summary>
/// This enum serve as a flag on a sample.
/// FREE        : Available to spawn static objects on top on
/// RESERVED    : Must be left free and must be reachable
/// OCCUPIED    : A static object have been spawned on it and is treated as non-path
/// </summary>
public enum SampleState
{
    FREE,
    RESERVED,
    OCCUPIED
}

// Represents a sample on the grid
public class Sample
{
    // This is the index of the room this sample is assigned to
    private int _roomIndex = -1;

    // When spawning static object to fill the level, the sample occupied by that object will be marked as OCCUPIED
    private SampleState _state = SampleState.FREE;

    public int RoomIndex
    {
        get  => _roomIndex;
        set => _roomIndex = value;
    }

    public SampleState State
    {
        get => _state;
        set => _state = value;
    }

    public Sample(int roomIndex)
    {
        _roomIndex = roomIndex;
    }
}
