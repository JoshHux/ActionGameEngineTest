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

using VelcroPhysics.Dynamics.Solver;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;
using VTransform = VelcroPhysics.Shared.VTransform;
using FixMath.NET;

namespace VelcroPhysics.Dynamics.VJoints
{
    // Gear VJoint:
    // C0 = (coordinate1 + ratio * coordinate2)_initial
    // C = (coordinate1 + ratio * coordinate2) - C0 = 0
    // J = [J1 ratio * J2]
    // K = J * invM * JT
    //   = J1 * invM1 * J1T + ratio * ratio * J2 * invM2 * J2T
    //
    // Revolute:
    // coordinate = rotation
    // Cdot = angularVelocity
    // J = [0 0 1]
    // K = J * invM * JT = invI
    //
    // Prismatic:
    // coordinate = dot(p - pg, ug)
    // Cdot = dot(v + cross(w, r), ug)
    // J = [ug cross(r, ug)]
    // K = J * invM * JT = invMass + invI * cross(r, ug)^2

    /// <summary>
    /// A gear VJoint is used to connect two VJoints together.
    /// Either VJoint can be a revolute or prismatic VJoint.
    /// You specify a gear ratio to bind the motions together:
    /// <![CDATA[coordinate1 + ratio * coordinate2 = ant]]>
    /// The ratio can be negative or positive. If one VJoint is a revolute VJoint
    /// and the other VJoint is a prismatic VJoint, then the ratio will have units
    /// of length or units of 1/length.
    /// Warning: You have to manually destroy the gear VJoint if VJointA or VJointB is destroyed.
    /// </summary>
    public class GearVJoint : VJoint
    {
        private Body _bodyA;
        private Body _bodyB;
        private Body _bodyC;
        private Body _bodyD;

        private Fix64 _constant;
        private Fix64 _iA, _iB, _iC, _iD;

        private Fix64 _impulse;

        // Solver temp
        private int _indexA, _indexB, _indexC, _indexD;

        private FVector2 _JvAC, _JvBD;
        private Fix64 _JwA, _JwB, _JwC, _JwD;
        private FVector2 _lcA, _lcB, _lcC, _lcD;

        // Solver shared
        private FVector2 _localAnchorA;

        private FVector2 _localAnchorB;
        private FVector2 _localAnchorC;
        private FVector2 _localAnchorD;

        private FVector2 _localAxisC;
        private FVector2 _localAxisD;
        private Fix64 _mA, _mB, _mC, _mD;
        private Fix64 _mass;
        private Fix64 _ratio;

        private Fix64 _referenceAngleA;
        private Fix64 _referenceAngleB;
        private VJointType _typeA;
        private VJointType _typeB;

        /// <summary>
        /// Requires two existing revolute or prismatic VJoints (any combination will work).
        /// The provided VJoints must attach a dynamic body to a static body.
        /// </summary>
        /// <param name="VJointA">The first VJoint.</param>
        /// <param name="VJointB">The second VJoint.</param>
        /// <param name="ratio">The ratio.</param>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        public GearVJoint(Body bodyA, Body bodyB, VJoint VJointA, VJoint VJointB, Fix64? holdRatio = null)
        {
            Fix64 ratio = holdRatio ?? Fix64.One;
            VJointType = VJointType.Gear;
            BodyA = bodyA;
            BodyB = bodyB;
            VJointA = VJointA;
            VJointB = VJointB;
            Ratio = ratio;

            _typeA = VJointA.VJointType;
            _typeB = VJointB.VJointType;

            Debug.Assert(_typeA == VJointType.Revolute || _typeA == VJointType.Prismatic ||
                         _typeA == VJointType.FixedRevolute || _typeA == VJointType.FixedPrismatic);
            Debug.Assert(_typeB == VJointType.Revolute || _typeB == VJointType.Prismatic ||
                         _typeB == VJointType.FixedRevolute || _typeB == VJointType.FixedPrismatic);

            Fix64 coordinateA, coordinateB;

            // TODO_ERIN there might be some problem with the VJoint edges in b2VJoint.

            _bodyC = VJointA.BodyA;
            _bodyA = VJointA.BodyB;

            // Get geometry of VJoint1
            var xfA = _bodyA._xf;
            var aA = _bodyA._sweep.A;
            var xfC = _bodyC._xf;
            var aC = _bodyC._sweep.A;

            if (_typeA == VJointType.Revolute)
            {
                var revolute = (RevoluteVJoint)VJointA;
                _localAnchorC = revolute.LocalAnchorA;
                _localAnchorA = revolute.LocalAnchorB;
                _referenceAngleA = revolute.ReferenceAngle;
                _localAxisC = FVector2.zero;

                coordinateA = aA - aC - _referenceAngleA;
            }
            else
            {
                var prismatic = (PrismaticVJoint)VJointA;
                _localAnchorC = prismatic.LocalAnchorA;
                _localAnchorA = prismatic.LocalAnchorB;
                _referenceAngleA = prismatic.ReferenceAngle;
                _localAxisC = prismatic.LocalXAxis;

                var pC = _localAnchorC;
                var pA = MathUtils.MulT(xfC.q, MathUtils.Mul(xfA.q, _localAnchorA) + (xfA.p - xfC.p));
                coordinateA = FVector2.Dot(pA - pC, _localAxisC);
            }

            _bodyD = VJointB.BodyA;
            _bodyB = VJointB.BodyB;

            // Get geometry of VJoint2
            var xfB = _bodyB._xf;
            var aB = _bodyB._sweep.A;
            var xfD = _bodyD._xf;
            var aD = _bodyD._sweep.A;

            if (_typeB == VJointType.Revolute)
            {
                var revolute = (RevoluteVJoint)VJointB;
                _localAnchorD = revolute.LocalAnchorA;
                _localAnchorB = revolute.LocalAnchorB;
                _referenceAngleB = revolute.ReferenceAngle;
                _localAxisD = FVector2.zero;

                coordinateB = aB - aD - _referenceAngleB;
            }
            else
            {
                var prismatic = (PrismaticVJoint)VJointB;
                _localAnchorD = prismatic.LocalAnchorA;
                _localAnchorB = prismatic.LocalAnchorB;
                _referenceAngleB = prismatic.ReferenceAngle;
                _localAxisD = prismatic.LocalXAxis;

                var pD = _localAnchorD;
                var pB = MathUtils.MulT(xfD.q, MathUtils.Mul(xfB.q, _localAnchorB) + (xfB.p - xfD.p));
                coordinateB = FVector2.Dot(pB - pD, _localAxisD);
            }

            _ratio = ratio;
            _constant = coordinateA + _ratio * coordinateB;
            _impulse = Fix64.Zero;
        }

        public override FVector2 WorldAnchorA
        {
            get => _bodyA.GetWorldPoint(_localAnchorA);
            set => Debug.Assert(false, "You can't set the world anchor on this VJoint type.");
        }

        public override FVector2 WorldAnchorB
        {
            get => _bodyB.GetWorldPoint(_localAnchorB);
            set => Debug.Assert(false, "You can't set the world anchor on this VJoint type.");
        }

        /// <summary>
        /// The gear ratio.
        /// </summary>
        public Fix64 Ratio
        {
            get => _ratio;
            set
            {
                Debug.Assert(MathUtils.IsValid(value));
                _ratio = value;
            }
        }

        /// <summary>
        /// The first revolute/prismatic VJoint attached to the gear VJoint.
        /// </summary>
        public VJoint VJointA { get; private set; }

        /// <summary>
        /// The second revolute/prismatic VJoint attached to the gear VJoint.
        /// </summary>
        public VJoint VJointB { get; private set; }

        public override FVector2 GetReactionForce(Fix64 invDt)
        {
            var P = _impulse * _JvAC;
            return invDt * P;
        }

        public override Fix64 GetReactionTorque(Fix64 invDt)
        {
            var L = _impulse * _JwA;
            return invDt * L;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _indexA = _bodyA.IslandIndex;
            _indexB = _bodyB.IslandIndex;
            _indexC = _bodyC.IslandIndex;
            _indexD = _bodyD.IslandIndex;
            _lcA = _bodyA._sweep.LocalCenter;
            _lcB = _bodyB._sweep.LocalCenter;
            _lcC = _bodyC._sweep.LocalCenter;
            _lcD = _bodyD._sweep.LocalCenter;
            _mA = _bodyA._invMass;
            _mB = _bodyB._invMass;
            _mC = _bodyC._invMass;
            _mD = _bodyD._invMass;
            _iA = _bodyA._invI;
            _iB = _bodyB._invI;
            _iC = _bodyC._invI;
            _iD = _bodyD._invI;

            var aA = data.Positions[_indexA].A;
            var vA = data.Velocities[_indexA].V;
            var wA = data.Velocities[_indexA].W;

            var aB = data.Positions[_indexB].A;
            var vB = data.Velocities[_indexB].V;
            var wB = data.Velocities[_indexB].W;

            var aC = data.Positions[_indexC].A;
            var vC = data.Velocities[_indexC].V;
            var wC = data.Velocities[_indexC].W;

            var aD = data.Positions[_indexD].A;
            var vD = data.Velocities[_indexD].V;
            var wD = data.Velocities[_indexD].W;

            Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

            _mass = Fix64.Zero;

            if (_typeA == VJointType.Revolute)
            {
                _JvAC = FVector2.zero;
                _JwA = Fix64.One;
                _JwC = Fix64.One;
                _mass += _iA + _iC;
            }
            else
            {
                var u = MathUtils.Mul(qC, _localAxisC);
                var rC = MathUtils.Mul(qC, _localAnchorC - _lcC);
                var rA = MathUtils.Mul(qA, _localAnchorA - _lcA);
                _JvAC = u;
                _JwC = MathUtils.Cross(rC, u);
                _JwA = MathUtils.Cross(rA, u);
                _mass += _mC + _mA + _iC * _JwC * _JwC + _iA * _JwA * _JwA;
            }

            if (_typeB == VJointType.Revolute)
            {
                _JvBD = FVector2.zero;
                _JwB = _ratio;
                _JwD = _ratio;
                _mass += _ratio * _ratio * (_iB + _iD);
            }
            else
            {
                var u = MathUtils.Mul(qD, _localAxisD);
                var rD = MathUtils.Mul(qD, _localAnchorD - _lcD);
                var rB = MathUtils.Mul(qB, _localAnchorB - _lcB);
                _JvBD = _ratio * u;
                _JwD = _ratio * MathUtils.Cross(rD, u);
                _JwB = _ratio * MathUtils.Cross(rB, u);
                _mass += _ratio * _ratio * (_mD + _mB) + _iD * _JwD * _JwD + _iB * _JwB * _JwB;
            }

            // Compute effective mass.
            _mass = _mass > Fix64.Zero ? Fix64.One / _mass : Fix64.Zero;

            if (Settings.EnableWarmstarting)
            {
                vA += _mA * _impulse * _JvAC;
                wA += _iA * _impulse * _JwA;
                vB += _mB * _impulse * _JvBD;
                wB += _iB * _impulse * _JwB;
                vC -= _mC * _impulse * _JvAC;
                wC -= _iC * _impulse * _JwC;
                vD -= _mD * _impulse * _JvBD;
                wD -= _iD * _impulse * _JwD;
            }
            else
            {
                _impulse = Fix64.Zero;
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
            data.Velocities[_indexC].V = vC;
            data.Velocities[_indexC].W = wC;
            data.Velocities[_indexD].V = vD;
            data.Velocities[_indexD].W = wD;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            var vA = data.Velocities[_indexA].V;
            var wA = data.Velocities[_indexA].W;
            var vB = data.Velocities[_indexB].V;
            var wB = data.Velocities[_indexB].W;
            var vC = data.Velocities[_indexC].V;
            var wC = data.Velocities[_indexC].W;
            var vD = data.Velocities[_indexD].V;
            var wD = data.Velocities[_indexD].W;

            var Cdot = FVector2.Dot(_JvAC, vA - vC) + FVector2.Dot(_JvBD, vB - vD);
            Cdot += _JwA * wA - _JwC * wC + (_JwB * wB - _JwD * wD);

            var impulse = -_mass * Cdot;
            _impulse += impulse;

            vA += _mA * impulse * _JvAC;
            wA += _iA * impulse * _JwA;
            vB += _mB * impulse * _JvBD;
            wB += _iB * impulse * _JwB;
            vC -= _mC * impulse * _JvAC;
            wC -= _iC * impulse * _JwC;
            vD -= _mD * impulse * _JvBD;
            wD -= _iD * impulse * _JwD;

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
            data.Velocities[_indexC].V = vC;
            data.Velocities[_indexC].W = wC;
            data.Velocities[_indexD].V = vD;
            data.Velocities[_indexD].W = wD;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            var cA = data.Positions[_indexA].C;
            var aA = data.Positions[_indexA].A;
            var cB = data.Positions[_indexB].C;
            var aB = data.Positions[_indexB].A;
            var cC = data.Positions[_indexC].C;
            var aC = data.Positions[_indexC].A;
            var cD = data.Positions[_indexD].C;
            var aD = data.Positions[_indexD].A;

            Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

            const Fix64 linearError = Fix64.Zero;

            Fix64 coordinateA, coordinateB;

            FVector2 JvAC, JvBD;
            Fix64 JwA, JwB, JwC, JwD;
            var mass = Fix64.Zero;

            if (_typeA == VJointType.Revolute)
            {
                JvAC = FVector2.zero;
                JwA = Fix64.One;
                JwC = Fix64.One;
                mass += _iA + _iC;

                coordinateA = aA - aC - _referenceAngleA;
            }
            else
            {
                var u = MathUtils.Mul(qC, _localAxisC);
                var rC = MathUtils.Mul(qC, _localAnchorC - _lcC);
                var rA = MathUtils.Mul(qA, _localAnchorA - _lcA);
                JvAC = u;
                JwC = MathUtils.Cross(rC, u);
                JwA = MathUtils.Cross(rA, u);
                mass += _mC + _mA + _iC * JwC * JwC + _iA * JwA * JwA;

                var pC = _localAnchorC - _lcC;
                var pA = MathUtils.MulT(qC, rA + (cA - cC));
                coordinateA = FVector2.Dot(pA - pC, _localAxisC);
            }

            if (_typeB == VJointType.Revolute)
            {
                JvBD = FVector2.zero;
                JwB = _ratio;
                JwD = _ratio;
                mass += _ratio * _ratio * (_iB + _iD);

                coordinateB = aB - aD - _referenceAngleB;
            }
            else
            {
                var u = MathUtils.Mul(qD, _localAxisD);
                var rD = MathUtils.Mul(qD, _localAnchorD - _lcD);
                var rB = MathUtils.Mul(qB, _localAnchorB - _lcB);
                JvBD = _ratio * u;
                JwD = _ratio * MathUtils.Cross(rD, u);
                JwB = _ratio * MathUtils.Cross(rB, u);
                mass += _ratio * _ratio * (_mD + _mB) + _iD * JwD * JwD + _iB * JwB * JwB;

                var pD = _localAnchorD - _lcD;
                var pB = MathUtils.MulT(qD, rB + (cB - cD));
                coordinateB = FVector2.Dot(pB - pD, _localAxisD);
            }

            var C = coordinateA + _ratio * coordinateB - _constant;

            var impulse = Fix64.Zero;
            if (mass > Fix64.Zero) impulse = -C / mass;

            cA += _mA * impulse * JvAC;
            aA += _iA * impulse * JwA;
            cB += _mB * impulse * JvBD;
            aB += _iB * impulse * JwB;
            cC -= _mC * impulse * JvAC;
            aC -= _iC * impulse * JwC;
            cD -= _mD * impulse * JvBD;
            aD -= _iD * impulse * JwD;

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;
            data.Positions[_indexC].C = cC;
            data.Positions[_indexC].A = aC;
            data.Positions[_indexD].C = cD;
            data.Positions[_indexD].A = aD;

            // TODO_ERIN not implemented
            return linearError < Settings.LinearSlop;
        }
    }
}