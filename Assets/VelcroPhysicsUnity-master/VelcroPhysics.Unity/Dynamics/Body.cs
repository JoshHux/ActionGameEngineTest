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

using System.Collections.Generic;
using VelcroPhysics.Collision.Broadphase;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Collision.Handlers;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Collision.TOI;
using VelcroPhysics.Dynamics.VJoints;
using VelcroPhysics.Extensions.Controllers.ControllerBase;
using VelcroPhysics.Extensions.PhysicsLogics.PhysicsLogicBase;
using VelcroPhysics.Templates;
using VelcroPhysics.Utilities;
using FixMath.NET;
using VTransform = VelcroPhysics.Shared.VTransform;

namespace VelcroPhysics.Dynamics
{
    public class Body
    {
        private BodyType _type;
        private Fix64 _inertia;
        private Fix64 _mass;

        internal BodyFlags _flags;
        internal Fix64 _invI;
        internal Fix64 _invMass;
        internal FVector2 _force;
        internal FVector2 _linearVelocity;
        internal Fix64 _angularVelocity;
        internal Sweep _sweep; // the swept motion for CCD
        internal Fix64 _torque;
        internal World _world;
        public VTransform _xf; // the body origin VTransform

        //Spax's addition
        public ParentConstraint constraint;

        internal Body(World world, BodyTemplate template)
        {
            FixtureList = new List<Fixture>(1);

            if (template.AllowCCD)
                _flags |= BodyFlags.BulletFlag;
            if (template.AllowRotation)
                _flags |= BodyFlags.FixedRotationFlag;
            if (template.AllowSleep)
                _flags |= BodyFlags.AutoSleepFlag;
            if (template.Awake)
                _flags |= BodyFlags.AwakeFlag;
            if (template.Active)
                _flags |= BodyFlags.Enabled;

            _world = world;

            _xf.p = template.Position;
            _xf.q.Set(template.Angle);

            _sweep.C0 = _xf.p;
            _sweep.C = _xf.p;
            _sweep.A0 = template.Angle;
            _sweep.A = template.Angle;

            _linearVelocity = template.LinearVelocity;
            _angularVelocity = template.AngularVelocity;

            LinearDamping = template.LinearDamping;
            AngularDamping = template.AngularDamping;
            GravityScale = Fix64.One;

            _type = template.Type;

            if (_type == BodyType.Dynamic)
            {
                _mass = Fix64.One;
                _invMass = Fix64.One;
            }
            else
            {
                _mass = Fix64.Zero;
                _invMass = Fix64.Zero;
            }

            UserData = template.UserData;
        }

        public ControllerFilter ControllerFilter { get; set; }

        public PhysicsLogicFilter PhysicsLogicFilter { get; set; }

        /// <summary>
        /// A unique id for this body.
        /// </summary>
        public int BodyId { get; internal set; }

        public Fix64 SleepTime { get; set; }

        public int IslandIndex { get; set; }

        /// <summary>
        /// Scale the gravity applied to this body.
        /// Defaults to 1. A value of 2 means Fix64 the gravity is applied to this body.
        /// </summary>
        public Fix64 GravityScale { get; set; }

        /// <summary>
        /// Set the user data. Use this to store your application specific data.
        /// </summary>
        /// <value>The user data.</value>
        public object UserData { get; set; }

        /// <summary>
        /// Gets the total number revolutions the body has made.
        /// </summary>
        /// <value>The revolutions.</value>
        public Fix64 Revolutions => Rotation / Fix64.Pi;

        /// <summary>
        /// Gets or sets the body type.
        /// Warning: Calling this mid-update might cause a crash.
        /// </summary>
        /// <value>The type of body.</value>
        public BodyType BodyType
        {
            get => _type;
            set
            {
                if (value == _type)
                    return;

                _type = value;

                ResetMassData();

                if (_type == BodyType.Static)
                {
                    _linearVelocity = FVector2.zero;
                    _angularVelocity = Fix64.Zero;
                    _sweep.A0 = _sweep.A;
                    _sweep.C0 = _sweep.C;
                    SynchronizeFixtures();
                }

                Awake = true;

                _force = FVector2.zero;
                _torque = Fix64.Zero;

                // Delete the attached contacts.
                var ce = ContactList;
                while (ce != null)
                {
                    var ce0 = ce;
                    ce = ce.Next;
                    _world.ContactManager.Destroy(ce0.Contact);
                }

                ContactList = null;

                // Touch the proxies so that new contacts will be created (when appropriate)
                var broadPhase = _world.ContactManager.BroadPhase;
                foreach (var fixture in FixtureList)
                {
                    var proxyCount = fixture.ProxyCount;
                    for (var j = 0; j < proxyCount; j++) broadPhase.TouchProxy(fixture.Proxies[j].ProxyId);
                }
            }
        }

        /// <summary>
        /// Get or sets the linear velocity of the center of mass.
        /// </summary>
        /// <value>The linear velocity.</value>
        public FVector2 LinearVelocity
        {
            get => _linearVelocity;
            set
            {
                UnityEngine.Debug.Assert(!Fix64.IsNaN(value.x) && !Fix64.IsNaN(value.y));

                if (_type == BodyType.Static)
                    return;

                if (FVector2.Dot(value, value) > Fix64.Zero)
                    Awake = true;

                _linearVelocity = value;
            }
        }

        /// <summary>
        /// Gets or sets the angular velocity. Radians/second.
        /// </summary>
        /// <value>The angular velocity.</value>
        public Fix64 AngularVelocity
        {
            get => _angularVelocity;
            set
            {
                UnityEngine.Debug.Assert(!Fix64.IsNaN(value));

                if (_type == BodyType.Static)
                    return;

                if (value * value > Fix64.Zero)
                    Awake = true;

                _angularVelocity = value;
            }
        }

        /// <summary>
        /// Gets or sets the linear damping.
        /// </summary>
        /// <value>The linear damping.</value>
        public Fix64 LinearDamping { get; set; }

        /// <summary>
        /// Gets or sets the angular damping.
        /// </summary>
        /// <value>The angular damping.</value>
        public Fix64 AngularDamping { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this body should be included in the CCD solver.
        /// </summary>
        /// <value><c>true</c> if this instance is included in CCD; otherwise, <c>false</c>.</value>
        public bool IsBullet
        {
            get => (_flags & BodyFlags.BulletFlag) == BodyFlags.BulletFlag;
            set
            {
                if (value)
                    _flags |= BodyFlags.BulletFlag;
                else
                    _flags &= ~BodyFlags.BulletFlag;
            }
        }

        /// <summary>
        /// You can disable sleeping on this body. If you disable sleeping, the
        /// body will be woken.
        /// </summary>
        /// <value><c>true</c> if sleeping is allowed; otherwise, <c>false</c>.</value>
        public bool SleepingAllowed
        {
            get => (_flags & BodyFlags.AutoSleepFlag) == BodyFlags.AutoSleepFlag;
            set
            {
                if (value)
                {
                    _flags |= BodyFlags.AutoSleepFlag;
                }
                else
                {
                    _flags &= ~BodyFlags.AutoSleepFlag;
                    Awake = true;
                }
            }
        }

        /// <summary>
        /// Set the sleep state of the body. A sleeping body has very
        /// low CPU cost.
        /// </summary>
        /// <value><c>true</c> if awake; otherwise, <c>false</c>.</value>
        public bool Awake
        {
            get => (_flags & BodyFlags.AwakeFlag) == BodyFlags.AwakeFlag;
            set
            {
                if (value)
                {
                    _flags |= BodyFlags.AwakeFlag;
                    SleepTime = Fix64.Zero;
                }
                else
                {
                    _flags &= ~BodyFlags.AwakeFlag;
                    ResetDynamics();
                    SleepTime = Fix64.Zero;
                }
            }
        }

        /// <summary>
        /// Set the active state of the body. An inactive body is not
        /// simulated and cannot be collided with or woken up.
        /// If you pass a flag of true, all fixtures will be added to the
        /// broad-phase.
        /// If you pass a flag of false, all fixtures will be removed from
        /// the broad-phase and all contacts will be destroyed.
        /// Fixtures and VJoints are otherwise unaffected. You may continue
        /// to create/destroy fixtures and VJoints on inactive bodies.
        /// Fixtures on an inactive body are implicitly inactive and will
        /// not participate in collisions, ray-casts, or queries.
        /// VJoints connected to an inactive body are implicitly inactive.
        /// An inactive body is still owned by a b2World object and remains
        /// in the body list.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get => (_flags & BodyFlags.Enabled) == BodyFlags.Enabled;

            set
            {
                if (value == Enabled)
                    return;

                if (value)
                {
                    _flags |= BodyFlags.Enabled;

                    // Create all proxies.
                    var broadPhase = _world.ContactManager.BroadPhase;
                    for (var i = 0; i < FixtureList.Count; i++) FixtureList[i].CreateProxies(broadPhase, ref _xf);

                    // Contacts are created the next time step.
                }
                else
                {
                    _flags &= ~BodyFlags.Enabled;

                    // Destroy all proxies.
                    var broadPhase = _world.ContactManager.BroadPhase;

                    for (var i = 0; i < FixtureList.Count; i++) FixtureList[i].DestroyProxies(broadPhase);

                    // Destroy the attached contacts.
                    var ce = ContactList;
                    while (ce != null)
                    {
                        var ce0 = ce;
                        ce = ce.Next;
                        _world.ContactManager.Destroy(ce0.Contact);
                    }

                    ContactList = null;
                }
            }
        }

        /// <summary>
        /// Set this body to have fixed rotation. This causes the mass
        /// to be reset.
        /// </summary>
        /// <value><c>true</c> if it has fixed rotation; otherwise, <c>false</c>.</value>
        public bool FixedRotation
        {
            get => (_flags & BodyFlags.FixedRotationFlag) == BodyFlags.FixedRotationFlag;
            set
            {
                if (value == FixedRotation)
                    return;

                if (value)
                    _flags |= BodyFlags.FixedRotationFlag;
                else
                    _flags &= ~BodyFlags.FixedRotationFlag;

                _angularVelocity = 0;
                ResetMassData();
            }
        }

        /// <summary>
        /// Gets all the fixtures attached to this body.
        /// </summary>
        /// <value>The fixture list.</value>
        public List<Fixture> FixtureList { get; internal set; }

        /// <summary>
        /// Get the list of all VJoints attached to this body.
        /// </summary>
        /// <value>The VJoint list.</value>
        public VJointEdge VJointList { get; internal set; }

        /// <summary>
        /// Get the list of all contacts attached to this body.
        /// Warning: this list changes during the time step and you may
        /// miss some collisions if you don't use ContactListener.
        /// </summary>
        /// <value>The contact list.</value>
        public ContactEdge ContactList { get; internal set; }

        /// <summary>
        /// Get the world body origin position.
        /// </summary>
        /// <returns>Return the world position of the body's origin.</returns>
        public FVector2 Position
        {
            get => _xf.p;
            set
            {
                UnityEngine.Debug.Assert(!Fix64.IsNaN(value.x) && !Fix64.IsNaN(value.y));

                SetVTransform(ref value, Rotation);
            }
        }

        /// <summary>
        /// Get the angle in radians.
        /// </summary>
        /// <returns>Return the current world rotation angle in radians.</returns>
        public Fix64 Rotation
        {
            get => _sweep.A;
            set
            {
                UnityEngine.Debug.Assert(!Fix64.IsNaN(value));

                SetVTransform(ref _xf.p, value);
            }
        }

        //Velcro: We don't add a setter here since it requires a branch, and we only use it internally
        internal bool IsIsland => (_flags & BodyFlags.IslandFlag) == BodyFlags.IslandFlag;

        public bool IsStatic => _type == BodyType.Static;

        public bool IsKinematic => _type == BodyType.Kinematic;

        public bool IsDynamic => _type == BodyType.Dynamic;

        /// <summary>
        /// Gets or sets a value indicating whether this body ignores gravity.
        /// </summary>
        /// <value><c>true</c> if it ignores gravity; otherwise, <c>false</c>.</value>
        public bool IgnoreGravity
        {
            get => (_flags & BodyFlags.IgnoreGravity) == BodyFlags.IgnoreGravity;
            set
            {
                if (value)
                    _flags |= BodyFlags.IgnoreGravity;
                else
                    _flags &= ~BodyFlags.IgnoreGravity;
            }
        }

        /// <summary>
        /// Get the world position of the center of mass.
        /// </summary>
        /// <value>The world position.</value>
        public FVector2 WorldCenter => _sweep.C;

        /// <summary>
        /// Get the local position of the center of mass.
        /// </summary>
        /// <value>The local position.</value>
        public FVector2 LocalCenter
        {
            get => _sweep.LocalCenter;
            set
            {
                if (_type != BodyType.Dynamic)
                    return;

                //Velcro: We support setting the mass independently

                // Move center of mass.
                var oldCenter = _sweep.C;
                _sweep.LocalCenter = value;
                _sweep.C0 = _sweep.C = MathUtils.Mul(ref _xf, ref _sweep.LocalCenter);

                // Update center of mass velocity.
                var a = _sweep.C - oldCenter;
                _linearVelocity += new FVector2(-_angularVelocity * a.y, _angularVelocity * a.x);
            }
        }

        /// <summary>
        /// Gets or sets the mass. Usually in kilograms (kg).
        /// </summary>
        /// <value>The mass.</value>
        public Fix64 Mass
        {
            get => _mass;
            set
            {
                UnityEngine.Debug.Assert(!Fix64.IsNaN(value));

                if (_type != BodyType.Dynamic)
                    return;

                //Velcro: We support setting the mass independently
                _mass = value;

                if (_mass <= Fix64.Zero)
                    _mass = Fix64.One;

                _invMass = Fix64.One / _mass;
            }
        }

        /// <summary>
        /// Get or set the rotational inertia of the body about the local origin. usually in kg-m^2.
        /// </summary>
        /// <value>The inertia.</value>
        public Fix64 Inertia
        {
            get => _inertia + _mass * FVector2.Dot(_sweep.LocalCenter, _sweep.LocalCenter);
            set
            {
                UnityEngine.Debug.Assert(!Fix64.IsNaN(value));

                if (_type != BodyType.Dynamic)
                    return;

                //Velcro: We support setting the inertia independently
                if (value > Fix64.Zero && !FixedRotation)
                {
                    _inertia = value - _mass * FVector2.Dot(_sweep.LocalCenter, _sweep.LocalCenter);
                    UnityEngine.Debug.Assert(_inertia > Fix64.Zero);
                    _invI = Fix64.One / _inertia;
                }
            }
        }

        public Fix64 Restitution
        {
            set
            {
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.Restitution = value;
                }
            }
        }

        public Fix64 Friction
        {
            set
            {
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.Friction = value;
                }
            }
        }

        public Category CollisionCategories
        {
            set
            {
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.CollisionCategories = value;
                }
            }
        }

        public Category CollidesWith
        {
            set
            {
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.CollidesWith = value;
                }
            }
        }

        /// <summary>
        /// Body objects can define which categories of bodies they wish to ignore CCD with.
        /// This allows certain bodies to be configured to ignore CCD with objects that
        /// aren't a penetration problem due to the way content has been prepared.
        /// This is compared against the other Body's fixture CollisionCategories within World.SolveTOI().
        /// </summary>
        public Category IgnoreCCDWith
        {
            set
            {
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.IgnoreCCDWith = value;
                }
            }
        }

        public short CollisionGroup
        {
            set
            {
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.CollisionGroup = value;
                }
            }
        }

        public bool IsSensor
        {
            set
            {
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.IsSensor = value;
                }
            }
        }

        public bool IgnoreCCD
        {
            get => (_flags & BodyFlags.IgnoreCCD) == BodyFlags.IgnoreCCD;
            set
            {
                if (value)
                    _flags |= BodyFlags.IgnoreCCD;
                else
                    _flags &= ~BodyFlags.IgnoreCCD;
            }
        }

        //spax's addition
        public UnityEngine.GameObject gameObject;
        public bool IsPushBox
        {
            set
            {
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.isPushBox = value;
                }


            }
        }


        public bool IsActivePushbox
        {
            set
            {
                if (FixtureList[0].isActivePushbox != value)
                {
                    this.Enabled = false;
                    this.Enabled = true;
                }
                for (var i = 0; i < FixtureList.Count; i++)
                {
                    var f = FixtureList[i];
                    f.isActivePushbox = value;
                }

            }
        }
        //end of Spax's additons

        /// <summary>
        /// Resets the dynamics of this body.
        /// Sets torque, force and linear/angular velocity to 0
        /// </summary>
        public void ResetDynamics()
        {
            _torque = 0;
            _angularVelocity = 0;
            _force = FVector2.zero;
            _linearVelocity = FVector2.zero;
        }

        /// <summary>
        /// Creates a fixture and attach it to this body.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// Contacts are not created until the next time step.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        public Fixture CreateFixture(FixtureTemplate template)
        {
            var f = new Fixture(this, template);
            f.FixtureId = _world._fixtureIdCounter++;
            return f;
        }

        public Fixture CreateFixture(Shape shape, object userData = null)
        {
            var template = new FixtureTemplate();
            template.Shape = shape;
            template.UserData = userData;

            return CreateFixture(template);
        }

        /// <summary>
        /// Destroy a fixture. This removes the fixture from the broad-phase and
        /// destroys all contacts associated with this fixture. This will
        /// automatically adjust the mass of the body if the body is dynamic and the
        /// fixture has positive density.
        /// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        /// <param name="fixture">The fixture to be removed.</param>
        public void DestroyFixture(Fixture fixture)
        {
            if (fixture == null)
                return;

            UnityEngine.Debug.Assert(fixture.Body == this);

            // Remove the fixture from this body's singly linked list.
            UnityEngine.Debug.Assert(FixtureList.Count > 0);

            // You tried to remove a fixture that not present in the fixturelist.
            UnityEngine.Debug.Assert(FixtureList.Contains(fixture));

            // Destroy any contacts associated with the fixture.
            var edge = ContactList;
            while (edge != null)
            {
                var c = edge.Contact;
                edge = edge.Next;

                var fixtureA = c.FixtureA;
                var fixtureB = c.FixtureB;

                if (fixture == fixtureA || fixture == fixtureB)
                    // This destroys the contact and removes it from
                    // this body's contact list.
                    _world.ContactManager.Destroy(c);
            }

            if (Enabled)
            {
                var broadPhase = _world.ContactManager.BroadPhase;
                fixture.DestroyProxies(broadPhase);
            }

            FixtureList.Remove(fixture);
            fixture.Destroy();
            fixture.Body = null;

            ResetMassData();
        }

        //Spax's additions
        public void DestroyContactsOnFixture(Fixture fixture)
        {
            if (fixture == null)
                return;

            //UnityEngine.Debug.Assert(fixture.Body == this);

            // Remove the fixture from this body's singly linked list.
            UnityEngine.Debug.Assert(FixtureList.Count > 0);

            // You tried to remove a fixture that not present in the fixturelist.
            UnityEngine.Debug.Assert(FixtureList.Contains(fixture));

            // Destroy any contacts associated with the fixture.
            var edge = ContactList;
            while (edge != null)
            {
                var c = edge.Contact;
                edge = edge.Next;

                var fixtureA = c.FixtureA;
                var fixtureB = c.FixtureB;

                if (fixture == fixtureA || fixture == fixtureB)
                    // This destroys the contact and removes it from
                    // this body's contact list.
                    _world.ContactManager.Destroy(c);
            }

            if (Enabled)
            {
                var broadPhase = _world.ContactManager.BroadPhase;
                fixture.DestroyProxies(broadPhase);
            }
        }

        /// <summary>
        /// Set the position of the body's origin and rotation.
        /// This breaks any contacts and wakes the other bodies.
        /// Manipulating a body's VTransform may cause non-physical behavior.
        /// </summary>
        /// <param name="position">The world position of the body's local origin.</param>
        /// <param name="rotation">The world rotation in radians.</param>
        public void SetVTransform(ref FVector2 position, Fix64 rotation, bool clearContacts = false)
        {
            SetVTransformIgnoreContacts(ref position, rotation);

            //spax's addition

            FindNewContacts();
        }

        //spax's addition
        public void FindNewContacts()
        {
            //we need to set this to awake if we want collisions to be calculated
            //after we change positions -Spax
            if (!Awake) Awake = true;
            //Velcro: We check for new contacts after a body has been moved.
            _world.ContactManager.FindNewContacts();
        }

        /// <summary>
        /// Set the position of the body's origin and rotation.
        /// This breaks any contacts and wakes the other bodies.
        /// Manipulating a body's VTransform may cause non-physical behavior.
        /// </summary>
        /// <param name="position">The world position of the body's local origin.</param>
        /// <param name="rotation">The world rotation in radians.</param>
        public void SetVTransform(FVector2 position, Fix64 rotation)
        {
            SetVTransform(ref position, rotation);
        }

        /// <summary>
        /// For teleporting a body without considering new contacts immediately.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="angle">The angle.</param>
        public void SetVTransformIgnoreContacts(ref FVector2 position, Fix64 angle)
        {
            _xf.q.Set(angle);
            _xf.p = position;

            _sweep.C = MathUtils.Mul(ref _xf, _sweep.LocalCenter);
            _sweep.A = angle;

            _sweep.C0 = _sweep.C;
            _sweep.A0 = angle;

            var broadPhase = _world.ContactManager.BroadPhase;
            for (var i = 0; i < FixtureList.Count; i++) FixtureList[i].Synchronize(broadPhase, ref _xf, ref _xf);
        }

        /// <summary>
        /// Get the body VTransform for the body's origin.
        /// </summary>
        /// <param name="VTransform">The VTransform of the body's origin.</param>
        public void GetVTransform(out VTransform VTransform)
        {
            VTransform = _xf;
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// </summary>
        /// <param name="force">The world force vector, usually in Newtons (N).</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyForce(FVector2 force, FVector2 point)
        {
            ApplyForce(ref force, ref point);
        }

        /// <summary>
        /// Applies a force at the center of mass.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(ref FVector2 force)
        {
            ApplyForce(ref force, ref _xf.p);
        }

        /// <summary>
        /// Applies a force at the center of mass.
        /// </summary>
        /// <param name="force">The force.</param>
        public void ApplyForce(FVector2 force)
        {
            ApplyForce(ref force, ref _xf.p);
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// </summary>
        /// <param name="force">The world force vector, usually in Newtons (N).</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyForce(ref FVector2 force, ref FVector2 point)
        {
            UnityEngine.Debug.Assert(!Fix64.IsNaN(force.x));
            UnityEngine.Debug.Assert(!Fix64.IsNaN(force.y));
            UnityEngine.Debug.Assert(!Fix64.IsNaN(point.x));
            UnityEngine.Debug.Assert(!Fix64.IsNaN(point.y));

            if (_type != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _force += force;
            _torque += MathUtils.Cross(point - _sweep.C, force);
        }

        /// <summary>
        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// </summary>
        /// <param name="torque">The torque about the z-axis (out of the screen), usually in N-m.</param>
        public void ApplyTorque(Fix64 torque)
        {
            UnityEngine.Debug.Assert(!Fix64.IsNaN(torque));

            if (BodyType != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _torque += torque;
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        public void ApplyLinearImpulse(FVector2 impulse)
        {
            ApplyLinearImpulse(ref impulse);
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass.
        /// This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyLinearImpulse(FVector2 impulse, FVector2 point)
        {
            ApplyLinearImpulse(ref impulse, ref point);
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        public void ApplyLinearImpulse(ref FVector2 impulse)
        {
            if (_type != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _linearVelocity += _invMass * impulse;
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass.
        /// This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyLinearImpulse(ref FVector2 impulse, ref FVector2 point)
        {
            if (_type != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _linearVelocity += _invMass * impulse;
            _angularVelocity += _invI * MathUtils.Cross(point - _sweep.C, impulse);
        }

        /// <summary>
        /// Apply an angular impulse.
        /// </summary>
        /// <param name="impulse">The angular impulse in units of kg*m*m/s.</param>
        public void ApplyAngularImpulse(Fix64 impulse)
        {
            if (_type != BodyType.Dynamic)
                return;

            //Velcro: We always wake the body. You told it to move.
            if (Awake == false)
                Awake = true;

            _angularVelocity += _invI * impulse;
        }

        /// <summary>
        /// This resets the mass properties to the sum of the mass properties of the fixtures.
        /// This normally does not need to be called unless you called SetMassData to override
        /// the mass and you later want to reset the mass.
        /// </summary>
        public void ResetMassData()
        {
            // Compute mass data from shapes. Each shape has its own density.
            _mass = Fix64.Zero;
            _invMass = Fix64.Zero;
            _inertia = Fix64.Zero;
            _invI = Fix64.Zero;
            _sweep.LocalCenter = FVector2.zero;

            //Velcro: We have mass on static bodies to support attaching VJoints to them
            // Kinematic bodies have zero mass.
            if (BodyType == BodyType.Kinematic)
            {
                _sweep.C0 = _xf.p;
                _sweep.C = _xf.p;
                _sweep.A0 = _sweep.A;
                return;
            }

            UnityEngine.Debug.Assert(BodyType == BodyType.Dynamic || BodyType == BodyType.Static);

            // Accumulate mass over all fixtures.
            var localCenter = FVector2.zero;
            foreach (var f in FixtureList)
            {
                if (f.Shape._density == Fix64.Zero)
                    continue;

                var massData = f.Shape.MassData;
                _mass += massData.Mass;
                localCenter += massData.Mass * massData.Centroid;
                _inertia += massData.Inertia;
            }

            //Velcro: Static bodies only have mass, they don't have other properties. A little hacky tho...
            if (BodyType == BodyType.Static)
            {
                _sweep.C0 = _sweep.C = _xf.p;
                return;
            }

            // Compute center of mass.
            if (_mass > Fix64.Zero)
            {
                _invMass = Fix64.One / _mass;
                localCenter *= _invMass;
            }
            else
            {
                // Force all bodies to have a positive mass.
                _mass = Fix64.One;
                _invMass = Fix64.One;
            }

            if (_inertia > Fix64.Zero && (_flags & BodyFlags.FixedRotationFlag) == 0)
            {
                // Center the inertia about the center of mass.
                _inertia -= _mass * FVector2.Dot(localCenter, localCenter);

                UnityEngine.Debug.Assert(_inertia > Fix64.Zero);
                _invI = Fix64.One / _inertia;
            }
            else
            {
                _inertia = Fix64.Zero;
                _invI = Fix64.Zero;
            }

            // Move center of mass.
            var oldCenter = _sweep.C;
            _sweep.LocalCenter = localCenter;
            _sweep.C0 = _sweep.C = MathUtils.Mul(ref _xf, ref _sweep.LocalCenter);

            // Update center of mass velocity.
            var a = _sweep.C - oldCenter;
            _linearVelocity += new FVector2(-_angularVelocity * a.y, _angularVelocity * a.x);
        }

        /// <summary>
        /// Get the world coordinates of a point given the local coordinates.
        /// </summary>
        /// <param name="localPoint">A point on the body measured relative the body's origin.</param>
        /// <returns>The same point expressed in world coordinates.</returns>
        public FVector2 GetWorldPoint(ref FVector2 localPoint)
        {
            return MathUtils.Mul(ref _xf, ref localPoint);
        }

        /// <summary>
        /// Get the world coordinates of a point given the local coordinates.
        /// </summary>
        /// <param name="localPoint">A point on the body measured relative the body's origin.</param>
        /// <returns>The same point expressed in world coordinates.</returns>
        public FVector2 GetWorldPoint(FVector2 localPoint)
        {
            return GetWorldPoint(ref localPoint);
        }

        /// <summary>
        /// Get the world coordinates of a vector given the local coordinates.
        /// Note that the vector only takes the rotation into account, not the position.
        /// </summary>
        /// <param name="localVector">A vector fixed in the body.</param>
        /// <returns>The same vector expressed in world coordinates.</returns>
        public FVector2 GetWorldVector(ref FVector2 localVector)
        {
            return MathUtils.Mul(ref _xf.q, localVector);
        }

        /// <summary>
        /// Get the world coordinates of a vector given the local coordinates.
        /// </summary>
        /// <param name="localVector">A vector fixed in the body.</param>
        /// <returns>The same vector expressed in world coordinates.</returns>
        public FVector2 GetWorldVector(FVector2 localVector)
        {
            return GetWorldVector(ref localVector);
        }

        /// <summary>
        /// Gets a local point relative to the body's origin given a world point.
        /// Note that the vector only takes the rotation into account, not the position.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The corresponding local point relative to the body's origin.</returns>
        public FVector2 GetLocalPoint(ref FVector2 worldPoint)
        {
            return MathUtils.MulT(ref _xf, worldPoint);
        }

        /// <summary>
        /// Gets a local point relative to the body's origin given a world point.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The corresponding local point relative to the body's origin.</returns>
        public FVector2 GetLocalPoint(FVector2 worldPoint)
        {
            return GetLocalPoint(ref worldPoint);
        }

        /// <summary>
        /// Gets a local vector given a world vector.
        /// Note that the vector only takes the rotation into account, not the position.
        /// </summary>
        /// <param name="worldVector">A vector in world coordinates.</param>
        /// <returns>The corresponding local vector.</returns>
        public FVector2 GetLocalVector(ref FVector2 worldVector)
        {
            return MathUtils.MulT(_xf.q, worldVector);
        }

        /// <summary>
        /// Gets a local vector given a world vector.
        /// Note that the vector only takes the rotation into account, not the position.
        /// </summary>
        /// <param name="worldVector">A vector in world coordinates.</param>
        /// <returns>The corresponding local vector.</returns>
        public FVector2 GetLocalVector(FVector2 worldVector)
        {
            return GetLocalVector(ref worldVector);
        }

        /// <summary>
        /// Get the world linear velocity of a world point attached to this body.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public FVector2 GetLinearVelocityFromWorldPoint(FVector2 worldPoint)
        {
            return GetLinearVelocityFromWorldPoint(ref worldPoint);
        }

        /// <summary>
        /// Get the world linear velocity of a world point attached to this body.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public FVector2 GetLinearVelocityFromWorldPoint(ref FVector2 worldPoint)
        {
            return _linearVelocity + MathUtils.Cross(_angularVelocity, worldPoint - _sweep.C);
        }

        /// <summary>
        /// Get the world velocity of a local point.
        /// </summary>
        /// <param name="localPoint">A point in local coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public FVector2 GetLinearVelocityFromLocalPoint(FVector2 localPoint)
        {
            return GetLinearVelocityFromLocalPoint(ref localPoint);
        }

        /// <summary>
        /// Get the world velocity of a local point.
        /// </summary>
        /// <param name="localPoint">A point in local coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public FVector2 GetLinearVelocityFromLocalPoint(ref FVector2 localPoint)
        {
            return GetLinearVelocityFromWorldPoint(GetWorldPoint(ref localPoint));
        }

        internal void SynchronizeFixtures()
        {
            var xf1 = new VTransform();
            xf1.q.Set(_sweep.A0);
            xf1.p = _sweep.C0 - MathUtils.Mul(xf1.q, _sweep.LocalCenter);

            var broadPhase = _world.ContactManager.BroadPhase;
            for (var i = 0; i < FixtureList.Count; i++) FixtureList[i].Synchronize(broadPhase, ref xf1, ref _xf);
        }

        internal void SynchronizeVTransform()
        {
            _xf.q.Set(_sweep.A);
            _xf.p = _sweep.C - MathUtils.Mul(_xf.q, _sweep.LocalCenter);
        }

        /// <summary>
        /// This is used to prevent connected bodies from colliding.
        /// It may lie, depending on the collideConnected flag.
        /// </summary>
        /// <param name="other">The other body.</param>
        internal bool ShouldCollide(Body other)
        {
            // At least one body should be dynamic.
            if (_type != BodyType.Dynamic && other._type != BodyType.Dynamic) return false;

            // Does a VJoint prevent collision?
            for (var jn = VJointList; jn != null; jn = jn.Next)
                if (jn.Other == other)
                    if (jn.VJoint.CollideConnected == false)
                        return false;

            return true;
        }

        internal void Advance(Fix64 alpha)
        {
            // Advance to the new safe time. This doesn't sync the broad-phase.
            _sweep.Advance(alpha);
            _sweep.C = _sweep.C0;
            _sweep.A = _sweep.A0;
            _xf.q.Set(_sweep.A);
            _xf.p = _sweep.C - MathUtils.Mul(_xf.q, _sweep.LocalCenter);
        }

        public event OnCollisionHandler OnCollision
        {
            add
            {
                foreach (var f in FixtureList) f.OnCollision += value;
            }
            remove
            {
                foreach (var f in FixtureList) f.OnCollision -= value;
            }
        }

        public event OnSeparationHandler OnSeparation
        {
            add
            {
                foreach (var f in FixtureList) f.OnSeparation += value;
            }
            remove
            {
                foreach (var f in FixtureList) f.OnSeparation -= value;
            }
        }

        //spax's additions
        public event OnCollisionHandler ContCollision
        {
            add
            {
                foreach (var f in FixtureList) f.ContCollision += value;
            }
            remove
            {
                foreach (var f in FixtureList) f.ContCollision -= value;
            }
        }
    }
}