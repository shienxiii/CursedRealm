using UnityEngine;

namespace ProceduralInterior.Types
{
    // <summary>
    /// Holds 2 Vector2Int points.
    /// For use cases where we need to store 2 points for whatever reason.
    /// </summary>
    public struct Vector2Int_Range
    {
        private Vector2Int _a;
        private Vector2Int _b;

        public Vector2Int A => _a;
        public Vector2Int B => _b;

        public Vector2Int_Range(Vector2Int inA, Vector2Int inB)
        {
            _a = inA;
            _b = inB;
        }

        /// <summary>
        /// Get the XY dimension of this Vector2Int_Range
        /// </summary>
        /// <returns></returns>
        public Vector2Int GetDimension()
        {
            Vector2Int diff = _b - _a;
            return new Vector2Int(Mathf.Abs(diff.x) + 1, Mathf.Abs(diff.y) + 1);
        }

        public bool Contains(Vector2Int point)
        {
            return point.x >= Mathf.Min(_a.x, _b.x) &&
                   point.x <= Mathf.Max(_a.x, _b.x) &&
                   point.y >= Mathf.Min(_a.y, _b.y) &&
                   point.y <= Mathf.Max(_a.y, _b.y);
        }
    }

    /// <summary>
    /// Holds 2 Vector3 points.
    /// For use cases where we need to store 2 points for whatever reason.
    /// </summary>
    public struct Vector3_Range
    {
        private Vector3 _a;
        private Vector3 _b;

        public Vector3 A => _a;
        public Vector3 B => _b;

        public Vector3_Range(Vector3 inA, Vector3 inB)
        {
            _a = inA;
            _b = inB;
        }

        public float Distance()
        {
            return (_b - _a).magnitude;
        }
    }
}