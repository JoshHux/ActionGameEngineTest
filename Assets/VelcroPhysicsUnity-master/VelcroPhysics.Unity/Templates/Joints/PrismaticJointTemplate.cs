using VelcroPhysics.Dynamics.VJoints;
using FixMath.NET;

namespace VelcroPhysics.Templates.VJoints
{
    /// <summary>
    /// Prismatic VJoint definition. This requires defining a line of
    /// motion using an axis and an anchor point. The definition uses local
    /// anchor points and a local axis so that the initial configuration
    /// can violate the constraint slightly. The VJoint translation is zero
    /// when the local anchor points coincide in world space. Using local
    /// anchors and a local axis helps when saving and loading a game.
    /// </summary>
    public class PrismaticVJointTemplate : VJointTemplate
    {
        public PrismaticVJointTemplate() : base(VJointType.Prismatic)
        {
        }

        /// <summary>
        /// Enable/disable the VJoint limit.
        /// </summary>
        public bool EnableLimit { get; set; }

        /// <summary>
        /// Enable/disable the VJoint motor.
        /// </summary>
        public bool EnableMotor { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public FVector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        /// </summary>
        public FVector2 LocalAnchorB { get; set; }

        /// <summary>
        /// The local translation unit axis in bodyA.
        /// </summary>
        public FVector2 LocalAxisA { get; set; }

        /// <summary>
        /// The lower translation limit, usually in meters.
        /// </summary>
        public Fix64 LowerTranslation { get; set; }

        /// <summary>
        /// The maximum motor torque, usually in N-m.
        /// </summary>
        public Fix64 MaxMotorForce { get; set; }

        /// <summary>
        /// The desired motor speed in radians per second.
        /// </summary>
        public Fix64 MotorSpeed { get; set; }

        /// <summary>
        /// The constrained angle between the bodies: bodyB_angle - bodyA_angle.
        /// </summary>
        public Fix64 ReferenceAngle { get; set; }

        /// <summary>
        /// The upper translation limit, usually in meters.
        /// </summary>
        public Fix64 UpperTranslation { get; set; }

        public override void SetDefaults()
        {
            LocalAxisA = new FVector2(1.0f,Fix64.Zero);
        }
    }
}