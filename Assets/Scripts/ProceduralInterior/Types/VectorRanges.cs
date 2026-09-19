using UnityEngine;

namespace ProceduralInterior.Types
{
    // <summary>
    /// Holds 2 Vector2Int points which represents a grid's span
    /// </summary>
    public struct GridSpan
    {
        public Vector2Int A;
        public Vector2Int B;

        public GridSpan(Vector2Int init)
        {
            A = B = init;
        }
        public GridSpan(Vector2Int inA, Vector2Int inB)
        {
            // ensure x and y in A is always smaller than B

            A = inA;
            B = inB;

            if (A.x > B.x)
            {
                int aX = B.x;
                B.x = A.x;
                A.x = aX;
            }

            if (A.y > B.y)
            {
                int aY = B.y;
                B.y = A.y;
                A.y = aY;
            }
        }

        /// <summary>
        /// Get the XY dimension of this Vector2Int_Range
        /// </summary>
        /// <returns></returns>
        public Vector2Int GetDimension()
        {
            if (A == null || B == null) return Vector2Int.zero;

            Vector2Int diff = B - A;
            return new Vector2Int(Mathf.Abs(diff.x) + 1, Mathf.Abs(diff.y) + 1);
        }

        public int GetArea()
        {
            Vector2Int dim = GetDimension();
            return dim.x * dim.y;
        }

        public bool Contains(Vector2Int point)
        {
            return point.x >= Mathf.Min(A.x, B.x) &&
                   point.x <= Mathf.Max(A.x, B.x) &&
                   point.y >= Mathf.Min(A.y, B.y) &&
                   point.y <= Mathf.Max(A.y, B.y);
        }
    }

    /// <summary>
    /// Holds 2 Vector3 points.
    /// For use cases where we need to store 2 points for whatever reason.
    /// </summary>
    public struct Vector3_Range
    {
        public Vector3 A;
        public Vector3 B;

        public Vector3_Range(Vector3 init)
        {
            A = B = init;
        }

        public Vector3_Range(Vector3 inA, Vector3 inB)
        {
            A = inA;
            B = inB;
        }

        public float Distance()
        {
            return (B - A).magnitude;
        }
    }
}