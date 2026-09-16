namespace ProceduralInterior.Types
{

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
        public int RoomIndex;

        // When spawning static object to fill the level, the sample occupied by that object will be marked as OCCUPIED
        public SampleState State = SampleState.FREE;
        
        public Sample()
        {
            RoomIndex = -1;
        }

        public Sample(int roomIndex)
        {
            RoomIndex = roomIndex;
        }
    }
}