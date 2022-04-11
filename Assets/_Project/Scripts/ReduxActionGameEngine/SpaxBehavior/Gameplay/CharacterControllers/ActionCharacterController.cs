using UnityEngine;
using ActionGameEngine.Data;
using ActionGameEngine.Data.Helpers;
using ActionGameEngine.Data.Helpers.Static;
using ActionGameEngine.Gameplay;
using ActionGameEngine.Enum;
using ActionGameEngine.Interfaces;
using FixMath.NET;
using Spax;

namespace ActionGameEngine
{
    public class ActionCharacterController : ControllableObject, ICollideable
    {
        //protected ShapeBase lockonTarget;
        //[SerializeField] private string path;
        //this is proration that is stored and not yet applied
        //proration will apply on moves that come after it, so we're storing it here for now
        [SerializeField] private string characterName;
        protected override void OnStart()
        {
            SpaxManager.instance.TrackObject(this);
            allignment = SpaxManager.instance.GetTrackingIndexOf(this);
            this.PrepCharacterData();
            base.OnStart();

            //this._pushBackForce = new FVector3();
        }

        public void PrepCharacterData() { this.data = SpaxJSONSaver.LoadCharacterData(characterName); }

        protected override void StateCleanUpdate() { if (status.GetInHitstop()) { return; } }
        protected override void PreUpdate() { if (status.GetInHitstop()) { return; } }
        protected override void SpaxUpdate()
        {
            base.SpaxUpdate();

            //this is a band-aid solution, because there's no exit event for collider, we always set to airborne
            //that way, whe the delegate is called in the physics step, it will either do nothing, or corrent to a grounded status
            status.RemoveTransitionFlags(TransitionFlag.GROUNDED);
            status.AddTransitionFlags(TransitionFlag.AIRBORNE);
        }

        //returns a value describing what happened when we get hit
        //called by attacker hitbox
        public override HitIndicator GetHit(int attackerID, HitboxData boxData, Fix64 proration)
        {
            return OnGetHit(attackerID, boxData, proration);
        }

        //TODO: what to do when we connect hit with enemy
        public override int ConnectedHit(HitboxData boxData, HitIndicator indicator)
        {
            status.AddCancelConditions(boxData.GetCancelConditions(indicator));
            //gain resources from the hitbox
            status.resources += boxData.resources;
            //we connected a hit, increase combo count
            status.comboCount++;
            //check if we got a comboed hit
            int comboed = EnumHelper.HasEnumInt((uint)indicator, (uint)HitIndicator.COMBOED_HIT);

            //the flip version of whether or not we comboed
            //if it wasn't a comboed hit, it must've been a hit to initiate a combo
            int firstHit = comboed ^ 1;

            //update the stored proration, we connected a hit
            //apply the hitbox's proration to stored proration
            this.ApplyProration(boxData.initProration, boxData.forcedProration, firstHit);
            return -1;
        }

        protected override void ProcessStateData(StateCondition curCond)
        {
            base.ProcessStateData(curCond);
            //apply acceleration is given direction, clamps to max fall speed if exceeded
            if (EnumHelper.HasEnum((uint)curCond, (int)StateCondition.CAN_MOVE))
            {
                //testing with 2d, only needs this for now
                Fix64 accel = data.acceleration.x * fromPlayer.X();
                Fix64 maxVel = data.maxVelocity.x * fromPlayer.X();
                calcVel = new FVector2(GameplayHelper.ApplyAcceleration(calcVel.x, accel, maxVel), calcVel.y);
                //Debug.Log(GameplayHelper.ApplyAcceleration(calcVel.x, accel, maxVel));

                //uncomment this section if working in 3d
                /*
                Fix64 accel = data.GetForwardsAccel() * fromPlayer.Y();
                Fix64 maxVel = data.GetMaxForwardsVel() * fromPlayer.Y();
                calcVel.X = GameplayHelper.ApplyAcceleration(calcVel.X, accel, maxVel);

                accel = data.GetSideAccel() * fromPlayer.X();
                maxVel = data.GetMaxSideVel() * fromPlayer.X();
                calcVel.X = GameplayHelper.ApplyAcceleration(calcVel.Z, accel, maxVel);
                */
            }

        }

        public override Fix64 GetProration() { return status.proration; }

        public HitIndicator OnGetHit(int attackerID, HitboxData boxData, Fix64 proration)
        {
            HitType type = boxData.type;
            bool onGround = EnumHelper.HasEnum((uint)status.GetStateConditions(), (int)StateCondition.GROUNDED);
            bool crouching = EnumHelper.HasEnum((uint)status.GetStateConditions(), (int)StateCondition.CROUCHING);
            HitIndicator indicator = 0;
            //check to see if invuln matches
            //checks to see if hit is strike box

            //check behind
            //from top-down
            //ePos=normalized enemy pos
            //cPos=normalized character pos
            //angle=atan(ePos.y*cPos.x-ePos.x*cPos.y, ePos.x*cPos.x-ePos.y*cPos.y)
            //if abs(angle) is larger than pi/2 rad, then the enemy is behind us

            if (EnumHelper.HasEnum((uint)type, (int)HitType.STRIKE))
            {
                //checks to see if unblockable or not
                if (!EnumHelper.HasEnum((uint)type, (int)HitType.UNBLOCKABLE))
                {
                    //blockable hit

                    //replace with block operations like checking if they're blocking the *right* way
                    bool isBlocking = EnumHelper.HasEnum((uint)status.currentState.stateConditions, (uint)StateCondition.GUARD_POINT);
                    var attackDir = (uint)(boxData.type & HitType.STRIKE) >> 1;
                    var blockDir = (uint)(status.currentState.stateConditions & StateCondition.GUARD_POINT) >> 12;
                    bool blocked = isBlocking && ((attackDir & blockDir) == blockDir);

                    if (blocked)
                    {
                        //set chip, blockstun, blockstop, etc.

                        AddPotentialHitbox(attackerID, boxData, HitIndicator.BLOCKED, proration);
                        return indicator;
                    }
                    else
                    {
                        if (EnumHelper.HasEnum((uint)type, (int)HitType.GRAB))
                        {
                            //this is where you would also put your operations for hitgrabs
                            indicator |= HitIndicator.GRABBED;
                        }
                    }
                }
                //clean hit
                //set damage, hitstun, hitstop, etc.
                //if we're in a stunned state when we're hit, then indicate a comboed hit
                if ((status.currentState.stateConditions & StateCondition.STUN_STATE) > 0) { indicator |= HitIndicator.COMBOED_HIT; }

                AddPotentialHitbox(attackerID, boxData, indicator, proration);

                return indicator;

            }
            else if (EnumHelper.HasEnum((uint)type, (int)HitType.GRAB))
            {
                //grab operations, like setting parent and the such
                indicator |= HitIndicator.GRABBED;
                AddPotentialHitbox(attackerID, boxData, indicator, proration);
                return indicator;
            }

            Debug.LogError("Invalid HitType, must have GRAB or STRIKE tag");
            //we got hit, but for some reason, we don't know what to do with it
            return HitIndicator.WHIFFED;
        }

        protected override void ProcessTransitionEvents(TransitionEvent transitionEvents, ResourceData resourceData = new ResourceData())
        {
            base.ProcessTransitionEvents(transitionEvents, resourceData);

            //if we need to face target when happens
            //if (EnumHelper.HasEnum((int)transitionEvents, (int)TransitionEvent.FACE_ENEMY)) { this.FaceTargetY(lockonTarget); }

        }

        protected override void ProcessFrameData(FrameData frame)
        {
            base.ProcessFrameData(frame);

            //we currently don't want the character to correct their direction, so we use this flag to know when to flip left/right
            if (EnumHelper.HasEnum((uint)frame.flags, (int)FrameEventFlag.AUTO_TURN))
            {
                status.facing *= -1;
                helper.facing = status.facing;
            }
        }

        //TODO: tell enemy to block when this is called
        protected override void FlagBlockToOthers() { }

        //being hit, process data
        protected override void ProcessHitbox(HitboxData boxData, int dir, Fix64 proration)
        {
            HitboxDataHolder hold = boxData.GetHolder(hitIndicator);
            //knockback force
            //BepuVector3 force = boxData.launchForce * new BepuVector3(0, Fix64.Sin(boxData.launchAngle), Fix64.Cos(boxData.launchAngle) * -1).Normalized();
            FVector2 force = (new FVector2(boxData.launchDir.x * dir, boxData.launchDir.y).normalized) * boxData.launchForce;
            //Debug.Log("knockback x val :: " + (dir));

            //has to be assigned directly to velocity to dodge hitstop starting from cancelling it out
            rb.Velocity = force;

            //send the box data to have damage and scaling applied
            DamageHealth(boxData, proration);

            //we're apply "getting hit" hitstop when we assign hitstun so that we don't lose any frames of hitstop
            //look at CleanUpNewState in the VulnerableObject script to see when we apply it

            //ApplyHitstop(hold.hitStopEnemy);
            status.AddTransitionFlags(TransitionFlag.GOT_HIT);

            Debug.Log("processing hit - x :: " + boxData.launchDir.x * dir + ", y :: " + boxData.launchDir.y + "; " + boxData.launchForce + " | " + boxData.damage + " | " + hold.hitStopEnemy);

        }

        // --- INTERFACE METHODS --- //
        //when a detector detects a collision, add transition flags
        public void TriggerCollided(EnvironmentDetector sender)
        {
            //message from grounded trigger, add grounded transition condition
            //and remove airborne transition condition
            if (sender.name == "GroundedTrigger")
            {
                status.AddTransitionFlags(TransitionFlag.GROUNDED);
                status.RemoveTransitionFlags(TransitionFlag.AIRBORNE);
            }
            //message from wall trigger, add walled transition condition
            else if (sender.name == "WallTrigger") { status.AddTransitionFlags(TransitionFlag.WALLED); }
        }

        //when a detector detects a collision, remove transition flags
        public void TriggerExitCollided(EnvironmentDetector sender)
        {
            //message from grounded trigger, remove grounded transition condition
            //and add airborne transition condition
            if (sender.name == "GroundedTrigger")
            {
                status.RemoveTransitionFlags(TransitionFlag.GROUNDED);
                status.AddTransitionFlags(TransitionFlag.AIRBORNE);
            }
            //message from wall trigger, remove walled transition condition
            else if (sender.name == "WallTrigger") { status.RemoveTransitionFlags(TransitionFlag.WALLED); }
        }

    }
}