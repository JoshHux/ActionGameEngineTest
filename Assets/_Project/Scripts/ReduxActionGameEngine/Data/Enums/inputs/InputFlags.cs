using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace ActionGameEngine.Enum
{
    //represents specific events that the input occured with
    //ONLY RELEVANT WHEN CHECKING INPUT FOR TRANSITION/COMMAND
    [System.Flags]
    [JsonConverter(typeof(StringEnumConverter))]

    public enum InputFlags : byte
    {
        PRESSED = 1 << 0,
        RELEASED = 1 << 1,
        DIR_AS_4WAY = 1 << 2,
        BTN_SIMUL_PRESS = 1 << 3,
        //there are no other inputs between the press/release of the exact input fragment
        NO_INTERRUPT = 1 << 4,
        //Check, are the inputs we're looking for currently not being held?
        CHECK_IS_UP = 1 << 5,
        //this flag means that any input from the fragment is okay
        ANY_IS_OKAY = 1 << 6,
        HELD_30F = 1 << 7,
        HELD = HELD_30F,
        NEED_PREV = HELD | BTN_SIMUL_PRESS | NO_INTERRUPT,


    }
}