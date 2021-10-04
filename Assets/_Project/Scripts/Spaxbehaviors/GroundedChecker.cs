using UnityEngine;
using Spax;
using BEPUUnity;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionTests;
public class GroundedChecker : SpaxBehavior
{

    private BEPUSphere triggerCollider;
    [SerializeField] private FighterController player;

    //number of colliders the trigger is in, useful in OnTriggerExit2D
    [SerializeField] private int triggeredWith;
    // Start is called before the first frame update
    protected override void OnStart()
    {
        triggerCollider = GetComponent<BEPUSphere>();
        player = GetComponentInParent<FighterController>();

        triggeredWith = 0;
        triggerCollider.GetEntity().CollisionInformation.Events.ContactCreated += OnBepuTriggerEnter;
        triggerCollider.GetEntity().CollisionInformation.Events.RemovingContact += OnBepuTriggerExit;
    }

    private void OnBepuTriggerEnter(EntityCollidable sender, Collidable other, CollidablePairHandler pair, ContactData contact)
    {
        GameObject hold = other.gameObject;


        //other object is in platform layer
        if (hold.layer == (6))
        {
            //Debug.Log("entering");

            //Debug.Log(this.gameObject.name + " collided with:: " + hold.name + " " + hold.layer);
            //one additional collider that is colliding with
            //Debug.Log("triggered by :: " + c.gameObject.name);
            triggeredWith += 1;
            player?.OnGrounded();
        }
    }

    void OnFixedCollisionStay(int c)
    {
        //  Debug.Log("staying with :: " + c.gameObject.name);
    }
    void OnBepuTriggerExit(EntityCollidable sender, Collidable other, CollidablePairHandler pair, Contact contact)
    {
        GameObject hold = other.gameObject;

        //Debug.Log("exited");

        //object is in platform layer
        if (hold.layer == (6))
        {
            //a collider has exited the trigger
            triggeredWith -= 1;

            //prevents a scenario where you exit a trigger right into another trigger
            if (triggeredWith == 0)
            {
                player?.OnNonGrounded();
            }
        }
    }

}