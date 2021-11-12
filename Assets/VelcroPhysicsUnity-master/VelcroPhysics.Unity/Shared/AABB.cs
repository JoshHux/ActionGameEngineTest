using VelcroPhysics.Collision.RayCast;
using VelcroPhysics.Utilities;
using FixMath.NET;

namespace VelcroPhysics.Shared
{
    /// <summary>
    /// An axis aligned bounding box.
    /// </summary>
    public struct AABB
    {
        /// <summary>
        /// The lower vertex
        /// </summary>
        public FVector2 LowerBound;

        /// <summary>
        /// The upper vertex
        /// </summary>
        public FVector2 UpperBound;

        public AABB(FVector2 min, FVector2 max)
            : this(ref min, ref max)
        {
        }

        public AABB(FVector2 center, Fix64 width, Fix64 height)
            : this(center - new FVector2(width / 2, height / 2), center + new FVector2(width / 2, height / 2))
        {
        }

        public AABB(ref FVector2 min, ref FVector2 max)
        {
            LowerBound = new FVector2(Fix64.Min(min.x, max.x), Fix64.Min(min.y, max.y));
            UpperBound = new FVector2(Fix64.Max(min.x, max.x), Fix64.Max(min.y, max.y));
        }

        public Fix64 Width => UpperBound.x - LowerBound.x;

        public Fix64 Height => UpperBound.y - LowerBound.y;

        /// <summary>
        /// Get the center of the AABB.
        /// </summary>
        public FVector2 Center => FixedMath.C0p5 * (LowerBound + UpperBound);

        /// <summary>
        /// Get the extents of the AABB (half-widths).
        /// </summary>
        public FVector2 Extents => FixedMath.C0p5 * (UpperBound - LowerBound);

        /// <summary>
        /// Get the perimeter length
        /// </summary>
        public Fix64 Perimeter
        {
            get
            {
                var wx = UpperBound.x - LowerBound.x;
                var wy = UpperBound.y - LowerBound.y;
                return 2 * (wx + wy);
            }
        }

        /// <summary>
        /// Gets the vertices of the AABB.
        /// </summary>
        /// <value>The corners of the AABB</value>
        public Vertices Vertices
        {
            get
            {
                var vertices = new Vertices(4);
                vertices.Add(UpperBound);
                vertices.Add(new FVector2(UpperBound.x, LowerBound.y));
                vertices.Add(LowerBound);
                vertices.Add(new FVector2(LowerBound.x, UpperBound.y));
                return vertices;
            }
        }

        /// <summary>
        /// First quadrant
        /// </summary>
        public AABB Q1 => new AABB(Center, UpperBound);

        /// <summary>
        /// Second quadrant
        /// </summary>
        public AABB Q2 => new AABB(new FVector2(LowerBound.x, Center.y), new FVector2(Center.x, UpperBound.y));

        /// <summary>
        /// Third quadrant
        /// </summary>
        public AABB Q3 => new AABB(LowerBound, Center);

        /// <summary>
        /// Forth quadrant
        /// </summary>
        public AABB Q4 => new AABB(new FVector2(Center.x, LowerBound.y), new FVector2(UpperBound.x, Center.y));

        /// <summary>
        /// Verify that the bounds are sorted. And the bounds are valid numbers (not NaN).
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            var d = UpperBound - LowerBound;
            var valid = d.x >= Fix64.Zero && d.y >= Fix64.Zero;
            return valid && LowerBound.IsValid() && UpperBound.IsValid();
        }

        /// <summary>
        /// Combine an AABB into this one.
        /// </summary>
        /// <param name="aabb">The AABB.</param>
        public void Combine(ref AABB aabb)
        {
            LowerBound = FVector2.Min(LowerBound, aabb.LowerBound);
            UpperBound = FVector2.Max(UpperBound, aabb.UpperBound);
        }

        /// <summary>
        /// Combine two AABBs into this one.
        /// </summary>
        /// <param name="aabb1">The aabb1.</param>
        /// <param name="aabb2">The aabb2.</param>
        public void Combine(ref AABB aabb1, ref AABB aabb2)
        {
            LowerBound = FVector2.Min(aabb1.LowerBound, aabb2.LowerBound);
            UpperBound = FVector2.Max(aabb1.UpperBound, aabb2.UpperBound);
        }

        /// <summary>
        /// Does this AABB contain the provided AABB.
        /// </summary>
        /// <param name="aabb">The AABB.</param>
        /// <returns>
        /// <c>true</c> if it contains the specified AABB; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref AABB aabb)
        {
            var result = LowerBound.x <= aabb.LowerBound.x;
            result = result && LowerBound.y <= aabb.LowerBound.y;
            result = result && aabb.UpperBound.x <= UpperBound.x;
            result = result && aabb.UpperBound.y <= UpperBound.y;
            return result;
        }

        /// <summary>
        /// Determines whether the AABB contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// <c>true</c> if it contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref FVector2 point)
        {
            //using epsilon to try and guard against Fix64 rounding errors.
            return point.x > LowerBound.x + Settings.Epsilon && point.x < UpperBound.x - Settings.Epsilon &&
                   point.y > LowerBound.y + Settings.Epsilon && point.y < UpperBound.y - Settings.Epsilon;
        }

        /// <summary>
        /// Test if the two AABBs overlap.
        /// </summary>
        /// <param name="a">The first AABB.</param>
        /// <param name="b">The second AABB.</param>
        /// <returns>True if they are overlapping.</returns>
        public static bool TestOverlap(ref AABB a, ref AABB b)
        {
            var d1 = b.LowerBound - a.UpperBound;
            var d2 = a.LowerBound - b.UpperBound;

            return d1.x <= 0 && d1.y <= 0 && d2.x <= 0 && d2.y <= 0;
        }

        /// <summary>
        /// Raycast against this AABB using the specified points and maxfraction (found in input)
        /// </summary>
        /// <param name="output">The results of the raycast.</param>
        /// <param name="input">The parameters for the raycast.</param>
        /// <returns>True if the ray intersects the AABB</returns>
        public bool RayCast(out RayCastOutput output, ref RayCastInput input, bool doInteriorCheck = true)
        {
            // From Real-time Collision Detection, p179.

            output = new RayCastOutput();

            var tmin = -Settings.MaxFix64;
            var tmax = Settings.MaxFix64;

            var p = input.Point1;
            var d = input.Point2 - input.Point1;
            var absD = MathUtils.Abs(d);

            var normal = FVector2.zero;

            for (var i = 0; i < 2; ++i)
            {
                var absD_i = i == 0 ? absD.x : absD.y;
                var lowerBound_i = i == 0 ? LowerBound.x : LowerBound.y;
                var upperBound_i = i == 0 ? UpperBound.x : UpperBound.y;
                var p_i = i == 0 ? p.x : p.y;

                if (absD_i < Settings.Epsilon)
                {
                    // Parallel.
                    if (p_i < lowerBound_i || upperBound_i < p_i) return false;
                }
                else
                {
                    var d_i = i == 0 ? d.x : d.y;

                    var inv_d = Fix64.One / d_i;
                    var t1 = (lowerBound_i - p_i) * inv_d;
                    var t2 = (upperBound_i - p_i) * inv_d;

                    // Sign of the normal vector.
                    var s = -Fix64.One;

                    if (t1 > t2)
                    {
                        MathUtils.Swap(ref t1, ref t2);
                        s = Fix64.One;
                    }

                    // Push the min up
                    if (t1 > tmin)
                    {
                        if (i == 0)
                            normal = new FVector2(s, normal.y);
                        else
                            normal = new FVector2(normal.x, s);

                        tmin = t1;
                    }

                    // Pull the max down
                    tmax = Fix64.Min(tmax, t2);

                    if (tmin > tmax) return false;
                }
            }

            // Does the ray start inside the box?
            // Does the ray intersect beyond the max fraction?
            if (doInteriorCheck && (tmin < Fix64.Zero || input.MaxFraction < tmin)) return false;

            // Intersection.
            output.Fraction = tmin;
            output.Normal = normal;
            return true;
        }
    }
}