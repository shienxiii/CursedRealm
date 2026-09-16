using System;
using System.Collections.Generic;
using ProceduralInterior.Types;
using UnityEngine;

/// <summary>
/// This class hold a single room in an Interior.
/// The values in this class all refers to an index in the owning Interior
/// </summary>
[Serializable]
public class Room
{
    // starting and ending grid point of the rectangle making up this room
    private Vector2Int_Range _range;

    /// A list of rectangular spaces making up this Room.
    /// Each rectangles are a span continuous samples making up this room.
    private List<Vector2Int_Range> _spaces = new List<Vector2Int_Range>();

    // walls making up this room, values are index to Interior._wallSamples in owning Interior
    private List<int> _wallSamples = new List<int>();

    // Actual wall information specific to this Room after merging and processing everything in _wallSamples
    private List<Wall> _walls = new List<Wall>();
    
    private Dictionary<int, int> _doors = new Dictionary<int, int>();

    public List<int> WallSamples => _wallSamples;
    public Vector2Int Start => _range.A;
    public Vector2Int End => _range.B;
    public Vector2Int Dimension => _range.GetDimension();
    public List<Vector2Int_Range> Spaces => _spaces;

    public List<Wall> Walls => _walls;
    
    /// <summary>
    // Doors to the another room
    // Key : Room index
    // Value : Wall index of the door
    /// </summary>
    public Dictionary<int, int> Doors => _doors;


    // Temporary Dictionary to the list of walls that can be converted to a door to another room
    // Key: Next room index | Value: Walls separating this room from the next room
    // NOTE: Clear after assigning door
    private Dictionary<int, List<int>> _neighbours = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> Neighbours => _neighbours;

    public Color debugColor;

    public Room(List<Vector2Int> samples)
    {
        System.Random rand = new System.Random();
        debugColor = new Color(((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255);

        if (samples.Count == 0)
        {
            _range = new Vector2Int_Range();
            return;
        }

        // Generate Space data
        Vector2Int start = samples[0];
        Vector2Int end = samples[0];

        for (int i = 1; i < samples.Count; i++)
        {
            Vector2Int p = samples[i];

            start.x = p.x < start.x ? p.x : start.x;
            start.y = p.y < start.y ? p.y : start.y;

            end.x = p.x > end.x ? p.x : end.x;
            end.y = p.y > end.y ? p.y : end.y;
        }

        _range = new Vector2Int_Range(start, end);
    }

    public void AddNeighbourForRoom(int connectingRoom, int wallIndex)
    {
        if(!_neighbours.ContainsKey(connectingRoom))
            _neighbours.Add(connectingRoom, new List<int> { wallIndex });
        else
            _neighbours[connectingRoom].Add(wallIndex);
    }


    public void RemoveNeighbourForRoom(int connectingRoom)
    {
        _neighbours.Remove(connectingRoom);
    }

    public void AddWallSample(int wallIndex)
    {
        _wallSamples.Add(wallIndex);
    }

    public bool AddDoor(int nextRoomIndex, int wallIndex)
    {
        if (_doors.ContainsKey(nextRoomIndex)) return false;

        _doors.Add(nextRoomIndex, wallIndex);
        return true;
    }
}