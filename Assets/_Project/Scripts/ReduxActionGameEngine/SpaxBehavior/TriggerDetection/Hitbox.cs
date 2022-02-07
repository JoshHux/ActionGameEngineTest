using System;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;
using VelcroPhysics.Templates;
using VelcroPhysics.Tools.Triangulation.TriangulationBase;
using VelcroPhysics.Utilities;
using VelcroPhysics.Factories;

using System.Linq;
using System.Collections.Generic;
using VelcroPhysics.Collision.ContactSystem;
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
        [SerializeField] private List<VelcroBody> curColliding;
        //previously collided with
        [SerializeField] private List<VelcroBody> wasColliding;
        //what gets queried, difference between the two previous lists
        [SerializeField] private List<VelcroBody> diffColliders;

        protected override void OnAwake()
        {
            base.OnAwake();
            activeTimer = new CallbackTimer();
            curCollidingGo = new List<GameObject>();
            curColliding = new List<VelcroBody>();
            wasColliding = new List<VelcroBody>();
            diffColliders = new List<VelcroBody>();
            activeTimer.OnEnd += ctx => DeactivateBox(ctx);

            isActive = false;
            //activeTimer.Invoke();
            //Debug.Log(activeTimer);
        }

        protected override void OnStart()
        {
            base.OnStart();
            trigger.Body.ContCollision += (a, b, c) => Test(c);

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

        private void Test(Contact c)
        {
            GameObject other = c.FixtureB.Body.gameObject;
            if (isActive)// && (other != null))
            {
                IAlligned otherObj = other.GetComponent<IAlligned>();

                int otherAllignment = otherObj.GetAllignment();
                //Debug.Log("triggered " + this.name + " with " + other.name);

                if (otherAllignment != this._allignment)
                {
                    //Debug.Log("collided -- " + this.name);

                    //Debug.Log("root difference");
                    VelcroBody newCol = other.GetComponent<VelcroBody>();
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
            data = boxData;
            activeTimer.StartTimer(data.duration);
            //trigger.localPosition = data.localPos;
            //trigger.localRotation = new BepuQuaternion(data.localRot.Z, data.localRot.Y, data.localRot.Z, trigger.localRotation.W);
            FVector2 newPos = data.localPos;
            int facing = owner.GetFacing();
            newPos.x *= facing;
            //trigger.Enabled = false;

            trigger.SetDimensions(data.localDim);


            trigger.Body.ContCollision += (a, b, c) => Test(c);
            //trigger.Body.OnCollision += (a, b, c) => Test(c);


            //trigger.Position = FVector2.zero;


            trigger.LocalPosition = newPos;

            trigger.Body.constraint.childRotation = FixedMath.C0p5;
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
            //gets the new bepuColliders to collide with
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

            trigger.Enabled = false;

            if (activeTimer.IsTicking())
            {
                activeTimer.EndTimer();
            }
            trigger.Body.OnCollision -= (a, b, c) => Test(c);

            trigger.MakeForRemoval();

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
                Body body = trigger.Body;

                var thing = (body.FixtureList[0].Shape as PolygonShape).Vertices;
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);

                //Debug.Log(thing[0].x + ", " + thing[0].y + " - " + thing[1].x + ", " + thing[1].y + " - " + thing[2].x + ", " + thing[2].y + " - " + thing[3].x + ", " + thing[3].y);

                //Debug.Log(body.FixtureList[0].)

                Gizmos.matrix = Matrix4x4.TRS(trigger.transform.position, Quaternion.identity, Vector3.one);
                //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
                if (trigger.GetBody() != null)
                    Gizmos.DrawCube(trigger.transform.position, new Vector2((float)(trigger as VelcroBox).Width * 0.8f, (float)(trigger as VelcroBox).Height * 0.8f));
            }
        }
#endif
    }
}