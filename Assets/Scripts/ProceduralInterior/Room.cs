using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room
{
    // samples occupied by this room
    private List<Vector2Int> _samples;
    private Vector2Int _start, _end; // starting and ending grid point of the rectangle making up this room
    private Vector2Int _size;

    // Key: Next room index | Value: Walls separating this room from the next room
    private Dictionary<int, List<int>> _next = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> Next => _next;

    public List<Vector2Int> Samples => _samples;
    public Vector2Int Start => _start;
    public Vector2Int End => _end;

    public Color debugColor;

    public Room(List<Vector2Int> samples)
    {
        _samples = samples;

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

    public void AddConnectingRoom(int connectingRoom, int wallIndex)
    {
        if(!_next.ContainsKey(connectingRoom))
            _next.Add(connectingRoom, new List<int> { wallIndex });
        else
            _next[connectingRoom].Add(wallIndex);
    }
}