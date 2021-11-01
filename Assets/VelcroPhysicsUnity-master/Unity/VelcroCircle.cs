using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

public class VelcroCircle : VelcroBody
{
    [SerializeField] private float _radius;
    public float Radius
    {
        get { return _radius; }
        set
        {
            _radius = value;
            this.AssignTransform(new Vector2(_radius, 0f));
        }
    }


    protected override void InstantiateBody(BodyType type, World world)
    {
        rb = BodyFactory.CreateCircle(world, _radius, _mass, new Vector2(transform.position.x, transform.position.y), 0f, type);
        //VelcroWorld.instance.world.AddBody(rb);
    }

    protected override void AssignTransform(Vector2 size)
    {
        FixtureFactory.AttachCircle(size.x, _mass, rb);
    }
}
