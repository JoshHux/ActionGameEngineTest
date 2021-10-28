using BEPUutilities;
namespace ActionGameEngine.Data
{
    [System.Serializable]
    public struct HurtboxData
    {

        public BepuVector3 localPos;
        public BepuVector3 localRot;
        public BepuVector3 localDim;

        public HurtboxData(BepuVector3 p, BepuVector3 r, BepuVector3 d)
        {
            localPos = p;
            localRot = r;
            localDim = d;
        }

    }


}
