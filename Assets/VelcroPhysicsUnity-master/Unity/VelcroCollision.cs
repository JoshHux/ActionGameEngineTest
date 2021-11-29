using UnityEngine;
using FixMath.NET;
using VelcroPhysics.Collision.ContactSystem;

public class VelcroCollision
{
    public VelcroBody collider;
    //other gameobject the collider collided with
    public GameObject gameObject;
    public FVector2 midPoint;

    public Contact c;

}
