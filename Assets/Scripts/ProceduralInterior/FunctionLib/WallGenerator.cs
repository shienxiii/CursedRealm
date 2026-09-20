using System.Collections.Generic;
using ProceduralInterior.Types;
using UnityEngine;

public static class WallGenerator
{
    /// <summary>
    /// Generate a list of Wall for a room based on the WallSample stored in the owning Interior
    /// </summary>
    /// <param name="inInterior">The Interior that owns the room</param>
    /// <param name="inRoom">The room which walls to be generated</param>
    public static void GenerateWallForRoom(Interior inInterior, int roomIndex)
    {
        // null check and index check
        if(inInterior == null || roomIndex < 0 || roomIndex >= inInterior.Rooms.Count) return;

        Room room = inInterior.Rooms[roomIndex];

        List<WallCollection> wallCollections = SortWallByDirectionAndLinearity(inInterior.WallSamples, roomIndex, room.WallSamples, room.Walls);

        foreach (WallCollection wallCollection in wallCollections)
            MergeWalls(wallCollection, room.Walls);
    }

    /// <summary>
    /// Sorts out the WallSamples into 
    /// </summary>
    /// <param name="inSamples"></param>
    /// <param name="inWallIndexes"></param>
    private static List<WallCollection> SortWallByDirectionAndLinearity(in List<WallSample> inSamples, in int inRoomIndex, in List<int> inWallIndexes, List<Wall> outDoors)
    {
        List<WallCollection> walls = new List<WallCollection>();

        foreach (int wallIndex in inWallIndexes)
        {
            WallSample sample = inSamples[wallIndex];

            // if the WallSample is for a door, add to outDooe and move to next
            if(sample.IsDoor)
            {
                outDoors.Add(new Wall(sample, sample.GetWallNormal(inRoomIndex)));
                continue;
            }

            // Sort into a WallCollection
            bool wallAdded = false;

            // parse against every existing WallCollection
            for (int i = 0; i < walls.Count && !wallAdded; i++)
                wallAdded = walls[i].TryAdd(sample);

            if(!wallAdded)
                walls.Add(new WallCollection(sample, inRoomIndex));
        }
        return walls;
    }

    public static void MergeWalls(WallCollection inWallCollection, List<Wall> outWalls)
    {
        // Sort the wall in order
        SortWalls(inWallCollection);

        List<Vector3Range> walls = inWallCollection.Walls;

        // And generate the final wall
        int start = 0;
        int end = 0;

        for(int i = 1; i < walls.Count; i++)
        {
            if (!CustomVectorMath.EqualWithTolerance(walls[end].B, walls[i].A))
            {
                // if not continuous, create a new wall with current start and end
                outWalls.Add(new Wall(walls[start].A, walls[end].B, inWallCollection.Normal, false));

                // reassign start
                start = i;
            }

            // update end to latest i
            end = i;
        }

        outWalls.Add(new Wall(walls[start].A, walls[end].B, inWallCollection.Normal, false));
    }

    public static void SortWalls(WallCollection inWallCollections)
    {
        // sort the walls so they're in ascending order based on their direction
        if(inWallCollections.Direction == Vector3.right)
            inWallCollections.Walls.Sort((a, b) => (a.A.x.CompareTo(b.A.x)));
        else
            inWallCollections.Walls.Sort((a, b) => (a.B.z.CompareTo(b.B.z)));
    }
}
