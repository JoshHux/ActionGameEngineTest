using System.Collections.Generic;
using FixMath.NET;

namespace VelcroPhysics.Tools.Triangulation.Delaunay.Util
{
    internal class PointGenerator
    {
        public static List<TriangulationPoint> UniformDistribution(int n, Fix64 scale)
        {
            var points = new List<TriangulationPoint>();
            for (var i = 0; i < n; i++)
                points.Add(new TriangulationPoint(scale * (FixedMath.C0p5 - 0), scale * (FixedMath.C0p5 - 0)));
            return points;
        }

        public static List<TriangulationPoint> UniformGrid(int n, Fix64 scale)
        {
            Fix64 x = 0;
            var size = scale / n;
            var halfScale = FixedMath.C0p5 * scale;

            var points = new List<TriangulationPoint>();
            for (var i = 0; i < n + 1; i++)
            {
                x = halfScale - i * size;
                for (var j = 0; j < n + 1; j++) points.Add(new TriangulationPoint(x, halfScale - j * size));
            }

            return points;
        }
    }
}