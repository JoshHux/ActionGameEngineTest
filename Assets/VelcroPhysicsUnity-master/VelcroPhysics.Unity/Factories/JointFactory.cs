using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.VJoints;

namespace VelcroPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for using VJoints.
    /// </summary>
    public static class VJointFactory
    {
        #region Motor VJoint

        public static MotorVJoint CreateMotorVJoint(World world, Body bodyA, Body bodyB, bool useWorldCoordinates = false)
        {
            var VJoint = new MotorVJoint(bodyA, bodyB, useWorldCoordinates);
            world.AddVJoint(VJoint);
            return VJoint;
        }

        #endregion

        #region Rope VJoint

        public static RopeVJoint CreateRopeVJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB,
            bool useWorldCoordinates = false)
        {
            var ropeVJoint = new RopeVJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
            world.AddVJoint(ropeVJoint);
            return ropeVJoint;
        }

        #endregion

        #region Weld VJoint

        public static WeldVJoint CreateWeldVJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB,
            bool useWorldCoordinates = false)
        {
            var weldVJoint = new WeldVJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
            world.AddVJoint(weldVJoint);
            return weldVJoint;
        }

        #endregion

        #region Prismatic VJoint

        public static PrismaticVJoint CreatePrismaticVJoint(World world, Body bodyA, Body bodyB, Vector2 anchor,
            Vector2 axis, bool useWorldCoordinates = false)
        {
            var VJoint = new PrismaticVJoint(bodyA, bodyB, anchor, axis, useWorldCoordinates);
            world.AddVJoint(VJoint);
            return VJoint;
        }

        #endregion

        #region Angle VJoint

        public static AngleVJoint CreateAngleVJoint(World world, Body bodyA, Body bodyB)
        {
            var angleVJoint = new AngleVJoint(bodyA, bodyB);
            world.AddVJoint(angleVJoint);
            return angleVJoint;
        }

        #endregion

        #region Gear VJoint

        public static GearVJoint CreateGearVJoint(World world, Body bodyA, Body bodyB, VJoint VJointA, VJoint VJointB,
            float ratio)
        {
            var gearVJoint = new GearVJoint(bodyA, bodyB, VJointA, VJointB, ratio);
            world.AddVJoint(gearVJoint);
            return gearVJoint;
        }

        #endregion

        #region Pulley VJoint

        public static PulleyVJoint CreatePulleyVJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA,
            Vector2 anchorB, Vector2 worldAnchorA, Vector2 worldAnchorB, float ratio, bool useWorldCoordinates = false)
        {
            var pulleyVJoint = new PulleyVJoint(bodyA, bodyB, anchorA, anchorB, worldAnchorA, worldAnchorB, ratio,
                useWorldCoordinates);
            world.AddVJoint(pulleyVJoint);
            return pulleyVJoint;
        }

        #endregion

        #region MouseVJoint

        public static FixedMouseVJoint CreateFixedMouseVJoint(World world, Body body, Vector2 worldAnchor)
        {
            var VJoint = new FixedMouseVJoint(body, worldAnchor);
            world.AddVJoint(VJoint);
            return VJoint;
        }

        #endregion

        #region Revolute VJoint

        public static RevoluteVJoint CreateRevoluteVJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA,
            Vector2 anchorB, bool useWorldCoordinates = false)
        {
            var VJoint = new RevoluteVJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
            world.AddVJoint(VJoint);
            return VJoint;
        }

        public static RevoluteVJoint CreateRevoluteVJoint(World world, Body bodyA, Body bodyB, Vector2 anchor)
        {
            var localanchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(anchor));
            var VJoint = new RevoluteVJoint(bodyA, bodyB, localanchorA, anchor);
            world.AddVJoint(VJoint);
            return VJoint;
        }

        #endregion

        #region Wheel VJoint

        public static WheelVJoint CreateWheelVJoint(World world, Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis,
            bool useWorldCoordinates = false)
        {
            var VJoint = new WheelVJoint(bodyA, bodyB, anchor, axis, useWorldCoordinates);
            world.AddVJoint(VJoint);
            return VJoint;
        }

        public static WheelVJoint CreateWheelVJoint(World world, Body bodyA, Body bodyB, Vector2 axis)
        {
            return CreateWheelVJoint(world, bodyA, bodyB, Vector2.zero, axis);
        }

        #endregion

        #region Distance VJoint

        public static DistanceVJoint CreateDistanceVJoint(World world, Body bodyA, Body bodyB, Vector2 anchorA,
            Vector2 anchorB, bool useWorldCoordinates = false)
        {
            var distanceVJoint = new DistanceVJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
            world.AddVJoint(distanceVJoint);
            return distanceVJoint;
        }

        public static DistanceVJoint CreateDistanceVJoint(World world, Body bodyA, Body bodyB)
        {
            return CreateDistanceVJoint(world, bodyA, bodyB, Vector2.zero, Vector2.zero);
        }

        #endregion

        #region Friction VJoint

        public static FrictionVJoint CreateFrictionVJoint(World world, Body bodyA, Body bodyB, Vector2 anchor,
            bool useWorldCoordinates = false)
        {
            var frictionVJoint = new FrictionVJoint(bodyA, bodyB, anchor, useWorldCoordinates);
            world.AddVJoint(frictionVJoint);
            return frictionVJoint;
        }

        public static FrictionVJoint CreateFrictionVJoint(World world, Body bodyA, Body bodyB)
        {
            return CreateFrictionVJoint(world, bodyA, bodyB, Vector2.zero);
        }

        #endregion
    }
}