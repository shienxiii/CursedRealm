using System;
using System.Collections.Generic;
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
            // make the sample is within _samples coverage
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
}
