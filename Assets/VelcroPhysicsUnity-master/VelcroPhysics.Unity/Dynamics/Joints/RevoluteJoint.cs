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
using FixMath.NET;

namespace VelcroPhysics.Dynamics.VJoints
{
    /// <summary>
    /// A revolute VJoint constrains to bodies to share a common point while they
    /// are free to rotate about the point. The relative rotation about the shared
    /// point is the VJoint angle. You can limit the relative rotation with
    /// a VJoint limit that specifies a lower and upper angle. You can use a motor
    /// to drive the relative rotation about the shared point. A maximum motor torque
    /// is provided so that infinite forces are not generated.
    /// </summary>
    public class RevoluteVJoint : VJoint
    {
        private bool _enableLimit;

        private bool _enableMotor;

        // Solver shared
        private FVector3 _impulse;

        // Solver temp
        private int _indexA;

        private int _indexB;
        private Fix64 _invIA;
        private Fix64 _invIB;
        private Fix64 _invMassA;
        private Fix64 _invMassB;
        private LimitState _limitState;
        private FVector2 _localCenterA;
        private FVector2 _localCenterB;
        private Fix64 _lowerAngle;
        private Mat33 _mass; // effective mass for point-to-point constraint.
        private Fix64 _maxMotorTorque;
        private Fix64 _motorImpulse;
        private Fix64 _motorMass; // effective mass for motor/limit angular constraint.
        private Fix64 _motorSpeed;
        private FVector2 _rA;
        private FVector2 _rB;
        private Fix64 _referenceAngle;
        private Fix64 _upperAngle;

        internal RevoluteVJoint()
        {
            VJointType = VJointType.Revolute;
        }

        /// <summary>
        /// Constructor of RevoluteVJoint.
        /// </summary>
        /// <param name="bodyA">The first body.</param>
        /// <param name="bodyB">The second body.</param>
        /// <param name="anchorA">The first body anchor.</param>
        /// <param name="anchorB">The second anchor.</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public RevoluteVJoint(Body bodyA, Body bodyB, FVector2 anchorA, FVector2 anchorB, bool useWorldCoordinates = false)
            : base(bodyA, bodyB)
        {
            VJointType = VJointType.Revolute;

            if (useWorldCoordinates)
            {
                LocalAnchorA = BodyA.GetLocalPoint(anchorA);
                LocalAnchorB = BodyB.GetLocalPoint(anchorB);
            }
            else
            {
                LocalAnchorA = anchorA;
                LocalAnchorB = anchorB;
            }

            ReferenceAngle = BodyB.Rotation - BodyA.Rotation;

            _impulse = FVector3.zero;
            _limitState = LimitState.Inactive;
        }

        /// <summary>
        /// Constructor of RevoluteVJoint.
        /// </summary>
        /// <param name="bodyA">The first body.</param>
        /// <param name="bodyB">The second body.</param>
        /// <param name="anchor">The shared anchor.</param>
        /// <param name="useWorldCoordinates"></param>
        public RevoluteVJoint(Body bodyA, Body bodyB, FVector2 anchor, bool useWorldCoordinates = false)
            : this(bodyA, bodyB, anchor, anchor, useWorldCoordinates)
        {
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
        /// The referance angle computed as BodyB angle minus BodyA angle.
        /// </summary>
        public Fix64 ReferenceAngle
        {
            get => _referenceAngle;
            set
            {
                WakeBodies();
                _referenceAngle = value;
            }
        }

        /// <summary>
        /// Get the current VJoint angle in radians.
        /// </summary>
        public Fix64 VJointAngle => BodyB._sweep.A - BodyA._sweep.A - ReferenceAngle;

        /// <summary>
        /// Get the current VJoint angle speed in radians per second.
        /// </summary>
        public Fix64 VJointSpeed => BodyB._angularVelocity - BodyA._angularVelocity;

        /// <summary>
        /// Is the VJoint limit enabled?
        /// </summary>
        /// <value><c>true</c> if [limit enabled]; otherwise, <c>false</c>.</value>
        public bool LimitEnabled
        {
            get => _enableLimit;
            set
            {
                if (_enableLimit == value)
                    return;

                WakeBodies();
                _enableLimit = value;
                _impulse.z = Fix64.Zero;
            }
        }

        /// <summary>
        /// Get the lower VJoint limit in radians.
        /// </summary>
        public Fix64 LowerLimit
        {
            get => _lowerAngle;
            set
            {
                if (_lowerAngle == value)
                    return;

                WakeBodies();
                _lowerAngle = value;
                _impulse.z = Fix64.Zero;
            }
        }

        /// <summary>
        /// Get the upper VJoint limit in radians.
        /// </summary>
        public Fix64 UpperLimit
        {
            get => _upperAngle;
            set
            {
                if (_upperAngle == value)
                    return;

                WakeBodies();
                _upperAngle = value;
                _impulse.z = Fix64.Zero;
            }
        }

        /// <summary>
        /// Is the VJoint motor enabled?
        /// </summary>
        /// <value><c>true</c> if [motor enabled]; otherwise, <c>false</c>.</value>
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
        /// Get or set the motor speed in radians per second.
        /// </summary>
        public Fix64 MotorSpeed
        {
            set
            {
                if (value == _motorSpeed)
                    return;

                WakeBodies();
                _motorSpeed = value;
            }
            get => _motorSpeed;
        }

        /// <summary>
        /// Get or set the maximum motor torque, usually in N-m.
        /// </summary>
        public Fix64 MaxMotorTorque
        {
            set
            {
                if (value == _maxMotorTorque)
                    return;

                WakeBodies();
                _maxMotorTorque = value;
            }
            get => _maxMotorTorque;
        }

        /// <summary>
        /// Get or set the current motor impulse, usually in N-m.
        /// </summary>
        public Fix64 MotorImpulse
        {
            get => _motorImpulse;
            set
            {
                if (value == _motorImpulse)
                    return;

                WakeBodies();
                _motorImpulse = value;
            }
        }

        /// <summary>
        /// Set the VJoint limits, usually in meters.
        /// </summary>
        /// <param name="lower">The lower limit</param>
        /// <param name="upper">The upper limit</param>
        public void SetLimits(Fix64 lower, Fix64 upper)
        {
            if (lower == _lowerAngle && upper == _upperAngle)
                return;

            WakeBodies();
            _upperAngle = upper;
            _lowerAngle = lower;
            _impulse.z = Fix64.Zero;
        }

        /// <summary>
        /// Gets the motor torque in N-m.
        /// </summary>
        /// <param name="invDt">The inverse delta time</param>
        public Fix64 GetMotorTorque(Fix64 invDt)
        {
            return invDt * _motorImpulse;
        }

        public override FVector2 GetReactionForce(Fix64 invDt)
        {
            var p = new FVector2(_impulse.x, _impulse.y);
            return invDt * p;
        }

        public override Fix64 GetReactionTorque(Fix64 invDt)
        {
            return invDt * _impulse.z;
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

            _rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            _rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
            //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

            Fix64 mA = _invMassA, mB = _invMassB;
            Fix64 iA = _invIA, iB = _invIB;

            var fixedRotation = iA + iB == Fix64.Zero;

            _mass.ex.x = mA + mB + _rA.y * _rA.y * iA + _rB.y * _rB.y * iB;
            _mass.ey.x = -_rA.y * _rA.x * iA - _rB.y * _rB.x * iB;
            _mass.ez.x = -_rA.y * iA - _rB.y * iB;
            _mass.ex.y = _mass.ey.x;
            _mass.ey.y = mA + mB + _rA.x * _rA.x * iA + _rB.x * _rB.x * iB;
            _mass.ez.y = _rA.x * iA + _rB.x * iB;
            _mass.ex.z = _mass.ez.x;
            _mass.ey.z = _mass.ez.y;
            _mass.ez.z = iA + iB;

            _motorMass = iA + iB;
            if (_motorMass > Fix64.Zero) _motorMass = Fix64.One / _motorMass;

            if (_enableMotor == false || fixedRotation) _motorImpulse = Fix64.Zero;

            if (_enableLimit && fixedRotation == false)
            {
                var VJointAngle = aB - aA - ReferenceAngle;
                if (Fix64.Abs(_upperAngle - _lowerAngle) < 2 * Settings.AngularSlop)
                {
                    _limitState = LimitState.Equal;
                }
                else if (VJointAngle <= _lowerAngle)
                {
                    if (_limitState != LimitState.AtLower) _impulse.z = Fix64.Zero;
                    _limitState = LimitState.AtLower;
                }
                else if (VJointAngle >= _upperAngle)
                {
                    if (_limitState != LimitState.AtUpper) _impulse.z = Fix64.Zero;
                    _limitState = LimitState.AtUpper;
                }
                else
                {
                    _limitState = LimitState.Inactive;
                    _impulse.z = Fix64.Zero;
                }
            }
            else
            {
                _limitState = LimitState.Inactive;
            }

            if (Settings.EnableWarmstarting)
            {
                // Scale impulses to support a variable time step.
                _impulse *= data.Step.dtRatio;
                _motorImpulse *= data.Step.dtRatio;

                var P = new FVector2(_impulse.x, _impulse.y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(_rA, P) + MotorImpulse + _impulse.z);

                vB += mB * P;
                wB += iB * (MathUtils.Cross(_rB, P) + MotorImpulse + _impulse.z);
            }
            else
            {
                _impulse = FVector3.zero;
                _motorImpulse = Fix64.Zero;
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

            Fix64 mA = _invMassA, mB = _invMassB;
            Fix64 iA = _invIA, iB = _invIB;

            var fixedRotation = iA + iB == Fix64.Zero;

            // Solve motor constraint.
            if (_enableMotor && _limitState != LimitState.Equal && fixedRotation == false)
            {
                var Cdot = wB - wA - _motorSpeed;
                var impulse = _motorMass * -Cdot;
                var oldImpulse = _motorImpulse;
                var maxImpulse = data.Step.dt * _maxMotorTorque;
                _motorImpulse = MathUtils.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve limit constraint.
            if (_enableLimit && _limitState != LimitState.Inactive && fixedRotation == false)
            {
                var Cdot1 = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);
                var Cdot2 = wB - wA;
                var Cdot = new FVector3(Cdot1.x, Cdot1.y, Cdot2);

                var impulse = -_mass.Solve33(Cdot);

                if (_limitState == LimitState.Equal)
                {
                    _impulse += impulse;
                }
                else if (_limitState == LimitState.AtLower)
                {
                    var newImpulse = _impulse.z + impulse.z;
                    if (newImpulse < Fix64.Zero)
                    {
                        var rhs = -Cdot1 + _impulse.z * new FVector2(_mass.ez.x, _mass.ez.y);
                        var reduced = _mass.Solve22(rhs);
                        impulse.x = reduced.x;
                        impulse.y = reduced.y;
                        impulse.z = -_impulse.z;
                        _impulse.x += reduced.x;
                        _impulse.y += reduced.y;
                        _impulse.z = Fix64.Zero;
                    }
                    else
                    {
                        _impulse += impulse;
                    }
                }
                else if (_limitState == LimitState.AtUpper)
                {
                    var newImpulse = _impulse.z + impulse.z;
                    if (newImpulse > Fix64.Zero)
                    {
                        var rhs = -Cdot1 + _impulse.z * new FVector2(_mass.ez.x, _mass.ez.y);
                        var reduced = _mass.Solve22(rhs);
                        impulse.x = reduced.x;
                        impulse.y = reduced.y;
                        impulse.z = -_impulse.z;
                        _impulse.x += reduced.x;
                        _impulse.y += reduced.y;
                        _impulse.z = Fix64.Zero;
                    }
                    else
                    {
                        _impulse += impulse;
                    }
                }

                var P = new FVector2(impulse.x, impulse.y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(_rA, P) + impulse.z);

                vB += mB * P;
                wB += iB * (MathUtils.Cross(_rB, P) + impulse.z);
            }
            else
            {
                // Solve point-to-point constraint
                var Cdot = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);
                var impulse = _mass.Solve22(-Cdot);

                _impulse.x += impulse.x;
                _impulse.y += impulse.y;

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
            var cA = data.Positions[_indexA].C;
            var aA = data.Positions[_indexA].A;
            var cB = data.Positions[_indexB].C;
            var aB = data.Positions[_indexB].A;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            var angularError = Fix64.Zero;
            Fix64 positionError;

            var fixedRotation = _invIA + _invIB == Fix64.Zero;

            // Solve angular limit constraint.
            if (_enableLimit && _limitState != LimitState.Inactive && fixedRotation == false)
            {
                var angle = aB - aA - ReferenceAngle;
                var limitImpulse = Fix64.Zero;

                if (_limitState == LimitState.Equal)
                {
                    // Prevent large angular corrections
                    var C = MathUtils.Clamp(angle - _lowerAngle, -Settings.MaxAngularCorrection,
                        Settings.MaxAngularCorrection);
                    limitImpulse = -_motorMass * C;
                    angularError = Fix64.Abs(C);
                }
                else if (_limitState == LimitState.AtLower)
                {
                    var C = angle - _lowerAngle;
                    angularError = -C;

                    // Prevent large angular corrections and allow some slop.
                    C = MathUtils.Clamp(C + Settings.AngularSlop, -Settings.MaxAngularCorrection, Fix64.Zero);
                    limitImpulse = -_motorMass * C;
                }
                else if (_limitState == LimitState.AtUpper)
                {
                    var C = angle - _upperAngle;
                    angularError = C;

                    // Prevent large angular corrections and allow some slop.
                    C = MathUtils.Clamp(C - Settings.AngularSlop, Fix64.Zero, Settings.MaxAngularCorrection);
                    limitImpulse = -_motorMass * C;
                }

                aA -= _invIA * limitImpulse;
                aB += _invIB * limitImpulse;
            }

            // Solve point-to-point constraint.
            {
                qA.Set(aA);
                qB.Set(aB);
                var rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
                var rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);

                var C = cB + rB - cA - rA;
                positionError = C.magnitude;

                Fix64 mA = _invMassA, mB = _invMassB;
                Fix64 iA = _invIA, iB = _invIB;

                var K = new Mat22();
                var Kexx = mA + mB + iA * rA.y * rA.y + iB * rB.y * rB.y;
                var Kexy = -iA * rA.x * rA.y - iB * rB.x * rB.y;
                var Keyx = K.ex.y;
                var Keyy = mA + mB + iA * rA.x * rA.x + iB * rB.x * rB.x;

                K.ex = new FVector2(Kexx, Kexy);
                K.ey = new FVector2(Keyx, Keyy);


                var impulse = -K.Solve(C);

                cA -= mA * impulse;
                aA -= iA * MathUtils.Cross(rA, impulse);

                cB += mB * impulse;
                aB += iB * MathUtils.Cross(rB, impulse);
            }

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }
    }
}