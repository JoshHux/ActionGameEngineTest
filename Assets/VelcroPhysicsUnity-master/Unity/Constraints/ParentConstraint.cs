using UnityEngine;
using FixMath.NET;
using VelcroPhysics.Dynamics;

public class ParentConstraint
{

    private Body parent;

    private Body child;
    private bool clearContacts = false;

    private FVector2 _childOffset;
    public FVector2 childOffset
    {
        get { return _childOffset; }
        set
        {
            _childOffset = value;
            clearContacts = true;
            //ParentUpdate();
        }
    }
    public Fix64 childRotation;

    public ParentConstraint(Body parent, Body child, FVector2 childOffset, Fix64 childRot)
    {
        this.parent = parent;
        this.child = child;

        this._childOffset = childOffset;
        this.childRotation = childRot;
    }

    public void ParentUpdate()
    {
        FVector2 newPos = _childOffset + parent.Position;
        Fix64 newRot = childRotation + parent.Rotation;
        child.SetVTransform(ref newPos, newRot, clearContacts);
        clearContacts = false;
    }
}
