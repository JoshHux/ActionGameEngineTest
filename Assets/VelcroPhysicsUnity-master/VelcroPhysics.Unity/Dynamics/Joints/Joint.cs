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

using System;
using UnityEngine;
using VelcroPhysics.Dynamics.Solver;

namespace VelcroPhysics.Dynamics.VJoints
{
    public abstract class VJoint
    {
        private float _breakpoint;
        private float _breakpointSquared;

        internal VJointEdge EdgeA = new VJointEdge();
        internal VJointEdge EdgeB = new VJointEdge();

        /// <summary>
        /// Indicate if this join is enabled or not. Disabling a VJoint
        /// means it is still in the simulation, but inactive.
        /// </summary>
        public bool Enabled = true;

        internal bool IslandFlag;

        protected VJoint()
        {
            Breakpoint = float.MaxValue;

            //Connected bodies should not collide by default
            CollideConnected = false;
        }

        protected VJoint(Body bodyA, Body bodyB) : this()
        {
            //Can't connect a VJoint to the same body twice.
            Debug.Assert(bodyA != bodyB);

            BodyA = bodyA;
            BodyB = bodyB;
        }

        /// <summary>
        /// Constructor for fixed VJoint
        /// </summary>
        protected VJoint(Body body) : this()
        {
            BodyA = body;
        }

        /// <summary>
        /// Gets or sets the type of the VJoint.
        /// </summary>
        /// <value>The type of the VJoint.</value>
        public VJointType VJointType { get; protected set; }

        /// <summary>
        /// Get the first body attached to this VJoint.
        /// </summary>
        public Body BodyA { get; internal set; }

        /// <summary>
        /// Get the second body attached to this VJoint.
        /// </summary>
        public Body BodyB { get; internal set; }

        /// <summary>
        /// Get the anchor point on bodyA in world coordinates.
        /// On some VJoints, this value indicate the anchor point within the world.
        /// </summary>
        public abstract Vector2 WorldAnchorA { get; set; }

        /// <summary>
        /// Get the anchor point on bodyB in world coordinates.
        /// On some VJoints, this value indicate the anchor point within the world.
        /// </summary>
        public abstract Vector2 WorldAnchorB { get; set; }

        /// <summary>
        /// Set the user data pointer.
        /// </summary>
        /// <value>The data.</value>
        public object UserData { get; set; }

        /// <summary>
        /// Set this flag to true if the attached bodies should collide.
        /// </summary>
        public bool CollideConnected { get; set; }

        /// <summary>
        /// The Breakpoint simply indicates the maximum Value the VJointError can be before it breaks.
        /// The default value is float.MaxValue, which means it never breaks.
        /// </summary>
        public float Breakpoint
        {
            get => _breakpoint;
            set
            {
                _breakpoint = value;
                _breakpointSquared = _breakpoint * _breakpoint;
            }
        }

        /// <summary>
        /// Fires when the VJoint is broken.
        /// </summary>
        public event Action<VJoint, float> Broke;

        /// <summary>
        /// Get the reaction force on body at the VJoint anchor in Newtons.
        /// </summary>
        /// <param name="invDt">The inverse delta time.</param>
        public abstract Vector2 GetReactionForce(float invDt);

        /// <summary>
        /// Get the reaction torque on the body at the VJoint anchor in N*m.
        /// </summary>
        /// <param name="invDt">The inverse delta time.</param>
        public abstract float GetReactionTorque(float invDt);

        protected void WakeBodies()
        {
            if (BodyA != null)
                BodyA.Awake = true;

            if (BodyB != null)
                BodyB.Awake = true;
        }

        /// <summary>
        /// Return true if the VJoint is a fixed type.
        /// </summary>
        public bool IsFixedType()
        {
            return VJointType == VJointType.FixedRevolute ||
                   VJointType == VJointType.FixedDistance ||
                   VJointType == VJointType.FixedPrismatic ||
                   VJointType == VJointType.FixedLine ||
                   VJointType == VJointType.FixedMouse ||
                   VJointType == VJointType.FixedAngle ||
                   VJointType == VJointType.FixedFriction;
        }

        internal abstract void InitVelocityConstraints(ref SolverData data);

        internal void Validate(float invDt)
        {
            if (!Enabled)
                return;

            var VJointErrorSquared = GetReactionForce(invDt).sqrMagnitude;

            if (Mathf.Abs(VJointErrorSquared) <= _breakpointSquared)
                return;

            Enabled = false;

            Broke?.Invoke(this, Mathf.Sqrt(VJointErrorSquared));
        }

        internal abstract void SolveVelocityConstraints(ref SolverData data);

        /// <summary>
        /// Solves the position constraints.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>returns true if the position errors are within tolerance.</returns>
        internal abstract bool SolvePositionConstraints(ref SolverData data);
    }
}