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

        //NOTE: PHASE OUT BY USING EITHER POSITIVE OR NEGATIVE ID FOR LEFT OR RIGHT FACING
        //dictionary of attackerID and direction they were facing
        protected Dictionary<int, int> opposingDir;

        protected override void OnAwake()
        {
            base.OnAwake();
            opposingBoxes = new Dictionary<int, HitboxData>();
            opposingDir = new Dictionary<int, int>();
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
                Debug.Log("possible reassigning of state duration");
                //of the state's duration is negative, we should assign the duration ourselves

                //hold the Data Holder, which has the stun values
                HitboxDataHolder hitData = this.GetHitboxData();

                //1 or 0 section
                //1 if the duration is negative, 0 if positive
                int curStateDur = status.currentState.duration >> 31;
                //flip of the int above, just to make numerical stuff easier
                int isPos = curStateDur ^ 1;

                //the new stun/state duration
                int stunDur = hitData.stun * curStateDur;
                //the state duration we keep
                int stateDur = status.currentState.duration * isPos;
                //new duration value to set to the timer
                int newDur = stunDur + stateDur;

                //assign new state duration
                stateTimer.StartTimer(newDur);

                //processed the hitboxes, clear the hitboxes
                opposingBoxes.Clear();

                //I wanna delete this so bad
                opposingDir.Clear();
            }

        }

        protected override void HurtboxQueryUpdate()
        {
            //only run if the dictionary is not empty
            if (opposingBoxes.Count > 0)
            {
                //TODO: check if we parry, if not, then don't take damage
                //TODO: check if we armor hits, if we do, then don't get stunned, unless the hit taken is a superhit

                int len = opposingBoxes.Count;

                for (int i = 0; i < len; i++)
                {
                    HitboxData boxData = opposingBoxes.ElementAt(i).Value;
                    int dir = opposingDir.ElementAt(i).Value;

                    ProcessHitbox(boxData, dir);
                }
                //we don't clear hitboxes here since we need them to determine how long the stun state lasts for
            }
        }

        protected override void ProcessFrameData(FrameData frame)
        {
            base.ProcessFrameData(frame);

            if (frame.HasHurtboxes()) { ActivateHurtboxes(frame.hurtboxes); }

        }

        //call to get the appropriate hitbox's AttackLevelVal
        private HitboxDataHolder GetHitboxData()
        {
            HitboxDataHolder ret = opposingBoxes[0].GetHolder(hitIndicator);
            //TODO: 
            return ret;
        }

        protected void ActivateHurtboxes(HurtboxData[] boxData)
        {
            int len = boxData.Length;
            for (int i = 0; i < len; i++)
            {
                HurtboxData data = boxData[i];
                Hurtbox box = hurtboxes[i];
                //Debug.Log("dafsadfs");
                box.ActivateHurtBox(data);
            }
        }

        protected void AddPotentialHitbox(int attackerID, HitboxData boxData, HitIndicator indicator, int dir)
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
                    Debug.Log("adding hitbox");
                    //add the new key-value pair to the dictionary
                    opposingBoxes.Add(attackerID, boxData);
                    opposingDir.Add(attackerID, dir);
                }
                hitIndicator = indicator;
            }
        }



        public int GetAllignment()
        {
            return allignment;
        }

        //call to subtract damage from our currentHp
        protected void DamageHealth(int damage)
        {
            status.SubtractCurrentHP(damage);
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

        public abstract HitIndicator GetHit(int attackerID, HitboxData boxData, int dir);

        //call to process whatever HitboxData we recieve
        protected abstract void ProcessHitbox(HitboxData boxData, int dir);

    }
}