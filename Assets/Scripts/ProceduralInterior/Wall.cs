using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Holds information of a stretch of wall surrounding a Room.
/// To be owned by a Room.
/// </summary>
public struct Wall
{
    private Vector3 _start;
    private Vector3 _end;
    private Vector3 _direction;
    private bool _isDoor;

    public Vector3 Start => _start;
    public Vector3 End => _end;
    public Vector3 Direction => _direction;
    public bool IsDoor => _isDoor;

    public Wall(Vector3 inStart, Vector3 inEnd, bool isDoor)
    {
        // we want Start to hold the lower X or if they're approximately the same, the lower z
        if (inStart.x < inEnd.x && !Mathf.Approximately(inStart.x, inEnd.x) ||
            inStart.z < inEnd.z && !Mathf.Approximately(inStart.z, inEnd.z))
        {
            _start = inStart;
            _end = inEnd;
        }
        else
        {
            _start = inEnd;
            _end = inStart;
        }

        // direction will always be either (1,0,0) or (0,0,1)
        _direction = _end - _start;
        _direction.Normalize();

        _isDoor = isDoor;

    }

    public Wall(in WallSample inWallSample)
    {
        _start = inWallSample.Start;
        _end = inWallSample.End;
        _direction = inWallSample.Direction;
        _isDoor =inWallSample.IsDoor;
    }
}
