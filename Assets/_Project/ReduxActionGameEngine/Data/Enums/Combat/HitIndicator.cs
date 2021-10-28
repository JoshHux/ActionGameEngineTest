using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace ActionGameEngine.Enum
{
    [System.Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    //used for gameplay to tell what gameplay modifiers to use when hit
    public enum HitIndicator : byte
    {

        GROUNDED = 1 << 0,// - on the ground, true or false
        CROUCHING = 1 << 1,// - crouching, true or false
        COUNTER_HIT = 1 << 2,// - counter hit, true or false
        BLOCKED = 1 << 3,// - blocked, true or false
        GRABBED = 1 << 4,// - grabbed, true or false
        SUPER = 1 << 5,// - super hit, true or false
        OTG = 1 << 6,// - on the ground hit, true or false
        WHIFFED = 1 << 7,// - on the ground hit, true or false
    }
}