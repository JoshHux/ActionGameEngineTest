using ActionGameEngine.Enum;
using ActionGameEngine.Data.Helpers;
using FixMath.NET;
namespace ActionGameEngine.Data
{
    [System.Serializable]
    public struct HitboxData
    {

        public FVector2 localPos;
        public FVector2 localRot;
        public FVector2 localDim;
        public FVector2 launchDir;
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
            bool isCounterHit = EnumHelper.HasEnum((uint)indicator, (uint)HitIndicator.COUNTER_HIT);
            if (isCounterHit)
            {
                return counterHitCancel;
            }

            return hitCancel;
        }

        //should only every be called and referenced by Vulnerable Object
        public HitboxDataHolder GetHolder(HitIndicator indicator)
        {
            //            AttackLevelVal val = GetAttackLevelVal();
            //            HitboxDataHolder ret = new HitboxDataHolder();
            //            bool isGrounded = EnumHelper.HasEnum((uint)indicator, (uint)HitIndicator.GROUNDED);
            //            bool isCrouching = EnumHelper.HasEnum((uint)indicator, (uint)HitIndicator.CROUCHING);
            //            bool isCounterHit = EnumHelper.HasEnum((uint)indicator, (uint)HitIndicator.COUNTER_HIT);
            //            bool isBlocking = EnumHelper.HasEnum((uint)indicator, (uint)HitIndicator.BLOCKED);
            //            
            //
            //            
            //
            //            ret.hitStopEnemy = val.GetHitstopEnemy(isCounterHit);
            //            ret.hitStopSelf = val.GetHitstopSelf(isCounterHit);
            //            ret.type = this.type;
            //
            //            if (isBlocking)
            //            {
            //                ret.stun = val.GetBlockstun(isGrounded, isCrouching);
            //            }
            //            else
            //            {
            //                ret.stun = val.GetHitstun(isGrounded, isCrouching, isCounterHit);
            //            }
            //
            //            if (isCounterHit)
            //            {
            //                ret.hitCause = this.hitCause;
            //            }
            //            else
            //            {
            //                ret.hitCause = this.counterHitCause;
            //            }
            //
            //            return ret;

            //branchless implementation, but I'm not sure if it's performant....

            //AttackLevelVal has all the stun values we want to access
            AttackLevelVal val = GetAttackLevelVal();
            //what we return
            HitboxDataHolder ret = new HitboxDataHolder();
            //1 or 0 section
            //1 if a grounded hit 0 if not
            int isGrounded = EnumHelper.HasEnumInt((uint)indicator, (uint)HitIndicator.GROUNDED);
            //1 if a crouching hit 0 if not
            int isCrouching = EnumHelper.HasEnumInt((uint)indicator, (uint)HitIndicator.CROUCHING);
            //1 if a counter hit 0 if not
            int isCounter = EnumHelper.HasEnumInt((uint)indicator, (uint)HitIndicator.COUNTER_HIT);
            //1 if a blocked hit state 0 if not
            int isBlocked = EnumHelper.HasEnumInt((uint)indicator, (uint)HitIndicator.BLOCKED);
            //flip of grounded int, for checking airborne
            int isAirborne = isGrounded ^ 1;
            //flip of blocked int, for checking unblocked hit
            int isUnblocked = isBlocked ^ 1;

            //hitstop stuff
            //base hitstop 
            int baseHitstopEnemy = val.hitstop;
            int baseHitstopSelf = val.hitstopSelf;
            //hitstop modifier
            int hitstopMod = val.counterHitstop * isCounter;
            //total hitstop values
            int hitstopEnemy = baseHitstopEnemy + hitstopMod;
            int hitstopSelf = baseHitstopSelf + hitstopMod;

            //hitstun stuff
            //base hitstun value
            int baseHitstun = val.standingStun;
            //air untech
            int baseAirHitstun = val.airUntechTime;
            //hitstun modifiers
            int hitstunModCH = val.counterHitstun * isCounter;
            int hitstunModCrouch = val.crouchingStun * isCrouching;
            int totalGroundedHitstunMod = hitstunModCH + hitstunModCrouch;
            //air untech mod (itself for now)
            int airHitstunModCH = val.airUntechTime * isCounter;
            //not really needed, but here in case we add any other air hitstun modifiers
            int totalAirHitstunMod = airHitstunModCH;
            //total hitstun values
            int groundedHitstun = baseHitstun + totalGroundedHitstunMod;
            int airHitstun = baseAirHitstun + totalAirHitstunMod;
            int realGroundedHitstun = groundedHitstun * isGrounded;
            int realAirHitstun = airHitstun * isAirborne;
            //total hitstun
            int hitstun = realGroundedHitstun + realAirHitstun;

            //blockstun stuff
            //base blockstun value
            int baseBlockstun = val.groundBlockstun;
            //air blockstun
            int baseAirBlockstun = val.airBlockstun;
            //blockstun modifiers
            int blockstunModCrouch = val.crouchingStun * isCrouching;
            //total blockstun values
            int groundedBlockstun = baseBlockstun + blockstunModCrouch;
            int realGroundedBlockstun = groundedBlockstun * isGrounded;
            int realAirBlockstun = baseAirBlockstun * isAirborne;
            //total blockstun
            int blockstun = realGroundedBlockstun + realAirBlockstun;

            //enum stuff
            //base enums
            StateCondition baseHitCause = this.hitCause;
            //enum mods
            StateCondition hitCauseMod = (StateCondition)((int)counterHitCause * isCounter);
            //total enum changes
            StateCondition hitCauseRet = baseHitCause | hitCauseMod;

            //real stun values
            int realHitstun = hitstun * isUnblocked;
            int realBlockstun = blockstun * isBlocked;
            //total stun
            int stun = realHitstun + realBlockstun;

            //assign the calculated values
            ret.stun = stun;
            ret.hitStopEnemy = hitstopEnemy;
            ret.hitStopSelf = hitstopSelf;
            ret.hitCause = hitCauseRet;
            ret.type = this.type;

            return ret;
        }

    }
}