using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Collision;
using FixMath.NET;

public class VelcroBox : VelcroBody
{
    [SerializeField] private Fix64 _height = 1;
    [SerializeField] private Fix64 _width = 1;
    public Fix64 Height
    {
        get { return _height; }
        set
        {
            _height = value;
            VelcroWorld.instance.AddBody(this);
        }
    }

    public Fix64 Width
    {
        get { return _width; }
        set
        {
            _width = value;
            //this.AssignTransform(new FVector2(_width, _height));
            VelcroWorld.instance.AddBody(this);
        }
    }

    public override void SetDimensions(FVector2 scale)
    {
        //VelcroWorldManager2D.instance.RemoveBody(this);
        //_rb.Enabled = false;
        //_rb.Enabled = true;
        this._width = scale.x;
        this._height = scale.y;
        //Debug.Log(gameObject.name + " rescaled");
        VelcroWorld.instance.AddBody(this);
    }



    protected override void InstantiateBody(BodyType type, World world)
    {
        // if (this.name == "Hitbox") { Debug.Log("addddddddddddddding"); }
        _rb = BodyFactory.CreateRectangle(world, _width, _height, _mass, new FVector2((Fix64)transform.position.x, (Fix64)transform.position.y), (Fix64)transform.rotation.eulerAngles.z * FixedMath.Deg2Rad, type);
        //VelcroWorld.instance.world.AddBody(rb);
        //Debug.Log("fixture count:" + _rb.FixtureList.Count);

    }

    protected override void AssignTransform(FVector2 size)
    {
        FixtureFactory.AttachRectangle(size.x, size.y, _mass, FVector2.zero, _rb);
    }
    private void OnDrawGizmos()
    {
        ActionGameEngine.Gameplay.Hitbox thing;
        ActionGameEngine.Gameplay.Hurtbox thing2;
        if (this.TryGetComponent<ActionGameEngine.Gameplay.Hitbox>(out thing))
        {
            Gizmos.color = Color.red;
        }
        else
        if (this.TryGetComponent<ActionGameEngine.Gameplay.Hurtbox>(out thing2))
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = (!IsKinematic && !IsStatic) ? Color.blue : Color.yellow;
        }
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
        Gizmos.DrawWireCube(Vector3.zero, new Vector2((float)_width, (float)_height));
    }
}
