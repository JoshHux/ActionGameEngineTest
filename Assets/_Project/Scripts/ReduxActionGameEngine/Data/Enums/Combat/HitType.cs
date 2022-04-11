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
        GRAB = 1 << 0, //(1)

        STRIKE_LOW = 1 << 1, //(2)
        STRIKE_MID = 1 << 2, //(4)
        STRIKE_HIGH = 1 << 3, //(8)
        //If paired with GRAB tag: not be able to be blocked with the corresponding position (IE. AIR only hits airborne, GROUND only hits grounded)
        //if paired with STRIKE tag: the move will only be ground OR air unblockable (IE. AIR is only unblockable in the air, but blockable on the ground)
        UNBLOCKABLE_AIR = 1 << 4, //(16)
        UNBLOCKABLE_GROUND = 1 << 5, //(32)
        UNBLOCKABLE_TRUE = 1 << 6, //(64)
        PROJECTILE = 1 << 7, //(128)
        SUPER = 1 << 8, //(256)
        ON_THE_GROUND = 1 << 9, //(512)

        FORCE_STANDING = 1 << 10, //(1024)
        FORCE_CROUCHING = 1 << 11, //(2048)
        STRIKE_LIGHT = 1 << 12, //(4096)
        STRIKE_MEDIUM = 1 << 13, //(8192)
        STRIKE_HEAVY = 1 << 14, //(16384)
        STRIKE = STRIKE_LOW | STRIKE_MID | STRIKE_HIGH,
        UNBLOCKABLE = UNBLOCKABLE_AIR | UNBLOCKABLE_GROUND | UNBLOCKABLE_TRUE,
        ENUM_MASK = STRIKE_LIGHT | STRIKE_MEDIUM | STRIKE_HEAVY | STRIKE,
    }
}
