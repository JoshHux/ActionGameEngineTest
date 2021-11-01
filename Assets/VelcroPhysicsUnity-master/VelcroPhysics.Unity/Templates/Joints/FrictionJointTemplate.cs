using UnityEngine;
using VelcroPhysics.Dynamics.VJoints;

namespace VelcroPhysics.Templates.VJoints
{
    public class FrictionVJointTemplate : VJointTemplate
    {
        public FrictionVJointTemplate() : base(VJointType.Friction)
        {
        }

        /// <summary>
        /// The local anchor point relative to bodyA's origin.
        /// </summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point relative to bodyB's origin.
        /// </summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>
        /// The maximum friction force in N.
        /// </summary>
        public float MaxForce { get; set; }

        /// <summary>
        /// The maximum friction torque in N-m.
        /// </summary>
        public float MaxTorque { get; set; }
    }
}