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
    public FVector2 Position { get { return _rb.Position; } set { _rb.SetVTransformIgnoreContacts(ref value, 0); } }
    public FVector2 LocalPosition
    {
        get
        {
            if (_rb.constraint == null)
            {
                return _rb.constraint.childOffset;
            }
            return FVector2.zero;
        }
        set
        {
            _rb.constraint.childOffset = value;
        }
    }

    public Fix64 Mass { get { return _mass; } set { _mass = value; } }

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

        if (lockRotation)
        {
            _rb._invI = 0;
        }
        _rb.IsSensor = IsTrigger;

        //set the collision layer
        Category layer = (Category)(1 << (this.gameObject.layer));
        _rb.CollisionCategories = layer;
        _rb.IgnoreCCDWith = (Category)VelcroWorld.instance.GetCollisions(this.gameObject.layer);
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

    public Body GetBody() { return _rb; }

    protected abstract void InstantiateBody(BodyType type, World world);
    protected abstract void AssignTransform(FVector2 size);




}
