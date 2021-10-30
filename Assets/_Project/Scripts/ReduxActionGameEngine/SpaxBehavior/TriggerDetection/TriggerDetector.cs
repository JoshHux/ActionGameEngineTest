using UnityEngine;
using Spax;

namespace ActionGameEngine
{
    public abstract class TriggerDetector : SpaxBehavior
    {

        /*
        protected ShapeBase trigger;
        //number of colliders the trigger is in, useful in OnTriggerExit2D
        protected int triggeredWith;

        // Start is called before the first frame update
        protected override void OnStart()
        {
            trigger = this.GetComponent<ShapeBase>();
            triggeredWith = 0;
            trigger.GetEntity().CollisionInformation.Events.ContactCreated += OnBepuTriggerEnter;
            trigger.GetEntity().CollisionInformation.Events.RemovingContact += OnBepuTriggerExit;
        
        }

        //is called when the trigger collides with something
        private void OnBepuTriggerEnter(EntityCollidable sender, Collidable other, CollidablePairHandler pair, ContactData contact)
        {
            GameObject hold = other.gameObject;


            //other object is in correct layer
            if (Physics.GetIgnoreLayerCollision(hold.layer, this.gameObject.layer))
            {
                //Debug.Log("entering");

                //Debug.Log(this.gameObject.name + " collided with:: " + hold.name + " " + hold.layer);
                //one additional collider that is colliding with
                //Debug.Log("triggered by :: " + c.gameObject.name);
                triggeredWith += 1;

                OnEnterTrigger(hold);
            }
        }
        //is called when the trigger exits collision with something
        private void OnBepuTriggerExit(EntityCollidable sender, Collidable other, CollidablePairHandler pair, Contact contact)
        {

            GameObject hold = other.gameObject;

            //Debug.Log("exited");

            //object is in platform layer
            if (Physics.GetIgnoreLayerCollision(hold.layer, this.gameObject.layer))
            {
                //a collider has exited the trigger
                triggeredWith -= 1;

                //prevents a scenario where you exit a trigger right into another trigger
                if (triggeredWith == 0)
                {
                    OnExitTrigger(hold);
                }
            }
        }
*/
        //what to do when trigger enters collision
        protected abstract void OnExitTrigger(GameObject other);
        //what to do when trigger exits collision
        protected abstract void OnEnterTrigger(GameObject other);

    }
}