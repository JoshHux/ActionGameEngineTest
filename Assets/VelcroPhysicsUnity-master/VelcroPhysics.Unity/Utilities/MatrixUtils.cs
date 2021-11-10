using UnityEngine;
using FixMath.NET;

namespace VelcroPhysics.Unity.Utilities
{
    public static class MatrixUtils
    {
        public static void CreateRotationZ(this ref Matrix4x4 matrix, Fix64 radians)
        {
            matrix.m00 = Fix64.Cos(radians);
            matrix.m01 = Fix64.Sin(radians);
            matrix.m10 = -matrix.m01;
            matrix.m11 = matrix.m00;
        }
    }
}