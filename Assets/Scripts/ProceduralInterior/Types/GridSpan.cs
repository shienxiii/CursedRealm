using UnityEngine;

namespace ProceduralInterior.Types
{
    // <summary>
    /// Holds 2 Vector2Int points which represents a grid's span.
    /// Constructor will always ensure both x and y of vector A to be smaller than vector B
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
                (B.x, A.x) = (A.x, B.x);

            if (A.y > B.y)
                (B.y, A.y) = (A.y, B.y);
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
}