using VelcroPhysics.Dynamics.VJoints;
using FixMath.NET;

namespace VelcroPhysics.Templates.VJoints
{
    /// <summary>
    /// Pulley VJoint definition. This requires two ground anchors,
    /// two dynamic body anchor points, and a pulley ratio.
    /// </summary>
    public class PulleyVJointTemplate : VJointTemplate
    {
        public PulleyVJointTemplate() : base(VJointType.Pulley)
        {
        }

        /// <summary>
        /// The first ground anchor in world coordinates. This point never moves.
        /// </summary>
        public FVector2 GroundAnchorA { get; set; }

        /// <summary>
        /// The second ground anchor in world coordinates. This point never moves.
        /// </summary>
        public FVector2 GroundAnchorB { get; set; }

        /// <summary>
        /// The a reference length for the segment attached to bodyA.
        /// </summary>
        public Fix64 LengthA { get; set; }

        /// <summary>
        /// The a reference length for the segment attached to bodyB.
        /// </summary>
        public Fix64 LengthB { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public FVector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        public FVector2 LocalAnchorB { get; set; }

        /// <summary>
        /// The pulley ratio, used to simulate a block-and-tackle.
        /// </summary>
        public Fix64 Ratio { get; set; }

        public override void SetDefaults()
        {
            GroundAnchorA = new FVector2(-1, Fix64.One);
            GroundAnchorB = new FVector2(1, Fix64.One);
            LocalAnchorA = new FVector2(-1, Fix64.Zero);
            LocalAnchorB = new FVector2(1, Fix64.Zero);
            Ratio = Fix64.One;
            CollideConnected = true;
        }
    }
}