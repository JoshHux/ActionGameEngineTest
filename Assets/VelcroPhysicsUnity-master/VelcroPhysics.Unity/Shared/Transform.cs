using FixMath.NET;

namespace VelcroPhysics.Shared
{
    /// <summary>
    /// A VTransform contains translation and rotation. It is used to represent
    /// the position and orientation of rigid frames.
    /// </summary>
    public struct VTransform
    {
        public FVector2 p;
        public Rot q;

        /// <summary>
        /// Initialize using a position vector and a rotation matrix.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The r.</param>
        public VTransform(ref FVector2 position, ref Rot rotation)
        {
            p = position;
            q = rotation;
        }

        /// <summary>
        /// Set this to the identity VTransform.
        /// </summary>
        public void SetIdentity()
        {
            p = FVector2.zero;
            q.SetIdentity();
        }

        /// <summary>
        /// Set this based on the position and angle.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="angle">The angle.</param>
        public void Set(FVector2 position, Fix64 angle)
        {
            p = position;
            q.Set(angle);
        }
    }
}