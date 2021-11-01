using UnityEngine;
using VelcroPhysics.Dynamics;

public class ParentConstraint
{

    private Body parent;

    private Body child;

    public Vector2 childOffset;
    public float childRotation;

    public ParentConstraint(Body parent, Body child, Vector2 childOffset, float childRot)
    {
        this.parent = parent;
        this.child = child;

        this.childOffset = childOffset;
        this.childRotation = childRot;
    }

    public void ParentUpdate()
    {
        Vector2 newPos = childOffset + parent.Position;
        float newRot = childRotation + parent.Rotation;
        child.SetVTransformIgnoreContacts(ref newPos, newRot);
    }
}
