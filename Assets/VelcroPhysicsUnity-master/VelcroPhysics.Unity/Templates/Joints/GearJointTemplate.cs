using VelcroPhysics.Dynamics.VJoints;
using FixMath.NET;

namespace VelcroPhysics.Templates.VJoints
{
    public class GearVJointTemplate : VJointTemplate
    {
        public GearVJointTemplate() : base(VJointType.Gear)
        {
        }

        /// <summary>
        /// The first revolute/prismatic VJoint attached to the gear VJoint.
        /// </summary>
        public VJoint VJointA { get; set; }

        /// <summary>
        /// The second revolute/prismatic VJoint attached to the gear VJoint.
        /// </summary>
        public VJoint VJointB { get; set; }

        /// <summary>
        /// The gear ratio.
        /// </summary>
        public Fix64 Ratio { get; set; }

        public override void SetDefaults()
        {
            Ratio = Fix64.One;
        }
    }
}