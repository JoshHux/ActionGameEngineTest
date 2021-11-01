using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

public abstract class VelcroBody : MonoBehaviour
{

    protected Body rb;

    protected Body parent;




    [SerializeField] protected float _mass = 0f;
    [SerializeField] protected bool IsTrigger = false;
    [SerializeField] protected bool IsKinematic = false;
    [SerializeField] protected bool IsStatic = false;
    [SerializeField] protected bool lockRotation = false;

    public Vector2 Velocity { get { return rb.LinearVelocity; } set { rb.LinearVelocity = value; } }
    public Vector2 Position { get { return rb.Position; } set { rb.SetVTransformIgnoreContacts(ref value, 0f); } }
    public Vector2 LocalPosition
    {
        get
        {
            if (parent == null)
            {
                return rb.Position;
            }
            return rb.Position - parent.Position;
        }
        set
        {
            Vector2 val = value + parent.Position;
            rb.SetVTransformIgnoreContacts(ref val, 0f);
        }
    }

    public float Mass { get { return _mass; } set { _mass = value; } }

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
            rb._invI = 0f;
        }
        rb.IsSensor = IsTrigger;

        if (parent != null)
        {
            VJointFactory.CreateWeldVJoint(world, parent, rb, Vector2.zero, Vector2.zero);
        }
    }

    void Update()
    {
        this.transform.localPosition = this.LocalPosition;
        this.transform.position = rb.Position;
        this.transform.rotation = Quaternion.Euler(0f, 0f, rb.Rotation * Mathf.Rad2Deg);
    }

    public Body GetBody() { return rb; }

    protected abstract void InstantiateBody(BodyType type, World world);
    protected abstract void AssignTransform(Vector2 size);




}
