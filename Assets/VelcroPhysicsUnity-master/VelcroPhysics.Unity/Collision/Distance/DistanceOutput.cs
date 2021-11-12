using UnityEngine;
using FixMath.NET;

namespace VelcroPhysics.Collision.Distance
{
    /// <summary>
    /// Output for Distance.ComputeDistance().
    /// </summary>
    public struct DistanceOutput
    {
        public Fix64 Distance;

        /// <summary>
        /// Number of GJK iterations used
        /// </summary>
        public int Iterations;

        /// <summary>
        /// Closest point on shapeA
        /// </summary>
        public FVector2 PointA;

        /// <summary>
        /// Closest point on shapeB
        /// </summary>
        public FVector2 PointB;
    }
}