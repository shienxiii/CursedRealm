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
    // samples occupied by this room, values are index to Interior._samples in owning Interior
    private List<Vector2Int> _samples;
    // walls making up this room, values are index to Interior._walls in owning Interior
    private List<int> _walls;
    // starting and ending grid point of the rectangle making up this room
    private Vector2Int _start, _end;
    private Vector2Int _size;

    /// <summary>
    // Doors to the another room
    // Key : Room index
    // Value : Wall index of the door
    /// </summary>
    private Dictionary<int, int> _doors = new Dictionary<int, int>();
    public Dictionary<int, int> Doors => _doors;

    public List<Vector2Int> Samples => _samples;
    public Vector2Int Start => _start;
    public Vector2Int End => _end;

    // Temporary Dictionary to the list of walls that can be converted to a door to another room
    // Key: Next room index | Value: Walls separating this room from the next room
    // NOTE: Clear after assigning door
    private Dictionary<int, List<int>> _doorCandidates = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> DoorCandidates => _doorCandidates;

    public Color debugColor;

    public Room(List<Vector2Int> samples)
    {
        _samples = samples;
        _walls = new List<int>();

        System.Random rand = new System.Random();
        debugColor = new Color(((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255);

        if (_samples.Count == 0)
        {
            _start = _end = _size = Vector2Int.zero;
            return;
        }

        // Generate Space data
        _start = _samples[0];
        _end = _samples[0];

        for (int i = 1; i < _samples.Count; i++)
        {
            Vector2Int p = _samples[i];

            _start.x = p.x < _start.x ? p.x : _start.x;
            _start.y = p.y < _start.y ? p.y : _start.y;

            _end.x = p.x > _end.x ? p.x : _end.x;
            _end.y = p.y > _end.y ? p.y : _end.y;
        }

        _size = new Vector2Int(_end.x - _start.x + 1, _end.y - _start.y + 1);
    }

    public void AddDoorCandidateForRoom(int connectingRoom, int wallIndex)
    {
        if(!_doorCandidates.ContainsKey(connectingRoom))
            _doorCandidates.Add(connectingRoom, new List<int> { wallIndex });
        else
            _doorCandidates[connectingRoom].Add(wallIndex);
    }


    public void RemoveDoorCandidatesForRoom(int connectingRoom)
    {
        _doorCandidates.Remove(connectingRoom);
    }

    public void AddWall(int wallIndex)
    {
        _walls.Add(wallIndex);
    }

    public bool AddDoor(int nextRoomIndex, int wallIndex)
    {
        if (_doors.ContainsKey(nextRoomIndex)) return false;

        _doors.Add(nextRoomIndex, wallIndex);
        return true;
    }
}