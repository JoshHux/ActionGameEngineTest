using FixMath.NET;
using System;

namespace BEPUutilities
{
    /// <summary>
    /// Provides XNA-like BepuQuaternion support.
    /// </summary>
    [Serializable]
    public struct BepuQuaternion : IEquatable<BepuQuaternion>
    {
        /// <summary>
        /// X component of the BepuQuaternion.
        /// </summary>
        public Fix64 X;

        /// <summary>
        /// Y component of the BepuQuaternion.
        /// </summary>
        public Fix64 Y;

        /// <summary>
        /// Z component of the BepuQuaternion.
        /// </summary>
        public Fix64 Z;

        /// <summary>
        /// W component of the BepuQuaternion.
        /// </summary>
        public Fix64 W;

        /// <summary>
        /// Constructs a new BepuQuaternion.
        /// </summary>
        /// <param name="x">X component of the BepuQuaternion.</param>
        /// <param name="y">Y component of the BepuQuaternion.</param>
        /// <param name="z">Z component of the BepuQuaternion.</param>
        /// <param name="w">W component of the BepuQuaternion.</param>
        public BepuQuaternion(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Adds two BepuQuaternions together.
        /// </summary>
        /// <param name="a">First BepuQuaternion to add.</param>
        /// <param name="b">Second BepuQuaternion to add.</param>
        /// <param name="result">Sum of the addition.</param>
        public static void Add(ref BepuQuaternion a, ref BepuQuaternion b, out BepuQuaternion result)
        {
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            result.W = a.W + b.W;
        }

        /// <summary>
        /// Multiplies two BepuQuaternions.
        /// </summary>
        /// <param name="a">First BepuQuaternion to multiply.</param>
        /// <param name="b">Second BepuQuaternion to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Multiply(ref BepuQuaternion a, ref BepuQuaternion b, out BepuQuaternion result)
        {
            Fix64 x = a.X;
            Fix64 y = a.Y;
            Fix64 z = a.Z;
            Fix64 w = a.W;
            Fix64 bX = b.X;
            Fix64 bY = b.Y;
            Fix64 bZ = b.Z;
            Fix64 bW = b.W;
            result.X = x * bW + bX * w + y * bZ - z * bY;
            result.Y = y * bW + bY * w + z * bX - x * bZ;
            result.Z = z * bW + bZ * w + x * bY - y * bX;
            result.W = w * bW - x * bX - y * bY - z * bZ;
        }

        /// <summary>
        /// Scales a BepuQuaternion.
        /// </summary>
        /// <param name="q">BepuQuaternion to multiply.</param>
        /// <param name="scale">Amount to multiply each component of the BepuQuaternion by.</param>
        /// <param name="result">Scaled BepuQuaternion.</param>
        public static void Multiply(ref BepuQuaternion q, Fix64 scale, out BepuQuaternion result)
        {
            result.X = q.X * scale;
            result.Y = q.Y * scale;
            result.Z = q.Z * scale;
            result.W = q.W * scale;
        }

        /// <summary>
        /// Multiplies two BepuQuaternions together in opposite order.
        /// </summary>
        /// <param name="a">First BepuQuaternion to multiply.</param>
        /// <param name="b">Second BepuQuaternion to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Concatenate(ref BepuQuaternion a, ref BepuQuaternion b, out BepuQuaternion result)
        {
            Fix64 aX = a.X;
            Fix64 aY = a.Y;
            Fix64 aZ = a.Z;
            Fix64 aW = a.W;
            Fix64 bX = b.X;
            Fix64 bY = b.Y;
            Fix64 bZ = b.Z;
            Fix64 bW = b.W;

            result.X = aW * bX + aX * bW + aZ * bY - aY * bZ;
            result.Y = aW * bY + aY * bW + aX * bZ - aZ * bX;
            result.Z = aW * bZ + aZ * bW + aY * bX - aX * bY;
            result.W = aW * bW - aX * bX - aY * bY - aZ * bZ;


        }

        /// <summary>
        /// Multiplies two BepuQuaternions together in opposite order.
        /// </summary>
        /// <param name="a">First BepuQuaternion to multiply.</param>
        /// <param name="b">Second BepuQuaternion to multiply.</param>
        /// <returns>Product of the multiplication.</returns>
        public static BepuQuaternion Concatenate(BepuQuaternion a, BepuQuaternion b)
        {
            BepuQuaternion result;
            Concatenate(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// BepuQuaternion representing the identity transform.
        /// </summary>
        public static BepuQuaternion Identity
        {
            get
            {
                return new BepuQuaternion(F64.C0, F64.C0, F64.C0, F64.C1);
            }
        }




        /// <summary>
        /// Constructs a BepuQuaternion from a rotation matrix.
        /// </summary>
        /// <param name="r">Rotation matrix to create the BepuQuaternion from.</param>
        /// <param name="q">BepuQuaternion based on the rotation matrix.</param>
        public static void CreateFromRotationMatrix(ref Matrix3x3 r, out BepuQuaternion q)
        {
            Fix64 trace = r.M11 + r.M22 + r.M33;
#if !WINDOWS
            q = new BepuQuaternion();
#endif
            if (trace >= F64.C0)
            {
                var S = Fix64.Sqrt(trace + F64.C1) * F64.C2; // S=4*qw 
                var inverseS = F64.C1 / S;
                q.W = F64.C0p25 * S;
                q.X = (r.M23 - r.M32) * inverseS;
                q.Y = (r.M31 - r.M13) * inverseS;
                q.Z = (r.M12 - r.M21) * inverseS;
            }
            else if ((r.M11 > r.M22) & (r.M11 > r.M33))
            {
                var S = Fix64.Sqrt(F64.C1 + r.M11 - r.M22 - r.M33) * F64.C2; // S=4*qx 
                var inverseS = F64.C1 / S;
                q.W = (r.M23 - r.M32) * inverseS;
                q.X = F64.C0p25 * S;
                q.Y = (r.M21 + r.M12) * inverseS;
                q.Z = (r.M31 + r.M13) * inverseS;
            }
            else if (r.M22 > r.M33)
            {
                var S = Fix64.Sqrt(F64.C1 + r.M22 - r.M11 - r.M33) * F64.C2; // S=4*qy
                var inverseS = F64.C1 / S;
                q.W = (r.M31 - r.M13) * inverseS;
                q.X = (r.M21 + r.M12) * inverseS;
                q.Y = F64.C0p25 * S;
                q.Z = (r.M32 + r.M23) * inverseS;
            }
            else
            {
                var S = Fix64.Sqrt(F64.C1 + r.M33 - r.M11 - r.M22) * F64.C2; // S=4*qz
                var inverseS = F64.C1 / S;
                q.W = (r.M12 - r.M21) * inverseS;
                q.X = (r.M31 + r.M13) * inverseS;
                q.Y = (r.M32 + r.M23) * inverseS;
                q.Z = F64.C0p25 * S;
            }
        }

        /// <summary>
        /// Creates a BepuQuaternion from a rotation matrix.
        /// </summary>
        /// <param name="r">Rotation matrix used to create a new BepuQuaternion.</param>
        /// <returns>BepuQuaternion representing the same rotation as the matrix.</returns>
        public static BepuQuaternion CreateFromRotationMatrix(Matrix3x3 r)
        {
            BepuQuaternion toReturn;
            CreateFromRotationMatrix(ref r, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Constructs a BepuQuaternion from a rotation matrix.
        /// </summary>
        /// <param name="r">Rotation matrix to create the BepuQuaternion from.</param>
        /// <param name="q">BepuQuaternion based on the rotation matrix.</param>
        public static void CreateFromRotationMatrix(ref Matrix r, out BepuQuaternion q)
        {
            Matrix3x3 downsizedMatrix;
            Matrix3x3.CreateFromMatrix(ref r, out downsizedMatrix);
            CreateFromRotationMatrix(ref downsizedMatrix, out q);
        }

        /// <summary>
        /// Creates a BepuQuaternion from a rotation matrix.
        /// </summary>
        /// <param name="r">Rotation matrix used to create a new BepuQuaternion.</param>
        /// <returns>BepuQuaternion representing the same rotation as the matrix.</returns>
        public static BepuQuaternion CreateFromRotationMatrix(Matrix r)
        {
            BepuQuaternion toReturn;
            CreateFromRotationMatrix(ref r, out toReturn);
            return toReturn;
        }


        /// <summary>
        /// Ensures the BepuQuaternion has unit length.
        /// </summary>
        /// <param name="BepuQuaternion">BepuQuaternion to normalize.</param>
        /// <returns>Normalized BepuQuaternion.</returns>
        public static BepuQuaternion Normalize(BepuQuaternion BepuQuaternion)
        {
            BepuQuaternion toReturn;
            Normalize(ref BepuQuaternion, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Ensures the BepuQuaternion has unit length.
        /// </summary>
        /// <param name="BepuQuaternion">BepuQuaternion to normalize.</param>
        /// <param name="toReturn">Normalized BepuQuaternion.</param>
        public static void Normalize(ref BepuQuaternion BepuQuaternion, out BepuQuaternion toReturn)
        {
            Fix64 inverse = F64.C1 / Fix64.Sqrt(BepuQuaternion.X * BepuQuaternion.X + BepuQuaternion.Y * BepuQuaternion.Y + BepuQuaternion.Z * BepuQuaternion.Z + BepuQuaternion.W * BepuQuaternion.W);
            toReturn.X = BepuQuaternion.X * inverse;
            toReturn.Y = BepuQuaternion.Y * inverse;
            toReturn.Z = BepuQuaternion.Z * inverse;
            toReturn.W = BepuQuaternion.W * inverse;
        }

        /// <summary>
        /// Scales the BepuQuaternion such that it has unit length.
        /// </summary>
        public void Normalize()
        {
            Fix64 inverse = F64.C1 / Fix64.Sqrt(X * X + Y * Y + Z * Z + W * W);
            X *= inverse;
            Y *= inverse;
            Z *= inverse;
            W *= inverse;
        }

        /// <summary>
        /// Computes the squared length of the BepuQuaternion.
        /// </summary>
        /// <returns>Squared length of the BepuQuaternion.</returns>
        public Fix64 LengthSquared()
        {
            return X * X + Y * Y + Z * Z + W * W;
        }

        /// <summary>
        /// Computes the length of the BepuQuaternion.
        /// </summary>
        /// <returns>Length of the BepuQuaternion.</returns>
        public Fix64 Length()
        {
            return Fix64.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }


        /// <summary>
        /// Blends two BepuQuaternions together to get an intermediate state.
        /// </summary>
        /// <param name="start">Starting point of the interpolation.</param>
        /// <param name="end">Ending point of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end point to use.</param>
        /// <param name="result">Interpolated intermediate BepuQuaternion.</param>
        public static void Slerp(ref BepuQuaternion start, ref BepuQuaternion end, Fix64 interpolationAmount, out BepuQuaternion result)
        {
			Fix64 cosHalfTheta = start.W * end.W + start.X * end.X + start.Y * end.Y + start.Z * end.Z;
            if (cosHalfTheta < F64.C0)
            {
                //Negating a BepuQuaternion results in the same orientation, 
                //but we need cosHalfTheta to be positive to get the shortest path.
                end.X = -end.X;
                end.Y = -end.Y;
                end.Z = -end.Z;
                end.W = -end.W;
                cosHalfTheta = -cosHalfTheta;
            }
            // If the orientations are similar enough, then just pick one of the inputs.
            if (cosHalfTheta > F64.C1m1em12)
            {
                result.W = start.W;
                result.X = start.X;
                result.Y = start.Y;
                result.Z = start.Z;
                return;
            }
            // Calculate temporary values.
            Fix64 halfTheta = Fix64.Acos(cosHalfTheta);
			Fix64 sinHalfTheta = Fix64.Sqrt(F64.C1 - cosHalfTheta * cosHalfTheta);

			Fix64 aFraction = Fix64.Sin((F64.C1 - interpolationAmount) * halfTheta) / sinHalfTheta;
			Fix64 bFraction = Fix64.Sin(interpolationAmount * halfTheta) / sinHalfTheta;

            //Blend the two BepuQuaternions to get the result!
            result.X = (Fix64)(start.X * aFraction + end.X * bFraction);
            result.Y = (Fix64)(start.Y * aFraction + end.Y * bFraction);
            result.Z = (Fix64)(start.Z * aFraction + end.Z * bFraction);
            result.W = (Fix64)(start.W * aFraction + end.W * bFraction);




        }

        /// <summary>
        /// Blends two BepuQuaternions together to get an intermediate state.
        /// </summary>
        /// <param name="start">Starting point of the interpolation.</param>
        /// <param name="end">Ending point of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end point to use.</param>
        /// <returns>Interpolated intermediate BepuQuaternion.</returns>
        public static BepuQuaternion Slerp(BepuQuaternion start, BepuQuaternion end, Fix64 interpolationAmount)
        {
            BepuQuaternion toReturn;
            Slerp(ref start, ref end, interpolationAmount, out toReturn);
            return toReturn;
        }


        /// <summary>
        /// Computes the conjugate of the BepuQuaternion.
        /// </summary>
        /// <param name="BepuQuaternion">BepuQuaternion to conjugate.</param>
        /// <param name="result">Conjugated BepuQuaternion.</param>
        public static void Conjugate(ref BepuQuaternion BepuQuaternion, out BepuQuaternion result)
        {
            result.X = -BepuQuaternion.X;
            result.Y = -BepuQuaternion.Y;
            result.Z = -BepuQuaternion.Z;
            result.W = BepuQuaternion.W;
        }

        /// <summary>
        /// Computes the conjugate of the BepuQuaternion.
        /// </summary>
        /// <param name="BepuQuaternion">BepuQuaternion to conjugate.</param>
        /// <returns>Conjugated BepuQuaternion.</returns>
        public static BepuQuaternion Conjugate(BepuQuaternion BepuQuaternion)
        {
            BepuQuaternion toReturn;
            Conjugate(ref BepuQuaternion, out toReturn);
            return toReturn;
        }



        /// <summary>
        /// Computes the inverse of the BepuQuaternion.
        /// </summary>
        /// <param name="BepuQuaternion">BepuQuaternion to invert.</param>
        /// <param name="result">Result of the inversion.</param>
        public static void Inverse(ref BepuQuaternion BepuQuaternion, out BepuQuaternion result)
        {
            Fix64 inverseSquaredNorm = BepuQuaternion.X * BepuQuaternion.X + BepuQuaternion.Y * BepuQuaternion.Y + BepuQuaternion.Z * BepuQuaternion.Z + BepuQuaternion.W * BepuQuaternion.W;
            result.X = -BepuQuaternion.X * inverseSquaredNorm;
            result.Y = -BepuQuaternion.Y * inverseSquaredNorm;
            result.Z = -BepuQuaternion.Z * inverseSquaredNorm;
            result.W = BepuQuaternion.W * inverseSquaredNorm;
        }

        /// <summary>
        /// Computes the inverse of the BepuQuaternion.
        /// </summary>
        /// <param name="BepuQuaternion">BepuQuaternion to invert.</param>
        /// <returns>Result of the inversion.</returns>
        public static BepuQuaternion Inverse(BepuQuaternion BepuQuaternion)
        {
            BepuQuaternion result;
            Inverse(ref BepuQuaternion, out result);
            return result;

        }

        /// <summary>
        /// Tests components for equality.
        /// </summary>
        /// <param name="a">First BepuQuaternion to test for equivalence.</param>
        /// <param name="b">Second BepuQuaternion to test for equivalence.</param>
        /// <returns>Whether or not the BepuQuaternions' components were equal.</returns>
        public static bool operator ==(BepuQuaternion a, BepuQuaternion b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        }

        /// <summary>
        /// Tests components for inequality.
        /// </summary>
        /// <param name="a">First BepuQuaternion to test for equivalence.</param>
        /// <param name="b">Second BepuQuaternion to test for equivalence.</param>
        /// <returns>Whether the BepuQuaternions' components were not equal.</returns>
        public static bool operator !=(BepuQuaternion a, BepuQuaternion b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
        }

        /// <summary>
        /// Negates the components of a BepuQuaternion.
        /// </summary>
        /// <param name="a">BepuQuaternion to negate.</param>
        /// <param name="b">Negated result.</param>
        public static void Negate(ref BepuQuaternion a, out BepuQuaternion b)
        {
            b.X = -a.X;
            b.Y = -a.Y;
            b.Z = -a.Z;
            b.W = -a.W;
        }      
        
        /// <summary>
        /// Negates the components of a BepuQuaternion.
        /// </summary>
        /// <param name="q">BepuQuaternion to negate.</param>
        /// <returns>Negated result.</returns>
        public static BepuQuaternion Negate(BepuQuaternion q)
        {
            BepuQuaternion result;
            Negate(ref q, out result);
            return result;
        }

        /// <summary>
        /// Negates the components of a BepuQuaternion.
        /// </summary>
        /// <param name="q">BepuQuaternion to negate.</param>
        /// <returns>Negated result.</returns>
        public static BepuQuaternion operator -(BepuQuaternion q)
        {
            BepuQuaternion result;
            Negate(ref q, out result);
            return result;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(BepuQuaternion other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj is BepuQuaternion)
            {
                return Equals((BepuQuaternion)obj);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }

        /// <summary>
        /// Transforms the vector using a BepuQuaternion.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void Transform(ref BepuVector3 v, ref BepuQuaternion rotation, out BepuVector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' BepuQuaternion
            //and perform standard BepuQuaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            Fix64 x2 = rotation.X + rotation.X;
            Fix64 y2 = rotation.Y + rotation.Y;
            Fix64 z2 = rotation.Z + rotation.Z;
            Fix64 xx2 = rotation.X * x2;
            Fix64 xy2 = rotation.X * y2;
            Fix64 xz2 = rotation.X * z2;
            Fix64 yy2 = rotation.Y * y2;
            Fix64 yz2 = rotation.Y * z2;
            Fix64 zz2 = rotation.Z * z2;
            Fix64 wx2 = rotation.W * x2;
            Fix64 wy2 = rotation.W * y2;
            Fix64 wz2 = rotation.W * z2;
            //Defer the component setting since they're used in computation.
            Fix64 transformedX = v.X * (F64.C1 - yy2 - zz2) + v.Y * (xy2 - wz2) + v.Z * (xz2 + wy2);
            Fix64 transformedY = v.X * (xy2 + wz2) + v.Y * (F64.C1 - xx2 - zz2) + v.Z * (yz2 - wx2);
            Fix64 transformedZ = v.X * (xz2 - wy2) + v.Y * (yz2 + wx2) + v.Z * (F64.C1 - xx2 - yy2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }

        /// <summary>
        /// Transforms the vector using a BepuQuaternion.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <returns>Transformed vector.</returns>
        public static BepuVector3 Transform(BepuVector3 v, BepuQuaternion rotation)
        {
            BepuVector3 toReturn;
            Transform(ref v, ref rotation, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Transforms a vector using a BepuQuaternion. Specialized for x,0,0 vectors.
        /// </summary>
        /// <param name="x">X component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformX(Fix64 x, ref BepuQuaternion rotation, out BepuVector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' BepuQuaternion
            //and perform standard BepuQuaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            Fix64 y2 = rotation.Y + rotation.Y;
            Fix64 z2 = rotation.Z + rotation.Z;
            Fix64 xy2 = rotation.X * y2;
            Fix64 xz2 = rotation.X * z2;
            Fix64 yy2 = rotation.Y * y2;
            Fix64 zz2 = rotation.Z * z2;
            Fix64 wy2 = rotation.W * y2;
            Fix64 wz2 = rotation.W * z2;
            //Defer the component setting since they're used in computation.
            Fix64 transformedX = x * (F64.C1 - yy2 - zz2);
            Fix64 transformedY = x * (xy2 + wz2);
            Fix64 transformedZ = x * (xz2 - wy2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }

        /// <summary>
        /// Transforms a vector using a BepuQuaternion. Specialized for 0,y,0 vectors.
        /// </summary>
        /// <param name="y">Y component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformY(Fix64 y, ref BepuQuaternion rotation, out BepuVector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' BepuQuaternion
            //and perform standard BepuQuaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            Fix64 x2 = rotation.X + rotation.X;
            Fix64 y2 = rotation.Y + rotation.Y;
            Fix64 z2 = rotation.Z + rotation.Z;
            Fix64 xx2 = rotation.X * x2;
            Fix64 xy2 = rotation.X * y2;
            Fix64 yz2 = rotation.Y * z2;
            Fix64 zz2 = rotation.Z * z2;
            Fix64 wx2 = rotation.W * x2;
            Fix64 wz2 = rotation.W * z2;
            //Defer the component setting since they're used in computation.
            Fix64 transformedX = y * (xy2 - wz2);
            Fix64 transformedY = y * (F64.C1 - xx2 - zz2);
            Fix64 transformedZ = y * (yz2 + wx2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }

        /// <summary>
        /// Transforms a vector using a BepuQuaternion. Specialized for 0,0,z vectors.
        /// </summary>
        /// <param name="z">Z component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformZ(Fix64 z, ref BepuQuaternion rotation, out BepuVector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' BepuQuaternion
            //and perform standard BepuQuaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            Fix64 x2 = rotation.X + rotation.X;
            Fix64 y2 = rotation.Y + rotation.Y;
            Fix64 z2 = rotation.Z + rotation.Z;
            Fix64 xx2 = rotation.X * x2;
            Fix64 xz2 = rotation.X * z2;
            Fix64 yy2 = rotation.Y * y2;
            Fix64 yz2 = rotation.Y * z2;
            Fix64 wx2 = rotation.W * x2;
            Fix64 wy2 = rotation.W * y2;
            //Defer the component setting since they're used in computation.
            Fix64 transformedX = z * (xz2 + wy2);
            Fix64 transformedY = z * (yz2 - wx2);
            Fix64 transformedZ = z * (F64.C1 - xx2 - yy2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }


        /// <summary>
        /// Multiplies two BepuQuaternions.
        /// </summary>
        /// <param name="a">First BepuQuaternion to multiply.</param>
        /// <param name="b">Second BepuQuaternion to multiply.</param>
        /// <returns>Product of the multiplication.</returns>
        public static BepuQuaternion operator *(BepuQuaternion a, BepuQuaternion b)
        {
            BepuQuaternion toReturn;
            Multiply(ref a, ref b, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Creates a BepuQuaternion from an axis and angle.
        /// </summary>
        /// <param name="axis">Axis of rotation.</param>
        /// <param name="angle">Angle to rotate around the axis.</param>
        /// <returns>BepuQuaternion representing the axis and angle rotation.</returns>
        public static BepuQuaternion CreateFromAxisAngle(BepuVector3 axis, Fix64 angle)
        {
			Fix64 halfAngle = angle * F64.C0p5;
			Fix64 s = Fix64.Sin(halfAngle);
            BepuQuaternion q;
            q.X = axis.X * s;
            q.Y = axis.Y * s;
            q.Z = axis.Z * s;
            q.W = Fix64.Cos(halfAngle);
            return q;
        }

        /// <summary>
        /// Creates a BepuQuaternion from an axis and angle.
        /// </summary>
        /// <param name="axis">Axis of rotation.</param>
        /// <param name="angle">Angle to rotate around the axis.</param>
        /// <param name="q">BepuQuaternion representing the axis and angle rotation.</param>
        public static void CreateFromAxisAngle(ref BepuVector3 axis, Fix64 angle, out BepuQuaternion q)
        {
			Fix64 halfAngle = angle * F64.C0p5;
			Fix64 s = Fix64.Sin(halfAngle);
            q.X = axis.X * s;
            q.Y = axis.Y * s;
            q.Z = axis.Z * s;
            q.W = Fix64.Cos(halfAngle);
        }

        /// <summary>
        /// Constructs a BepuQuaternion from yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw of the rotation.</param>
        /// <param name="pitch">Pitch of the rotation.</param>
        /// <param name="roll">Roll of the rotation.</param>
        /// <returns>BepuQuaternion representing the yaw, pitch, and roll.</returns>
        public static BepuQuaternion CreateFromYawPitchRoll(Fix64 yaw, Fix64 pitch, Fix64 roll)
        {
            BepuQuaternion toReturn;
            CreateFromYawPitchRoll(yaw, pitch, roll, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Constructs a BepuQuaternion from yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw of the rotation.</param>
        /// <param name="pitch">Pitch of the rotation.</param>
        /// <param name="roll">Roll of the rotation.</param>
        /// <param name="q">BepuQuaternion representing the yaw, pitch, and roll.</param>
        public static void CreateFromYawPitchRoll(Fix64 yaw, Fix64 pitch, Fix64 roll, out BepuQuaternion q)
        {
			Fix64 halfRoll = roll * F64.C0p5;
			Fix64 halFix64itch = pitch * F64.C0p5;
			Fix64 halfYaw = yaw * F64.C0p5;

			Fix64 sinRoll = Fix64.Sin(halfRoll);
			Fix64 sinPitch = Fix64.Sin(halFix64itch);
			Fix64 sinYaw = Fix64.Sin(halfYaw);

			Fix64 cosRoll = Fix64.Cos(halfRoll);
			Fix64 cosPitch = Fix64.Cos(halFix64itch);
			Fix64 cosYaw = Fix64.Cos(halfYaw);

			Fix64 cosYawCosPitch = cosYaw * cosPitch;
			Fix64 cosYawSinPitch = cosYaw * sinPitch;
			Fix64 sinYawCosPitch = sinYaw * cosPitch;
			Fix64 sinYawSinPitch = sinYaw * sinPitch;

            q.X = cosYawSinPitch * cosRoll + sinYawCosPitch * sinRoll;
            q.Y = sinYawCosPitch * cosRoll - cosYawSinPitch * sinRoll;
            q.Z = cosYawCosPitch * sinRoll - sinYawSinPitch * cosRoll;
            q.W = cosYawCosPitch * cosRoll + sinYawSinPitch * sinRoll;

        }

        /// <summary>
        /// Computes the angle change represented by a normalized BepuQuaternion.
        /// </summary>
        /// <param name="q">BepuQuaternion to be converted.</param>
        /// <returns>Angle around the axis represented by the BepuQuaternion.</returns>
        public static Fix64 GetAngleFromBepuQuaternion(ref BepuQuaternion q)
        {
            Fix64 qw = Fix64.Abs(q.W);
            if (qw > F64.C1)
                return F64.C0;
            return F64.C2 * Fix64.Acos(qw);
        }

        /// <summary>
        /// Computes the axis angle representation of a normalized BepuQuaternion.
        /// </summary>
        /// <param name="q">BepuQuaternion to be converted.</param>
        /// <param name="axis">Axis represented by the BepuQuaternion.</param>
        /// <param name="angle">Angle around the axis represented by the BepuQuaternion.</param>
        public static void GetAxisAngleFromBepuQuaternion(ref BepuQuaternion q, out BepuVector3 axis, out Fix64 angle)
        {
#if !WINDOWS
            axis = new BepuVector3();
#endif
            Fix64 qw = q.W;
            if (qw > F64.C0)
            {
                axis.X = q.X;
                axis.Y = q.Y;
                axis.Z = q.Z;
            }
            else
            {
                axis.X = -q.X;
                axis.Y = -q.Y;
                axis.Z = -q.Z;
                qw = -qw;
            }

            Fix64 lengthSquared = axis.LengthSquared();
            if (lengthSquared > F64.C1em14)
            {
                BepuVector3.Divide(ref axis, Fix64.Sqrt(lengthSquared), out axis);
                angle = F64.C2 * Fix64.Acos(MathHelper.Clamp(qw, -1, F64.C1));
            }
            else
            {
                axis = Toolbox.UpVector;
                angle = F64.C0;
            }
        }

        /// <summary>
        /// Computes the BepuQuaternion rotation between two normalized vectors.
        /// </summary>
        /// <param name="v1">First unit-length vector.</param>
        /// <param name="v2">Second unit-length vector.</param>
        /// <param name="q">BepuQuaternion representing the rotation from v1 to v2.</param>
        public static void GetBepuQuaternionBetweenNormalizedVectors(ref BepuVector3 v1, ref BepuVector3 v2, out BepuQuaternion q)
        {
            Fix64 dot;
            BepuVector3.Dot(ref v1, ref v2, out dot);
            //For non-normal vectors, the multiplying the axes length squared would be necessary:
            //Fix64 w = dot + (Fix64)Math.Sqrt(v1.LengthSquared() * v2.LengthSquared());
            if (dot < F64.Cm0p9999) //parallel, opposing direction
            {
                //If this occurs, the rotation required is ~180 degrees.
                //The problem is that we could choose any perpendicular axis for the rotation. It's not uniquely defined.
                //The solution is to pick an arbitrary perpendicular axis.
                //Project onto the plane which has the lowest component magnitude.
                //On that 2d plane, perform a 90 degree rotation.
                Fix64 absX = Fix64.Abs(v1.X);
                Fix64 absY = Fix64.Abs(v1.Y);
                Fix64 absZ = Fix64.Abs(v1.Z);
                if (absX < absY && absX < absZ)
                    q = new BepuQuaternion(F64.C0, -v1.Z, v1.Y, F64.C0);
                else if (absY < absZ)
                    q = new BepuQuaternion(-v1.Z, F64.C0, v1.X, F64.C0);
                else
                    q = new BepuQuaternion(-v1.Y, v1.X, F64.C0, F64.C0);
            }
            else
            {
                BepuVector3 axis;
                BepuVector3.Cross(ref v1, ref v2, out axis);
                q = new BepuQuaternion(axis.X, axis.Y, axis.Z, dot + F64.C1);
            }
            q.Normalize();
        }

        //The following two functions are highly similar, but it's a bit of a brain teaser to phrase one in terms of the other.
        //Providing both simplifies things.

        /// <summary>
        /// Computes the rotation from the start orientation to the end orientation such that end = BepuQuaternion.Concatenate(start, relative).
        /// </summary>
        /// <param name="start">Starting orientation.</param>
        /// <param name="end">Ending orientation.</param>
        /// <param name="relative">Relative rotation from the start to the end orientation.</param>
        public static void GetRelativeRotation(ref BepuQuaternion start, ref BepuQuaternion end, out BepuQuaternion relative)
        {
            BepuQuaternion startInverse;
            Conjugate(ref start, out startInverse);
            Concatenate(ref startInverse, ref end, out relative);
        }

        
        /// <summary>
        /// Transforms the rotation into the local space of the target basis such that rotation = BepuQuaternion.Concatenate(localRotation, targetBasis)
        /// </summary>
        /// <param name="rotation">Rotation in the original frame of reference.</param>
        /// <param name="targetBasis">Basis in the original frame of reference to transform the rotation into.</param>
        /// <param name="localRotation">Rotation in the local space of the target basis.</param>
        public static void GetLocalRotation(ref BepuQuaternion rotation, ref BepuQuaternion targetBasis, out BepuQuaternion localRotation)
        {
            BepuQuaternion basisInverse;
            Conjugate(ref targetBasis, out basisInverse);
            Concatenate(ref rotation, ref basisInverse, out localRotation);
        }

        /// <summary>
        /// Gets a string representation of the BepuQuaternion.
        /// </summary>
        /// <returns>String representing the BepuQuaternion.</returns>
        public override string ToString()
        {
            return "{ X: " + X + ", Y: " + Y + ", Z: " + Z + ", W: " + W + "}";
        }
    }
}
