namespace VelcroPhysics.Dynamics.VJoints
{
    public enum VJointType
    {
        Unknown,
        Revolute,
        Prismatic,
        Distance,
        Pulley,

        //Mouse, <- We have fixed mouse
        Gear,
        Wheel,
        Weld,
        Friction,
        Rope,
        Motor,

        //Velcro note: From here on and down, it is only FPE VJoints
        Angle,
        FixedMouse,
        FixedRevolute,
        FixedDistance,
        FixedLine,
        FixedPrismatic,
        FixedAngle,
        FixedFriction
    }
}