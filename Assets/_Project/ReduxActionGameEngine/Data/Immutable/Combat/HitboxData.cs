using ActionGameEngine.Enum;
using BEPUutilities;
using FixMath.NET;
namespace ActionGameEngine.Data
{
    [System.Serializable]
    public struct HitboxData
    {

        public BepuVector3 localPos;
        public BepuVector3 localRot;
        public BepuVector3 localDim;
        public Fix64 launchAngle;
        public Fix64 launchForce;
        public int duration;
        public int attackLv;
        public int damage;
        public int chipDamage;
        //higher priority means that it'll override hitboxes from the same entity with lower priority
        public int priority;

        //when you hit a move, what to apply to the hit entity
        public StateCondition hitCause;
        public CancelConditions onHitCancel;

        public HitType type;

    }
}