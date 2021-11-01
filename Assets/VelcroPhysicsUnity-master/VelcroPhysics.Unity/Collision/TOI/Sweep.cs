using UnityEngine;
using VelcroPhysics.Utilities;
using VTransform = VelcroPhysics.Shared.VTransform;

namespace VelcroPhysics.Collision.TOI
{
    /// <summary>
    /// This describes the motion of a body/shape for TOI computation.
    /// Shapes are defined with respect to the body origin, which may
    /// no coincide with the center of mass. However, to support dynamics
    /// we must interpolate the center of mass position.
    /// </summary>
    public struct Sweep
    {
        /// <summary>
        /// World angles
        /// </summary>
        public float A;

        public float A0;

        /// <summary>
        /// Fraction of the current time step in the range [0,1]
        /// c0 and a0 are the positions at alpha0.
        /// </summary>
        public float Alpha0;

        /// <summary>
        /// Center world positions
        /// </summary>
        public Vector2 C;

        public Vector2 C0;

        /// <summary>
        /// Local center of mass position
        /// </summary>
        public Vector2 LocalCenter;

        /// <summary>
        /// Get the interpolated VTransform at a specific time.
        /// </summary>
        /// <param name="xfb">The VTransform.</param>
        /// <param name="beta">beta is a factor in [0,1], where 0 indicates alpha0.</param>
        public void GetVTransform(out VTransform xfb, float beta)
        {
            xfb = new VTransform();
            xfb.p.x = (1.0f - beta) * C0.x + beta * C.x;
            xfb.p.y = (1.0f - beta) * C0.y + beta * C.y;
            var angle = (1.0f - beta) * A0 + beta * A;
            xfb.q.Set(angle);

            // Shift to origin
            xfb.p -= MathUtils.Mul(xfb.q, LocalCenter);
        }

        /// <summary>
        /// Advance the sweep forward, yielding a new initial state.
        /// </summary>
        /// <param name="alpha">new initial time</param>
        public void Advance(float alpha)
        {
            Debug.Assert(Alpha0 < 1.0f);
            var beta = (alpha - Alpha0) / (1.0f - Alpha0);
            C0 += beta * (C - C0);
            A0 += beta * (A - A0);
            Alpha0 = alpha;
        }

        /// <summary>
        /// Normalize the angles.
        /// </summary>
        public void Normalize()
        {
            var d = Mathf.PI * 2f * Mathf.Floor(A0 / (Mathf.PI * 2f));
            A0 -= d;
            A -= d;
        }
    }
}