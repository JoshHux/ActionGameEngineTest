using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace ActionGameEngine.Enum
{
    //hopefully self-explainatory, these are the overall characteristics that the state has
    //these will tell the character what to do or what happends while in that state
    //2 bytes
    [System.Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StateCondition : uint
    {
        GROUNDED = 1 << 0,
        AIRBORNE = 1 << 1,
        APPLY_GRAV = 1 << 2,
        APPLY_FRICTION = 1 << 3,
        STUNNED = 1 << 4,
        //for grabs, makes sure you're only hit by the hit that knocks you out of the grab
        //can also be used for cinematic supers
        STUNNED_SPECIAL = 1 << 5,
        //stunned and are on the ground
        STUNNED_OTG = 1 << 6,
        //whether or not we can tech
        AIR_UNTECHABLE = 1 << 7,
        GRAB_TECHABLE = 1 << 8,
        COUNTER_HIT = 1 << 9,
        INVULNERABLE_STRIKE = 1 << 10,
        INVULNERABLE_GRAB = 1 << 11,
        GUARD_POINT_LOW = 1 << 12,
        GUARD_POINT_MID = 1 << 13,
        GUARD_POINT_HIGH = 1 << 14,
        //if we can parry or not
        PARRY = 1 << 15,
        BOUNCE_WALL_MEDIUM = 1 << 16,
        BOUNCE_WALL_WEAK = 1 << 17,
        //when you want a character to ignore collision and pass through another character
        PASS_THROUGH = 1 << 18,
        CAN_WALK = 1 << 19,
        CAN_RUN = 1 << 20,
        //is able to rotate to change orientation
        IS_ABLE_TO_TURN = 1 << 21,
        CROUCHING = 1 << 22,
        BOUNCE_GROUND_MEDIUM = 1 << 23,
        BOUNCE_GROUND_WEAK = 1 << 24,
        //character will automatically turn to face pre-defined target
        AUTO_TURN = IS_ABLE_TO_TURN | 1 << 25,
        CAN_TRANSITION_TO_SELF = 1 << 26,
        NO_PARENT_CANCELS = 1 << 28,
        //don't look at parent transitions
        NO_PARENT_TRANS = 1 << 29,
        //don't look at parent StateCondition
        NO_PARENT_COND = 1 << 30,
        INVULNERABLE = INVULNERABLE_STRIKE | INVULNERABLE_GRAB,
        GUARD_POINT = GUARD_POINT_LOW | GUARD_POINT_MID | GUARD_POINT_HIGH,
        BOUNCE_GROUND_STRONG = BOUNCE_GROUND_MEDIUM | BOUNCE_GROUND_WEAK,
        BOUNCE_WALL_STRONG = BOUNCE_WALL_MEDIUM | BOUNCE_WALL_WEAK,
        BOUNCE_GROUND = BOUNCE_GROUND_STRONG,
        BOUNCE_WALL = BOUNCE_WALL_STRONG,
        BOUNCE = BOUNCE_WALL | BOUNCE_GROUND,
        CAN_MOVE = CAN_WALK | CAN_RUN,
        STUN_STATE = STUNNED | STUNNED_SPECIAL | STUNNED_OTG,
    }
}
