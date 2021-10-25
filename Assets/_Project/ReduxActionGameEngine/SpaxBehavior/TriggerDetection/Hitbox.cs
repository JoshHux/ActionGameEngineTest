using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BEPUUnity;
using BEPUutilities;
using ActionGameEngine.Data;
using ActionGameEngine.Enum;


namespace ActionGameEngine.Gameplay
{
    public class Hitbox : TriggerDetector
    {
        //record from CombatObject
        private int combatID = 0;
        private int allignment = 0;
        private CallbackTimer activeTimer;
        private HitboxData data;
        //currently colliding with, but the parent gameobject, to prevent multi-hits when we don't want them
        [SerializeField] private List<GameObject> curCollidingGo;
        //currently colliding with
        [SerializeField] private List<ShapeBase> curColliding;
        //previously collided with
        [SerializeField] private List<ShapeBase> wasColliding;
        //what gets queried, difference between the two previous lists
        [SerializeField] private List<ShapeBase> diffbepuColliders;

        protected override void OnAwake()
        {
            base.OnAwake();
            activeTimer = new CallbackTimer();
            curCollidingGo = new List<GameObject>();
            curColliding = new List<ShapeBase>();
            wasColliding = new List<ShapeBase>();
            diffbepuColliders = new List<ShapeBase>();

            activeTimer.OnEnd += DeactivateBox;
        }

        protected override void OnStart()
        {
            base.OnStart();
            CombatObject root = this.transform.parent.parent.gameObject.GetComponent<CombatObject>();
            combatID = root.GetCombatID();
            allignment = root.GetAllignment();
        }

        protected override void OnEnterTrigger(GameObject other)
        {
            if (activeTimer.IsTicking())
            {
                //root object are different
                if (other.transform.parent.parent != this.transform.parent.parent)
                {
                    //Debug.Log("root difference");
                    ShapeBase newCol = other.GetComponent<ShapeBase>();
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
            trigger.localPosition = data.localPos;
            trigger.localRotation = new BepuQuaternion(data.localRot.Z, data.localRot.Y, data.localRot.Z, trigger.localRotation.W);

            activeTimer.StartTimer(data.duration);
        }

        public int GetAllignment() { return allignment; }
        public bool IsActiveBox() { return activeTimer.IsTicking(); }

        public HitboxData GetHitboxData() { return data; }

        public HitIndicator QueryHitboxCollisions()
        {
            HitIndicator ret = 0;
            //gets the new bepuColliders to collide with
            diffbepuColliders = curColliding.Except(wasColliding).ToList();
            //remember what we WERE colliding with
            wasColliding = curColliding.ToList();
            int len = diffbepuColliders.Count;
            bool clash = true;

            for (int i = 0; i < len; i++)
            {
                Hitbox hitbox;
                Hurtbox hurtbox;
                if (curColliding[i].TryGetComponent<Hitbox>(out hitbox))
                {

                    //Debug.Log("Querying  -  " + (box != null) + " " + (box.GetAllignment() != playerIndex));
                    if ((hitbox != null) && (hitbox.GetAllignment() != allignment) && clash)
                    {

                        //TODO: return what happens when you clash with another hitbox

                        //ret = hitbox.HitThisBox(combatID, data);
                    }
                }
                else if (curColliding[i].TryGetComponent<Hurtbox>(out hurtbox))
                {

                    //Debug.Log("Querying  -  " + (box != null) + " " + (box.GetAllignment() != playerIndex));
                    if ((hurtbox != null) && (hurtbox.GetAllignment() != allignment))
                    {
                        clash = false;
                        ret = hurtbox.HitThisBox(combatID, data);
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
            diffbepuColliders.Clear();

            //renderer stuff
            Transform renderer = this.transform.GetChild(0);

            trigger.localPosition = BepuVector3.Zero;

            if (renderer != null)
            {
                renderer.gameObject.SetActive(false);
                renderer.localScale = new Vector3(0f, 0f, 0f);
                renderer.localPosition = Vector3.zero;
            }

            if (activeTimer.IsTicking())
            {
                activeTimer.EndTimer();
            }
            //Debug.Log("Deactivating Hitbox :: " + gameObject.name);
        }
    }
}