using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ActionGameEngine.Enum;
using ActionGameEngine.Data.Helpers;
using ActionGameEngine.Data;
using ActionGameEngine.Interfaces;
using BEPUutilities;
using FixMath.NET;
namespace ActionGameEngine.Gameplay
{
    public abstract class VulnerableObject : LivingObject, IDamageable
    {
        protected Hurtbox[] hurtboxes;
        //the attacker will not hit damageables of the same allignment
        protected int allignment = -1;
        //says whether or we blocked the hits, etc.
        //says whether we use counterhit, hit, or blocked data
        //the dictionary being nonzero will tell us if we were even hit at all
        protected HitIndicator hitIndicator;
        //dictionary of attackerID and hitbox it put forward to process
        protected Dictionary<int, HitboxData> opposingBoxes;

        protected override void OnAwake()
        {
            base.OnAwake();
            opposingBoxes = new Dictionary<int, HitboxData>();
        }

        protected override void OnStart()
        {
            base.OnStart();
            GameObject hurtHolder = ObjectFinder.FindChildWithTag(this.gameObject, "Hurtboxes");
            hurtboxes = hurtHolder.GetComponentsInChildren<Hurtbox>();

            ResetHealth();
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

                    ProcessHitbox(boxData);
                }

                //processed the hitboxes, clear the hitboxes
                opposingBoxes.Clear();
            }
        }

        protected override void ActivateHurtboxes(HurtboxData[] boxData)
        {
            int len = boxData.Length;
            for (int i = 0; i < len; i++)
            {
                HurtboxData data = boxData[i];
                Hurtbox box = hurtboxes[i];

                box.ActivateHurtBox(data);
            }
        }

        protected void AddPotentialHitbox(int attackerID, HitboxData boxData, HitIndicator indicator)
        {
            //last hitbox grabbed us
            bool justGrabbed = EnumHelper.HasEnum((int)indicator, 16);
            //we were hit by a grab hitbox
            bool wasGrabbed = EnumHelper.HasEnum((int)hitIndicator, 16);
            //if we get grabbed, ignore every other hitbox
            if ((!wasGrabbed) || justGrabbed)
            {
                //whether or not we should override the hitboxes we currently remember
                //and the attackerID is lower
                //and the priority is higher (probably not needed but included just in case)

                //!! only relevant if we were previously grabbed and we are being grabbed!!
                bool shouldOverwriteBox = (opposingBoxes.ElementAt(0).Key > attackerID) && (opposingBoxes.ElementAt(0).Value.priority < boxData.priority);
                bool overrideBoxes = wasGrabbed && shouldOverwriteBox;

                //we override if:
                //we were grabbed and we should override hitboxes
                //or
                //we are grabbed and we did not get grabbed previously
                overrideBoxes = overrideBoxes || ((!wasGrabbed) && justGrabbed);


                if (overrideBoxes)
                {
                    opposingBoxes.Clear();
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

        public abstract HitIndicator GetHit(int attackerID, HitboxData boxData);

        //call to process whatever HitboxData we recieve
        protected abstract void ProcessHitbox(HitboxData boxData);

    }
}