using UnityEngine;
using FixMath.NET;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Collision.Filtering;

public abstract class VelcroBody : MonoBehaviour
{

    protected Body _rb;

    public Body Body
    {
        get
        {

            //makes sure it's never null
            if (_rb == null)
            {

                StartPhysics();
            }

            return _rb;
        }
    }
    [SerializeField] protected Fix64 _mass = 0;
    [SerializeField] protected bool IsTrigger = false;
    [SerializeField] protected bool IsKinematic = false;
    [SerializeField] protected bool IsStatic = false;
    [SerializeField] protected bool lockRotation = false;
    public Body parent;

    public FVector2 Velocity
    {
        get { return _rb.LinearVelocity; }
        set
        {
            _rb.LinearVelocity = value;
        }
    }
    public FVector2 Position { get { return _rb.Position; } set { _rb.SetVTransform(ref value, 0); } }
    public FVector2 LocalPosition
    {
        get
        {
            if (_rb.constraint != null)
            {
                return _rb.constraint.childOffset;
            }
            return FVector2.zero;
        }
        set
        {
            _rb.constraint.childOffset = value;
            _rb.constraint.ParentUpdate();
        }
    }

    public Fix64 Mass { get { return _mass; } set { _mass = value; } }

    public bool Enabled { get { return _rb.Enabled; } set { _rb.Enabled = value; } }

    void Start()
    {
        VelcroWorld.instance.AddBody(this);

    }

    private void StartPhysics()
    {
        if ((_rb == null) && (VelcroWorldManager2D.instance != null))
        {
            VelcroWorld.instance.AddBody(this);
        }
    }

    // Start is called before the first frame update
    public void Initialize(World world)
    {
        BodyType type = BodyType.Dynamic;
        if (IsKinematic)
        {
            type = BodyType.Kinematic;
        }
        else if (IsStatic)
        {
            type = BodyType.Static;
        }

        InstantiateBody(type, world);

        ResolveColliderType();
    }

    void Update()
    {
        if (_rb != null)
        {
            //this.transform.localPosition = this.LocalPosition;
            this.transform.position = new Vector2((float)_rb.Position.x, (float)_rb.Position.y);
            this.transform.rotation = Quaternion.Euler(0f, 0, (float)(_rb.Rotation * FixedMath.Rad2Deg));
        }
    }

    public void PrepColliderType()
    {
        BodyType type = BodyType.Dynamic;
        if (IsKinematic)
        {
            type = BodyType.Kinematic;
        }
        else if (IsStatic)
        {
            type = BodyType.Static;
        }
    }

    public void ResolveColliderType()
    {
        if (lockRotation)
        {
            _rb._invI = 0;
        }
        _rb.IsSensor = IsTrigger;

        //set the collision layer
        Category layer = (Category)(1 << (this.gameObject.layer));
        _rb.CollisionCategories = layer;
        //_rb.IgnoreCCDWith = (Category)VelcroWorld.instance.GetCollisions(this.gameObject.layer);
        _rb.CollidesWith = VelcroWorld.instance.GetCollisions(this.gameObject.layer);
        //Debug.Log(this.gameObject.name + ": " + this.gameObject.layer + " results in category : " + _rb.FixtureList[0]._collisionCategories + ", collides with : " + _rb.FixtureList[0]._collidesWith);
        _rb.gameObject = this.gameObject;
    }

    public void MakeForRemoval()
    {
        VelcroWorldManager2D.instance.RemoveBody(this);
        _rb = null;
        //Debug.Log(_rb == null);
        //VelcroWorldManager2D.instance.RemoveBody(this);

    }



    public Body GetBody() { return _rb; }
    public virtual void SetDimensions(FVector2 scale) { }
    protected abstract void InstantiateBody(BodyType type, World world);
    protected abstract void AssignTransform(FVector2 size);
    public void FindNewContacts() { _rb.FindNewContacts(); }




}
