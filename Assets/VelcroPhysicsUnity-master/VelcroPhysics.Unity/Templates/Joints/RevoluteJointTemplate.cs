using VelcroPhysics.Dynamics.VJoints;
using FixMath.NET;

namespace VelcroPhysics.Templates.VJoints
{
    /// <summary>
    /// Revolute VJoint definition. This requires defining an
    /// anchor point where the bodies are joined. The definition
    /// uses local anchor points so that the initial configuration
    /// can violate the constraint slightly. You also need to
    /// specify the initial relative angle for VJoint limits. This
    /// helps when saving and loading a game.
    /// The local anchor points are measured from the body's origin
    /// rather than the center of mass because:
    /// 1. you might not know where the center of mass will be.
    /// 2. if you add/remove shapes from a body and recompute the mass,
    /// the VJoints will be broken.
    /// </summary>
    public class RevoluteVJointTemplate : VJointTemplate
    {
        public RevoluteVJointTemplate() : base(VJointType.Revolute)
        {
        }

        /// <summary>
        /// A flag to enable VJoint limits.
        /// </summary>
        public bool EnableLimit { get; set; }

        /// <summary>
        /// A flag to enable the VJoint motor.
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
        /// The lower angle for the VJoint limit (radians).
        /// </summary>
        public Fix64 LowerAngle { get; set; }

        /// <summary>
        /// The maximum motor torque used to achieve the desired motor speed. Usually in N-m.
        /// </summary>
        public Fix64 MaxMotorTorque { get; set; }

        /// <summary>
        /// The desired motor speed. Usually in radians per second.
        /// </summary>
        public Fix64 MotorSpeed { get; set; }

        /// <summary>
        /// The bodyB angle minus bodyA angle in the reference state (radians).
        /// </summary>
        public Fix64 ReferenceAngle { get; set; }

        /// <summary>
        /// The upper angle for the VJoint limit (radians).
        /// </summary>
        public Fix64 UpperAngle { get; set; }
    }
}