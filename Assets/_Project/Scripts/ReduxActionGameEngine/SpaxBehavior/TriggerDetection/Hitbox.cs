using System;
using FlatPhysics.Unity;
using FlatPhysics.Contact;

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ActionGameEngine.Data;
using ActionGameEngine.Enum;
using ActionGameEngine.Interfaces;
using FixMath.NET;

namespace ActionGameEngine.Gameplay
{
    public class Hitbox : TriggerDetector, IAlligned
    {

        //record from CombatObject
        [ReadOnly, SerializeField] private int _ownerID = 0;

        private bool isActive = false;
        [ReadOnly, SerializeField] private int _allignment = 0;
        //todo: find a place to tick the timer
        [SerializeField] private CallbackTimer activeTimer;
        private CombatObject owner;
        private HitboxData data;
        //currently colliding with, but the parent gameobject, to prevent multi-hits when we don't want them
        [SerializeField] private List<GameObject> curCollidingGo;
        //currently colliding with
        [SerializeField] private List<FRigidbody> curColliding;
        //previously collided with
        [SerializeField] private List<FRigidbody> wasColliding;
        //what gets queried, difference between the two previous lists
        [SerializeField] private List<FRigidbody> diffColliders;

        protected override void OnAwake()
        {
            base.OnAwake();
            activeTimer = new CallbackTimer();
            curCollidingGo = new List<GameObject>();
            curColliding = new List<FRigidbody>();
            wasColliding = new List<FRigidbody>();
            diffColliders = new List<FRigidbody>();
            activeTimer.OnEnd += ctx => DeactivateBox(ctx);

            isActive = false;
            //activeTimer.Invoke();
            //Debug.Log(activeTimer);
        }

        protected override void OnStart()
        {
            base.OnStart();
            trigger.Body.OnOverlap += (c) => Test(c);

        }

        public void Initialize()
        {
            owner = this.transform.parent.parent.gameObject.GetComponent<CombatObject>();
            _ownerID = owner.GetCombatID();
            _allignment = owner.GetAllignment();

        }

        protected override void OnEnterTrigger(GameObject other)
        {

            //Debug.Log("triggered -- " + this.name);
            /*if (isActive)
            {
                IAlligned otherObj = other.GetComponent<IAlligned>();

                int otherAllignment = otherObj.GetAllignment();

                if (otherAllignment != this._allignment)
                {
                    //Debug.Log("collided -- " + this.name);

                    //Debug.Log("root difference");
                    VelcroBody newCol = other.GetComponent<VelcroBody>();
                    if (!curColliding.Contains(newCol) && !curCollidingGo.Contains(other.transform.parent.gameObject))
                    {
                        Debug.Log(_ownerID + " -- overlapping - " + other.name);
                        curColliding.Add(newCol);
                        curCollidingGo.Add(other.transform.gameObject);
                        //curCollidingGo.Add(other.transform.parent.gameObject);
                    }

                }
            }*/
        }

        private void Test(ContactData c)
        {
            GameObject other = c.other;
            if (isActive)// && (other != null))
            {
                IAlligned otherObj = other.GetComponent<IAlligned>();

                int otherAllignment = otherObj.GetAllignment();
                //Debug.Log("triggered " + this.name + " with " + other.name);

                if (otherAllignment != this._allignment)
                {
                    //Debug.Log("collided -- " + this.name);

                    //Debug.Log("root difference");
                    FRigidbody newCol = other.GetComponent<FRigidbody>();
                    if (!curColliding.Contains(newCol) && !curCollidingGo.Contains(other.transform.parent.gameObject))
                    {
                        //Debug.Log(_ownerID + " -- overlapping - " + other.name);
                        curColliding.Add(newCol);
                        curCollidingGo.Add(other.transform.gameObject);
                        //curCollidingGo.Add(other.transform.parent.gameObject);
                    }

                }
            }
        }


        protected override void OnExitTrigger(GameObject other) {/*don't need this*/}

        public void ActivateHitBox(HitboxData boxData)
        {
            isActive = true;
            this.trigger.Awake = true;
            data = boxData;
            activeTimer.StartTimer(data.duration);
            FVector2 newPos = data.localPos;
            int facing = owner.GetFacing();
            newPos.x *= facing;
            //trigger.Enabled = false;

            trigger.SetDimensions(data.localDim);



            trigger.LocalPosition = newPos;

            //trigger.FindNewContacts();
            //we need to set this after we jiggle the hitbox's location so that it doesn't add hitboxes  that aren't there
            //trigger.Enabled = true;
            //trigger.FindNewContacts();


        }

        public void TickTimer()
        {
            isActive = activeTimer.TickTimer();
        }


        public void SetAllignment(int allignment) { this._allignment = allignment; }

        public int GetAllignment() { return _allignment; }
        public bool IsActive() { return isActive; }

        public HitboxData GetHitboxData() { return data; }

        public HitIndicator QueryHitboxCollisions()
        {

            HitIndicator ret = 0;
            //gets the new colliders to collide with
            diffColliders = curColliding.Except(wasColliding).ToList();
            //remember what we WERE colliding with
            wasColliding = curColliding.ToList();
            int len = diffColliders.Count;
            bool clash = true;

            for (int i = 0; i < len; i++)
            {
                Hitbox hitbox;
                Hurtbox hurtbox;
                if (curColliding[i].TryGetComponent<Hitbox>(out hitbox))
                {

                    //Debug.Log("Querying  -  " + (box != null) + " " + (box.GetAllignment() != playerIndex));
                    if ((hitbox != null) && (hitbox.GetAllignment() != _allignment) && clash)
                    {

                        //TODO: return what happens when you clash with another hitbox

                        //ret = hitbox.HitThisBox(ownerID, data);
                    }
                }
                else if (curColliding[i].TryGetComponent<Hurtbox>(out hurtbox))
                {

                    //Debug.Log("Querying  -  " + (box != null) + " " + (box.GetAllignment() != playerIndex));
                    if ((hurtbox != null) && (hurtbox.GetAllignment() != _allignment))
                    {
                        clash = false;

                        //has a sign to denote what direction the attacker was facing when we were hit
                        int signedID = owner.GetFacing() * _ownerID;
                        ret = hurtbox.HitThisBox(signedID, data);
                        //Debug.Log("hit with hitbox :: " + gameObject.name+" "+ret);
                        //  player.OnHit(data, box.GetHit());
                        //Debug.Log("found");
                    }
                }
            }
            return ret;
        }

        public void DeactivateBox(object sender = null)
        {
            //refreshes list to prepare for collision queries
            curCollidingGo.Clear();
            curColliding.Clear();
            wasColliding.Clear();
            diffColliders.Clear();


            if (activeTimer.IsTicking())
            {
                activeTimer.EndTimer();
            }

            this.trigger.Awake = false;


            Debug.Log("Deactivating Hitbox :: " + gameObject.name + " " + " " + curCollidingGo.Count + " " + curColliding.Count + " " + wasColliding.Count);

        }

        private void test()
        {
            //GameObject other = c.Body.gameObject;
            //if (other.GetComponent<Hurtbox>() != null && other.GetComponent<Hurtbox>().GetAllignment() != this._allignment)
            //    Debug.Log("FSdafsadjk " + other.name);
            Debug.Log("FSdafsadjk " + gameObject.name);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (trigger != null)
            {

            }
        }
#endif
    }
}