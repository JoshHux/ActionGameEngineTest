

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
    }
}