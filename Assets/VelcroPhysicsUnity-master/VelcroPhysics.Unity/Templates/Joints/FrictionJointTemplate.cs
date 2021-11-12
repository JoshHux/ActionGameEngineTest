using VelcroPhysics.Dynamics.VJoints;
using FixMath.NET;

namespace VelcroPhysics.Templates.VJoints
{
    public class FrictionVJointTemplate : VJointTemplate
    {
        public FrictionVJointTemplate() : base(VJointType.Friction)
        {
        }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public FVector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        /// </summary>
        public FVector2 LocalAnchorB { get; set; }

        /// <summary>
        /// The maximum friction force in N.
        /// </summary>
        public Fix64 MaxForce { get; set; }

        /// <summary>
        /// The maximum friction torque in N-m.
        /// </summary>
        public Fix64 MaxTorque { get; set; }
    }
}