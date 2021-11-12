using UnityEngine;
using FixMath.NET;
using VelcroPhysics.Dynamics;

public class ParentConstraint
{

    private Body parent;

    private Body child;

    public FVector2 childOffset;
    public Fix64 childRotation;

    public ParentConstraint(Body parent, Body child, FVector2 childOffset, Fix64 childRot)
    {
        this.parent = parent;
        this.child = child;

        this.childOffset = childOffset;
        this.childRotation = childRot;
    }

    public void ParentUpdate()
    {
        FVector2 newPos = childOffset + parent.Position;
        Fix64 newRot = childRotation + parent.Rotation;
        child.SetVTransformIgnoreContacts(ref newPos, newRot);
    }
}
