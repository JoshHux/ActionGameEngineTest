using VelcroPhysics.Dynamics.VJoints;
using FixMath.NET;

namespace VelcroPhysics.Templates.VJoints
{
    public class MotorVJointTemplate : VJointTemplate
    {
        public MotorVJointTemplate() : base(VJointType.Motor)
        {
        }

        /// <summary>
        /// The bodyB angle minus bodyA angle in radians.
        /// </summary>
        public Fix64 AngularOffset { get; set; }

        /// <summary>
        /// Position correction factor in the range [0,1].
        /// </summary>
        public Fix64 CorrectionFactor { get; set; }

        /// <summary>
        /// Position of bodyB minus the position of bodyA, in bodyA's frame, in meters.
        /// </summary>
        public FVector2 LinearOffset { get; set; }

        /// <summary>
        /// The maximum motor force in N.
        /// </summary>
        public Fix64 MaxForce { get; set; }

        /// <summary>
        /// The maximum motor torque in N-m.
        /// </summary>
        public Fix64 MaxTorque { get; set; }

        public override void SetDefaults()
        {
            MaxForce =Fix64.One;
            MaxTorque =Fix64.One;
            CorrectionFactor = 0.3f;
        }
    }
}