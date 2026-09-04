using System;
using UnityEngine;

[Serializable]
public struct Wall
{
    public int RoomA;
    public int RoomB;

    // We want to know the wall span
    public Vector3 Start;
    public Vector3 End;

    public bool HasDoor;

    public Wall(int inRoomA, int inRoomB,
                Vector3 inStart, Vector3 inEnd)
    {
        RoomA = inRoomA;
        RoomB = inRoomB;
        
        // we want Start to hold the lower X or if they're approximately the same, the lower z
        if(inStart.x < inEnd.x && !Mathf.Approximately(inStart.x, inEnd.x) ||
            inStart.z < inEnd.z && !Mathf.Approximately(inStart.z, inEnd.z))
        {
            Start = inStart;
            End = inEnd;
        }
        else
        {
            Start = inEnd;
            End = inStart;
        }    
        
        HasDoor = false;
    }

    public readonly bool Equals(Wall inWall)
    {
        return this == inWall;
    }

    public override bool Equals(object obj)
    {
        return obj is Wall && Equals((Wall)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public override readonly string ToString()
    {
        return String.Format($"Wall | {RoomA}:{RoomB} | {Start}-{End}");
    }

    public static bool operator ==(Wall wallA, Wall wallB)
    {
        // check that the wall span are start and end are approximately the same location
        /*return (CustomVectorMath.EqualWithTolerance(wallA.Start, wallB.Start) && CustomVectorMath.EqualWithTolerance(wallA.End, wallB.End)) ||
            (CustomVectorMath.EqualWithTolerance(wallA.Start, wallB.End) && CustomVectorMath.EqualWithTolerance(wallA.End, wallB.Start));*/

        return (wallA.Start == wallB.Start && wallA.End == wallB.End) ||
            (wallA.Start == wallB.End && wallA.End == wallB.Start);
    }

    public static bool operator !=(Wall wallA, Wall wallB)
    {
        return !(wallA == wallB);
    }
}