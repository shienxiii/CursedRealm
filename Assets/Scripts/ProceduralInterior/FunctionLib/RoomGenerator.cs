using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RoomGenerator
{
    public static void GenerateRooms(Interior interior, List<Vector2Int> validSamples)
    {
        if (!interior.Settings || (interior.Samples?.Length ?? 0) == 0 || validSamples.Count == 0) return;

        int roomCount = interior.InteriorRandom.Next(interior.Settings.MinRoomCount, interior.Settings.MaxRoomCount + 1);
        int roomSizeOffset = Mathf.Abs(interior.Settings.Offset);

        // We want to cache the room samples before creating the Room objects
        List<List<Vector2Int>> roomCache = new List<List<Vector2Int>>();

        int targetSize = 0;

        while (validSamples.Count > 0)
        {
            if (roomCount - interior.Rooms.Count <= 0 || validSamples.Count < interior.Settings.MinSamplesPerRoom)
                targetSize = validSamples.Count;
            else
                targetSize = Math.Clamp((validSamples.Count / roomCount) + interior.InteriorRandom.Next(-roomSizeOffset, roomSizeOffset + 1),
                            interior.Settings.MinSamplesPerRoom,
                            validSamples.Count);

            List<Vector2Int> roomSamples = new List<Vector2Int>(); // samples to be cached as a room
            List<Vector2Int> cardinalSamples = new List<Vector2Int>(); // samples directly next to the samples in roomSamples that are currently -1

            int roomIndex = roomCache.Count;

            // get a starting point to generate the room and add to the cardinalSamples list
            cardinalSamples.Add(SamplerHelperFunctions.GetRandomElement(validSamples, interior.InteriorRandom, false));

            // Generate the room recursively
            GenerateRoom_Recursive(interior, validSamples, roomSamples, cardinalSamples, targetSize, roomIndex);

            if (roomSamples.Count < interior.Settings.MinSamplesPerRoom)
            {
                int result = TryMeldSamplesToExistingRoom(interior, roomSamples, roomCache, roomIndex);
                if (result >= 0)
                    continue;
            }

            roomCache.Add(roomSamples);

        }

        foreach (var room in roomCache)
            interior.Rooms.Add(new Room(room));
    }

    private static void GenerateRoom_Recursive(Interior interior, List<Vector2Int> validSamples, List<Vector2Int> roomSamples, List<Vector2Int> cardinalSamples, int targetSize, int roomIndex)
    {
        // Get a random sample from cardinalSamples and remove the sample from both roomSamples and cardinalSamples
        Vector2Int currentSample = SamplerHelperFunctions.GetRandomElement(cardinalSamples, interior.InteriorRandom, true);
        validSamples.Remove(currentSample);

        // Assign roomIndex to the position on interior.Samples and assign currentSample to roomSamples
        interior.Samples[currentSample.x, currentSample.y].RoomIndex = roomIndex;
        roomSamples.Add(currentSample);

        ExtendRoomInDirection(interior, validSamples, roomSamples, cardinalSamples, currentSample, new Vector2Int(1, 0), roomIndex);
        ExtendRoomInDirection(interior, validSamples, roomSamples, cardinalSamples, currentSample, new Vector2Int(-1, 0), roomIndex);
        ExtendRoomInDirection(interior, validSamples, roomSamples, cardinalSamples, currentSample, new Vector2Int(0, 1), roomIndex);
        ExtendRoomInDirection(interior, validSamples, roomSamples, cardinalSamples, currentSample, new Vector2Int(0, -1), roomIndex);

        // collect the cardinal samples for currentSample
        SamplerHelperFunctions.GetCardinalSample(validSamples, cardinalSamples, currentSample);

        // Call this function recursively until roomSamples.Count equal or exceed targetSize or cardinalSamples is empty
        if (roomSamples.Count < targetSize && cardinalSamples.Count > 0)
            GenerateRoom_Recursive(interior, validSamples, roomSamples, cardinalSamples, targetSize, roomIndex);

    }

    private static void ExtendRoomInDirection(Interior interior, List<Vector2Int> validSamples, List<Vector2Int> roomSamples, List<Vector2Int> cardinalSamples, Vector2Int fromSample, Vector2Int direction, int roomIndex)
    {
        Vector2Int currentSample = fromSample + direction;

        if (!cardinalSamples.Contains(currentSample)) return;

        // we got a valid sample, include it in the room and remove from AvailableSamples and CardinalSamples
        interior.Samples[currentSample.x, currentSample.y].RoomIndex = roomIndex;
        roomSamples.Add(currentSample);
        validSamples.Remove(currentSample);
        cardinalSamples.Remove(currentSample);

        // run this function recursively until reaching a sample point that's not in CardinalSamples
        ExtendRoomInDirection(interior, validSamples, roomSamples, cardinalSamples, currentSample, direction, roomIndex);

        // collect the cardinal samples for currentSample
        SamplerHelperFunctions.GetCardinalSample(validSamples, cardinalSamples, currentSample);
    }

    /// <summary>
    /// Try to meld the provided roomSamples to the closest neighbouring room
    /// </summary>
    /// <param name="roomSamples">list of samples to meld</param>
    /// <param name="currentIndex">this is the roomIndex to ignore besides -1</param>
    private static int TryMeldSamplesToExistingRoom(Interior interior, List<Vector2Int> roomSamples, List<List<Vector2Int>> inRoomCache, int currentIndex)
    {
        List<int> roomCandidates = new List<int>();

        void GetCardinalSampleValue(Vector2Int sample)
        {
            // make the sample is within interior.Samples coverage
            if (sample.x < 0 || sample.y < 0 || sample.x >= interior.SizeX || sample.y >= interior.SizeY || interior.Samples[sample.x, sample.y] == null) return;

            int index = interior.Samples[sample.x, sample.y].RoomIndex;

            if (index != currentIndex && index > -1 && !roomCandidates.Contains(index))
                roomCandidates.Add(index);
        }
        ;

        foreach (Vector2Int sample in roomSamples)
        {
            GetCardinalSampleValue(sample + new Vector2Int(1, 0));
            GetCardinalSampleValue(sample + new Vector2Int(-1, 0));
            GetCardinalSampleValue(sample + new Vector2Int(0, 1));
            GetCardinalSampleValue(sample + new Vector2Int(0, -1));
        }

        if (roomCandidates.Count == 0) return -1;

        int newIndex = SamplerHelperFunctions.GetRandomElement(roomCandidates, interior.InteriorRandom);

        foreach (Vector2Int sample in roomSamples)
        {
            interior.Samples[sample.x, sample.y].RoomIndex = newIndex;
            inRoomCache[newIndex].Add(sample);
        }

        return newIndex;
    }

    public static void SampleWallAndRoomConnections(Interior interior)
    {
        if (interior.Samples == null || interior.Settings == null)
            return;

        int width = interior.Samples.GetLength(0);
        int height = interior.Samples.GetLength(1);

        float cellSize = interior.Settings.SampleDimension.x;
        float halfSize = cellSize * 0.5f;

        HashSet<WallSample> walls = new HashSet<WallSample>();

        // Test if there is a wall between 2 sample point and add the wall
        void ParseWall(in Vector3 start, in Vector3 end, in int roomA, in Vector2Int sampleA, in Vector2Int sampleB)
        {
            // test to see if next sample is within spline
            bool isInside = sampleB.x >= 0 && sampleB.y >= 0 && sampleB.x < width && sampleB.y < height && interior.Samples[sampleB.x, sampleB.y] != null;

            int roomB = isInside ? interior.Samples[sampleB.x, sampleB.y].RoomIndex : -1;

            if (roomA == roomB) return;

            WallSample newWall = new WallSample(roomA, roomB, new Vector2Int(sampleA.x, sampleA.y), new Vector2Int(sampleB.x, sampleB.y), start, end);
            if (!walls.Add(newWall)) return;

            interior.WallSamples.Add(newWall);

            int wallIndex = interior.WallSamples.Count - 1;

            // if roomA and roomB are index to actual room, add room connection here
            if (roomA > -1 && roomB > -1)
            {
                interior.Rooms[roomA].AddNeighbourForRoom(roomB, wallIndex);
                interior.Rooms[roomB].AddNeighbourForRoom(roomA, wallIndex);
            }

        }

        // test from index [-1, -1] to remove the need to test for wall in left or rear direction
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int roomA = x > -1 && z > -1 && interior.Samples[x, z] != null ? interior.Samples[x, z].RoomIndex : -1;

                /** Grid Guide
                 *  tl-----tr    
                 *  |      |
                 *  |      |
                 *  bl-----br
                 */
                Vector3 tl = interior.GridPointToWorldPoint(x, z + 1, false);
                Vector3 br = interior.GridPointToWorldPoint(x + 1, z, false);
                Vector3 tr = interior.GridPointToWorldPoint(x + 1, z + 1, false);

                ParseWall(br, tr, roomA,new Vector2Int(x, z), new Vector2Int(x + 1, z));
                ParseWall(tl, tr, roomA, new Vector2Int(x, z), new Vector2Int(x, z + 1));

                if (x == 0 || z == 0)
                {
                    Vector3 bl = interior.GridPointToWorldPoint(x, z, false);
                    if (x == 0)
                        ParseWall(bl, tl, roomA, new Vector2Int(x, z), new Vector2Int(x - 1, z));

                    if (z == 0)
                        ParseWall(bl, br, roomA, new Vector2Int(x, z), new Vector2Int(x, z - 1));
                }
            }
        }

        for (int i = 0; i < interior.Rooms.Count; i++)
        {
            string message = string.Format($"Room {i} : Connected Rooms {interior.Rooms[i].Neighbour.Count} |");
            foreach (KeyValuePair<int, List<int>> connection in interior.Rooms[i].Neighbour)
                message = string.Format($"{message} [{connection.Key} : {connection.Value.Count} walls]");

            Debug.Log(message);
        }
    }

    public static void RandomizePath(Interior interior)
    {
        // Go through the room connection starting from a random room and work through all
        // DoorCandidates to determine whether to break connection or not
    }

    public static void SampleDoor(Interior interior)
    {
        for (int roomIndex = 0; roomIndex < interior.Rooms.Count; roomIndex++)
        {
            Room current= interior.Rooms[roomIndex];
            Dictionary<int, List<int>> doorCandidates = current.Neighbour;
            List<int> nextRooms = current.Neighbour.Keys.ToList();

            foreach (int nextIndex in nextRooms)
            {
                Room next = interior.Rooms[nextIndex];

                int wallIndex = SamplerHelperFunctions.GetRandomElement(doorCandidates[nextIndex], interior.InteriorRandom);

                // flag the selected wall for a door
                WallSample newDoor = interior.WallSamples[wallIndex];
                newDoor.IsDoor = true;
                interior.WallSamples[wallIndex] = newDoor;

                // flag the samples on both sides of the door as reserved
                interior.Samples[newDoor.RoomA.Value.x, newDoor.RoomA.Value.y].State = SampleState.RESERVED;
                interior.Samples[newDoor.RoomB.Value.x, newDoor.RoomB.Value.y].State = SampleState.RESERVED;

                // Remove both rooms from each others ConnectingWalls Dictionary
                interior.Rooms[roomIndex].RemoveNeighbourForRoom(nextIndex);
                interior.Rooms[nextIndex].RemoveNeighbourForRoom(roomIndex);

                // Add reference to door
                interior.Rooms[roomIndex].AddDoor(nextIndex, wallIndex);
                interior.Rooms[roomIndex].AddDoor(roomIndex, wallIndex);
            }
        }

        foreach (WallSample wall in interior.WallSamples)
        {
            Debug.Log(wall);
        }
    }
}
