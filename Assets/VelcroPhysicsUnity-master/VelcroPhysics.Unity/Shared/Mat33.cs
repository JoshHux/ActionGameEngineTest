using VelcroPhysics.Utilities;
using FixMath.NET;

namespace VelcroPhysics.Shared
{
    /// <summary>
    /// A 3-by-3 matrix. Stored in column-major order.
    /// </summary>
    public struct Mat33
    {
        public FVector3 ex, ey, ez;

        /// <summary>
        /// Construct this matrix using columns.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <param name="c3">The c3.</param>
        public Mat33(FVector3 c1, FVector3 c2, FVector3 c3)
        {
            ex = c1;
            ey = c2;
            ez = c3;
        }

        /// <summary>
        /// Set this matrix to all zeros.
        /// </summary>
        public void SetZero()
        {
            ex = FVector3.zero;
            ey = FVector3.zero;
            ez = FVector3.zero;
        }

        /// <summary>
        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public FVector3 Solve33(FVector3 b)
        {
            var det = FVector3.Dot(ex, FVector3.Cross(ey, ez));
            if (det !=Fix64.Zero) det =Fix64.One / det;

            return new FVector3(det * FVector3.Dot(b, FVector3.Cross(ey, ez)), det * FVector3.Dot(ex, FVector3.Cross(b, ez)),
                det * FVector3.Dot(ex, FVector3.Cross(ey, b)));
        }

        /// <summary>
        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases. Solve only the upper
        /// 2-by-2 matrix equation.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public FVector2 Solve22(FVector2 b)
        {
            Fix64 a11 = ex.x, a12 = ey.x, a21 = ex.y, a22 = ey.y;
            var det = a11 * a22 - a12 * a21;

            if (det !=Fix64.Zero) det =Fix64.One / det;

            return new FVector2(det * (a22 * b.x - a12 * b.y), det * (a11 * b.y - a21 * b.x));
        }

        /// Get the inverse of this matrix as a 2-by-2.
        /// Returns the zero matrix if singular.
        public void GetInverse22(ref Mat33 M)
        {
            Fix64 a = ex.x, b = ey.x, c = ex.y, d = ey.y;
            var det = a * d - b * c;
            if (det !=Fix64.Zero) det =Fix64.One / det;

            M.ex.x = det * d;
            M.ey.x = -det * b;
            M.ex.z =Fix64.Zero;
            M.ex.y = -det * c;
            M.ey.y = det * a;
            M.ey.z =Fix64.Zero;
            M.ez.x =Fix64.Zero;
            M.ez.y =Fix64.Zero;
            M.ez.z =Fix64.Zero;
        }

        /// Get the symmetric inverse of this matrix as a 3-by-3.
        /// Returns the zero matrix if singular.
        public void GetSymInverse33(ref Mat33 M)
        {
            var det = MathUtils.Dot(ex, MathUtils.Cross((FVector3) ey, ez));
            if (det !=Fix64.Zero) det =Fix64.One / det;

            Fix64 a11 = ex.x, a12 = ey.x, a13 = ez.x;
            Fix64 a22 = ey.y, a23 = ez.y;
            var a33 = ez.z;

            M.ex.x = det * (a22 * a33 - a23 * a23);
            M.ex.y = det * (a13 * a23 - a12 * a33);
            M.ex.z = det * (a12 * a23 - a13 * a22);

            M.ey.x = M.ex.y;
            M.ey.y = det * (a11 * a33 - a13 * a13);
            M.ey.z = det * (a13 * a12 - a11 * a23);

            M.ez.x = M.ex.z;
            M.ez.y = M.ey.z;
            M.ez.z = det * (a11 * a22 - a12 * a12);
        }
    }
}