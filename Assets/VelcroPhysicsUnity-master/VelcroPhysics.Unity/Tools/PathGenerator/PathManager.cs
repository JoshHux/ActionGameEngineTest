﻿using System;
using System.Collections.Generic;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.VJoints;
using VelcroPhysics.Factories;
using VelcroPhysics.Shared;
using VelcroPhysics.Tools.Triangulation.TriangulationBase;
using FixMath.NET;

namespace VelcroPhysics.Tools.PathGenerator
{
    /// <summary>
    /// An easy to use manager for creating paths.
    /// </summary>
    public static class PathManager
    {
        #region LinkType enum

        public enum LinkType
        {
            Revolute,
            Slider
        }

        #endregion

        //Contributed by Matthew Bettcher

        /// <summary>
        /// Convert a path into a set of edges and attaches them to the specified body.
        /// Note: use only for static edges.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="body">The body.</param>
        /// <param name="subdivisions">The subdivisions.</param>
        public static void ConvertPathToEdges(Path path, Body body, int subdivisions)
        {
            var verts = path.GetVertices(subdivisions);

            if (path.Closed)
            {
                var chain = new ChainShape(verts, true);
                body.CreateFixture(chain);
            }
            else
            {
                for (var i = 1; i < verts.Count; i++) body.CreateFixture(new EdgeShape(verts[i], verts[i - 1]));
            }
        }

        /// <summary>
        /// Convert a closed path into a polygon.
        /// Convex decomposition is automatically performed.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="body">The body.</param>
        /// <param name="density">The density.</param>
        /// <param name="subdivisions">The subdivisions.</param>
        public static void ConvertPathToPolygon(Path path, Body body, Fix64 density, int subdivisions)
        {
            if (!path.Closed)
                throw new Exception("The path must be closed to convert to a polygon.");

            List<FVector2> verts = path.GetVertices(subdivisions);

            var decomposedVerts = Triangulate.ConvexPartition(new Vertices(verts), TriangulationAlgorithm.Bayazit);

            foreach (var item in decomposedVerts) body.CreateFixture(new PolygonShape(item, density));
        }

        /// <summary>
        /// Duplicates the given Body along the given path for approximately the given copies.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="path">The path.</param>
        /// <param name="shapes">The shapes.</param>
        /// <param name="type">The type.</param>
        /// <param name="copies">The copies.</param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, IEnumerable<Shape> shapes,
            BodyType type, int copies, object userData = null)
        {
            var centers = path.SubdivideEvenly(copies);
            var bodyList = new List<Body>();

            for (var i = 0; i < centers.Count; i++)
            {
                // copy the type from original body
                var b = BodyFactory.CreateBody(world, new FVector2(centers[i].x, centers[i].y), centers[i].z, type,
                    userData);

                foreach (var shape in shapes) b.CreateFixture(shape);

                bodyList.Add(b);
            }

            return bodyList;
        }

        /// <summary>
        /// Duplicates the given Body along the given path for approximately the given copies.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="path">The path.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="type">The type.</param>
        /// <param name="copies">The copies.</param>
        /// <param name="userData">The user data.</param>
        /// <returns></returns>
        public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, Shape shape, BodyType type,
            int copies, object userData)
        {
            var shapes = new List<Shape>(1);
            shapes.Add(shape);

            return EvenlyDistributeShapesAlongPath(world, path, shapes, type, copies, userData);
        }

        public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, Shape shape, BodyType type,
            int copies)
        {
            return EvenlyDistributeShapesAlongPath(world, path, shape, type, copies, null);
        }

        /// <summary>
        /// Moves the given body along the defined path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="body">The body.</param>
        /// <param name="time">The time.</param>
        /// <param name="strength">The strength.</param>
        /// <param name="timeStep">The time step.</param>
        public static void MoveBodyOnPath(Path path, Body body, Fix64 time, Fix64 strength, Fix64 timeStep)
        {
            var destination = path.GetPosition(time);
            var positionDelta = body.Position - destination;
            var velocity = positionDelta / timeStep * strength;

            body.LinearVelocity = -velocity;
        }

        /// <summary>
        /// Attaches the bodies with revolute VJoints.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="bodies">The bodies.</param>
        /// <param name="localAnchorA">The local anchor A.</param>
        /// <param name="localAnchorB">The local anchor B.</param>
        /// <param name="connectFirstAndLast">if set to <c>true</c> [connect first and last].</param>
        /// <param name="collideConnected">if set to <c>true</c> [collide connected].</param>
        public static List<RevoluteVJoint> AttachBodiesWithRevoluteVJoint(World world, List<Body> bodies,
            FVector2 localAnchorA, FVector2 localAnchorB, bool connectFirstAndLast, bool collideConnected)
        {
            var VJoints = new List<RevoluteVJoint>(bodies.Count + 1);

            for (var i = 1; i < bodies.Count; i++)
            {
                var VJoint = new RevoluteVJoint(bodies[i], bodies[i - 1], localAnchorA, localAnchorB);
                VJoint.CollideConnected = collideConnected;
                world.AddVJoint(VJoint);
                VJoints.Add(VJoint);
            }

            if (connectFirstAndLast)
            {
                var lastVJoint = new RevoluteVJoint(bodies[0], bodies[bodies.Count - 1], localAnchorA, localAnchorB);
                lastVJoint.CollideConnected = collideConnected;
                world.AddVJoint(lastVJoint);
                VJoints.Add(lastVJoint);
            }

            return VJoints;
        }
    }
}