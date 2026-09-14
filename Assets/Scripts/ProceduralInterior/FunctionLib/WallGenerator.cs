using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WallCollection
{
    private int _roomIndex;
    private List<Wall> _walls;
    private Vector3 _direction;
    private float _linearity;
    private Vector3 _normal;

    public List<Wall> Walls => _walls;
    public Vector3 Direction => _direction;
    public Vector3 Normal => _normal;

    public WallCollection(WallSample inWallSample, in int inRoomIndex)
    {
        _roomIndex = inRoomIndex;
        _normal = inWallSample.GetWallNormal(_roomIndex);
        _direction = inWallSample.Direction;

        Wall firstWall = new Wall(inWallSample, _normal);

        _walls = new() { firstWall };

        _linearity = _direction == Vector3.right
            ? firstWall.Start.z
            : firstWall.Start.x;
    }

    private bool CanContain(WallSample sample)
    {
        if (sample.Direction != _direction)
            return false;

        Vector3 normal = sample.GetWallNormal(_roomIndex);

        if (Vector3.Dot(normal, _normal) < 0.9999f) return false;

        float linearity = _direction == Vector3.right
            ? sample.Start.z
            : sample.Start.x;

        return Mathf.Approximately(_linearity, linearity);
    }

    public bool TryAdd(WallSample sample)
    {
        if (!CanContain(sample)) return false;

        _walls.Add(new Wall(sample, _normal));

        return true;
    }
}

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
        // Final result after combining all wall, to be initialized in SortWallByDirectionAndLinearity()
        List<Wall> finalWalls = new List<Wall>();

        List<WallCollection> wallCollections = SortWallByDirectionAndLinearity(inInterior.WallSamples, roomIndex, room.WallSamples, ref finalWalls);

        foreach (WallCollection wallCollection in wallCollections)
            MergeWalls(wallCollection, ref finalWalls);

        room.SetWalls(finalWalls);
        Debug.Log("Finish Generate Wall For Room " + finalWalls.Count);
    }

    /// <summary>
    /// Sorts out the WallSamples into 
    /// </summary>
    /// <param name="inSamples"></param>
    /// <param name="inWallIndexes"></param>
    private static List<WallCollection> SortWallByDirectionAndLinearity(in List<WallSample> inSamples, in int inRoomIndex, in List<int> inWallIndexes, ref List<Wall> outDoors)
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

    public static void MergeWalls(WallCollection inWallCollection, ref List<Wall> outWalls)
    {
        // Sort the wall in order
        SortWalls(inWallCollection);

        List<Wall> walls = inWallCollection.Walls;

        // And generate the final wall
        int start = 0;
        int end = 0;

        for(int i = 1; i < walls.Count; i++)
        {
            if (!CustomVectorMath.EqualWithTolerance(walls[end].End, walls[i].Start))
            {
                // if not continuous, create a new wall with current start and end
                outWalls.Add(new Wall(walls[start].Start, walls[end].End, inWallCollection.Normal, false));

                // reassign start
                start = i;
            }

            // update end to latest i
            end = i;
        }

        outWalls.Add(new Wall(walls[start].Start, walls[end].End, inWallCollection.Normal, false));
    }

    public static void SortWalls(WallCollection inWallCollections)
    {
        // sort the walls so they're in ascending order based on their direction
        if(inWallCollections.Direction == Vector3.right)
            inWallCollections.Walls.Sort((a, b) => (a.Start.x.CompareTo(b.Start.x)));
        else
            inWallCollections.Walls.Sort((a, b) => (a.Start.z.CompareTo(b.Start.z)));
    }
}
