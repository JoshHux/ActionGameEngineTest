using FixMath.NET;
namespace ActionGameEngine.Data
{
    [System.Serializable]
    public struct HurtboxData
    {

        public FVector2 localPos;
        public FVector2 localRot;
        public FVector2 localDim;

        public HurtboxData(FVector2 p, FVector2 r, FVector2 d)
        {
            localPos = p;
            localRot = r;
            localDim = d;
        }

    }


}
