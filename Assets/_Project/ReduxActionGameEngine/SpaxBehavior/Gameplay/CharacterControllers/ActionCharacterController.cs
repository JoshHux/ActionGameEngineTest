using UnityEngine;
using ActionGameEngine.Data;
using ActionGameEngine.Data.Helpers;
using ActionGameEngine.Gameplay;
using ActionGameEngine.Enum;
using BEPUUnity;
using BEPUutilities;
using FixMath.NET;

namespace ActionGameEngine
{
    public class ActionCharacterController : ControllableObject
    {
        protected ShapeBase lockonTarget;

        public override HitIndicator GetHit(int attackerID, HitboxData boxData)
        {
            return OnGetHit(attackerID, boxData);
        }

        public override int ConnectedHit(HitboxData boxData)
        {
            return -1;
        }

        public HitIndicator OnGetHit(int attackerID, HitboxData boxData)
        {
            HitType type = boxData.type;
            bool onGround = EnumHelper.HasEnum((int)status.GetStateConditions(), (int)StateCondition.GROUNDED);
            bool crouching = EnumHelper.HasEnum((int)status.GetStateConditions(), (int)StateCondition.CROUCHING);
            HitIndicator indicator = 0;
            //check to see if invuln matches
            //checks to see if hit is strike box
            if (EnumHelper.HasEnum((int)type, (int)HitType.STRIKE))
            {
                //checks to see if unblockable or not
                if (!EnumHelper.HasEnum((int)type, (int)HitType.UNBLOCKABLE))
                {
                    //blockable hit

                    //replace with block operations like checking if they're blocking the right way
                    bool isBlocking = true;

                    if (isBlocking)
                    {
                        //set chip, blockstun, blockstop, etc.

                        AddPotentialHitbox(attackerID, boxData, HitIndicator.BLOCKED);
                        return indicator;
                    }
                    else
                    {
                        if (EnumHelper.HasEnum((int)type, (int)HitType.GRAB))
                        {
                            //this is where you would also put your operations for hitgrabs
                            indicator |= HitIndicator.GRABBED;
                        }
                    }
                }
                //set damage, hitstun, hitstop, etc.
                AddPotentialHitbox(attackerID, boxData, indicator);

                return indicator;

            }
            else if (EnumHelper.HasEnum((int)type, (int)HitType.GRAB))
            {
                //grab operations, like setting parent and the such
                indicator |= HitIndicator.GRABBED;
                AddPotentialHitbox(attackerID, boxData, indicator);
                return indicator;
            }
            else
            {
                Debug.LogError("Invalid HitType, must have GRAB or STRIKE tag");
            }

            return HitIndicator.WHIFFED;
        }

        protected override void ProcessTransitionEvents(int transitionEvents)
        {
            base.ProcessTransitionEvents(transitionEvents);

            //if we need to face target when happens
            if (EnumHelper.HasEnum(transitionEvents, (int)TransitionEvent.FACE_ENEMY)) { this.FaceTargetY(lockonTarget); }

        }

        //TODO: tell enemy to block when this is called
        protected override void FlagBlockToOthers() { }

        //being hit, process data
        protected override void ProcessHitbox(HitboxData boxData)
        {
            HitboxDataHolder hold = boxData.GetHolder(hitIndicator);
            //knockback force
            BepuVector3 force = boxData.launchForce * new BepuVector3(0, Fix64.Sin(boxData.launchAngle), Fix64.Cos(boxData.launchAngle) * -1).Normalized();

            DamageHealth(boxData.damage);
            ApplyHitstop(hold.hitStopEnemy);

        }



    }
}