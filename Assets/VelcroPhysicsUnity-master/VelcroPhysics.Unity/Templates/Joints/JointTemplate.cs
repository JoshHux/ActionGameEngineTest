using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.VJoints;

namespace VelcroPhysics.Templates.VJoints
{
    public class VJointTemplate : IDefaults
    {
        public VJointTemplate(VJointType type)
        {
            Type = type;
        }

        /// <summary>
        /// The first attached body.
        /// </summary>
        public Body BodyA { get; set; }

        /// <summary>
        /// The second attached body.
        /// </summary>
        public Body BodyB { get; set; }

        /// <summary>
        /// Set this flag to true if the attached bodies should collide.
        /// </summary>
        public bool CollideConnected { get; set; }

        /// <summary>
        /// The VJoint type is set automatically for concrete VJoint types.
        /// </summary>
        public VJointType Type { get; }

        /// <summary>
        /// Use this to attach application specific data.
        /// </summary>
        public object UserData { get; set; }

        public virtual void SetDefaults()
        {
        }
    }
}