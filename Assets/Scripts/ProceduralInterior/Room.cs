using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class hold a single room in an Interior.
/// The values in this class all refers to an index in the owning Interior
/// </summary>
[Serializable]
public class Room
{
    // walls making up this room, values are index to Interior._wallSamples in owning Interior
    private List<int> _wallSamples;
    // starting and ending grid point of the rectangle making up this room
    private Vector2Int _start, _end;

    /// <summary>
    // Doors to the another room
    // Key : Room index
    // Value : Wall index of the door
    /// </summary>
    private Dictionary<int, int> _doors = new Dictionary<int, int>();

    public List<int> WallSamples => _wallSamples;
    public Vector2Int Start => _start;
    public Vector2Int End => _end;
    public Dictionary<int, int> Doors => _doors;


    // Temporary Dictionary to the list of walls that can be converted to a door to another room
    // Key: Next room index | Value: Walls separating this room from the next room
    // NOTE: Clear after assigning door
    private Dictionary<int, List<int>> _neighbours = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> Neighbours => _neighbours;

    public Color debugColor;

    public Room(List<Vector2Int> samples)
    {
        _wallSamples = new List<int>();

        System.Random rand = new System.Random();
        debugColor = new Color(((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255);

        if (samples.Count == 0)
        {
            _start = _end = Vector2Int.zero;
            return;
        }

        // Generate Space data
        _start = samples[0];
        _end = samples[0];

        for (int i = 1; i < samples.Count; i++)
        {
            Vector2Int p = samples[i];

            _start.x = p.x < _start.x ? p.x : _start.x;
            _start.y = p.y < _start.y ? p.y : _start.y;

            _end.x = p.x > _end.x ? p.x : _end.x;
            _end.y = p.y > _end.y ? p.y : _end.y;
        }
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

    public void AddWall(int wallIndex)
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