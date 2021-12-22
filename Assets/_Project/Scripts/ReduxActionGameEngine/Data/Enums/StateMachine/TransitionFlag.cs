using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace ActionGameEngine.Enum
{
    [System.Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    //for any events in gameplay that might warrant a state to transition
    //these conditions are for information coming from gameplay to the statemachine
    public enum TransitionFlag : short
    {
        //When character's grounded trigger detects that the character is on the ground
        GROUNDED = 1 << 0,
        //When character's grounded trigger detects that the character is in the air
        AIRBORNE = 1 << 1,
        //When character's wall trigger detects that the character is touching a wall
        WALLED = 1 << 2,
        //When the current state's duration has fully passed
        STATE_END = 1 << 3,
        //When a character hits another character
        HIT_CONFIRM = 1 << 4,
        //When a character is hit by another character
        GOT_HIT = 1 << 5,
        //When a character is grabbed by another character
        GOT_GRABBED = 1 << 6,
        //When a character that was previously installed, has their install timer run out
        UNINSTALL = 1 << 7,
        //When you want the player to guard, IE for proximity block
        GUARD = 1 << 8,
    }
}