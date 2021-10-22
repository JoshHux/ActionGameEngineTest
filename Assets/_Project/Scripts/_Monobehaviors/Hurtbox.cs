using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BEPUUnity;
using BEPUutilities;
using Spax.StateMachine;
using UnityEngine;

namespace Spax
{
    public class Hurtbox : FixedBehavior
    {
        [SerializeField]
        private BEPUSphere rb;

        [SerializeField]
        private FrameTimer timer;

        //if negative, then the box can hurt people on its team
        //the value is the team it is on
        [SerializeField]
        private int playerIndex = -1;

        [SerializeField]
        private HurtBoxData data;

        private FighterController player;

        //public Hitbox hitbox;
        //public int i = 0;
        // Start is called before the first frame update
        protected override void OnStart()
        {
            rb = this.GetComponent<BEPUSphere>();

            //ShapeBase.position=new BepuVector3(0,1,5);
            player = transform.parent.GetComponentInParent<FighterController>();
            timer = new FrameTimer();
            playerIndex = player.playerID;
        }

        // Update is called once per frame
        protected override void PostUpdate()
        {
            //i++;
            /*if (hitbox != null)
                if (hitbox.ShapeBase.localPosition!=(this.ShapeBase.localPosition))
                {
                    //Debug.Log("i+ ::"+i+"\n hurtbox :: " + ShapeBase.localPosition + "\n hitbox :: " + hitbox.ShapeBase.localPosition);

                }*/
            //Debug.Log("hurt :: " + ShapeBase.tsParent.localRotation);
            //i++;
            //the time is ticking if the hitbox is active
            //Debug.Log(timer.IsTicking() + " || " + i);
            if (!timer.TickTimer())
            {
                //Debug.Log("hurt :: "+ShapeBase.Center);
                //corrects position of hurtbox when rotating
                //ShapeBase.Center = BepuVector3.Transform(data.offset, Fix64Matrix.CreateFromQuaternion(ShapeBase.tsParent.rotation));
            }
        }

        public void SetBoxData(HurtBoxData boxData)
        {
            data = boxData;

            //            Debug.Log("Activate Hurtbox of " + gameObject.transform.root.name);
            //timer.StartTimer(data.duration);
            //collider._body.position = new BepuVector3(-data.offset.X, -data.offset.Y, -data.offset.Z);
            //ShapeBase.Body.Fix64Position =ShapeBase._body.position+ new BepuVector3(-data.offset.X, -data.offset.Y, -data.offset.Z);
            rb.localPosition =
                new BepuVector3(data.offset.X, data.offset.Y, data.offset.Z);
            rb.radius = data.size.Z;

            Transform renderer = this.transform.GetChild(0);

            if (renderer != null)
            {
                if (rb.radius > 0)
                {
                    //Debug.Log("assigned");
                    renderer.localScale =
                        new Vector3((float)rb.radius * 2,
                            (float)rb.radius * 2,
                            (float)rb.radius * 2);
                    renderer.position =
                        new Vector3((float)rb.position.X,
                            (float)rb.position.Y,
                            (float)rb.position.Z);
                }
            }
            //ShapeBase.size = new BepuVector3(data.size.X, data.size.Y, data.size.Z);
            //ShapeBase = new BepuVector3(-data.offset.X,  -data.offset.Y, -data.offset.Z);
        }

        public void DeactivateBox()
        {
        }

        public int GetsHit(HitBoxData hitData)
        {
            if (player)
                return player.GetHit(hitData);
            else
            {
                Debug.Log("Enemy got Hit!");
            }
            return 0;
        }

        public int GetAllignment()
        {
            return playerIndex;
        }

        public bool IsActiveBox()
        {
            return !timer.IsTicking();
        }

        public virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
        }
    }
}
