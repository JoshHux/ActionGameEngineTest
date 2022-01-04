using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ActionGameEngine.Enum;
using ActionGameEngine.Data;
using ActionGameEngine.Data.Helpers;
using ActionGameEngine.Interfaces;
using FixMath.NET;
using Spax;

namespace ActionGameEngine.Gameplay
{
    public abstract class VulnerableObject : LivingObject, IDamageable
    {
        protected Hurtbox[] hurtboxes;
        //the attacker will not hit damageables of the same allignment
        [ReadOnly, SerializeField] protected int allignment = -1;
        //says whether or we blocked the hits, etc.
        //says whether we use counterhit, hit, or blocked data
        //the dictionary being nonzero will tell us if we were even hit at all
        protected HitIndicator hitIndicator;
        //dictionary of attackerID and hitbox it put forward to process
        protected Dictionary<int, HitboxData> opposingBoxes;

        //this is proration that is stored and not yet applied
        //proration will apply on moves that come after it, so we're storing it here for now
        private Fix64 storedProration;
        //these are the number of hits to be added to the combo counter, not taken into account when scaling the damage
        private int storedComboCount;

        //NOTE: PHASE OUT BY USING EITHER POSITIVE OR NEGATIVE ID FOR LEFT OR RIGHT FACING
        //dictionary of attackerID and direction they were facing
        //protected Dictionary<int, int> opposingDir;

        protected override void OnAwake()
        {
            base.OnAwake();
            opposingBoxes = new Dictionary<int, HitboxData>();
            storedProration = Fix64.One;
            storedComboCount = 0;
            //opposingDir = new Dictionary<int, int>();
        }

        protected override void OnStart()
        {
            base.OnStart();

            //find the parent of all hurtboxes
            GameObject hurtHolder = ObjectFinder.FindChildWithTag(this.gameObject, "Hurtboxes");
            //stor all of the hurtboxes
            hurtboxes = hurtHolder.GetComponentsInChildren<Hurtbox>();
            //initialize all hurtboxes
            int len = hurtboxes.Length;
            for (int i = 0; i < len; i++)
            {
                Hurtbox box = hurtboxes[i];
                box.Initialize();
            }
            ResetHealth();
        }

        protected override void CleanUpNewState()
        {

            //BRANCHLESS FCK YEAH
            //edit: nevermind...

            //if the length is nonzero, then we know that we were hit and should look for a hitbox to process
            if (opposingBoxes.Count > 0)
            {
                //if the state's duration is negative, we should assign the duration ourselves

                //hitbox's data is still here in case we want to look at it still
                HitboxData hold = this.GetHitboxData();
                //hold the Data Holder, which has the stun values
                HitboxDataHolder hitData = hold.GetHolder(hitIndicator);

                //Debug.Log("before bit operations");
                //1 or 0 section
                //1 if the duration is negative, 0 if positive
                uint unsignedDur = (uint)status.currentState.duration;
                uint signedBit = unsignedDur >> 31;
                int curStateDur = (int)signedBit;
                //Debug.Log("after bit operations");

                //flip of the int above, just to make numerical stuff easier
                int isPos = curStateDur ^ 1;

                //the new stun/state duration
                int stunDur = hitData.stun * curStateDur;
                //the state duration we keep
                int stateDur = status.currentState.duration * isPos;
                //new duration value to set to the timer
                int newDur = stunDur + stateDur;

                //whether we're airborne or not
                //bool isAirborne = EnumHelper.HasEnum((uint)status.GetStateConditions(), (uint)StateCondition.AIRBORNE);

                //TODO (maybe): make branchless, setting the untech flag and/or either timer durations to zero based on the airborne flag

                //if we're airborne, then we set the timer to untech time instead of hitstun
                //we don't care about setting the state timer, since there shouldn't be an automatic transition from air hitstun when the duration ends
                /*if (isAirborne)
                {
                    persistentTimer.StartTimer(newDur, StateCondition.AIR_UNTECHABLE);
                    status.AddPersistenConditions(StateCondition.AIR_UNTECHABLE);
                }*/

                //EPIPHANY: STATE dURATION IS UNTECH TIMER FOR AIRBORNE HITSUN
                //ON END CANCEL CONDITION WITH INPUT REQUIREMENTS PREVENTS UNWANTED TECHING
                //assign new state duration
                stateTimer.StartTimer(newDur);

                //apply hitstop, we're doing this here so that we prevent a scenario where we lose a frame of hitstop
                //this would occur becuase the start of hitstop is at the end of the a frame
                //and the ticking of hitstop is at the start of the frame
                ApplyHitstop(hitData.hitStopEnemy);

                //processed the hitboxes, clear the hitboxes
                opposingBoxes.Clear();
                Debug.Log("possible reassigning of state duration of " + status.currentState.stateName + " to " + newDur + "\n" + hitIndicator);

                //I wanna delete this so bad
                //opposingDir.Clear();
            }

        }

        protected override void HurtboxQueryUpdate()
        {
            //reset storedProration to 1
            this.storedProration = Fix64.One;
            //reset storedComboCount to 0
            this.storedComboCount = 0;

            //only run if the dictionary is not empty
            if (opposingBoxes.Count > 0)
            {
                //TODO: check if we parry, if not, then don't take damage
                //TODO: check if we armor hits, if we do, then don't get stunned, unless the hit taken is a superhit

                int len = opposingBoxes.Count;

                for (int i = 0; i < len; i++)
                {
                    HitboxData boxData = opposingBoxes.ElementAt(i).Value;
                    //int dir = opposingDir.ElementAt(i).Value;F
                    int key = opposingBoxes.ElementAt(i).Key;

                    //another branchless thing I'm proud of
                    //passing -1 if attacker was facing left and 1 if attacker was facing right
                    //IMPORTANT NOTE: SIGNED INT LEFT SHIFT IS ARITHMATIC YOU MUST CONVERT TO UINT TO GET LOGICAL SHIFT
                    uint unsignedKey = (uint)key;
                    uint signedBit = unsignedKey >> 31;
                    int isNegative = (int)signedBit;
                    //-2 if the integer is negative, 0 if positive
                    int negativePass = -2 * isNegative;
                    //attacker facing direction
                    //adds -2+1 if the key is negative, resulting in -1
                    //or 0+1 if the key is positive, resulting in 1
                    int dir = negativePass + 1;
                    Debug.Log("key :: " + key + " | negative shift :: " + isNegative + " | -2 multiplication :: " + negativePass + " | concluded direction :: " + dir);
                    ProcessHitbox(boxData, dir);
                }
                //we don't clear hitboxes here since we need them to determine how long the stun state lasts for
            }

            //apply storedProration to the overall proration, we aren't doing more damage calcs
            status.proration *= this.storedProration;
            //apply storedComboCount to the overall combo count
            status.comboCount += this.storedComboCount;
        }

        protected override void ProcessFrameData(FrameData frame)
        {
            base.ProcessFrameData(frame);

            if (frame.HasHurtboxes()) { ActivateHurtboxes(frame.hurtboxes); }

        }

        //call to get the appropriate hitbox's AttackLevelVal
        private HitboxData GetHitboxData()
        {
            HitboxData ret = opposingBoxes.ToArray<KeyValuePair<int, HitboxData>>()[0].Value;
            //TODO: implement algorithm to find and prioritize hitboxes with the highest priority
            return ret;
        }

        protected void ActivateHurtboxes(HurtboxData[] boxData)
        {
            //TODO: REDO HIT/HURTBOX ACTIVATION
            //each hurt/hitbox (will be reffered to as "box" or "boxes" from now on) will each have an id
            //that will correspond to its index in the below loop
            //the ActivateHurtboxes function will be a callback that each box will have their hooks in
            //the handler will accept an array of box-data that each box will draw from based on its id
            //if the list is shorter than the id, then the box is deactivated

            int len = boxData.Length;
            for (int i = 0; i < len; i++)
            {
                HurtboxData data = boxData[i];
                Hurtbox box = hurtboxes[i];
                //Debug.Log("dafsadfs");
                box.ActivateHurtBox(data);
            }
        }

        protected void AddPotentialHitbox(int attackerID, HitboxData boxData, HitIndicator indicator)
        {
            Debug.Log("potentially adding hitbox");
            //last hitbox grabbed us
            bool justGrabbed = EnumHelper.HasEnum((uint)indicator, 16);
            //we were hit by a grab hitbox
            bool wasGrabbed = EnumHelper.HasEnum((uint)hitIndicator, 16);
            //if we get grabbed, ignore every other hitbox
            if ((!wasGrabbed) || justGrabbed)
            {
                //whether or not we should override the hitboxes we currently remember
                //and the attackerID is lower
                //and the priority is higher (probably not needed but included just in case)

                //!! only relevant if we were previously grabbed and we are being grabbed!!
                bool shouldOverwriteBox = (opposingBoxes.Count > 0) && (opposingBoxes.ElementAt(0).Key > attackerID) && (opposingBoxes.ElementAt(0).Value.priority < boxData.priority);
                bool overrideBoxes = wasGrabbed && shouldOverwriteBox;

                //we override if:
                //we were grabbed and we should override hitboxes
                //or
                //we are grabbed and we did not get grabbed previously
                overrideBoxes = overrideBoxes || ((!wasGrabbed) && justGrabbed);


                if (overrideBoxes)
                {
                    //opposingBoxes.Clear();
                }
                //if we already got a hitbox from that attacker
                if (opposingBoxes.ContainsKey(attackerID))
                {
                    HitboxData hold = opposingBoxes[attackerID];

                    //if the new potential hitbox has a higher priority
                    if (boxData.priority > hold.priority)
                    {
                        opposingBoxes[attackerID] = boxData;

                    }
                }
                //we haven't received a hitbox from that attacker
                else
                {
                    //Debug.Log("adding hitbox :: " + attackerID);
                    //add the new key-value pair to the dictionary
                    opposingBoxes.Add(attackerID, boxData);
                }
                hitIndicator = indicator;
            }
        }



        public int GetAllignment()
        {
            return allignment;
        }

        //call to subtract damage from our currentHp
        protected void DamageHealth(HitboxData data)
        {
            //1 if we blocked
            int wasBlocked = EnumHelper.HasEnumInt((uint)hitIndicator, (uint)HitIndicator.BLOCKED);
            //1 if we didn't block
            int wasHit = wasBlocked ^ 1;


            //each damage value, recorded and multiplied by a "boolean"
            int chip = wasBlocked * data.chipDamage;
            int hit = wasHit * data.damage;

            //Some maths so that proration applies only on hit and not block
            Fix64 fPro = (wasHit * data.forcedProration) + (wasBlocked * Fix64.One);
            Fix64 iPro = (wasHit * data.initProration) + (wasBlocked * Fix64.One);

            //Either the chip or the on hit damage
            int rawDamage = chip + hit;

            //damage scaled using the proration accumulated so far
            //if the hit is blocked, then fPro and iPro are set to 1, making the scaling irrelavent
            int scaledDamage = this.ApplyProration(rawDamage, fPro, iPro);

            //if the hit is blocked, then this is se to 0
            int minDamage = data.minDamage * wasHit;

            //gets at least the minimum damage if the proration is too high
            //if the hit is blocked, then we'll get the chip damage or 0 (minDamage is 0 when the hit is blocked)
            int damage = System.Math.Max(scaledDamage, minDamage);

            //add to the stored combo hits if we aren't blocking
            this.storedComboCount += 1 * wasHit;

            //change our current hp value based on the final damage calculations
            status.SubtractCurrentHP(damage);
        }

        //call to apply and store proration
        private int ApplyProration(int rawDamage, Fix64 forcedPro, Fix64 initPro)
        {
            //recording the basic relevant damage values
            int ret = rawDamage;
            //the current proration applied to this hit
            Fix64 curPro = status.proration;
            //proration for the current hit in a combo
            //TODO: make a method to determine the scaling based on the current combo count
            Fix64 comboPro = Fix64.One;
            Fix64 fPro = forcedPro;
            Fix64 iPro = initPro;

            //makes initial proration irrelavent if the hit wasn't the first hit in a combo
            if (status.comboCount > 0) { iPro = Fix64.One; }

            //combines the prorations
            Fix64 boxPro = fPro * iPro;

            //store the proration to be used later
            this.storedProration *= boxPro;


            //scales the damage
            ret = (int)(ret * curPro * comboPro);

            return ret;
        }

        //call to subtract damage from our currentHp
        protected void HealHealth(int damage)
        {
            status.AddCurrentHP(damage);
        }

        //call to reset out currentHp back to our maxHP
        protected void ResetHealth()
        {
            status.SetCurrentHP(data.maxHp);
        }

        public abstract HitIndicator GetHit(int attackerID, HitboxData boxData);

        //call to process whatever HitboxData we recieve
        protected abstract void ProcessHitbox(HitboxData boxData, int dir);

    }
}