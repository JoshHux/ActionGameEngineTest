
namespace FixMath.NET
{
    [System.Serializable]
    public struct FVector3
    {

        public Fix64 x;
        public Fix64 y;
        public Fix64 z;

        public FVector3(Fix64 x, Fix64 y, Fix64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}