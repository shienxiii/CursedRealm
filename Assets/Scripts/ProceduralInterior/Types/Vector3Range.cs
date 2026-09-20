using UnityEngine;

namespace ProceduralInterior.Types
{
    /// <summary>
    /// Holds 2 Vector3 points.
    /// For use cases where we need to store 2 points for whatever reason.
    /// </summary>
    public struct Vector3Range
    {
        public Vector3 A;
        public Vector3 B;

        public Vector3Range(Vector3 init)
        {
            A = B = init;
        }

        public Vector3Range(Vector3 inA, Vector3 inB)
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