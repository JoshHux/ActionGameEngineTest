using UnityEngine;
using VelcroPhysics.Shared;
using VelcroPhysics.Shared.Optimization;
using VelcroPhysics.Utilities;
using VTransform = VelcroPhysics.Shared.VTransform;
using FixMath.NET;

namespace VelcroPhysics.Collision.Narrowphase
{
    public static class WorldManifold
    {
        /// <summary>
        /// Evaluate the manifold with supplied VTransforms. This assumes
        /// modest motion from the original state. This does not change the
        /// point count, impulses, etc. The radii must come from the Shapes
        /// that generated the manifold.
        /// </summary>
        public static void Initialize(ref Manifold manifold, ref VTransform xfA, Fix64 radiusA, ref VTransform xfB,
            Fix64 radiusB, out FVector2 normal, out FixedArray2<FVector2> points, out FixedArray2<Fix64> separations)
        {
            normal = FVector2.zero;
            points = new FixedArray2<FVector2>();
            separations = new FixedArray2<Fix64>();

            if (manifold.PointCount == 0) return;

            switch (manifold.Type)
            {
                case ManifoldType.Circles:
                    {
                        normal = new FVector2(1, Fix64.Zero);
                        var pointA = MathUtils.Mul(ref xfA, manifold.LocalPoint);
                        var pointB = MathUtils.Mul(ref xfB, manifold.Points.Value0.LocalPoint);
                        if (Fix64.Sqrt(FVector2.Distance(pointA, pointB)) > Settings.Epsilon * Settings.Epsilon)
                        {
                            normal = pointB - pointA;
                            normal.Normalize();
                        }

                        var cA = pointA + radiusA * normal;
                        var cB = pointB - radiusB * normal;
                        points.Value0 = FixedMath.C0p5 * (cA + cB);
                        separations.Value0 = FVector2.Dot(cB - cA, normal);
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        normal = MathUtils.Mul(xfA.q, manifold.LocalNormal);
                        var planePoint = MathUtils.Mul(ref xfA, manifold.LocalPoint);

                        for (var i = 0; i < manifold.PointCount; ++i)
                        {
                            var clipPoint = MathUtils.Mul(ref xfB, manifold.Points[i].LocalPoint);
                            var cA = clipPoint + (radiusA - FVector2.Dot(clipPoint - planePoint, normal)) * normal;
                            var cB = clipPoint - radiusB * normal;
                            points[i] = FixedMath.C0p5 * (cA + cB);
                            separations[i] = FVector2.Dot(cB - cA, normal);
                        }
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        normal = MathUtils.Mul(xfB.q, manifold.LocalNormal);
                        var planePoint = MathUtils.Mul(ref xfB, manifold.LocalPoint);

                        for (var i = 0; i < manifold.PointCount; ++i)
                        {
                            var clipPoint = MathUtils.Mul(ref xfA, manifold.Points[i].LocalPoint);
                            var cB = clipPoint + (radiusB - FVector2.Dot(clipPoint - planePoint, normal)) * normal;
                            var cA = clipPoint - radiusA * normal;
                            points[i] = FixedMath.C0p5 * (cA + cB);
                            separations[i] = FVector2.Dot(cA - cB, normal);
                        }

                        // Ensure normal points from A to B.
                        normal = -normal;
                    }
                    break;
            }
        }
    }
}