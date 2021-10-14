namespace ActionGameEngine.Enum
{
    //hopefully self-explainatory, these are the overall characteristics that the state has
    //these will tell the character what to do or what happends while in that state
    //2 bytes
    [System.Flags]
    public enum StateCondition : ushort
    {
        GROUNDED = 1 << 0,
        AIRBORNE = 1 << 1,
        APPLY_GRAV = 1 << 2,
        APPLY_FRICTION = 1 << 3,
        INVULNERABLE = 1 << 4,
        STRIKE = INVULNERABLE | 1 << 5,
        GRAB = INVULNERABLE | 1 << 6,
        GUARD_POINT = 1 << 7,
        LOW = GUARD_POINT | 1 << 8,
        MID = GUARD_POINT | 1 << 9,
        HIGH = GUARD_POINT | 1 << 10,
        //don't look at parent transitions
        NO_PARENT_TRANS = 1 << 14,
        //don't look at parent StateCondition
        NO_PARENT_COND = 1 << 15
    }
}
