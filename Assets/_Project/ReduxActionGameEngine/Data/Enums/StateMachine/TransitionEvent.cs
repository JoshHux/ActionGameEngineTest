namespace ActionGameEngine.Enum
{
    //for any events to influence gamestate once transitioned to
    [System.Flags]
    public enum TransitionEvent : byte
    {
        KILL_X_VEL = 1 << 0,
        KILL_Y_VEL = 1 << 1,
        KILL_Z_VEL = 1 << 2,
        //auto-correct to face target
        FACE_ENEMY = 1 << 3,
        //deactivate any still-active hitboxes
        CLEAN_HITBOXES = 1 << 4,
        //to flag enemy to block (if using something like proximity blocking)
        FLAG_BLOCK = 1 << 7,
        KILL_VEL = KILL_X_VEL | KILL_Y_VEL | KILL_Z_VEL,
    }
}