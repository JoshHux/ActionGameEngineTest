using UnityEngine;
using FlatPhysics.Unity;
using FlatPhysics.Contact;
using Spax;

namespace ActionGameEngine
{
    public abstract class TriggerDetector : SpaxBehavior
    {


        protected FRigidbody trigger;
        //number of colliders the trigger is in, useful in OnTriggerExit2D
        protected int triggeredWith;

        // Start is called before the first frame update
        protected override void OnStart()
        {
            trigger = this.GetComponent<FRigidbody>();
            triggeredWith = 0;
            //Debug.Log("called");
            //trigger.GetEntity().CollisionInformation.Events.ContactCreated += OnBepuTriggerEnter;
            //trigger.GetEntity().CollisionInformation.Events.RemovingContact += OnBepuTriggerExit;

            trigger.Body.OnOverlap += (c) => OnVelcroTriggerEnter(c);

        }

        //is called when the trigger collides with something
        private void OnVelcroTriggerEnter(ContactData c)
        {
            GameObject hold = c.other;
            //Debug.Log(trigger.Body.FixtureList[0]._collisionCategories + " " + this.gameObject.name + " triggered with : " + hold.name + ", " + c.collider.Body.FixtureList[0]._collisionCategories);

            //Debug.Log("entering");

            //Debug.Log(this.gameObject.name + " collided with:: " + hold.name + " " + hold.layer);
            //one additional collider that is colliding with
            //Debug.Log("triggered by :: " + c.gameObject.name);
            //triggeredWith += 1;

            OnEnterTrigger(hold);

        }
        //is called when the trigger exits collision with something
        private void OnVelcroTriggerExit(ContactData c)
        {
            GameObject hold = c.other;
            //Debug.Log("exited");
            //a collider has exited the trigger
            //triggeredWith -= 1;

            //prevents a scenario where you exit a trigger right into another trigger
            if (triggeredWith == 0)
            {
                OnExitTrigger(hold);
            }

        }

        //what to do when trigger enters collision
        protected abstract void OnExitTrigger(GameObject other);
        //what to do when trigger exits collision
        protected abstract void OnEnterTrigger(GameObject other);

    }
}