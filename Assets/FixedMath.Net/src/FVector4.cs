

namespace FixMath.NET
{
    [System.Serializable]
    public struct FVector4
    {

        public readonly Fix64 x;
        public readonly Fix64 y;
        public readonly Fix64 z;
        public readonly Fix64 w;

        public FVector4(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static FVector4 operator +(FVector4 a, FVector4 b)
        {
            return new FVector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }
    }
}