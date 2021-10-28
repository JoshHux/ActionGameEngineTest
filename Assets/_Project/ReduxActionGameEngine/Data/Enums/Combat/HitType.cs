using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace ActionGameEngine.Enum
{
    [System.Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    //type of hit hitbox is, IE. unblockable, grab, strike, low, etc.
    //1 byte
    public enum HitType : ushort
    {
        GRAB = 1 << 0,

        STRIKE_LOW = 1 << 1,
        STRIKE_MIDDLE = 1 << 2,
        STRIKE_HIGH = 1 << 3,
        //If paired with GRAB tag: not be able to be blocked with the corresponding position (IE. AIR only hits airborne, GROUND only hits grounded)
        //if paired with STRIKE tag: the move will only be ground OR air unblockable (IE. AIR is only unblockable in the air, but blockable on the ground)
        UNBLOCKABLE_AIR = 1 << 4,
        UNBLOCKABLE_GROUND = 1 << 5,
        UNBLOCKABLE_TRUE = 1 << 6,
        PROJECTILE = 1 << 7,
        SUPER = 1 << 8,
        ON_THE_GROUND = 1 << 9,

        FORCE_STANDING = 1 << 10,
        FORCE_CROUCHING = 1 << 11,
        STRIKE = STRIKE_LOW | STRIKE_MIDDLE | STRIKE_HIGH,
        UNBLOCKABLE = UNBLOCKABLE_AIR | UNBLOCKABLE_GROUND | UNBLOCKABLE_TRUE,
    }
}
