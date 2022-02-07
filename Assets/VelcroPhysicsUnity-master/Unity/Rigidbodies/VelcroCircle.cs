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
            this.AssignTransform(new FVector2(_radius, 0));
        }
    }


    protected override void InstantiateBody(BodyType type, World world)
    {
        _rb = BodyFactory.CreateCircle(world, _radius, _mass, new FVector2((Fix64)transform.position.x, (Fix64)transform.position.y), type);
        //VelcroWorld.instance.world.AddBody(rb);
    }

    protected override void AssignTransform(FVector2 size)
    {
        FixtureFactory.AttachCircle(size.x, _mass, _rb);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = (!IsKinematic && !IsStatic) ? Color.green : Color.red;


        if (Application.isPlaying)
        {
            Vector3 pos = new Vector3((float)this._rb.Position.x, (float)this._rb.Position.y, 0f);
            Gizmos.matrix = Matrix4x4.TRS(pos, transform.rotation, Vector3.one);
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
            Gizmos.DrawWireSphere(Vector2.zero, (float)_radius);
        }
        else
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
            Gizmos.DrawWireSphere(Vector2.zero, (float)_radius);
        }
    }
}
