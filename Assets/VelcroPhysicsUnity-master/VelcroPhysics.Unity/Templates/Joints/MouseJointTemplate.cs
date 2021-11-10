using VelcroPhysics.Dynamics.VJoints;
using FixMath.NET;

namespace VelcroPhysics.Templates.VJoints
{
    /// <summary>
    /// Mouse VJoint definition. This requires a world target point,
    /// tuning parameters, and the time step.
    /// </summary>
    public class MouseVJointTemplate : VJointTemplate
    {
        public MouseVJointTemplate() : base(VJointType.FixedMouse)
        {
        }

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        public Fix64 DampingRatio { get; set; }

        /// <summary>
        /// The response speed.
        /// </summary>
        public Fix64 FrequencyHz { get; set; }

        /// <summary>
        /// The maximum constraint force that can be exerted
        /// to move the candidate body. Usually you will express
        /// as some multiple of the weight (multiplier * mass * gravity).
        /// </summary>
        public Fix64 MaxForce { get; set; }

        /// <summary>
        /// The initial world target point. This is assumed
        /// to coincide with the body anchor initially.
        /// </summary>
        public FVector2 Target { get; set; }

        public override void SetDefaults()
        {
            FrequencyHz = 5.0f;
            DampingRatio = 0.7f;
        }
    }
}