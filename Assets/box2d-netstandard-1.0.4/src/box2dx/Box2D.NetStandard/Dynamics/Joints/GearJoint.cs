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

// Gear BJoint:
// C0 = (coordinate1 + ratio * coordinate2)_initial
// C = C0 - (cordinate1 + ratio * coordinate2) = 0
// Cdot = -(Cdot1 + ratio * Cdot2)
// J = -[J1 ratio * J2]
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

using Box2DX.Common;
using FixMath.NET;

namespace Box2DX.Dynamics
{
	/// <summary>
	/// Gear joint definition. This definition requires two existing
	/// revolute or prismatic joints (any combination will work).
	/// The provided joints must attach a dynamic body to a static body.
	/// </summary>
	public class GearBJointDef : BJointDef
	{
		public GearBJointDef()
		{
			Type = BJointType.GearBJoint;
			BJoint1 = null;
			BJoint2 = null;
			Ratio = Fix64.One;
		}

		/// <summary>
		/// The first revolute/prismatic joint attached to the gear joint.
		/// </summary>
		public BJoint BJoint1;

		/// <summary>
		/// The second revolute/prismatic joint attached to the gear joint.
		/// </summary>
		public BJoint BJoint2;

		/// <summary>
		/// The gear ratio.
		/// @see GearBJoint for explanation.
		/// </summary>
		public Fix64 Ratio;
	}

	/// <summary>
	/// A gear joint is used to connect two joints together. Either joint
	/// can be a revolute or prismatic joint. You specify a gear ratio
	/// to bind the motions together:
	/// coordinate1 + ratio * coordinate2 = constant
	/// The ratio can be negative or positive. If one joint is a revolute joint
	/// and the other joint is a prismatic joint, then the ratio will have units
	/// of length or units of 1/length.
	/// @warning The revolute and prismatic joints must be attached to
	/// fixed bodies (which must be body1 on those joints).
	/// </summary>
	public class GearBJoint : BJoint
	{
		public Body _ground1;
		public Body _ground2;

		// One of these is NULL.
		public RevoluteBJoint _revolute1;
		public PrismaticBJoint _prismatic1;

		// One of these is NULL.
		public RevoluteBJoint _revolute2;
		public PrismaticBJoint _prismatic2;

		public FVec2 _groundAnchor1;
		public FVec2 _groundAnchor2;

		public FVec2 _localAnchor1;
		public FVec2 _localAnchor2;

		public Jacobian _J;

		public Fix64 _constant;
		public Fix64 _ratio;

		// Effective mass
		public Fix64 _mass;

		// Impulse for accumulation/warm starting.
		public Fix64 _impulse;

		public override FVec2 Anchor1 { get { return _body1.GetWorldPoint(_localAnchor1); } }
		public override FVec2 Anchor2 { get { return _body2.GetWorldPoint(_localAnchor2); } }

		public override FVec2 GetReactionForce(Fix64 inv_dt)
		{
			// TODO_ERIN not tested
			FVec2 P = _impulse * _J.Linear2;
			return inv_dt * P;
		}

		public override Fix64 GetReactionTorque(Fix64 inv_dt)
		{
			// TODO_ERIN not tested
			FVec2 r = Common.Math.Mul(_body2.GetXForm().R, _localAnchor2 - _body2.GetLocalCenter());
			FVec2 P = _impulse * _J.Linear2;
			Fix64 L = _impulse * _J.Angular2 - FVec2.Cross(r, P);
			return inv_dt * L;
		}

		/// <summary>
		/// Get the gear ratio.
		/// </summary>
		public Fix64 Ratio { get { return _ratio; } }

		public GearBJoint(GearBJointDef def)
			: base(def)
		{
			BJointType type1 = def.BJoint1.GetType();
			BJointType type2 = def.BJoint2.GetType();

			Box2DXDebug.Assert(type1 == BJointType.RevoluteBJoint || type1 == BJointType.PrismaticBJoint);
			Box2DXDebug.Assert(type2 == BJointType.RevoluteBJoint || type2 == BJointType.PrismaticBJoint);
			Box2DXDebug.Assert(def.BJoint1.GetBody1().IsStatic());
			Box2DXDebug.Assert(def.BJoint2.GetBody1().IsStatic());

			_revolute1 = null;
			_prismatic1 = null;
			_revolute2 = null;
			_prismatic2 = null;

			Fix64 coordinate1, coordinate2;

			_ground1 = def.BJoint1.GetBody1();
			_body1 = def.BJoint1.GetBody2();
			if (type1 == BJointType.RevoluteBJoint)
			{
				_revolute1 = (RevoluteBJoint)def.BJoint1;
				_groundAnchor1 = _revolute1._localAnchor1;
				_localAnchor1 = _revolute1._localAnchor2;
				coordinate1 = _revolute1.BJointAngle;
			}
			else
			{
				_prismatic1 = (PrismaticBJoint)def.BJoint1;
				_groundAnchor1 = _prismatic1._localAnchor1;
				_localAnchor1 = _prismatic1._localAnchor2;
				coordinate1 = _prismatic1.BJointTranslation;
			}

			_ground2 = def.BJoint2.GetBody1();
			_body2 = def.BJoint2.GetBody2();
			if (type2 == BJointType.RevoluteBJoint)
			{
				_revolute2 = (RevoluteBJoint)def.BJoint2;
				_groundAnchor2 = _revolute2._localAnchor1;
				_localAnchor2 = _revolute2._localAnchor2;
				coordinate2 = _revolute2.BJointAngle;
			}
			else
			{
				_prismatic2 = (PrismaticBJoint)def.BJoint2;
				_groundAnchor2 = _prismatic2._localAnchor1;
				_localAnchor2 = _prismatic2._localAnchor2;
				coordinate2 = _prismatic2.BJointTranslation;
			}

			_ratio = def.Ratio;

			_constant = coordinate1 + _ratio * coordinate2;

			_impulse = Fix64.Zero;
		}

		internal override void InitVelocityConstraints(TimeStep step)
		{
			Body g1 = _ground1;
			Body g2 = _ground2;
			Body b1 = _body1;
			Body b2 = _body2;

			Fix64 K = Fix64.Zero;
			_J.SetZero();

			if (_revolute1!=null)
			{
				_J.Angular1 = -Fix64.One;
				K += b1._invI;
			}
			else
			{
				FVec2 ug = Common.Math.Mul(g1.GetXForm().R, _prismatic1._localXAxis1);
				FVec2 r = Common.Math.Mul(b1.GetXForm().R, _localAnchor1 - b1.GetLocalCenter());
				Fix64 crug = FVec2.Cross(r, ug);
				_J.Linear1 = -ug;
				_J.Angular1 = -crug;
				K += b1._invMass + b1._invI * crug * crug;
			}

			if (_revolute2!=null)
			{
				_J.Angular2 = -_ratio;
				K += _ratio * _ratio * b2._invI;
			}
			else
			{
				FVec2 ug = Common.Math.Mul(g2.GetXForm().R, _prismatic2._localXAxis1);
				FVec2 r = Common.Math.Mul(b2.GetXForm().R, _localAnchor2 - b2.GetLocalCenter());
				Fix64 crug = FVec2.Cross(r, ug);
				_J.Linear2 = -_ratio * ug;
				_J.Angular2 = -_ratio * crug;
				K += _ratio * _ratio * (b2._invMass + b2._invI * crug * crug);
			}

			// Compute effective mass.
			Box2DXDebug.Assert(K > Fix64.Zero);
			_mass = Fix64.One / K;

			if (step.WarmStarting)
			{
				// Warm starting.
				b1._linearVelocity += b1._invMass * _impulse * _J.Linear1;
				b1._angularVelocity += b1._invI * _impulse * _J.Angular1;
				b2._linearVelocity += b2._invMass * _impulse * _J.Linear2;
				b2._angularVelocity += b2._invI * _impulse * _J.Angular2;
			}
			else
			{
				_impulse = Fix64.Zero;
			}
		}

		internal override void SolveVelocityConstraints(TimeStep step)
		{
			Body b1 = _body1;
			Body b2 = _body2;

			Fix64 Cdot = _J.Compute(b1._linearVelocity, b1._angularVelocity, b2._linearVelocity, b2._angularVelocity);

			Fix64 impulse = _mass * (-Cdot);
			_impulse += impulse;

			b1._linearVelocity += b1._invMass * impulse * _J.Linear1;
			b1._angularVelocity += b1._invI * impulse * _J.Angular1;
			b2._linearVelocity += b2._invMass * impulse * _J.Linear2;
			b2._angularVelocity += b2._invI * impulse * _J.Angular2;
		}

		internal override bool SolvePositionConstraints(Fix64 baumgarte)
		{
			Fix64 linearError = Fix64.Zero;

			Body b1 = _body1;
			Body b2 = _body2;

			Fix64 coordinate1, coordinate2;
			if (_revolute1 != null)
			{
				coordinate1 = _revolute1.BJointAngle;
			}
			else
			{
				coordinate1 = _prismatic1.BJointTranslation;
			}

			if (_revolute2 != null)
			{
				coordinate2 = _revolute2.BJointAngle;
			}
			else
			{
				coordinate2 = _prismatic2.BJointTranslation;
			}

			Fix64 C = _constant - (coordinate1 + _ratio * coordinate2);

			Fix64 impulse = _mass * (-C);

			b1._sweep.C += b1._invMass * impulse * _J.Linear1;
			b1._sweep.A += b1._invI * impulse * _J.Angular1;
			b2._sweep.C += b2._invMass * impulse * _J.Linear2;
			b2._sweep.A += b2._invI * impulse * _J.Angular2;

			b1.SynchronizeTransform();
			b2.SynchronizeTransform();

			//TODO_ERIN not implemented
			return linearError < Settings.LinearSlop;
		}
	}
}
