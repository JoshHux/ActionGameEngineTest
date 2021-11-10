using System;
using FixMath.NET;

namespace VelcroPhysics.Utilities
{
    public static class VectorUtils
    {
        public static FVector2 VTransform(this FVector2 position, Matrix4x4 matrix)
        {
            VTransform(ref position, ref matrix, out position);
            return position;
        }

        public static void VTransform(this ref FVector2 position, ref Matrix4x4 matrix, out FVector2 result)
        {
            result = new FVector2(position.x * matrix.m00 + position.y * matrix.m10 + matrix.m30,
                position.x * matrix.m01 + position.y * matrix.m11 + matrix.m31);
        }

        public static void VTransform(this FVector2[] sourceArray, ref Matrix4x4 matrix, FVector2[] destinationArray)
        {
            throw new NotImplementedException();
        }

        public static void VTransform(this FVector2[] sourceArray, int sourceIndex, ref Matrix4x4 matrix,
            FVector2[] destinationArray, int destinationIndex, int length)
        {
            throw new NotImplementedException();
        }

        public static FVector2 CatmullRom(FVector2 value1, FVector2 value2, FVector2 value3, FVector2 value4, Fix64 amount)
        {
            return new FVector2(
                CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

        public static void CatmullRom(ref FVector2 value1, ref FVector2 value2, ref FVector2 value3, ref FVector2 value4,
            Fix64 amount, out FVector2 result)
        {
            result = new FVector2(
                CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

        public static Fix64 CatmullRom(Fix64 value1, Fix64 value2, Fix64 value3, Fix64 value4, Fix64 amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using Fix64s not to lose precission
            var amountSquared = amount * amount;
            var amountCubed = amountSquared * amount;

            return (Fix64)(0.5 * (2.0 * value2 + (value3 - value1) * amount
                                                + (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared
                                                + (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }
    }
}