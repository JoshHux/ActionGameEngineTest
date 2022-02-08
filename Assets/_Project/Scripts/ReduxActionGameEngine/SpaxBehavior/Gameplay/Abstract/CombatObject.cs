using UnityEngine;
using ActionGameEngine.Interfaces;
using ActionGameEngine.Enum;
using ActionGameEngine.Data;
using Spax;

namespace ActionGameEngine.Gameplay
{
    public abstract class CombatObject : VulnerableObject, IDamager
    {
        //essentially, attackerID of this object, 
        //unique to this object
        protected int combatID;
        protected Hitbox[] hitboxes;

        protected override void OnStart()
        {
            base.OnStart();
            //assignment of combat id based on what index we were added, should be nonzero
            //I'll probably change this later, it's better to keep this for now
            combatID = SpaxManager.instance.GetTrackingIndexOf(this) + 1;


            GameObject hitHolder = ObjectFinder.FindChildWithTag(this.gameObject, "Hitboxes");
            hitboxes = hitHolder.GetComponentsInChildren<Hitbox>();

            //initialize our boxes
            int len = hitboxes.Length;
            for (int i = 0; i < len; i++)
            {
                Hitbox box = hitboxes[i];

                box.Initialize();
            }


        }

        protected override void HitboxQueryUpdate()
        {
            int len = hitboxes.Length;
            //make invalid hitbox with really low priority, overridden if attack connected
            HitboxData boxData = new HitboxData();
            boxData.priority = int.MinValue;
            HitIndicator indicator = HitIndicator.WHIFFED;

            for (int i = 0; i < len; i++)
            {
                Hitbox box = hitboxes[i];
                if (box.IsActive())
                {
                    indicator = box.QueryHitboxCollisions();
                    HitboxData potenBoxData = box.GetHitboxData();
                    //record hitbox for later if attack connectes
                    if ((potenBoxData.priority > boxData.priority) && (!EnumHelper.HasEnum((uint)indicator, (int)HitIndicator.WHIFFED)))
                    { boxData = box.GetHitboxData(); }

                    if (!status.inHitstop)
                    {
                        //Debug.Log("ticking hitbox");
                        box.TickTimer();
                    }
                }
            }

            //we connected with a hitbox
            //aka. we didn't whiff
            if (!EnumHelper.HasEnum((uint)indicator, (int)HitIndicator.WHIFFED))
            {
                status.AddCancelConditions(boxData.GetCancelConditions(indicator));
            }
        }

        protected override void ProcessFrameData(FrameData frame)
        {
            base.ProcessFrameData(frame);

            if (frame.HasHitboxes()) { ActivateHitboxes(frame.hitboxes); }
        }

        protected override void ProcessTransitionEvents(TransitionEvent transitionEvents)
        {
            TransitionEvent te = transitionEvents;
            base.ProcessTransitionEvents(te);

            //deactivate our active hitboxes
            if (EnumHelper.HasEnum((uint)te, (int)TransitionEvent.CLEAN_HITBOXES)) { this.DeactivateHitboxes(); }
            //we may want other enemy to block
            if (EnumHelper.HasEnum((uint)te, (int)TransitionEvent.FLAG_BLOCK)) { this.FlagBlockToOthers(); }
        }


        protected void ActivateHitboxes(HitboxData[] boxData)
        {
            int len = boxData.Length;
            for (int i = 0; i < len; i++)
            {
                HitboxData data = boxData[i];
                Hitbox box = hitboxes[i];

                box.ActivateHitBox(data);
            }
        }


        public int GetCombatID()
        {
            return combatID;
        }

        private void DeactivateHitboxes()
        {
            int len = hitboxes.Length;

            for (int i = 0; i < len; i++)
            {
                Hitbox box = hitboxes[i];
                //deactivate any active hitboxes
                if (box.IsActive()) { box.DeactivateBox(); }
            }
        }

        //PLEASE TREAT AS PRIVATE
        public abstract int ConnectedHit(HitboxData boxData);
        //call to tell any relevant enemies to block (for proximity blocking)
        protected abstract void FlagBlockToOthers();

    }
}