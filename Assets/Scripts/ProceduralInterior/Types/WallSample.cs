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
    public struct WallSample
    {
        private KeyValuePair<int, Vector2Int> _roomA;
        private KeyValuePair<int, Vector2Int> _roomB;
        private Vector3 _start;
        private Vector3 _end;
        private Vector3 _direction;
        // Flag for if this wall is a door
        public bool IsDoor;


        /*** PUBLIC PROPERTIES ***/
        public KeyValuePair<int, Vector2Int> RoomA => _roomA;
        public KeyValuePair<int, Vector2Int> RoomB => _roomB;
        // We want to know the wall span
        public Vector3 Start => _start;
        public Vector3 End => _end;

        // Direction of wall stretch, for the sake of simplicy,
        // direction will always be either (1,0,0) or (0,0,1)
        public Vector3 Direction => _direction;



        public WallSample(int inRoomA, int inRoomB,
                    Vector2Int inSampleA, Vector2Int inSampleB,
                    Vector3 inStart, Vector3 inEnd)
        {
            // we want to keep reference both reference even if one of them is -1
            _roomA = new KeyValuePair<int, Vector2Int>(inRoomA, inSampleA);
            _roomB = new KeyValuePair<int, Vector2Int>(inRoomB, inSampleB);

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

            IsDoor = false;
        }

        public bool Equals(WallSample inWall)
        {
            return this == inWall;
        }

        public override bool Equals(object obj)
        {
            return obj is WallSample && Equals((WallSample)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        public override string ToString()
        {
            return String.Format($"Wall | {_roomA.Key} : {_roomB.Key} | {Start}-{End} | DIR : {Direction}");
        }

        public static bool operator ==(WallSample wallA, WallSample wallB)
        {
            // check that the wall span are start and end are approximately the same location
            /*return (CustomVectorMath.EqualWithTolerance(wallA.Start, wallB.Start) && CustomVectorMath.EqualWithTolerance(wallA.End, wallB.End)) ||
                (CustomVectorMath.EqualWithTolerance(wallA.Start, wallB.End) && CustomVectorMath.EqualWithTolerance(wallA.End, wallB.Start));*/

            return (wallA.Start == wallB.Start && wallA.End == wallB.End) ||
                (wallA.Start == wallB.End && wallA.End == wallB.Start);
        }

        public static bool operator !=(WallSample wallA, WallSample wallB)
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
            if (inRoomIndex != _roomA.Key && inRoomIndex != _roomB.Key) return Vector3.zero;

            Vector3 a = new Vector3(_roomA.Value.x, 0.0f, _roomA.Value.y);
            Vector3 b = new Vector3(_roomB.Value.x, 0.0f, _roomB.Value.y);

            if (_roomA.Key == inRoomIndex)
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