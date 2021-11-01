using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Collision;

public class VelcroBox : VelcroBody
{
    [SerializeField] private float _height;
    [SerializeField] private float _width;
    public float Height
    {
        get { return _height; }
        set
        {
            _height = value;
            this.AssignTransform(new Vector2(_width, _height));
        }
    }

    public float Width
    {
        get { return _width; }
        set
        {
            _width = value;
            this.AssignTransform(new Vector2(_width, _height));
        }
    }

    protected override void InstantiateBody(BodyType type, World world)
    {

        _rb = BodyFactory.CreateRectangle(world, _width, _height, _mass, new Vector2(transform.position.x, transform.position.y), transform.rotation.eulerAngles.z, type);
        //VelcroWorld.instance.world.AddBody(rb);
    }

    protected override void AssignTransform(Vector2 size)
    {
        FixtureFactory.AttachRectangle(size.x, size.y, _mass, Vector2.zero, _rb);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = (!IsKinematic && !IsStatic) ? Color.green : Color.red;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
        Gizmos.DrawWireCube(Vector3.zero, new Vector2(_width, _height));
    }
}
