using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a collection of sample point in a Room.
/// Used to reduce memory consumption of sample information and reduce number of floor mesh to spawn.
/// </summary>
public struct Space
{
    public Vector2Int StartPoint;
    public Vector2Int Size;
}

public class Room
{
    // samples occupied by this room
    private List<Vector2Int> _samples;
    private List<Room> _connectedRooms;
    public List<Vector2Int> Samples => _samples;
    public List<Room> ConnectedRooms => _connectedRooms;


    public Color debugColor;
    public Color debugColor_a;

    public bool[,] roomArea;
    public Vector2Int start;
    public Vector2Int size;

    public Room()
    {
        System.Random rand = new System.Random();
        debugColor = new Color(((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255);
        debugColor_a = debugColor;
        debugColor_a.a = 0.5f;
        Debug.Log("Room default construct");
    }

    public Room(List<Vector2Int> samples)
    {
        _samples = samples;

        System.Random rand = new System.Random();
        debugColor = new Color(((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255);
        debugColor_a = debugColor;
        debugColor_a.a = 0.5f;


        if (_samples.Count == 0) return;

        // Generate Space data
        Vector2Int x0y0 = _samples[0], x1y1 = _samples[0];

        for (int i = 1; i < _samples.Count; i++)
        {
            Vector2Int p = _samples[i];

            x0y0.x = p.x < x0y0.x ? p.x : x0y0.x;
            x0y0.y = p.y < x0y0.y ? p.y : x0y0.y;

            x1y1.x = p.x > x1y1.x ? p.x : x1y1.x;
            x1y1.y = p.y > x1y1.y ? p.y : x1y1.y;
        }

        size = new Vector2Int(x1y1.x - x0y0.x + 1, x1y1.y - x0y0.y + 1);

        roomArea = new bool[size.x, size.y];

        foreach (Vector2Int p in samples)
        {
            roomArea[p.x - x0y0.x, p.y - x0y0.y] = true;
        }

        start = x0y0;
    }

    public void AppendSamples(List<Vector2Int> inSamples)
    {
        _samples.AddRange(inSamples);
    }
}
