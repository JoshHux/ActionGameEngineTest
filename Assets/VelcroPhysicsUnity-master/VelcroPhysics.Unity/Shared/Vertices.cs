using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VelcroPhysics.Utilities;
using FixMath.NET;
using Debug = System.Diagnostics.Debug;

namespace VelcroPhysics.Shared
{
    [DebuggerDisplay("Count = {Count} Vertices = {ToString()}")]
    public class Vertices : List<FVector2>
    {
        public Vertices()
        {
        }

        public Vertices(int capacity) : base(capacity)
        {
        }

        public Vertices(IEnumerable<FVector2> vertices)
        {
            AddRange(vertices);
        }

        internal bool AttachedToBody { get; set; }

        /// <summary>
        /// You can add holes to this collection.
        /// It will get respected by some of the triangulation algoithms, but otherwise not used.
        /// </summary>
        public List<Vertices> Holes { get; set; }

        /// <summary>
        /// Gets the next index. Used for iterating all the edges with wrap-around.
        /// </summary>
        /// <param name="index">The current index</param>
        public int NextIndex(int index)
        {
            return index + 1 > Count - 1 ? 0 : index + 1;
        }

        /// <summary>
        /// Gets the next vertex. Used for iterating all the edges with wrap-around.
        /// </summary>
        /// <param name="index">The current index</param>
        public FVector2 NextVertex(int index)
        {
            return this[NextIndex(index)];
        }

        /// <summary>
        /// Gets the previous index. Used for iterating all the edges with wrap-around.
        /// </summary>
        /// <param name="index">The current index</param>
        public int PreviousIndex(int index)
        {
            return index - 1 < 0 ? Count - 1 : index - 1;
        }

        /// <summary>
        /// Gets the previous vertex. Used for iterating all the edges with wrap-around.
        /// </summary>
        /// <param name="index">The current index</param>
        public FVector2 PreviousVertex(int index)
        {
            return this[PreviousIndex(index)];
        }

        /// <summary>
        /// Gets the signed area.
        /// If the area is less than 0, it indicates that the polygon is clockwise winded.
        /// </summary>
        /// <returns>The signed area</returns>
        public Fix64 GetSignedArea()
        {
            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (Count < 3)
                return 0;

            int i;
            Fix64 area = 0;

            for (i = 0; i < Count; i++)
            {
                var j = (i + 1) % Count;

                var vi = this[i];
                var vj = this[j];

                area += vi.x * vj.y;
                area -= vi.y * vj.x;
            }

            area /= 2.0f;
            return area;
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <returns></returns>
        public Fix64 GetArea()
        {
            var area = GetSignedArea();
            return area < 0 ? -area : area;
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <returns></returns>
        public FVector2 GetCentroid()
        {
            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (Count < 3)
                return new FVector2(Fix64.NaN, Fix64.NaN);

            // Same algorithm is used by Box2D
            var c = FVector2.zero;
            var area =Fix64.Zero;
            const Fix64 inv3 =Fix64.One / 3.0f;

            for (var i = 0; i < Count; ++i)
            {
                // Triangle vertices.
                var current = this[i];
                var next = i + 1 < Count ? this[i + 1] : this[0];

                var triangleArea = 0.5f * (current.x * next.y - current.y * next.x);
                area += triangleArea;

                // Area weighted centroid
                c += triangleArea * inv3 * (current + next);
            }

            // Centroid
            c *=Fix64.One / area;
            return c;
        }

        /// <summary>
        /// Returns an AABB that fully contains this polygon.
        /// </summary>
        public AABB GetAABB()
        {
            AABB aabb;
            var lowerBound = new FVector2(Fix64.MaxValue, Fix64.MaxValue);
            var upperBound = new FVector2(Fix64.MinValue, Fix64.MinValue);

            for (var i = 0; i < Count; ++i)
            {
                if (this[i].x < lowerBound.x) lowerBound.x = this[i].x;
                if (this[i].x > upperBound.x) upperBound.x = this[i].x;

                if (this[i].y < lowerBound.y) lowerBound.y = this[i].y;
                if (this[i].y > upperBound.y) upperBound.y = this[i].y;
            }

            aabb.LowerBound = lowerBound;
            aabb.UpperBound = upperBound;

            return aabb;
        }

        /// <summary>
        /// Translates the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Translate(FVector2 value)
        {
            Translate(ref value);
        }

        /// <summary>
        /// Translates the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The vector.</param>
        public void Translate(ref FVector2 value)
        {
            Debug.Assert(!AttachedToBody,
                "Translating vertices that are used by a Body can result in unstable behavior. Use Body.Position instead.");

            for (var i = 0; i < Count; i++)
                this[i] += value;

            if (Holes != null && Holes.Count > 0)
                foreach (var hole in Holes)
                    hole.Translate(ref value);
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(FVector2 value)
        {
            Scale(ref value);
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(ref FVector2 value)
        {
            Debug.Assert(!AttachedToBody, "Scaling vertices that are used by a Body can result in unstable behavior.");

            for (var i = 0; i < Count; i++)
                this[i] *= value;

            if (Holes != null && Holes.Count > 0)
                foreach (var hole in Holes)
                    hole.Scale(ref value);
        }

        /// <summary>
        /// Rotate the vertices with the defined value in radians.
        /// Warning: Using this method on an active set of vertices of a Body,
        /// will cause problems with collisions. Use Body.Rotation instead.
        /// </summary>
        /// <param name="value">The amount to rotate by in radians.</param>
        public void Rotate(Fix64 value)
        {
            Debug.Assert(!AttachedToBody, "Rotating vertices that are used by a Body can result in unstable behavior.");

            var num1 = Fix64.Cos(value);
            var num2 = Fix64.Sin(value);

            for (var i = 0; i < Count; i++)
            {
                var position = this[i];
                this[i] = new FVector2(position.x * num1 + position.y * -num2, position.x * num2 + position.y * num1);
            }

            if (Holes != null && Holes.Count > 0)
                foreach (var hole in Holes)
                    hole.Rotate(value);
        }

        /// <summary>
        /// Determines whether the polygon is convex.
        /// O(n^2) running time.
        /// Assumptions:
        /// - The polygon is in counter clockwise order
        /// - The polygon has no overlapping edges
        /// </summary>
        /// <returns>
        /// <c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvex()
        {
            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (Count < 3)
                return false;

            //Triangles are always convex
            if (Count == 3)
                return true;

            // Checks the polygon is convex and the interior is to the left of each edge.
            for (var i = 0; i < Count; ++i)
            {
                var next = i + 1 < Count ? i + 1 : 0;
                var edge = this[next] - this[i];

                for (var j = 0; j < Count; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i || j == next)
                        continue;

                    var r = this[j] - this[i];

                    var s = edge.x * r.y - edge.y * r.x;

                    if (s <=Fix64.Zero)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Indicates if the vertices are in counter clockwise order.
        /// Warning: If the area of the polygon is 0, it is unable to determine the winding.
        /// </summary>
        public bool IsCounterClockWise()
        {
            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (Count < 3)
                return false;

            return GetSignedArea() >Fix64.Zero;
        }

        /// <summary>
        /// Forces the vertices to be counter clock wise order.
        /// </summary>
        public void ForceCounterClockWise()
        {
            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (Count < 3)
                return;

            if (!IsCounterClockWise())
                Reverse();
        }

        /// <summary>
        /// Checks if the vertices forms an simple polygon by checking for edge crossings.
        /// </summary>
        public bool IsSimple()
        {
            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (Count < 3)
                return false;

            for (var i = 0; i < Count; ++i)
            {
                var a1 = this[i];
                var a2 = NextVertex(i);
                for (var j = i + 1; j < Count; ++j)
                {
                    var b1 = this[j];
                    var b2 = NextVertex(j);

                    FVector2 temp;

                    if (LineUtils.LineIntersect2(ref a1, ref a2, ref b1, ref b2, out temp))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the polygon is valid for use in the engine.
        /// Performs a full check, for simplicity, convexity,
        /// orientation, minimum angle, and volume.
        /// From Eric Jordan's convex decomposition library
        /// </summary>
        /// <returns>PolygonError.NoError if there were no error.</returns>
        public PolygonError CheckPolygon()
        {
            //TODO: TBM
            //if (Count < 3 || Count > Settings.MaxPolygonVertices)
            //    return PolygonError.InvalidAmountOfVertices;

            if (!IsSimple())
                return PolygonError.NotSimple;

            if (GetArea() <= Fix64.Epsilon)
                return PolygonError.AreaTooSmall;

            if (!IsConvex())
                return PolygonError.NotConvex;

            //Check if the sides are of adequate length.
            for (var i = 0; i < Count; ++i)
            {
                var next = i + 1 < Count ? i + 1 : 0;
                var edge = this[next] - this[i];
                if (edge.sqrMagnitude <= Fix64.Epsilon * Fix64.Epsilon) return PolygonError.SideTooSmall;
            }

            if (!IsCounterClockWise())
                return PolygonError.NotCounterClockWise;

            return PolygonError.NoError;
        }

        /// <summary>
        /// Projects to axis.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public void ProjectToAxis(ref FVector2 axis, out Fix64 min, out Fix64 max)
        {
            // To project a point on an axis use the dot product
            var dotProduct = FVector2.Dot(axis, this[0]);
            min = dotProduct;
            max = dotProduct;

            for (var i = 0; i < Count; i++)
            {
                dotProduct = FVector2.Dot(this[i], axis);
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else
                {
                    if (dotProduct > max) max = dotProduct;
                }
            }
        }

        /// <summary>
        /// Winding number test for a point in a polygon.
        /// </summary>
        /// See more info about the algorithm here: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        /// <param name="point">The point to be tested.</param>
        /// <returns>
        /// -1 if the winding number is zero and the point is outside
        /// the polygon, 1 if the point is inside the polygon, and 0 if the point
        /// is on the polygons edge.
        /// </returns>
        public int PointInPolygon(ref FVector2 point)
        {
            // Winding number
            var wn = 0;

            // Iterate through polygon's edges
            for (var i = 0; i < Count; i++)
            {
                // Get points
                var p1 = this[i];
                var p2 = this[NextIndex(i)];

                // Test if a point is directly on the edge
                var edge = p2 - p1;
                var area = MathUtils.Area(ref p1, ref p2, ref point);
                if (area == 0f && FVector2.Dot(point - p1, edge) >= 0f && FVector2.Dot(point - p2, edge) <= 0f) return 0;

                // Test edge for intersection with ray from point
                if (p1.y <= point.y)
                {
                    if (p2.y > point.y && area > 0f) ++wn;
                }
                else
                {
                    if (p2.y <= point.y && area < 0f) --wn;
                }
            }

            return wn == 0 ? -1 : 1;
        }

        /// <summary>
        /// Compute the sum of the angles made between the test point and each pair of points making up the polygon.
        /// If this sum is 2pi then the point is an interior point, if 0 then the point is an exterior point.
        /// ref: http://ozviz.wasp.uwa.edu.au/~pbourke/geometry/insidepoly/  - Solution 2
        /// </summary>
        public bool PointInPolygonAngle(ref FVector2 point)
        {
            Fix64 angle = 0;

            // Iterate through polygon's edges
            for (var i = 0; i < Count; i++)
            {
                // Get points
                var p1 = this[i] - point;
                var p2 = this[NextIndex(i)] - point;

                angle += FVector2.Angle(p1, p2);
            }

            if (Fix64.Abs(angle) < Fix64.PI) return false;

            return true;
        }

        /// <summary>
        /// VTransforms the polygon using the defined matrix.
        /// </summary>
        /// <param name="VTransform">The matrix to use as VTransformation.</param>
        public void VTransform(ref Matrix4x4 VTransform)
        {
            // VTransform main polygon
            for (var i = 0; i < Count; i++)
                this[i].VTransform(VTransform);

            // VTransform holes
            if (Holes != null && Holes.Count > 0)
                for (var i = 0; i < Holes.Count; i++)
                {
                    var temp = Holes[i].ToArray();
                    temp.VTransform(ref VTransform, temp);

                    Holes[i] = new Vertices(temp);
                }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < Count; i++)
            {
                builder.Append(this[i]);
                if (i < Count - 1) builder.Append(" ");
            }

            return builder.ToString();
        }
    }
}