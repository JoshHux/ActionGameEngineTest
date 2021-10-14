namespace ActionGameEngine.Enum
{
    public enum CancelConditions : int
    {

        GROUNDED = 1 << 0,
        AIRBORNE = 1 << 1,
        JUMP = 1 << 2,
        DASH = 1 << 3,
        GUARD = 1 << 4,
        NORMAL = 1 << 5,
        COMMAND_NORMAL = 1 << 6,
        SPECIAL = 1 << 7,
        SUPER = 1 << 8,
        GRAB = 1 << 9,
        //For rekkas or any other move that you would want to specifically follow up on
        FLUP_LV1 = 1 << 10,
        FLUP_LV2 = 1 << 11,
        SUPER_LV1 = SUPER | 1 << 12,
        SUPER_LV2 = SUPER | 1 << 13,
        SPCL_LV1 = SPECIAL | 1 << 14,
        SPCL_LV2 = SPECIAL | 1 << 15,
        SPCL_LV3 = SPECIAL | 1 << 16,
        SPCL_LV4 = SPECIAL | 1 << 17,
        SPCL_LV5 = SPECIAL | 1 << 18,
        SPCL_LV6 = SPECIAL | 1 << 19,
        CMDNORM_LV1 = COMMAND_NORMAL | 1 << 20,
        CMDNORM_LV2 = COMMAND_NORMAL | 1 << 21,
        CMDNORM_LV3 = COMMAND_NORMAL | 1 << 22,
        CMDNORM_LV4 = COMMAND_NORMAL | 1 << 23,
        CMDNORM_LV5 = COMMAND_NORMAL | 1 << 24,
        CMDNORM_LV6 = COMMAND_NORMAL | 1 << 25,
        NORM_LV1 = NORMAL | 1 << 26,
        NORM_LV2 = NORMAL | 1 << 27,
        NORM_LV3 = NORMAL | 1 << 28,
        NORM_LV4 = NORMAL | 1 << 29,
        NORM_LV5 = NORMAL | 1 << 30,
        NORM_LV6 = NORMAL | 1 << 31,
    }

}
