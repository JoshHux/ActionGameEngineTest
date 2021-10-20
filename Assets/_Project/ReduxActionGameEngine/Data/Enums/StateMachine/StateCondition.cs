namespace ActionGameEngine.Enum
{
    //hopefully self-explainatory, these are the overall characteristics that the state has
    //these will tell the character what to do or what happends while in that state
    //2 bytes
    [System.Flags]
    public enum StateCondition : int
    {
        GROUNDED = 1 << 0,
        AIRBORNE = 1 << 1,
        APPLY_GRAV = 1 << 2,
        APPLY_FRICTION = 1 << 3,
        STUNNED = 1 << 4,
        //for grabs, makes sure you're only hit by the hit that knocks you out of the grab
        //can also be used for cinematic supers
        STUNNED_SPECIAL = 1 << 4,
        AIR_TECHABLE = 1 << 5,
        GRAB_TECHABLE = 1 << 5,
        COUNTER_HIT = 1 << 6,
        INVULNERABLE_STRIKE = 1 << 7,
        INVULNERABLE_GRAB = 1 << 8,
        GUARD_POINT_LOW = 1 << 9,
        GUARD_POINT_MID = 1 << 10,
        GUARD_POINT_HIGH = 1 << 11,
        BOUNCE_WALL = 1 << 12,
        BOUNCE_GROUND = 1 << 13,
        //when you want a character to ignore collision and pass through another character
        PASS_THROUGH = 1 << 14,
        CAN_WALK = 1 << 15,
        CAN_RUN = 1 << 16,
        //is able to rotate to change orientation
        IS_ABLE_TO_TURN = 1 << 17,
        //character will automatically turn to face pre-defined target
        AUTO_TURN = IS_ABLE_TO_TURN | 1 << 16,
        CAN_TRANSITION_TO_SELF = 1 << 29,
        //don't look at parent transitions
        NO_PARENT_TRANS = 1 << 30,
        //don't look at parent StateCondition
        NO_PARENT_COND = 1 << 31,
        INVULNERABLE = INVULNERABLE_STRIKE | INVULNERABLE_GRAB,
        GUARD_POINT = GUARD_POINT_LOW | GUARD_POINT_MID | GUARD_POINT_HIGH,
        BOUNCE = BOUNCE_WALL | BOUNCE_GROUND,
        CAN_MOVE = CAN_WALK | CAN_RUN,
    }
}
