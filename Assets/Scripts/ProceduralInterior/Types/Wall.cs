using UnityEngine;


namespace ProceduralInterior.Types
{
    /// <summary>
    /// Holds information of a stretch of wall surrounding a Room.
    /// To be owned by a Room.
    /// </summary>
    public struct Wall
    {
        public Vector3 Start;
        public Vector3 End;
        public Vector3 Normal;
        public readonly bool IsDoor;

        public Wall(Vector3 inStart, Vector3 inEnd, Vector3 inNormal, bool isDoor)
        {
            // we want Start to hold the lower X or if they're approximately the same, the lower z
            if (inStart.x < inEnd.x && !Mathf.Approximately(inStart.x, inEnd.x) ||
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

            Normal = inNormal;
            IsDoor = isDoor;
            
            
            // to ensure we can sort the wall based on start and end point,
            // inverse _start and _end if normal points to left or forward
            if (CustomVectorMath.EqualWithTolerance(Normal, Vector3.forward) ||
                CustomVectorMath.EqualWithTolerance(Normal, Vector3.left))
                (Start, End) = (End, Start);
        }

        public Wall(in WallSample inWallSample, Vector3 inNormal)
        {
            Start = inWallSample.Start;
            End = inWallSample.End;
            Normal = inNormal;
            IsDoor = inWallSample.IsDoor;
            
            // to ensure we can sort the wall based on start and end point,
            // inverse _start and _end if normal points to left or forward
            if (CustomVectorMath.EqualWithTolerance(Normal, Vector3.forward) ||
                CustomVectorMath.EqualWithTolerance(Normal, Vector3.left))
                (Start, End) = (End, Start);
        }

        public void OffsetStart(Vector3 direction, float distance)
        {
            Start += (direction * distance);
        }
        
        public void OffsetEnd(Vector3 direction, float distance)
        {
            End += (direction * distance);
        }
        
        // Size length of wall from start to end
        public float GetLength()
        {
            return (End - Start).magnitude;
        }

        // Center point of the wall, with Y-axis at floor level
        public Vector3 GetCenter()
        {
            return (Start + End) / 2;
        }
    }
}