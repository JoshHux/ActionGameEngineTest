using UnityEngine;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;
using VTransform = VelcroPhysics.Shared.VTransform;
using FixMath.NET;

namespace VelcroPhysics.Collision.RayCast
{
    public static class RayCastHelper
    {
        public static bool RayCastEdge(ref FVector2 start, ref FVector2 end, ref RayCastInput input,
            ref VTransform VTransform, out RayCastOutput output)
        {
            // p = p1 + t * d
            // v = v1 + s * e
            // p1 + t * d = v1 + s * e
            // s * e - t * d = p1 - v1

            output = new RayCastOutput();

            // Put the ray into the edge's frame of reference.
            var p1 = MathUtils.MulT(VTransform.q, input.Point1 - VTransform.p);
            var p2 = MathUtils.MulT(VTransform.q, input.Point2 - VTransform.p);
            var d = p2 - p1;

            var v1 = start;
            var v2 = end;
            var e = v2 - v1;
            var normal = new FVector2(e.y, -e.x); //TODO: Could possibly cache the normal.
            normal.Normalize();

            // q = p1 + t * d
            // dot(normal, q - v1) = 0
            // dot(normal, p1 - v1) + t * dot(normal, d) = 0
            var numerator = FVector2.Dot(normal, v1 - p1);
            var denominator = FVector2.Dot(normal, d);

            if (denominator ==Fix64.Zero) return false;

            var t = numerator / denominator;
            if (t <Fix64.Zero || input.MaxFraction < t) return false;

            var q = p1 + t * d;

            // q = v1 + s * r
            // s = dot(q - v1, r) / dot(r, r)
            var r = v2 - v1;
            var rr = FVector2.Dot(r, r);
            if (rr ==Fix64.Zero) return false;

            var s = FVector2.Dot(q - v1, r) / rr;
            if (s <Fix64.Zero ||Fix64.One < s) return false;

            output.Fraction = t;
            if (numerator >Fix64.Zero)
                output.Normal = -MathUtils.MulT(VTransform.q, normal);
            else
                output.Normal = MathUtils.MulT(VTransform.q, normal);
            return true;
        }

        public static bool RayCastCircle(ref FVector2 pos, Fix64 radius, ref RayCastInput input, ref VTransform VTransform,
            out RayCastOutput output)
        {
            // Collision Detection in Interactive 3D Environments by Gino van den Bergen
            // From Section 3.1.2
            // x = s + a * r
            // norm(x) = radius

            output = new RayCastOutput();

            var position = VTransform.p + MathUtils.Mul(VTransform.q, pos);
            var s = input.Point1 - position;
            var b = FVector2.Dot(s, s) - radius * radius;

            // Solve quadratic equation.
            var r = input.Point2 - input.Point1;
            var c = FVector2.Dot(s, r);
            var rr = FVector2.Dot(r, r);
            var sigma = c * c - rr * b;

            // Check for negative discriminant and short segment.
            if (sigma <Fix64.Zero || rr < Settings.Epsilon) return false;

            // Find the point of intersection of the line with the circle.
            var a = -(c + Fix64.Sqrt(sigma));

            // Is the intersection point on the segment?
            if (0.0f <= a && a <= input.MaxFraction * rr)
            {
                a /= rr;
                output.Fraction = a;
                output.Normal = s + a * r;
                output.Normal.Normalize();
                return true;
            }

            return false;
        }

        public static bool RayCastPolygon(Vertices vertices, Vertices normals, ref RayCastInput input,
            ref VTransform VTransform, out RayCastOutput output)
        {
            output = new RayCastOutput();

            // Put the ray into the polygon's frame of reference.
            var p1 = MathUtils.MulT(VTransform.q, input.Point1 - VTransform.p);
            var p2 = MathUtils.MulT(VTransform.q, input.Point2 - VTransform.p);
            var d = p2 - p1;

            Fix64 lower =Fix64.Zero, upper = input.MaxFraction;

            var index = -1;

            for (var i = 0; i < vertices.Count; ++i)
            {
                // p = p1 + a * d
                // dot(normal, p - v) = 0
                // dot(normal, p1 - v) + a * dot(normal, d) = 0
                var numerator = FVector2.Dot(normals[i], vertices[i] - p1);
                var denominator = FVector2.Dot(normals[i], d);

                if (denominator ==Fix64.Zero)
                {
                    if (numerator <Fix64.Zero) return false;
                }
                else
                {
                    // Note: we want this predicate without division:
                    // lower < numerator / denominator, where denominator < 0
                    // Since denominator < 0, we have to flip the inequality:
                    // lower < numerator / denominator <==> denominator * lower > numerator.
                    if (denominator <Fix64.Zero && numerator < lower * denominator)
                    {
                        // Increase lower.
                        // The segment enters this half-space.
                        lower = numerator / denominator;
                        index = i;
                    }
                    else if (denominator >Fix64.Zero && numerator < upper * denominator)
                    {
                        // Decrease upper.
                        // The segment exits this half-space.
                        upper = numerator / denominator;
                    }
                }

                // The use of epsilon here causes the assert on lower to trip
                // in some cases. Apparently the use of epsilon was to make edge
                // shapes work, but now those are handled separately.
                //if (upper < lower - b2_epsilon)
                if (upper < lower) return false;
            }

            Debug.Assert(0.0f <= lower && lower <= input.MaxFraction);

            if (index >= 0)
            {
                output.Fraction = lower;
                output.Normal = MathUtils.Mul(VTransform.q, normals[index]);
                return true;
            }

            return false;
        }
    }
}