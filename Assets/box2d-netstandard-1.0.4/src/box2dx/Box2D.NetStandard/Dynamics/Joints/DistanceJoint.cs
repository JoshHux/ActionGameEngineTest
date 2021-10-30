﻿/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

// 1-D constrained system
// m (v2 - v1) = lambda
// v2 + (beta/h) * x1 + gamma * lambda = 0, gamma has units of inverse mass.
// x2 = x1 + h * v2

// 1-D mass-damper-spring system
// m (v2 - v1) + h * d * v2 + h * k * 

// C = norm(p2 - p1) - L
// u = (p2 - p1) / norm(p2 - p1)
// Cdot = dot(u, v2 + cross(w2, r2) - v1 - cross(w1, r1))
// J = [-u -cross(r1, u) u cross(r2, u)]
// K = J * invM * JT
//   = invMass1 + invI1 * cross(r1, u)^2 + invMass2 + invI2 * cross(r2, u)^2

using Box2DX.Common;
using FixMath.NET;

namespace Box2DX.Dynamics
{
    /// <summary>
    /// Distance joint definition. This requires defining an
    /// anchor point on both bodies and the non-zero length of the
    /// distance joint. The definition uses local anchor points
    /// so that the initial configuration can violate the constraint
    /// slightly. This helps when saving and loading a game.
    /// @warning Do not use a zero or short length.
    /// </summary>
    public class DistanceBJointDef : BJointDef
    {
        public DistanceBJointDef()
        {
            Type = BJointType.DistanceBJoint;
            LocalAnchor1.Set(Fix64.Zero, Fix64.Zero);
            LocalAnchor2.Set(Fix64.Zero, Fix64.Zero);
            Length = Fix64.One;
            FrequencyHz = Fix64.Zero;
            DampingRatio = Fix64.Zero;
        }

        /// <summary>
        /// Initialize the bodies, anchors, and length using the world anchors.
        /// </summary>
        public void Initialize(Body body1, Body body2, FVec2 anchor1, FVec2 anchor2)
        {
            Body1 = body1;
            Body2 = body2;
            LocalAnchor1 = body1.GetLocalPoint(anchor1);
            LocalAnchor2 = body2.GetLocalPoint(anchor2);
            FVec2 d = anchor2 - anchor1;
            Length = d.Length();
        }

        /// <summary>
        /// The local anchor point relative to body1's origin.
        /// </summary>
        public FVec2 LocalAnchor1;

        /// <summary>
        /// The local anchor point relative to body2's origin.
        /// </summary>
        public FVec2 LocalAnchor2;

        /// <summary>
        /// The equilibrium length between the anchor points.
        /// </summary>
        public Fix64 Length;

        /// <summary>
        /// The response speed.
        /// </summary>
        public Fix64 FrequencyHz;

        /// <summary>
        /// The damping ratio. 0 = no damping, 1 = critical damping.
        /// </summary>
        public Fix64 DampingRatio;
    }

    /// <summary>
    /// A distance joint constrains two points on two bodies
    /// to remain at a fixed distance from each other. You can view
    /// this as a massless, rigid rod.
    /// </summary>
    public class DistanceBJoint : BJoint
    {
        public FVec2 _localAnchor1;
        public FVec2 _localAnchor2;
        public FVec2 _u;
        public Fix64 _frequencyHz;
        public Fix64 _dampingRatio;
        public Fix64 _gamma;
        public Fix64 _bias;
        public Fix64 _impulse;
        public Fix64 _mass;     // effective mass for the constraint.
        public Fix64 _length;

        public override FVec2 Anchor1
        {
            get { return _body1.GetWorldPoint(_localAnchor1); }
        }

        public override FVec2 Anchor2
        {
            get { return _body2.GetWorldPoint(_localAnchor2); }
        }

        public override FVec2 GetReactionForce(Fix64 inv_dt)
        {
            return (inv_dt * _impulse) * _u;
        }

        public override Fix64 GetReactionTorque(Fix64 inv_dt)
        {
            return Fix64.Zero;
        }

        public DistanceBJoint(DistanceBJointDef def)
            : base(def)
        {
            _localAnchor1 = def.LocalAnchor1;
            _localAnchor2 = def.LocalAnchor2;
            _length = def.Length;
            _frequencyHz = def.FrequencyHz;
            _dampingRatio = def.DampingRatio;
            _impulse = Fix64.Zero;
            _gamma = Fix64.Zero;
            _bias = Fix64.Zero;
        }

        internal override void InitVelocityConstraints(TimeStep step)
        {
            Body b1 = _body1;
            Body b2 = _body2;

            // Compute the effective mass matrix.
            FVec2 r1 = Common.Math.Mul(b1.GetXForm().R, _localAnchor1 - b1.GetLocalCenter());
            FVec2 r2 = Common.Math.Mul(b2.GetXForm().R, _localAnchor2 - b2.GetLocalCenter());
            _u = b2._sweep.C + r2 - b1._sweep.C - r1;

            // Handle singularity.
            Fix64 length = _u.Length();
            if (length > Settings.LinearSlop)
            {
                _u *= Fix64.One / length;
            }
            else
            {
                _u.Set(Fix64.Zero, Fix64.Zero);
            }

            Fix64 cr1u = FVec2.Cross(r1, _u);
            Fix64 cr2u = FVec2.Cross(r2, _u);
            Fix64 invMass = b1._invMass + b1._invI * cr1u * cr1u + b2._invMass + b2._invI * cr2u * cr2u;
            Box2DXDebug.Assert(invMass > Settings.FLT_EPSILON);
            _mass = Fix64.One / invMass;

            if (_frequencyHz > Fix64.Zero)
            {
                Fix64 C = length - _length;

                // Frequency
                Fix64 omega = (Fix64)2.0f * Settings.Pi * _frequencyHz;

                // Damping coefficient
                Fix64 d = (Fix64)2.0f * _mass * _dampingRatio * omega;

                // Spring stiffness
                Fix64 k = _mass * omega * omega;

                // magic formulas
                _gamma = Fix64.One / (step.Dt * (d + step.Dt * k));
                _bias = C * step.Dt * k * _gamma;

                _mass = Fix64.One / (invMass + _gamma);
            }

            if (step.WarmStarting)
            {
                //Scale the inpulse to support a variable timestep.
                _impulse *= step.DtRatio;
                FVec2 P = _impulse * _u;
                b1._linearVelocity -= b1._invMass * P;
                b1._angularVelocity -= b1._invI * FVec2.Cross(r1, P);
                b2._linearVelocity += b2._invMass * P;
                b2._angularVelocity += b2._invI * FVec2.Cross(r2, P);
            }
            else
            {
                _impulse = Fix64.Zero;
            }
        }

        internal override bool SolvePositionConstraints(Fix64 baumgarte)
        {
            if (_frequencyHz > Fix64.Zero)
            {
                //There is no possition correction for soft distace constraint.
                return true;
            }

            Body b1 = _body1;
            Body b2 = _body2;

            FVec2 r1 = Common.Math.Mul(b1.GetXForm().R, _localAnchor1 - b1.GetLocalCenter());
            FVec2 r2 = Common.Math.Mul(b2.GetXForm().R, _localAnchor2 - b2.GetLocalCenter());

            FVec2 d = b2._sweep.C + r2 - b1._sweep.C - r1;

            Fix64 length = d.Normalize();
            Fix64 C = length - _length;
            C = Common.Math.Clamp(C, -Settings.MaxLinearCorrection, Settings.MaxLinearCorrection);

            Fix64 impulse = -_mass * C;
            _u = d;
            FVec2 P = impulse * _u;

            b1._sweep.C -= b1._invMass * P;
            b1._sweep.A -= b1._invI * FVec2.Cross(r1, P);
            b2._sweep.C += b2._invMass * P;
            b2._sweep.A += b2._invI * FVec2.Cross(r2, P);

            b1.SynchronizeTransform();
            b2.SynchronizeTransform();

            return Fix64.Abs(C) < Settings.LinearSlop;
        }

        internal override void SolveVelocityConstraints(TimeStep step)
        {
            //B2_NOT_USED(step);

            Body b1 = _body1;
            Body b2 = _body2;

            FVec2 r1 = Common.Math.Mul(b1.GetXForm().R, _localAnchor1 - b1.GetLocalCenter());
            FVec2 r2 = Common.Math.Mul(b2.GetXForm().R, _localAnchor2 - b2.GetLocalCenter());

            // Cdot = dot(u, v + cross(w, r))
            FVec2 v1 = b1._linearVelocity + FVec2.Cross(b1._angularVelocity, r1);
            FVec2 v2 = b2._linearVelocity + FVec2.Cross(b2._angularVelocity, r2);
            Fix64 Cdot = FVec2.Dot(_u, v2 - v1);
            Fix64 impulse = -_mass * (Cdot + _bias + _gamma * _impulse);
            _impulse += impulse;

            FVec2 P = impulse * _u;
            b1._linearVelocity -= b1._invMass * P;
            b1._angularVelocity -= b1._invI * FVec2.Cross(r1, P);
            b2._linearVelocity += b2._invMass * P;
            b2._angularVelocity += b2._invI * FVec2.Cross(r2, P);
        }
    }
}
