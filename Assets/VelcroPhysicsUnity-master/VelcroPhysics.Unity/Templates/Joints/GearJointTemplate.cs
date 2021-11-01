using VelcroPhysics.Dynamics.VJoints;

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
        public float Ratio { get; set; }

        public override void SetDefaults()
        {
            Ratio = 1.0f;
        }
    }
}