﻿using VelcroPhysics.Shared;
using FixMath.NET;

namespace VelcroPhysics.Utilities
{
    /// <summary>
    /// Collection of helper methods for misc collisions.
    /// Does Fix64 tolerance and line collisions with lines and AABBs.
    /// </summary>
    public static class LineUtils
    {
        public static Fix64 DistanceBetweenPointAndLineSegment(ref FVector2 point, ref FVector2 start, ref FVector2 end)
        {
            if (start == end)
                return FVector2.Distance(point, start);

            var v = end - start;
            var w = point - start;

            var c1 = FVector2.Dot(w, v);
            if (c1 <= 0)
                return FVector2.Distance(point, start);

            var c2 = FVector2.Dot(v, v);
            if (c2 <= c1)
                return FVector2.Distance(point, end);

            var b = c1 / c2;
            var pointOnLine = start + v * b;
            return FVector2.Distance(point, pointOnLine);
        }

        // From Eric Jordan's convex decomposition library
        /// <summary>
        /// Check if the lines a0->a1 and b0->b1 cross.
        /// If they do, intersectionPoint will be filled
        /// with the point of crossing.
        /// Grazing lines should not return true.
        /// </summary>
        public static bool LineIntersect2(ref FVector2 a0, ref FVector2 a1, ref FVector2 b0, ref FVector2 b1,
            out FVector2 intersectionPoint)
        {
            intersectionPoint = FVector2.zero;

            if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
                return false;

            var x1 = a0.x;
            var y1 = a0.y;
            var x2 = a1.x;
            var y2 = a1.y;
            var x3 = b0.x;
            var y3 = b0.y;
            var x4 = b1.x;
            var y4 = b1.y;

            //AABB early exit
            if (Fix64.Max(x1, x2) < Fix64.Min(x3, x4) || Fix64.Max(x3, x4) < Fix64.Min(x1, x2))
                return false;

            if (Fix64.Max(y1, y2) < Fix64.Min(y3, y4) || Fix64.Max(y3, y4) < Fix64.Min(y1, y2))
                return false;

            var ua = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
            var ub = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);
            var denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (Fix64.Abs(denom) < Settings.Epsilon)
                //Lines are too close to parallel to call
                return false;
            ua /= denom;
            ub /= denom;

            if (0 < ua && ua < 1 && 0 < ub && ub < 1)
            {
                var intersectionPointx = x1 + ua * (x2 - x1);
                var intersectionPointy = y1 + ua * (y2 - y1);

                intersectionPoint = new FVector2(intersectionPointx, intersectionPointy);
                return true;
            }

            return false;
        }

        //From Mark Bayazit's convex decomposition algorithm
        public static FVector2 LineIntersect(FVector2 p1, FVector2 p2, FVector2 q1, FVector2 q2)
        {
            var i = FVector2.zero;
            var a1 = p2.y - p1.y;
            var b1 = p1.x - p2.x;
            var c1 = a1 * p1.x + b1 * p1.y;
            var a2 = q2.y - q1.y;
            var b2 = q1.x - q2.x;
            var c2 = a2 * q1.x + b2 * q1.x;
            var det = a1 * b2 - a2 * b1;

            if (!MathUtils.Fix64Equals(det, 0))
            {
                // lines are not parallel
                var ix = (b2 * c1 - b1 * c2) / det;
                var iy = (a1 * c2 - a2 * c1) / det;

                i = new FVector2(ix, iy);
            }

            return i;
        }

        /// <summary>
        /// This method detects if two line segments (or lines) intersect,
        /// and, if so, the point of intersection. Use the <paramref name="firstIsSegment" /> and
        /// <paramref name="secondIsSegment" /> parameters to set whether the intersection point
        /// must be on the first and second line segments. Setting these
        /// both to true means you are doing a line-segment to line-segment
        /// intersection. Setting one of them to true means you are doing a
        /// line to line-segment intersection test, and so on.
        /// Note: If two line segments are coincident, then
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// Author: Jeremy Bell
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="point">
        /// This is set to the intersection
        /// point if an intersection is detected.
        /// </param>
        /// <param name="firstIsSegment">
        /// Set this to true to require that the
        /// intersection point be on the first line segment.
        /// </param>
        /// <param name="secondIsSegment">
        /// Set this to true to require that the
        /// intersection point be on the second line segment.
        /// </param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(ref FVector2 point1, ref FVector2 point2, ref FVector2 point3, ref FVector2 point4,
            bool firstIsSegment, bool secondIsSegment, out FVector2 point)
        {
            point = new FVector2();

            // these are reused later.
            // each lettered sub-calculation is used twice, except
            // for b and d, which are used 3 times
            var a = point4.y - point3.y;
            var b = point2.x - point1.x;
            var c = point4.x - point3.x;
            var d = point2.y - point1.y;

            // denominator to solution of linear system
            var denom = a * b - c * d;

            // if denominator is 0, then lines are parallel
            if (!(denom >= -Settings.Epsilon && denom <= Settings.Epsilon))
            {
                var e = point1.y - point3.y;
                var f = point1.x - point3.x;
                var oneOverDenom = Fix64.One / denom;

                // numerator of first equation
                var ua = c * e - a * f;
                ua *= oneOverDenom;

                // check if intersection point of the two lines is on line segment 1
                if (!firstIsSegment || ua >= Fix64.Zero && ua <= Fix64.One)
                {
                    // numerator of second equation
                    var ub = b * e - d * f;
                    ub *= oneOverDenom;

                    // check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (!secondIsSegment || ub >= Fix64.Zero && ub <= Fix64.One)
                        // check if they are coincident (no collision in this case)
                        if (ua != 0 || ub != 0)
                        {
                            //There is an intersection
                            var pointx = point1.x + ua * b;
                            var pointy = point1.y + ua * d;

                            point = new FVector2(pointx, pointy);
                            return true;
                        }
                }
            }

            return false;
        }

        /// <summary>
        /// This method detects if two line segments (or lines) intersect,
        /// and, if so, the point of intersection. Use the <paramref name="firstIsSegment" /> and
        /// <paramref name="secondIsSegment" /> parameters to set whether the intersection point
        /// must be on the first and second line segments. Setting these
        /// both to true means you are doing a line-segment to line-segment
        /// intersection. Setting one of them to true means you are doing a
        /// line to line-segment intersection test, and so on.
        /// Note: If two line segments are coincident, then
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// Author: Jeremy Bell
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">
        /// This is set to the intersection
        /// point if an intersection is detected.
        /// </param>
        /// <param name="firstIsSegment">
        /// Set this to true to require that the
        /// intersection point be on the first line segment.
        /// </param>
        /// <param name="secondIsSegment">
        /// Set this to true to require that the
        /// intersection point be on the second line segment.
        /// </param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(FVector2 point1, FVector2 point2, FVector2 point3, FVector2 point4,
            bool firstIsSegment, bool secondIsSegment, out FVector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment,
                out intersectionPoint);
        }

        /// <summary>
        /// This method detects if two line segments intersect,
        /// and, if so, the point of intersection.
        /// Note: If two line segments are coincident, then
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">
        /// This is set to the intersection
        /// point if an intersection is detected.
        /// </param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(ref FVector2 point1, ref FVector2 point2, ref FVector2 point3, ref FVector2 point4,
            out FVector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
        }

        /// <summary>
        /// This method detects if two line segments intersect,
        /// and, if so, the point of intersection.
        /// Note: If two line segments are coincident, then
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">
        /// This is set to the intersection
        /// point if an intersection is detected.
        /// </param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(FVector2 point1, FVector2 point2, FVector2 point3, FVector2 point4,
            out FVector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
        }

        /// <summary>
        /// Get all intersections between a line segment and a list of vertices
        /// representing a polygon. The vertices reuse adjacent points, so for example
        /// edges one and two are between the first and second vertices and between the
        /// second and third vertices. The last edge is between vertex vertices.Count - 1
        /// and verts0. (ie, vertices from a Geometry or AABB)
        /// </summary>
        /// <param name="point1">The first point of the line segment to test</param>
        /// <param name="point2">The second point of the line segment to test.</param>
        /// <param name="vertices">The vertices, as described above</param>
        public static Vertices LineSegmentVerticesIntersect(ref FVector2 point1, ref FVector2 point2, Vertices vertices)
        {
            var intersectionPoints = new Vertices();

            for (var i = 0; i < vertices.Count; i++)
            {
                FVector2 point;
                if (LineIntersect(vertices[i], vertices[vertices.NextIndex(i)], point1, point2, true, true, out point))
                    intersectionPoints.Add(point);
            }

            return intersectionPoints;
        }

        /// <summary>
        /// Get all intersections between a line segment and an AABB.
        /// </summary>
        /// <param name="point1">The first point of the line segment to test</param>
        /// <param name="point2">The second point of the line segment to test.</param>
        /// <param name="aabb">The AABB that is used for testing intersection.</param>
        public static Vertices LineSegmentAABBIntersect(ref FVector2 point1, ref FVector2 point2, AABB aabb)
        {
            return LineSegmentVerticesIntersect(ref point1, ref point2, aabb.Vertices);
        }
    }
}