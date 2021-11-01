/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using UnityEngine;
using VelcroPhysics.Dynamics.Solver;

namespace VelcroPhysics.Dynamics.VJoints
{
    /// <summary>
    /// Maintains a fixed angle between two bodies
    /// </summary>
    public class AngleVJoint : VJoint
    {
        private float _bias;
        private float _VJointError;
        private float _massFactor;
        private float _targetAngle;

        internal AngleVJoint()
        {
            VJointType = VJointType.Angle;
        }

        /// <summary>
        /// Constructor for AngleVJoint
        /// </summary>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        public AngleVJoint(Body bodyA, Body bodyB)
            : base(bodyA, bodyB)
        {
            VJointType = VJointType.Angle;
            BiasFactor = .2f;
            MaxImpulse = float.MaxValue;
        }

        public override Vector2 WorldAnchorA
        {
            get => BodyA.Position;
            set => Debug.Assert(false, "You can't set the world anchor on this VJoint type.");
        }

        public override Vector2 WorldAnchorB
        {
            get => BodyB.Position;
            set => Debug.Assert(false, "You can't set the world anchor on this VJoint type.");
        }

        /// <summary>
        /// The desired angle between BodyA and BodyB
        /// </summary>
        public float TargetAngle
        {
            get => _targetAngle;
            set
            {
                if (value != _targetAngle)
                {
                    _targetAngle = value;
                    WakeBodies();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bias factor.
        /// Defaults to 0.2
        /// </summary>
        public float BiasFactor { get; set; }

        /// <summary>
        /// Gets or sets the maximum impulse
        /// Defaults to float.MaxValue
        /// </summary>
        public float MaxImpulse { get; set; }

        /// <summary>
        /// Gets or sets the softness of the VJoint
        /// Defaults to 0
        /// </summary>
        public float Softness { get; set; }

        public override Vector2 GetReactionForce(float invDt)
        {
            //TODO
            //return _inv_dt * _impulse;
            return Vector2.zero;
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            var indexA = BodyA.IslandIndex;
            var indexB = BodyB.IslandIndex;

            var aW = data.Positions[indexA].A;
            var bW = data.Positions[indexB].A;

            _VJointError = bW - aW - TargetAngle;
            _bias = -BiasFactor * data.Step.inv_dt * _VJointError;
            _massFactor = (1 - Softness) / (BodyA._invI + BodyB._invI);
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            var indexA = BodyA.IslandIndex;
            var indexB = BodyB.IslandIndex;

            var p = (_bias - data.Velocities[indexB].W + data.Velocities[indexA].W) * _massFactor;

            data.Velocities[indexA].W -= BodyA._invI * Mathf.Sign(p) * Mathf.Min(Mathf.Abs(p), MaxImpulse);
            data.Velocities[indexB].W += BodyB._invI * Mathf.Sign(p) * Mathf.Min(Mathf.Abs(p), MaxImpulse);
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            //no position solving for this VJoint
            return true;
        }
    }
}