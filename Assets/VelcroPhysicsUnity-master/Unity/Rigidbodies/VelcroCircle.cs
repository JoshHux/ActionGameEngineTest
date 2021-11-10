using UnityEngine;
using FixMath.NET;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

public class VelcroCircle : VelcroBody
{
    [SerializeField] private Fix64 _radius;
    public Fix64 Radius
    {
        get { return _radius; }
        set
        {
            _radius = value;
            this.AssignTransform(new FVector2(_radius, 0f));
        }
    }


    protected override void InstantiateBody(BodyType type, World world)
    {
        _rb = BodyFactory.CreateCircle(world, _radius, _mass, new FVector2(transform.position.x, transform.position.y), type);
        //VelcroWorld.instance.world.AddBody(rb);
    }

    protected override void AssignTransform(FVector2 size)
    {
        FixtureFactory.AttachCircle(size.x, _mass, _rb);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = (!IsKinematic && !IsStatic) ? Color.green : Color.red;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
        Gizmos.DrawWireSphere(FVector2.zero, _radius);
    }
}
