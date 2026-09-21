using System;
using System.Collections.Generic;
using ProceduralInterior.Types;
using System.Linq;
using UnityEngine;

namespace ProceduralInterior.FunctionLib
{
    public static class RoomGenerator
    {
        /// <summary>
        /// To be called after an interior grid sample have been initialized and valid samples have been determined.
        /// Generate all the rooms from the provided interior and validSamples
        /// </summary>
        /// <param name="interior"></param>
        /// <param name="validSamples"></param>
        public static void GenerateRooms(Interior interior, List<Vector2Int> validSamples)
        {
            if (interior == null || validSamples == null ||
                interior.Settings == null || (interior.Grid?.Length ?? 0) == 0
                || validSamples.Count == 0) return;

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
            {
                int roomIndex = interior.Rooms.Count;
                Room newRoom = new Room(room);
                interior.Rooms.Add(newRoom);
                SpaceGenerator.CalculateRoomSpaces(interior, roomIndex);
            }
        }

        /// <summary>
        /// Use recursion to generate a Room
        /// NEVER CALL FROM ANYWHERE ELSE OTHER THAN RoomGenerator::GenerateRooms()
        /// AS NULL CHECKS HAVE BEEN OMITTED DUE TO THIS BEING CALLED A MASSIVE AMOUNT OF TIMES ON EVERY ROOM GENERATION
        /// </summary>
        /// <param name="interior">the interior in which to generate the room for</param>
        /// <param name="validSamples">list of index to samples that are available to be used for room generation</param>
        /// <param name="roomSamples">list of index to samples for the current room being generated</param>
        /// <param name="cardinalSamples">list of index to available samples to directly next to all roomSamples in the cardinal directions</param>
        /// <param name="targetSize">the minimum number of samples we want in roomSamples</param>
        /// <param name="roomIndex">index of the room currently being generated</param>
        private static void GenerateRoom_Recursive(Interior interior, List<Vector2Int> validSamples, List<Vector2Int> roomSamples, List<Vector2Int> cardinalSamples, int targetSize, int roomIndex)
        {
            // Get a random sample from cardinalSamples and remove the sample from both roomSamples and cardinalSamples
            Vector2Int currentSample = SamplerHelperFunctions.GetRandomElement(cardinalSamples, interior.InteriorRandom, true);
            validSamples.Remove(currentSample);

            // Assign roomIndex to the position on interior.Samples and assign currentSample to roomSamples
            interior.Grid[currentSample.x, currentSample.y].RoomIndex = roomIndex;
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

        /// <summary>
        /// During room generation, when we select a sample to put include in a room,
        /// we also want to extend the inclusion in all 4 cardinal directions for all samples that are already in cardinalSamples.
        /// DO NOT CALL ANYWHERE ELSE OTHER THAN RoomGenerator::GenerateRoom_Recursive()
        /// AS NULL CHECKS HAVE BEEN OMITTED DUE TO THIS BEING CALLED A MASSIVE AMOUNT OF TIMES ON EVERY ROOM GENERATION
        /// </summary>
        /// <param name="interior">the interior in which to generate the room for</param>
        /// <param name="validSamples">list of index to samples that are available to be used for room generation</param>
        /// <param name="roomSamples">list of index to samples for the current room being generated</param>
        /// <param name="cardinalSamples">list of index to available samples to directly next to all roomSamples in the cardinal directions</param>
        /// <param name="fromSample">the sample to extend out from</param>
        /// <param name="direction">the direction to extend towards</param>
        /// <param name="roomIndex">index of the room currently being generated</param>
        private static void ExtendRoomInDirection(Interior interior, List<Vector2Int> validSamples, List<Vector2Int> roomSamples, List<Vector2Int> cardinalSamples, Vector2Int fromSample, Vector2Int direction, int roomIndex)
        {
            Vector2Int currentSample = fromSample + direction;

            if (!cardinalSamples.Contains(currentSample)) return;

            // we got a valid sample, include it in the room and remove from AvailableSamples and CardinalSamples
            interior.Grid[currentSample.x, currentSample.y].RoomIndex = roomIndex;
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
        /// <return>index of the room melded into</return>
        private static int TryMeldSamplesToExistingRoom(Interior interior, List<Vector2Int> roomSamples, List<List<Vector2Int>> inRoomCache, int currentIndex)
        {
            if (interior == null || interior.Grid == null || roomSamples == null || inRoomCache == null) return -1;

            List<int> roomCandidates = new List<int>();

            Sample[,] grid = interior.Grid;
            int maxX = grid.GetLength(0);
            int maxY = grid.GetLength(1);

            void GetCardinalSampleValue(Vector2Int sample)
            {
                // make the sample is within interior.Samples coverage
                if (sample.x < 0 || sample.y < 0 || sample.x >= maxX || sample.y >= maxY || grid[sample.x, sample.y] == null) return;

                int index = interior.Grid[sample.x, sample.y].RoomIndex;

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
                interior.Grid[sample.x, sample.y].RoomIndex = newIndex;
                inRoomCache[newIndex].Add(sample);
            }

            return newIndex;
        }

        /// <summary>
        /// Sample an Interior's grid an determine indentify where walls should be placed
        /// and what room is directly next to each other
        /// </summary>
        /// <param name="interior">the Interior to sample</param>
        public static void SampleWallAndRoomConnections(Interior interior)
        {
            if (interior == null || interior.Grid == null || interior.Settings == null)
                return;

            int width = interior.Grid.GetLength(0);
            int height = interior.Grid.GetLength(1);

            float cellSize = interior.Settings.SampleDimension.x;
            float halfSize = cellSize * 0.5f;

            HashSet<WallSample> walls = new HashSet<WallSample>();

            // Test if there is a wall between 2 sample point and add the wall
            void ParseWall(in Vector3 start, in Vector3 end, in int roomA, in Vector2Int sampleA, in Vector2Int sampleB)
            {
                // test to see if next sample is within spline
                bool isInside = sampleB.x >= 0 && sampleB.y >= 0 && sampleB.x < width && sampleB.y < height && interior.Grid[sampleB.x, sampleB.y] != null;

                int roomB = isInside ? interior.Grid[sampleB.x, sampleB.y].RoomIndex : -1;

                if (roomA == roomB) return;

                WallSample newWall = new WallSample(roomA, roomB, new Vector2Int(sampleA.x, sampleA.y), new Vector2Int(sampleB.x, sampleB.y), start, end);
                if (!walls.Add(newWall)) return;

                interior.WallSamples.Add(newWall);

                int wallIndex = interior.WallSamples.Count - 1;

                // add reference to the sample in the rooms
                if (roomA >= 0)
                    interior.Rooms[roomA].AddWallSample(wallIndex);

                if (roomB >= 0)
                    interior.Rooms[roomB].AddWallSample(wallIndex);

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
                    int roomA = x > -1 && z > -1 && interior.Grid[x, z] != null ? interior.Grid[x, z].RoomIndex : -1;

                    /** Grid Guide
                     *  tl-----tr    
                     *  |      |
                     *  |      |
                     *  bl-----br
                     */
                    Vector3 tl = interior.GridPointToWorldPoint(x, z + 1, false);
                    Vector3 br = interior.GridPointToWorldPoint(x + 1, z, false);
                    Vector3 tr = interior.GridPointToWorldPoint(x + 1, z + 1, false);

                    ParseWall(br, tr, roomA, new Vector2Int(x, z), new Vector2Int(x + 1, z));
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

            /*for (int i = 0; i < interior.Rooms.Count; i++)
            {
                string message = string.Format($"Room {i} : Connected Rooms {interior.Rooms[i].Neighbours.Count} |");
                foreach (KeyValuePair<int, List<int>> connection in interior.Rooms[i].Neighbours)
                    message = string.Format($"{message} [{connection.Key} : {connection.Value.Count} walls]");

                Debug.Log(message);
            }*/
        }

        /// <summary>
        /// Test if we can break relationship between 2 rooms
        /// </summary>
        /// <param name="inRooms">list holding the rooms</param>
        /// <param name="roomA">index of room 1</param>
        /// <param name="roomB">index of room 2</param>
        /// <returns></returns>
        private static bool CanBreakDirectPath(List<Room> inRooms, int roomA, int roomB)
        {
            if (inRooms == null || inRooms.Count == 0 || roomA < 0 || roomB < 0 ||
                roomA >= inRooms.Count || roomB >= inRooms.Count) return false;

            HashSet<int> visited = new HashSet<int>();
            Queue<int> queue = new Queue<int>();

            visited.Add(roomA);
            queue.Enqueue(roomA);

            while (queue.Count > 0)
            {
                int roomIndex = queue.Dequeue();
                Room room = inRooms[roomIndex];

                foreach (KeyValuePair<int, List<int>> n in room.Neighbours)
                {
                    // ignore direct connection between roomA and roomB
                    if (roomIndex == roomA && n.Key == roomB) continue;

                    // otherwise add unvisited room to the queue
                    if (visited.Add(n.Key))
                    {
                        // if we reached roomB, then prematurely end this and return true
                        if (n.Key == roomB) return true;

                        queue.Enqueue(n.Key);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Go through all Room's relations in an Interior and determine if it certain rooms can still
        /// be reached if it's connection is severed with another room.
        /// If yes, flip coins and determine whether to sever it or keep it
        /// </summary>
        /// <param name="interior"></param>
        public static void SampleDoor(Interior interior)
        {
            if (interior == null) return;

            for (int roomIndex = 0; roomIndex < interior.Rooms.Count; roomIndex++)
            {
                Room room = interior.Rooms[roomIndex];
                Dictionary<int, List<int>> neighbour = room.Neighbours;
                List<int> nextRooms = room.Neighbours.Keys.ToList();

                foreach (int nextIndex in nextRooms)
                {
                    // Check if we want to establish path with this room or break the path
                    bool canBreakPath = CanBreakDirectPath(interior.Rooms, roomIndex, nextIndex);
                    bool keepPath = (interior.InteriorRandom.Next(100) % 4) > 2;

                    if (canBreakPath && !keepPath)
                    {
                        // Remove both rooms from each others ConnectingWalls Dictionary
                        interior.Rooms[roomIndex].RemoveNeighbourForRoom(nextIndex);
                        interior.Rooms[nextIndex].RemoveNeighbourForRoom(roomIndex);
                        continue;
                    }

                    int wallIndex = SamplerHelperFunctions.GetRandomElement(neighbour[nextIndex], interior.InteriorRandom);

                    // flag the selected wall for a door
                    WallSample newDoor = interior.WallSamples[wallIndex];
                    newDoor.IsDoor = true;
                    interior.WallSamples[wallIndex] = newDoor;

                    // flag the samples on both sides of the door as reserved
                    interior.Grid[newDoor.RoomA.Value.x, newDoor.RoomA.Value.y].State = SampleState.RESERVED;
                    interior.Grid[newDoor.RoomB.Value.x, newDoor.RoomB.Value.y].State = SampleState.RESERVED;

                    // Remove both rooms from each others ConnectingWalls Dictionary
                    interior.Rooms[roomIndex].RemoveNeighbourForRoom(nextIndex);
                    interior.Rooms[nextIndex].RemoveNeighbourForRoom(roomIndex);

                    // Add reference to door
                    interior.Rooms[roomIndex].AddDoor(nextIndex, wallIndex);
                    interior.Rooms[roomIndex].AddDoor(roomIndex, wallIndex);
                }
            }
        }

    }
}