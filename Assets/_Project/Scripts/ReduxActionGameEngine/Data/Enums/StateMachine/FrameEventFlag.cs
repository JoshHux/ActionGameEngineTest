using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace ActionGameEngine.Enum
{
    //flag for any events that a frame might have, like setting or applying velocity
    [System.Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FrameEventFlag : uint
    {
        APPLY_VEL = 1 << 0,
        SET_VEL = APPLY_VEL | 1 << 1,
        //tells character that there is a TimerEvent to look at
        SET_TIMER = 1 << 2,
        ACTIVATE_HITBOXES = 1 << 3,
        ACTIVATE_HURTBOXES = 1 << 4,
        DRAIN_RESOURCES = 1 << 5,
        GAIN_RESOURCES = 1 << 6,
        //for 2d games, reaching tells the character to turn and face the other direction
        AUTO_TURN = 1 << 7,
        SPAWN_PROJECTILE = 1 << 8,
        SUPER_FLASH = (uint)1 << 31,
    }
}