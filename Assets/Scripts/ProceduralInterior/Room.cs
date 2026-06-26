using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room
{
    // samples occupied by this room
    private List<Vector2Int> _samples;
    private List<Room> _rooms;
    public List<Vector2Int> Samples => _samples;
    public List<Room> Rooms => _rooms;


    public Color debugColor;

    public Room()
    {
        System.Random rand = new System.Random();
        debugColor = new Color(((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255);
    }

    public Room(List<Vector2Int> samples)
    {
        _samples = samples;

        System.Random rand = new System.Random();
        debugColor = new Color(((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255, ((float)rand.Next(255)) / 255);
    }

    public void AppendSamples(List<Vector2Int> inSamples)
    {
        _samples.AddRange(inSamples);
    }
}
