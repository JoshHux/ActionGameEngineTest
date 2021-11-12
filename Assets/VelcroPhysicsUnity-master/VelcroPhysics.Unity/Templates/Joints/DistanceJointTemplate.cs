using VelcroPhysics.Dynamics.VJoints;
using FixMath.NET;

namespace VelcroPhysics.Templates.VJoints
{
    /// <summary>
    /// Distance VJoint definition. This requires defining an
    /// anchor point on both bodies and the non-zero length of the
    /// distance VJoint. The definition uses local anchor points
    /// so that the initial configuration can violate the constraint
    /// slightly. This helps when saving and loading a game.
    /// <remarks>Do not use a zero or a short length.</remarks>
    /// </summary>
    public class DistanceVJointTemplate : VJointTemplate
    {
        public DistanceVJointTemplate() : base(VJointType.Distance)
        {
        }

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        public Fix64 DampingRatio { get; set; }

        /// <summary>
        /// The mass-spring-damper frequency in Hertz. A value of 0 disables softness.
        /// </summary>
        public Fix64 FrequencyHz { get; set; }

        /// <summary>
        /// The natural length between the anchor points.
        /// </summary>
        public Fix64 Length { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public FVector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        /// </summary>
        public FVector2 LocalAnchorB { get; set; }

        public override void SetDefaults()
        {
            Length =Fix64.One;
        }
    }
}