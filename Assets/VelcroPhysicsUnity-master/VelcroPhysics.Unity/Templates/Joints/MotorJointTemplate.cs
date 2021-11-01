using UnityEngine;
using VelcroPhysics.Dynamics.VJoints;

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
        public float AngularOffset { get; set; }

        /// <summary>
        /// Position correction factor in the range [0,1].
        /// </summary>
        public float CorrectionFactor { get; set; }

        /// <summary>
        /// Position of bodyB minus the position of bodyA, in bodyA's frame, in meters.
        /// </summary>
        public Vector2 LinearOffset { get; set; }

        /// <summary>
        /// The maximum motor force in N.
        /// </summary>
        public float MaxForce { get; set; }

        /// <summary>
        /// The maximum motor torque in N-m.
        /// </summary>
        public float MaxTorque { get; set; }

        public override void SetDefaults()
        {
            MaxForce = 1.0f;
            MaxTorque = 1.0f;
            CorrectionFactor = 0.3f;
        }
    }
}