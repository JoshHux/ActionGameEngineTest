using VelcroPhysics.Collision.Narrowphase;
using VelcroPhysics.Utilities;
using FixMath.NET;
using VTransform = VelcroPhysics.Shared.VTransform;
using Debug = UnityEngine.Debug;

namespace VelcroPhysics.Dynamics.Solver
{
    public static class PositionSolverManifold
    {
        public static void Initialize(ContactPositionConstraint pc, VTransform xfA, VTransform xfB, int index,
            out FVector2 normal, out FVector2 point, out Fix64 separation)
        {
            UnityEngine.Debug.Assert(pc.PointCount > 0);

            switch (pc.Type)
            {
                case ManifoldType.Circles:
                    {
                        var pointA = MathUtils.Mul(ref xfA, pc.LocalPoint);
                        var pointB = MathUtils.Mul(ref xfB, pc.LocalPoints[0]);
                        normal = pointB - pointA;

                        //Velcro: Fix to handle zero normalization
                        if (normal != FVector2.zero)
                            normal.Normalize();

                        point = FixedMath.C0p5 * (pointA + pointB);
                        separation = FVector2.Dot(pointB - pointA, normal) - pc.RadiusA - pc.RadiusB;
                    }
                    break;

                case ManifoldType.FaceA:
                    {
                        normal = MathUtils.Mul(xfA.q, pc.LocalNormal);
                        var planePoint = MathUtils.Mul(ref xfA, pc.LocalPoint);

                        var clipPoint = MathUtils.Mul(ref xfB, pc.LocalPoints[index]);
                        separation = FVector2.Dot(clipPoint - planePoint, normal) - pc.RadiusA - pc.RadiusB;
                        point = clipPoint;
                    }
                    break;

                case ManifoldType.FaceB:
                    {
                        normal = MathUtils.Mul(xfB.q, pc.LocalNormal);
                        var planePoint = MathUtils.Mul(ref xfB, pc.LocalPoint);

                        var clipPoint = MathUtils.Mul(ref xfA, pc.LocalPoints[index]);
                        separation = FVector2.Dot(clipPoint - planePoint, normal) - pc.RadiusA - pc.RadiusB;
                        point = clipPoint;

                        // Ensure normal points from A to B
                        normal = -normal;
                    }
                    break;
                default:
                    normal = FVector2.zero;
                    point = FVector2.zero;
                    separation = 0;
                    break;
            }
        }
    }
}