/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using UnityEngine;
using VelcroPhysics.Dynamics.Solver;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Dynamics.VJoints
{
    // Point-to-point constraint
    // Cdot = v2 - v1
    //      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
    // J = [-I -r1_skew I r2_skew ]
    // Identity used:
    // w k % (rx i + ry j) = w * (-ry i + rx j)

    // Angle constraint
    // Cdot = w2 - w1
    // J = [0 0 -1 0 0 1]
    // K = invI1 + invI2

    /// <summary>
    /// Friction VJoint. This is used for top-down friction.
    /// It provides 2D translational friction and angular friction.
    /// </summary>
    public class FrictionVJoint : VJoint
    {
        private float _angularImpulse;
        private float _angularMass;

        // Solver temp
        private int _indexA;

        private int _indexB;
        private float _invIA;
        private float _invIB;
        private float _invMassA;

        private float _invMassB;

        // Solver shared
        private Vector2 _linearImpulse;

        private Mat22 _linearMass;
        private Vector2 _localCenterA;
        private Vector2 _localCenterB;
        private Vector2 _rA;
        private Vector2 _rB;

        internal FrictionVJoint()
        {
            VJointType = VJointType.Friction;
        }

        /// <summary>
        /// Constructor for FrictionVJoint.
        /// </summary>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="anchor"></param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public FrictionVJoint(Body bodyA, Body bodyB, Vector2 anchor, bool useWorldCoordinates = false)
            : base(bodyA, bodyB)
        {
            VJointType = VJointType.Friction;

            if (useWorldCoordinates)
            {
                LocalAnchorA = BodyA.GetLocalPoint(anchor);
                LocalAnchorB = BodyB.GetLocalPoint(anchor);
            }
            else
            {
                LocalAnchorA = anchor;
                LocalAnchorB = anchor;
            }
        }

        /// <summary>
        /// The local anchor point on BodyA
        /// </summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point on BodyB
        /// </summary>
        public Vector2 LocalAnchorB { get; set; }

        public override Vector2 WorldAnchorA
        {
            get => BodyA.GetWorldPoint(LocalAnchorA);
            set => LocalAnchorA = BodyA.GetLocalPoint(value);
        }

        public override Vector2 WorldAnchorB
        {
            get => BodyB.GetWorldPoint(LocalAnchorB);
            set => LocalAnchorB = BodyB.GetLocalPoint(value);
        }

        /// <summary>
        /// The maximum friction force in N.
        /// </summary>
        public float MaxForce { get; set; }

        /// <summary>
        /// The maximum friction torque in N-m.
        /// </summary>
        public float MaxTorque { get; set; }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * _linearImpulse;
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * _angularImpulse;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _indexA = BodyA.IslandIndex;
            _indexB = BodyB.IslandIndex;
            _localCenterA = BodyA._sweep.LocalCenter;
            _localCenterB = BodyB._sweep.LocalCenter;
            _invMassA = BodyA._invMass;
            _invMassB = BodyB._invMass;
            _invIA = BodyA._invI;
            _invIB = BodyB._invI;

            var aA = data.Positions[_indexA].A;
            var vA = data.Velocities[_indexA].V;
            var wA = data.Velocities[_indexA].W;

            var aB = data.Positions[_indexB].A;
            var vB = data.Velocities[_indexB].V;
            var wB = data.Velocities[_indexB].W;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            // Compute the effective mass matrix.
            _rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            _rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
            //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            var K = new Mat22();
            K.ex.x = mA + mB + iA * _rA.y * _rA.y + iB * _rB.y * _rB.y;
            K.ex.y = -iA * _rA.x * _rA.y - iB * _rB.x * _rB.y;
            K.ey.x = K.ex.y;
            K.ey.y = mA + mB + iA * _rA.x * _rA.x + iB * _rB.x * _rB.x;

            _linearMass = K.Inverse;

            _angularMass = iA + iB;
            if (_angularMass > 0.0f) _angularMass = 1.0f / _angularMass;

            if (Settings.EnableWarmstarting)
            {
                // Scale impulses to support a variable time step.
                _linearImpulse *= data.Step.dtRatio;
                _angularImpulse *= data.Step.dtRatio;

                var P = new Vector2(_linearImpulse.x, _linearImpulse.y);
                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(_rA, P) + _angularImpulse);
                vB += mB * P;
                wB += iB * (MathUtils.Cross(_rB, P) + _angularImpulse);
            }
            else
            {
                _linearImpulse = Vector2.zero;
                _angularImpulse = 0.0f;
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            var vA = data.Velocities[_indexA].V;
            var wA = data.Velocities[_indexA].W;
            var vB = data.Velocities[_indexB].V;
            var wB = data.Velocities[_indexB].W;

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            var h = data.Step.dt;

            // Solve angular friction
            {
                var Cdot = wB - wA;
                var impulse = -_angularMass * Cdot;

                var oldImpulse = _angularImpulse;
                var maxImpulse = h * MaxTorque;
                _angularImpulse = MathUtils.Clamp(_angularImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _angularImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve linear friction
            {
                var Cdot = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);

                var impulse = -MathUtils.Mul(ref _linearMass, Cdot);
                var oldImpulse = _linearImpulse;
                _linearImpulse += impulse;

                var maxImpulse = h * MaxForce;

                if (_linearImpulse.sqrMagnitude > maxImpulse * maxImpulse)
                {
                    _linearImpulse.Normalize();
                    _linearImpulse *= maxImpulse;
                }

                impulse = _linearImpulse - oldImpulse;

                vA -= mA * impulse;
                wA -= iA * MathUtils.Cross(_rA, impulse);

                vB += mB * impulse;
                wB += iB * MathUtils.Cross(_rB, impulse);
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
        }
    }
}