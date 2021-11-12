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
    // Linear constraint (point-to-line)
    // d = pB - pA = xB + rB - xA - rA
    // C = dot(ay, d)
    // Cdot = dot(d, cross(wA, ay)) + dot(ay, vB + cross(wB, rB) - vA - cross(wA, rA))
    //      = -dot(ay, vA) - dot(cross(d + rA, ay), wA) + dot(ay, vB) + dot(cross(rB, ay), vB)
    // J = [-ay, -cross(d + rA, ay), ay, cross(rB, ay)]

    // Spring linear constraint
    // C = dot(ax, d)
    // Cdot = = -dot(ax, vA) - dot(cross(d + rA, ax), wA) + dot(ax, vB) + dot(cross(rB, ax), vB)
    // J = [-ax -cross(d+rA, ax) ax cross(rB, ax)]

    // Motor rotational constraint
    // Cdot = wB - wA
    // J = [0 0 -1 0 0 1]

    /// <summary>
    /// A wheel VJoint. This VJoint provides two degrees of freedom: translation
    /// along an axis fixed in bodyA and rotation in the plane. You can use a
    /// VJoint limit to restrict the range of motion and a VJoint motor to drive
    /// the rotation or to model rotational friction.
    /// This VJoint is designed for vehicle suspensions.
    /// </summary>
    public class WheelVJoint : VJoint
    {
        private FVector2 _ax, _ay;
        private FVector2 _axis;

        private Fix64 _bias;
        private bool _enableMotor;
        private Fix64 _gamma;

        private Fix64 _impulse;

        // Solver temp
        private int _indexA;

        private int _indexB;
        private Fix64 _invIA;
        private Fix64 _invIB;
        private Fix64 _invMassA;
        private Fix64 _invMassB;
        private FVector2 _localCenterA;

        private FVector2 _localCenterB;

        // Solver shared
        private FVector2 _localYAxis;

        private Fix64 _mass;

        private Fix64 _maxMotorTorque;
        private Fix64 _motorImpulse;
        private Fix64 _motorMass;
        private Fix64 _motorSpeed;
        private Fix64 _sAx, _sBx;
        private Fix64 _sAy, _sBy;
        private Fix64 _springImpulse;
        private Fix64 _springMass;

        internal WheelVJoint()
        {
            VJointType = VJointType.Wheel;
        }

        /// <summary>
        /// Constructor for WheelVJoint
        /// </summary>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        /// <param name="anchor">The anchor point</param>
        /// <param name="axis">The axis</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public WheelVJoint(Body bodyA, Body bodyB, FVector2 anchor, FVector2 axis, bool useWorldCoordinates = false)
            : base(bodyA, bodyB)
        {
            VJointType = VJointType.Wheel;

            if (useWorldCoordinates)
            {
                LocalAnchorA = bodyA.GetLocalPoint(anchor);
                LocalAnchorB = bodyB.GetLocalPoint(anchor);
            }
            else
            {
                LocalAnchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(anchor));
                LocalAnchorB = anchor;
            }

            Axis = axis; //Velcro only: We maintain the original value as it is supposed to.
        }

        /// <summary>
        /// The local anchor point on BodyA
        /// </summary>
        public FVector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point on BodyB
        /// </summary>
        public FVector2 LocalAnchorB { get; set; }

        public override FVector2 WorldAnchorA
        {
            get => BodyA.GetWorldPoint(LocalAnchorA);
            set => LocalAnchorA = BodyA.GetLocalPoint(value);
        }

        public override FVector2 WorldAnchorB
        {
            get => BodyB.GetWorldPoint(LocalAnchorB);
            set => LocalAnchorB = BodyB.GetLocalPoint(value);
        }

        /// <summary>
        /// The axis at which the suspension moves.
        /// </summary>
        public FVector2 Axis
        {
            get => _axis;
            set
            {
                _axis = value;
                LocalXAxis = BodyA.GetLocalVector(_axis);
                _localYAxis = MathUtils.Cross(1, LocalXAxis);
            }
        }

        /// <summary>
        /// The axis in local coordinates relative to BodyA
        /// </summary>
        public FVector2 LocalXAxis { get; private set; }

        /// <summary>
        /// The desired motor speed in radians per second.
        /// </summary>
        public Fix64 MotorSpeed
        {
            get => _motorSpeed;
            set
            {
                if (value == _motorSpeed)
                    return;

                WakeBodies();
                _motorSpeed = value;
            }
        }

        /// <summary>
        /// The maximum motor torque, usually in N-m.
        /// </summary>
        public Fix64 MaxMotorTorque
        {
            get => _maxMotorTorque;
            set
            {
                if (value == _maxMotorTorque)
                    return;

                WakeBodies();
                _maxMotorTorque = value;
            }
        }

        /// <summary>
        /// Suspension frequency, zero indicates no suspension
        /// </summary>
        public Fix64 Frequency { get; set; }

        /// <summary>
        /// Suspension damping ratio, one indicates critical damping
        /// </summary>
        public Fix64 DampingRatio { get; set; }

        /// <summary>
        /// Gets the translation along the axis
        /// </summary>
        public Fix64 VJointTranslation
        {
            get
            {
                var bA = BodyA;
                var bB = BodyB;

                var pA = bA.GetWorldPoint(LocalAnchorA);
                var pB = bB.GetWorldPoint(LocalAnchorB);
                var d = pB - pA;
                var axis = bA.GetWorldVector(LocalXAxis);

                var translation = FVector2.Dot(d, axis);
                return translation;
            }
        }

        public Fix64 VJointLinearSpeed
        {
            get
            {
                var bA = BodyA;
                var bB = BodyB;

                VTransform xfA;
                bA.GetVTransform(out xfA);

                VTransform xfB;
                bB.GetVTransform(out xfB);

                var rA = MathUtils.Mul(xfA.q, LocalAnchorA - bA._sweep.LocalCenter);
                var rB = MathUtils.Mul(xfB.q, LocalAnchorB - bB._sweep.LocalCenter);
                var p1 = bA._sweep.C + rA;
                var p2 = bB._sweep.C + rB;
                var d = p2 - p1;
                var axis = MathUtils.Mul(xfA.q, LocalXAxis);

                var vA = bA.LinearVelocity;
                var vB = bB.LinearVelocity;
                var wA = bA.AngularVelocity;
                var wB = bB.AngularVelocity;

                var speed = FVector2.Dot(d, MathUtils.Cross(wA, axis)) + FVector2.Dot(axis,
                    vB + MathUtils.Cross(wB, rB) - vA - MathUtils.Cross(wA, rA));
                return speed;
            }
        }

        public Fix64 VJointAngle
        {
            get
            {
                var bA = BodyA;
                var bB = BodyB;
                return bB._sweep.A - bA._sweep.A;
            }
        }

        /// <summary>
        /// Gets the angular velocity of the VJoint
        /// </summary>
        public Fix64 VJointAngularSpeed
        {
            get
            {
                var wA = BodyA.AngularVelocity;
                var wB = BodyB.AngularVelocity;
                return wB - wA;
            }
        }

        /// <summary>
        /// Enable/disable the VJoint motor.
        /// </summary>
        public bool MotorEnabled
        {
            get => _enableMotor;
            set
            {
                if (value == _enableMotor)
                    return;

                WakeBodies();
                _enableMotor = value;
            }
        }

        /// <summary>
        /// Gets the torque of the motor
        /// </summary>
        /// <param name="invDt">inverse delta time</param>
        public Fix64 GetMotorTorque(Fix64 invDt)
        {
            return invDt * _motorImpulse;
        }

        public override FVector2 GetReactionForce(Fix64 invDt)
        {
            return invDt * (_impulse * _ay + _springImpulse * _ax);
        }

        public override Fix64 GetReactionTorque(Fix64 invDt)
        {
            return invDt * _motorImpulse;
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

            Fix64 mA = _invMassA, mB = _invMassB;
            Fix64 iA = _invIA, iB = _invIB;

            var cA = data.Positions[_indexA].C;
            var aA = data.Positions[_indexA].A;
            var vA = data.Velocities[_indexA].V;
            var wA = data.Velocities[_indexA].W;

            var cB = data.Positions[_indexB].C;
            var aB = data.Positions[_indexB].A;
            var vB = data.Velocities[_indexB].V;
            var wB = data.Velocities[_indexB].W;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            // Compute the effective masses.
            var rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            var rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
            var d1 = cB + rB - cA - rA;

            // Point to line constraint
            {
                _ay = MathUtils.Mul(qA, _localYAxis);
                _sAy = MathUtils.Cross(d1 + rA, _ay);
                _sBy = MathUtils.Cross(rB, _ay);

                _mass = mA + mB + iA * _sAy * _sAy + iB * _sBy * _sBy;

                if (_mass > Fix64.Zero) _mass = Fix64.One / _mass;
            }

            // Spring constraint
            _springMass = Fix64.Zero;
            _bias = Fix64.Zero;
            _gamma = Fix64.Zero;
            if (Frequency > Fix64.Zero)
            {
                _ax = MathUtils.Mul(qA, LocalXAxis);
                _sAx = MathUtils.Cross(d1 + rA, _ax);
                _sBx = MathUtils.Cross(rB, _ax);

                var invMass = mA + mB + iA * _sAx * _sAx + iB * _sBx * _sBx;

                if (invMass > Fix64.Zero)
                {
                    _springMass = Fix64.One / invMass;

                    var C = FVector2.Dot(d1, _ax);

                    // Frequency
                    var omega =2 * Fix64.Pi * Frequency;

                    // Damping coefficient
                    var d =2 * _springMass * DampingRatio * omega;

                    // Spring stiffness
                    var k = _springMass * omega * omega;

                    // magic formulas
                    var h = data.Step.dt;
                    _gamma = h * (d + h * k);
                    if (_gamma > Fix64.Zero) _gamma = Fix64.One / _gamma;

                    _bias = C * h * k * _gamma;

                    _springMass = invMass + _gamma;
                    if (_springMass > Fix64.Zero) _springMass = Fix64.One / _springMass;
                }
            }
            else
            {
                _springImpulse = Fix64.Zero;
            }

            // Rotational motor
            if (_enableMotor)
            {
                _motorMass = iA + iB;
                if (_motorMass > Fix64.Zero) _motorMass = Fix64.One / _motorMass;
            }
            else
            {
                _motorMass = Fix64.Zero;
                _motorImpulse = Fix64.Zero;
            }

            if (Settings.EnableWarmstarting)
            {
                // Account for variable time step.
                _impulse *= data.Step.dtRatio;
                _springImpulse *= data.Step.dtRatio;
                _motorImpulse *= data.Step.dtRatio;

                var P = _impulse * _ay + _springImpulse * _ax;
                var LA = _impulse * _sAy + _springImpulse * _sAx + _motorImpulse;
                var LB = _impulse * _sBy + _springImpulse * _sBx + _motorImpulse;

                vA -= _invMassA * P;
                wA -= _invIA * LA;

                vB += _invMassB * P;
                wB += _invIB * LB;
            }
            else
            {
                _impulse = Fix64.Zero;
                _springImpulse = Fix64.Zero;
                _motorImpulse = Fix64.Zero;
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Fix64 mA = _invMassA, mB = _invMassB;
            Fix64 iA = _invIA, iB = _invIB;

            var vA = data.Velocities[_indexA].V;
            var wA = data.Velocities[_indexA].W;
            var vB = data.Velocities[_indexB].V;
            var wB = data.Velocities[_indexB].W;

            // Solve spring constraint
            {
                var Cdot = FVector2.Dot(_ax, vB - vA) + _sBx * wB - _sAx * wA;
                var impulse = -_springMass * (Cdot + _bias + _gamma * _springImpulse);
                _springImpulse += impulse;

                var P = impulse * _ax;
                var LA = impulse * _sAx;
                var LB = impulse * _sBx;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }

            // Solve rotational motor constraint
            {
                var Cdot = wB - wA - _motorSpeed;
                var impulse = -_motorMass * Cdot;

                var oldImpulse = _motorImpulse;
                var maxImpulse = data.Step.dt * _maxMotorTorque;
                _motorImpulse = MathUtils.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve point to line constraint
            {
                var Cdot = FVector2.Dot(_ay, vB - vA) + _sBy * wB - _sAy * wA;
                var impulse = -_mass * Cdot;
                _impulse += impulse;

                var P = impulse * _ay;
                var LA = impulse * _sAy;
                var LB = impulse * _sBy;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            var cA = data.Positions[_indexA].C;
            var aA = data.Positions[_indexA].A;
            var cB = data.Positions[_indexB].C;
            var aB = data.Positions[_indexB].A;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            var rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            var rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
            var d = cB - cA + rB - rA;

            var ay = MathUtils.Mul(qA, _localYAxis);

            var sAy = MathUtils.Cross(d + rA, ay);
            var sBy = MathUtils.Cross(rB, ay);

            var C = FVector2.Dot(d, ay);

            var k = _invMassA + _invMassB + _invIA * _sAy * _sAy + _invIB * _sBy * _sBy;

            Fix64 impulse;
            if (k != Fix64.Zero)
                impulse = -C / k;
            else
                impulse = Fix64.Zero;

            var P = impulse * ay;
            var LA = impulse * sAy;
            var LB = impulse * sBy;

            cA -= _invMassA * P;
            aA -= _invIA * LA;
            cB += _invMassB * P;
            aB += _invIB * LB;

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return Fix64.Abs(C) <= Settings.LinearSlop;
        }
    }
}