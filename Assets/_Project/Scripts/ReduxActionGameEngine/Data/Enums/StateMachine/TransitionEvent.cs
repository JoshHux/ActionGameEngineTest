using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace ActionGameEngine.Enum
{
    [System.Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    //for any events to influence gamestate once transitioned to
    public enum TransitionEvent : byte
    {
        KILL_X_VEL = 1 << 0,
        KILL_Y_VEL = 1 << 1,
        KILL_Z_VEL = 1 << 2,
        //auto-correct to face target
        FACE_ENEMY = 1 << 3,
        //deactivate any still-active hitboxes
        CLEAN_HITBOXES = 1 << 4,
        //indicates that we've exited a stun state
        RESET_STUN = 1 << 5,
        //to flag enemy to block (if using something like proximity blocking)
        FLAG_BLOCK = 1 << 7,
        KILL_VEL = KILL_X_VEL | KILL_Y_VEL | KILL_Z_VEL,
    }
}