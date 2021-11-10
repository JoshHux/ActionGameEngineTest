using System.Collections.Generic;
using FixMath.NET;
using UnityEngine;
using FixMath.NET;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;

public class VelcroWorldManager2D
{
    public static VelcroWorldManager2D instance;

    private World world;

    private Dictionary<Body, GameObject> goBodDict;

    public void Initialize()
    {
        instance = this;
        if (world == null)
        {
            world = new World(new FVector2(0f, 0f));
        }
        else
        {
            world.Clear();
        }

        goBodDict = new Dictionary<Body, GameObject>();

        world.ContactManager.BeginContact += CollisionEnter;
        world.ContactManager.EndContact += CollisionExit;

    }

    public World GetWorld() { return world; }
    public void Step() { world.Step(Time.fixedDeltaTime); }
    public void AddBody(VelcroBody newBody)
    {
        Body body = newBody.GetBody();
        //not null, return, already added
        if (body != null) { return; }

        newBody.Initialize(world);
        body = newBody.GetBody();
        goBodDict.Add(body, newBody.gameObject);
        if (newBody.transform.transform.parent != null && newBody.transform.parent.GetComponentInParent<VelcroBody>() != null)
        {
            VelcroBody parentGo = newBody.transform.parent.GetComponentInParent<VelcroBody>();
            Body parent = parentGo.Body;
            newBody.parent = parent;

            body.constraint = new ParentConstraint(parent, body, (newBody.transform.position - parentGo.transform.position), (body.Rotation - parent.Rotation));
        }
        world.ProcessChanges();
    }
    public void RemoveBody(VelcroBody newBody)
    {
        Body body = newBody.GetBody();
        if (goBodDict.ContainsKey(body)) { goBodDict.Remove(body); }
        world.RemoveBody(body);

        world.ProcessChanges();
    }


    private bool CollisionEnter(Contact contact)
    {
        if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor)
        {
            TriggerEnter(contact);
        }
        else
        {
            CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, contact, "OnVelcroCollisionEnter");
        }

        return true;
    }
    private void CollisionExit(Contact contact)
    {

        if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor)
        {
            TriggerExit(contact);
        }
        else
        {
            CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, contact, "OnVelcroCollisionExit");
        }
    }

    private void TriggerEnter(Contact contact) { CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, contact, "OnVelcroTriggerEnter"); }
    private void TriggerExit(Contact contact) { CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, contact, "OnVelcroTriggerExit"); }

    private void CollisionDetected(Body body1, Body body2, Contact contact, string callbackName)
    {
        if (!goBodDict.ContainsKey(body1) || !goBodDict.ContainsKey(body2))
        { return; }

        GameObject b1 = goBodDict[body1];
        GameObject b2 = goBodDict[body2];

        if (b1 == null || b2 == null)
        {
            return;
        }

        b1.SendMessage(callbackName, GetCollisionInfo(body2, contact), SendMessageOptions.DontRequireReceiver);
        b2.SendMessage(callbackName, GetCollisionInfo(body1, contact), SendMessageOptions.DontRequireReceiver);

        //FixedPointManager.UpdateCoroutines();
    }

    private VelcroCollision GetCollisionInfo(Body other, Contact contact)
    {
        VelcroCollision ret = new VelcroCollision();

        ret.gameObject = goBodDict[other];
        ret.collider = ret.gameObject.GetComponent<VelcroBody>();
        ret.c = contact;

        ret.midPoint = contact.Manifold.LocalPoint;

        return ret;
    }
}
