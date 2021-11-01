using UnityEngine;
using VelcroPhysics.Dynamics.VJoints;

namespace VelcroPhysics.Templates.VJoints
{
    /// <summary>
    /// Rope VJoint definition. This requires two body anchor points and
    /// a maximum lengths.
    /// <remarks>By default the connected objects will not collide.</remarks>
    /// </summary>
    public class RopeVJointTemplate : VJointTemplate
    {
        public RopeVJointTemplate() : base(VJointType.Rope)
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
        /// The maximum length of the rope.
        /// <remarks>This must be larger than Settings.LinearSlop or the VJoint will have no effect.</remarks>
        /// </summary>
        public float MaxLength { get; set; }

        public override void SetDefaults()
        {
            LocalAnchorA = new Vector2(-1.0f, 0.0f);
            LocalAnchorB = new Vector2(1.0f, 0.0f);
        }
    }
}