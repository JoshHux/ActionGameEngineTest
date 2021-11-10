using System.Collections.Generic;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Utilities;
using FixMath.NET;

namespace VelcroPhysics.Tools.PathGenerator
{
    public static class LinkFactory
    {
        /// <summary>
        /// Creates a chain.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="linkWidth">The width.</param>
        /// <param name="linkHeight">The height.</param>
        /// <param name="numberOfLinks">The number of links.</param>
        /// <param name="linkDensity">The link density.</param>
        /// <param name="attachRopeVJoint">
        /// Creates a rope VJoint between start and end. This enforces the length of the rope. Said in
        /// another way: it makes the rope less bouncy.
        /// </param>
        /// <returns></returns>
        public static Path CreateChain(World world, FVector2 start, FVector2 end, Fix64 linkWidth, Fix64 linkHeight,
            int numberOfLinks, Fix64 linkDensity, bool attachRopeVJoint)
        {
            Debug.Assert(numberOfLinks >= 2);

            //Chain start / end
            var path = new Path();
            path.Add(start);
            path.Add(end);

            //A single chainlink
            var shape = new PolygonShape(PolygonUtils.CreateRectangle(linkWidth, linkHeight), linkDensity);

            //Use PathManager to create all the chainlinks based on the chainlink created before.
            var chainLinks =
                PathManager.EvenlyDistributeShapesAlongPath(world, path, shape, BodyType.Dynamic, numberOfLinks);

            //TODO
            //if (fixStart)
            //{
            //    //Fix the first chainlink to the world
            //    VJointFactory.CreateFixedRevoluteVJoint(world, chainLinks[0], new FVector2(0, -(linkHeight / 2)),
            //                                          chainLinks[0].Position);
            //}

            //if (fixEnd)
            //{
            //    //Fix the last chainlink to the world
            //    VJointFactory.CreateFixedRevoluteVJoint(world, chainLinks[chainLinks.Count - 1],
            //                                          new FVector2(0, (linkHeight / 2)),
            //                                          chainLinks[chainLinks.Count - 1].Position);
            //}

            //Attach all the chainlinks together with a revolute VJoint
            PathManager.AttachBodiesWithRevoluteVJoint(world, chainLinks, new FVector2(0, -linkHeight),
                new FVector2(0, linkHeight), false, false);

            if (attachRopeVJoint)
                VJointFactory.CreateRopeVJoint(world, chainLinks[0], chainLinks[chainLinks.Count - 1], FVector2.zero,
                    FVector2.zero);

            return path;
        }
    }
}