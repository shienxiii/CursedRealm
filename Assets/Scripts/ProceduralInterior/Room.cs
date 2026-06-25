using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room
{
    // samples occupied by this room
    private List<Vector2Int> _samples;
    public List<Vector2Int> Samples => _samples;

    public Room() { }

    public Room(List<Vector2Int> samples)
    {
        _samples = samples;
    }

    public void AppendSamples(List<Vector2Int> inSamples)
    {
        _samples.AddRange(inSamples);
    }
}
