using ProceduralInterior.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralInterior.Types
{
    public class WallCollection
    {
        // Need to store the room index to query the wall sample when checking wall's normal
        private int _roomIndex;
        private List<Vector3_Range> _walls;
        private Vector3 _direction;
        private Vector3 _normal;
        private float _linearity;

        public List<Vector3_Range> Walls => _walls;
        public Vector3 Direction => _direction;
        public Vector3 Normal => _normal;

        public WallCollection(WallSample inWallSample, in int inRoomIndex)
        {
            _roomIndex = inRoomIndex;
            _normal = inWallSample.GetWallNormal(_roomIndex);
            _direction = inWallSample.Direction;

            Vector3_Range firstWall = inWallSample.GetAsVectorRange(); ;

            _walls = new() { firstWall };

            _linearity = _direction == Vector3.right
                ? firstWall.A.z
                : firstWall.A.x;
        }

        private bool CanContain(WallSample sample)
        {
            if (sample.Direction != _direction)
                return false;

            Vector3 normal = sample.GetWallNormal(_roomIndex);

            if (Vector3.Dot(normal, _normal) < 0.9999f) return false;

            float linearity = _direction == Vector3.right
                ? sample.Start.z
                : sample.Start.x;

            return Mathf.Approximately(_linearity, linearity);
        }

        public bool TryAdd(WallSample sample)
        {
            if (!CanContain(sample)) return false;

            _walls.Add(sample.GetAsVectorRange());

            return true;
        }
    }
}