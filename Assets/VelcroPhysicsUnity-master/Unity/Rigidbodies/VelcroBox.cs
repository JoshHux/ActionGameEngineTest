using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Collision;
using FixMath.NET;

public class VelcroBox : VelcroBody
{
    [SerializeField] private Fix64 _height;
    [SerializeField] private Fix64 _width;
    public Fix64 Height
    {
        get { return _height; }
        set
        {
            _height = value;
            this.AssignTransform(new FVector2(_width, _height));
        }
    }

    public Fix64 Width
    {
        get { return _width; }
        set
        {
            _width = value;
            this.AssignTransform(new FVector2(_width, _height));
        }
    }

    protected override void InstantiateBody(BodyType type, World world)
    {

        _rb = BodyFactory.CreateRectangle(world, _width, _height, _mass, new FVector2((Fix64)transform.position.x, (Fix64)transform.position.y), (Fix64)transform.rotation.eulerAngles.z * FixedMath.Deg2Rad, type);
        //VelcroWorld.instance.world.AddBody(rb);
    }

    protected override void AssignTransform(FVector2 size)
    {
        FixtureFactory.AttachRectangle(size.x, size.y, _mass, FVector2.zero, _rb);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = (!IsKinematic && !IsStatic) ? Color.green : Color.red;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
        Gizmos.DrawWireCube(Vector3.zero, new Vector2((float)_width, (float)_height));
    }
}
