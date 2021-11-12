﻿/*
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
using System.Collections.Generic;
using System.Diagnostics;
using VelcroPhysics.Collision.Broadphase;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Distance;
using VelcroPhysics.Collision.RayCast;
using VelcroPhysics.Collision.TOI;
using VelcroPhysics.Dynamics.Handlers;
using VelcroPhysics.Dynamics.VJoints;
using VelcroPhysics.Dynamics.Solver;
using VelcroPhysics.Extensions.Controllers.ControllerBase;
using VelcroPhysics.Shared;
using VelcroPhysics.Templates;
using FixMath.NET;
using Debug = UnityEngine.Debug;

namespace VelcroPhysics.Dynamics
{
    /// <summary>
    /// The world class manages all physics entities, dynamic simulation,
    /// and asynchronous queries.
    /// </summary>
    public class World
    {
        private Fix64 _invDt0;
        private Body[] _stack = new Body[64];
        private bool _stepComplete;
        private HashSet<Body> _bodyAddList = new HashSet<Body>();
        private HashSet<Body> _bodyRemoveList = new HashSet<Body>();
        private HashSet<VJoint> _VJointAddList = new HashSet<VJoint>();
        private HashSet<VJoint> _VJointRemoveList = new HashSet<VJoint>();
        private Func<Fixture, bool> _queryAABBCallback;
        private Func<int, bool> _queryAABBCallbackWrapper;
        private Fixture _myFixture;
        private FVector2 _point1;
        private FVector2 _point2;
        private List<Fixture> _testPointAllFixtures;
        private Stopwatch _watch = new Stopwatch();
        private Func<Fixture, FVector2, FVector2, Fix64, Fix64> _rayCastCallback;
        private Func<RayCastInput, int, Fix64> _rayCastCallbackWrapper;

        internal Queue<Contact> _contactPool = new Queue<Contact>(256);
        internal bool _worldHasNewFixture;

        internal int _bodyIdCounter;
        internal int _fixtureIdCounter;

        /// <summary>
        /// Fires whenever a body has been added
        /// </summary>
        public BodyHandler BodyAdded;

        /// <summary>
        /// Fires whenever a body has been removed
        /// </summary>
        public BodyHandler BodyRemoved;

        /// <summary>
        /// Fires whenever a fixture has been added
        /// </summary>
        public FixtureHandler FixtureAdded;

        /// <summary>
        /// Fires whenever a fixture has been removed
        /// </summary>
        public FixtureHandler FixtureRemoved;

        /// <summary>
        /// Fires whenever a VJoint has been added
        /// </summary>
        public VJointHandler VJointAdded;

        /// <summary>
        /// Fires whenever a VJoint has been removed
        /// </summary>
        public VJointHandler VJointRemoved;

        /// <summary>
        /// Fires every time a controller is added to the World.
        /// </summary>
        public ControllerHandler ControllerAdded;

        /// <summary>
        /// Fires every time a controller is removed form the World.
        /// </summary>
        public ControllerHandler ControllerRemoved;

        /// <summary>
        /// Initializes a new instance of the <see cref="World" /> class.
        /// </summary>
        public World(FVector2 gravity)
        {
            Island = new Island();
            Enabled = true;
            ControllerList = new List<Controller>();
            BreakableBodyList = new List<BreakableBody>();
            BodyList = new List<Body>(32);
            VJointList = new List<VJoint>(32);

            _queryAABBCallbackWrapper = QueryAABBCallbackWrapper;
            _rayCastCallbackWrapper = RayCastCallbackWrapper;

            ContactManager = new ContactManager(new DynamicTreeBroadPhase());
            Gravity = gravity;
        }

        private void ProcessRemovedVJoints()
        {
            if (_VJointRemoveList.Count > 0)
            {
                foreach (var VJoint in _VJointRemoveList)
                {
                    var collideConnected = VJoint.CollideConnected;

                    // Remove from the world list.
                    VJointList.Remove(VJoint);

                    // Disconnect from island graph.
                    var bodyA = VJoint.BodyA;
                    var bodyB = VJoint.BodyB;

                    // Wake up connected bodies.
                    bodyA.Awake = true;

                    // WIP David
                    if (!VJoint.IsFixedType()) bodyB.Awake = true;

                    // Remove from body 1.
                    if (VJoint.EdgeA.Prev != null) VJoint.EdgeA.Prev.Next = VJoint.EdgeA.Next;

                    if (VJoint.EdgeA.Next != null) VJoint.EdgeA.Next.Prev = VJoint.EdgeA.Prev;

                    if (VJoint.EdgeA == bodyA.VJointList) bodyA.VJointList = VJoint.EdgeA.Next;

                    VJoint.EdgeA.Prev = null;
                    VJoint.EdgeA.Next = null;

                    // WIP David
                    if (!VJoint.IsFixedType())
                    {
                        // Remove from body 2
                        if (VJoint.EdgeB.Prev != null) VJoint.EdgeB.Prev.Next = VJoint.EdgeB.Next;

                        if (VJoint.EdgeB.Next != null) VJoint.EdgeB.Next.Prev = VJoint.EdgeB.Prev;

                        if (VJoint.EdgeB == bodyB.VJointList) bodyB.VJointList = VJoint.EdgeB.Next;

                        VJoint.EdgeB.Prev = null;
                        VJoint.EdgeB.Next = null;

                        // If the VJoint prevents collisions, then flag any contacts for filtering.
                        if (collideConnected == false)
                        {
                            var edge = bodyB.ContactList;
                            while (edge != null)
                            {
                                if (edge.Other == bodyA)
                                    // Flag the contact for filtering at the next time step (where either
                                    // body is awake).
                                    edge.Contact._flags |= ContactFlags.FilterFlag;

                                edge = edge.Next;
                            }
                        }
                    }

                    VJointRemoved?.Invoke(VJoint);
                }

                _VJointRemoveList.Clear();
            }
        }

        private void ProcessAddedVJoints()
        {
            if (_VJointAddList.Count > 0)
            {
                foreach (var VJoint in _VJointAddList)
                {
                    // Connect to the world list.
                    VJointList.Add(VJoint);

                    // Connect to the bodies' doubly linked lists.
                    VJoint.EdgeA.VJoint = VJoint;
                    VJoint.EdgeA.Other = VJoint.BodyB;
                    VJoint.EdgeA.Prev = null;
                    VJoint.EdgeA.Next = VJoint.BodyA.VJointList;

                    if (VJoint.BodyA.VJointList != null)
                        VJoint.BodyA.VJointList.Prev = VJoint.EdgeA;

                    VJoint.BodyA.VJointList = VJoint.EdgeA;

                    // WIP David
                    if (!VJoint.IsFixedType())
                    {
                        VJoint.EdgeB.VJoint = VJoint;
                        VJoint.EdgeB.Other = VJoint.BodyA;
                        VJoint.EdgeB.Prev = null;
                        VJoint.EdgeB.Next = VJoint.BodyB.VJointList;

                        if (VJoint.BodyB.VJointList != null)
                            VJoint.BodyB.VJointList.Prev = VJoint.EdgeB;

                        VJoint.BodyB.VJointList = VJoint.EdgeB;

                        var bodyA = VJoint.BodyA;
                        var bodyB = VJoint.BodyB;

                        // If the VJoint prevents collisions, then flag any contacts for filtering.
                        if (VJoint.CollideConnected == false)
                        {
                            var edge = bodyB.ContactList;
                            while (edge != null)
                            {
                                if (edge.Other == bodyA)
                                    // Flag the contact for filtering at the next time step (where either
                                    // body is awake).
                                    edge.Contact._flags |= ContactFlags.FilterFlag;

                                edge = edge.Next;
                            }
                        }
                    }

                    VJointAdded?.Invoke(VJoint);

                    // Note: creating a VJoint doesn't wake the bodies.
                }

                _VJointAddList.Clear();
            }
        }

        private void ProcessAddedBodies()
        {
            if (_bodyAddList.Count > 0)
            {
                foreach (var body in _bodyAddList)
                {
                    // Add to world list.
                    BodyList.Add(body);

                    BodyAdded?.Invoke(body);
                }

                _bodyAddList.Clear();
            }
        }

        private void ProcessRemovedBodies()
        {
            if (_bodyRemoveList.Count > 0)
            {
                foreach (var body in _bodyRemoveList)
                {
                    UnityEngine.Debug.Assert(BodyList.Count > 0);

                    // You tried to remove a body that is not contained in the BodyList.
                    // Are you removing the body more than once?
                    UnityEngine.Debug.Assert(BodyList.Contains(body));

                    // Delete the attached VJoints.
                    var je = body.VJointList;
                    while (je != null)
                    {
                        var je0 = je;
                        je = je.Next;

                        RemoveVJoint(je0.VJoint, false);
                    }

                    body.VJointList = null;

                    // Delete the attached contacts.
                    var ce = body.ContactList;
                    while (ce != null)
                    {
                        var ce0 = ce;
                        ce = ce.Next;
                        ContactManager.Destroy(ce0.Contact);
                    }

                    body.ContactList = null;

                    // Delete the attached fixtures. This destroys broad-phase proxies.
                    for (var i = 0; i < body.FixtureList.Count; i++)
                    {
                        body.FixtureList[i].DestroyProxies(ContactManager.BroadPhase);
                        body.FixtureList[i].Destroy();
                    }

                    body.FixtureList = null;

                    // Remove world body list.
                    BodyList.Remove(body);

                    BodyRemoved?.Invoke(body);
                }

                _bodyRemoveList.Clear();
            }
        }

        private bool QueryAABBCallbackWrapper(int proxyId)
        {
            var proxy = ContactManager.BroadPhase.GetProxy(proxyId);
            return _queryAABBCallback(proxy.Fixture);
        }

        private Fix64 RayCastCallbackWrapper(RayCastInput rayCastInput, int proxyId)
        {
            var proxy = ContactManager.BroadPhase.GetProxy(proxyId);
            var fixture = proxy.Fixture;
            var index = proxy.ChildIndex;
            RayCastOutput output;
            var hit = fixture.RayCast(out output, ref rayCastInput, index);

            if (hit)
            {
                var fraction = output.Fraction;
                var point = (Fix64.One - fraction) * rayCastInput.Point1 + fraction * rayCastInput.Point2;
                return _rayCastCallback(fixture, point, output.Normal, fraction);
            }

            return rayCastInput.MaxFraction;
        }

        private void Solve(ref TimeStep step)
        {
            // Size the island for the worst case.
            Island.Reset(BodyList.Count,
                ContactManager.ContactList.Count,
                VJointList.Count,
                ContactManager);

            // Clear all the island flags.
            foreach (var b in BodyList) b._flags &= ~BodyFlags.IslandFlag;

            foreach (var c in ContactManager.ContactList) c._flags &= ~ContactFlags.IslandFlag;

            foreach (var j in VJointList) j.IslandFlag = false;

            // Build and simulate all awake islands.
            var stackSize = BodyList.Count;
            if (stackSize > _stack.Length)
                _stack = new Body[UnityEngine.Mathf.Max(_stack.Length * 2, stackSize)];

            for (var index = BodyList.Count - 1; index >= 0; index--)
            {
                var seed = BodyList[index];
                if ((seed._flags & BodyFlags.IslandFlag) == BodyFlags.IslandFlag) continue;

                if (seed.Awake == false || seed.Enabled == false) continue;

                // The seed can be dynamic or kinematic.
                if (seed.BodyType == BodyType.Static) continue;

                // Reset island and stack.
                Island.Clear();
                var stackCount = 0;
                _stack[stackCount++] = seed;

                seed._flags |= BodyFlags.IslandFlag;

                // Perform a depth first search (DFS) on the constraint graph.
                while (stackCount > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    var b = _stack[--stackCount];
                    UnityEngine.Debug.Assert(b.Enabled);
                    Island.Add(b);

                    // Make sure the body is awake (without resetting sleep timer).
                    b._flags |= BodyFlags.AwakeFlag;

                    // To keep islands as small as possible, we don't
                    // propagate islands across static bodies.
                    if (b.BodyType == BodyType.Static) continue;

                    // Search all contacts connected to this body.
                    for (var ce = b.ContactList; ce != null; ce = ce.Next)
                    {
                        var contact = ce.Contact;

                        // Has this contact already been added to an island?
                        if (contact.IslandFlag) continue;

                        // Is this contact solid and touching?
                        if (ce.Contact.Enabled == false || ce.Contact.IsTouching == false) continue;

                        // Skip sensors.
                        var sensorA = contact.FixtureA.IsSensor;
                        var sensorB = contact.FixtureB.IsSensor;
                        if (sensorA || sensorB) continue;

                        Island.Add(contact);
                        contact._flags |= ContactFlags.IslandFlag;

                        var other = ce.Other;

                        // Was the other body already added to this island?
                        if (other.IsIsland) continue;

                        UnityEngine.Debug.Assert(stackCount < stackSize);
                        _stack[stackCount++] = other;

                        other._flags |= BodyFlags.IslandFlag;
                    }

                    // Search all VJoints connect to this body.
                    for (var je = b.VJointList; je != null; je = je.Next)
                    {
                        if (je.VJoint.IslandFlag) continue;

                        var other = je.Other;

                        // WIP David
                        //Enter here when it's a non-fixed VJoint. Non-fixed VJoints have a other body.
                        if (other != null)
                        {
                            // Don't simulate VJoints connected to inactive bodies.
                            if (other.Enabled == false) continue;

                            Island.Add(je.VJoint);
                            je.VJoint.IslandFlag = true;

                            if (other.IsIsland) continue;

                            UnityEngine.Debug.Assert(stackCount < stackSize);
                            _stack[stackCount++] = other;

                            other._flags |= BodyFlags.IslandFlag;
                        }
                        else
                        {
                            Island.Add(je.VJoint);
                            je.VJoint.IslandFlag = true;
                        }
                    }
                }

                Island.Solve(ref step, ref Gravity);

                // Post solve cleanup.
                for (var i = 0; i < Island.BodyCount; ++i)
                {
                    // Allow static bodies to participate in other islands.
                    var b = Island.Bodies[i];
                    if (b.BodyType == BodyType.Static) b._flags &= ~BodyFlags.IslandFlag;
                }
            }

            // Synchronize fixtures, check for out of range bodies.

            foreach (var b in BodyList)
            {
                // If a body was not in an island then it did not move.
                if (!b.IsIsland) continue;

                if (b.BodyType == BodyType.Static) continue;

                // Update fixtures (for broad-phase).
                b.SynchronizeFixtures();
            }

            // Look for new contacts.
            ContactManager.FindNewContacts();
        }

        private void SolveTOI(ref TimeStep step)
        {
            Island.Reset(2 * Settings.MaxTOIContacts, Settings.MaxTOIContacts, 0, ContactManager);

            if (_stepComplete)
            {
                for (var i = 0; i < BodyList.Count; i++)
                {
                    BodyList[i]._flags &= ~BodyFlags.IslandFlag;
                    BodyList[i]._sweep.Alpha0 = Fix64.Zero;
                }

                for (var i = 0; i < ContactManager.ContactList.Count; i++)
                {
                    var c = ContactManager.ContactList[i];

                    // Invalidate TOI
                    c._flags &= ~ContactFlags.IslandFlag;
                    c._flags &= ~ContactFlags.TOIFlag;
                    c._toiCount = 0;
                    c._toi = Fix64.One;
                }
            }

            // Find TOI events and solve them.
            for (; ; )
            {
                // Find the first TOI.
                Contact minContact = null;
                var minAlpha = Fix64.One;

                for (var i = 0; i < ContactManager.ContactList.Count; i++)
                {
                    var c = ContactManager.ContactList[i];

                    // Is this contact disabled?
                    if (c.Enabled == false) continue;

                    // Prevent excessive sub-stepping.
                    if (c._toiCount > Settings.MaxSubSteps) continue;

                    Fix64 alpha;
                    if (c.TOIFlag)
                    {
                        // This contact has a valid cached TOI.
                        alpha = c._toi;
                    }
                    else
                    {
                        var fA = c.FixtureA;
                        var fB = c.FixtureB;

                        // Is there a sensor?
                        if (fA.IsSensor || fB.IsSensor) continue;

                        var bA = fA.Body;
                        var bB = fB.Body;

                        var typeA = bA.BodyType;
                        var typeB = bB.BodyType;
                        UnityEngine.Debug.Assert(typeA == BodyType.Dynamic || typeB == BodyType.Dynamic);

                        var activeA = bA.Awake && typeA != BodyType.Static;
                        var activeB = bB.Awake && typeB != BodyType.Static;

                        // Is at least one body active (awake and dynamic or kinematic)?
                        if (activeA == false && activeB == false) continue;

                        var collideA = (bA.IsBullet || typeA != BodyType.Dynamic) &&
                                       (fA.IgnoreCCDWith & fB.CollisionCategories) == 0 && !bA.IgnoreCCD;
                        var collideB = (bB.IsBullet || typeB != BodyType.Dynamic) &&
                                       (fB.IgnoreCCDWith & fA.CollisionCategories) == 0 && !bB.IgnoreCCD;

                        // Are these two non-bullet dynamic bodies?
                        if (collideA == false && collideB == false) continue;

                        // Compute the TOI for this contact.
                        // Put the sweeps onto the same time interval.
                        var alpha0 = bA._sweep.Alpha0;

                        if (bA._sweep.Alpha0 < bB._sweep.Alpha0)
                        {
                            alpha0 = bB._sweep.Alpha0;
                            bA._sweep.Advance(alpha0);
                        }
                        else if (bB._sweep.Alpha0 < bA._sweep.Alpha0)
                        {
                            alpha0 = bA._sweep.Alpha0;
                            bB._sweep.Advance(alpha0);
                        }

                        UnityEngine.Debug.Assert(alpha0 < Fix64.One);

                        // Compute the time of impact in interval [0, minTOI]
                        var input = new TOIInput();
                        input.ProxyA = new DistanceProxy(fA.Shape, c.ChildIndexA);
                        input.ProxyB = new DistanceProxy(fB.Shape, c.ChildIndexB);
                        input.SweepA = bA._sweep;
                        input.SweepB = bB._sweep;
                        input.TMax = Fix64.One;

                        TOIOutput output;
                        TimeOfImpact.CalculateTimeOfImpact(ref input, out output);

                        // Beta is the fraction of the remaining portion of the .
                        var beta = output.T;
                        if (output.State == TOIOutputState.Touching)
                            alpha = Fix64.Min(alpha0 + (Fix64.One - alpha0) * beta, Fix64.One);
                        else
                            alpha = Fix64.One;

                        c._toi = alpha;
                        c._flags &= ~ContactFlags.TOIFlag;
                    }

                    if (alpha < minAlpha)
                    {
                        // This is the minimum TOI found so far.
                        minContact = c;
                        minAlpha = alpha;
                    }
                }

                if (minContact == null || Fix64.One - 10 * Settings.Epsilon < minAlpha)
                {
                    // No more TOI events. Done!
                    _stepComplete = true;
                    break;
                }

                // Advance the bodies to the TOI.
                var fA1 = minContact.FixtureA;
                var fB1 = minContact.FixtureB;
                var bA0 = fA1.Body;
                var bB0 = fB1.Body;

                var backup1 = bA0._sweep;
                var backup2 = bB0._sweep;

                bA0.Advance(minAlpha);
                bB0.Advance(minAlpha);

                // The TOI contact likely has some new contact points.
                minContact.Update(ContactManager);
                minContact._flags &= ~ContactFlags.TOIFlag;
                ++minContact._toiCount;

                // Is the contact solid?
                if (minContact.Enabled == false || minContact.IsTouching == false)
                {
                    // Restore the sweeps.
                    minContact._flags &= ~ContactFlags.EnabledFlag;
                    bA0._sweep = backup1;
                    bB0._sweep = backup2;
                    bA0.SynchronizeVTransform();
                    bB0.SynchronizeVTransform();
                    continue;
                }

                bA0.Awake = true;
                bB0.Awake = true;

                // Build the island
                Island.Clear();
                Island.Add(bA0);
                Island.Add(bB0);
                Island.Add(minContact);

                bA0._flags |= BodyFlags.IslandFlag;
                bB0._flags |= BodyFlags.IslandFlag;
                minContact._flags &= ~ContactFlags.IslandFlag;

                // Get contacts on bodyA and bodyB.
                Body[] bodies = { bA0, bB0 };
                for (var i = 0; i < 2; ++i)
                {
                    var body = bodies[i];
                    if (body.BodyType == BodyType.Dynamic)
                        for (var ce = body.ContactList; ce != null; ce = ce.Next)
                        {
                            var contact = ce.Contact;

                            if (Island.BodyCount == Island.BodyCapacity) break;

                            if (Island.ContactCount == Island.ContactCapacity) break;

                            // Has this contact already been added to the island?
                            if (contact.IslandFlag) continue;

                            // Only add static, kinematic, or bullet bodies.
                            var other = ce.Other;
                            if (other.BodyType == BodyType.Dynamic &&
                                body.IsBullet == false && other.IsBullet == false)
                                continue;

                            // Skip sensors.
                            if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor) continue;

                            // Tentatively advance the body to the TOI.
                            var backup = other._sweep;
                            if (!other.IsIsland) other.Advance(minAlpha);

                            // Update the contact points
                            contact.Update(ContactManager);

                            // Was the contact disabled by the user?
                            if (contact.Enabled == false)
                            {
                                other._sweep = backup;
                                other.SynchronizeVTransform();
                                continue;
                            }

                            // Are there contact points?
                            if (contact.IsTouching == false)
                            {
                                other._sweep = backup;
                                other.SynchronizeVTransform();
                                continue;
                            }

                            // Add the contact to the island
                            minContact._flags |= ContactFlags.IslandFlag;
                            Island.Add(contact);

                            // Has the other body already been added to the island?
                            if (other.IsIsland) continue;

                            // Add the other body to the island.
                            other._flags |= BodyFlags.IslandFlag;

                            if (other.BodyType != BodyType.Static) other.Awake = true;

                            Island.Add(other);
                        }
                }

                TimeStep subStep;
                subStep.dt = (Fix64.One - minAlpha) * step.dt;
                subStep.inv_dt = Fix64.One / subStep.dt;
                subStep.dtRatio = Fix64.One;
                Island.SolveTOI(ref subStep, bA0.IslandIndex, bB0.IslandIndex);

                // Reset island flags and synchronize broad-phase proxies.
                for (var i = 0; i < Island.BodyCount; ++i)
                {
                    var body = Island.Bodies[i];
                    body._flags &= ~BodyFlags.IslandFlag;

                    if (body.BodyType != BodyType.Dynamic) continue;

                    body.SynchronizeFixtures();

                    // Invalidate all contact TOIs on this displaced body.
                    for (var ce = body.ContactList; ce != null; ce = ce.Next)
                    {
                        ce.Contact._flags &= ~ContactFlags.TOIFlag;
                        ce.Contact._flags &= ~ContactFlags.IslandFlag;
                    }
                }

                // Commit fixture proxy movements to the broad-phase so that new contacts are created.
                // Also, some contacts can be destroyed.
                ContactManager.FindNewContacts();

                if (Settings.EnableSubStepping)
                {
                    _stepComplete = false;
                    break;
                }
            }
        }

        public List<Controller> ControllerList { get; private set; }

        public List<BreakableBody> BreakableBodyList { get; private set; }

        public Fix64 UpdateTime { get; private set; }

        public Fix64 ContinuousPhysicsTime { get; private set; }

        public Fix64 ControllersUpdateTime { get; private set; }

        public Fix64 AddRemoveTime { get; private set; }

        public Fix64 NewContactsTime { get; private set; }

        public Fix64 ContactsUpdateTime { get; private set; }

        public Fix64 SolveUpdateTime { get; private set; }

        /// <summary>
        /// Get the number of broad-phase proxies.
        /// </summary>
        /// <value>The proxy count.</value>
        public int ProxyCount => ContactManager.BroadPhase.ProxyCount;

        /// <summary>
        /// Change the global gravity vector.
        /// </summary>
        /// <value>The gravity.</value>
        public FVector2 Gravity;

        /// <summary>
        /// Get the contact manager for testing.
        /// </summary>
        /// <value>The contact manager.</value>
        public ContactManager ContactManager { get; private set; }

        /// <summary>
        /// Get the world body list.
        /// </summary>
        /// <value>The head of the world body list.</value>
        public List<Body> BodyList { get; private set; }

        /// <summary>
        /// Get the world VJoint list.
        /// </summary>
        /// <value>The VJoint list.</value>
        public List<VJoint> VJointList { get; private set; }

        /// <summary>
        /// Get the world contact list. With the returned contact, use Contact.GetNext to get
        /// the next contact in the world list. A null contact indicates the end of the list.
        /// </summary>
        /// <value>The head of the world contact list.</value>
        public List<Contact> ContactList => ContactManager.ContactList;

        /// <summary>
        /// If false, the whole simulation stops. It still processes added and removed geometries.
        /// </summary>
        public bool Enabled { get; set; }

        public Island Island { get; private set; }

        /// <summary>
        /// Add a rigid body.
        /// </summary>
        /// <returns></returns>
        internal void AddBody(Body body)
        {
            UnityEngine.Debug.Assert(!_bodyAddList.Contains(body), "You are adding the same body more than once.");

            if (!_bodyAddList.Contains(body))
                _bodyAddList.Add(body);
        }

        /// <summary>
        /// Destroy a rigid body.
        /// Warning: This automatically deletes all associated shapes and VJoints.
        /// </summary>
        /// <param name="body">The body.</param>
        public void RemoveBody(Body body)
        {
            UnityEngine.Debug.Assert(!_bodyRemoveList.Contains(body),
                "The body is already marked for removal. You are removing the body more than once.");

            if (!_bodyRemoveList.Contains(body))
                _bodyRemoveList.Add(body);
        }

        /// <summary>
        /// Create a VJoint to constrain bodies together. This may cause the connected bodies to cease colliding.
        /// </summary>
        /// <param name="VJoint">The VJoint.</param>
        public void AddVJoint(VJoint VJoint)
        {
            UnityEngine.Debug.Assert(!_VJointAddList.Contains(VJoint), "You are adding the same VJoint more than once.");

            if (!_VJointAddList.Contains(VJoint))
                _VJointAddList.Add(VJoint);
        }

        private void RemoveVJoint(VJoint VJoint, bool doCheck)
        {
            if (doCheck)
                UnityEngine.Debug.Assert(!_VJointRemoveList.Contains(VJoint),
                    "The VJoint is already marked for removal. You are removing the VJoint more than once.");

            if (!_VJointRemoveList.Contains(VJoint))
                _VJointRemoveList.Add(VJoint);
        }

        /// <summary>
        /// Destroy a VJoint. This may cause the connected bodies to begin colliding.
        /// </summary>
        /// <param name="VJoint">The VJoint.</param>
        public void RemoveVJoint(VJoint VJoint)
        {
            RemoveVJoint(VJoint, true);
        }

        /// <summary>
        /// All adds and removes are cached by the World during a World step.
        /// To process the changes before the world updates again, call this method.
        /// </summary>
        public void ProcessChanges()
        {
            ProcessAddedBodies();
            ProcessAddedVJoints();

            ProcessRemovedBodies();
            ProcessRemovedVJoints();
        }

        /// <summary>
        /// Take a time step. This performs collision detection, integration,
        /// and constraint solution.
        /// </summary>
        /// <param name="dt">The amount of time to simulate, this should not vary.</param>
        public void Step(Fix64 dt)
        {
            if (!Enabled)
                return;

            if (Settings.EnableDiagnostics)
                _watch.Start();

            ProcessChanges();

            if (Settings.EnableDiagnostics)
                AddRemoveTime = (Fix64)_watch.ElapsedTicks;

            // If new fixtures were added, we need to find the new contacts.
            if (_worldHasNewFixture)
            {
                ContactManager.FindNewContacts();
                _worldHasNewFixture = false;
            }

            if (Settings.EnableDiagnostics)
                NewContactsTime = (Fix64)_watch.ElapsedTicks - AddRemoveTime;

            //Velcro only: moved position and velocity iterations into Settings.cs
            TimeStep step;
            step.inv_dt = dt > Fix64.Zero ? Fix64.One / dt : Fix64.Zero;
            step.dt = dt;
            step.dtRatio = _invDt0 * dt;

            //Update controllers
            int len = ControllerList.Count;
            for (var i = 0; i < len; i++) ControllerList[i].Update(dt);

            if (Settings.EnableDiagnostics)
                ControllersUpdateTime = (Fix64)_watch.ElapsedTicks - (Fix64)(AddRemoveTime + NewContactsTime);

            // Update contacts. This is where some contacts are destroyed.
            ContactManager.Collide();

            if (Settings.EnableDiagnostics)
                ContactsUpdateTime = (Fix64)_watch.ElapsedTicks - (AddRemoveTime + NewContactsTime + ControllersUpdateTime);

            // Integrate velocities, solve velocity constraints, and integrate positions.
            Solve(ref step);

            if (Settings.EnableDiagnostics)
                SolveUpdateTime = (Fix64)_watch.ElapsedTicks -
                                  (AddRemoveTime + NewContactsTime + ControllersUpdateTime + ContactsUpdateTime);

            // Handle TOI events.
            if (Settings.ContinuousPhysics) SolveTOI(ref step);

            if (Settings.EnableDiagnostics)
                ContinuousPhysicsTime = (Fix64)_watch.ElapsedTicks -
                                        (AddRemoveTime + NewContactsTime + ControllersUpdateTime + ContactsUpdateTime +
                                         SolveUpdateTime);

            if (Settings.AutoClearForces)
                ClearForces();

            len = BreakableBodyList.Count;
            for (var i = 0; i < len; i++) BreakableBodyList[i].Update();
            len = BodyList.Count;
            for (var i = 0; i < len; i++)
            {
                Body hold = BodyList[i];
                ParentConstraint constraint = hold.constraint;

                if (constraint != null) { constraint.ParentUpdate(); }
            }

            _invDt0 = step.inv_dt;

            if (Settings.EnableDiagnostics)
            {
                _watch.Stop();
                UpdateTime = (Fix64)_watch.ElapsedTicks;
                _watch.Reset();
            }
        }

        /// <summary>
        /// Call this after you are done with time steps to clear the forces. You normally
        /// call this after each call to Step, unless you are performing sub-steps. By default,
        /// forces will be automatically cleared, so you don't need to call this function.
        /// </summary>
        public void ClearForces()
        {
            for (var i = 0; i < BodyList.Count; i++)
            {
                var body = BodyList[i];
                body._force = FVector2.zero;
                body._torque = Fix64.Zero;
            }
        }

        /// <summary>
        /// Query the world for all fixtures that potentially overlap the provided AABB.
        /// Inside the callback:
        /// Return true: Continues the query
        /// Return false: Terminate the query
        /// </summary>
        /// <param name="callback">A user implemented callback class.</param>
        /// <param name="aabb">The AABB query box.</param>
        public void QueryAABB(Func<Fixture, bool> callback, ref AABB aabb)
        {
            _queryAABBCallback = callback;
            ContactManager.BroadPhase.Query(_queryAABBCallbackWrapper, ref aabb);
            _queryAABBCallback = null;
        }

        /// <summary>
        /// Query the world for all fixtures that potentially overlap the provided AABB.
        /// Use the overload with a callback for filtering and better performance.
        /// </summary>
        /// <param name="aabb">The AABB query box.</param>
        /// <returns>A list of fixtures that were in the affected area.</returns>
        public List<Fixture> QueryAABB(ref AABB aabb)
        {
            var affected = new List<Fixture>();

            QueryAABB(fixture =>
            {
                affected.Add(fixture);
                return true;
            }, ref aabb);

            return affected;
        }

        /// <summary>
        /// Ray-cast the world for all fixtures in the path of the ray. Your callback
        /// controls whether you get the closest point, any point, or n-points.
        /// The ray-cast ignores shapes that contain the starting point.
        /// Inside the callback:
        /// return -1: ignore this fixture and continue
        /// return 0: terminate the ray cast
        /// return fraction: clip the ray to this point
        /// return 1: don't clip the ray and continue
        /// </summary>
        /// <param name="callback">A user implemented callback class.</param>
        /// <param name="point1">The ray starting point.</param>
        /// <param name="point2">The ray ending point.</param>
        public void RayCast(Func<Fixture, FVector2, FVector2, Fix64, Fix64> callback, FVector2 point1, FVector2 point2)
        {
            var input = new RayCastInput();
            input.MaxFraction = Fix64.One;
            input.Point1 = point1;
            input.Point2 = point2;

            _rayCastCallback = callback;
            ContactManager.BroadPhase.RayCast(_rayCastCallbackWrapper, ref input);
            _rayCastCallback = null;
        }

        public List<Fixture> RayCast(FVector2 point1, FVector2 point2)
        {
            var affected = new List<Fixture>();

            RayCast((f, p, n, fr) =>
            {
                affected.Add(f);
                return 1;
            }, point1, point2);

            return affected;
        }

        public void AddController(Controller controller)
        {
            UnityEngine.Debug.Assert(!ControllerList.Contains(controller), "You are adding the same controller more than once.");

            controller.World = this;
            ControllerList.Add(controller);

            ControllerAdded?.Invoke(controller);
        }

        public void RemoveController(Controller controller)
        {
            UnityEngine.Debug.Assert(ControllerList.Contains(controller),
                "You are removing a controller that is not in the simulation.");

            if (ControllerList.Contains(controller))
            {
                ControllerList.Remove(controller);

                ControllerRemoved?.Invoke(controller);
            }
        }

        public void AddBreakableBody(BreakableBody breakableBody)
        {
            BreakableBodyList.Add(breakableBody);
        }

        public void RemoveBreakableBody(BreakableBody breakableBody)
        {
            //The breakable body list does not contain the body you tried to remove.
            UnityEngine.Debug.Assert(BreakableBodyList.Contains(breakableBody));

            BreakableBodyList.Remove(breakableBody);
        }

        public Fixture TestPoint(FVector2 point)
        {
            AABB aabb;
            var d = new FVector2(Settings.Epsilon, Settings.Epsilon);
            aabb.LowerBound = point - d;
            aabb.UpperBound = point + d;

            _myFixture = null;
            _point1 = point;

            // Query the world for overlapping shapes.
            QueryAABB(TestPointCallback, ref aabb);

            return _myFixture;
        }

        private bool TestPointCallback(Fixture fixture)
        {
            var inside = fixture.TestPoint(ref _point1);
            if (inside)
            {
                _myFixture = fixture;
                return false;
            }

            // Continue the query.
            return true;
        }

        /// <summary>
        /// Returns a list of fixtures that are at the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public List<Fixture> TestPointAll(FVector2 point)
        {
            AABB aabb;
            var d = new FVector2(Settings.Epsilon, Settings.Epsilon);
            aabb.LowerBound = point - d;
            aabb.UpperBound = point + d;

            _point2 = point;
            _testPointAllFixtures = new List<Fixture>();

            // Query the world for overlapping shapes.
            QueryAABB(TestPointAllCallback, ref aabb);

            return _testPointAllFixtures;
        }

        private bool TestPointAllCallback(Fixture fixture)
        {
            var inside = fixture.TestPoint(ref _point2);
            if (inside)
                _testPointAllFixtures.Add(fixture);

            // Continue the query.
            return true;
        }

        /// Shift the world origin. Useful for large worlds.
        /// The body shift formula is: position -= newOrigin
        /// @param newOrigin the new origin with respect to the old origin
        /// Warning: Calling this method mid-update might cause a crash.
        public void ShiftOrigin(FVector2 newOrigin)
        {
            foreach (var b in BodyList)
            {
                b._xf.p -= newOrigin;
                b._sweep.C0 -= newOrigin;
                b._sweep.C -= newOrigin;
            }

            foreach (var VJoint in VJointList)
            {
                //VJoint.ShiftOrigin(newOrigin); //TODO: uncomment
            }

            ContactManager.BroadPhase.ShiftOrigin(newOrigin);
        }

        public void Clear()
        {
            ProcessChanges();

            for (var i = BodyList.Count - 1; i >= 0; i--) RemoveBody(BodyList[i]);

            for (var i = ControllerList.Count - 1; i >= 0; i--) RemoveController(ControllerList[i]);

            for (var i = BreakableBodyList.Count - 1; i >= 0; i--) RemoveBreakableBody(BreakableBodyList[i]);

            ProcessChanges();
        }

        internal Body CreateBody(BodyTemplate template)
        {
            var b = new Body(this, template);
            b.BodyId = _bodyIdCounter++;

            AddBody(b);
            return b;
        }
    }
}