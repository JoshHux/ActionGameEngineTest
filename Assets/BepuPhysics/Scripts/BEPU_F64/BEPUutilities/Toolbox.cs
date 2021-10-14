using System;
using System.Collections.Generic;
using BEPUphysics.CollisionTests;
using BEPUutilities.DataStructures;
using BEPUutilities.ResourceManagement;
using FixMath.NET;

namespace BEPUutilities
{
    //TODO: It would be nice to split and improve this monolith into individually superior, organized components.


    /// <summary>
    /// Helper class with many algorithms for intersection testing and 3D math.
    /// </summary>
    public static class Toolbox
    {
        /// <summary>
        /// Large tolerance value. Defaults to 1e-5f.
        /// </summary>
        public static Fix64 BigEpsilon = F64.C1 / new Fix64(100000);

        /// <summary>
        /// Tolerance value. Defaults to 1e-7f.
        /// </summary>
        public static Fix64 Epsilon = F64.C1 / new Fix64(10000000);

        /// <summary>
        /// Represents an invalid BepuVector3.
        /// </summary>
        public static readonly BepuVector3 NoVector = new BepuVector3(-Fix64.MaxValue, -Fix64.MaxValue, -Fix64.MaxValue);

        /// <summary>
        /// Reference for a vector with dimensions (0,0,1).
        /// </summary>
        public static BepuVector3 BackVector = BepuVector3.Backward;

        /// <summary>
        /// Reference for a vector with dimensions (0,-1,0).
        /// </summary>
        public static BepuVector3 DownVector = BepuVector3.Down;

        /// <summary>
        /// Reference for a vector with dimensions (0,0,-1).
        /// </summary>
        public static BepuVector3 ForwardVector = BepuVector3.Forward;

        /// <summary>
        /// Refers to the identity BepuQuaternion.
        /// </summary>
        public static BepuQuaternion IdentityOrientation = BepuQuaternion.Identity;

        /// <summary>
        /// Reference for a vector with dimensions (-1,0,0).
        /// </summary>
        public static BepuVector3 LeftVector = BepuVector3.Left;

        /// <summary>
        /// Reference for a vector with dimensions (1,0,0).
        /// </summary>
        public static BepuVector3 RightVector = BepuVector3.Right;

        /// <summary>
        /// Reference for a vector with dimensions (0,1,0).
        /// </summary>
        public static BepuVector3 UpVector = BepuVector3.Up;

        /// <summary>
        /// Matrix containing zeroes for every element.
        /// </summary>
        public static Matrix ZeroMatrix = new Matrix(F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0, F64.C0);

        /// <summary>
        /// Reference for a vector with dimensions (0,0,0).
        /// </summary>
        public static BepuVector3 ZeroVector = BepuVector3.Zero;

        /// <summary>
        /// Refers to the rigid identity transformation.
        /// </summary>
        public static RigidTransform RigidIdentity = RigidTransform.Identity;

        #region Segment/Ray-Triangle Tests

        /// <summary>
        /// Determines the intersection between a ray and a triangle.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length to travel in units of the direction's length.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="hitClockwise">True if the the triangle was hit on the clockwise face, false otherwise.</param>
        /// <param name="hit">Hit data of the ray, if any</param>
        /// <returns>Whether or not the ray and triangle intersect.</returns>
        public static bool FindRayTriangleIntersection(ref Ray ray, Fix64 maximumLength, ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c, out bool hitClockwise, out RayHit hit)
        {
            hitClockwise = false;
            hit = new RayHit();
            BepuVector3 ab, ac;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3.Subtract(ref c, ref a, out ac);

            BepuVector3.Cross(ref ab, ref ac, out hit.Normal);
            if (hit.Normal.LengthSquared() < Epsilon)
                return false; //Degenerate triangle!

            Fix64 d;
            BepuVector3.Dot(ref ray.Direction, ref hit.Normal, out d);
            d = -d;

            hitClockwise = d >= F64.C0;

            BepuVector3 ap;
            BepuVector3.Subtract(ref ray.Position, ref a, out ap);

            BepuVector3.Dot(ref ap, ref hit.Normal, out hit.T);
            hit.T /= d;
            if (hit.T < F64.C0 || hit.T > maximumLength)
                return false;//Hit is behind origin, or too far away.

            BepuVector3.Multiply(ref ray.Direction, hit.T, out hit.Location);
            BepuVector3.Add(ref ray.Position, ref hit.Location, out hit.Location);

            // Compute barycentric coordinates
            BepuVector3.Subtract(ref hit.Location, ref a, out ap);
            Fix64 ABdotAB, ABdotAC, ABdotAP;
            Fix64 ACdotAC, ACdotAP;
            BepuVector3.Dot(ref ab, ref ab, out ABdotAB);
            BepuVector3.Dot(ref ab, ref ac, out ABdotAC);
            BepuVector3.Dot(ref ab, ref ap, out ABdotAP);
            BepuVector3.Dot(ref ac, ref ac, out ACdotAC);
            BepuVector3.Dot(ref ac, ref ap, out ACdotAP);

            Fix64 denom = F64.C1 / (ABdotAB * ACdotAC - ABdotAC * ABdotAC);
            Fix64 u = (ACdotAC * ABdotAP - ABdotAC * ACdotAP) * denom;
            Fix64 v = (ABdotAB * ACdotAP - ABdotAC * ABdotAP) * denom;

            return (u >= -Toolbox.BigEpsilon) && (v >= -Toolbox.BigEpsilon) && (u + v <= F64.C1 + Toolbox.BigEpsilon);

        }

        /// <summary>
        /// Determines the intersection between a ray and a triangle.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length to travel in units of the direction's length.</param>
        /// <param name="sidedness">Sidedness of the triangle to test.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="hit">Hit data of the ray, if any</param>
        /// <returns>Whether or not the ray and triangle intersect.</returns>
        public static bool FindRayTriangleIntersection(ref Ray ray, Fix64 maximumLength, TriangleSidedness sidedness, ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c, out RayHit hit)
        {
            hit = new RayHit();
            BepuVector3 ab, ac;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3.Subtract(ref c, ref a, out ac);

            BepuVector3.Cross(ref ab, ref ac, out hit.Normal);
            if (hit.Normal.LengthSquared() < Epsilon)
                return false; //Degenerate triangle!

            Fix64 d;
            BepuVector3.Dot(ref ray.Direction, ref hit.Normal, out d);
            d = -d;
            switch (sidedness)
            {
                case TriangleSidedness.DoubleSided:
                    if (d <= F64.C0) //Pointing the wrong way.  Flip the normal.
                    {
                        BepuVector3.Negate(ref hit.Normal, out hit.Normal);
                        d = -d;
                    }
                    break;
                case TriangleSidedness.Clockwise:
                    if (d <= F64.C0) //Pointing the wrong way.  Can't hit.
                        return false;

                    break;
                case TriangleSidedness.Counterclockwise:
                    if (d >= F64.C0) //Pointing the wrong way.  Can't hit.
                        return false;

                    BepuVector3.Negate(ref hit.Normal, out hit.Normal);
                    d = -d;
                    break;
            }

            BepuVector3 ap;
            BepuVector3.Subtract(ref ray.Position, ref a, out ap);

            BepuVector3.Dot(ref ap, ref hit.Normal, out hit.T);
            hit.T /= d;
            if (hit.T < F64.C0 || hit.T > maximumLength)
                return false;//Hit is behind origin, or too far away.

            BepuVector3.Multiply(ref ray.Direction, hit.T, out hit.Location);
            BepuVector3.Add(ref ray.Position, ref hit.Location, out hit.Location);

            // Compute barycentric coordinates
            BepuVector3.Subtract(ref hit.Location, ref a, out ap);
            Fix64 ABdotAB, ABdotAC, ABdotAP;
            Fix64 ACdotAC, ACdotAP;
            BepuVector3.Dot(ref ab, ref ab, out ABdotAB);
            BepuVector3.Dot(ref ab, ref ac, out ABdotAC);
            BepuVector3.Dot(ref ab, ref ap, out ABdotAP);
            BepuVector3.Dot(ref ac, ref ac, out ACdotAC);
            BepuVector3.Dot(ref ac, ref ap, out ACdotAP);

            Fix64 denom = F64.C1 / (ABdotAB * ACdotAC - ABdotAC * ABdotAC);
            Fix64 u = (ACdotAC * ABdotAP - ABdotAC * ACdotAP) * denom;
            Fix64 v = (ABdotAB * ACdotAP - ABdotAC * ABdotAP) * denom;

            return (u >= -Toolbox.BigEpsilon) && (v >= -Toolbox.BigEpsilon) && (u + v <= F64.C1 + Toolbox.BigEpsilon);

        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane defined by three points.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="d">First vertex of a triangle which lies on the plane.</param>
        /// <param name="e">Second vertex of a triangle which lies on the plane.</param>
        /// <param name="f">Third vertex of a triangle which lies on the plane.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(BepuVector3 a, BepuVector3 b, BepuVector3 d, BepuVector3 e, BepuVector3 f, out BepuVector3 q)
        {
            Plane p;
            p.Normal = BepuVector3.Cross(e - d, f - d);
            p.D = BepuVector3.Dot(p.Normal, d);
            Fix64 t;
            return GetSegmentPlaneIntersection(a, b, p, out t, out q);
        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second enpoint of segment.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(BepuVector3 a, BepuVector3 b, Plane p, out BepuVector3 q)
        {
            Fix64 t;
            return GetLinePlaneIntersection(ref a, ref b, ref p, out t, out q) && t >= F64.C0 && t <= F64.C1;
        }

        /// <summary>
        /// Finds the intersection between the given segment and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along segment to intersection.</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the segment intersects the plane.</returns>
        public static bool GetSegmentPlaneIntersection(BepuVector3 a, BepuVector3 b, Plane p, out Fix64 t, out BepuVector3 q)
        {
            return GetLinePlaneIntersection(ref a, ref b, ref p, out t, out q) && t >= F64.C0 && t <= F64.C1;
        }

        /// <summary>
        /// Finds the intersection between the given line and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment defining the line.</param>
        /// <param name="b">Second endpoint of segment defining the line.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along line to intersection (A + t * AB).</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the line intersects the plane.  If false, the line is parallel to the plane's surface.</returns>
        public static bool GetLinePlaneIntersection(ref BepuVector3 a, ref BepuVector3 b, ref Plane p, out Fix64 t, out BepuVector3 q)
        {
            BepuVector3 ab;
            BepuVector3.Subtract(ref b, ref a, out ab);
            Fix64 denominator;
            BepuVector3.Dot(ref p.Normal, ref ab, out denominator);
            if (denominator < Epsilon && denominator > -Epsilon)
            {
                //Surface of plane and line are parallel (or very close to it).
                q = new BepuVector3();
                t = Fix64.MaxValue;
                return false;
            }
            Fix64 numerator;
            BepuVector3.Dot(ref p.Normal, ref a, out numerator);
            t = (p.D - numerator) / denominator;
            //Compute the intersection position.
            BepuVector3.Multiply(ref ab, t, out q);
            BepuVector3.Add(ref a, ref q, out q);
            return true;
        }

        /// <summary>
        /// Finds the intersection between the given ray and the given plane.
        /// </summary>
        /// <param name="ray">Ray to test against the plane.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along line to intersection (A + t * AB).</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the line intersects the plane.  If false, the line is parallel to the plane's surface.</returns>
        public static bool GetRayPlaneIntersection(ref Ray ray, ref Plane p, out Fix64 t, out BepuVector3 q)
        {
            Fix64 denominator;
            BepuVector3.Dot(ref p.Normal, ref ray.Direction, out denominator);
            if (denominator < Epsilon && denominator > -Epsilon)
            {
                //Surface of plane and line are parallel (or very close to it).
                q = new BepuVector3();
                t = Fix64.MaxValue;
                return false;
            }
            Fix64 numerator;
            BepuVector3.Dot(ref p.Normal, ref ray.Position, out numerator);
            t = (p.D - numerator) / denominator;
            //Compute the intersection position.
            BepuVector3.Multiply(ref ray.Direction, t, out q);
            BepuVector3.Add(ref ray.Position, ref q, out q);
            return t >= F64.C0;
        }

        #endregion

        #region Point-Triangle Tests

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p.
        /// </summary>
        /// <param name="a">First vertex of triangle.</param>
        /// <param name="b">Second vertex of triangle.</param>
        /// <param name="c">Third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        /// <returns>Voronoi region containing the closest point.</returns>
        public static VoronoiRegion GetClosestPointOnTriangleToPoint(ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c, ref BepuVector3 p, out BepuVector3 closestPoint)
        {
            Fix64 v, w;
            BepuVector3 ab;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3 ac;
            BepuVector3.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            BepuVector3 ap;
            BepuVector3.Subtract(ref p, ref a, out ap);
            Fix64 d1;
            BepuVector3.Dot(ref ab, ref ap, out d1);
            Fix64 d2;
            BepuVector3.Dot(ref ac, ref ap, out d2);
            if (d1 <= F64.C0 && d2 < F64.C0)
            {
                closestPoint = a;
                return VoronoiRegion.A;
            }
            //Vertex region B?
            BepuVector3 bp;
            BepuVector3.Subtract(ref p, ref b, out bp);
            Fix64 d3;
            BepuVector3.Dot(ref ab, ref bp, out d3);
            Fix64 d4;
            BepuVector3.Dot(ref ac, ref bp, out d4);
            if (d3 >= F64.C0 && d4 <= d3)
            {
                closestPoint = b;
                return VoronoiRegion.B;
            }
            //Edge region AB?
            Fix64 vc = d1 * d4 - d3 * d2;
            if (vc <= F64.C0 && d1 >= F64.C0 && d3 <= F64.C0)
            {
                v = d1 / (d1 - d3);
                BepuVector3.Multiply(ref ab, v, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref a, out closestPoint);
                return VoronoiRegion.AB;
            }
            //Vertex region C?
            BepuVector3 cp;
            BepuVector3.Subtract(ref p, ref c, out cp);
            Fix64 d5;
            BepuVector3.Dot(ref ab, ref cp, out d5);
            Fix64 d6;
            BepuVector3.Dot(ref ac, ref cp, out d6);
            if (d6 >= F64.C0 && d5 <= d6)
            {
                closestPoint = c;
                return VoronoiRegion.C;
            }
            //Edge region AC?
            Fix64 vb = d5 * d2 - d1 * d6;
            if (vb <= F64.C0 && d2 >= F64.C0 && d6 <= F64.C0)
            {
                w = d2 / (d2 - d6);
                BepuVector3.Multiply(ref ac, w, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref a, out closestPoint);
                return VoronoiRegion.AC;
            }
            //Edge region BC?
            Fix64 va = d3 * d6 - d5 * d4;
            if (va <= F64.C0 && (d4 - d3) >= F64.C0 && (d5 - d6) >= F64.C0)
            {
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                BepuVector3.Subtract(ref c, ref b, out closestPoint);
                BepuVector3.Multiply(ref closestPoint, w, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref b, out closestPoint);
                return VoronoiRegion.BC;
            }
            //Inside triangle?
            Fix64 denom = F64.C1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            BepuVector3 abv;
            BepuVector3.Multiply(ref ab, v, out abv);
            BepuVector3 acw;
            BepuVector3.Multiply(ref ac, w, out acw);
            BepuVector3.Add(ref a, ref abv, out closestPoint);
            BepuVector3.Add(ref closestPoint, ref acw, out closestPoint);
            return VoronoiRegion.ABC;
        }

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p and provides the subsimplex whose voronoi region contains the point.
        /// </summary>
        /// <param name="a">First vertex of triangle.</param>
        /// <param name="b">Second vertex of triangle.</param>
        /// <param name="c">Third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnTriangleToPoint(ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c, ref BepuVector3 p, RawList<BepuVector3> subsimplex, out BepuVector3 closestPoint)
        {
            subsimplex.Clear();
            Fix64 v, w;
            BepuVector3 ab;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3 ac;
            BepuVector3.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            BepuVector3 ap;
            BepuVector3.Subtract(ref p, ref a, out ap);
            Fix64 d1;
            BepuVector3.Dot(ref ab, ref ap, out d1);
            Fix64 d2;
            BepuVector3.Dot(ref ac, ref ap, out d2);
            if (d1 <= F64.C0 && d2 < F64.C0)
            {
                subsimplex.Add(a);
                closestPoint = a;
                return;
            }
            //Vertex region B?
            BepuVector3 bp;
            BepuVector3.Subtract(ref p, ref b, out bp);
            Fix64 d3;
            BepuVector3.Dot(ref ab, ref bp, out d3);
            Fix64 d4;
            BepuVector3.Dot(ref ac, ref bp, out d4);
            if (d3 >= F64.C0 && d4 <= d3)
            {
                subsimplex.Add(b);
                closestPoint = b;
                return;
            }
            //Edge region AB?
            Fix64 vc = d1 * d4 - d3 * d2;
            if (vc <= F64.C0 && d1 >= F64.C0 && d3 <= F64.C0)
            {
                subsimplex.Add(a);
                subsimplex.Add(b);
                v = d1 / (d1 - d3);
                BepuVector3.Multiply(ref ab, v, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref a, out closestPoint);
                return;
            }
            //Vertex region C?
            BepuVector3 cp;
            BepuVector3.Subtract(ref p, ref c, out cp);
            Fix64 d5;
            BepuVector3.Dot(ref ab, ref cp, out d5);
            Fix64 d6;
            BepuVector3.Dot(ref ac, ref cp, out d6);
            if (d6 >= F64.C0 && d5 <= d6)
            {
                subsimplex.Add(c);
                closestPoint = c;
                return;
            }
            //Edge region AC?
            Fix64 vb = d5 * d2 - d1 * d6;
            if (vb <= F64.C0 && d2 >= F64.C0 && d6 <= F64.C0)
            {
                subsimplex.Add(a);
                subsimplex.Add(c);
                w = d2 / (d2 - d6);
                BepuVector3.Multiply(ref ac, w, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref a, out closestPoint);
                return;
            }
            //Edge region BC?
            Fix64 va = d3 * d6 - d5 * d4;
            if (va <= F64.C0 && (d4 - d3) >= F64.C0 && (d5 - d6) >= F64.C0)
            {
                subsimplex.Add(b);
                subsimplex.Add(c);
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                BepuVector3.Subtract(ref c, ref b, out closestPoint);
                BepuVector3.Multiply(ref closestPoint, w, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref b, out closestPoint);
                return;
            }
            //Inside triangle?
            subsimplex.Add(a);
            subsimplex.Add(b);
            subsimplex.Add(c);
            Fix64 denom = F64.C1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            BepuVector3 abv;
            BepuVector3.Multiply(ref ab, v, out abv);
            BepuVector3 acw;
            BepuVector3.Multiply(ref ac, w, out acw);
            BepuVector3.Add(ref a, ref abv, out closestPoint);
            BepuVector3.Add(ref closestPoint, ref acw, out closestPoint);
        }

        /// <summary>
        /// Determines the closest point on a triangle given by points a, b, and c to point p and provides the subsimplex whose voronoi region contains the point.
        /// </summary>
        /// <param name="q">Simplex containing triangle for testing.</param>
        /// <param name="i">Index of first vertex of triangle.</param>
        /// <param name="j">Index of second vertex of triangle.</param>
        /// <param name="k">Index of third vertex of triangle.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1, c = 2.</param>
        /// <param name="baryCoords">Barycentric coordinates of the point on the triangle.</param>
        /// <param name="closestPoint">Closest point on tetrahedron to point.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnTriangleToPoint(RawList<BepuVector3> q, int i, int j, int k, ref BepuVector3 p, RawList<int> subsimplex, RawList<Fix64> baryCoords, out BepuVector3 closestPoint)
        {
            subsimplex.Clear();
            baryCoords.Clear();
            Fix64 v, w;
            BepuVector3 a = q[i];
            BepuVector3 b = q[j];
            BepuVector3 c = q[k];
            BepuVector3 ab;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3 ac;
            BepuVector3.Subtract(ref c, ref a, out ac);
            //Vertex region A?
            BepuVector3 ap;
            BepuVector3.Subtract(ref p, ref a, out ap);
            Fix64 d1;
            BepuVector3.Dot(ref ab, ref ap, out d1);
            Fix64 d2;
            BepuVector3.Dot(ref ac, ref ap, out d2);
            if (d1 <= F64.C0 && d2 < F64.C0)
            {
                subsimplex.Add(i);
                baryCoords.Add(F64.C1);
                closestPoint = a;
                return; //barycentric coordinates (1,0,0)
            }
            //Vertex region B?
            BepuVector3 bp;
            BepuVector3.Subtract(ref p, ref b, out bp);
            Fix64 d3;
            BepuVector3.Dot(ref ab, ref bp, out d3);
            Fix64 d4;
            BepuVector3.Dot(ref ac, ref bp, out d4);
            if (d3 >= F64.C0 && d4 <= d3)
            {
                subsimplex.Add(j);
                baryCoords.Add(F64.C1);
                closestPoint = b;
                return; //barycentric coordinates (0,1,0)
            }
            //Edge region AB?
            Fix64 vc = d1 * d4 - d3 * d2;
            if (vc <= F64.C0 && d1 >= F64.C0 && d3 <= F64.C0)
            {
                subsimplex.Add(i);
                subsimplex.Add(j);
                v = d1 / (d1 - d3);
                baryCoords.Add(F64.C1 - v);
                baryCoords.Add(v);
                BepuVector3.Multiply(ref ab, v, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref a, out closestPoint);
                return; //barycentric coordinates (1-v, v, 0)
            }
            //Vertex region C?
            BepuVector3 cp;
            BepuVector3.Subtract(ref p, ref c, out cp);
            Fix64 d5;
            BepuVector3.Dot(ref ab, ref cp, out d5);
            Fix64 d6;
            BepuVector3.Dot(ref ac, ref cp, out d6);
            if (d6 >= F64.C0 && d5 <= d6)
            {
                subsimplex.Add(k);
                baryCoords.Add(F64.C1);
                closestPoint = c;
                return; //barycentric coordinates (0,0,1)
            }
            //Edge region AC?
            Fix64 vb = d5 * d2 - d1 * d6;
            if (vb <= F64.C0 && d2 >= F64.C0 && d6 <= F64.C0)
            {
                subsimplex.Add(i);
                subsimplex.Add(k);
                w = d2 / (d2 - d6);
                baryCoords.Add(F64.C1 - w);
                baryCoords.Add(w);
                BepuVector3.Multiply(ref ac, w, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref a, out closestPoint);
                return; //barycentric coordinates (1-w, 0, w)
            }
            //Edge region BC?
            Fix64 va = d3 * d6 - d5 * d4;
            if (va <= F64.C0 && (d4 - d3) >= F64.C0 && (d5 - d6) >= F64.C0)
            {
                subsimplex.Add(j);
                subsimplex.Add(k);
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                baryCoords.Add(F64.C1 - w);
                baryCoords.Add(w);
                BepuVector3.Subtract(ref c, ref b, out closestPoint);
                BepuVector3.Multiply(ref closestPoint, w, out closestPoint);
                BepuVector3.Add(ref closestPoint, ref b, out closestPoint);
                return; //barycentric coordinates (0, 1 - w ,w)
            }
            //Inside triangle?
            subsimplex.Add(i);
            subsimplex.Add(j);
            subsimplex.Add(k);
            Fix64 denom = F64.C1 / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;
            baryCoords.Add(F64.C1 - v - w);
            baryCoords.Add(v);
            baryCoords.Add(w);
            BepuVector3 abv;
            BepuVector3.Multiply(ref ab, v, out abv);
            BepuVector3 acw;
            BepuVector3.Multiply(ref ac, w, out acw);
            BepuVector3.Add(ref a, ref abv, out closestPoint);
            BepuVector3.Add(ref closestPoint, ref acw, out closestPoint);
            //return a + ab * v + ac * w; //barycentric coordinates (1 - v - w, v, w)
        }

        /// <summary>
        /// Determines if supplied point is within the triangle as defined by the provided vertices.
        /// </summary>
        /// <param name="vA">A vertex of the triangle.</param>
        /// <param name="vB">A vertex of the triangle.</param>
        /// <param name="vC">A vertex of the triangle.</param>
        /// <param name="p">The point for comparison against the triangle.</param>
        /// <returns>Whether or not the point is within the triangle.</returns>
        public static bool IsPointInsideTriangle(ref BepuVector3 vA, ref BepuVector3 vB, ref BepuVector3 vC, ref BepuVector3 p)
        {
            Fix64 u, v, w;
            GetBarycentricCoordinates(ref p, ref vA, ref vB, ref vC, out u, out v, out w);
            //Are the barycoords valid?
            return (u > -Epsilon) && (v > -Epsilon) && (w > -Epsilon);
        }

        /// <summary>
        /// Determines if supplied point is within the triangle as defined by the provided vertices.
        /// </summary>
        /// <param name="vA">A vertex of the triangle.</param>
        /// <param name="vB">A vertex of the triangle.</param>
        /// <param name="vC">A vertex of the triangle.</param>
        /// <param name="p">The point for comparison against the triangle.</param>
        /// <param name="margin">Extra area on the edges of the triangle to include.  Can be negative.</param>
        /// <returns>Whether or not the point is within the triangle.</returns>
        public static bool IsPointInsideTriangle(ref BepuVector3 vA, ref BepuVector3 vB, ref BepuVector3 vC, ref BepuVector3 p, Fix64 margin)
        {
            Fix64 u, v, w;
            GetBarycentricCoordinates(ref p, ref vA, ref vB, ref vC, out u, out v, out w);
            //Are the barycoords valid?
            return (u > -margin) && (v > -margin) && (w > -margin);
        }

        #endregion

        #region Point-Line Tests

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        public static void GetClosestPointOnSegmentToPoint(ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 p, out BepuVector3 closestPoint)
        {
            BepuVector3 ab;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3 ap;
            BepuVector3.Subtract(ref p, ref a, out ap);
            Fix64 t;
            BepuVector3.Dot(ref ap, ref ab, out t);
            if (t <= F64.C0)
            {
                closestPoint = a;
            }
            else
            {
                Fix64 denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    BepuVector3 tab;
                    BepuVector3.Multiply(ref ab, t, out tab);
                    BepuVector3.Add(ref a, ref tab, out closestPoint);
                }
            }
        }

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="a">First endpoint of segment.</param>
        /// <param name="b">Second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnSegmentToPoint(ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 p, List<BepuVector3> subsimplex, out BepuVector3 closestPoint)
        {
            subsimplex.Clear();
            BepuVector3 ab;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3 ap;
            BepuVector3.Subtract(ref p, ref a, out ap);
            Fix64 t;
            BepuVector3.Dot(ref ap, ref ab, out t);
            if (t <= F64.C0)
            {
                //t = 0;//Don't need this for returning purposes.
                subsimplex.Add(a);
                closestPoint = a;
            }
            else
            {
                Fix64 denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    //t = 1;//Don't need this for returning purposes.
                    subsimplex.Add(b);
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    subsimplex.Add(a);
                    subsimplex.Add(b);
                    BepuVector3 tab;
                    BepuVector3.Multiply(ref ab, t, out tab);
                    BepuVector3.Add(ref a, ref tab, out closestPoint);
                }
            }
        }

        /// <summary>
        /// Determines the closest point on the provided segment ab to point p.
        /// </summary>
        /// <param name="q">List of points in the containing simplex.</param>
        /// <param name="i">Index of first endpoint of segment.</param>
        /// <param name="j">Index of second endpoint of segment.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1.</param>
        /// <param name="baryCoords">Barycentric coordinates of the point.</param>
        /// <param name="closestPoint">Closest point on the edge to p.</param>
        [Obsolete("Used for simplex tests; consider using the PairSimplex and its variants instead for simplex-related testing.")]
        public static void GetClosestPointOnSegmentToPoint(List<BepuVector3> q, int i, int j, ref BepuVector3 p, List<int> subsimplex, List<Fix64> baryCoords, out BepuVector3 closestPoint)
        {
            BepuVector3 a = q[i];
            BepuVector3 b = q[j];
            subsimplex.Clear();
            baryCoords.Clear();
            BepuVector3 ab;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3 ap;
            BepuVector3.Subtract(ref p, ref a, out ap);
            Fix64 t;
            BepuVector3.Dot(ref ap, ref ab, out t);
            if (t <= F64.C0)
            {
                subsimplex.Add(i);
                baryCoords.Add(F64.C1);
                closestPoint = a;
            }
            else
            {
                Fix64 denom = ab.X * ab.X + ab.Y * ab.Y + ab.Z * ab.Z;
                if (t >= denom)
                {
                    subsimplex.Add(j);
                    baryCoords.Add(F64.C1);
                    closestPoint = b;
                }
                else
                {
                    t = t / denom;
                    subsimplex.Add(i);
                    subsimplex.Add(j);
                    baryCoords.Add(F64.C1 - t);
                    baryCoords.Add(t);
                    BepuVector3 tab;
                    BepuVector3.Multiply(ref ab, t, out tab);
                    BepuVector3.Add(ref a, ref tab, out closestPoint);
                }
            }
        }


        /// <summary>
        /// Determines the shortest squared distance from the point to the line.
        /// </summary>
        /// <param name="p">Point to check against the line.</param>
        /// <param name="a">First point on the line.</param>
        /// <param name="b">Second point on the line.</param>
        /// <returns>Shortest squared distance from the point to the line.</returns>
        public static Fix64 GetSquaredDistanceFromPointToLine(ref BepuVector3 p, ref BepuVector3 a, ref BepuVector3 b)
        {
            BepuVector3 ap, ab;
            BepuVector3.Subtract(ref p, ref a, out ap);
            BepuVector3.Subtract(ref b, ref a, out ab);
            Fix64 e;
            BepuVector3.Dot(ref ap, ref ab, out e);
            return ap.LengthSquared() - e * e / ab.LengthSquared();
        }

        #endregion

        #region Line-Line Tests

        /// <summary>
        /// Computes closest points c1 and c2 betwen segments p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenSegments(BepuVector3 p1, BepuVector3 q1, BepuVector3 p2, BepuVector3 q2, out BepuVector3 c1, out BepuVector3 c2)
        {
			Fix64 s, t;
            GetClosestPointsBetweenSegments(ref p1, ref q1, ref p2, ref q2, out s, out t, out c1, out c2);
        }

        /// <summary>
        /// Computes closest points c1 and c2 betwen segments p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="s">Distance along the line to the point for first segment.</param>
        /// <param name="t">Distance along the line to the point for second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenSegments(ref BepuVector3 p1, ref BepuVector3 q1, ref BepuVector3 p2, ref BepuVector3 q2,
                                                           out Fix64 s, out Fix64 t, out BepuVector3 c1, out BepuVector3 c2)
        {
            //Segment direction vectors
            BepuVector3 d1;
            BepuVector3.Subtract(ref q1, ref p1, out d1);
            BepuVector3 d2;
            BepuVector3.Subtract(ref q2, ref p2, out d2);
            BepuVector3 r;
            BepuVector3.Subtract(ref p1, ref p2, out r);
            //distance
            Fix64 a = d1.LengthSquared();
            Fix64 e = d2.LengthSquared();
            Fix64 f;
            BepuVector3.Dot(ref d2, ref r, out f);

            if (a <= Epsilon && e <= Epsilon)
            {
                //These segments are more like points.
                s = t = F64.C0;
                c1 = p1;
                c2 = p2;
                return;
            }
            if (a <= Epsilon)
            {
                // First segment is basically a point.
                s = F64.C0;
                t = MathHelper.Clamp(f / e, F64.C0, F64.C1);
            }
            else
            {
				Fix64 c = BepuVector3.Dot(d1, r);
                if (e <= Epsilon)
                {
                    // Second segment is basically a point.
                    t = F64.C0;
                    s = MathHelper.Clamp(-c / a, F64.C0, F64.C1);
                }
                else
                {
					Fix64 b = BepuVector3.Dot(d1, d2);
					Fix64 denom = a * e - b * b;

                    // If segments not parallel, compute closest point on L1 to L2, and
                    // clamp to segment S1. Else pick some s (here .5f)
                    if (denom != F64.C0)
                        s = MathHelper.Clamp((b * f - c * e) / denom, F64.C0, F64.C1);
                    else //Parallel, just use .5f
                        s = F64.C0p5;


                    t = (b * s + f) / e;

                    if (t < F64.C0)
                    {
                        //Closest point is before the segment.
                        t = F64.C0;
                        s = MathHelper.Clamp(-c / a, F64.C0, F64.C1);
                    }
                    else if (t > F64.C1)
                    {
                        //Closest point is after the segment.
                        t = F64.C1;
                        s = MathHelper.Clamp((b - c) / a, F64.C0, F64.C1);
                    }
                }
            }

            BepuVector3.Multiply(ref d1, s, out c1);
            BepuVector3.Add(ref c1, ref p1, out c1);
            BepuVector3.Multiply(ref d2, t, out c2);
            BepuVector3.Add(ref c2, ref p2, out c2);
        }


        /// <summary>
        /// Computes closest points c1 and c2 betwen lines p1q1 and p2q2.
        /// </summary>
        /// <param name="p1">First point of first segment.</param>
        /// <param name="q1">Second point of first segment.</param>
        /// <param name="p2">First point of second segment.</param>
        /// <param name="q2">Second point of second segment.</param>
        /// <param name="s">Distance along the line to the point for first segment.</param>
        /// <param name="t">Distance along the line to the point for second segment.</param>
        /// <param name="c1">Closest point on first segment.</param>
        /// <param name="c2">Closest point on second segment.</param>
        public static void GetClosestPointsBetweenLines(ref BepuVector3 p1, ref BepuVector3 q1, ref BepuVector3 p2, ref BepuVector3 q2,
                                                           out Fix64 s, out Fix64 t, out BepuVector3 c1, out BepuVector3 c2)
        {
            //Segment direction vectors
            BepuVector3 d1;
            BepuVector3.Subtract(ref q1, ref p1, out d1);
            BepuVector3 d2;
            BepuVector3.Subtract(ref q2, ref p2, out d2);
            BepuVector3 r;
            BepuVector3.Subtract(ref p1, ref p2, out r);
			//distance
			Fix64 a = d1.LengthSquared();
			Fix64 e = d2.LengthSquared();
			Fix64 f;
            BepuVector3.Dot(ref d2, ref r, out f);

            if (a <= Epsilon && e <= Epsilon)
            {
                //These segments are more like points.
                s = t = F64.C0;
                c1 = p1;
                c2 = p2;
                return;
            }
            if (a <= Epsilon)
            {
                // First segment is basically a point.
                s = F64.C0;
                t = MathHelper.Clamp(f / e, F64.C0, F64.C1);
            }
            else
            {
				Fix64 c = BepuVector3.Dot(d1, r);
                if (e <= Epsilon)
                {
                    // Second segment is basically a point.
                    t = F64.C0;
                    s = MathHelper.Clamp(-c / a, F64.C0, F64.C1);
                }
                else
                {
					Fix64 b = BepuVector3.Dot(d1, d2);
					Fix64 denom = a * e - b * b;

                    // If segments not parallel, compute closest point on L1 to L2, and
                    // clamp to segment S1. Else pick some s (here .5f)
                    if (denom != F64.C0)
                        s = (b * f - c * e) / denom;
                    else //Parallel, just use .5f
                        s = F64.C0p5;


                    t = (b * s + f) / e;
                }
            }

            BepuVector3.Multiply(ref d1, s, out c1);
            BepuVector3.Add(ref c1, ref p1, out c1);
            BepuVector3.Multiply(ref d2, t, out c2);
            BepuVector3.Add(ref c2, ref p2, out c2);
        }



        #endregion


        #region Point-Plane Tests

        /// <summary>
        /// Determines if vectors o and p are on opposite sides of the plane defined by a, b, and c.
        /// </summary>
        /// <param name="o">First point for comparison.</param>
        /// <param name="p">Second point for comparison.</param>
        /// <param name="a">First vertex of the plane.</param>
        /// <param name="b">Second vertex of plane.</param>
        /// <param name="c">Third vertex of plane.</param>
        /// <returns>Whether or not vectors o and p reside on opposite sides of the plane.</returns>
        public static bool ArePointsOnOppositeSidesOFix64lane(ref BepuVector3 o, ref BepuVector3 p, ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c)
        {
            BepuVector3 ab, ac, ap, ao;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3.Subtract(ref c, ref a, out ac);
            BepuVector3.Subtract(ref p, ref a, out ap);
            BepuVector3.Subtract(ref o, ref a, out ao);
            BepuVector3 q;
            BepuVector3.Cross(ref ab, ref ac, out q);
			Fix64 signp;
            BepuVector3.Dot(ref ap, ref q, out signp);
			Fix64 signo;
            BepuVector3.Dot(ref ao, ref q, out signo);
            if (signp * signo <= F64.C0)
                return true;
            return false;
        }

        /// <summary>
        /// Determines the distance between a point and a plane..
        /// </summary>
        /// <param name="point">Point to project onto plane.</param>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="pointOnPlane">Point located on the plane.</param>
        /// <returns>Distance from the point to the plane.</returns>
        public static Fix64 GetDistancePointToPlane(ref BepuVector3 point, ref BepuVector3 normal, ref BepuVector3 pointOnPlane)
        {
            BepuVector3 offset;
            BepuVector3.Subtract(ref point, ref pointOnPlane, out offset);
			Fix64 dot;
            BepuVector3.Dot(ref normal, ref offset, out dot);
            return dot / normal.LengthSquared();
        }

        /// <summary>
        /// Determines the location of the point when projected onto the plane defined by the normal and a point on the plane.
        /// </summary>
        /// <param name="point">Point to project onto plane.</param>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="pointOnPlane">Point located on the plane.</param>
        /// <param name="projectedPoint">Projected location of point onto plane.</param>
        public static void GetPointProjectedOnPlane(ref BepuVector3 point, ref BepuVector3 normal, ref BepuVector3 pointOnPlane, out BepuVector3 projectedPoint)
        {
			Fix64 dot;
            BepuVector3.Dot(ref normal, ref point, out dot);
			Fix64 dot2;
            BepuVector3.Dot(ref pointOnPlane, ref normal, out dot2);
			Fix64 t = (dot - dot2) / normal.LengthSquared();
            BepuVector3 multiply;
            BepuVector3.Multiply(ref normal, t, out multiply);
            BepuVector3.Subtract(ref point, ref multiply, out projectedPoint);
        }

        /// <summary>
        /// Determines if a point is within a set of planes defined by the edges of a triangle.
        /// </summary>
        /// <param name="point">Point for comparison.</param>
        /// <param name="planes">Edge planes.</param>
        /// <param name="centroid">A point known to be inside of the planes.</param>
        /// <returns>Whether or not the point is within the edge planes.</returns>
        public static bool IsPointWithinFaceExtrusion(BepuVector3 point, List<Plane> planes, BepuVector3 centroid)
        {
            foreach (Plane plane in planes)
            {
				Fix64 centroidPlaneDot;
                plane.DotCoordinate(ref centroid, out centroidPlaneDot);
				Fix64 pointPlaneDot;
                plane.DotCoordinate(ref point, out pointPlaneDot);
                if (!((centroidPlaneDot <= Epsilon && pointPlaneDot <= Epsilon) || (centroidPlaneDot >= -Epsilon && pointPlaneDot >= -Epsilon)))
                {
                    //Point's NOT the same side of the centroid, so it's 'outside.'
                    return false;
                }
            }
            return true;
        }


        #endregion

        #region Tetrahedron Tests
        //Note: These methods are unused in modern systems, but are kept around for verification.

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="a">First vertex of the tetrahedron.</param>
        /// <param name="b">Second vertex of the tetrahedron.</param>
        /// <param name="c">Third vertex of the tetrahedron.</param>
        /// <param name="d">Fourth vertex of the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        public static void GetClosestPointOnTetrahedronToPoint(ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c, ref BepuVector3 d, ref BepuVector3 p, out BepuVector3 closestPoint)
        {
            // Start out assuming point inside all halfspaces, so closest to itself
            closestPoint = p;
            BepuVector3 pq;
            BepuVector3 q;
			Fix64 bestSqDist = Fix64.MaxValue;
            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref b, ref c, ref p, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref c, ref d, ref p, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref d, ref b, ref p, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref b, ref d, ref c, ref p, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                }
            }
        }

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="a">First vertex of the tetrahedron.</param>
        /// <param name="b">Second vertex of the tetrahedron.</param>
        /// <param name="c">Third vertex of the tetrahedron.</param>
        /// <param name="d">Fourth vertex of the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        [Obsolete("This method was used for older GJK simplex tests.  If you need simplex tests, consider the PairSimplex class and its variants.")]
        public static void GetClosestPointOnTetrahedronToPoint(ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c, ref BepuVector3 d, ref BepuVector3 p, RawList<BepuVector3> subsimplex, out BepuVector3 closestPoint)
        {
            // Start out assuming point inside all halfspaces, so closest to itself
            subsimplex.Clear();
            subsimplex.Add(a); //Provides a baseline; if the object is not outside of any planes, then it's inside and the subsimplex is the tetrahedron itself.
            subsimplex.Add(b);
            subsimplex.Add(c);
            subsimplex.Add(d);
            closestPoint = p;
            BepuVector3 pq;
            BepuVector3 q;
			Fix64 bestSqDist = Fix64.MaxValue;
            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref b, ref c, ref p, subsimplex, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref c, ref d, ref p, subsimplex, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(ref a, ref d, ref b, ref p, subsimplex, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(ref b, ref d, ref c, ref p, subsimplex, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.X * pq.X + pq.Y * pq.Y + pq.Z * pq.Z;
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                }
            }
        }

        /// <summary>
        /// Determines the closest point on a tetrahedron to a provided point p.
        /// </summary>
        /// <param name="tetrahedron">List of 4 points composing the tetrahedron.</param>
        /// <param name="p">Point for comparison.</param>
        /// <param name="subsimplex">The source of the voronoi region which contains the point, enumerated as a = 0, b = 1, c = 2, d = 3.</param>
        /// <param name="baryCoords">Barycentric coordinates of p on the tetrahedron.</param>
        /// <param name="closestPoint">Closest point on the tetrahedron to the point.</param>
        [Obsolete("This method was used for older GJK simplex tests.  If you need simplex tests, consider the PairSimplex class and its variants.")]
        public static void GetClosestPointOnTetrahedronToPoint(RawList<BepuVector3> tetrahedron, ref BepuVector3 p, RawList<int> subsimplex, RawList<Fix64> baryCoords, out BepuVector3 closestPoint)
        {
            var subsimplexCandidate = CommonResources.GetIntList();
            var baryCoordsCandidate = CommonResources.GetFloatList();
            BepuVector3 a = tetrahedron[0];
            BepuVector3 b = tetrahedron[1];
            BepuVector3 c = tetrahedron[2];
            BepuVector3 d = tetrahedron[3];
            closestPoint = p;
            BepuVector3 pq;
			Fix64 bestSqDist = Fix64.MaxValue;
            subsimplex.Clear();
            subsimplex.Add(0); //Provides a baseline; if the object is not outside of any planes, then it's inside and the subsimplex is the tetrahedron itself.
            subsimplex.Add(1);
            subsimplex.Add(2);
            subsimplex.Add(3);
            baryCoords.Clear();
            BepuVector3 q;
            bool baryCoordsFound = false;

            // If point outside face abc then compute closest point on abc
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref d, ref a, ref b, ref c))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 1, 2, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.LengthSquared();
                // Update best closest point if (squared) distance is less than current best
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face acd
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref b, ref a, ref c, ref d))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 2, 3, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face adb
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref c, ref a, ref d, ref b))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 0, 3, 1, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            // Repeat test for face bdc
            if (ArePointsOnOppositeSidesOFix64lane(ref p, ref a, ref b, ref d, ref c))
            {
                GetClosestPointOnTriangleToPoint(tetrahedron, 1, 3, 2, ref p, subsimplexCandidate, baryCoordsCandidate, out q);
                BepuVector3.Subtract(ref q, ref p, out pq);
				Fix64 sqDist = pq.LengthSquared();
                if (sqDist < bestSqDist)
                {
                    closestPoint = q;
                    subsimplex.Clear();
                    baryCoords.Clear();
                    for (int k = 0; k < subsimplexCandidate.Count; k++)
                    {
                        subsimplex.Add(subsimplexCandidate[k]);
                        baryCoords.Add(baryCoordsCandidate[k]);
                    }
                    //subsimplex.AddRange(subsimplexCandidate);
                    //baryCoords.AddRange(baryCoordsCandidate);
                    baryCoordsFound = true;
                }
            }
            if (!baryCoordsFound)
            {
				//subsimplex is the entire tetrahedron, can only occur when objects intersect!  Determinants of each of the tetrahedrons based on triangles composing the sides and the point itself.
				//This is basically computing the volume of parallelepipeds (triple scalar product).
				//Could be quicker just to do it directly.
				Fix64 abcd = (new Matrix(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, F64.C1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, F64.C1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, F64.C1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, F64.C1)).Determinant();
				Fix64 pbcd = (new Matrix(p.X, p.Y, p.Z, F64.C1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, F64.C1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, F64.C1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, F64.C1)).Determinant();
				Fix64 apcd = (new Matrix(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, F64.C1,
                                         p.X, p.Y, p.Z, F64.C1,
                                         tetrahedron[2].X, tetrahedron[2].Y, tetrahedron[2].Z, F64.C1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, F64.C1)).Determinant();
				Fix64 abpd = (new Matrix(tetrahedron[0].X, tetrahedron[0].Y, tetrahedron[0].Z, F64.C1,
                                         tetrahedron[1].X, tetrahedron[1].Y, tetrahedron[1].Z, F64.C1,
                                         p.X, p.Y, p.Z, F64.C1,
                                         tetrahedron[3].X, tetrahedron[3].Y, tetrahedron[3].Z, F64.C1)).Determinant();
                abcd = F64.C1 / abcd;
                baryCoords.Add(pbcd * abcd); //u
                baryCoords.Add(apcd * abcd); //v
                baryCoords.Add(abpd * abcd); //w
                baryCoords.Add(F64.C1 - baryCoords[0] - baryCoords[1] - baryCoords[2]); //x = 1-u-v-w
            }
            CommonResources.GiveBack(subsimplexCandidate);
            CommonResources.GiveBack(baryCoordsCandidate);
        }

        #endregion





        #region Miscellaneous

        ///<summary>
        /// Tests a ray against a sphere.
        ///</summary>
        ///<param name="ray">Ray to test.</param>
        ///<param name="spherePosition">Position of the sphere.</param>
        ///<param name="radius">Radius of the sphere.</param>
        ///<param name="maximumLength">Maximum length of the ray in units of the ray direction's length.</param>
        ///<param name="hit">Hit data of the ray, if any.</param>
        ///<returns>Whether or not the ray hits the sphere.</returns>
        public static bool RayCastSphere(ref Ray ray, ref BepuVector3 spherePosition, Fix64 radius, Fix64 maximumLength, out RayHit hit)
        {
            BepuVector3 normalizedDirection;
			Fix64 length = ray.Direction.Length();
            BepuVector3.Divide(ref ray.Direction, length, out normalizedDirection);
            maximumLength *= length;
            hit = new RayHit();
            BepuVector3 m;
            BepuVector3.Subtract(ref ray.Position, ref spherePosition, out m);
			Fix64 b = BepuVector3.Dot(m, normalizedDirection);
			Fix64 c = m.LengthSquared() - radius * radius;

            if (c > F64.C0 && b > F64.C0)
                return false;
			Fix64 discriminant = b * b - c;
            if (discriminant < F64.C0)
                return false;

            hit.T = -b - Fix64.Sqrt(discriminant);
            if (hit.T < F64.C0)
                hit.T = F64.C0;
            if (hit.T > maximumLength)
                return false;
            hit.T /= length;
            BepuVector3.Multiply(ref normalizedDirection, hit.T, out hit.Location);
            BepuVector3.Add(ref hit.Location, ref ray.Position, out hit.Location);
            BepuVector3.Subtract(ref hit.Location, ref spherePosition, out hit.Normal);
            hit.Normal.Normalize();
            return true;
        }


        /// <summary>
        /// Computes the velocity of a point as if it were attached to an object with the given center and velocity.
        /// </summary>
        /// <param name="point">Point to compute the velocity of.</param>
        /// <param name="center">Center of the object to which the point is attached.</param>
        /// <param name="linearVelocity">Linear velocity of the object.</param>
        /// <param name="angularVelocity">Angular velocity of the object.</param>
        /// <param name="velocity">Velocity of the point.</param>
        public static void GetVelocityOFix64oint(ref BepuVector3 point, ref BepuVector3 center, ref BepuVector3 linearVelocity, ref BepuVector3 angularVelocity, out BepuVector3 velocity)
        {
            BepuVector3 offset;
            BepuVector3.Subtract(ref point, ref center, out offset);
            BepuVector3.Cross(ref angularVelocity, ref offset, out velocity);
            BepuVector3.Add(ref velocity, ref linearVelocity, out velocity);
        }

        /// <summary>
        /// Computes the velocity of a point as if it were attached to an object with the given center and velocity.
        /// </summary>
        /// <param name="point">Point to compute the velocity of.</param>
        /// <param name="center">Center of the object to which the point is attached.</param>
        /// <param name="linearVelocity">Linear velocity of the object.</param>
        /// <param name="angularVelocity">Angular velocity of the object.</param>
        /// <returns>Velocity of the point.</returns>
        public static BepuVector3 GetVelocityOFix64oint(BepuVector3 point, BepuVector3 center, BepuVector3 linearVelocity, BepuVector3 angularVelocity)
        {
            BepuVector3 toReturn;
            GetVelocityOFix64oint(ref point, ref center, ref linearVelocity, ref angularVelocity, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Expands a bounding box by the given sweep.
        /// </summary>
        /// <param name="boundingBox">Bounding box to expand.</param>
        /// <param name="sweep">Sweep to expand the bounding box with.</param>
        public static void ExpandBoundingBox(ref BoundingBox boundingBox, ref BepuVector3 sweep)
        {
            if (sweep.X > F64.C0)
                boundingBox.Max.X += sweep.X;
            else
                boundingBox.Min.X += sweep.X;

            if (sweep.Y > F64.C0)
                boundingBox.Max.Y += sweep.Y;
            else
                boundingBox.Min.Y += sweep.Y;

            if (sweep.Z > F64.C0)
                boundingBox.Max.Z += sweep.Z;
            else
                boundingBox.Min.Z += sweep.Z;
        }

        /// <summary>
        /// Computes the bounding box of three points.
        /// </summary>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="aabb">Bounding box of the triangle.</param>
        public static void GetTriangleBoundingBox(ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c, out BoundingBox aabb)
        {
#if !WINDOWS
            aabb = new BoundingBox();
#endif
            //X axis
            if (a.X > b.X && a.X > c.X)
            {
                //A is max
                aabb.Max.X = a.X;
                aabb.Min.X = b.X > c.X ? c.X : b.X;
            }
            else if (b.X > c.X)
            {
                //B is max
                aabb.Max.X = b.X;
                aabb.Min.X = a.X > c.X ? c.X : a.X;
            }
            else
            {
                //C is max
                aabb.Max.X = c.X;
                aabb.Min.X = a.X > b.X ? b.X : a.X;
            }
            //Y axis
            if (a.Y > b.Y && a.Y > c.Y)
            {
                //A is max
                aabb.Max.Y = a.Y;
                aabb.Min.Y = b.Y > c.Y ? c.Y : b.Y;
            }
            else if (b.Y > c.Y)
            {
                //B is max
                aabb.Max.Y = b.Y;
                aabb.Min.Y = a.Y > c.Y ? c.Y : a.Y;
            }
            else
            {
                //C is max
                aabb.Max.Y = c.Y;
                aabb.Min.Y = a.Y > b.Y ? b.Y : a.Y;
            }
            //Z axis
            if (a.Z > b.Z && a.Z > c.Z)
            {
                //A is max
                aabb.Max.Z = a.Z;
                aabb.Min.Z = b.Z > c.Z ? c.Z : b.Z;
            }
            else if (b.Z > c.Z)
            {
                //B is max
                aabb.Max.Z = b.Z;
                aabb.Min.Z = a.Z > c.Z ? c.Z : a.Z;
            }
            else
            {
                //C is max
                aabb.Max.Z = c.Z;
                aabb.Min.Z = a.Z > b.Z ? b.Z : a.Z;
            }
        }






        /// <summary>
        /// Updates the BepuQuaternion using RK4 integration.
        /// </summary>
        /// <param name="q">BepuQuaternion to update.</param>
        /// <param name="localInertiaTensorInverse">Local-space inertia tensor of the object being updated.</param>
        /// <param name="angularMomentum">Angular momentum of the object.</param>
        /// <param name="dt">Time since last frame, in seconds.</param>
        /// <param name="newOrientation">New orientation BepuQuaternion.</param>
        public static void UpdateOrientationRK4(ref BepuQuaternion q, ref Matrix3x3 localInertiaTensorInverse, ref BepuVector3 angularMomentum, Fix64 dt, out BepuQuaternion newOrientation)
        {
            //TODO: This is a little goofy
            //BepuQuaternion diff = differentiateBepuQuaternion(ref q, ref localInertiaTensorInverse, ref angularMomentum);
            BepuQuaternion d1;
            DifferentiateBepuQuaternion(ref q, ref localInertiaTensorInverse, ref angularMomentum, out d1);
            BepuQuaternion s2;
            BepuQuaternion.Multiply(ref d1, dt * F64.C0p5, out s2);
            BepuQuaternion.Add(ref q, ref s2, out s2);

            BepuQuaternion d2;
            DifferentiateBepuQuaternion(ref s2, ref localInertiaTensorInverse, ref angularMomentum, out d2);
            BepuQuaternion s3;
            BepuQuaternion.Multiply(ref d2, dt * F64.C0p5, out s3);
            BepuQuaternion.Add(ref q, ref s3, out s3);

            BepuQuaternion d3;
            DifferentiateBepuQuaternion(ref s3, ref localInertiaTensorInverse, ref angularMomentum, out d3);
            BepuQuaternion s4;
            BepuQuaternion.Multiply(ref d3, dt, out s4);
            BepuQuaternion.Add(ref q, ref s4, out s4);

            BepuQuaternion d4;
            DifferentiateBepuQuaternion(ref s4, ref localInertiaTensorInverse, ref angularMomentum, out d4);

            BepuQuaternion.Multiply(ref d1, dt / F64.C6, out d1);
            BepuQuaternion.Multiply(ref d2, dt / F64.C3, out d2);
            BepuQuaternion.Multiply(ref d3, dt / F64.C3, out d3);
            BepuQuaternion.Multiply(ref d4, dt / F64.C6, out d4);
            BepuQuaternion added;
            BepuQuaternion.Add(ref q, ref d1, out added);
            BepuQuaternion.Add(ref added, ref d2, out added);
            BepuQuaternion.Add(ref added, ref d3, out added);
            BepuQuaternion.Add(ref added, ref d4, out added);
            BepuQuaternion.Normalize(ref added, out newOrientation);
        }


        /// <summary>
        /// Finds the change in the rotation state BepuQuaternion provided the local inertia tensor and angular velocity.
        /// </summary>
        /// <param name="orientation">Orienatation of the object.</param>
        /// <param name="localInertiaTensorInverse">Local-space inertia tensor of the object being updated.</param>
        /// <param name="angularMomentum">Angular momentum of the object.</param>
        ///  <param name="orientationChange">Change in BepuQuaternion.</param>
        public static void DifferentiateBepuQuaternion(ref BepuQuaternion orientation, ref Matrix3x3 localInertiaTensorInverse, ref BepuVector3 angularMomentum, out BepuQuaternion orientationChange)
        {
            BepuQuaternion normalizedOrientation;
            BepuQuaternion.Normalize(ref orientation, out normalizedOrientation);
            Matrix3x3 tempRotMat;
            Matrix3x3.CreateFromBepuQuaternion(ref normalizedOrientation, out tempRotMat);
            Matrix3x3 tempInertiaTensorInverse;
            Matrix3x3.MultiplyTransposed(ref tempRotMat, ref localInertiaTensorInverse, out tempInertiaTensorInverse);
            Matrix3x3.Multiply(ref tempInertiaTensorInverse, ref tempRotMat, out tempInertiaTensorInverse);
            BepuVector3 halfspin;
            Matrix3x3.Transform(ref angularMomentum, ref tempInertiaTensorInverse, out halfspin);
            BepuVector3.Multiply(ref halfspin, F64.C0p5, out halfspin);
            var halfspinBepuQuaternion = new BepuQuaternion(halfspin.X, halfspin.Y, halfspin.Z, F64.C0);
            BepuQuaternion.Multiply(ref halfspinBepuQuaternion, ref normalizedOrientation, out orientationChange);
        }


        /// <summary>
        /// Gets the barycentric coordinates of the point with respect to a triangle's vertices.
        /// </summary>
        /// <param name="p">Point to compute the barycentric coordinates of.</param>
        /// <param name="a">First vertex in the triangle.</param>
        /// <param name="b">Second vertex in the triangle.</param>
        /// <param name="c">Third vertex in the triangle.</param>
        /// <param name="aWeight">Weight of the first vertex.</param>
        /// <param name="bWeight">Weight of the second vertex.</param>
        /// <param name="cWeight">Weight of the third vertex.</param>
        public static void GetBarycentricCoordinates(ref BepuVector3 p, ref BepuVector3 a, ref BepuVector3 b, ref BepuVector3 c, out Fix64 aWeight, out Fix64 bWeight, out Fix64 cWeight)
        {
            BepuVector3 ab, ac;
            BepuVector3.Subtract(ref b, ref a, out ab);
            BepuVector3.Subtract(ref c, ref a, out ac);
            BepuVector3 triangleNormal;
            BepuVector3.Cross(ref ab, ref ac, out triangleNormal);
            Fix64 x = triangleNormal.X < F64.C0 ? -triangleNormal.X : triangleNormal.X;
            Fix64 y = triangleNormal.Y < F64.C0 ? -triangleNormal.Y : triangleNormal.Y;
            Fix64 z = triangleNormal.Z < F64.C0 ? -triangleNormal.Z : triangleNormal.Z;

            Fix64 numeratorU, numeratorV, denominator;
            if (x >= y && x >= z)
            {
                //The projection of the triangle on the YZ plane is the largest.
                numeratorU = (p.Y - b.Y) * (b.Z - c.Z) - (b.Y - c.Y) * (p.Z - b.Z); //PBC
                numeratorV = (p.Y - c.Y) * (c.Z - a.Z) - (c.Y - a.Y) * (p.Z - c.Z); //PCA
                denominator = triangleNormal.X;
            }
            else if (y >= z)
            {
                //The projection of the triangle on the XZ plane is the largest.
                numeratorU = (p.X - b.X) * (b.Z - c.Z) - (b.X - c.X) * (p.Z - b.Z); //PBC
                numeratorV = (p.X - c.X) * (c.Z - a.Z) - (c.X - a.X) * (p.Z - c.Z); //PCA
                denominator = -triangleNormal.Y;
            }
            else
            {
                //The projection of the triangle on the XY plane is the largest.
                numeratorU = (p.X - b.X) * (b.Y - c.Y) - (b.X - c.X) * (p.Y - b.Y); //PBC
                numeratorV = (p.X - c.X) * (c.Y - a.Y) - (c.X - a.X) * (p.Y - c.Y); //PCA
                denominator = triangleNormal.Z;
            }

            if (denominator < F64.Cm1em9 || denominator > F64.C1em9)
            {
                denominator = F64.C1 / denominator;
                aWeight = numeratorU * denominator;
                bWeight = numeratorV * denominator;
                cWeight = F64.C1 - aWeight - bWeight;
            }
            else
            {
				//It seems to be a degenerate triangle.
				//In that case, pick one of the closest vertices.
				//MOST of the time, this will happen when the vertices
				//are all very close together (all three points form a single point).
				//Sometimes, though, it could be that it's more of a line.
				//If it's a little inefficient, don't worry- this is a corner case anyway.

				Fix64 distance1, distance2, distance3;
                BepuVector3.DistanceSquared(ref p, ref a, out distance1);
                BepuVector3.DistanceSquared(ref p, ref b, out distance2);
                BepuVector3.DistanceSquared(ref p, ref c, out distance3);
                if (distance1 < distance2 && distance1 < distance3)
                {
                    aWeight = F64.C1;
                    bWeight = F64.C0;
                    cWeight = F64.C0;
                }
                else if (distance2 < distance3)
                {
                    aWeight = F64.C0;
                    bWeight = F64.C1;
                    cWeight = F64.C0;
                }
                else
                {
                    aWeight = F64.C0;
                    bWeight = F64.C0;
                    cWeight = F64.C1;
                }
            }


        }




        #endregion
    }
}