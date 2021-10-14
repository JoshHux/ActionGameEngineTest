namespace ActionGameEngine.Enum
{
    //represents specific events that the input occured with
    public enum InputFlags : byte
    {
        PRESSED = 1 << 0,
        RELEASED = 1 << 1,
        DIR_AS_4WAY = 1 << 2,
        BTN_SIMUL_PRESS = 1 << 3,
        //there are no other 
        NO_INTERRUPT = 1 << 4,
        HELD_10F = 1 << 5,
        HELD_20F = 1 << 6,
        HELD_30F = 1 << 7
    }
}