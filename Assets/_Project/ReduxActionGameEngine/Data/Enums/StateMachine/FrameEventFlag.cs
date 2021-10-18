namespace ActionGameEngine.Enum
{
    //flag for any events that a frame might have, like setting or applying velocity
    //1 byte
    [System.Flags]
    public enum FrameEventFlag : byte
    {
        APPLY_VEL = 1 << 0,
        SET_VEL = APPLY_VEL | 1 << 1,
        //tells character that there is a TimerEvent to look at
        SET_TIMER = 1 << 2,
        //for 2d games, reaching tells the character to turn and face the other direction
        AUTO_TURN = 1 << 7
    }
}
