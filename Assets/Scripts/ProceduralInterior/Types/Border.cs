using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralInterior.Types
{
    /// <summary>
    /// This class hold information of a wall segment of an Interior.
    /// The values in this class all in the context of the owning Interior
    /// </summary>
    [Serializable]
    public struct Border
    {
        public KeyValuePair<int, Vector2Int> RoomA;
        public KeyValuePair<int, Vector2Int> RoomB;
        private Vector3Range _endPoints;
        private Vector3 _direction;
        // Flag for if this border is a door
        public bool IsDoor;


        /*** PUBLIC PROPERTIES ***/
        /*public KeyValuePair<int, Vector2Int> RoomA => _roomA;
        public KeyValuePair<int, Vector2Int> RoomB => _roomB;*/
        // We want to know the sample span
        public Vector3 Start => _endPoints.A;
        public Vector3 End => _endPoints.B;

        // Direction of wall stretch, for the sake of simplicy,
        // direction will always be either (1,0,0) or (0,0,1)
        public Vector3 Direction => _direction;

        public Border(int inRoomA, int inRoomB,
                    Vector2Int inSampleA, Vector2Int inSampleB,
                    Vector3 inStart, Vector3 inEnd)
        {
            // we want to keep reference both reference even if one of them is -1
            RoomA = new KeyValuePair<int, Vector2Int>(inRoomA, inSampleA);
            RoomB = new KeyValuePair<int, Vector2Int>(inRoomB, inSampleB);

            // we want Start to hold the lower X or if they're approximately the same, the lower z
            if (inStart.x < inEnd.x && !Mathf.Approximately(inStart.x, inEnd.x) ||
                inStart.z < inEnd.z && !Mathf.Approximately(inStart.z, inEnd.z))
            {
                _endPoints.A = inStart;
                _endPoints.B = inEnd;
            }
            else
            {
                _endPoints.A = inEnd;
                _endPoints.B = inStart;
            }


            // direction will always be either (1,0,0) or (0,0,1)
            _direction = _endPoints.B - _endPoints.A;
            _direction.Normalize();

            IsDoor = false;
        }

        public bool Equals(Border inWall)
        {
            return this == inWall;
        }

        public override bool Equals(object obj)
        {
            return obj is Border && Equals((Border)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        public override string ToString()
        {
            return String.Format($"Wall | {RoomA.Key} : {RoomB.Key} | {Start}-{End} | DIR : {Direction}");
        }

        public static bool operator ==(Border wallA, Border wallB)
        {
            // check that the wall span are start and end are approximately the same location
            /*return (CustomVectorMath.EqualWithTolerance(wallA.Start, wallB.Start) && CustomVectorMath.EqualWithTolerance(wallA.End, wallB.End)) ||
                (CustomVectorMath.EqualWithTolerance(wallA.Start, wallB.End) && CustomVectorMath.EqualWithTolerance(wallA.End, wallB.Start));*/

            return (wallA.Start == wallB.Start && wallA.End == wallB.End) ||
                (wallA.Start == wallB.End && wallA.End == wallB.Start);
        }

        public static bool operator !=(Border wallA, Border wallB)
        {
            return !(wallA == wallB);
        }

        /// <summary>
        /// Evaluate RoomA and RoomB and return the wall normal, the direction that points toward the room
        /// </summary>
        /// <param name="inRoomIndex"></param>
        /// <returns></returns>
        public Vector3 GetWallNormal(int inRoomIndex)
        {
            if (inRoomIndex != RoomA.Key && inRoomIndex != RoomB.Key) return Vector3.zero;

            Vector3 a = new Vector3(RoomA.Value.x, 0.0f, RoomA.Value.y);
            Vector3 b = new Vector3(RoomB.Value.x, 0.0f, RoomB.Value.y);

            if (RoomA.Key == inRoomIndex)
                return (a - b).normalized;

            return (b - a).normalized;
        }


        /// <summary>
        /// Return this wall sample's information as a Vector3_Range,
        /// where A == Start and B == End
        /// </summary>
        /// <returns></returns>
        public Vector3Range GetAsVectorRange()
        {
            return new Vector3Range(Start, End);
        }
    }

    

}