using UnityEngine;


namespace ProceduralInterior.Types
{
    /// <summary>
    /// Holds information of a stretch of wall surrounding a Room.
    /// To be owned by a Room.
    /// </summary>
    public struct Wall
    {
        private Vector3 _start;
        private Vector3 _end;
        private Vector3 _direction;
        private Vector3 _normal;
        private bool _isDoor;

        public Vector3 Start => _start;
        public Vector3 End => _end;
        public Vector3 Direction => _direction;
        public Vector3 Normal => _normal;
        public bool IsDoor => _isDoor;

        public Wall(Vector3 inStart, Vector3 inEnd, Vector3 inNormal, bool isDoor)
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

            _normal = inNormal;
            _isDoor = isDoor;
            
            
            // to ensure we can sort the wall based on start and end point,
            // inverse _start and _end if normal points to left or forward
            if (CustomVectorMath.EqualWithTolerance(_normal, Vector3.forward) ||
                CustomVectorMath.EqualWithTolerance(_normal, Vector3.left))
                (_start, _end) = (_end, _start);


            _direction = _end - _start;
            _direction.Normalize();
        }

        public Wall(in WallSample inWallSample, Vector3 inNormal)
        {
            _start = inWallSample.Start;
            _end = inWallSample.End;
            _normal = inNormal;
            _isDoor = inWallSample.IsDoor;
            
            // to ensure we can sort the wall based on start and end point,
            // inverse _start and _end if normal points to left or forward
            if (CustomVectorMath.EqualWithTolerance(_normal, Vector3.forward) ||
                CustomVectorMath.EqualWithTolerance(_normal, Vector3.left))
                (_start, _end) = (_end, _start);
            
            _direction = _end - _start;
            _direction.Normalize();
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