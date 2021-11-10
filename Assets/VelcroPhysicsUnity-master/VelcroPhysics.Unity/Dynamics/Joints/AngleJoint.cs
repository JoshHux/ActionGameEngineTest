/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using UnityEngine;
using VelcroPhysics.Dynamics.Solver;
using FixMath.NET;

namespace VelcroPhysics.Dynamics.VJoints
{
    /// <summary>
    /// Maintains a fixed angle between two bodies
    /// </summary>
    public class AngleVJoint : VJoint
    {
        private Fix64 _bias;
        private Fix64 _VJointError;
        private Fix64 _massFactor;
        private Fix64 _targetAngle;

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
            MaxImpulse = Fix64.MaxValue;
        }

        public override FVector2 WorldAnchorA
        {
            get => BodyA.Position;
            set => Debug.Assert(false, "You can't set the world anchor on this VJoint type.");
        }

        public override FVector2 WorldAnchorB
        {
            get => BodyB.Position;
            set => Debug.Assert(false, "You can't set the world anchor on this VJoint type.");
        }

        /// <summary>
        /// The desired angle between BodyA and BodyB
        /// </summary>
        public Fix64 TargetAngle
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
        public Fix64 BiasFactor { get; set; }

        /// <summary>
        /// Gets or sets the maximum impulse
        /// Defaults to Fix64.MaxValue
        /// </summary>
        public Fix64 MaxImpulse { get; set; }

        /// <summary>
        /// Gets or sets the softness of the VJoint
        /// Defaults to 0
        /// </summary>
        public Fix64 Softness { get; set; }

        public override FVector2 GetReactionForce(Fix64 invDt)
        {
            //TODO
            //return _inv_dt * _impulse;
            return FVector2.zero;
        }

        public override Fix64 GetReactionTorque(Fix64 invDt)
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

            data.Velocities[indexA].W -= BodyA._invI * Fix64.Sign(p) * Fix64.Min(Fix64.Abs(p), MaxImpulse);
            data.Velocities[indexB].W += BodyB._invI * Fix64.Sign(p) * Fix64.Min(Fix64.Abs(p), MaxImpulse);
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            //no position solving for this VJoint
            return true;
        }
    }
}