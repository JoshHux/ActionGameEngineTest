using UnityEngine;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;
using VTransform = VelcroPhysics.Shared.VTransform;

namespace VelcroPhysics.Collision
{
    public static class AABBHelper
    {
        public static void ComputeEdgeAABB(ref Vector2 start, ref Vector2 end, ref VTransform VTransform, out AABB aabb)
        {
            var v1 = MathUtils.Mul(ref VTransform, ref start);
            var v2 = MathUtils.Mul(ref VTransform, ref end);

            aabb.LowerBound = Vector2.Min(v1, v2);
            aabb.UpperBound = Vector2.Max(v1, v2);

            var r = new Vector2(Settings.PolygonRadius, Settings.PolygonRadius);
            aabb.LowerBound = aabb.LowerBound - r;
            aabb.UpperBound = aabb.UpperBound + r;
        }

        public static void ComputeCircleAABB(ref Vector2 pos, float radius, ref VTransform VTransform, out AABB aabb)
        {
            var p = VTransform.p + MathUtils.Mul(VTransform.q, pos);
            aabb.LowerBound = new Vector2(p.x - radius, p.y - radius);
            aabb.UpperBound = new Vector2(p.x + radius, p.y + radius);
        }

        public static void ComputePolygonAABB(Vertices vertices, ref VTransform VTransform, out AABB aabb)
        {
            var lower = MathUtils.Mul(ref VTransform, vertices[0]);
            var upper = lower;

            for (var i = 1; i < vertices.Count; ++i)
            {
                var v = MathUtils.Mul(ref VTransform, vertices[i]);
                lower = Vector2.Min(lower, v);
                upper = Vector2.Max(upper, v);
            }

            var r = new Vector2(Settings.PolygonRadius, Settings.PolygonRadius);
            aabb.LowerBound = lower - r;
            aabb.UpperBound = upper + r;
        }
    }
}