﻿/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System.Runtime.InteropServices;
using UnityEngine;
using VelcroPhysics.Shared;
using VTransform = VelcroPhysics.Shared.VTransform;

namespace VelcroPhysics.Utilities
{
    public static class MathUtils
    {
        public static float Cross(ref Vector2 a, ref Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static float Cross(Vector2 a, Vector2 b)
        {
            return Cross(ref a, ref b);
        }

        /// Perform the cross product on two vectors.
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public static Vector2 Cross(Vector2 a, float s)
        {
            return new Vector2(s * a.y, -s * a.x);
        }

        public static Vector2 Cross(float s, Vector2 a)
        {
            return new Vector2(-s * a.y, s * a.x);
        }

        public static Vector2 Abs(Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }

        public static Vector2 Mul(ref Mat22 A, Vector2 v)
        {
            return Mul(ref A, ref v);
        }

        public static Vector2 Mul(ref Mat22 A, ref Vector2 v)
        {
            return new Vector2(A.ex.x * v.x + A.ey.x * v.y, A.ex.y * v.x + A.ey.y * v.y);
        }

        public static Vector2 Mul(ref VTransform T, Vector2 v)
        {
            return Mul(ref T, ref v);
        }

        public static Vector2 Mul(ref VTransform T, ref Vector2 v)
        {
            var x = T.q.c * v.x - T.q.s * v.y + T.p.x;
            var y = T.q.s * v.x + T.q.c * v.y + T.p.y;

            return new Vector2(x, y);
        }

        public static Vector2 MulT(ref Mat22 A, Vector2 v)
        {
            return MulT(ref A, ref v);
        }

        public static Vector2 MulT(ref Mat22 A, ref Vector2 v)
        {
            return new Vector2(v.x * A.ex.x + v.y * A.ex.y, v.x * A.ey.x + v.y * A.ey.y);
        }

        public static Vector2 MulT(ref VTransform T, Vector2 v)
        {
            return MulT(ref T, ref v);
        }

        public static Vector2 MulT(ref VTransform T, ref Vector2 v)
        {
            var px = v.x - T.p.x;
            var py = v.y - T.p.y;
            var x = T.q.c * px + T.q.s * py;
            var y = -T.q.s * px + T.q.c * py;

            return new Vector2(x, y);
        }

        // A^T * B
        public static void MulT(ref Mat22 A, ref Mat22 B, out Mat22 C)
        {
            C = new Mat22();
            C.ex.x = A.ex.x * B.ex.x + A.ex.y * B.ex.y;
            C.ex.y = A.ey.x * B.ex.x + A.ey.y * B.ex.y;
            C.ey.x = A.ex.x * B.ey.x + A.ex.y * B.ey.y;
            C.ey.y = A.ey.x * B.ey.x + A.ey.y * B.ey.y;
        }

        /// Multiply a matrix times a vector.
        public static Vector3 Mul(Mat33 A, Vector3 v)
        {
            return v.x * A.ex + v.y * A.ey + v.z * A.ez;
        }

        // v2 = A.q.Rot(B.q.Rot(v1) + B.p) + A.p
        //    = (A.q * B.q).Rot(v1) + A.q.Rot(B.p) + A.p
        public static VTransform Mul(VTransform A, VTransform B)
        {
            var C = new VTransform();
            C.q = Mul(A.q, B.q);
            C.p = Mul(A.q, B.p) + A.p;
            return C;
        }

        // v2 = A.q' * (B.q * v1 + B.p - A.p)
        //    = A.q' * B.q * v1 + A.q' * (B.p - A.p)
        public static void MulT(ref VTransform A, ref VTransform B, out VTransform C)
        {
            C = new VTransform();
            C.q = MulT(A.q, B.q);
            C.p = MulT(A.q, B.p - A.p);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        /// Multiply a matrix times a vector.
        public static Vector2 Mul22(Mat33 A, Vector2 v)
        {
            return new Vector2(A.ex.x * v.x + A.ey.x * v.y, A.ex.y * v.x + A.ey.y * v.y);
        }

        /// Multiply two rotations: q * r
        public static Rot Mul(Rot q, Rot r)
        {
            // [qc -qs] * [rc -rs] = [qc*rc-qs*rs -qc*rs-qs*rc]
            // [qs  qc]   [rs  rc]   [qs*rc+qc*rs -qs*rs+qc*rc]
            // s = qs * rc + qc * rs
            // c = qc * rc - qs * rs
            Rot qr;
            qr.s = q.s * r.c + q.c * r.s;
            qr.c = q.c * r.c - q.s * r.s;
            return qr;
        }

        public static Vector2 MulT(VTransform T, Vector2 v)
        {
            var px = v.x - T.p.x;
            var py = v.y - T.p.y;
            var x = T.q.c * px + T.q.s * py;
            var y = -T.q.s * px + T.q.c * py;

            return new Vector2(x, y);
        }

        /// Transpose multiply two rotations: qT * r
        public static Rot MulT(Rot q, Rot r)
        {
            // [ qc qs] * [rc -rs] = [qc*rc+qs*rs -qc*rs+qs*rc]
            // [-qs qc]   [rs  rc]   [-qs*rc+qc*rs qs*rs+qc*rc]
            // s = qc * rs - qs * rc
            // c = qc * rc + qs * rs
            Rot qr;
            qr.s = q.c * r.s - q.s * r.c;
            qr.c = q.c * r.c + q.s * r.s;
            return qr;
        }

        // v2 = A.q' * (B.q * v1 + B.p - A.p)
        //    = A.q' * B.q * v1 + A.q' * (B.p - A.p)
        public static VTransform MulT(VTransform A, VTransform B)
        {
            var C = new VTransform();
            C.q = MulT(A.q, B.q);
            C.p = MulT(A.q, B.p - A.p);
            return C;
        }

        /// Rotate a vector
        public static Vector2 Mul(Rot q, Vector2 v)
        {
            return new Vector2(q.c * v.x - q.s * v.y, q.s * v.x + q.c * v.y);
        }

        /// Inverse rotate a vector
        public static Vector2 MulT(Rot q, Vector2 v)
        {
            return new Vector2(q.c * v.x + q.s * v.y, -q.s * v.x + q.c * v.y);
        }

        /// Get the skew vector such that dot(skew_vec, other) == cross(vec, other)
        public static Vector2 Skew(Vector2 input)
        {
            return new Vector2(-input.y, input.x);
        }

        /// <summary>
        /// This function is used to ensure that a floating point number is
        /// not a NaN or infinity.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>
        /// <c>true</c> if the specified x is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(float x)
        {
            if (float.IsNaN(x))
                // NaN.
                return false;

            return !float.IsInfinity(x);
        }

        public static bool IsValid(this Vector2 x)
        {
            return IsValid(x.x) && IsValid(x.y);
        }

        /// <summary>
        /// This is a approximate yet fast inverse square-root.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public static float InvSqrt(float x)
        {
            var convert = new FloatConverter();
            convert.x = x;
            var xhalf = 0.5f * x;
            convert.i = 0x5f3759df - (convert.i >> 1);
            x = convert.x;
            x = x * (1.5f - xhalf * x * x);
            return x;
        }

        public static int Clamp(int a, int low, int high)
        {
            return Mathf.Max(low, Mathf.Min(a, high));
        }

        public static float Clamp(float a, float low, float high)
        {
            return Mathf.Max(low, Mathf.Min(a, high));
        }


        public static Vector2 Clamp(Vector2 a, Vector2 low, Vector2 high)
        {
            return Vector2.Max(low, Vector2.Min(a, high));
        }

        public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
        {
            c = a.x * b.y - a.y * b.x;
        }

        /// Perform the dot product on two vectors.
        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Returns a positive number if c is to the left of the line going from a to b.
        /// </summary>
        /// <returns>
        /// Positive number if point is left, negative if point is right,
        /// and 0 if points are collinear.
        /// </returns>
        public static float Area(Vector2 a, Vector2 b, Vector2 c)
        {
            return Area(ref a, ref b, ref c);
        }

        /// <summary>
        /// Returns a positive number if c is to the left of the line going from a to b.
        /// </summary>
        /// <returns>
        /// Positive number if point is left, negative if point is right,
        /// and 0 if points are collinear.
        /// </returns>
        public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
        }

        /// <summary>
        /// Determines if three vertices are collinear (ie. on a straight line)
        /// </summary>
        /// <param name="a">First vertex</param>
        /// <param name="b">Second vertex</param>
        /// <param name="c">Third vertex</param>
        /// <param name="tolerance">The tolerance</param>
        /// <returns></returns>
        public static bool IsCollinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance = 0)
        {
            return FloatInRange(Area(ref a, ref b, ref c), -tolerance, tolerance);
        }

        public static void Cross(float s, ref Vector2 a, out Vector2 b)
        {
            b = new Vector2(-s * a.y, s * a.x);
        }

        public static bool FloatEquals(float value1, float value2)
        {
            return Mathf.Abs(value1 - value2) <= Settings.Epsilon;
        }

        /// <summary>
        /// Checks if a floating point Value is equal to another,
        /// within a certain tolerance.
        /// </summary>
        /// <param name="value1">The first floating point Value.</param>
        /// <param name="value2">The second floating point Value.</param>
        /// <param name="delta">The floating point tolerance.</param>
        /// <returns>True if the values are "equal", false otherwise.</returns>
        public static bool FloatEquals(float value1, float value2, float delta)
        {
            return FloatInRange(value1, value2 - delta, value2 + delta);
        }

        /// <summary>
        /// Checks if a floating point Value is within a specified
        /// range of values (inclusive).
        /// </summary>
        /// <param name="value">The Value to check.</param>
        /// <param name="min">The minimum Value.</param>
        /// <param name="max">The maximum Value.</param>
        /// <returns>
        /// True if the Value is within the range specified,
        /// false otherwise.
        /// </returns>
        public static bool FloatInRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static Vector2 Mul(ref Rot rot, Vector2 axis)
        {
            return Mul(rot, axis);
        }

        public static Vector2 MulT(ref Rot rot, Vector2 axis)
        {
            return MulT(rot, axis);
        }

        #region Nested type: FloatConverter

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatConverter
        {
            [FieldOffset(0)] public float x;

            [FieldOffset(0)] public int i;
        }

        #endregion
    }
}