using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Utilities;
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

        //gets the new polygon vertices to attach to the body
        //var polyVert = PolygonUtils.CreateRectangle(this._width / 2, this._height / 2);
        //attach the new verticies to body
        //FixtureFactory.AttachPolygon(polyVert, this._mass, this._rb);

        //resolve any extra collider shenanigans
        //ResolveColliderType();
    }



    protected override void InstantiateBody(BodyType type, World world)
    {

        var rawTransRot = (Fix64)transform.rotation.eulerAngles.z * -1;

        // if (this.name == "Hitbox") { Debug.Log("addddddddddddddding"); }
        _rb = BodyFactory.CreateRectangle(world, _width, _height, _mass, new FVector2((Fix64)transform.position.x, (Fix64)transform.position.y), rawTransRot * FixedMath.Deg2Rad, type);
        //VelcroWorld.instance.world.AddBody(rb);
        //Debug.Log("fixture count:" + _rb.FixtureList.Count);

    }

    protected override void AssignTransform(FVector2 size)
    {
        FixtureFactory.AttachRectangle(size.x, size.y, _mass, FVector2.zero, _rb);
    }
    private void OnDrawGizmos()
    {
        if (this._rb != null)
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


            //else
            //{

            Vector2 hold = new Vector2((float)this._rb.Position.x, (float)this._rb.Position.y);
            Gizmos.color = (!IsKinematic && !IsStatic) ? Color.blue : Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(hold, Quaternion.Euler(0f, (float)((_rb.Rotation * FixedMath.Rad2Deg) * -1), 0f), Vector3.one);
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
            Gizmos.DrawWireCube(Vector3.zero, new Vector3((float)_width, 1f, (float)_height));

            //}
        }
        else if (this.IsStatic)
        {

            Vector3 hold = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
            Gizmos.color = (!IsKinematic && !IsStatic) ? Color.blue : Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(hold, Quaternion.Euler(transform.rotation.eulerAngles.x, (float)(transform.rotation.eulerAngles.z), transform.rotation.eulerAngles.z), Vector3.one);
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
            Gizmos.DrawWireCube(Vector3.zero, new Vector3((float)_width, 1f, (float)_height));
        }
    }
}
