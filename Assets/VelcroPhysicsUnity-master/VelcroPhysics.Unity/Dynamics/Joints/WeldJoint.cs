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
    // Point-to-point constraint
    // C = p2 - p1
    // Cdot = v2 - v1
    //      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
    // J = [-I -r1_skew I r2_skew ]
    // Identity used:
    // w k % (rx i + ry j) = w * (-ry i + rx j)

    // Angle constraint
    // C = angle2 - angle1 - referenceAngle
    // Cdot = w2 - w1
    // J = [0 0 -1 0 0 1]
    // K = invI1 + invI2

    /// <summary>
    /// A weld VJoint essentially glues two bodies together. A weld VJoint may
    /// distort somewhat because the island constraint solver is approximate.
    /// The VJoint is soft constraint based, which means the two bodies will move
    /// relative to each other, when a force is applied. To combine two bodies
    /// in a rigid fashion, combine the fixtures to a single body instead.
    /// </summary>
    public class WeldVJoint : VJoint
    {
        private Fix64 _bias;

        private Fix64 _gamma;

        // Solver shared
        private FVector3 _impulse;

        // Solver temp
        private int _indexA;

        private int _indexB;
        private Fix64 _invIA;
        private Fix64 _invIB;
        private Fix64 _invMassA;
        private Fix64 _invMassB;
        private FVector2 _localCenterA;
        private FVector2 _localCenterB;
        private Mat33 _mass;
        private FVector2 _rA;
        private FVector2 _rB;

        internal WeldVJoint()
        {
            VJointType = VJointType.Weld;
        }

        /// <summary>
        /// You need to specify an anchor point where they are attached.
        /// The position of the anchor point is important for computing the reaction torque.
        /// </summary>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        /// <param name="anchorA">The first body anchor.</param>
        /// <param name="anchorB">The second body anchor.</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public WeldVJoint(Body bodyA, Body bodyB, FVector2 anchorA, FVector2 anchorB, bool useWorldCoordinates = false)
            : base(bodyA, bodyB)
        {
            VJointType = VJointType.Weld;

            if (useWorldCoordinates)
            {
                LocalAnchorA = bodyA.GetLocalPoint(anchorA);
                LocalAnchorB = bodyB.GetLocalPoint(anchorB);
            }
            else
            {
                LocalAnchorA = anchorA;
                LocalAnchorB = anchorB;
            }

            ReferenceAngle = BodyB.Rotation - BodyA.Rotation;
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
        /// The bodyB angle minus bodyA angle in the reference state (radians).
        /// </summary>
        public Fix64 ReferenceAngle { get; set; }

        /// <summary>
        /// The frequency of the VJoint. A higher frequency means a stiffer VJoint, but
        /// a too high value can cause the VJoint to oscillate.
        /// Default is 0, which means the VJoint does no spring calculations.
        /// </summary>
        public Fix64 FrequencyHz { get; set; }

        /// <summary>
        /// The damping on the VJoint. The damping is only used when
        /// the VJoint has a frequency (> 0). A higher value means more damping.
        /// </summary>
        public Fix64 DampingRatio { get; set; }

        public override FVector2 GetReactionForce(Fix64 invDt)
        {
            return invDt * new FVector2(_impulse.x, _impulse.y);
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

            var K = new Mat33();
            K.ex.x = mA + mB + _rA.y * _rA.y * iA + _rB.y * _rB.y * iB;
            K.ey.x = -_rA.y * _rA.x * iA - _rB.y * _rB.x * iB;
            K.ez.x = -_rA.y * iA - _rB.y * iB;
            K.ex.y = K.ey.x;
            K.ey.y = mA + mB + _rA.x * _rA.x * iA + _rB.x * _rB.x * iB;
            K.ez.y = _rA.x * iA + _rB.x * iB;
            K.ex.z = K.ez.x;
            K.ey.z = K.ez.y;
            K.ez.z = iA + iB;

            if (FrequencyHz >Fix64.Zero)
            {
                K.GetInverse22(ref _mass);

                var invM = iA + iB;
                var m = invM >Fix64.Zero ?Fix64.One / invM :Fix64.Zero;

                var C = aB - aA - ReferenceAngle;

                // Frequency
                var omega =2 * Fix64.Pi * FrequencyHz;

                // Damping coefficient
                var d =2 * m * DampingRatio * omega;

                // Spring stiffness
                var k = m * omega * omega;

                // magic formulas
                var h = data.Step.dt;
                _gamma = h * (d + h * k);
                _gamma = _gamma !=Fix64.Zero ?Fix64.One / _gamma :Fix64.Zero;
                _bias = C * h * k * _gamma;

                invM += _gamma;
                _mass.ez.z = invM !=Fix64.Zero ?Fix64.One / invM :Fix64.Zero;
            }
            else if (K.ez.z ==Fix64.Zero)
            {
                K.GetInverse22(ref _mass);
                _gamma =Fix64.Zero;
                _bias =Fix64.Zero;
            }
            else
            {
                K.GetSymInverse33(ref _mass);
                _gamma =Fix64.Zero;
                _bias =Fix64.Zero;
            }

            if (Settings.EnableWarmstarting)
            {
                // Scale impulses to support a variable time step.
                _impulse *= data.Step.dtRatio;

                var P = new FVector2(_impulse.x, _impulse.y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(_rA, P) + _impulse.z);

                vB += mB * P;
                wB += iB * (MathUtils.Cross(_rB, P) + _impulse.z);
            }
            else
            {
                _impulse = FVector3.zero;
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

            if (FrequencyHz >Fix64.Zero)
            {
                var Cdot2 = wB - wA;

                var impulse2 = -_mass.ez.z * (Cdot2 + _bias + _gamma * _impulse.z);
                _impulse.z += impulse2;

                wA -= iA * impulse2;
                wB += iB * impulse2;

                var Cdot1 = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);

                var impulse1 = -MathUtils.Mul22(_mass, Cdot1);
                _impulse.x += impulse1.x;
                _impulse.y += impulse1.y;

                var P = impulse1;

                vA -= mA * P;
                wA -= iA * MathUtils.Cross(_rA, P);

                vB += mB * P;
                wB += iB * MathUtils.Cross(_rB, P);
            }
            else
            {
                var Cdot1 = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);
                var Cdot2 = wB - wA;
                var Cdot = new FVector3(Cdot1.x, Cdot1.y, Cdot2);

                var impulse = -MathUtils.Mul(_mass, Cdot);
                _impulse += impulse;

                var P = new FVector2(impulse.x, impulse.y);

                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(_rA, P) + impulse.z);

                vB += mB * P;
                wB += iB * (MathUtils.Cross(_rB, P) + impulse.z);
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

            var rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            var rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);

            Fix64 positionError, angularError;

            var K = new Mat33();
            K.ex.x = mA + mB + rA.y * rA.y * iA + rB.y * rB.y * iB;
            K.ey.x = -rA.y * rA.x * iA - rB.y * rB.x * iB;
            K.ez.x = -rA.y * iA - rB.y * iB;
            K.ex.y = K.ey.x;
            K.ey.y = mA + mB + rA.x * rA.x * iA + rB.x * rB.x * iB;
            K.ez.y = rA.x * iA + rB.x * iB;
            K.ex.z = K.ez.x;
            K.ey.z = K.ez.y;
            K.ez.z = iA + iB;

            if (FrequencyHz >Fix64.Zero)
            {
                var C1 = cB + rB - cA - rA;

                positionError = C1.magnitude;
                angularError =Fix64.Zero;

                var P = -K.Solve22(C1);

                cA -= mA * P;
                aA -= iA * MathUtils.Cross(rA, P);

                cB += mB * P;
                aB += iB * MathUtils.Cross(rB, P);
            }
            else
            {
                var C1 = cB + rB - cA - rA;
                var C2 = aB - aA - ReferenceAngle;

                positionError = C1.magnitude;
                angularError = Fix64.Abs(C2);

                var C = new FVector3(C1.x, C1.y, C2);

                FVector3 impulse;
                if (K.ez.z >Fix64.Zero)
                {
                    impulse = -K.Solve33(C);
                }
                else
                {
                    var impulse2 = -K.Solve22(C1);
                    impulse = new FVector3(impulse2.x, impulse2.y,Fix64.Zero);
                }

                var P = new FVector2(impulse.x, impulse.y);

                cA -= mA * P;
                aA -= iA * (MathUtils.Cross(rA, P) + impulse.z);

                cB += mB * P;
                aB += iB * (MathUtils.Cross(rB, P) + impulse.z);
            }

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;

            return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }
    }
}