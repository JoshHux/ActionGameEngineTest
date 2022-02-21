using UnityEngine;
using FixMath.NET;
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
        private Fix64 storedProration;

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
            storedProration = Fix64.One;


        }

        protected override void HitboxQueryUpdate()
        {
            int len = hitboxes.Length;
            //make invalid hitbox with really low priority, overridden if attack connected
            HitboxData boxData = new HitboxData();
            boxData.priority = int.MinValue;
            //EQUIVOLENT TO 0
            HitIndicator indicator = HitIndicator.WHIFFED;

            for (int i = 0; i < len; i++)
            {
                Hitbox box = hitboxes[i];
                if (box.IsActive())
                {
                    indicator = box.QueryHitboxCollisions();
                    HitboxData potenBoxData = box.GetHitboxData();
                    //record hitbox for later if attack connectes
                    if ((potenBoxData.priority > boxData.priority) && (!(indicator > 0)))
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
            if (!(indicator > 0))
            {
                //we connected a hit, apply necessary calcs
                this.ConnectedHit(boxData, indicator);
            }
        }

        protected override void ProcessFrameData(FrameData frame)
        {
            base.ProcessFrameData(frame);

            if (frame.HasHitboxes()) { ActivateHitboxes(frame.hitboxes); }
        }

        protected override void CleanUpNewState()
        {
            base.CleanUpNewState();
            //we apply proration after we transition to a new state, that way multi-hit states don't have scaling until afterwards
            //apply storedProration to the overall proration, we aren't doing more damage calcs
            status.proration *= this.storedProration;
            //reset storedProration to 1, apply the proration gathered to the proration for the next state
            this.storedProration = Fix64.One;
        }

        protected override void ProcessTransitionEvents(TransitionEvent transitionEvents, ResourceData resourceData = new ResourceData())
        {
            TransitionEvent te = transitionEvents;
            base.ProcessTransitionEvents(te, resourceData);

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

        //call to apply and store the proration
        //call when we want to update storedProration
        protected void ApplyProration(Fix64 forcedPro, Fix64 initPro, int firstHit)
        {

            //We'll update proration on next state
            //the current proration applied to this hit
            //Fix64 curPro = status.proration;
            //proration for the current hit in a combo
            //TODO: make a method to determine the scaling based on the current combo count
            Fix64 comboPro = Fix64.One;
            Fix64 fPro = forcedPro;

            //flip firstHit, if it's 0, then it'll become 1, effectively eliminating initial proration
            int comboedHit = firstHit ^ 1;
            Fix64 iPro = initPro * firstHit + Fix64.One * comboedHit;


            //combines the prorations
            Fix64 boxPro = fPro * iPro;

            //store the proration to be used later
            this.storedProration *= boxPro;
        }

        //PLEASE TREAT AS PRIVATE
        public abstract int ConnectedHit(HitboxData boxData, HitIndicator indicator);

        public abstract Fix64 GetProration();
        //call to tell any relevant enemies to block (for proximity blocking)
        protected abstract void FlagBlockToOthers();

    }
}