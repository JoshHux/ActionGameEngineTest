using ActionGameEngine.Enum;
using ActionGameEngine.Data.Helpers;
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
        public int damage;
        public int chipDamage;
        //higher priority means that it'll override hitboxes from the same entity with lower priority
        public int priority;

        public int attackLv;

        //when you hit a move, what to apply to the hit entity
        public StateCondition hitCause;
        //when you hit a move, what to apply to the counter-hit entity
        public StateCondition counterHitCause;
        //what we can cancel into when we land a hit
        public CancelConditions hitCancel;
        //what we can cancel into when we land a counter-hit
        public CancelConditions counterHitCancel;

        public HitType type;

        private AttackLevelVal GetAttackLevelVal()
        {
            return AttackLevel.GetVal(attackLv);
        }

        public CancelConditions GetCancelConditions(HitIndicator indicator)
        {
            bool isCounterHit = EnumHelper.HasEnum((int)indicator, (int)HitIndicator.COUNTER_HIT);
            if (isCounterHit)
            {
                return counterHitCancel;
            }

            return hitCancel;
        }

        //should only every be called and referenced by Vulnerable Object
        public HitboxDataHolder GetHolder(HitIndicator indicator)
        {
            AttackLevelVal val = GetAttackLevelVal();
            HitboxDataHolder ret = new HitboxDataHolder();
            bool isGrounded = EnumHelper.HasEnum((int)indicator, (int)HitIndicator.GROUNDED);
            bool isCrouching = EnumHelper.HasEnum((int)indicator, (int)HitIndicator.CROUCHING);
            bool isCounterHit = EnumHelper.HasEnum((int)indicator, (int)HitIndicator.COUNTER_HIT);
            bool isBlocking = EnumHelper.HasEnum((int)indicator, (int)HitIndicator.BLOCKED);


            ret.hitStopEnemy = val.GetHitstopEnemy(isCounterHit);
            ret.hitStopSelf = val.GetHitstopSelf(isCounterHit);
            ret.type = this.type;

            if (isBlocking)
            {
                ret.stun = val.GetBlockstun(isGrounded, isCrouching);
            }
            else
            {
                ret.stun = val.GetHitstun(isGrounded, isCrouching, isCounterHit);
            }

            if (isCounterHit)
            {
                ret.hitCause = this.hitCause;
            }
            else
            {
                ret.hitCause = this.counterHitCause;
            }

            return ret;
        }

    }
}