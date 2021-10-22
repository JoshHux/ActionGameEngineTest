using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Spax.StateMachine;
using BEPUUnity;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionTests;
using BEPUutilities;

namespace Spax
{
    public class Hitbox : FixedBehavior
    {

        //[SerializeField] public ShapeBase ShapeBase;
        [SerializeField] private BEPUSphere bepuCollider;
        //[SerializeField] private Fix64BoxbepuCollider ShapeBase;

        [SerializeField] private bool isActive;
        //currently colliding with, but the parent gameobject, to prevent multi-hits when we don't want them
        [SerializeField] private List<GameObject> curCollidingGo;
        //currently colliding with
        [SerializeField] private List<ShapeBase> curColliding;
        //previously collided with
        [SerializeField] private List<ShapeBase> wasColliding;
        //what gets queried, difference between the two previous lists
        [SerializeField] private List<ShapeBase> diffbepuColliders;

        [SerializeField] private FrameTimer timer;

        //if negative, then the box can hurt people on its team
        //the value is the team it is on
        [SerializeField] private int playerIndex = 0;
        private int facing;


        [SerializeField] private HitBoxData data;
        private FighterController player;

        private HitBoxData emptyData;

        // Start is called before the first frame update
        protected override void OnStart()
        {
            //ShapeBase = this.GetComponent<ShapeBase>();

            bepuCollider = this.GetComponent<BEPUSphere>();
            //ShapeBase = this.GetComponent<Fix64BoxbepuCollider>();
            //ShapeBase.position=new BepuVector3(0,1,5);
            player = transform.parent.GetComponentInParent<FighterController>();
            timer = new FrameTimer();
            timer.onEnd += DeactivateBox;
            playerIndex = (player != null) ? player.playerID : 0;
            emptyData = new HitBoxData();
            bepuCollider.GetEntity().CollisionInformation.Events.ContactCreated += OnBepuTriggerEnter;

        }

        protected override void PostUpdate()
        {
            if (player != null && !player.IsInHitstop())
            {
                //Debug.Log("hit :: " + ShapeBase.tsParent.localRotation);

                //i++;
                //the time is ticking if the hitbox is active
                if (timer.TickTimer())
                {
                    //Debug.Log("hit :: "+ShapeBase.Center);
                    //corrects position of hurtbox when rotating
                    //ShapeBase.Center = BepuVector3.Transform(data.offset, Fix64Matrix.CreateFromQuaternion(ShapeBase.tsParent.rotation));
                }
            }
        }

        public int QueryCollisions()
        {

            //Debug.Log("hit :: " + ShapeBase.tsParent.localRotation);

            //i++;
            //the time is ticking if the hitbox is active


            //Debug.Log("hit :: "+ShapeBase.Center);
            //corrects position of hurtbox when rotating
            //ShapeBase.Center = BepuVector3.Transform(data.offset, Fix64Matrix.CreateFromQuaternion(ShapeBase.tsParent.rotation));

            //ShapeBase._body.Orientation = (Fix64Matrix.CreateFromQuaternion(ShapeBase.tsParent.rotation));
            //ShapeBase.localRotation=Fix64Quaternion.identity;
            //Debug.Log("active :: " + BepuVector3.Transform(new BepuVector3(data.offset.X, data.offset.Y, data.offset.Z), Fix64Matrix.CreateFromQuaternion(ShapeBase.tsParent.localRotation)) + " || forwards :: " + ShapeBase.tsParent.forward);
            //query collisions if the hitbox is supposed to be active
            //basically a hitscan, the cast is off of world position, not local position
            //bepuCollider.Center = new BepuVector3(ShapeBase.position.x + data.offset.X, ShapeBase.position.y + data.offset.Y, ShapeBase.position.z + data.offset.Z);
            //ShapeBase.Center = data.offset;


            //gets the new bepuColliders to collide with
            diffbepuColliders = curColliding.Except(wasColliding).ToList();
            //remember what we WERE colliding with
            wasColliding = curColliding.ToList();
            int len = diffbepuColliders.Count;

            for (int i = 0; i < len; i++)
            {
                Hurtbox box = curColliding[i].GetComponent<Hurtbox>();

                //Debug.Log("Querying  -  " + (box != null) + " " + (box.GetAllignment() != playerIndex));
                if ((box != null) && (box.GetAllignment() != playerIndex))
                {
                    int ret = box.GetsHit(data);
                    //Debug.Log("hit with hitbox :: " + gameObject.name+" "+ret);
                    return ret;
                    //  player.OnHit(data, box.GetHit());
                    //Debug.Log("found");
                }


            }



            return 0;
        }


        // public void Hit(IDamageable box)
        // {
        //     //insert dmg amount - parameter or from somewhere else
        //     Debug.Log("I hit you with " + 2000);
        //     box.GetsHit(2000);
        // }


        public void SetBoxData(HitBoxData boxData)
        {

            data = boxData;


            timer.StartTimer(data.duration);


            //bepuCollider._body.position = new BepuVector3(-data.offset.X, -data.offset.Y, -data.offset.Z);
            //ShapeBase.Body.Fix64Position =ShapeBase._body.position+ new BepuVector3(-data.offset.X, -data.offset.Y, -data.offset.Z);
            bepuCollider.localPosition = new BepuVector3(data.offset.X, data.offset.Y, data.offset.Z);
            //ShapeBase.size = new BepuVector3(data.size.X, data.size.Y, data.size.Z);
            bepuCollider.radius = data.size.Z;

            //renderer stuff
            Transform renderer = this.transform.GetChild(0);

            if (renderer != null)
            {
                if (bepuCollider.radius > 0)
                {
                    //Debug.Log("Activate Hitbox of " + gameObject.transform.root.name);

                    renderer.localScale = new Vector3((float)bepuCollider.radius * 2, (float)bepuCollider.radius * 2, (float)bepuCollider.radius * 2);
                    renderer.position = new Vector3((float)bepuCollider.position.X, (float)bepuCollider.position.Y, (float)bepuCollider.position.Z);
                    renderer.gameObject.SetActive(true);
                }
            }

            //Debug.Log(data.size.Z);
            //ShapeBase = new BepuVector3(-data.offset.X,  -data.offset.Y, -data.offset.Z);

        }

        public HitBoxData GetBoxData()
        {
            return data;
        }

        public void DeactivateBox(object sender = null)
        {


            //ShapeBase.Center = new BepuVector3(0, 0, 0);
            //bepuCollider.radius = 1;

            //ShapeBase.size = new BepuVector3(0, 0, 0);
            //refreshes list to prepare for collision queries
            curCollidingGo.Clear();
            curColliding.Clear();
            wasColliding.Clear();
            diffbepuColliders.Clear();

            //renderer stuff
            Transform renderer = this.transform.GetChild(0);

            bepuCollider.localPosition = BepuVector3.Zero;

            if (renderer != null)
            {
                renderer.gameObject.SetActive(false);
                renderer.localScale = new Vector3(0f, 0f, 0f);
                renderer.localPosition = Vector3.zero;


            }

            if (timer.IsTicking())
            {
                timer.ForceTimerEnd();
            }
            //Debug.Log("Deactivating Hitbox :: " + gameObject.name);
        }

        public bool IsActiveBox()
        {
            return timer.IsTicking();
        }

        private void OnBepuTriggerEnter(EntityCollidable sender, Collidable other, CollidablePairHandler pair, ContactData contact)
        {
            if (timer.IsTicking())
            {
                //for some reason, it seems to be colliding with a box that doesn't have an entity, which makes no sense
                //I think it's colliding with the player, so I made a guideline, hit/hurt-boxes are spheres, environmental objects are boxes 
                if (!(pair is BoxSpherePairHandler))
                {


                    GameObject hold = other.gameObject;
                    //making sure we always get the other gameobject
                    //likely unecessary, but safe than sorry
                    if (hold == this.gameObject)
                    {
                        Debug.Log("swapping");
                        //Debug.Log("bef" + hold);
                        hold = sender.gameObject;
                        //Debug.Log("aft" + hold);
                    }
                    //Debug.Log("exited");

                    //is on the hitbox/hurbox collision layer
                    if (hold.layer == 7 || hold.layer == 8)
                    {
                        //a bepuCollider is overlapping with the trigger

                        //Debug.Log("active hit " + hold.transform.parent.parent.gameObject + " , " + hold + " -- " + this.transform.parent.parent.gameObject + " , " + this.gameObject);
                        //root object are different
                        if (hold.transform.parent.parent != this.transform.parent.parent)
                        {
                            //Debug.Log("root difference");
                            ShapeBase newCol = hold.GetComponent<ShapeBase>();
                            if (!curColliding.Contains(newCol) && !curCollidingGo.Contains(hold.transform.parent.gameObject))
                            {
                                //Debug.Log("overlapping - " + player.gameObject.name);
                                curColliding.Add(newCol);
                                curCollidingGo.Add(hold.transform.parent.gameObject);
                            }
                        }
                    }
                }
            }
        }


    }
}
