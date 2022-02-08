using ActionGameEngine.Interfaces;
using UnityEngine;

namespace ActionGameEngine
{
    //for grounded or wall collision, alternatively this can be used to check for a player or other object
    public class EnvironmentDetector : TriggerDetector
    {
        private ICollideable colObj;
        private bool _prevStatus;

        protected override void OnStart()
        {
            base.OnStart();
            this._prevStatus = false;
            colObj = transform.parent.parent.gameObject.GetComponent<ActionGameEngine.ActionCharacterController>();
            //Debug.Log(transform.parent.parent.gameObject);
        }

        protected override void OnEnterTrigger(GameObject other)
        {
            //Debug.Log("collided -- " + this.name);

            //Debug.Log(other.name);
            colObj.TriggerCollided(this);
        }


        protected override void OnExitTrigger(GameObject other)
        {
            colObj.TriggerExitCollided(this);
        }
    }
}