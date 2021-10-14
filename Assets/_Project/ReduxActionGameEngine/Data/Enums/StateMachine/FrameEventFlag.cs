namespace ActionGameEngine.Enum
{
    //flag for any events that a frame might have, like setting or applying velocity
    //1 byte
    public enum FrameEventFlag : byte
    {
        APPLY_VEL = 1 << 0,
        SET_VEL = APPLY_VEL | 1 << 1,
        //for 2d games, reaching tells the character to turn and face the other direction
        TURN_AROUND = 1 << 7
    }
}
