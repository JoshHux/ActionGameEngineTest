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
    // d = p2 - p1 = x2 + r2 - x1 - r1
    // C = dot(perp, d)
    // Cdot = dot(d, cross(w1, perp)) + dot(perp, v2 + cross(w2, r2) - v1 - cross(w1, r1))
    //      = -dot(perp, v1) - dot(cross(d + r1, perp), w1) + dot(perp, v2) + dot(cross(r2, perp), v2)
    // J = [-perp, -cross(d + r1, perp), perp, cross(r2,perp)]
    //
    // Angular constraint
    // C = a2 - a1 + a_initial
    // Cdot = w2 - w1
    // J = [0 0 -1 0 0 1]
    //
    // K = J * invM * JT
    //
    // J = [-a -s1 a s2]
    //     [0  -1  0  1]
    // a = perp
    // s1 = cross(d + r1, a) = cross(p2 - x1, a)
    // s2 = cross(r2, a) = cross(p2 - x2, a)
    // Motor/Limit linear constraint
    // C = dot(ax1, d)
    // Cdot = = -dot(ax1, v1) - dot(cross(d + r1, ax1), w1) + dot(ax1, v2) + dot(cross(r2, ax1), v2)
    // J = [-ax1 -cross(d+r1,ax1) ax1 cross(r2,ax1)]
    // Block Solver
    // We develop a block solver that includes the VJoint limit. This makes the limit stiff (inelastic) even
    // when the mass has poor distribution (leading to large torques about the VJoint anchor points).
    //
    // The Jacobian has 3 rows:
    // J = [-uT -s1 uT s2] // linear
    //     [0   -1   0  1] // angular
    //     [-vT -a1 vT a2] // limit
    //
    // u = perp
    // v = axis
    // s1 = cross(d + r1, u), s2 = cross(r2, u)
    // a1 = cross(d + r1, v), a2 = cross(r2, v)
    // M * (v2 - v1) = JT * df
    // J * v2 = bias
    //
    // v2 = v1 + invM * JT * df
    // J * (v1 + invM * JT * df) = bias
    // K * df = bias - J * v1 = -Cdot
    // K = J * invM * JT
    // Cdot = J * v1 - bias
    //
    // Now solve for f2.
    // df = f2 - f1
    // K * (f2 - f1) = -Cdot
    // f2 = invK * (-Cdot) + f1
    //
    // Clamp accumulated limit impulse.
    // lower: f2(3) = max(f2(3), 0)
    // upper: f2(3) = min(f2(3), 0)
    //
    // Solve for correct f2(1:2)
    // K(1:2, 1:2) * f2(1:2) = -Cdot(1:2) - K(1:2,3) * f2(3) + K(1:2,1:3) * f1
    //                       = -Cdot(1:2) - K(1:2,3) * f2(3) + K(1:2,1:2) * f1(1:2) + K(1:2,3) * f1(3)
    // K(1:2, 1:2) * f2(1:2) = -Cdot(1:2) - K(1:2,3) * (f2(3) - f1(3)) + K(1:2,1:2) * f1(1:2)
    // f2(1:2) = invK(1:2,1:2) * (-Cdot(1:2) - K(1:2,3) * (f2(3) - f1(3))) + f1(1:2)
    //
    // Now compute impulse to be applied:
    // df = f2 - f1

    /// <summary>
    /// A prismatic VJoint. This VJoint provides one degree of freedom: translation
    /// along an axis fixed in bodyA. Relative rotation is prevented. You can
    /// use a VJoint limit to restrict the range of motion and a VJoint motor to
    /// drive the motion or to model VJoint friction.
    /// </summary>
    public class PrismaticVJoint : VJoint
    {
        private Fix64 _a1, _a2;
        private FVector2 _axis, _perp;
        private FVector2 _axis1;
        private bool _enableLimit;
        private bool _enableMotor;
        private FVector3 _impulse;

        // Solver temp
        private int _indexA;

        private int _indexB;
        private Fix64 _invIA;
        private Fix64 _invIB;
        private Fix64 _invMassA;
        private Fix64 _invMassB;
        private Mat33 _K;
        private LimitState _limitState;
        private FVector2 _localCenterA;
        private FVector2 _localCenterB;
        private FVector2 _localYAxisA;
        private Fix64 _lowerTranslation;
        private Fix64 _maxMotorForce;
        private Fix64 _motorMass;
        private Fix64 _motorSpeed;
        private Fix64 _s1, _s2;
        private Fix64 _upperTranslation;

        internal PrismaticVJoint()
        {
            VJointType = VJointType.Prismatic;
        }

        /// <summary>
        /// This requires defining a line of
        /// motion using an axis and an anchor point. The definition uses local
        /// anchor points and a local axis so that the initial configuration
        /// can violate the constraint slightly. The VJoint translation is zero
        /// when the local anchor points coincide in world space. Using local
        /// anchors and a local axis helps when saving and loading a game.
        /// </summary>
        /// <param name="bodyA">The first body.</param>
        /// <param name="bodyB">The second body.</param>
        /// <param name="anchorA">The first body anchor.</param>
        /// <param name="anchorB">The second body anchor.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public PrismaticVJoint(Body bodyA, Body bodyB, FVector2 anchorA, FVector2 anchorB, FVector2 axis,
            bool useWorldCoordinates = false)
            : base(bodyA, bodyB)
        {
            Initialize(anchorA, anchorB, axis, useWorldCoordinates);
        }

        public PrismaticVJoint(Body bodyA, Body bodyB, FVector2 anchor, FVector2 axis, bool useWorldCoordinates = false)
            : base(bodyA, bodyB)
        {
            Initialize(anchor, anchor, axis, useWorldCoordinates);
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
        /// Get the current VJoint translation, usually in meters.
        /// </summary>
        /// <value></value>
        public Fix64 VJointTranslation
        {
            get
            {
                var d = BodyB.GetWorldPoint(LocalAnchorB) - BodyA.GetWorldPoint(LocalAnchorA);
                var axis = BodyA.GetWorldVector(LocalXAxis);

                return FVector2.Dot(d, axis);
            }
        }

        /// <summary>
        /// Get the current VJoint translation speed, usually in meters per second.
        /// </summary>
        /// <value></value>
        public Fix64 VJointSpeed
        {
            get
            {
                VTransform xf1, xf2;
                BodyA.GetVTransform(out xf1);
                BodyB.GetVTransform(out xf2);

                var r1 = MathUtils.Mul(ref xf1.q, LocalAnchorA - BodyA.LocalCenter);
                var r2 = MathUtils.Mul(ref xf2.q, LocalAnchorB - BodyB.LocalCenter);
                var p1 = BodyA._sweep.C + r1;
                var p2 = BodyB._sweep.C + r2;
                var d = p2 - p1;
                var axis = BodyA.GetWorldVector(LocalXAxis);

                var v1 = BodyA._linearVelocity;
                var v2 = BodyB._linearVelocity;
                var w1 = BodyA._angularVelocity;
                var w2 = BodyB._angularVelocity;

                var speed = FVector2.Dot(d, MathUtils.Cross(w1, axis)) + FVector2.Dot(axis,
                    v2 + MathUtils.Cross(w2, r2) - v1 - MathUtils.Cross(w1, r1));
                return speed;
            }
        }

        /// <summary>
        /// Is the VJoint limit enabled?
        /// </summary>
        /// <value><c>true</c> if [limit enabled]; otherwise, <c>false</c>.</value>
        public bool LimitEnabled
        {
            get => _enableLimit;
            set
            {
                UnityEngine.Debug.Assert(BodyA.FixedRotation == false || BodyB.FixedRotation == false,
                    "Warning: limits does currently not work with fixed rotation");

                if (value == _enableLimit)
                    return;

                WakeBodies();
                _enableLimit = value;
                _impulse.z = 0;
            }
        }

        /// <summary>
        /// Get the lower VJoint limit, usually in meters.
        /// </summary>
        /// <value></value>
        public Fix64 LowerLimit
        {
            get => _lowerTranslation;
            set
            {
                if (value == _lowerTranslation)
                    return;

                WakeBodies();
                _lowerTranslation = value;
                _impulse.z = Fix64.Zero;
            }
        }

        /// <summary>
        /// Get the upper VJoint limit, usually in meters.
        /// </summary>
        /// <value></value>
        public Fix64 UpperLimit
        {
            get => _upperTranslation;
            set
            {
                if (value == _upperTranslation)
                    return;

                WakeBodies();
                _upperTranslation = value;
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
        /// Set the motor speed, usually in meters per second.
        /// </summary>
        /// <value>The speed.</value>
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
        /// Set the maximum motor force, usually in N.
        /// </summary>
        /// <value>The force.</value>
        public Fix64 MaxMotorForce
        {
            get => _maxMotorForce;
            set
            {
                if (value == _maxMotorForce)
                    return;

                WakeBodies();
                _maxMotorForce = value;
            }
        }

        /// <summary>
        /// Get the current motor impulse, usually in N.
        /// </summary>
        /// <value></value>
        public Fix64 MotorImpulse { get; set; }

        /// <summary>
        /// The axis at which the VJoint moves.
        /// </summary>
        public FVector2 Axis
        {
            get => _axis1;
            set
            {
                _axis1 = value;
                LocalXAxis = BodyA.GetLocalVector(_axis1);
                LocalXAxis.Normalize();
                _localYAxisA = MathUtils.Cross(1, LocalXAxis);
            }
        }

        /// <summary>
        /// The axis in local coordinates relative to BodyA
        /// </summary>
        public FVector2 LocalXAxis { get; private set; }

        /// <summary>
        /// The reference angle.
        /// </summary>
        public Fix64 ReferenceAngle { get; set; }

        private void Initialize(FVector2 localAnchorA, FVector2 localAnchorB, FVector2 axis, bool useWorldCoordinates)
        {
            VJointType = VJointType.Prismatic;

            if (useWorldCoordinates)
            {
                LocalAnchorA = BodyA.GetLocalPoint(localAnchorA);
                LocalAnchorB = BodyB.GetLocalPoint(localAnchorB);
            }
            else
            {
                LocalAnchorA = localAnchorA;
                LocalAnchorB = localAnchorB;
            }

            Axis = axis; //Velcro only: store the orignal value for use in Serialization
            ReferenceAngle = BodyB.Rotation - BodyA.Rotation;

            _limitState = LimitState.Inactive;
        }

        /// <summary>
        /// Set the VJoint limits, usually in meters.
        /// </summary>
        /// <param name="lower">The lower limit</param>
        /// <param name="upper">The upper limit</param>
        public void SetLimits(Fix64 lower, Fix64 upper)
        {
            if (upper == _upperTranslation && lower == _lowerTranslation)
                return;

            WakeBodies();
            _upperTranslation = upper;
            _lowerTranslation = lower;
            _impulse.z = Fix64.Zero;
        }

        /// <summary>
        /// Gets the motor force.
        /// </summary>
        /// <param name="invDt">The inverse delta time</param>
        public Fix64 GetMotorForce(Fix64 invDt)
        {
            return invDt * MotorImpulse;
        }

        public override FVector2 GetReactionForce(Fix64 invDt)
        {
            return invDt * (_impulse.x * _perp + (MotorImpulse + _impulse.z) * _axis);
        }

        public override Fix64 GetReactionTorque(Fix64 invDt)
        {
            return invDt * _impulse.y;
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
            var d = cB - cA + rB - rA;

            Fix64 mA = _invMassA, mB = _invMassB;
            Fix64 iA = _invIA, iB = _invIB;

            // Compute motor Jacobian and effective mass.
            {
                _axis = MathUtils.Mul(qA, LocalXAxis);
                _a1 = MathUtils.Cross(d + rA, _axis);
                _a2 = MathUtils.Cross(rB, _axis);

                _motorMass = mA + mB + iA * _a1 * _a1 + iB * _a2 * _a2;
                if (_motorMass > Fix64.Zero) _motorMass = Fix64.One / _motorMass;
            }

            // Prismatic constraint.
            {
                _perp = MathUtils.Mul(qA, _localYAxisA);

                _s1 = MathUtils.Cross(d + rA, _perp);
                _s2 = MathUtils.Cross(rB, _perp);

                var k11 = mA + mB + iA * _s1 * _s1 + iB * _s2 * _s2;
                var k12 = iA * _s1 + iB * _s2;
                var k13 = iA * _s1 * _a1 + iB * _s2 * _a2;
                var k22 = iA + iB;
                if (k22 == Fix64.Zero)
                    // For bodies with fixed rotation.
                    k22 = Fix64.One;
                var k23 = iA * _a1 + iB * _a2;
                var k33 = mA + mB + iA * _a1 * _a1 + iB * _a2 * _a2;

                _K.ex = new FVector3(k11, k12, k13);
                _K.ey = new FVector3(k12, k22, k23);
                _K.ez = new FVector3(k13, k23, k33);
            }

            // Compute motor and limit terms.
            if (_enableLimit)
            {
                var VJointTranslation = FVector2.Dot(_axis, d);
                if (Fix64.Abs(_upperTranslation - _lowerTranslation) < 2 * Settings.LinearSlop)
                {
                    _limitState = LimitState.Equal;
                }
                else if (VJointTranslation <= _lowerTranslation)
                {
                    if (_limitState != LimitState.AtLower)
                    {
                        _limitState = LimitState.AtLower;
                        _impulse.z = Fix64.Zero;
                    }
                }
                else if (VJointTranslation >= _upperTranslation)
                {
                    if (_limitState != LimitState.AtUpper)
                    {
                        _limitState = LimitState.AtUpper;
                        _impulse.z = Fix64.Zero;
                    }
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
                _impulse.z = Fix64.Zero;
            }

            if (_enableMotor == false) MotorImpulse = Fix64.Zero;

            if (Settings.EnableWarmstarting)
            {
                // Account for variable time step.
                _impulse *= data.Step.dtRatio;
                MotorImpulse *= data.Step.dtRatio;

                var P = _impulse.x * _perp + (MotorImpulse + _impulse.z) * _axis;
                var LA = _impulse.x * _s1 + _impulse.y + (MotorImpulse + _impulse.z) * _a1;
                var LB = _impulse.x * _s2 + _impulse.y + (MotorImpulse + _impulse.z) * _a2;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }
            else
            {
                _impulse = FVector3.zero;
                MotorImpulse = Fix64.Zero;
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

            // Solve linear motor constraint.
            if (_enableMotor && _limitState != LimitState.Equal)
            {
                var Cdot = FVector2.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                var impulse = _motorMass * (_motorSpeed - Cdot);
                var oldImpulse = MotorImpulse;
                var maxImpulse = data.Step.dt * _maxMotorForce;
                MotorImpulse = MathUtils.Clamp(MotorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = MotorImpulse - oldImpulse;

                var P = impulse * _axis;
                var LA = impulse * _a1;
                var LB = impulse * _a2;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }

            var cdotX = FVector2.Dot(_perp, vB - vA) + _s2 * wB - _s1 * wA;
            var cdotY = wB - wA;
            var Cdot1 = new FVector2(cdotX, cdotY);

            if (_enableLimit && _limitState != LimitState.Inactive)
            {
                // Solve prismatic and limit constraint in block form.
                Fix64 Cdot2;
                Cdot2 = FVector2.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                var Cdot = new FVector3(Cdot1.x, Cdot1.y, Cdot2);

                var f1 = _impulse;
                var df = _K.Solve33(-Cdot);
                _impulse += df;

                if (_limitState == LimitState.AtLower)
                    _impulse.z = Fix64.Max(_impulse.z, Fix64.Zero);
                else if (_limitState == LimitState.AtUpper) _impulse.z = Fix64.Min(_impulse.z, Fix64.Zero);

                // f2(1:2) = invK(1:2,1:2) * (-Cdot(1:2) - K(1:2,3) * (f2(3) - f1(3))) + f1(1:2)
                var b = -Cdot1 - (_impulse.z - f1.z) * new FVector2(_K.ez.x, _K.ez.y);
                var f2r = _K.Solve22(b) + new FVector2(f1.x, f1.y);
                _impulse.x = f2r.x;
                _impulse.y = f2r.y;

                df = _impulse - f1;

                var P = df.x * _perp + df.z * _axis;
                var LA = df.x * _s1 + df.y + df.z * _a1;
                var LB = df.x * _s2 + df.y + df.z * _a2;

                vA -= mA * P;
                wA -= iA * LA;

                vB += mB * P;
                wB += iB * LB;
            }
            else
            {
                // Limit is inactive, just solve the prismatic constraint in block form.
                var df = _K.Solve22(-Cdot1);
                _impulse.x += df.x;
                _impulse.y += df.y;

                var P = df.x * _perp;
                var LA = df.x * _s1 + df.y;
                var LB = df.x * _s2 + df.y;

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

            Fix64 mA = _invMassA, mB = _invMassB;
            Fix64 iA = _invIA, iB = _invIB;

            // Compute fresh Jacobians
            var rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            var rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
            var d = cB + rB - cA - rA;

            var axis = MathUtils.Mul(qA, LocalXAxis);
            var a1 = MathUtils.Cross(d + rA, axis);
            var a2 = MathUtils.Cross(rB, axis);
            var perp = MathUtils.Mul(qA, _localYAxisA);

            var s1 = MathUtils.Cross(d + rA, perp);
            var s2 = MathUtils.Cross(rB, perp);

            FVector3 impulse;
            var C1x = FVector2.Dot(perp, d);
            var C1y = aB - aA - ReferenceAngle;
            var C1 = new FVector2(C1x, C1y);

            var linearError = Fix64.Abs(C1.x);
            var angularError = Fix64.Abs(C1.y);

            var active = false;
            var C2 = Fix64.Zero;
            if (_enableLimit)
            {
                var translation = FVector2.Dot(axis, d);
                if (Fix64.Abs(_upperTranslation - _lowerTranslation) < 2 * Settings.LinearSlop)
                {
                    // Prevent large angular corrections
                    C2 = MathUtils.Clamp(translation, -Settings.MaxLinearCorrection, Settings.MaxLinearCorrection);
                    linearError = Fix64.Max(linearError, Fix64.Abs(translation));
                    active = true;
                }
                else if (translation <= _lowerTranslation)
                {
                    // Prevent large linear corrections and allow some slop.
                    C2 = MathUtils.Clamp(translation - _lowerTranslation + Settings.LinearSlop,
                        -Settings.MaxLinearCorrection, Fix64.Zero);
                    linearError = Fix64.Max(linearError, _lowerTranslation - translation);
                    active = true;
                }
                else if (translation >= _upperTranslation)
                {
                    // Prevent large linear corrections and allow some slop.
                    C2 = MathUtils.Clamp(translation - _upperTranslation - Settings.LinearSlop, Fix64.Zero,
                        Settings.MaxLinearCorrection);
                    linearError = Fix64.Max(linearError, translation - _upperTranslation);
                    active = true;
                }
            }

            if (active)
            {
                var k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
                var k12 = iA * s1 + iB * s2;
                var k13 = iA * s1 * a1 + iB * s2 * a2;
                var k22 = iA + iB;
                if (k22 == Fix64.Zero)
                    // For fixed rotation
                    k22 = Fix64.One;
                var k23 = iA * a1 + iB * a2;
                var k33 = mA + mB + iA * a1 * a1 + iB * a2 * a2;

                var K = new Mat33();
                K.ex = new FVector3(k11, k12, k13);
                K.ey = new FVector3(k12, k22, k23);
                K.ez = new FVector3(k13, k23, k33);

                var C = new FVector3();
                C.x = C1.x;
                C.y = C1.y;
                C.z = C2;

                impulse = K.Solve33(-C);
            }
            else
            {
                var k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
                var k12 = iA * s1 + iB * s2;
                var k22 = iA + iB;
                if (k22 == Fix64.Zero) k22 = Fix64.One;

                var K = new Mat22();
                K.ex = new FVector2(k11, k12);
                K.ey = new FVector2(k12, k22);

                var impulse1 = K.Solve(-C1);
                impulse = new FVector3();
                impulse.x = impulse1.x;
                impulse.y = impulse1.y;
                impulse.z = Fix64.Zero;
            }

            var P = impulse.x * perp + impulse.z * axis;
            var LA = impulse.x * s1 + impulse.y + impulse.z * a1;
            var LB = impulse.x * s2 + impulse.y + impulse.z * a2;

            cA -= mA * P;
            aA -= iA * LA;
            cB += mB * P;
            aB += iB * LB;

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return linearError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }
    }
}