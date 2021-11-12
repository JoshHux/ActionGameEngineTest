using UnityEngine;
using VelcroPhysics.Collision.Distance;
using VelcroPhysics.Collision.Narrowphase;
using VelcroPhysics.Utilities;
using VTransform = VelcroPhysics.Shared.VTransform;
using FixMath.NET;

namespace VelcroPhysics.Collision.TOI
{
    public static class SeparationFunction
    {
        public static void Initialize(ref SimplexCache cache, DistanceProxy proxyA, ref Sweep sweepA,
            DistanceProxy proxyB, ref Sweep sweepB, Fix64 t1, out FVector2 axis, out FVector2 localPoint,
            out SeparationFunctionType type)
        {
            int count = cache.Count;
            UnityEngine.Debug.Assert(0 < count && count < 3);

            VTransform xfA, xfB;
            sweepA.GetVTransform(out xfA, t1);
            sweepB.GetVTransform(out xfB, t1);

            if (count == 1)
            {
                localPoint = FVector2.zero;
                type = SeparationFunctionType.Points;
                var localPointA = proxyA.Vertices[cache.IndexA[0]];
                var localPointB = proxyB.Vertices[cache.IndexB[0]];
                var pointA = MathUtils.Mul(ref xfA, localPointA);
                var pointB = MathUtils.Mul(ref xfB, localPointB);
                axis = pointB - pointA;
                axis.Normalize();
            }
            else if (cache.IndexA[0] == cache.IndexA[1])
            {
                // Two points on B and one on A.
                type = SeparationFunctionType.FaceB;
                var localPointB1 = proxyB.Vertices[cache.IndexB[0]];
                var localPointB2 = proxyB.Vertices[cache.IndexB[1]];

                var a = localPointB2 - localPointB1;
                axis = new FVector2(a.y, -a.x);
                axis.Normalize();
                var normal = MathUtils.Mul(ref xfB.q, axis);

                localPoint = FixedMath.C0p5 * (localPointB1 + localPointB2);
                var pointB = MathUtils.Mul(ref xfB, localPoint);

                var localPointA = proxyA.Vertices[cache.IndexA[0]];
                var pointA = MathUtils.Mul(ref xfA, localPointA);

                var s = FVector2.Dot(pointA - pointB, normal);
                if (s < Fix64.Zero) axis = -axis;
            }
            else
            {
                // Two points on A and one or two points on B.
                type = SeparationFunctionType.FaceA;
                var localPointA1 = proxyA.Vertices[cache.IndexA[0]];
                var localPointA2 = proxyA.Vertices[cache.IndexA[1]];

                var a = localPointA2 - localPointA1;
                axis = new FVector2(a.y, -a.x);
                axis.Normalize();
                var normal = MathUtils.Mul(ref xfA.q, axis);

                localPoint = FixedMath.C0p5 * (localPointA1 + localPointA2);
                var pointA = MathUtils.Mul(ref xfA, localPoint);

                var localPointB = proxyB.Vertices[cache.IndexB[0]];
                var pointB = MathUtils.Mul(ref xfB, localPointB);

                var s = FVector2.Dot(pointB - pointA, normal);
                if (s < Fix64.Zero) axis = -axis;
            }

            //Velcro note: the returned value that used to be here has been removed, as it was not used.
        }

        public static Fix64 FindMinSeparation(out int indexA, out int indexB, Fix64 t, DistanceProxy proxyA,
            ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, ref FVector2 axis, ref FVector2 localPoint,
            SeparationFunctionType type)
        {
            VTransform xfA, xfB;
            sweepA.GetVTransform(out xfA, t);
            sweepB.GetVTransform(out xfB, t);

            switch (type)
            {
                case SeparationFunctionType.Points:
                    {
                        var axisA = MathUtils.MulT(ref xfA.q, axis);
                        var axisB = MathUtils.MulT(ref xfB.q, -axis);

                        indexA = proxyA.GetSupport(axisA);
                        indexB = proxyB.GetSupport(axisB);

                        var localPointA = proxyA.Vertices[indexA];
                        var localPointB = proxyB.Vertices[indexB];

                        var pointA = MathUtils.Mul(ref xfA, localPointA);
                        var pointB = MathUtils.Mul(ref xfB, localPointB);

                        var separation = FVector2.Dot(pointB - pointA, axis);
                        return separation;
                    }

                case SeparationFunctionType.FaceA:
                    {
                        var normal = MathUtils.Mul(ref xfA.q, axis);
                        var pointA = MathUtils.Mul(ref xfA, localPoint);

                        var axisB = MathUtils.MulT(ref xfB.q, -normal);

                        indexA = -1;
                        indexB = proxyB.GetSupport(axisB);

                        var localPointB = proxyB.Vertices[indexB];
                        var pointB = MathUtils.Mul(ref xfB, localPointB);

                        var separation = FVector2.Dot(pointB - pointA, normal);
                        return separation;
                    }

                case SeparationFunctionType.FaceB:
                    {
                        var normal = MathUtils.Mul(ref xfB.q, axis);
                        var pointB = MathUtils.Mul(ref xfB, localPoint);

                        var axisA = MathUtils.MulT(ref xfA.q, -normal);

                        indexB = -1;
                        indexA = proxyA.GetSupport(axisA);

                        var localPointA = proxyA.Vertices[indexA];
                        var pointA = MathUtils.Mul(ref xfA, localPointA);

                        var separation = FVector2.Dot(pointA - pointB, normal);
                        return separation;
                    }

                default:
                    UnityEngine.Debug.Assert(false);
                    indexA = -1;
                    indexB = -1;
                    return Fix64.Zero;
            }
        }

        public static Fix64 Evaluate(int indexA, int indexB, Fix64 t, DistanceProxy proxyA, ref Sweep sweepA,
            DistanceProxy proxyB, ref Sweep sweepB, ref FVector2 axis, ref FVector2 localPoint,
            SeparationFunctionType type)
        {
            VTransform xfA, xfB;
            sweepA.GetVTransform(out xfA, t);
            sweepB.GetVTransform(out xfB, t);

            switch (type)
            {
                case SeparationFunctionType.Points:
                    {
                        var localPointA = proxyA.Vertices[indexA];
                        var localPointB = proxyB.Vertices[indexB];

                        var pointA = MathUtils.Mul(ref xfA, localPointA);
                        var pointB = MathUtils.Mul(ref xfB, localPointB);
                        var separation = FVector2.Dot(pointB - pointA, axis);

                        return separation;
                    }
                case SeparationFunctionType.FaceA:
                    {
                        var normal = MathUtils.Mul(ref xfA.q, axis);
                        var pointA = MathUtils.Mul(ref xfA, localPoint);

                        var localPointB = proxyB.Vertices[indexB];
                        var pointB = MathUtils.Mul(ref xfB, localPointB);

                        var separation = FVector2.Dot(pointB - pointA, normal);
                        return separation;
                    }
                case SeparationFunctionType.FaceB:
                    {
                        var normal = MathUtils.Mul(ref xfB.q, axis);
                        var pointB = MathUtils.Mul(ref xfB, localPoint);

                        var localPointA = proxyA.Vertices[indexA];
                        var pointA = MathUtils.Mul(ref xfA, localPointA);

                        var separation = FVector2.Dot(pointA - pointB, normal);
                        return separation;
                    }
                default:
                    UnityEngine.Debug.Assert(false);
                    return Fix64.Zero;
            }
        }
    }
}