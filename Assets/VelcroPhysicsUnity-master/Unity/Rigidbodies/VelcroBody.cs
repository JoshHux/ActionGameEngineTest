using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

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
    [SerializeField] protected float _mass = 0f;
    [SerializeField] protected bool IsTrigger = false;
    [SerializeField] protected bool IsKinematic = false;
    [SerializeField] protected bool IsStatic = false;
    [SerializeField] protected bool lockRotation = false;
    public Body parent;

    public Vector2 Velocity { get { return _rb.LinearVelocity; } set { _rb.LinearVelocity = value; } }
    public Vector2 Position { get { return _rb.Position; } set { _rb.SetVTransformIgnoreContacts(ref value, 0f); } }
    public Vector2 LocalPosition
    {
        get
        {
            if (_rb.constraint == null)
            {
                return _rb.constraint.childOffset;
            }
            return Vector2.zero;
        }
        set
        {
            _rb.constraint.childOffset = value;
        }
    }

    public float Mass { get { return _mass; } set { _mass = value; } }

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
            _rb._invI = 0f;
        }
        _rb.IsSensor = IsTrigger;
    }

    void Update()
    {
        if (_rb != null)
        {
            //this.transform.localPosition = this.LocalPosition;
            this.transform.position = _rb.Position;
            this.transform.rotation = Quaternion.Euler(0f, 0f, _rb.Rotation * Mathf.Rad2Deg);
        }
    }

    public Body GetBody() { return _rb; }

    protected abstract void InstantiateBody(BodyType type, World world);
    protected abstract void AssignTransform(Vector2 size);




}
