/*
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

using Box2DX.Common;
using FixMath.NET;

namespace Box2DX.Dynamics
{
	public enum BJointType
	{
		UnknownBJoint,
		RevoluteBJoint,
		PrismaticBJoint,
		DistanceBJoint,
		PulleyBJoint,
		MouseBJoint,
		GearBJoint,
		LineBJoint
	}

	public enum LimitState
	{
		InactiveLimit,
		AtLowerLimit,
		AtUpperLimit,
		EqualLimits
	}

	public struct Jacobian
	{
		public FVec2 Linear1;
		public Fix64 Angular1;
		public FVec2 Linear2;
		public Fix64 Angular2;

		public void SetZero()
		{
			Linear1.SetZero(); Angular1 = Fix64.Zero;
			Linear2.SetZero(); Angular2 = Fix64.Zero;
		}

		public void Set(FVec2 x1, Fix64 a1, FVec2 x2, Fix64 a2)
		{
			Linear1 = x1; Angular1 = a1;
			Linear2 = x2; Angular2 = a2;
		}

		public Fix64 Compute(FVec2 x1, Fix64 a1, FVec2 x2, Fix64 a2)
		{
			return FVec2.Dot(Linear1, x1) + Angular1 * a1 + FVec2.Dot(Linear2, x2) + Angular2 * a2;
		}
	}

#warning "CAS"
	/// <summary>
	/// A joint edge is used to connect bodies and joints together
	/// in a joint graph where each body is a node and each joint
	/// is an edge. A joint edge belongs to a doubly linked list
	/// maintained in each attached body. Each joint has two joint
	/// nodes, one for each attached body.
	/// </summary>
	public class BJointEdge
	{
		/// <summary>
		/// Provides quick access to the other body attached.
		/// </summary>
		public Body Other;

		/// <summary>
		/// The joint.
		/// </summary>
		public BJoint BJoint;

		/// <summary>
		/// The previous joint edge in the body's joint list.
		/// </summary>
		public BJointEdge Prev;

		/// <summary>
		/// The next joint edge in the body's joint list.
		/// </summary>
		public BJointEdge Next;
	}

#warning "CAS"
	/// <summary>
	/// BJoint definitions are used to construct joints.
	/// </summary>
	public class BJointDef
	{
		public BJointDef()
		{
			Type = BJointType.UnknownBJoint;
			UserData = null;
			Body1 = null;
			Body2 = null;
			CollideConnected = false;
		}

		/// <summary>
		/// The joint type is set automatically for concrete joint types.
		/// </summary>
		public BJointType Type;

		/// <summary>
		/// Use this to attach application specific data to your joints.
		/// </summary>
		public object UserData;

		/// <summary>
		/// The first attached body.
		/// </summary>
		public Body Body1;

		/// <summary>
		/// The second attached body.
		/// </summary>
		public Body Body2;

		/// <summary>
		/// Set this flag to true if the attached bodies should collide.
		/// </summary>
		public bool CollideConnected;
	}

	/// <summary>
	/// The base joint class. BJoints are used to constraint two bodies together in
	/// various fashions. Some joints also feature limits and motors.
	/// </summary>
	public abstract class BJoint
	{
		protected BJointType _type;
		internal BJoint _prev;
		internal BJoint _next;
		internal BJointEdge _node1 = new BJointEdge();
		internal BJointEdge _node2 = new BJointEdge();
		internal Body _body1;
		internal Body _body2;

		internal bool _islandFlag;
		internal bool _collideConnected;

		protected object _userData;

		// Cache here per time step to reduce cache misses.
		protected FVec2 _localCenter1, _localCenter2;
		protected Fix64 _invMass1, _invI1;
		protected Fix64 _invMass2, _invI2;

		/// <summary>
		/// Get the type of the concrete joint.
		/// </summary>
		public new BJointType GetType()
		{
			return _type;
		}

		/// <summary>
		/// Get the first body attached to this joint.
		/// </summary>
		/// <returns></returns>
		public Body GetBody1()
		{
			return _body1;
		}

		/// <summary>
		/// Get the second body attached to this joint.
		/// </summary>
		/// <returns></returns>
		public Body GetBody2()
		{
			return _body2;
		}

		/// <summary>
		/// Get the anchor point on body1 in world coordinates.
		/// </summary>
		/// <returns></returns>
		public abstract FVec2 Anchor1 { get; }

		/// <summary>
		/// Get the anchor point on body2 in world coordinates.
		/// </summary>
		/// <returns></returns>
		public abstract FVec2 Anchor2 { get; }

		/// <summary>
		/// Get the reaction force on body2 at the joint anchor.
		/// </summary>		
		public abstract FVec2 GetReactionForce(Fix64 inv_dt);

		/// <summary>
		/// Get the reaction torque on body2.
		/// </summary>		
		public abstract Fix64 GetReactionTorque(Fix64 inv_dt);

		/// <summary>
		/// Get the next joint the world joint list.
		/// </summary>
		/// <returns></returns>
		public BJoint GetNext()
		{
			return _next;
		}

		/// <summary>
		/// Get/Set the user data pointer.
		/// </summary>
		/// <returns></returns>
		public object UserData
		{
			get { return _userData; }
			set { _userData = value; }
		}

		protected BJoint(BJointDef def)
		{
			_type = def.Type;
			_prev = null;
			_next = null;
			_body1 = def.Body1;
			_body2 = def.Body2;
			_collideConnected = def.CollideConnected;
			_islandFlag = false;
			_userData = def.UserData;
		}

		internal static BJoint Create(BJointDef def)
		{
			BJoint joint = null;

			switch (def.Type)
			{
				case BJointType.DistanceBJoint:
					{
						joint = new DistanceBJoint((DistanceBJointDef)def);
					}
					break;
				case BJointType.MouseBJoint:
					{
						joint = new MouseBJoint((MouseBJointDef)def);
					}
					break;
				case BJointType.PrismaticBJoint:
					{
						joint = new PrismaticBJoint((PrismaticBJointDef)def);
					}
					break;
				case BJointType.RevoluteBJoint:
					{
						joint = new RevoluteBJoint((RevoluteBJointDef)def);
					}
					break;
				case BJointType.PulleyBJoint:
					{
						joint = new PulleyBJoint((PulleyBJointDef)def);
					}
					break;
				case BJointType.GearBJoint:
					{
						joint = new GearBJoint((GearBJointDef)def);
					}
					break;
				case BJointType.LineBJoint:
					{
						joint = new LineBJoint((LineBJointDef)def);
					}
					break;
				default:
					Box2DXDebug.Assert(false);
					break;
			}

			return joint;
		}

		internal static void Destroy(BJoint joint)
		{
			joint = null;
		}

		internal abstract void InitVelocityConstraints(TimeStep step);
		internal abstract void SolveVelocityConstraints(TimeStep step);

		// This returns true if the position errors are within tolerance.
		internal abstract bool SolvePositionConstraints(Fix64 baumgarte);

		internal void ComputeXForm(ref XForm xf, FVec2 center, FVec2 localCenter, Fix64 angle)
		{
			xf.R.Set(angle);
			xf.Position = center - Box2DX.Common.Math.Mul(xf.R, localCenter);
		}
	}
}
