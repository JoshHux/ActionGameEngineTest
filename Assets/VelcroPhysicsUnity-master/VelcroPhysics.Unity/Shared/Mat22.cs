using FixMath.NET;

namespace VelcroPhysics.Shared
{
    /// <summary>
    /// A 2-by-2 matrix. Stored in column-major order.
    /// </summary>
    public struct Mat22
    {
        public FVector2 ex, ey;

        /// <summary>
        /// Construct this matrix using columns.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        public Mat22(FVector2 c1, FVector2 c2)
        {
            ex = c1;
            ey = c2;
        }

        /// <summary>
        /// Construct this matrix using scalars.
        /// </summary>
        /// <param name="a11">The a11.</param>
        /// <param name="a12">The a12.</param>
        /// <param name="a21">The a21.</param>
        /// <param name="a22">The a22.</param>
        public Mat22(Fix64 a11, Fix64 a12, Fix64 a21, Fix64 a22)
        {
            ex = new FVector2(a11, a21);
            ey = new FVector2(a12, a22);
        }

        public Mat22 Inverse
        {
            get
            {
                Fix64 a = ex.x, b = ey.x, c = ex.y, d = ey.y;
                var det = a * d - b * c;
                if (det != Fix64.Zero) det = Fix64.One / det;

                var result = new Mat22();
                var rexx = det * d;
                var rexy = -det * c;
                var reyx = -det * b;
                var reyy = det * a;

                result.ex = new FVector2(rexx, rexy);
                result.ey = new FVector2(reyx, reyy);

                return result;
            }
        }

        /// <summary>
        /// Initialize this matrix using columns.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        public void Set(FVector2 c1, FVector2 c2)
        {
            ex = c1;
            ey = c2;
        }

        /// <summary>
        /// Set this to the identity matrix.
        /// </summary>
        public void SetIdentity()
        {
            //ex.x =Fix64.One;
            //ey.x =Fix64.Zero;
            //ex.y =Fix64.Zero;
            //ey.y =Fix64.One;
            ex = new FVector2(1, 0);
            ey = new FVector2(0, 1);
        }

        /// <summary>
        /// Set this matrix to all zeros.
        /// </summary>
        public void SetZero()
        {
            //ex.x =Fix64.Zero;
            //ey.x =Fix64.Zero;
            //ex.y =Fix64.Zero;
            //ey.y =Fix64.Zero;
            ex = new FVector2();
            ey = new FVector2();
        }

        /// <summary>
        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public FVector2 Solve(FVector2 b)
        {
            Fix64 a11 = ex.x, a12 = ey.x, a21 = ex.y, a22 = ey.y;
            var det = a11 * a22 - a12 * a21;
            if (det != Fix64.Zero) det = Fix64.One / det;

            return new FVector2(det * (a22 * b.x - a12 * b.y), det * (a11 * b.y - a21 * b.x));
        }

        public static void Add(ref Mat22 A, ref Mat22 B, out Mat22 R)
        {
            R.ex = A.ex + B.ex;
            R.ey = A.ey + B.ey;
        }
    }
}