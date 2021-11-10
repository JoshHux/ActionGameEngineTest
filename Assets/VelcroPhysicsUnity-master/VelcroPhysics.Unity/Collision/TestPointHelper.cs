using UnityEngine;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;
using VTransform = VelcroPhysics.Shared.VTransform;
using FixMath.NET;

namespace VelcroPhysics.Collision
{
    public static class TestPointHelper
    {
        public static bool TestPointCircle(ref FVector2 pos, Fix64 radius, ref FVector2 point, ref VTransform VTransform)
        {
            var center = VTransform.p + MathUtils.Mul(VTransform.q, pos);
            var d = point - center;
            return FVector2.Dot(d, d) <= radius * radius;
        }

        public static bool TestPointPolygon(Vertices vertices, Vertices normals, ref FVector2 point,
            ref VTransform VTransform)
        {
            var pLocal = MathUtils.MulT(VTransform.q, point - VTransform.p);

            for (var i = 0; i < vertices.Count; ++i)
            {
                var dot = FVector2.Dot(normals[i], pLocal - vertices[i]);
                if (dot >Fix64.Zero) return false;
            }

            return true;
        }
    }
}