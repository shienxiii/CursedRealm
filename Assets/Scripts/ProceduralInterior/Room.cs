using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public Vector2Int start, end;
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
        start = _samples[0];
        end = _samples[0];

        for (int i = 1; i < _samples.Count; i++)
        {
            Vector2Int p = _samples[i];

            start.x = p.x < start.x ? p.x : start.x;
            start.y = p.y < start.y ? p.y : start.y;

            end.x = p.x > end.x ? p.x : end.x;
            end.y = p.y > end.y ? p.y : end.y;
        }

        size = new Vector2Int(end.x - start.x + 1, end.y - start.y + 1);

        roomArea = new bool[size.x, size.y];

        foreach (Vector2Int p in samples)
        {
            roomArea[p.x - start.x, p.y - start.y] = true;
        }
    }

    public void AppendSamples(List<Vector2Int> inSamples)
    {
        _samples.AddRange(inSamples);
    }
}
