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
        private int _ownerID = 0;
        private int _allignment = 0;
        private CallbackTimer activeTimer;
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

            activeTimer.OnEnd += DeactivateBox;
        }

        protected override void OnStart()
        {
            base.OnStart();
            owner = this.transform.parent.parent.gameObject.GetComponent<CombatObject>();
            _ownerID = owner.GetCombatID();
            _allignment = owner.GetAllignment();
        }

        protected override void OnEnterTrigger(GameObject other)
        {

            if (activeTimer.IsTicking())
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
                        //Debug.Log("overlapping - " + player.gameObject.name);
                        curColliding.Add(newCol);
                        curCollidingGo.Add(other.transform.parent.gameObject);
                    }

                }
            }
        }


        protected override void OnExitTrigger(GameObject other) {/*don't need this*/}

        public void ActivateHitBox(HitboxData boxData)
        {
            data = boxData;
            //trigger.localPosition = data.localPos;
            //trigger.localRotation = new BepuQuaternion(data.localRot.Z, data.localRot.Y, data.localRot.Z, trigger.localRotation.W);
            FVector2 newPos = data.localPos;
            int facing = owner.GetFacing();
            newPos.x *= facing;

            trigger.LocalPosition = newPos;
            trigger.SetDimensions(data.localDim);
            activeTimer.StartTimer(data.duration);
            //Debug.Log("scaled");
        }

        public void SetAllignment(int allignment) { this._allignment = allignment; }

        public int GetAllignment() { return _allignment; }
        public bool IsActive() { return activeTimer.IsTicking(); }

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
                        ret = hurtbox.HitThisBox(_ownerID, data);
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
            //Debug.Log("Deactivating Hitbox :: " + gameObject.name);

        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {

            Gizmos.color = Color.red;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
            Gizmos.DrawCube(Vector3.zero, new Vector2((float)data.localDim.x, (float)data.localDim.y));

        }
#endif
    }
}