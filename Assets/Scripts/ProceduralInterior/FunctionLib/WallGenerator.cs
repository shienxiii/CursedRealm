using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WallCollection
{
    private List<Wall> _walls;
    private Vector3 _direction;
    private float _linearity;

    public List<Wall> Walls => _walls;
    public Vector3 Direction => _direction;

    public WallCollection(WallSample inWallSample)
    {
        Wall firstWall = new Wall(inWallSample);

        _walls = new() { firstWall };

        _direction = inWallSample.Direction;

        _linearity = _direction == Vector3.right
            ? firstWall.Start.z
            : firstWall.Start.x;
    }

    private bool CanContain(WallSample sample)
    {
        if (sample.Direction != _direction)
            return false;

        float linearity = _direction == Vector3.right
            ? sample.Start.z
            : sample.Start.x;

        return Mathf.Approximately(_linearity, linearity);
    }

    public bool TryAdd(WallSample sample)
    {
        if (!CanContain(sample)) return false;

        _walls.Add(new Wall(sample));

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
    public static void GenerateWallForRoom(Interior inInterior, Room inRoom)
    {
        // inInterior must own the room
        if (!inInterior.Rooms.Contains(inRoom)) return;

        // Final result after combining all wall, to be initialized in SortWallByDirectionAndLinearity()
        List<Wall> finalWalls;

        List<WallCollection> wallCollections = SortWallByDirectionAndLinearity(inInterior.WallSamples, inRoom.WallSamples, out finalWalls);


    }

    /// <summary>
    /// Sorts out the WallSamples into 
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="wallIndexes"></param>
    private static List<WallCollection> SortWallByDirectionAndLinearity(in List<WallSample> samples, in List<int> wallIndexes, out List<Wall> outDoors)
    {
        outDoors = new List<Wall>();

        List<WallCollection> walls = new List<WallCollection>();

        foreach (int wallIndex in wallIndexes)
        {
            WallSample sample = samples[wallIndex];

            // if the WallSample is for a door, add to outDooe and move to next
            if(sample.IsDoor)
            {
                outDoors.Add(new Wall(sample));
                continue;
            }

            // Sort into a WallCollection
            bool wallAdded = false;

            // parse against every existing WallCollection
            for (int i = 0; i < walls.Count && !wallAdded; i++)
                wallAdded = walls[i].TryAdd(sample);

            if(!wallAdded)
                walls.Add(new WallCollection(sample));
        }

        return walls;
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
